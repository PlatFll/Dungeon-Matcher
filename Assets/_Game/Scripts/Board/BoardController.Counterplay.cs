using System;
using System.Collections.Generic;
using UnityEngine;

public partial class BoardController
{
    public string DescribeOwnedBoardThreats(EnemyActor owner)
    {
        if(owner==null) return "";
        var text=new System.Text.StringBuilder();
        int id=owner.GetInstanceID(),pins=0,mines=0,barriers=0,banners=0;
        foreach(var pin in pinnedGemOwners) if(pin.Value==id) pins++;
        foreach(var mine in minedCellOwners) if(mine.Value==id) mines++;
        foreach(var barrier in barricadeCells.Values) if(barrier.OwnerInstanceId==id) barriers++;
        foreach(var banner in royalBannerCells.Values) if(banner.OwnerInstanceId==id) banners++;
        if(pins+mines+barriers+banners>0) text.AppendLine($"Owned restrictions: {pins} chains/ice, {mines} holes, {barriers} barricades, {banners} banners.");
        foreach(var pair in gemPairThreats) if(pair.Owner==owner && IsGemPairThreatValid(pair))
            text.AppendLine($"Hammer: clear either marked gem within {Mathf.Max(0,pair.DueMove-completedValidPlayerMoves)} moves.");
        foreach(var set in gemSetThreats) if(set.Owner==owner && !set.Ended)
            text.AppendLine($"{(set.RestorationPresentation?"Restoration":"Judgment")}: {set.Targets.Count} marks remain, {Mathf.Max(0,set.DueMove-completedValidPlayerMoves)} moves left.");
        foreach(var lane in laneThreats) if(lane.Owner==owner && !lane.Ended)
            text.AppendLine($"Bombardment: row {lane.Row+1}, column {lane.Column+1}, {Mathf.Max(0,lane.DueMove-completedValidPlayerMoves)} moves left. Clearing the lane does not cancel it.");
        return text.ToString();
    }
    public sealed class ResponseOption
    {
        public Gem Source;
        public Gem Target;
        public readonly HashSet<Gem> Clears = new HashSet<Gem>();
        public bool UsesSpecial;
        public bool CreatesSpecial;
        public bool BreaksObstacle;
    }

    // Combat supplies the current goals; this board query contains no character
    // or enemy-specific rules and never clears gems or awards anything.
    public Func<IReadOnlyList<ResponseOption>, bool> UsefulResponseValidator { get; set; }

    private bool RetainsUsefulResponse()
    {
        return HasAvailableMove() && (UsefulResponseValidator == null || UsefulResponseValidator(GetImmediateResponses()));
    }

    public List<ResponseOption> GetImmediateResponses()
    {
        var result = new List<ResponseOption>();
        if (gems == null) return result;
        var types = BuildCurrentTypeGrid();
        var crystals = BuildCurrentCrystalGrid();
        var candidates = new List<HintMoveCandidate>();
        for (int y = 0; y < height; y++) for (int x = 0; x < width; x++)
        {
            if (x + 1 < width) AddHintCandidatesForSwap(types, crystals, x, y, x + 1, y, candidates);
            if (y + 1 < height) AddHintCandidatesForSwap(types, crystals, x, y, x, y + 1, candidates);
        }
        foreach (var candidate in candidates)
        {
            var a = candidate.SourceGem; var b = candidate.TargetGem;
            var option = new ResponseOption { Source = a, Target = b,
                UsesSpecial = a.SpecialType == GemSpecialType.ColorCrystal || b.SpecialType == GemSpecialType.ColorCrystal };
            if (option.UsesSpecial) { option.Clears.Add(a); option.Clears.Add(b); result.Add(option); continue; }
            types[a.Column,a.Row] = b.Type; types[b.Column,b.Row] = a.Type;
            var atFirst=new HashSet<Gem>();var atSecond=new HashSet<Gem>();
            CollectResponseLine(a.Column,a.Row,a,b,types,crystals,atFirst);
            CollectResponseLine(b.Column,b.Row,a,b,types,crystals,atSecond);
            option.CreatesSpecial=atFirst.Count>=4 || atSecond.Count>=4;
            option.Clears.UnionWith(atFirst);option.Clears.UnionWith(atSecond);
            types[a.Column,a.Row] = a.Type; types[b.Column,b.Row] = b.Type;
            foreach (var gem in option.Clears)
            {
                option.UsesSpecial |= gem.SpecialType!=GemSpecialType.None;
                option.BreaksObstacle |= IsGemPinned(gem);
                foreach (var direction in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
                {
                    var cell = new Vector2Int(gem.Column, gem.Row) + direction;
                    option.BreaksObstacle |= barricadeCells.ContainsKey(cell) || (direction==Vector2Int.up && royalBannerCells.ContainsKey(cell));
                }
            }
            result.Add(option);
        }
        return result;
    }

    private void CollectResponseLine(int x, int y, Gem a, Gem b, GemType[,] types, bool[,] crystals, HashSet<Gem> clears)
    {
        foreach (var axis in new[] { Vector2Int.right, Vector2Int.up })
        {
            var cells = new List<Vector2Int> { new Vector2Int(x,y) };
            foreach (int sign in new[] { -1, 1 })
                for (int step = 1; ; step++)
                {
                    int cx=x+axis.x*step*sign, cy=y+axis.y*step*sign;
                    if (!IsCellPlayable(cx,cy) || GetGem(cx,cy)==null || crystals[cx,cy] || types[cx,cy]!=types[x,y]) break;
                    cells.Add(new Vector2Int(cx,cy));
                }
            if (cells.Count < 3) continue;
            foreach (var cell in cells)
            {
                Gem gem = GetGem(cell.x,cell.y);
                clears.Add(gem == a ? b : gem == b ? a : gem);
            }
        }
    }

    private HashSet<Gem> ImmediatelyClearableOrdinaryGems()
    {
        var targets = new HashSet<Gem>();
        foreach (var option in GetImmediateResponses())
            foreach (var gem in option.Clears) if (IsOrdinaryGemOnBoard(gem)) targets.Add(gem);
        return targets;
    }

    private int ReserveWarningDeadline(int requestedMoves)
    {
        int due = completedValidPlayerMoves + Mathf.Max(1, requestedMoves);
        foreach (var threat in gemSetThreats)
            if (!threat.Ended && threat.Owner != null && !threat.Owner.IsDefeated)
                due = Mathf.Max(due, threat.DueMove + Mathf.Max(1, requestedMoves));
        foreach (var threat in gemPairThreats)
            if (IsGemPairThreatValid(threat)) due = Mathf.Max(due, threat.DueMove + Mathf.Max(1, requestedMoves));
        foreach (var threat in laneThreats)
            if (!threat.Ended && threat.Owner!=null && !threat.Owner.IsDefeated)
                due=Mathf.Max(due,threat.DueMove+Mathf.Max(1,requestedMoves));
        return due;
    }
}
