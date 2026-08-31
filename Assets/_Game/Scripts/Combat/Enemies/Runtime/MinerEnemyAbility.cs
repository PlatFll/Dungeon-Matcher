using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(EnemyActor))]
public sealed class MinerEnemyAbility :
    MonoBehaviour,
    IEnemySpecialAbilityRuntime
{
    private const int MaximumOwnedMines = 3;

    [Header("Runtime Debug Information")]
    [SerializeField]
    private int ownedMinedTileCount;

    private EnemyActor enemyActor;
    private BoardController boardController;

    private int ownerInstanceId;
    private bool restoreQueued;
    private bool isAttemptingReadyAbility;

    public void InitializeSpecialAbility(
        EnemyActor initializedEnemy,
        BoardController initializedBoard,
        IReadOnlyList<EnemyActor> activeEnemies)
    {
        Unsubscribe();

        enemyActor = initializedEnemy;
        boardController = initializedBoard;

        restoreQueued = false;
        isAttemptingReadyAbility = false;

        ownerInstanceId =
            enemyActor != null
                ? enemyActor.GetInstanceID()
                : 0;

        RefreshOwnedMineCount();

        if (enemyActor == null ||
            boardController == null)
        {
            Debug.LogError(
                "MinerEnemyAbility requires an initialized " +
                "EnemyActor and BoardController.",
                this
            );

            return;
        }

        if (!enemyActor.HasSpecialAbility)
        {
            Debug.LogWarning(
                $"{enemyActor.name} has MinerEnemyAbility but its " +
                "EnemyDefinition does not enable a special ability.",
                this
            );
        }

        Subscribe();
    }

    private void Subscribe()
    {
        if (enemyActor == null)
        {
            return;
        }

        enemyActor.SpecialBecameReady -=
            HandleSpecialBecameReady;

        enemyActor.SpecialBecameReady +=
            HandleSpecialBecameReady;

        enemyActor.SpecialAbilityImpactReached -=
            HandleSpecialAbilityImpactReached;

        enemyActor.SpecialAbilityImpactReached +=
            HandleSpecialAbilityImpactReached;

        enemyActor.AnimationActionReleased -=
            HandleAnimationActionReleased;

        enemyActor.AnimationActionReleased +=
            HandleAnimationActionReleased;

        enemyActor.Defeated -=
            HandleEnemyDefeated;

        enemyActor.Defeated +=
            HandleEnemyDefeated;
    }

    private void Unsubscribe()
    {
        if (enemyActor == null)
        {
            return;
        }

        enemyActor.SpecialBecameReady -=
            HandleSpecialBecameReady;

        enemyActor.SpecialAbilityImpactReached -=
            HandleSpecialAbilityImpactReached;

        enemyActor.AnimationActionReleased -=
            HandleAnimationActionReleased;

        enemyActor.Defeated -=
            HandleEnemyDefeated;
    }

    private void HandleSpecialBecameReady(
        EnemyActor readyEnemy)
    {
        if (readyEnemy == null ||
            readyEnemy != enemyActor ||
            readyEnemy.IsDefeated ||
            boardController == null)
        {
            return;
        }

        TryUseReadyAbility();
    }

    private void HandleAnimationActionReleased(
        EnemyActor releasedEnemy)
    {
        if (releasedEnemy == null ||
            releasedEnemy != enemyActor ||
            releasedEnemy.IsDefeated ||
            !releasedEnemy.IsSpecialReady)
        {
            return;
        }

        /*
         * If the Miner became ready while an animation-timed auto attack still
         * owned the enemy, preserve the ready charge and retry immediately after
         * that impact releases the shared action window.
         */
        TryUseReadyAbility();
    }

    private void TryUseReadyAbility()
    {
        if (isAttemptingReadyAbility ||
            enemyActor == null ||
            boardController == null ||
            enemyActor.IsDefeated ||
            !enemyActor.IsSpecialReady)
        {
            return;
        }

        isAttemptingReadyAbility = true;

        try
        {
            RefreshOwnedMineCount();

            /*
             * The cap belongs to this Miner, not to the board globally.
             * Other Miners may still own up to three different holes each.
             */
            if (ownedMinedTileCount >=
                MaximumOwnedMines)
            {
                return;
            }

            bool timeFromAnimation =
                enemyActor.Definition != null &&
                enemyActor.Definition
                    .TimeSpecialAbilityFromAnimation;

            /*
             * Never start an Ability animation over a gameplay-critical auto
             * attack. If that attack is waiting for AutoAttackImpact, the ready
             * special is retained and AnimationActionReleased retries afterward.
             */
            if (enemyActor
                    .IsAutoAttackAnimationActionActive)
            {
                return;
            }

            bool ownsSpecialAnimationAction = false;

            if (timeFromAnimation)
            {
                ownsSpecialAnimationAction =
                    enemyActor
                        .TryBeginSpecialAbilityAnimationAction();

                if (!ownsSpecialAnimationAction)
                {
                    return;
                }
            }

            bool queued =
                boardController.TryQueueMineRandomCell(
                    enemyActor,
                    MaximumOwnedMines,
                    timeFromAnimation
                );

            if (!queued)
            {
                if (ownsSpecialAnimationAction)
                {
                    /*
                     * Releasing action ownership raises AnimationActionReleased.
                     * The re-entrancy guard above prevents a failed queue attempt
                     * from recursively retrying itself on the same call stack.
                     */
                    enemyActor
                        .EndSpecialAbilityAnimationAction();
                }

                return;
            }

            enemyActor.NotifySpecialAbilityUsed();

            /*
             * Reset only after the board accepts the request. If no valid tile
             * can be mined, the ready state remains visible/debuggable instead
             * of silently consuming another five player moves.
             */
            enemyActor.ResetSpecialCounter();
        }
        finally
        {
            isAttemptingReadyAbility = false;
        }
    }

    private void HandleSpecialAbilityImpactReached(
        EnemyActor impactEnemy)
    {
        if (impactEnemy == null ||
            impactEnemy != enemyActor ||
            impactEnemy.IsDefeated ||
            boardController == null ||
            impactEnemy.Definition == null ||
            !impactEnemy.Definition
                .TimeSpecialAbilityFromAnimation)
        {
            return;
        }

        boardController.NotifyMineAnimationImpact(
            impactEnemy
        );

        /*
         * The gameplay-critical frame has happened. Release action ownership so
         * a ready auto attack may proceed even if the visual clip has a tail.
         */
        impactEnemy.EndSpecialAbilityAnimationAction();
    }

    private void HandleEnemyDefeated(
        EnemyActor defeatedEnemy)
    {
        QueueOwnedTileRestoration();
        Unsubscribe();
    }

    private void QueueOwnedTileRestoration()
    {
        if (restoreQueued ||
            boardController == null ||
            ownerInstanceId == 0)
        {
            return;
        }

        RefreshOwnedMineCount();

        /*
         * A Miner killed before its first activation owns no persistent
         * board state, so do not briefly lock the board for a no-op restore.
         */
        if (ownedMinedTileCount <= 0)
        {
            return;
        }

        restoreQueued = true;

        boardController.QueueRestoreMinedCells(
            ownerInstanceId
        );
    }

    private void RefreshOwnedMineCount()
    {
        ownedMinedTileCount =
            boardController != null &&
            ownerInstanceId != 0
                ? boardController
                    .GetMinedCellCountForOwner(
                        ownerInstanceId
                    )
                : 0;
    }

    private void OnDestroy()
    {
        /*
         * Defeat normally restores the holes first. This fallback also keeps
         * prototype wave-clears or unexpected object destruction from leaving
         * permanent mined cells behind.
         */
        QueueOwnedTileRestoration();

        if (enemyActor != null)
        {
            enemyActor.EndSpecialAbilityAnimationAction();
        }

        Unsubscribe();
    }
}
