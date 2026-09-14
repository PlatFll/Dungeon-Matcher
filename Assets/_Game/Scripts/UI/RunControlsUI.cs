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
    private readonly Text[] charges = new Text[2];
    private Text hint, music, sfx, saveError;
    private IDisposable inputBlock;
    private bool paused, victoryShown;
    private float priorTimeScale;
    private static Sprite cooldownSprite;

    private void Start()
    {
        session=GetComponent<RunSession>();
        var ability=FindFirstObjectByType<AbilityButtonUI>();
        if(ability==null)return;
        canvas=ability.GetComponentInParent<Canvas>().rootCanvas;
        foreach(var rect in canvas.GetComponentsInChildren<RectTransform>(true))
            if(rect.name=="SafeArea" || rect.name=="SafeAreaRoot") { safeRoot=rect;break; }
        if(safeRoot==null) safeRoot=ability.transform.parent.parent as RectTransform;
        for(int i=0;i<2;i++)
        {
            var kind=(ConsumableKind)i;
            slots[i]=GameUi.Button(kind.ToString(),ability.transform.parent,"",new Vector2(64,64),new Vector2(i==0?-136:136,-24),()=>
            { if(kind==ConsumableKind.HealthPotion) session.TryUsePotion();else session.ToggleBombTargeting(); });
            slots[i].image.sprite=Resources.Load<Sprite>("UI/Consumables/Slot");
            slots[i].image.color=Color.white;
            var rect=GameUi.Rect("Icon",slots[i].transform,new Vector2(48,48),Vector2.zero);
            icons[i]=rect.gameObject.AddComponent<Image>();
            icons[i].sprite=Resources.Load<Sprite>(i==0?"UI/Consumables/Potion":"UI/Consumables/Bomb");
            icons[i].preserveAspect=true;icons[i].raycastTarget=false;
            var shade=GameUi.Rect("Cooldown",slots[i].transform,new Vector2(52,52),Vector2.zero);
            cooldownFills[i]=shade.gameObject.AddComponent<Image>();
            cooldownFills[i].sprite=TextureSprite();cooldownFills[i].type=Image.Type.Filled;
            cooldownFills[i].fillMethod=Image.FillMethod.Vertical;cooldownFills[i].fillOrigin=0;
            cooldownFills[i].color=new Color(0.05f,0.02f,0.09f,0.75f);cooldownFills[i].raycastTarget=false;
            charges[i]=GameUi.Label("Charges",slots[i].transform,"",new Vector2(60,24),new Vector2(0,-19),19);
        }
        hint=GameUi.Label("ConsumableHint",ability.transform.parent,"",new Vector2(400,28),Vector2.zero,16);
        var settings=GameUi.Button("Settings",safeRoot,"Settings",new Vector2(90,36),Vector2.zero,OpenSettings);
        var settingsRect=(RectTransform)settings.transform;settingsRect.anchorMin=settingsRect.anchorMax=settingsRect.pivot=new Vector2(1,1);
        settingsRect.anchoredPosition=new Vector2(-20,-18);
        var saveRetry=GameUi.Button("RetrySave",safeRoot,"Retry save",new Vector2(180,40),new Vector2(0,280),()=>session.RetrySave());
        saveError=saveRetry.GetComponentInChildren<Text>();
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
        for(int i=0;i<2;i++)
        {
            var kind=(ConsumableKind)i;int count=session.Charges(kind);float cooldown=session.Cooldown(kind);
            slots[i].interactable=session.CanUse(kind)||(i==1&&session.Board.IsSelectingTarget);
            icons[i].color=count>0?Color.white:new Color(0.3f,0.3f,0.3f,0.6f);
            cooldownFills[i].fillAmount=count==0?1:cooldown/BalanceV1.Current.consumableCooldown;
            charges[i].text=cooldown>0?$"{count} | {Mathf.CeilToInt(cooldown)}s":count.ToString();
        }
        hint.rectTransform.anchoredPosition=new Vector2(0,((RectTransform)hint.transform.parent).rect.height*.5f+16);
        hint.text=session.Board.IsSelectingTarget?"Tap a gem. Tap Bomb again to cancel.":"";
        if(saveError!=null)saveError.transform.parent.gameObject.SetActive(session.NeedsSaveRetry);
        if(session.IsVictory&&!session.Board.IsBusy&&!victoryShown){victoryShown=true;OpenVictory();}
    }
    public void OpenSettings()
    {
        if(session==null||session.IsFinished||overlay!=null)return;
        session.CancelTargeting();BuildOverlay("Settings");Pause();
        GameUi.Button("Resume",panel,"Resume",new Vector2(280,44),new Vector2(0,95),Close);
        GameUi.Button("Retry",panel,"Retry",new Vector2(280,44),new Vector2(0,40),()=>ConfirmExit("Game"));
        GameUi.Button("Quit",panel,"Quit to Menu",new Vector2(280,44),new Vector2(0,-15),()=>ConfirmExit("MainMenu"));
        var m=GameUi.Button("Music",panel,"",new Vector2(280,44),new Vector2(0,-85),()=>{AudioPreferences.SetMusicMuted(!AudioPreferences.MusicMuted);RefreshAudio();});music=m.GetComponentInChildren<Text>();
        var s=GameUi.Button("SFX",panel,"",new Vector2(280,44),new Vector2(0,-140),()=>{AudioPreferences.SetSfxMuted(!AudioPreferences.SfxMuted);RefreshAudio();});sfx=s.GetComponentInChildren<Text>();RefreshAudio();
    }
    private void BuildOverlay(string title)
    {
        overlay=GameUi.Rect("RunSettingsOverlay",canvas.transform,Vector2.zero,Vector2.zero);
        overlay.anchorMin=Vector2.zero;overlay.anchorMax=Vector2.one;overlay.offsetMin=overlay.offsetMax=Vector2.zero;
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
    private void RefreshAudio(){if(music!=null)music.text="Music: "+(AudioPreferences.MusicMuted?"Muted":"On");if(sfx!=null)sfx.text="SFX: "+(AudioPreferences.SfxMuted?"Muted":"On");}
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
    public void Close(){if(overlay!=null)Destroy(overlay.gameObject);overlay=null;RestorePause();}
    private void OnDestroy(){Close();}
}
