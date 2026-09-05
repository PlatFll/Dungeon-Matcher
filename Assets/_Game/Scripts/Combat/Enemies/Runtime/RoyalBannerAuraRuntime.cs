using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class RoyalBannerAuraRuntime : MonoBehaviour
{
    private BoardController boardController;
    private IReadOnlyList<EnemyActor> activeEnemies;
    private float attackSpeedMultiplier = 1f;
    private bool isInitialized;
    private bool isCleanedUp;

    private readonly HashSet<int>
        activeBannerIds =
            new HashSet<int>();

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
            board.GetComponent<RoyalBannerAuraRuntime>();

        if (runtime == null)
        {
            runtime =
                board.gameObject.AddComponent<
                    RoyalBannerAuraRuntime>();
        }

        runtime.RegisterBanner(
            board,
            enemies,
            installedBannerId,
            speedMultiplier
        );
    }

    private void RegisterBanner(
        BoardController board,
        IReadOnlyList<EnemyActor> enemies,
        int installedBannerId,
        float speedMultiplier)
    {
        if (!isInitialized)
        {
            boardController = board;
            activeEnemies = enemies;
            attackSpeedMultiplier =
                Mathf.Clamp(speedMultiplier, 1f, 5f);
            isInitialized = true;
            isCleanedUp = false;

            boardController.RoyalBannerRemoved +=
                HandleBannerRemoved;
        }
        else
        {
            /*
             * All current standards use the same aura value. Keeping the
             * strongest registered value makes this coordinator safe if that
             * tuning later becomes data-driven without allowing duplicate
             * standards to stack multiplicatively.
             */
            attackSpeedMultiplier =
                Mathf.Max(
                    attackSpeedMultiplier,
                    Mathf.Clamp(speedMultiplier, 1f, 5f)
                );
        }

        activeBannerIds.Add(installedBannerId);
        RefreshAffectedEnemies();
    }

    private void Update()
    {
        if (activeBannerIds.Count > 0)
        {
            RefreshAffectedEnemies();
        }
    }

    private void RefreshAffectedEnemies()
    {
        if (activeEnemies == null ||
            activeBannerIds.Count == 0)
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

            if (autoAttack == null)
            {
                continue;
            }

            if (!affectedAutoAttacks.Contains(autoAttack))
            {
                affectedAutoAttacks.Add(autoAttack);
            }

            if (!Mathf.Approximately(
                    autoAttack.RuntimeAttackSpeedMultiplier,
                    attackSpeedMultiplier))
            {
                autoAttack.SetRuntimeAttackSpeedMultiplier(
                    attackSpeedMultiplier
                );
            }
        }
    }

    private void HandleBannerRemoved(
        int removedBannerId)
    {
        if (!activeBannerIds.Remove(removedBannerId))
        {
            return;
        }

        /*
         * Multiple standards do not stack their speed bonus, but either one
         * keeps the shared Crown aura alive. Removing one banner therefore
         * never clears the remaining standard's effect.
         */
        if (activeBannerIds.Count > 0)
        {
            RefreshAffectedEnemies();
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
         * Never erase a later system's different runtime modifier. Only undo
         * the value that this aura is still visibly responsible for.
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
        if (isCleanedUp)
        {
            return;
        }

        isCleanedUp = true;

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
        activeBannerIds.Clear();
    }

    private void OnDestroy()
    {
        Cleanup();
    }
}
