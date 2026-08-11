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

    public void InitializeSpecialAbility(
        EnemyActor initializedEnemy,
        BoardController initializedBoard)
    {
        Unsubscribe();

        enemyActor = initializedEnemy;
        boardController = initializedBoard;

        restoreQueued = false;

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
            readyEnemy.Definition != null &&
            readyEnemy.Definition
                .TimeSpecialAbilityFromAnimation;

        bool queued =
            boardController.TryQueueMineRandomCell(
                readyEnemy,
                MaximumOwnedMines,
                timeFromAnimation
            );

        if (!queued)
        {
            return;
        }

        readyEnemy.NotifySpecialAbilityUsed();

        /*
         * Reset only after the board accepts the request. If no valid tile
         * can be mined, the ready state remains visible/debuggable instead
         * of silently consuming another five player moves.
         */
        readyEnemy.ResetSpecialCounter();
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
        Unsubscribe();
    }
}
