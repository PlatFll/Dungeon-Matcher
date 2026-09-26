using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BoardController
{
    private Func<Gem, bool> targetSelection;
    public bool IsSelectingTarget => targetSelection != null;

    public void SetTargetSelection(Func<Gem, bool> accept)
    {
        pointerStartGem = null;
        ClearSelection();
        targetSelection = accept;
    }

    public bool TryClearPlayerArea(Gem center, int radius, Func<bool> commitUse)
    {
        if (IsBusy || IsExternalInputBlocked || center == null || !IsGemStillOnBoard(center) ||
            combatController == null || combatController.PlayerActor == null ||
            combatController.PlayerActor.IsDefeated || !combatController.WaveController.IsWaveActive ||
            Time.timeScale <= 0 || commitUse == null) return false;
        var targets = new HashSet<Gem>();
        radius = Mathf.Clamp(radius, 0, 2);
        for (int x = center.Column - radius; x <= center.Column + radius; x++)
            for (int y = center.Row - radius; y <= center.Row + radius; y++)
            {
                var gem = GetGem(x, y);
                if (gem != null && IsCellPlayable(x, y)) targets.Add(gem);
            }
        if (targets.Count == 0) return false;
        isBusy = true;
        // Persistence accepts the item transaction while the board owns the action.
        // No gameplay effect runs before this succeeds, and no coroutine can interleave.
        bool committed = false;
        try { committed = commitUse(); }
        finally { if (!committed) isBusy = false; }
        if (!committed) return false;
        targetSelection = null;
        StartCoroutine(ResolvePlayerAreaClear(targets));
        return true;
    }

    private IEnumerator ResolvePlayerAreaClear(HashSet<Gem> targets)
    {
        try
        {
            var expanded = BuildBombExpandedClearSet(targets, true, out var crystals);
            ReportBombClearsToCombat(null, expanded, 0, BoardClearSource.Bomb);
            yield return ClearMatchesWithBurstTargets(expanded, null, activateSpecials: true, genericBurstTargets: targets);
            yield return ResolveBombTriggeredCrystalRequests(crystals);
            yield return CollapseAndRefillBoard();
            var matches = FindAllMatches();
            if (matches.Count > 0) yield return ResolveCascades(matches, null, null);
            else if (!HasAvailableMove()) yield return ReshuffleBoard();
        }
        finally { isBusy = false; }
    }
}
