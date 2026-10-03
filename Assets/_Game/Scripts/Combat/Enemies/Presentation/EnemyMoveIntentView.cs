using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Development move intent, using runtime text and an explicit target link.</summary>
[DefaultExecutionOrder(11000)]
[DisallowMultipleComponent]
public sealed class EnemyMoveIntentView : MonoBehaviour
{
    private EnemyActor actor;
    private EnemyAutoAttack attack;
    private EnemyChannelRuntime channel;
    private TMP_Text text;
    private RectTransform bar;
    private Image fill;
    private RectTransform link;
    private RectTransform visual;
    public void Initialize(EnemyActor owner)
    {
        actor=owner;
        if(actor==null) return;
        attack=actor.GetComponent<EnemyAutoAttack>();
        channel=actor.GetComponent<EnemyChannelRuntime>();
        visual=actor.transform.Find("VisualRoot") as RectTransform;
    }
    private void Start()
    {
        if(actor==null) Initialize(GetComponent<EnemyActor>());
        if(actor==null || !CombatMoveClock.Active) { enabled=false;return; }
        var slot=GetComponentInParent<EnemySlotUI>();
        if(slot==null) return;
        text=GameUi.Label("MoveIntent",slot.transform,"",new Vector2(108,16),Vector2.zero,12);
        text.textWrappingMode=TextWrappingModes.NoWrap;
        bar=GameUi.Rect("ChannelTrack",text.transform,new Vector2(64,4),new Vector2(0,-9));
        var track=bar.gameObject.AddComponent<Image>();track.color=new Color(.12f,.18f,.12f);track.raycastTarget=false;
        var f=GameUi.Rect("RemainingMoves",bar,new Vector2(64,4),Vector2.zero);fill=f.gameObject.AddComponent<Image>();fill.color=new Color(.6f,.9f,.4f);
        fill.raycastTarget=false;
        f.anchorMin=f.anchorMax=f.pivot=new Vector2(0,.5f);f.anchoredPosition=Vector2.zero;
        var canvas=GetComponentInParent<Canvas>();
        if(canvas!=null)
        {
            link=GameUi.Rect("HealTargetLink",canvas.transform,Vector2.one,Vector2.zero);
            var image=link.gameObject.AddComponent<Image>();image.color=new Color(.6f,.95f,.4f,.85f);image.raycastTarget=false;
            link.pivot=new Vector2(0,.5f);link.SetAsLastSibling();
        }
    }
    private void LateUpdate()
    {
        if(text==null || actor==null) return;
        bool visible=!actor.IsDefeated;
        text.gameObject.SetActive(visible);
        if(!visible) { if(link!=null) link.gameObject.SetActive(false);return; }
        // Follow the actual rendered canvas after the pixel presenter resizes
        // it. A fixed slot-center offset crosses the face on taller screens.
        PositionIntent();
        bool casting=channel!=null && channel.IsChanneling;
        var roots=GetComponent<RootbinderEnemyAbility>();
        text.text=casting ? $"HEAL IN {channel.ResponseMoves}" : channel!=null && channel.BlocksBasic ? "RECOVER" :
            roots!=null && roots.IsWarning ? $"VINES IN {roots.ResponseMoves}" :
            $"HIT IN {Mathf.CeilToInt(attack?.RemainingAttackTime ?? 0)}";
        text.color=casting?new Color(.65f,1f,.45f):Color.white;
        bar.gameObject.SetActive(casting);
        if(casting) fill.rectTransform.sizeDelta=new Vector2(64*Mathf.Clamp01(channel.ResponseMoves/2f),4);
        if(link!=null)
        {
            link.gameObject.SetActive(casting && channel.Target!=null && !actor.IsDefeated);
            if(link.gameObject.activeSelf)
            {
                var parent=(RectTransform)link.parent;
                var targetView=channel.Target.GetComponent<EnemyMoveIntentView>();
                targetView?.PositionIntent();
                Vector2 from=parent.InverseTransformPoint(text.rectTransform.TransformPoint(new Vector3(0,-12,0)));
                Vector2 to=parent.InverseTransformPoint(targetView!=null && targetView.text!=null
                    ? targetView.text.rectTransform.TransformPoint(new Vector3(0,-12,0)) : channel.Target.transform.position);
                Vector2 delta=to-from;link.localPosition=new Vector3(from.x,from.y,0);link.sizeDelta=new Vector2(delta.magnitude,2);
                link.localRotation=Quaternion.Euler(0,0,Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg);
            }
        }
    }
    private void PositionIntent()
    {
        if(text==null || visual==null) return;
        text.rectTransform.position=visual.TransformPoint(new Vector3(visual.rect.center.x,visual.rect.yMax,0));
        text.rectTransform.anchoredPosition+=Vector2.up*16;
        GameplayPixelGrid.Snap(text.rectTransform);
    }
    private void OnDisable() { if(text!=null) text.gameObject.SetActive(false);if(link!=null) link.gameObject.SetActive(false); }
    private void OnDestroy() { if(text!=null) Destroy(text.gameObject);if(link!=null) Destroy(link.gameObject); }
}
