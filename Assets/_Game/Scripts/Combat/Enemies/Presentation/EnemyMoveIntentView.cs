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
    private TMP_Text buffs;
    private RectTransform bar;
    private Image fill;
    private RectTransform link;
    private RectTransform visual;
    private RectTransform weakness;
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
        weakness=slot.GetComponentInChildren<EnemyWeaknessIndicatorUI>(true)?.transform as RectTransform;
        buffs=GameUi.Label("EnemyBuffs",slot.transform,"",new Vector2(56,20),Vector2.zero,8);
        buffs.textWrappingMode=TextWrappingModes.NoWrap;
        buffs.color=new Color(.72f,.94f,.5f);buffs.raycastTarget=false;
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
        buffs.gameObject.SetActive(visible && (actor.IsWarded || actor.FortifiedStacks>0));
        buffs.text=actor.FortifiedStacks>0?$"{(actor.IsWarded?"WARD / FORT":"FORTIFIED")} {actor.FortifiedStacks}":actor.IsWarded?"WARDED":"";
        buffs.rectTransform.sizeDelta=new Vector2(actor.FortifiedStacks>0?56:42,20);
        buffs.color=actor.FortifiedStacks>0?new Color(1,.55f,.79f):new Color(.72f,.94f,.5f);
        if(!visible) { if(link!=null) link.gameObject.SetActive(false);return; }
        // Follow the actual rendered canvas after the pixel presenter resizes
        // it. A fixed slot-center offset crosses the face on taller screens.
        PositionIntent();
        bool casting=channel!=null && channel.IsChanneling;
        var roots=GetComponent<RootbinderEnemyAbility>();
        var milestone=GetComponent<ForestMilestoneEnemyAbility>();
        bool ritual=milestone!=null && milestone.IsPreparing;
        var pressure=GetComponent<ForestPressureAbility>();
        bool warning=pressure!=null && pressure.IsPreparing;
        var recipient=casting?channel.Target:null;
        var aquatic=GetComponent<AquaticEnemyAbility>();
        text.text=casting ? $"HEAL IN {channel.ResponseMoves}" :
            milestone!=null && milestone.IsPreparing ? $"{milestone.CastName} IN {milestone.ResponseMoves}" :
            milestone!=null && milestone.BlocksBasic ? "CASTING" :
            roots!=null && roots.IsWarning ? $"ROOT IN {roots.ResponseMoves}" :
            warning ? $"{pressure.CastName} IN {pressure.ResponseMoves}" :
            CombatMoveClock.MoveBasics ? $"HIT IN {Mathf.CeilToInt(attack?.RemainingAttackTime ?? 0)}" :
            $"HIT {attack?.RemainingAttackTime ?? 0:0.0}s";
        text.color=casting||ritual||warning?new Color(.65f,1f,.45f):Color.white;
        if(aquatic!=null && aquatic.IsPreparing)
        { text.text=$"{aquatic.CastName} IN {aquatic.ResponseMoves}"; text.color=new Color(.6f,.92f,1f); recipient=aquatic.Target; }
        bar.gameObject.SetActive(casting||ritual);
        if(casting||ritual) fill.rectTransform.sizeDelta=new Vector2(64*Mathf.Clamp01((casting?channel.ResponseMoves/2f:(float)milestone.ResponseMoves/milestone.ChannelMoves)),4);
        if(link!=null)
        {
            link.gameObject.SetActive(recipient!=null && recipient!=actor && !actor.IsDefeated);
            if(link.gameObject.activeSelf)
            {
                var parent=(RectTransform)link.parent;
                var targetView=recipient.GetComponent<EnemyMoveIntentView>();
                targetView?.PositionIntent();
                Vector2 from=parent.InverseTransformPoint(text.rectTransform.TransformPoint(new Vector3(0,-12,0)));
                Vector2 to=parent.InverseTransformPoint(targetView!=null && targetView.text!=null
                    ? targetView.text.rectTransform.TransformPoint(new Vector3(0,-12,0)) : recipient.transform.position);
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
        if(buffs!=null && weakness!=null)
        {
            // Share the weakness lane. Below it is the battle frame's mask;
            // above the sprite is already reserved for cast/attack counters.
            buffs.rectTransform.position=weakness.TransformPoint(new Vector3(weakness.rect.xMax+3+buffs.rectTransform.rect.width/2,weakness.rect.center.y,0));
            GameplayPixelGrid.Snap(buffs.rectTransform);
        }
    }
    private void OnDisable() { if(text!=null) text.gameObject.SetActive(false);if(buffs!=null) buffs.gameObject.SetActive(false);if(link!=null) link.gameObject.SetActive(false); }
    private void OnDestroy() { if(text!=null) Destroy(text.gameObject);if(buffs!=null) Destroy(buffs.gameObject);if(link!=null) Destroy(link.gameObject); }
}
