using UnityEngine;
using UnityEngine.UI;

public static class CasterSigilArt
{
    private static readonly Sprite[] sprites=new Sprite[3];
    public static Sprite ForSlot(int slot)
    {
        if(slot<0 || slot>2)return null;
        if(sprites[slot]==null)sprites[slot]=Resources.Load<Sprite>("UI/CasterSigils/"+new[]{"Triangle","Square","Ring"}[slot]);
        return sprites[slot];
    }
}
[DefaultExecutionOrder(11300)]
public sealed class EnemySlotSigilView : MonoBehaviour
{
    private EnemySlotUI slot;
    private Image image;
    public int SlotIndex {get;private set;}=-1;
    private void Awake()=>slot=GetComponent<EnemySlotUI>();
    private void LateUpdate()
    {
        var actor=slot.CurrentEnemy;
        SlotIndex=actor!=null && !actor.IsDefeated ? RunSession.Current?.Waves?.ContinuationSlot(actor)??-1 : -1;
        if(CombatMoveClock.Unified) {if(image!=null)image.enabled=false;return;}
        if(SlotIndex<0 || SlotIndex>2 || slot.CombatBarRect==null) {if(image!=null)image.enabled=false;return;}
        if(image==null)
        {
            var rect=GameUi.Rect("CasterSlotSigil",transform,new Vector2(12,12),Vector2.zero);
            image=rect.gameObject.AddComponent<Image>();image.raycastTarget=false;
        }
        image.sprite=CasterSigilArt.ForSlot(SlotIndex);image.enabled=image.sprite!=null;
        var bar=slot.CombatBarRect;
        image.rectTransform.position=bar.TransformPoint(new Vector3(bar.rect.xMax+8,bar.rect.center.y,0));
        GameplayPixelGrid.Snap(image.rectTransform);
    }
    private void OnDisable(){if(image!=null)image.enabled=false;}
}
