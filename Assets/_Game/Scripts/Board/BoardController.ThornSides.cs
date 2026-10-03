using System.Collections.Generic;
using UnityEngine;

public partial class BoardController
{
    private int ChooseThornSafeSide(Vector2Int cell)
    {
        var clearable=ImmediatelyClearableOrdinaryCells();
        var sides=new List<int>();
        foreach(var direction in BarricadeHitDirections)
            if(clearable.Contains(cell+direction)) sides.Add(SideBit(direction));
        return sides.Count==0?0:sides[GameplayRandom.Range(0,sides.Count)];
    }
    private void RefreshThornSides(BarricadeCellState state,float extent)
    {
        // The approved sprite has a smooth top edge. Rotate it to the saved
        // safe side; the markers stay in board coordinates for clear feedback.
        var safeDirection=Vector2Int.up;
        foreach(var direction in BarricadeHitDirections)
            if((state.ThornSafeSide&SideBit(direction))!=0) safeDirection=direction;
        var rotation=Quaternion.Euler(0,0,Mathf.Atan2(safeDirection.y,safeDirection.x)*Mathf.Rad2Deg-90);
        state.ViewObject.transform.localRotation=rotation;
        foreach(var direction in BarricadeHitDirections)
        {
            string name="ThornSide_"+SideBit(direction);
            var existing=state.ViewObject.transform.Find(name);
            if(existing!=null) continue;
            bool safe=(state.ThornSafeSide&SideBit(direction))!=0;
            var marker=new GameObject(name);marker.transform.SetParent(state.ViewObject.transform,false);
            marker.transform.localPosition=Quaternion.Inverse(rotation)*(Vector3)(new Vector2(direction.x,direction.y)*extent*.46f);
            marker.transform.localRotation=Quaternion.Inverse(rotation);
            var view=marker.AddComponent<SpriteRenderer>();view.sprite=GetBarricadeFallbackSprite();
            marker.transform.localScale=new Vector3((direction.x==0?extent*.6f:extent*.03f)/view.sprite.bounds.size.x,
                (direction.y==0?extent*.6f:extent*.03f)/view.sprite.bounds.size.y,1);
            view.sortingLayerName="Gems";view.sortingOrder=23;view.maskInteraction=SpriteMaskInteraction.VisibleInsideMask;
            view.color=safe?new Color(.8f,1,.55f):new Color(1,.38f,.17f);
        }
    }
}
