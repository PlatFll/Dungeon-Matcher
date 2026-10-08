using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Read-only two-row view of authoritative attack, warning and slot state.</summary>
[DefaultExecutionOrder(11010)]
public sealed class EnemyUnifiedIntentView : MonoBehaviour
{
    private EnemyActor actor;
    private EnemyAutoAttack attack;
    private RectTransform root, visual, link, wave;
    private Image sigil, warning;
    private PixelCounterText basicCount, specialCount;
    private CombatStatusStrip statuses;
    private readonly List<CombatStatusDisplay> displayed = new List<CombatStatusDisplay>();
    public RectTransform ActionRoot => root;
    public CombatStatusStrip Statuses => statuses;

    private void Start()
    {
        actor=GetComponent<EnemyActor>(); attack=GetComponent<EnemyAutoAttack>();
        var slot=GetComponentInParent<EnemySlotUI>();
        if(actor==null || slot==null) {enabled=false;return;}
        visual=transform.Find("VisualRoot") as RectTransform;
        wave=Object.FindFirstObjectByType<GameplayPixelLayoutController>()?.TopHud.Find("WaveTracker") as RectTransform;
        root=GameUi.Rect("ActionIntent",slot.transform,new Vector2(44,44),Vector2.zero);
        var sword=GameUi.Rect("Sword",root,new Vector2(24,24),new Vector2(-9,11)).gameObject.AddComponent<Image>();
        sword.sprite=CombatStatusStrip.Icon("Sword"); sword.raycastTarget=false;
        sigil=GameUi.Rect("Slot",root,new Vector2(12,12),new Vector2(-9,-11)).gameObject.AddComponent<Image>();
        sigil.raycastTarget=false;
        basicCount=PixelCounterText.Create("AttackMoves",root,new Vector2(20,22),new Vector2(12,11),20);
        specialCount=PixelCounterText.Create("SpecialMoves",root,new Vector2(20,22),new Vector2(12,-11),20);
        warning=GameUi.Rect("ImpactUnderline",root,new Vector2(40,2),new Vector2(0,-23)).gameObject.AddComponent<Image>();
        warning.raycastTarget=false; warning.color=new Color32(255,191,81,255);
        statuses=GameUi.Rect("EnemyStatuses",slot.transform,Vector2.zero,Vector2.zero).gameObject.AddComponent<CombatStatusStrip>();
        var canvas=GetComponentInParent<Canvas>();
        if(canvas!=null)
        {
            link=GameUi.Rect("HealTargetLink",canvas.transform,Vector2.one,Vector2.zero);
            var image=link.gameObject.AddComponent<Image>();image.color=new Color(.6f,.95f,.4f,.85f);image.raycastTarget=false;
            link.pivot=new Vector2(0,.5f);
        }
    }
    private void LateUpdate()
    {
        if(root==null || actor==null)return;
        bool visible=!actor.IsDefeated;
        root.gameObject.SetActive(visible);
        if(!visible) {statuses.gameObject.SetActive(false);if(link!=null)link.gameObject.SetActive(false);return;}
        int slot=RunSession.Current?.Waves?.ContinuationSlot(actor)??-1;
        sigil.sprite=CasterSigilArt.ForSlot(slot);
        int response=ResponseMoves(); bool preparing=response>=0;
        bool special=actor.HasSpecialAbility && actor.Definition.SpecialAbilityKind!=EnemySpecialAbilityKind.Bloodrage;
        var ivory=new Color32(253,245,229,255);
        basicCount.Set(Mathf.Max(0,Mathf.CeilToInt(attack?.RemainingAttackTime??0)).ToString(),ivory);
        specialCount.Set(preparing?response.ToString():special?Mathf.Max(0,actor.SpecialTurnRequirement-actor.CurrentSpecialTurnCount).ToString():"",
            preparing?warning.color:ivory);
        sigil.color=special || preparing?ivory:new Color32(146,143,140,255);
        warning.enabled=preparing;
        CombatStatusDisplay.Enemy(actor,displayed);
        int columns=Mathf.Max(1,Mathf.FloorToInt(((RectTransform)root.parent).rect.width/28));
        statuses.Show(displayed,columns,description=>RunSession.Current?.GetComponent<RunControlsUI>()?.OpenGuide(description));
        if(visual!=null)
        {
            root.position=visual.TransformPoint(new Vector3(visual.rect.center.x+12,visual.rect.yMax,0));
            root.anchoredPosition+=Vector2.up*26;
            if(wave!=null)
            {
                // Oversized motion canvases include transparent headroom. Keep
                // their entire HUD plus transient cast copy below the header.
                float ceiling=root.parent.InverseTransformPoint(wave.TransformPoint(new Vector3(0,wave.rect.yMin,0))).y;
                float maximum=ceiling-6-44-statuses.Rect.rect.height-4-root.rect.height*.5f;
                if(root.localPosition.y>maximum)
                {
                    var position=root.localPosition;position.y=maximum;position.x+=40;root.localPosition=position;
                }
            }
            statuses.Rect.position=root.TransformPoint(new Vector3(-12,root.rect.yMax+4+statuses.Rect.rect.height/2,0));
            GameplayPixelGrid.Snap(root); GameplayPixelGrid.Snap(statuses.Rect);
        }
        UpdateTargetLink();
    }
    private void UpdateTargetLink()
    {
        if(link==null)return;
        var channel=GetComponent<EnemyChannelRuntime>();
        var aquatic=GetComponent<AquaticEnemyAbility>();
        var recipient=channel?.IsChanneling==true?channel.Target:aquatic?.IsPreparing==true?aquatic.Target:null;
        link.gameObject.SetActive(recipient!=null && recipient!=actor && !recipient.IsDefeated);
        if(!link.gameObject.activeSelf)return;
        var targetView=recipient.GetComponent<EnemyUnifiedIntentView>();
        var parent=(RectTransform)link.parent;
        // Link beneath the rows, outside the character canvases and number glyphs.
        Vector2 from=parent.InverseTransformPoint(root.TransformPoint(new Vector3(0,root.rect.yMin,0)));
        Vector2 to=parent.InverseTransformPoint(targetView?.ActionRoot!=null
            ?targetView.ActionRoot.TransformPoint(new Vector3(0,targetView.ActionRoot.rect.yMin,0)):recipient.transform.position);
        Vector2 delta=to-from;link.localPosition=from;link.sizeDelta=new Vector2(delta.magnitude,1);
        link.localRotation=Quaternion.Euler(0,0,Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg);
    }
    private int ResponseMoves()
    {
        var heal=GetComponent<EnemyChannelRuntime>(); if(heal?.IsChanneling==true)return heal.ResponseMoves;
        var roots=GetComponent<RootbinderEnemyAbility>(); if(roots?.IsWarning==true)return roots.ResponseMoves;
        var forest=GetComponent<ForestMilestoneEnemyAbility>(); if(forest?.IsPreparing==true)return forest.ResponseMoves;
        var pressure=GetComponent<ForestPressureAbility>(); if(pressure?.IsPreparing==true)return pressure.ResponseMoves;
        var aquatic=GetComponent<AquaticEnemyAbility>(); if(aquatic?.IsPreparing==true)return aquatic.ResponseMoves;
        var mine=GetComponent<MineEnemyAbility>(); if(mine?.IsPilot==true)return mine.PilotMoves;
        if(mine?.IsPreparing==true)return mine.ResponseMoves;
        int king=GetComponent<KingEnemyAbility>()?.WarningMovesRemaining??-1; if(king>=0)return king;
        int minister=GetComponent<RoyalArchbishopEnemyAbility>()?.WarningMovesRemaining??-1; if(minister>=0)return minister;
        return GetComponent<SiegeSergeantEnemyAbility>()?.WarningMovesRemaining??-1;
    }
    private void OnDisable() {if(root!=null)root.gameObject.SetActive(false);if(statuses!=null)statuses.gameObject.SetActive(false);if(link!=null)link.gameObject.SetActive(false);}
    private void OnDestroy() {if(root!=null)Destroy(root.gameObject);if(statuses!=null)Destroy(statuses.gameObject);if(link!=null)Destroy(link.gameObject);}
}
