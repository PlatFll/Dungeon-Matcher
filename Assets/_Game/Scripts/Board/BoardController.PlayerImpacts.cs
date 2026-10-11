using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>A read-only candidate. Coordinate validity is rechecked at impact.</summary>
public readonly struct PlayerBoardImpactTarget
{
    public readonly Vector2Int Cell;
    public readonly Gem Gem;
    public readonly int Durability;
    public readonly bool Barrier, Vine, Frozen;
    public PlayerBoardImpactTarget(Vector2Int cell, Gem gem, int durability, bool barrier, bool vine, bool frozen)
    { Cell=cell; Gem=gem; Durability=durability; Barrier=barrier; Vine=vine; Frozen=frozen; }
}

public sealed class PlayerBoardImpactSequence
{
    public int Count;
    public float LeadSeconds, StepSeconds;
    public Func<int, IReadOnlyList<PlayerBoardImpactTarget>, Vector2Int?> Select;
    public Func<bool> Cancelled;
    public Action<int, Vector2Int> Approaching;
    public Action<int, Vector2Int, bool> Impacted;
    public Action Completed;
}

public partial class BoardController
{
    // A generic player sequence owns the board through every impact, chain and
    // refill. Character runtimes select targets; they never mutate occupancy.
    public List<PlayerBoardImpactTarget> CollectPlayerImpactTargets(bool includeEmpty = false)
    {
        var result = new List<PlayerBoardImpactTarget>();
        if (gems == null) return result;
        for (int y=0; y<height; y++) for (int x=0; x<width; x++)
        {
            var cell=new Vector2Int(x,y);
            if (IsCellMined(x,y) || IsCellRoyalBanner(x,y)) continue;
            if (barricadeCells.TryGetValue(cell,out var barrier))
            {
                // Root sides must be opened through their existing vine response.
                if (IsRoot(barrier) && barrier.OpenRootSides == 0) continue;
                result.Add(new PlayerBoardImpactTarget(cell,null,barrier.RemainingDurability,true,false,false));
                continue;
            }
            var gem=GetGem(x,y);
            if (gem == null && !includeEmpty) continue;
            result.Add(new PlayerBoardImpactTarget(cell,gem,0,false,
                gem!=null && IsVineGem(gem,out _),gem!=null && IsGemFrozen(gem)));
        }
        return result;
    }

    public bool CanAcceptPlayerImpactSequence => isActiveAndEnabled && gems!=null && !IsBusy &&
        !IsExternalInputBlocked && !IsSelectingTarget && Time.timeScale>0 &&
        combatController?.PlayerActor!=null && !combatController.PlayerActor.IsDefeated &&
        combatController.WaveController!=null && combatController.WaveController.IsWaveActive &&
        CollectPlayerImpactTargets().Count>0;

    public bool TryPlayerImpactSequence(PlayerBoardImpactSequence request)
    {
        if (request==null || request.Select==null || request.Count<1 || !CanAcceptPlayerImpactSequence) return false;
        isBusy=true;
        ClearSelection();
        StartCoroutine(ResolvePlayerImpactSequence(request));
        return true;
    }

    private IEnumerator ResolvePlayerImpactSequence(PlayerBoardImpactSequence request)
    {
        var deferredCrystals=new List<BombTriggeredCrystalRequest>();
        var reservedCrystals=new HashSet<Gem>();
        try
        {
            // Acceptance returns before effects, so the controller can spend
            // energy and emit its one cast cue first.
            yield return null;
            if (request.LeadSeconds>0) yield return new WaitForSeconds(request.LeadSeconds);
            for (int step=0; step<Mathf.Clamp(request.Count,1,32); step++)
            {
                if (request.Cancelled?.Invoke()==true || combatController.PlayerActor.IsDefeated) break;
                var candidates=CollectPlayerImpactTargets(true);
                candidates.RemoveAll(t=>t.Gem!=null && reservedCrystals.Contains(t.Gem));
                var selected=request.Select(step,candidates);
                if (!selected.HasValue) break;
                var cell=selected.Value;
                if (!candidates.Exists(t=>t.Cell==cell)) break;
                request.Approaching?.Invoke(step,cell);
                if (request.StepSeconds>0) yield return new WaitForSeconds(request.StepSeconds);
                if (request.Cancelled?.Invoke()==true || combatController.PlayerActor.IsDefeated) break;
                if (barricadeCells.TryGetValue(cell,out var barrier))
                    DamageBarricadeCell(cell,barrier,true,false);
                else
                {
                    var gem=GetGem(cell.x,cell.y);
                    if (gem!=null)
                    {
                        if (gem.SpecialType==GemSpecialType.ColorCrystal)
                        {
                            deferredCrystals.Add(new BombTriggeredCrystalRequest(gem,GetRandomGemType()));
                            reservedCrystals.Add(gem);
                        }
                        else
                        {
                            var seed=new HashSet<Gem>{gem};
                            var expanded=BuildBombExpandedClearSet(seed,true,out var crystals);
                            foreach (var crystal in crystals)
                                if (reservedCrystals.Add(crystal.CrystalGem)) deferredCrystals.Add(crystal);
                            ReportBombClearsToCombat(null,expanded,0,BoardClearSource.Ability);
                            yield return ClearMatchesWithBurstTargets(expanded,null,true,seed);
                        }
                    }
                }
                request.Impacted?.Invoke(step,cell,barricadeCells.ContainsKey(cell));
            }
            yield return ResolveBombTriggeredCrystalRequests(deferredCrystals);
            yield return CollapseAndRefillBoard();
            var matches=FindAllMatches();
            if (matches.Count>0) yield return ResolveCascades(matches,null,null);
            else if (!HasAvailableMove()) yield return ReshuffleBoard();
        }
        finally
        {
            isBusy=false;
            request.Completed?.Invoke();
        }
    }
}
