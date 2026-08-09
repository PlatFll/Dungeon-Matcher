using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(EnemyActor))]
public sealed class CrossbowGuardEnemyAbility :
    MonoBehaviour,
    IEnemySpecialAbilityRuntime
{
    private const int MaximumOwnedPins = 2;

    [Header("Runtime Debug Information")]
    [SerializeField]
    private int ownedPinCount;

    private EnemyActor enemyActor;
    private BoardController boardController;

    private int ownerInstanceId;
    private bool releaseQueued;

    public void InitializeSpecialAbility(
        EnemyActor initializedEnemy,
        BoardController initializedBoard)
    {
        Unsubscribe();

        enemyActor = initializedEnemy;
        boardController = initializedBoard;

        releaseQueued = false;

        ownerInstanceId =
            enemyActor != null
                ? enemyActor.GetInstanceID()
                : 0;

        RefreshOwnedPinCount();

        if (enemyActor == null ||
            boardController == null)
        {
            Debug.LogError(
                "CrossbowGuardEnemyAbility requires an initialized " +
                "EnemyActor and BoardController.",
                this
            );

            return;
        }

        Subscribe();
    }

    private void Subscribe()
    {
        if (enemyActor == null ||
            boardController == null)
        {
            return;
        }

        enemyActor.SpecialBecameReady -=
            HandleSpecialBecameReady;

        enemyActor.SpecialBecameReady +=
            HandleSpecialBecameReady;

        enemyActor.Defeated -=
            HandleEnemyDefeated;

        enemyActor.Defeated +=
            HandleEnemyDefeated;

        boardController.ValidPlayerMoveCompleted -=
            HandleValidPlayerMoveCompleted;

        boardController.ValidPlayerMoveCompleted +=
            HandleValidPlayerMoveCompleted;
    }

    private void Unsubscribe()
    {
        if (enemyActor != null)
        {
            enemyActor.SpecialBecameReady -=
                HandleSpecialBecameReady;

            enemyActor.Defeated -=
                HandleEnemyDefeated;
        }

        if (boardController != null)
        {
            boardController.ValidPlayerMoveCompleted -=
                HandleValidPlayerMoveCompleted;
        }
    }

    private void HandleSpecialBecameReady(
        EnemyActor readyEnemy)
    {
        if (readyEnemy == null ||
            readyEnemy != enemyActor ||
            readyEnemy.IsDefeated)
        {
            return;
        }

        TryFireBolt();
    }

    private void HandleValidPlayerMoveCompleted(
        int completedMoveNumber)
    {
        if (enemyActor == null ||
            enemyActor.IsDefeated ||
            !enemyActor.IsSpecialReady)
        {
            return;
        }

        /*
         * Normally SpecialBecameReady fires the shot immediately. This retry
         * covers the rare case where no safe target existed on that exact
         * board state; the ready charge is retained until a later valid move
         * produces a legal target.
         */
        TryFireBolt();
    }

    private void TryFireBolt()
    {
        if (enemyActor == null ||
            boardController == null ||
            enemyActor.IsDefeated ||
            !enemyActor.IsSpecialReady)
        {
            return;
        }

        RefreshOwnedPinCount();

        if (ownedPinCount >=
            MaximumOwnedPins)
        {
            /*
             * The guard still attempts its ability every three player turns,
             * but cannot stockpile a hidden ready shot beyond the two-bolt cap.
             * Once a bolt breaks, a fresh three-turn cycle is required.
             */
            enemyActor.ResetSpecialCounter();
            return;
        }

        bool queued =
            boardController.TryQueuePinRandomGem(
                enemyActor,
                MaximumOwnedPins
            );

        if (!queued)
        {
            return;
        }

        enemyActor.ResetSpecialCounter();
    }

    private void HandleEnemyDefeated(
        EnemyActor defeatedEnemy)
    {
        QueueOwnedPinRelease();
        Unsubscribe();
    }

    private void QueueOwnedPinRelease()
    {
        if (releaseQueued ||
            boardController == null ||
            ownerInstanceId == 0)
        {
            return;
        }

        RefreshOwnedPinCount();

        if (ownedPinCount <= 0)
        {
            return;
        }

        releaseQueued = true;

        boardController.QueueReleasePinnedGems(
            ownerInstanceId
        );
    }

    private void RefreshOwnedPinCount()
    {
        ownedPinCount =
            boardController != null &&
            ownerInstanceId != 0
                ? boardController
                    .GetPinnedGemCountForOwner(
                        ownerInstanceId
                    )
                : 0;
    }

    private void OnDestroy()
    {
        QueueOwnedPinRelease();
        Unsubscribe();
    }
}
