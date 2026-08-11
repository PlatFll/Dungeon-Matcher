using System;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(EnemyActor))]
public sealed class EnemyAutoAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField]
    private bool attackAutomatically = true;

    [SerializeField]
    [Tooltip(
        "The enemy waits for its full attack interval " +
        "before performing its first attack."
    )]
    private bool waitBeforeFirstAttack = true;

    [Header("Animation Safety")]
    [SerializeField]
    [Min(0.1f)]
    [Tooltip(
        "Maximum real-time wait for an AutoAttackImpact Animation Event. " +
        "If the event is missing or the animation is interrupted, the " +
        "attack resolves automatically so the enemy cannot stall forever."
    )]
    private float animationImpactTimeout = 3f;

    [Header("Runtime Debug Information")]
    [SerializeField]
    private float remainingAttackTime;

    [SerializeField]
    private bool isRunning;

    [SerializeField]
    private bool isWaitingForAnimationImpact;

    [SerializeField]
    private int pendingAttackDamage;

    private EnemyActor enemyActor;
    private EnemyStagger enemyStagger;
    private PlayerActor playerTarget;
    private Coroutine attackCoroutine;

    public event Action<EnemyAutoAttack>
        AttackStarted;

    public event Action<
        EnemyAutoAttack,
        int,
        bool
    > AttackResolved;

    public EnemyActor EnemyActor =>
        enemyActor;

    public PlayerActor PlayerTarget =>
        playerTarget;

    public float RemainingAttackTime =>
        remainingAttackTime;

    public bool IsRunning =>
        isRunning;

    public bool IsWaitingForAnimationImpact =>
        isWaitingForAnimationImpact;

    public bool IsPausedByStagger =>
        enemyStagger != null &&
        enemyStagger.IsStaggered;

    public float CooldownNormalized
    {
        get
        {
            if (enemyActor == null ||
                enemyActor.AttackInterval <= 0f)
            {
                return 0f;
            }

            return Mathf.Clamp01(
                remainingAttackTime /
                enemyActor.AttackInterval
            );
        }
    }

    public void Initialize(
        EnemyActor enemy,
        PlayerActor target)
    {
        StopAttacking();
        Unsubscribe();

        enemyActor = enemy;
        playerTarget = target;

        enemyStagger =
            GetComponent<EnemyStagger>();

        if (enemyActor == null)
        {
            Debug.LogError(
                "EnemyAutoAttack requires an EnemyActor.",
                this
            );

            return;
        }

        if (playerTarget == null)
        {
            Debug.LogError(
                $"{name} cannot attack without a " +
                "PlayerActor target.",
                this
            );

            return;
        }

        if (enemyStagger == null)
        {
            Debug.LogWarning(
                $"{name} has no EnemyStagger component. " +
                "Its attack timer cannot be paused by stagger.",
                this
            );
        }

        Subscribe();

        if (attackAutomatically)
        {
            TryStartAttacking();
        }
    }

    public void TryStartAttacking()
    {
        if (attackCoroutine != null)
        {
            return;
        }

        if (!CanContinueAttackLoop())
        {
            return;
        }

        attackCoroutine =
            StartCoroutine(
                AttackLoop()
            );
    }

    public void StopAttacking()
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }

        CancelPendingAnimationImpact();
        isRunning = false;
        remainingAttackTime = 0f;
    }

    public bool PerformAttackImmediately()
    {
        if (!CanPerformAttack())
        {
            return false;
        }

        int attackDamage =
            Mathf.Max(
                0,
                enemyActor.Damage
            );

        if (ShouldTimeAttackFromAnimation())
        {
            /*
             * A gameplay-critical attack owns the enemy's action window until
             * its impact resolves. A special ability therefore cannot replace
             * this animation before AutoAttackImpact has had a chance to fire.
             */
            if (!enemyActor
                    .TryBeginAutoAttackAnimationAction())
            {
                return false;
            }

            pendingAttackDamage = attackDamage;
            isWaitingForAnimationImpact = true;

            AttackStarted?.Invoke(this);
            return true;
        }

        AttackStarted?.Invoke(this);
        return ResolveAttackDamage(attackDamage);
    }

    public bool ResolveAnimationImpact()
    {
        if (!isWaitingForAnimationImpact)
        {
            return false;
        }

        int attackDamage =
            pendingAttackDamage;

        /*
         * Clear the pending payload before applying damage. Duplicate Animation
         * Events cannot resolve twice, but the action ownership remains held
         * until the gameplay impact itself has completed.
         */
        isWaitingForAnimationImpact = false;
        pendingAttackDamage = 0;

        if (!CanContinueAttackLoop())
        {
            ReleaseAutoAttackAnimationAction();
            return false;
        }

        bool damageApplied =
            ResolveAttackDamage(
                attackDamage
            );

        ReleaseAutoAttackAnimationAction();
        return damageApplied;
    }

    private bool ResolveAttackDamage(
        int attackDamage)
    {
        bool damageApplied =
            playerTarget.TryTakeDamage(
                attackDamage,
                enemyActor
            );

        AttackResolved?.Invoke(
            this,
            attackDamage,
            damageApplied
        );

        return damageApplied;
    }

    private IEnumerator AttackLoop()
    {
        isRunning = true;

        if (!waitBeforeFirstAttack)
        {
            yield return WaitUntilAttackCanStart();

            if (CanPerformAttack())
            {
                PerformAttackImmediately();
            }

            if (isWaitingForAnimationImpact)
            {
                yield return
                    WaitForAnimationImpactOrTimeout();
            }
        }

        while (CanContinueAttackLoop())
        {
            remainingAttackTime =
                Mathf.Max(
                    0.1f,
                    enemyActor.AttackInterval
                );

            while (remainingAttackTime > 0f)
            {
                if (!CanContinueAttackLoop())
                {
                    FinishCoroutine();
                    yield break;
                }

                /*
                 * Stagger pauses the countdown without
                 * resetting or reducing the stored time.
                 */
                if (IsPausedByStagger)
                {
                    yield return null;
                    continue;
                }

                remainingAttackTime =
                    Mathf.Max(
                        0f,
                        remainingAttackTime -
                        Time.deltaTime
                    );

                yield return null;
            }

            /*
             * Once the cooldown reaches zero, retain the ready attack while a
             * stagger or gameplay-critical special action owns this enemy.
             * The cooldown does not restart until the waiting attack actually
             * gets its turn.
             */
            yield return WaitUntilAttackCanStart();

            if (!CanContinueAttackLoop())
            {
                FinishCoroutine();
                yield break;
            }

            if (CanPerformAttack())
            {
                PerformAttackImmediately();
            }

            if (isWaitingForAnimationImpact)
            {
                yield return
                    WaitForAnimationImpactOrTimeout();
            }
        }

        FinishCoroutine();
    }

    private IEnumerator WaitUntilAttackCanStart()
    {
        while (CanContinueAttackLoop() &&
               !CanPerformAttack())
        {
            yield return null;
        }
    }

    private IEnumerator
        WaitForAnimationImpactOrTimeout()
    {
        float timeout =
            Mathf.Max(
                0.1f,
                animationImpactTimeout
            );

        float waitStartedAt =
            Time.realtimeSinceStartup;

        while (CanContinueAttackLoop() &&
               isWaitingForAnimationImpact)
        {
            if (Time.realtimeSinceStartup -
                waitStartedAt >= timeout)
            {
                Debug.LogWarning(
                    $"{name} waited {timeout:0.##}s for " +
                    "AutoAttackImpact. Resolving the attack through the " +
                    "failsafe so animation setup cannot stall its runtime.",
                    this
                );

                ResolveAnimationImpact();
                yield break;
            }

            yield return null;
        }

        if (isWaitingForAnimationImpact)
        {
            CancelPendingAnimationImpact();
        }
    }

    private bool ShouldTimeAttackFromAnimation()
    {
        return
            enemyActor != null &&
            enemyActor.Definition != null &&
            enemyActor.Definition
                .TimeAutoAttackFromAnimation;
    }

    private void CancelPendingAnimationImpact()
    {
        isWaitingForAnimationImpact = false;
        pendingAttackDamage = 0;
        ReleaseAutoAttackAnimationAction();
    }

    private void ReleaseAutoAttackAnimationAction()
    {
        if (enemyActor != null)
        {
            enemyActor.EndAutoAttackAnimationAction();
        }
    }

    private bool CanContinueAttackLoop()
    {
        return
            isActiveAndEnabled &&
            enemyActor != null &&
            enemyActor.IsInitialized &&
            !enemyActor.IsDefeated &&
            playerTarget != null &&
            playerTarget.IsInitialized &&
            !playerTarget.IsDefeated;
    }

    private bool CanPerformAttack()
    {
        return
            CanContinueAttackLoop() &&
            !IsPausedByStagger &&
            !isWaitingForAnimationImpact &&
            !enemyActor
                .IsSpecialAbilityAnimationActionActive;
    }

    private void Subscribe()
    {
        if (enemyActor != null)
        {
            enemyActor.Defeated +=
                HandleEnemyDefeated;
        }

        if (playerTarget != null)
        {
            playerTarget.Initialized +=
                HandlePlayerInitialized;

            playerTarget.Defeated +=
                HandlePlayerDefeated;

            playerTarget.Revived +=
                HandlePlayerRevived;
        }
    }

    private void Unsubscribe()
    {
        if (enemyActor != null)
        {
            enemyActor.Defeated -=
                HandleEnemyDefeated;
        }

        if (playerTarget != null)
        {
            playerTarget.Initialized -=
                HandlePlayerInitialized;

            playerTarget.Defeated -=
                HandlePlayerDefeated;

            playerTarget.Revived -=
                HandlePlayerRevived;
        }
    }

    private void HandleEnemyDefeated(
        EnemyActor enemy)
    {
        StopAttacking();
    }

    private void HandlePlayerInitialized(
        PlayerActor player)
    {
        if (attackAutomatically)
        {
            TryStartAttacking();
        }
    }

    private void HandlePlayerDefeated(
        PlayerActor player)
    {
        StopAttacking();
    }

    private void HandlePlayerRevived(
        PlayerActor player,
        int revivalCount)
    {
        if (attackAutomatically)
        {
            TryStartAttacking();
        }
    }

    private void FinishCoroutine()
    {
        CancelPendingAnimationImpact();
        attackCoroutine = null;
        isRunning = false;
        remainingAttackTime = 0f;
    }

    private void OnDisable()
    {
        StopAttacking();
    }

    private void OnDestroy()
    {
        StopAttacking();
        Unsubscribe();
    }

    [ContextMenu("Prototype/Attack Player Now")]
    private void DebugAttackImmediately()
    {
        if (Application.isPlaying)
        {
            PerformAttackImmediately();
        }
    }
}
