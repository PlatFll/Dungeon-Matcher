using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Decorates the existing home controls while MainMenuController retains all navigation.</summary>
public sealed class MainMenuArtwork : MonoBehaviour
{
    private Image character;
    private Animator animator;
    private RectTransform settings;
    private GlobalMenuPixelRoot layout;
    private RectTransform gearRect,tagline;
    private bool hasSavedRun;
    public void Initialize(GameObject home,Button play,Button mastery,TMP_Text account,Button endSaved)
    {
        var stage=GlobalMenuSkin.Root(home.transform,"NativeMenuStage",new Vector2(360,640));
        layout=stage.GetComponent<GlobalMenuPixelRoot>();
        layout.AlignTop=true;
        var hall=GlobalMenuSkin.Art("MenuDungeonArtwork",stage,"HallTall",new Vector2(360,800),new Vector2(0,-80));
        hall.transform.SetAsFirstSibling();
        GlobalMenuSkin.Art("DungeonMatcherLogo",stage,"Logo",new Vector2(280,124),new Vector2(0,236));
        character=GlobalMenuSkin.Art("MenuCharacter",stage,"CrownedSkull",new Vector2(64,64),new Vector2(0,53));
        animator=character.gameObject.AddComponent<Animator>();animator.updateMode=AnimatorUpdateMode.UnscaledTime;
        Arrange(play,stage,"Play","CrownedSkull",new Vector2(0,-74));
        Arrange(home.transform.Find("CharactersButton").GetComponent<Button>(),stage,"Secondary","Characters",new Vector2(-72,-129));
        Arrange(home.transform.Find("ShopButton").GetComponent<Button>(),stage,"Secondary","Shop",new Vector2(72,-129));
        Arrange(mastery,stage,"Secondary","Mastery",new Vector2(-72,-180));
        mastery.GetComponentInChildren<TMP_Text>().text="GEM\nMASTERY";
        Arrange(home.transform.Find("PracticeButton").GetComponent<Button>(),stage,"Secondary","Practice",new Vector2(72,-180));
        stage.Find("PracticeButton").GetComponentInChildren<TMP_Text>().text="FREE\nPRACTICE";
        Arrange(home.transform.Find("ChallengesButton").GetComponent<Button>(),stage,"Challenge","Challenges",new Vector2(0,-231));
        GlobalMenuSkin.Place(account.rectTransform,stage,new Vector2(330,22),new Vector2(0,-35));
        GlobalMenuSkin.StyleText(account,16);
        GlobalMenuSkin.Place((RectTransform)endSaved.transform,stage,new Vector2(184,38),new Vector2(0,-277));
        GlobalMenuSkin.DressButton(endSaved,"Neutral","Suspend");
        var footer=GlobalMenuSkin.Label("Tagline",stage,"MATCH   BATTLE   DESCEND",new Vector2(330,16),new Vector2(0,-307),16);
        footer.color=new Color32(170,158,188,255);tagline=footer.rectTransform;
        var gear=GlobalMenuSkin.Button("MenuSettings",stage,"","Gear",null,new Vector2(158,296),OpenSettings);
        gear.GetComponentInChildren<TMP_Text>().text="";
        gearRect=(RectTransform)gear.transform;
        var title=home.transform.Find("Title");if(title!=null)title.gameObject.SetActive(false);
        var oldContent=home.transform.Find("HomeContent");if(oldContent!=null)oldContent.gameObject.SetActive(false);
        if(home.TryGetComponent<Image>(out var fill))fill.color=new Color32(17,12,29,255);
        var backdrop=home.GetComponentInParent<Canvas>().transform.Find("Backdrop");
        if(backdrop!=null&&backdrop.TryGetComponent<Image>(out var bg))bg.color=new Color32(17,12,29,255);
    }
    private static void Arrange(Button button,Transform parent,string family,string icon,Vector2 position)
    {
        GlobalMenuSkin.Place((RectTransform)button.transform,parent,GlobalMenuSkin.Load(family).rect.size,position);
        GlobalMenuSkin.DressButton(button,family,icon);
    }
    public void Refresh(PlayerDefinition definition,bool saved)
    {
        hasSavedRun=saved;
        if(definition==null||character==null)return;
        // Same selected/saved identity and approved idle used by the existing character menu.
        character.sprite=definition.BattleCharacterSprite;
        if(animator.runtimeAnimatorController!=definition.BattleAnimatorController)
            animator.runtimeAnimatorController=definition.BattleAnimatorController;
    }
    private void LateUpdate()
    {
        if(layout!=null)
        {
            gearRect.anchoredPosition=new Vector2(158,Mathf.Floor(Mathf.Min(296,layout.AvailableHeight/2-20)));
            tagline.anchoredPosition=new Vector2(0,Mathf.Ceil(layout.AvailableHeight>640?332-layout.AvailableHeight:Mathf.Max(-307,-layout.AvailableHeight/2+12)));
            tagline.gameObject.SetActive(!hasSavedRun);
        }
        if(character!=null && character.sprite!=null)
        {
            var size=character.sprite.rect.size;
            character.rectTransform.sizeDelta=size;
            character.rectTransform.anchoredPosition=new Vector2(0,20+size.y*.5f-character.sprite.pivot.y);
        }
    }
    private void OpenSettings()
    {
        if(settings!=null)return;
        // HomeScreen is an inset serialized panel. The dimmer/input shield must
        // cover the canvas, including the decorative art outside that panel.
        settings=GameUi.Rect("MenuSettingsOverlay",GetComponentInParent<Canvas>().rootCanvas.transform,Vector2.zero,Vector2.zero);
        settings.anchorMin=Vector2.zero;settings.anchorMax=Vector2.one;settings.offsetMin=settings.offsetMax=Vector2.zero;
        settings.gameObject.AddComponent<Image>().color=new Color(0,0,0,.65f);
        var stage=GlobalMenuSkin.Root(settings,"SettingsStage",new Vector2(360,640));
        var panel=GameUi.Panel("Panel",stage,new Vector2(240,248));GlobalMenuSkin.Panel(panel,new Vector2(240,248),"SETTINGS");
        GlobalMenuSkin.SettingsRows(panel,54);
        GlobalMenuSkin.Button("CloseMenuSettings",panel,"BACK","Affirmative","Resume",new Vector2(0,-92),CloseSettings);
    }
    private void CloseSettings(){if(settings!=null)Destroy(settings.gameObject);settings=null;}
    private void OnDisable()=>CloseSettings();
}
