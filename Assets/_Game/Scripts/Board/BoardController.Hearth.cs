using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BoardController
{
    private readonly HashSet<int> extinguishingHearthOwners=new HashSet<int>();
    public event Action<int> HearthMoveAdvanced;
    public event Action<int> HearthDestroyed;
    public bool HasHearthEmber(int owner=0)
    {
        foreach(var state in barricadeCells.Values)
            if(state.Style==EnemyBarricadeStyle.HearthEmber && (owner==0 || state.OwnerInstanceId==owner))return true;
        return false;
    }
    public void QueueExtinguishHearth(int owner)
    {
        if(owner==0 || !isActiveAndEnabled || !HasHearthEmber(owner))return;
        extinguishingHearthOwners.Add(owner);
        EnqueueBoardMutation(new BoardMutationRequest{Kind=BoardMutationKind.RemoveHearth,OwnerInstanceId=owner});
        TryStartBoardMutationProcessor();
    }
    private IEnumerator ExecuteRemoveHearth(int owner)
    {
        bool removed=false;
        foreach(var pair in new List<KeyValuePair<Vector2Int,BarricadeCellState>>(barricadeCells))
        {
            if(pair.Value.Style!=EnemyBarricadeStyle.HearthEmber || pair.Value.OwnerInstanceId!=owner)continue;
            barricadeCells.Remove(pair.Key);DestroyBarricadeView(pair.Value);
            NotifyRoyalBannerSpaceOpened(pair.Key.x,pair.Key.y);removed=true;
        }
        extinguishingHearthOwners.Remove(owner);
        if(removed)yield return ResolveEnvironmentalBoardChange();
    }
    private void HearthBroken(Vector2Int cell,BarricadeCellState state)
    {
        if(state.Style!=EnemyBarricadeStyle.HearthEmber)return;
        if(extinguishingHearthOwners.Contains(state.OwnerInstanceId))return;
        HearthDestroyed?.Invoke(state.OwnerInstanceId);
        if(ResolvingEnvironmentalMutation)return;
        int thawed=0;
        // Stable cardinal order, one layer per physical gem, never retaliation.
        foreach(var direction in BarricadeHitDirections)
        {
            var gem=GetGem(cell.x+direction.x,cell.y+direction.y);
            if(gem==null || !frostGems.TryGetValue(gem.BoardIdentity,out var ice))continue;
            ice.Hit(FrostMove);if(ice.stage==FrostStage.None)frostGems.Remove(ice.identity);
            if(++thawed==2)break;
        }
        if(thawed>0)PublishFrost();
    }
}
