using UnityEngine;

// Presentation only. Native overlay assets can replace the shared frozen-gem
// fallback independently of the board-owned ice stage and response deadline.
[DisallowMultipleComponent]
public sealed class FrostwakeGemView : MonoBehaviour
{
    private SpriteRenderer overlay;
    public void Show(FrostStage stage,FrostwakeRules rules,Sprite fallback)
    {
        if(overlay==null)
        {
            var child=new GameObject("FrostwakeIce");child.transform.SetParent(transform,false);
            overlay=child.AddComponent<SpriteRenderer>();
            var gem=GetComponent<SpriteRenderer>();
            overlay.sortingLayerName=gem!=null?gem.sortingLayerName:"Gems";
            overlay.sortingOrder=(gem!=null?gem.sortingOrder:0)+13;
            overlay.maskInteraction=SpriteMaskInteraction.VisibleInsideMask;
        }
        var sprite=rules==null?null:stage==FrostStage.Frost?rules.frostOverlay:
            stage==FrostStage.HeavyIce?rules.heavyIceOverlay:rules.bifrostOverlay;
        overlay.sprite=sprite!=null?sprite:fallback;
        overlay.enabled=stage!=FrostStage.None && overlay.sprite!=null;
        overlay.color=sprite!=null?Color.white:stage==FrostStage.Frost?new Color(.8f,.95f,1f,.55f):
            stage==FrostStage.HeavyIce?new Color(.35f,.8f,1f,.85f):new Color(1f,.65f,.95f,1f);
    }
    private void OnDestroy(){if(overlay!=null)Destroy(overlay.gameObject);}
}
