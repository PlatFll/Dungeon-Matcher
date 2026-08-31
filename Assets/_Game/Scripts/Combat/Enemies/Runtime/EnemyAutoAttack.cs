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
    private bool isAttackSequenceInProgress;

    [SerializeField]
    private int pendingAttackDamage;

    [SerializeField]
    private bool pendingAnimationHitIsFinal;

    private EnemyActor enemyActor;
    private EnemyStagger enemyStagger;
    private PlayerActor playerTarget;
    private Coroutine attackCoroutine;
    private Coroutine attackSequenceCoroutine;

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

    public bool IsAttackSequenceInProgress =>
        isAttackSequenceInProgress;

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

        CancelAttackSequence();
        isRunning = false;
        remainingAttackTime = 0f;
    }

    public bool PerformAttackImmediately()
    {
        if (!CanPerformAttack())
        {
            return false;
        }

        bool timeFromAnimation =
            ShouldTimeAttackFromAnimation();

        int primaryDamage =
            Mathf.Max(
                0,
                enemyActor.Damage
            );

        int followUpDamage =
            Mathf.Max(
                0,
                enemyActor.FollowUpDamage
            );

        if (!timeFromAnimation)
        {
            isAttackSequenceInProgress = true;

            bool primaryDamageApplied =
                PerformImmediateHit(
                    primaryDamage
                );

            if (followUpDamage > 0 &&
                CanContinueAttackLoop())
            {
                PerformImmediateHit(
                    followUpDamage
                );
            }

            isAttackSequenceInProgress = false;
            return primaryDamageApplied;
        }

        /*
         * One gameplay-critical action owns the complete attack sequence.
         * A special ability therefore cannot replace either hit before its
         * AutoAttackImpact has had a chance to fire.
         */
        if (!enemyActor
                .TryBeginAutoAttackAnimationAction())
        {
            return false;
        }

        isAttackSequenceInProgress = true;

        attackSequenceCoroutine =
            StartCoroutine(
                PerformAnimationTimedAttackSequence(
                    primaryDamage,
                    followUpDamage
                )
            );

        return true;
    }

    public bool ResolveAnimationImpact()
    {
        if (!isWaitingForAnimationImpact)
        {
            return false;
        }

        int attackDamage =
            pendingAttackDamage;

        bool isFinalHit =
            pendingAnimationHitIsFinal;

        /*
         * Clear the pending payload before applying damage. Duplicate Animation
         * Events cannot resolve twice, but the action ownership remains held
         * until the complete attack sequence has finished.
         */
        isWaitingForAnimationImpact = false;
        pendingAttackDamage = 0;
        pendingAnimationHitIsFinal = false;

        if (!CanContinueAttackLoop())
        {
            return false;
        }

        bool damageApplied =
            ResolveAttackDamage(
                attackDamage
            );

        if (isFinalHit)
        {
            FinishAttackSequence();
        }

        return damageApplied;
    }

    private IEnumerator
        PerformAnimationTimedAttackSequence(
            int primaryDamage,
            int followUpDamage)
    {
        yield return
            PerformAnimationTimedHit(
                primaryDamage,
                followUpDamage <= 0
            );

        if (!isAttackSequenceInProgress)
        {
            yield break;
        }

        if (followUpDamage > 0 &&
            CanContinueAttackLoop())
        {
            /*
             * Arm the second impact on a later frame. Duplicate Animation
             * Events emitted by the first strike therefore see no pending
             * payload and cannot consume the follow-up hit.
             */
            yield return null;

            if (isAttackSequenceInProgress &&
                CanContinueAttackLoop())
            {
                yield return
                    PerformAnimationTimedHit(
                        followUpDamage,
                        isFinalHit: true
                    );
            }
        }

        if (isAttackSequenceInProgress)
        {
            FinishAttackSequence();
        }
    }

    private IEnumerator PerformAnimationTimedHit(
        int attackDamage,
        bool isFinalHit)
    {
        if (!CanContinueAttackLoop())
        {
            yield break;
        }

        pendingAttackDamage = attackDamage;
        pendingAnimationHitIsFinal = isFinalHit;
        isWaitingForAnimationImpact = true;

        AttackStarted?.Invoke(this);

        yield return
            WaitForAnimationImpactOrTimeout();
    }

    private bool PerformImmediateHit(
        int attackDamage)
    {
        AttackStarted?.Invoke(this);

        if (!CanContinueAttackLoop())
        {
            return false;
        }

        return ResolveAttackDamage(
            attackDamage
        );
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

            if (isAttackSequenceInProgress)
            {
                yield return
                    WaitForAttackSequenceToFinish();
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

            if (isAttackSequenceInProgress)
            {
                yield return
                    WaitForAttackSequenceToFinish();
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

    private IEnumerator WaitForAttackSequenceToFinish()
    {
        while (CanContinueAttackLoop() &&
               isAttackSequenceInProgress)
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
        pendingAnimationHitIsFinal = false;
        ReleaseAutoAttackAnimationAction();
    }

    private void CancelAttackSequence()
    {
        if (attackSequenceCoroutine != null)
        {
            StopCoroutine(attackSequenceCoroutine);
            attackSequenceCoroutine = null;
        }

        isAttackSequenceInProgress = false;
        CancelPendingAnimationImpact();
    }

    private void FinishAttackSequence()
    {
        isWaitingForAnimationImpact = false;
        pendingAttackDamage = 0;
        pendingAnimationHitIsFinal = false;
        isAttackSequenceInProgress = false;
        attackSequenceCoroutine = null;

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
            !isAttackSequenceInProgress &&
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
        CancelAttackSequence();
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
