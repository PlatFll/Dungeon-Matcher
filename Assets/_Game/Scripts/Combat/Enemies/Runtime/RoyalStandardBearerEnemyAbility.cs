using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(EnemyActor))]
public sealed class RoyalStandardBearerEnemyAbility :
    MonoBehaviour,
    IEnemySpecialAbilityRuntime
{
    private const int MaximumOwnedBanners = 1;
    private const float BannerAttackSpeedMultiplier = 1.20f;

    private EnemyActor enemyActor;
    private BoardController boardController;
    private IReadOnlyList<EnemyActor> activeEnemies;
    private EnemySpecialActionAvailability specialActionAvailability;
    private int ownerInstanceId;
    private bool ownershipOrphaned;

    public void InitializeSpecialAbility(
        EnemyActor initializedEnemy,
        BoardController initializedBoard,
        IReadOnlyList<EnemyActor> initializedActiveEnemies)
    {
        specialActionAvailability?.Dispose();
        Unsubscribe();

        enemyActor = initializedEnemy;
        boardController = initializedBoard;
        activeEnemies = initializedActiveEnemies;
        ownerInstanceId =
            enemyActor != null
                ? enemyActor.GetInstanceID()
                : 0;
        ownershipOrphaned = false;

        if (enemyActor == null ||
            boardController == null ||
            activeEnemies == null)
        {
            Debug.LogError(
                "RoyalStandardBearerEnemyAbility requires an initialized " +
                "EnemyActor, BoardController, and active-enemy roster.",
                this
            );
            return;
        }

        specialActionAvailability =
            new EnemySpecialActionAvailability(
                this,
                enemyActor,
                boardController,
                TryRaiseStandard
            );

        Subscribe();
    }

    private void Subscribe()
    {
        enemyActor.SpecialBecameReady -= HandleSpecialBecameReady;
        enemyActor.SpecialBecameReady += HandleSpecialBecameReady;
        enemyActor.Defeated -= HandleEnemyDefeated;
        enemyActor.Defeated += HandleEnemyDefeated;

        boardController.ValidPlayerMoveCompleted -=
            HandleValidPlayerMoveCompleted;
        boardController.ValidPlayerMoveCompleted +=
            HandleValidPlayerMoveCompleted;
    }

    private void Unsubscribe()
    {
        if (enemyActor != null)
        {
            enemyActor.SpecialBecameReady -= HandleSpecialBecameReady;
            enemyActor.Defeated -= HandleEnemyDefeated;
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
        if (readyEnemy == enemyActor &&
            readyEnemy != null &&
            !readyEnemy.IsDefeated)
        {
            specialActionAvailability?.RequestExecution();
        }
    }

    private void HandleValidPlayerMoveCompleted(
        int completedMoveNumber)
    {
        if (enemyActor != null &&
            !enemyActor.IsDefeated &&
            enemyActor.IsSpecialReady)
        {
            specialActionAvailability?.RequestExecution();
        }
    }

    private bool TryRaiseStandard()
    {
        if (enemyActor == null ||
            boardController == null ||
            enemyActor.IsDefeated ||
            !enemyActor.IsSpecialReady)
        {
            return false;
        }

        if (boardController.GetRoyalBannerCountForOwner(
                ownerInstanceId) >= MaximumOwnedBanners)
        {
            /*
             * Do not stockpile an instant replacement while a banner is still
             * travelling. Once it reaches the bottom, a fresh cadence is
             * required before this bearer can plant another standard.
             */
            enemyActor.ResetSpecialCounter();
            return true;
        }

        bool queued =
            boardController.TryQueuePlaceRoyalBanner(
                enemyActor,
                HandleBannerPlacementCompleted
            );

        if (!queued)
        {
            return false;
        }

        enemyActor.ResetSpecialCounter();
        return true;
    }

    private void HandleBannerPlacementCompleted(
        bool succeeded)
    {
        if (!succeeded ||
            boardController == null ||
            activeEnemies == null)
        {
            return;
        }

        int bannerId =
            boardController.GetRoyalBannerIdForOwner(
                ownerInstanceId
            );

        if (bannerId <= 0)
        {
            return;
        }

        RoyalBannerAuraRuntime.Install(
            boardController,
            activeEnemies,
            bannerId,
            BannerAttackSpeedMultiplier
        );
    }

    private void HandleEnemyDefeated(
        EnemyActor defeatedEnemy)
    {
        specialActionAvailability?.Dispose();
        OrphanOwnedBanner();
        Unsubscribe();
    }

    private void OrphanOwnedBanner()
    {
        if (ownershipOrphaned ||
            boardController == null ||
            ownerInstanceId == 0)
        {
            return;
        }

        ownershipOrphaned = true;

        /*
         * Killing the bearer does not erase an already-planted standard. Its
         * board object and aura persist until gravity carries it to row zero.
         */
        boardController.OrphanRoyalBannerForOwner(
            ownerInstanceId
        );
    }

    private void OnDestroy()
    {
        specialActionAvailability?.Dispose();
        OrphanOwnedBanner();
        Unsubscribe();
    }
}
