using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class VineNodeSnapshot
{
    // Old gem identity fields remain only for migration of prototype saves.
    public int gemId, bornMove, ownerSlot = -1, limit = 3;
    public long ownerId;
    public bool environmental, nonSpreading;
    public bool cellOverlay;
    public int x, y, rootId;
}

public partial class BoardController
{
    private const int EnvironmentalVineOwner = -2147483000;
    private readonly List<VineNodeSnapshot> vineNodes = new List<VineNodeSnapshot>();
    private readonly Dictionary<Vector2Int, GameObject> vineViews = new Dictionary<Vector2Int, GameObject>();
    private int nextVineGrowthMove = 2, nextRootId;
    public int VineCount { get { PruneVines(); return vineNodes.Count; } }
    public int NextVineGrowthMove => nextVineGrowthMove;
    public int RestrictionCount => pinnedGemOwners.Count + pendingPinTargetOwners.Count;
    public event Action RootsChanged;

    private bool IsReservedVine(Gem gem) => gemSetThreats.Exists(t => t.Vine && !t.Ended && t.Targets.Contains(gem));
    private bool IsProtectedWarningTarget(Gem gem)
    {
        if(gem!=null && cellResponseThreats.Exists(t=>!t.Ended && t.Cells.Contains(new Vector2Int(gem.Column,gem.Row)))) return true;
        foreach(var pair in gemPairThreats) if(!pair.Ended && (pair.First==gem || pair.Second==gem)) return true;
        foreach(var set in gemSetThreats) if(!set.Ended && set.Targets.Contains(gem)) return true;
        foreach(var lane in laneThreats) if(!lane.Ended && (lane.Row==gem.Row || lane.Column==gem.Column)) return true;
        return false;
    }
    private Gem FindVineGem(int identity)
    {
        foreach(var gem in gems) if(gem!=null && gem.BoardIdentity==identity) return gem;
        return null;
    }
    private EnemyActor VineOwner(long id)
    {
        if(RunSession.Current?.Waves==null) return null;
        foreach(var actor in RunSession.Current.Waves.ActiveEnemies)
            if(actor!=null && !actor.IsDefeated && actor.PersistentId==id) return actor;
        return null;
    }
    private void PruneVines()
    {
        foreach(var node in new List<VineNodeSnapshot>(vineNodes))
            if(!IsCellPlayable(node.x,node.y)) RemoveVine(node);
    }
    public bool IsVineGem(Gem gem, out bool anchor)
    { anchor=false;return gem!=null && vineNodes.Exists(n=>n.x==gem.Column && n.y==gem.Row); }
    public bool IsCellVined(int x,int y) => vineNodes.Exists(n=>n.x==x && n.y==y);
    public int OwnedVineCount(EnemyActor owner) => owner==null?0:vineNodes.FindAll(n=>n.ownerId==owner.PersistentId).Count;
    public int OwnedRootCount(EnemyActor owner) => owner==null?0:GetBarricadeCountForOwner(owner.GetInstanceID());
    private static bool IsRoot(BarricadeCellState state) => state!=null &&
        (state.Style==EnemyBarricadeStyle.Root || state.Style==EnemyBarricadeStyle.Heartroot);

    // Roots reuse structural occupancy, useful-response checks, durability hits,
    // refill and the existing board mutation queue. Vines never enter pin maps.
    public bool TryQueueRootWarning(EnemyActor owner,int count,int durability,bool heart,bool spreading,Action<GemSetThreat> completed)
    {
        if(owner==null || owner.IsDefeated || OwnedRootCount(owner)>0) return false;
        var request=new BoardMutationRequest { Kind=BoardMutationKind.MarkGemSet,OwnerActor=owner,
            TargetCount=count,WarningMoves=1,Vine=true,RootDurability=durability,
            BarricadeStyle=heart?EnemyBarricadeStyle.Heartroot:EnemyBarricadeStyle.Root,RootSpreading=spreading };
        request.Completed=ok=>completed?.Invoke(ok?request.SetThreat:null);
        EnqueueBoardMutation(request);TryStartBoardMutationProcessor();return true;
    }
    public bool TryQueuePlantRoots(EnemyActor owner,int count,int durability,bool heart,bool spreading,Action<bool> completed)
    {
        if(owner==null || owner.IsDefeated || OwnedRootCount(owner)>0) return false;
        EnqueueBoardMutation(new BoardMutationRequest { Kind=BoardMutationKind.PlaceBarricades,OwnerActor=owner,
            OwnerInstanceId=owner.GetInstanceID(),BarricadeCount=count,MaximumOwnedBarricades=count,
            BarricadeDurability=durability,BarricadeStyle=heart?EnemyBarricadeStyle.Heartroot:EnemyBarricadeStyle.Root,
            RootSpreading=spreading,ProtectSpecialGems=true,Completed=completed });
        TryStartBoardMutationProcessor();return true;
    }
    private bool CanHostRoot(Gem gem)
    {
        if(!IsEnvironmentalOrdinaryGem(gem) || IsGemPinned(gem)) return false;
        foreach(var d in BarricadeHitDirections)
        {
            int x=gem.Column+d.x,y=gem.Row+d.y;
            if(!IsCellPlayable(x,y) || GetGem(x,y)==null) return false;
        }
        return true;
    }
    public bool TryQueueResolveVines(GemSetThreat threat,Action<bool> completed)
    {
        if(threat==null || !threat.Vine || threat.Ended || threat.Queued || completedValidPlayerMoves<threat.DueMove) return false;
        threat.Queued=true;
        EnqueueBoardMutation(new BoardMutationRequest { Kind=BoardMutationKind.ResolveVines,OwnerActor=threat.Owner,
            SetThreat=threat,Completed=completed });TryStartBoardMutationProcessor();return true;
    }
    private IEnumerator ExecuteResolveVines(BoardMutationRequest request)
    {
        var threat=request.SetThreat;
        if(threat==null || threat.Ended) yield break;
        CancelGemSetThreat(threat);
        if(!TelegraphOwnerCanExecute(threat.Owner) || threat.RootDurability<=0) yield break;
        request.OwnerInstanceId=threat.Owner.GetInstanceID();
        request.BarricadeCount=threat.Targets.Count;request.MaximumOwnedBarricades=threat.Targets.Count;
        request.BarricadeDurability=threat.RootDurability;request.BarricadeStyle=threat.RootStyle;
        request.RootSpreading=threat.RootSpreading;request.ProtectSpecialGems=true;
        yield return ExecutePlaceBarricadesRequest(request);
    }
    private void SeedRoot(Vector2Int cell,BarricadeCellState state,EnemyActor owner)
    {
        foreach(var d in BarricadeHitDirections)
            AddVine(cell+d,owner?.PersistentId ?? 0,state.RootId,!state.RootSpreading,false);
    }
    private void AddVine(Vector2Int cell,long ownerId,int rootId,bool nonSpreading,bool environmental)
    {
        if(!IsCellPlayable(cell.x,cell.y) || GetGem(cell.x,cell.y)==null || IsCellVined(cell.x,cell.y)) return;
        var node=new VineNodeSnapshot {cellOverlay=true,x=cell.x,y=cell.y,rootId=rootId,ownerId=ownerId,
            bornMove=completedValidPlayerMoves,nonSpreading=nonSpreading,environmental=environmental};
        vineNodes.Add(node);CreateVineView(node,true);
    }
    private GameplayThemeDefinition VineTheme => GameplayThemeSkin.Current ?? Resources.Load<GameplayThemeDefinition>("Zones/ForestTheme");
    private void CreateVineView(VineNodeSnapshot node,bool animate=false)
    {
        var cell=new Vector2Int(node.x,node.y);
        if(vineViews.ContainsKey(cell)) return;
        var theme=VineTheme;
        var sprite=theme?.vineOverlay;
        if(sprite==null) return; // Optional art cannot affect legal play.
        var view=new GameObject("VineOverlay_"+node.x+"_"+node.y);view.transform.SetParent(transform,false);
        view.transform.localPosition=GetCellLocalPosition(node.x,node.y);
        view.transform.localScale=Vector3.one*(cellSize/Mathf.Max(sprite.bounds.size.x,sprite.bounds.size.y));
        var renderer=view.AddComponent<SpriteRenderer>();renderer.sprite=sprite;
        renderer.sortingLayerName="Gems";renderer.sortingOrder=8;renderer.maskInteraction=SpriteMaskInteraction.VisibleInsideMask;
        view.AddComponent<VineOverlayMotion>().Initialize(renderer,sprite,theme.vineSpreadFrames,animate && RunSession.Current?.Continuation?.IsRestoring!=true);
        vineViews[cell]=view;
    }
    private void RemoveVine(VineNodeSnapshot node,bool damaged=false)
    {
        AnswerCellResponse(new Vector2Int(node.x,node.y),true);
        vineNodes.Remove(node);var cell=new Vector2Int(node.x,node.y);
        foreach(var d in BarricadeHitDirections)
            if(barricadeCells.TryGetValue(cell+d,out var root) && IsRoot(root)) root.OpenRootSides |= SideBit(-d);
        if(vineViews.TryGetValue(cell,out var view))
        {
            vineViews.Remove(cell);
            if(view!=null)
            {
                if(damaged && RunSession.Current?.Continuation?.IsRestoring!=true && view.TryGetComponent<VineOverlayMotion>(out var motion))
                    motion.Retire(VineTheme?.vineHitFrames);
                else {view.SetActive(false);Destroy(view);}
            }
        }
    }
    private void ClearVinesForDestruction(HashSet<Gem> cleared,HashSet<Gem> preserved)
    {
        if(cleared==null) return;
        foreach(var gem in cleared)
        {
            if(gem==null || (preserved!=null && preserved.Contains(gem))) continue;
            AnswerCellResponse(new Vector2Int(gem.Column,gem.Row),false);
            var node=vineNodes.Find(n=>n.x==gem.Column && n.y==gem.Row);
            if(node==null) continue;
            // Opening is recorded after this clear's root-hit check. This clear
            // opens a side; a later distinct clear must actually strike it.
            foreach(var d in BarricadeHitDirections)
            {
                var cell=new Vector2Int(node.x+d.x,node.y+d.y);
                if(barricadeCells.TryGetValue(cell,out var state) && IsRoot(state))
                    state.OpenRootSides |= SideBit(-d);
            }
            RemoveVine(node,true);
        }
    }
    private static int SideBit(Vector2Int direction)
    { for(int i=0;i<BarricadeHitDirections.Length;i++) if(BarricadeHitDirections[i]==direction) return 1<<i;return 0; }
    private bool RootAcceptsHit(Vector2Int rootCell,BarricadeCellState state,Gem source)
    { return !IsCellVined(source.Column,source.Row) && (state.OpenRootSides&SideBit(new Vector2Int(source.Column,source.Row)-rootCell))!=0; }
    private void RootDestroyed(BarricadeCellState state)
    {
        if(!IsRoot(state)) return;
        foreach(var node in new List<VineNodeSnapshot>(vineNodes)) if(node.rootId==state.RootId) RemoveVine(node);
        RootsChanged?.Invoke();
    }
    public bool QueueVineSurge(EnemyActor owner,Action<bool> completed)
    {
        if(owner==null || owner.IsDefeated) return false;
        EnqueueBoardMutation(new BoardMutationRequest {Kind=BoardMutationKind.AdvanceVines,OwnerActor=owner,Completed=completed});
        TryStartBoardMutationProcessor();return true;
    }
    public bool QueueVineHarvest(EnemyActor owner,Action<int> harvested,Action<bool> completed)
    {
        if(owner==null || owner.IsDefeated) return false;
        EnqueueBoardMutation(new BoardMutationRequest {Kind=BoardMutationKind.HarvestVines,OwnerActor=owner,VinesHarvested=harvested,Completed=completed});
        TryStartBoardMutationProcessor();return true;
    }
    private void ExecuteVineHarvest(BoardMutationRequest request)
    {
        if(!TelegraphOwnerCanExecute(request.OwnerActor)) return;
        int count=VineCount;
        foreach(var node in new List<VineNodeSnapshot>(vineNodes)) RemoveVine(node);
        // Consumption also opens root sides; their occupancy/durability remains.
        foreach(var state in barricadeCells.Values) if(IsRoot(state)) state.OpenRootSides=15;
        request.Succeeded=true;request.VinesHarvested?.Invoke(count);
    }
    public IEnumerator AdvanceVineNetworks(int move)
    {
        if(move<nextVineGrowthMove) yield break;
        int interval=RunSession.Current?.Zone?.Definition?.vineCadenceMoves ?? 2;
        // The normal deadline advances only here, never from a forced surge.
        nextVineGrowthMove=move+Mathf.Max(1,interval);
        EnqueueBoardMutation(new BoardMutationRequest {Kind=BoardMutationKind.AdvanceVines,EnvironmentalPin=true});
        TryStartBoardMutationProcessor();while(IsBusy) yield return null;
    }
    private void ExecuteVineGrowth(BoardMutationRequest request)
    {
        if(request.OwnerActor!=null && !TelegraphOwnerCanExecute(request.OwnerActor)) return;
        PruneVines();var candidates=new List<VineNodeSnapshot>();
        foreach(var node in new List<VineNodeSnapshot>(vineNodes))
        {
            if(node.nonSpreading || (!node.environmental && VineOwner(node.ownerId)==null)) continue;
            foreach(var d in BarricadeHitDirections) candidates.Add(new VineNodeSnapshot {x=node.x+d.x,y=node.y+d.y,
                ownerId=node.ownerId,rootId=node.rootId,environmental=node.environmental});
        }
        // Living spreading roots can regrow after a harvest; the weak root cannot.
        foreach(var entry in barricadeCells) if(IsRoot(entry.Value) && entry.Value.RootSpreading)
            foreach(var d in BarricadeHitDirections) candidates.Add(new VineNodeSnapshot {x=entry.Key.x+d.x,y=entry.Key.y+d.y,
                ownerId=entry.Value.RootOwnerId,rootId=entry.Value.RootId});
        int cap=RunSession.Current?.Zone?.Definition?.maximumVineOverlays ?? 12;
        int spreadLimit=RunSession.Current?.Zone?.Definition?.maximumVineSpreadPerPulse ?? 2;
        int added=0;
        // Use only the initial frontier: no recursive growth in the same pulse.
        while(candidates.Count>0 && vineNodes.Count<cap && added<spreadLimit)
        {
            int index=GameplayRandom.Range(0,candidates.Count);var node=candidates[index];candidates.RemoveAt(index);
            int before=vineNodes.Count;AddVine(new Vector2Int(node.x,node.y),node.ownerId,node.rootId,false,node.environmental);
            if(vineNodes.Count>before) added++;
        }
        if(request.EnvironmentalPin && RunSession.Current?.Zone?.Definition?.growsVines==true && vineNodes.Count<cap)
        {
            var edges=new List<Vector2Int>();
            for(int y=0;y<height;y++) for(int x=0;x<width;x++)
                if((x==0||y==0||x==width-1||y==height-1) && IsCellPlayable(x,y) && GetGem(x,y)!=null && !IsCellVined(x,y)) edges.Add(new Vector2Int(x,y));
            if(edges.Count>0) AddVine(edges[GameplayRandom.Range(0,edges.Count)],0,0,false,true);
        }
        request.Succeeded=true;
    }
    public void QueueEnvironmentalVine(Gem target)
    {
        if(target==null || !IsCellPlayable(target.Column,target.Row)) return;
        EnqueueBoardMutation(new BoardMutationRequest {Kind=BoardMutationKind.AddVine,TargetGem=target});TryStartBoardMutationProcessor();
    }
    public void RemoveVineSource(EnemyActor owner)
    {
        if(this==null || !isActiveAndEnabled || gems==null) return;
        foreach(var warning in new List<GemSetThreat>(gemSetThreats))
            if(warning.Vine && (owner!=null?warning.Owner==owner:warning.Environmental)) CancelGemSetThreat(warning);
        EnqueueBoardMutation(new BoardMutationRequest {Kind=BoardMutationKind.RemoveVines,OwnerInstanceId=owner!=null?owner.GetInstanceID():0,
            VineOwnerId=owner!=null?owner.PersistentId:0});TryStartBoardMutationProcessor();
    }
    private IEnumerator ExecuteRemoveVineSource(BoardMutationRequest request)
    {
        foreach(var node in new List<VineNodeSnapshot>(vineNodes))
            if(request.VineOwnerId==0?node.environmental:node.ownerId==request.VineOwnerId) RemoveVine(node);
        bool removed=false;
        foreach(var entry in new List<KeyValuePair<Vector2Int,BarricadeCellState>>(barricadeCells))
            if(IsRoot(entry.Value) && entry.Value.RootOwnerId==request.VineOwnerId && request.VineOwnerId!=0)
            { barricadeCells.Remove(entry.Key);QueueRoyalBannerGravityOpening(entry.Key.x,entry.Key.y);CancelBarricadeVisualRoutine(entry.Value);DestroyBarricadeView(entry.Value);removed=true; }
        if(removed) { RootsChanged?.Invoke();yield return ResolveEnvironmentalBoardChange(); }
        request.Succeeded=true;
    }
    public List<VineNodeSnapshot> CaptureVines(Func<int,int> ownerSlot)
    {
        PruneVines();var result=new List<VineNodeSnapshot>();
        foreach(var node in vineNodes)
        {
            var saved=JsonUtility.FromJson<VineNodeSnapshot>(JsonUtility.ToJson(node));var actor=VineOwner(node.ownerId);
            saved.ownerSlot=actor==null?-1:ownerSlot(actor.GetInstanceID());
            saved.gemId=GetGem(node.x,node.y)?.BoardIdentity ?? 0;result.Add(saved);
        }
        return result;
    }
    public void RestoreVines(List<VineNodeSnapshot> saved)
    {
        foreach(var view in vineViews.Values) if(view!=null) {view.SetActive(false);Destroy(view);}
        vineViews.Clear();vineNodes.Clear();
        // Old committed travel snapshots could carry forest overlays into the
        // dungeon. Retire them before the first destination frame is revealed.
        if(saved==null || RunSession.Current?.Zone?.Definition?.growsVines==false) return;
        foreach(var original in saved)
        {
            var node=JsonUtility.FromJson<VineNodeSnapshot>(JsonUtility.ToJson(original));
            if(!node.cellOverlay)
            {
                // Upgrade former gem-bound vine pins, leaving real chains intact.
                var gem=FindVineGem(node.gemId);if(gem==null || node.nonSpreading) continue;
                node.x=gem.Column;node.y=gem.Row;node.cellOverlay=true;
            }
            if(IsCellPlayable(node.x,node.y) && !IsCellVined(node.x,node.y)) {vineNodes.Add(node);CreateVineView(node);}
        }
    }
    public GemSetThreat RestoredVineCast(EnemyActor owner) => gemSetThreats.Find(t=>t.Owner==owner && t.Vine && !t.Ended);
}
