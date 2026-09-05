using System.Collections.Generic;
using UnityEngine;

public sealed class RoyalBannerAuraRuntime : MonoBehaviour
{
    private BoardController boardController;
    private IReadOnlyList<EnemyActor> activeEnemies;
    private int bannerId;
    private float attackSpeedMultiplier = 1f;

    private readonly HashSet<EnemyAutoAttack>
        affectedAutoAttacks =
            new HashSet<EnemyAutoAttack>();

    public static void Install(
        BoardController board,
        IReadOnlyList<EnemyActor> enemies,
        int installedBannerId,
        float speedMultiplier)
    {
        if (board == null ||
            enemies == null ||
            installedBannerId <= 0)
        {
            return;
        }

        RoyalBannerAuraRuntime runtime =
            board.gameObject.AddComponent<RoyalBannerAuraRuntime>();

        runtime.Initialize(
            board,
            enemies,
            installedBannerId,
            speedMultiplier
        );
    }

    private void Initialize(
        BoardController board,
        IReadOnlyList<EnemyActor> enemies,
        int installedBannerId,
        float speedMultiplier)
    {
        boardController = board;
        activeEnemies = enemies;
        bannerId = installedBannerId;
        attackSpeedMultiplier =
            Mathf.Clamp(speedMultiplier, 1f, 5f);

        boardController.RoyalBannerRemoved +=
            HandleBannerRemoved;

        RefreshAffectedEnemies();
    }

    private void Update()
    {
        RefreshAffectedEnemies();
    }

    private void RefreshAffectedEnemies()
    {
        if (activeEnemies == null)
        {
            return;
        }

        List<EnemyAutoAttack> stale = null;

        foreach (EnemyAutoAttack autoAttack
                 in affectedAutoAttacks)
        {
            EnemyActor actor =
                autoAttack != null
                    ? autoAttack.EnemyActor
                    : null;

            if (autoAttack != null &&
                actor != null &&
                actor.IsInitialized &&
                !actor.IsDefeated &&
                actor.Definition != null &&
                actor.Definition.CrownSoldier)
            {
                continue;
            }

            stale ??= new List<EnemyAutoAttack>();
            stale.Add(autoAttack);
        }

        if (stale != null)
        {
            foreach (EnemyAutoAttack autoAttack in stale)
            {
                RemoveSpeedModifier(autoAttack);
                affectedAutoAttacks.Remove(autoAttack);
            }
        }

        for (int index = 0;
             index < activeEnemies.Count;
             index++)
        {
            EnemyActor actor = activeEnemies[index];

            if (actor == null ||
                !actor.IsInitialized ||
                actor.IsDefeated ||
                actor.Definition == null ||
                !actor.Definition.CrownSoldier)
            {
                continue;
            }

            EnemyAutoAttack autoAttack =
                actor.GetComponent<EnemyAutoAttack>();

            if (autoAttack == null ||
                affectedAutoAttacks.Contains(autoAttack))
            {
                continue;
            }

            autoAttack.SetRuntimeAttackSpeedMultiplier(
                attackSpeedMultiplier
            );

            affectedAutoAttacks.Add(autoAttack);
        }
    }

    private void HandleBannerRemoved(
        int removedBannerId)
    {
        if (removedBannerId != bannerId)
        {
            return;
        }

        Cleanup();
        Destroy(this);
    }

    private void RemoveSpeedModifier(
        EnemyAutoAttack autoAttack)
    {
        if (autoAttack == null)
        {
            return;
        }

        /*
         * Never erase a later system's different runtime modifier. With the
         * current Royal encounter constraints only one Standard Bearer can be
         * active, but this guard also keeps cleanup safe if another modifier is
         * introduced later.
         */
        if (Mathf.Approximately(
                autoAttack.RuntimeAttackSpeedMultiplier,
                attackSpeedMultiplier))
        {
            autoAttack.ResetRuntimeAttackSpeedMultiplier();
        }
    }

    private void Cleanup()
    {
        if (boardController != null)
        {
            boardController.RoyalBannerRemoved -=
                HandleBannerRemoved;
        }

        foreach (EnemyAutoAttack autoAttack
                 in affectedAutoAttacks)
        {
            RemoveSpeedModifier(autoAttack);
        }

        affectedAutoAttacks.Clear();
    }

    private void OnDestroy()
    {
        Cleanup();
    }
}
