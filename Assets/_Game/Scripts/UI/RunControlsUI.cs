using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class RunControlsUI : MonoBehaviour
{
    private RunSession session;
    private Canvas canvas;
    private RectTransform safeRoot, overlay, panel;
    private readonly Button[] slots = new Button[2];
    private readonly Image[] icons = new Image[2], cooldownFills = new Image[2];
    private readonly TMP_Text[] charges = new TMP_Text[2];
    private TMP_Text hint, music, sfx, saveError;
    private TMP_Text motion, vibration;
    private Button confirmBomb;
    private RectTransform abilityRect;
    private readonly System.Collections.Generic.Dictionary<Button, EnemySlotUI> enemyInspectButtons = new System.Collections.Generic.Dictionary<Button, EnemySlotUI>();
    private IDisposable inputBlock;
    private bool paused, victoryShown;
    private bool restoringOverlay;
    private TMP_Text recovery;
    private bool recoveryFailed;
    private string lesson;
    private float lessonUntil;
    private float priorTimeScale;
    private static Sprite cooldownSprite;
    private Image resonance;
    public bool IsModalOpen => overlay!=null;
    public Canvas GameplayCanvas => canvas;
    public Button SettingsButton { get; private set; }
    public void RestoreTimeAfterTransition(float scale) { if(paused) priorTimeScale=scale; }

    private void Start()
    {
        session=GetComponent<RunSession>();
        var ability=FindFirstObjectByType<AbilityButtonUI>();
        if(ability==null)return;
        abilityRect=ability.transform as RectTransform;
        canvas=ability.GetComponentInParent<Canvas>().rootCanvas;
        foreach(var rect in canvas.GetComponentsInChildren<RectTransform>(true))
            if(rect.name=="SafeArea" || rect.name=="SafeAreaRoot") { safeRoot=rect;break; }
        if(safeRoot==null) safeRoot=ability.transform.parent.parent as RectTransform;
        for(int i=0;i<2;i++)
        {
            var kind=(ConsumableKind)i;
            slots[i]=GameUi.Button(kind.ToString(),ability.transform.parent,"",new Vector2(64,64),new Vector2(i==0?-136:136,-17),()=>
            { if(kind==ConsumableKind.HealthPotion) session.TryUsePotion();else session.ToggleBombTargeting(); });
            slots[i].image.sprite=Resources.Load<Sprite>("UI/Consumables/Slot");
            slots[i].image.color=Color.white;
            slots[i].image.type=Image.Type.Simple;
            var tile = Resources.Load<Sprite>("UI/Consumables/SlotHighlighted");
            if (tile != null)
            {
                slots[i].transition=Selectable.Transition.SpriteSwap;
                slots[i].image.CrossFadeColor(Color.white,0,true,true);
                slots[i].spriteState=new SpriteState {
                    highlightedSprite=tile, selectedSprite=tile,
                    pressedSprite=Resources.Load<Sprite>("UI/Consumables/SlotPressed"),
                    disabledSprite=Resources.Load<Sprite>("UI/Consumables/SlotDisabled") };
            }
            else { slots[i].transition=Selectable.Transition.ColorTint; slots[i].spriteState=default; }
            GameplayThemeSkin.Supply(slots[i]);
            var rect=GameUi.Rect("Icon",slots[i].transform,new Vector2(48,48),Vector2.zero);
            icons[i]=rect.gameObject.AddComponent<Image>();
            icons[i].sprite=Resources.Load<Sprite>(i==0?"UI/Consumables/Potion":"UI/Consumables/Bomb");
            icons[i].preserveAspect=true;icons[i].raycastTarget=false;
            var shade=GameUi.Rect("Cooldown",slots[i].transform,new Vector2(52,52),Vector2.zero);
            cooldownFills[i]=shade.gameObject.AddComponent<Image>();
            cooldownFills[i].sprite=TextureSprite();cooldownFills[i].type=Image.Type.Filled;
            cooldownFills[i].fillMethod=Image.FillMethod.Vertical;cooldownFills[i].fillOrigin=0;
            cooldownFills[i].color=new Color(0.05f,0.02f,0.09f,0.75f);cooldownFills[i].raycastTarget=false;
            var countBacking=GameUi.Rect("CountBacking",slots[i].transform,new Vector2(22,16),new Vector2(23,-24));
            countBacking.gameObject.AddComponent<Image>().color=GameUi.Face;
            countBacking.GetComponent<Image>().raycastTarget=false;
            charges[i]=GameUi.Label("Charges",countBacking,"",new Vector2(22,16),Vector2.zero,12);
        }
        hint=GameUi.Label("ConsumableHint",ability.transform.parent,"",new Vector2(400,28),Vector2.zero,16);
        if(session.Zone?.Definition?.theme?.resonanceIcon!=null)
        {
            var badge=GameUi.Rect("ZoneResonance",ability.transform.parent,new Vector2(32,32),Vector2.zero);
            resonance=badge.gameObject.AddComponent<Image>();resonance.sprite=session.Zone.Definition.theme.resonanceIcon;
            resonance.raycastTarget=false;
        }
        confirmBomb=GameUi.Button("ConfirmBomb",safeRoot,"Use Bomb",new Vector2(150,40),new Vector2(0,-100),()=>session.ConfirmBomb());
        gameObject.AddComponent<BombTargetPreview>();
        var settings=GameUi.Button("Settings",safeRoot,"",new Vector2(48,48),Vector2.zero,OpenSettings);
        SettingsButton=settings;
        // The existing settings control stays above transition smoke.
        var settingsCanvas=settings.gameObject.AddComponent<Canvas>();
        settingsCanvas.overrideSorting=true;settingsCanvas.sortingOrder=canvas.sortingOrder+210;
        settings.gameObject.AddComponent<GraphicRaycaster>();
        FinalizedUiSkin.Button(settings, gear:true);
        GameplayThemeSkin.Button(settings,settings:true);
        var settingsRect=(RectTransform)settings.transform;settingsRect.anchorMin=settingsRect.anchorMax=settingsRect.pivot=new Vector2(1,1);
        settingsRect.anchoredPosition=new Vector2(-20,-18);
        var guide=GameUi.Button("CombatGuide",safeRoot,"Guide",new Vector2(88,40),Vector2.zero,()=>OpenGuide(CombatGuide.Basics));
        GameplayThemeSkin.Button(guide);GameplayThemeSkin.Button(confirmBomb);
        var guideRect=(RectTransform)guide.transform;guideRect.anchorMin=guideRect.anchorMax=guideRect.pivot=new Vector2(1,1);guideRect.anchoredPosition=new Vector2(-78,-22);
        foreach (var slot in FindObjectsByType<EnemySlotUI>(FindObjectsSortMode.None))
        {
            if (slot.EnemySpawnAnchor == null) continue;
            var selectedSlot = slot;
            // Spawn anchors contain actors only: summon availability checks their
            // child count while waiting for death presentation to finish.
            var inspect=GameUi.Button("InspectEnemy",slot.transform,"",new Vector2(100,120),Vector2.zero,
                ()=>OpenGuide(CombatGuide.Enemy(selectedSlot.CurrentEnemy)));
            inspect.image.color=Color.clear;
            inspect.transition=Selectable.Transition.None;
            enemyInspectButtons.Add(inspect,slot);
        }
        var saveRetry=GameUi.Button("RetrySave",safeRoot,"Retry save",new Vector2(180,40),new Vector2(0,280),()=>session.RetrySave());
        GameplayThemeSkin.Button(saveRetry);
        saveError=saveRetry.GetComponentInChildren<TMP_Text>();
        saveRetry.gameObject.SetActive(false);
    }
    private static Sprite TextureSprite()
    {
        if(cooldownSprite!=null)return cooldownSprite;
        var texture=new Texture2D(1,1){name="ConsumableCooldown",filterMode=FilterMode.Point};
        texture.SetPixel(0,0,Color.white);texture.Apply();
        return cooldownSprite=Sprite.Create(texture,new Rect(0,0,1,1),new Vector2(.5f,.5f));
    }
    private void Update()
    {
        if(session==null || slots[0]==null)return;
        if(session.Continuation!=null && session.Continuation.IsRestoring && overlay==null)
        {
            BuildOverlay("Continuing your run..."); restoringOverlay=true;
            recovery=GameUi.Label("Recovery",panel,"Restoring your board, build and pending actions.",new Vector2(330,110),Vector2.zero,20);
        }
        if(restoringOverlay && !recoveryFailed && !string.IsNullOrEmpty(session.Continuation.Error))
        {
            recoveryFailed=true;
            recovery.text="This run could not be restored. Your saved run is preserved. Return to the menu to try again.";
            GameUi.Button("RecoveryMenu",panel,"Return to Menu",new Vector2(280,44),new Vector2(0,-120),()=>UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu"));
        }
        foreach (var entry in enemyInspectButtons)
            if (entry.Key != null && entry.Value != null)
            {
                entry.Key.transform.position = entry.Value.EnemySpawnAnchor.position;
                entry.Key.gameObject.SetActive(entry.Value.IsOccupied);
            }
        for(int i=0;i<2;i++)
        {
            var kind=(ConsumableKind)i;int count=session.Charges(kind);float cooldown=session.Cooldown(kind);
            slots[i].interactable=session.CanUse(kind)||(i==1&&session.Board.IsSelectingTarget);
            icons[i].color=count>0?Color.white:new Color(0.3f,0.3f,0.3f,0.6f);
            cooldownFills[i].fillAmount=count==0?1:cooldown/(CombatMoveClock.MoveEffects ? 2 : BalanceV1.Current.consumableCooldown);
            charges[i].text=cooldown>0?$"{count} | {Mathf.CeilToInt(cooldown)}{(CombatMoveClock.MoveEffects ? "m" : "s")}":count.ToString();
            Vector2 countSize=new Vector2(cooldown>0?60:22,16);
            charges[i].rectTransform.sizeDelta=countSize;
            ((RectTransform)charges[i].transform.parent).sizeDelta=countSize;
            ((RectTransform)charges[i].transform.parent).anchoredPosition=new Vector2(cooldown>0?0:23,-24);
        }
        hint.rectTransform.anchoredPosition=new Vector2(0,-((RectTransform)hint.transform.parent).rect.height*.5f+31);
        UpdateLesson();
        hint.rectTransform.sizeDelta=new Vector2(430,28);
        hint.fontSize=9;
        hint.text=session.Board.IsSelectingTarget?"Preview, then confirm. Tap Bomb to cancel.\nBarricades take 1 hit; specials can extend the blast.":lesson ?? (session.IsPractice?"Practice: no stock spent or progression earned.":"");
        if(resonance!=null)
        {
            bool show=!session.Board.IsSelectingTarget && lesson==null;
            resonance.gameObject.SetActive(show);
            resonance.rectTransform.anchoredPosition=hint.rectTransform.anchoredPosition+new Vector2(-120,0);
            if(show) hint.text=session.Zone.CurrentGem+" resonance: +15% gem damage";
        }
        confirmBomb.gameObject.SetActive(session.HasBombPreview && session.Board.IsSelectingTarget);
        if (confirmBomb.gameObject.activeSelf)
        {
            confirmBomb.transform.position = abilityRect.position;
            confirmBomb.interactable=session.CanUse(ConsumableKind.Bomb);
        }
        if(saveError!=null)saveError.transform.parent.gameObject.SetActive(session.NeedsSaveRetry);
        if(session.IsVictory&&!session.Board.IsBusy&&!victoryShown){victoryShown=true;OpenVictory();}
    }
    public void OpenSettings()
    {
        if(session==null||session.IsFinished||overlay!=null)return;
        session.CancelTargeting();BuildOverlay("Settings");Pause();
        panel.sizeDelta=new Vector2(420,690);
        panel.Find("Title").GetComponent<RectTransform>().anchoredPosition=new Vector2(0,292);
        GameUi.Button("Resume",panel,"Resume",new Vector2(320,44),new Vector2(0,195),Close);
        GameUi.Button("Suspend",panel,session.IsPractice?"Leave practice":"Suspend to Menu",new Vector2(320,44),new Vector2(0,137),Suspend);
        GameUi.Button("Retry",panel,"End run and retry",new Vector2(320,44),new Vector2(0,79),()=>ConfirmExit("Game"));
        GameUi.Button("Quit",panel,"End run",new Vector2(320,44),new Vector2(0,21),()=>ConfirmExit("MainMenu"));
        var m=GameUi.Button("Music",panel,"",new Vector2(320,44),new Vector2(0,-60),()=>{AudioPreferences.SetMusicMuted(!AudioPreferences.MusicMuted);RefreshAudio();});music=m.GetComponentInChildren<TMP_Text>();
        var s=GameUi.Button("SFX",panel,"",new Vector2(320,44),new Vector2(0,-118),()=>{AudioPreferences.SetSfxMuted(!AudioPreferences.SfxMuted);RefreshAudio();});sfx=s.GetComponentInChildren<TMP_Text>();RefreshAudio();
        var mtn=GameUi.Button("ReducedMotion",panel,"",new Vector2(320,44),new Vector2(0,-176),()=>{PresentationPreferences.SetReducedMotion(!PresentationPreferences.ReducedMotion);RefreshAudio();});motion=mtn.GetComponentInChildren<TMP_Text>();RefreshAudio();
        var vib=GameUi.Button("Vibration",panel,"",new Vector2(320,44),new Vector2(0,-234),()=>{AudioPreferences.SetVibrationMuted(!AudioPreferences.VibrationMuted);RefreshAudio();});vibration=vib.GetComponentInChildren<TMP_Text>();RefreshAudio();
        GameUi.Label("SuspendHint",panel,"Suspend keeps your board and build.\nNo combat time passes while you are away.",new Vector2(360,52),new Vector2(0,-300),17);
    }
    private void UpdateLesson()
    {
        if(session.Continuation.IsRestoring || session.IsFinished || Time.timeScale<=0) return;
        if(lesson!=null && Time.time<lessonUntil) return;
        lesson=null;
        var account=AccountProgression.Current;
        if(!account.HasSeenLesson("combat-clocks"))
        { ShowLesson("combat-clocks",CombatMoveClock.MoveBasics ? "Legacy forest test: attacks and abilities count moves." : "Attacks count seconds; abilities count moves.\nTap Guide or an enemy to inspect while paused."); return; }
        foreach(var enemy in session.Waves.ActiveEnemies)
        {
            if(!enemy.HasSpecialAbility) continue;
            string key="mechanic-"+enemy.Definition.SpecialAbilityKind;
            if(!account.HasSeenLesson(key))
            { ShowLesson(key,$"New threat: {enemy.Definition.DisplayName}.\nTap its portrait to learn the counter."); return; }
        }
    }
    private void ShowLesson(string key,string text)
    {
        if(AccountProgression.Current.RememberLesson(key)) { lesson=text; lessonUntil=Time.time+8; }
    }
    public void OpenGuide(string text)
    {
        if(text==CombatGuide.Basics && session?.Zone?.Definition?.periodicallyFloods==true)
            text+="\n\nDROWNED COURT\nFlooding lasts 10-12 accepted moves. AIR starts at five; each move spends one. Pop a bubbled gem to restore two. At zero AIR take 5 damage after enemy actions, except the final move drains first. Break Air Coffers or defeat their owners to recover captured bubbles. Sapphire gem damage gains 15%.";
        if (session == null || session.IsFinished || overlay != null) return;
        session.CancelTargeting(); BuildOverlay("Combat guide · paused"); Pause();
        panel.sizeDelta=new Vector2(500,660);
        panel.Find("Title").GetComponent<RectTransform>().anchoredPosition=new Vector2(0,280);
        var viewport=GameUi.Rect("GuideViewport",panel,new Vector2(450,410),new Vector2(0,25));
        // Text-only scrolling uses rect clipping, avoiding an unnecessary
        // stencil-write material over the native pixel font.
        viewport.gameObject.AddComponent<RectMask2D>();
        var content=GameUi.Label("GuideText",viewport,text,new Vector2(440,410),Vector2.zero,20);
        content.alignment=TextAlignmentOptions.TopLeft;
        content.rectTransform.anchorMin=content.rectTransform.anchorMax=content.rectTransform.pivot=new Vector2(.5f,1);
        content.rectTransform.anchoredPosition=Vector2.zero;
        content.rectTransform.sizeDelta=new Vector2(440,Mathf.Max(410,content.preferredHeight));
        var scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.content=content.rectTransform;scroll.viewport=viewport;scroll.horizontal=false;
        GameUi.Button("AbilityGuide",panel,"Ability",new Vector2(135,40),new Vector2(-150,-210),()=>{Close();OpenGuide(CombatGuide.Ability(session.Player));});
        GameUi.Button("BuildGuide",panel,"Build",new Vector2(135,40),new Vector2(0,-210),()=>{Close();OpenGuide(CombatGuide.Build(RunUpgradeRuntime.Current));});
        GameUi.Button("ResumeGuide",panel,"Resume",new Vector2(135,40),new Vector2(150,-210),Close);
        GameUi.Label("InspectionHint",panel,"Tap an enemy portrait to inspect its current state.",new Vector2(450,40),new Vector2(0,-270),17);
        GameplayThemeSkin.Screen(panel.gameObject);
    }
    private void BuildOverlay(string title)
    {
        overlay=GameUi.Rect("RunSettingsOverlay",canvas.transform,Vector2.zero,Vector2.zero);
        overlay.anchorMin=Vector2.zero;overlay.anchorMax=Vector2.one;overlay.offsetMin=overlay.offsetMax=Vector2.zero;
        var modalCanvas=overlay.gameObject.AddComponent<Canvas>();
        modalCanvas.overrideSorting=true;modalCanvas.sortingOrder=canvas.sortingOrder+220;
        overlay.gameObject.AddComponent<GraphicRaycaster>();
        var dim=overlay.gameObject.AddComponent<Image>();dim.color=new Color(0,0,0,0.8f);
        panel=GameUi.Panel("Panel",overlay,new Vector2(380,420));
        GameUi.Label("Title",panel,title,new Vector2(340,54),new Vector2(0,163),28);
    }
    private void Pause()
    {
        if(paused)return;
        inputBlock=session.Board.AcquireExternalInputBlock();priorTimeScale=Time.timeScale;Time.timeScale=0;paused=true;
    }
    private void RestorePause()
    {
        inputBlock?.Dispose();inputBlock=null;
        if(paused&&Time.timeScale==0)Time.timeScale=priorTimeScale;
        paused=false;
    }
    private void RefreshAudio(){if(music!=null)music.text="Music: "+(AudioPreferences.MusicMuted?"Muted":"On");if(sfx!=null)sfx.text="SFX: "+(AudioPreferences.SfxMuted?"Muted":"On");if(motion!=null)motion.text="Reduced motion: "+(PresentationPreferences.ReducedMotion?"On":"Off");if(vibration!=null)vibration.text="Vibration: "+(AudioPreferences.VibrationMuted?"Off":"On");}
    private void ConfirmExit(string scene)
    {
        if(overlay!=null){Destroy(overlay.gameObject);overlay=null;}
        BuildOverlay(scene=="Game"?"Retry this run?":"Leave this run?");
        GameUi.Label("Policy",panel,"Completed waves earn gold.\nThe current partial wave earns none.\nUnused consumables stay owned.",new Vector2(330,110),new Vector2(0,60),20);
        GameUi.Button("Confirm",panel,"Confirm",new Vector2(280,48),new Vector2(0,-45),()=>Exit(scene));
        GameUi.Button("Cancel",panel,"Back",new Vector2(280,48),new Vector2(0,-110),()=>{Close();OpenSettings();});
    }
    private void OpenVictory()
    {
        Close();BuildOverlay("Artifact defended!");Pause();
        GameUi.Label("Reward",panel,(session.RewardFinalized?AccountProgression.Current.LastReward:AccountProgression.Current.PreviewReward("Victory"))?.ToString()??"Saving reward...",new Vector2(330,150),new Vector2(0,25),21);
        GameUi.Button("Retry",panel,"Retry",new Vector2(280,44),new Vector2(0,-85),()=>Exit("Game"));
        GameUi.Button("Menu",panel,"Quit to Menu",new Vector2(280,44),new Vector2(0,-140),()=>Exit("MainMenu"));
    }
    private void Exit(string scene)
    {
        RestorePause();
        if(session.ExitTo(scene))return;
        Pause();
        if(panel!=null)GameUi.Label("SaveFailed",panel,"Save failed. Check storage, then retry.",new Vector2(350,36),new Vector2(0,115),17);
    }
    private void Suspend()
    {
        RestorePause();
        bool success=session.IsPractice?session.ExitTo("MainMenu"):session.SuspendToMenu();
        if(success) return;
        Pause();
        if(panel!=null && panel.Find("SaveFailed")==null)
            GameUi.Label("SaveFailed",panel,"Could not save. Check storage and try again.",new Vector2(380,35),new Vector2(0,-290),16);
    }
    public void Close(){if(overlay!=null)Destroy(overlay.gameObject);overlay=null;restoringOverlay=false;recoveryFailed=false;recovery=null;RestorePause();}
    private void OnDestroy(){Close();}
}
