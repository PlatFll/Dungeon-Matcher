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

    [Header("Runtime Debug Information")]
    [SerializeField]
    private float remainingAttackTime;

    [SerializeField]
    private bool isRunning;

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

        isRunning = false;
        remainingAttackTime = 0f;
    }

    public bool PerformAttackImmediately()
    {
        if (!CanPerformAttack())
        {
            return false;
        }

        AttackStarted?.Invoke(this);

        int attackDamage =
            Mathf.Max(
                0,
                enemyActor.Damage
            );

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
            while (CanContinueAttackLoop() &&
                   IsPausedByStagger)
            {
                yield return null;
            }

            if (CanPerformAttack())
            {
                PerformAttackImmediately();
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
             * Protect against stagger being applied on the
             * exact frame the countdown reaches zero.
             */
            while (CanContinueAttackLoop() &&
                   IsPausedByStagger)
            {
                yield return null;
            }

            if (!CanContinueAttackLoop())
            {
                FinishCoroutine();
                yield break;
            }

            PerformAttackImmediately();
        }

        FinishCoroutine();
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
            !IsPausedByStagger;
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