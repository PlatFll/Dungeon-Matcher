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

    [Header("Presentation Safety")]
    [SerializeField]
    [Min(0.1f)]
    [Tooltip(
        "Maximum real-time wait for an auto-attack presentation checkpoint. " +
        "If feedback or an Animation Event is missing or interrupted, the " +
        "attack advances automatically so the enemy cannot stall forever."
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
    private bool isWaitingForPresentationImpact;

    [SerializeField]
    private bool isWaitingForPresentationCompletion;

    [SerializeField]
    private bool isAttackSequenceInProgress;

    [SerializeField]
    private int pendingAttackDamage;

    [SerializeField]
    private bool pendingHitFinishesSequence;

    [SerializeField]
    private int activeAttackPresentationId;

    private EnemyActor enemyActor;
    private EnemyStagger enemyStagger;
    private PlayerActor playerTarget;
    private Coroutine attackCoroutine;
    private Coroutine attackSequenceCoroutine;
    private int nextAttackPresentationId;

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

    public int ActiveAttackPresentationId =>
        activeAttackPresentationId;

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

        float followUpAttackDelay =
            followUpDamage > 0 &&
            enemyActor.Definition != null
                ? Mathf.Max(
                    0f,
                    enemyActor.Definition
                        .FollowUpAttackDelay
                )
                : 0f;

        if (!timeFromAnimation &&
            followUpDamage <= 0)
        {
            return PerformImmediateHit(
                primaryDamage
            );
        }

        /*
         * One gameplay-critical action owns the complete attack sequence.
         * A special ability therefore cannot replace either hit before its
         * synchronized impact and return checkpoints have completed.
         */
        if (!enemyActor
                .TryBeginAutoAttackAnimationAction())
        {
            return false;
        }

        isAttackSequenceInProgress = true;

        attackSequenceCoroutine =
            StartCoroutine(
                PerformTimedAttackSequence(
                    primaryDamage,
                    followUpDamage,
                    followUpAttackDelay,
                    timeFromAnimation
                )
            );

        return true;
    }

    public bool ResolveAnimationImpact()
    {
        return ResolvePendingAttackImpact(
            activeAttackPresentationId,
            fromAnimation: true
        );
    }

    public bool ResolvePresentationImpact(
        int presentationId)
    {
        return ResolvePendingAttackImpact(
            presentationId,
            fromAnimation: false
        );
    }

    public bool CompleteAttackPresentation(
        int presentationId)
    {
        if (!isAttackSequenceInProgress ||
            presentationId <= 0 ||
            presentationId !=
                activeAttackPresentationId ||
            !isWaitingForPresentationCompletion)
        {
            return false;
        }

        isWaitingForPresentationCompletion = false;
        ClearPresentationIdIfComplete();
        return true;
    }

    private bool ResolvePendingAttackImpact(
        int presentationId,
        bool fromAnimation)
    {
        bool isWaitingForExpectedImpact =
            fromAnimation
                ? isWaitingForAnimationImpact
                : isWaitingForPresentationImpact;

        if (!isAttackSequenceInProgress ||
            presentationId <= 0 ||
            presentationId !=
                activeAttackPresentationId ||
            !isWaitingForExpectedImpact)
        {
            return false;
        }

        int attackDamage =
            pendingAttackDamage;

        bool isFinalHit =
            pendingHitFinishesSequence;

        /*
         * Clear the pending payload before applying damage. Duplicate feedback
         * callbacks or Animation Events cannot resolve the same hit twice, but
         * action ownership remains held until the complete sequence finishes.
         */
        isWaitingForAnimationImpact = false;
        isWaitingForPresentationImpact = false;
        pendingAttackDamage = 0;
        pendingHitFinishesSequence = false;

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
        else
        {
            ClearPresentationIdIfComplete();
        }

        return damageApplied;
    }

    private IEnumerator
        PerformTimedAttackSequence(
            int primaryDamage,
            int followUpDamage,
            float followUpAttackDelay,
            bool timeFromAnimation)
    {
        yield return
            PerformTimedHit(
                primaryDamage,
                timeFromAnimation,
                finishSequenceAtImpact:
                    followUpDamage <= 0,
                waitForPresentationCompletion:
                    followUpDamage > 0
            );

        if (!isAttackSequenceInProgress)
        {
            yield break;
        }

        if (followUpDamage > 0 &&
            CanContinueAttackLoop())
        {
            if (followUpAttackDelay > 0f)
            {
                yield return
                    WaitForFollowUpAttackDelay(
                        followUpAttackDelay
                    );
            }

            if (!isAttackSequenceInProgress ||
                !CanContinueAttackLoop())
            {
                yield break;
            }

            yield return
                PerformTimedHit(
                    followUpDamage,
                    timeFromAnimation,
                    finishSequenceAtImpact: false,
                    waitForPresentationCompletion: true
                );
        }

        if (isAttackSequenceInProgress)
        {
            FinishAttackSequence();
        }
    }

    private IEnumerator WaitForFollowUpAttackDelay(
        float delay)
    {
        float remainingDelay =
            Mathf.Max(
                0f,
                delay
            );

        /*
         * The accepted auto-attack action remains owned throughout this
         * recovery beat. Specials and another cooldown-driven attack cannot
         * enter between hits, while defeat or cancellation ends the combo.
         */
        while (remainingDelay > 0f &&
               isAttackSequenceInProgress &&
               CanContinueAttackLoop())
        {
            remainingDelay =
                Mathf.Max(
                    0f,
                    remainingDelay -
                    Time.deltaTime
                );

            yield return null;
        }
    }

    private IEnumerator PerformTimedHit(
        int attackDamage,
        bool timeFromAnimation,
        bool finishSequenceAtImpact,
        bool waitForPresentationCompletion)
    {
        if (!CanContinueAttackLoop())
        {
            yield break;
        }

        int presentationId =
            BeginTimedHit(
                attackDamage,
                timeFromAnimation,
                finishSequenceAtImpact,
                waitForPresentationCompletion
            );

        AttackStarted?.Invoke(this);

        if (timeFromAnimation)
        {
            yield return
                WaitForAnimationImpactOrTimeout(
                    presentationId
                );
        }
        else
        {
            yield return
                WaitForPresentationImpactOrTimeout(
                    presentationId
                );
        }

        if (!isAttackSequenceInProgress ||
            !CanContinueAttackLoop())
        {
            yield break;
        }

        if (waitForPresentationCompletion)
        {
            yield return
                WaitForPresentationCompletionOrTimeout(
                    presentationId
                );
        }
    }

    private int BeginTimedHit(
        int attackDamage,
        bool timeFromAnimation,
        bool finishSequenceAtImpact,
        bool waitForPresentationCompletion)
    {
        nextAttackPresentationId =
            nextAttackPresentationId == int.MaxValue
                ? 1
                : nextAttackPresentationId + 1;

        activeAttackPresentationId =
            nextAttackPresentationId;

        pendingAttackDamage = attackDamage;
        pendingHitFinishesSequence =
            finishSequenceAtImpact;
        isWaitingForAnimationImpact =
            timeFromAnimation;
        isWaitingForPresentationImpact =
            !timeFromAnimation;
        isWaitingForPresentationCompletion =
            waitForPresentationCompletion;

        return activeAttackPresentationId;
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
        WaitForAnimationImpactOrTimeout(
            int presentationId)
    {
        float timeout =
            Mathf.Max(
                0.1f,
                animationImpactTimeout
            );

        float waitStartedAt =
            Time.realtimeSinceStartup;

        while (CanContinueAttackLoop() &&
               isWaitingForAnimationImpact &&
               activeAttackPresentationId ==
                   presentationId)
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

                if (isWaitingForPresentationCompletion)
                {
                    CompleteAttackPresentation(
                        presentationId
                    );
                }

                yield break;
            }

            yield return null;
        }

        if (isWaitingForAnimationImpact &&
            activeAttackPresentationId ==
                presentationId)
        {
            CancelAttackSequence();
        }
    }

    private IEnumerator
        WaitForPresentationImpactOrTimeout(
            int presentationId)
    {
        float timeout = GetPresentationTimeout();
        float waitStartedAt =
            Time.realtimeSinceStartup;

        while (CanContinueAttackLoop() &&
               isWaitingForPresentationImpact &&
               activeAttackPresentationId ==
                   presentationId)
        {
            if (Time.realtimeSinceStartup -
                waitStartedAt >= timeout)
            {
                Debug.LogWarning(
                    $"{name} waited {timeout:0.##}s for its " +
                    "attack-lunge impact. Resolving the hit and return " +
                    "through the failsafe so feedback cannot stall its runtime.",
                    this
                );

                ResolvePresentationImpact(
                    presentationId
                );
                CompleteAttackPresentation(
                    presentationId
                );
                yield break;
            }

            yield return null;
        }

        if (isWaitingForPresentationImpact &&
            activeAttackPresentationId ==
                presentationId)
        {
            CancelAttackSequence();
        }
    }

    private IEnumerator
        WaitForPresentationCompletionOrTimeout(
            int presentationId)
    {
        float timeout = GetPresentationTimeout();
        float waitStartedAt =
            Time.realtimeSinceStartup;

        while (CanContinueAttackLoop() &&
               isWaitingForPresentationCompletion &&
               activeAttackPresentationId ==
                   presentationId)
        {
            if (Time.realtimeSinceStartup -
                waitStartedAt >= timeout)
            {
                Debug.LogWarning(
                    $"{name} waited {timeout:0.##}s for its attack " +
                    "presentation to return to rest. Advancing through the " +
                    "failsafe so feedback cannot stall its runtime.",
                    this
                );

                CompleteAttackPresentation(
                    presentationId
                );
                yield break;
            }

            yield return null;
        }

        if (isWaitingForPresentationCompletion &&
            activeAttackPresentationId ==
                presentationId)
        {
            CancelAttackSequence();
        }
    }

    private float GetPresentationTimeout()
    {
        return Mathf.Max(
            0.1f,
            animationImpactTimeout
        );
    }

    private bool ShouldTimeAttackFromAnimation()
    {
        return
            enemyActor != null &&
            enemyActor.Definition != null &&
            enemyActor.Definition
                .TimeAutoAttackFromAnimation;
    }

    private void ResetPendingAttackPresentation()
    {
        isWaitingForAnimationImpact = false;
        isWaitingForPresentationImpact = false;
        isWaitingForPresentationCompletion = false;
        pendingAttackDamage = 0;
        pendingHitFinishesSequence = false;
        activeAttackPresentationId = 0;
    }

    private void ClearPresentationIdIfComplete()
    {
        if (!isWaitingForAnimationImpact &&
            !isWaitingForPresentationImpact &&
            !isWaitingForPresentationCompletion)
        {
            activeAttackPresentationId = 0;
        }
    }

    private void CancelAttackSequence()
    {
        if (attackSequenceCoroutine != null)
        {
            StopCoroutine(attackSequenceCoroutine);
            attackSequenceCoroutine = null;
        }

        isAttackSequenceInProgress = false;
        ResetPendingAttackPresentation();
        ReleaseAutoAttackAnimationAction();
    }

    private void FinishAttackSequence()
    {
        ResetPendingAttackPresentation();
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
            !isWaitingForPresentationImpact &&
            !isWaitingForPresentationCompletion &&
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
