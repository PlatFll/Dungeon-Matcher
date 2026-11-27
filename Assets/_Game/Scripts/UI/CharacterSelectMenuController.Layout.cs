using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed partial class CharacterSelectMenuController
{
    private static readonly Color Cream = new Color32(255, 239, 210, 255);
    private static readonly Color Growth = new Color32(180, 232, 134, 255);
    private static readonly Color Ink = new Color32(55, 34, 51, 255);
    private RectTransform safe, page, chamber, figures, details, collection, footer, modal, cardPage;
    private Image hero, affinity, abilityIcon, costCoin;
    private RectTransform footAnchor, contactShadow;
    private Animator heroIdle;
    private PlayerDefinition previewDefinition;
    private TMP_Text nameLabel, abilityName, abilityBody, wallet, costLabel, progress, feedback, startLabel, resetMessage, titleLabel;
    private readonly TMP_Text[] currents = new TMP_Text[6], nexts = new TMP_Text[6], arrows = new TMP_Text[6];
    private readonly Image[] statIcons = new Image[6];
    private Button levelUp, reset, cardsTab, charactersTab, gemsTab;
    private Image header;
    private Vector2 lastSize;

    private static Sprite Art(string name) => Resources.Load<Sprite>("UI/CharacterChamber/" + name);
    private static Sprite ShopArt(string name) => Resources.Load<Sprite>("UI/MerchantStall/" + name);

    private static RectTransform Box(string name, Transform parent, float x, float y, float w, float h)
    {
        var rt = GameUi.Rect(name, parent, new Vector2(w,h), Vector2.zero);
        rt.anchorMin = rt.anchorMax = rt.pivot = Vector2.up;
        rt.anchoredPosition = new Vector2(x,-y);
        return rt;
    }

    private static Image Picture(string name, Transform parent, Sprite sprite, float x, float y, float w, float h, bool fit = false)
    {
        var img = Box(name,parent,x,y,w,h).gameObject.AddComponent<Image>();
        img.sprite = sprite;
        img.raycastTarget = false;
        img.preserveAspect = fit;
        if (sprite != null && sprite.border != Vector4.zero && !fit) img.type = Image.Type.Sliced;
        return img;
    }

    // Combat sprites share a floor pivot even when the canvas includes extra space for weapons.
    private static void Ground(Image image, Sprite sprite, float x, float baseline, float zoom)
    {
        image.sprite = sprite;
        if (sprite == null) { image.enabled = false; return; }
        image.enabled = true;
        var rect = image.rectTransform;
        rect.pivot = sprite.pivot / sprite.rect.size;
        rect.sizeDelta = sprite.rect.size * zoom;
        rect.anchoredPosition = new Vector2(x,-baseline);
    }

    private void SetPreview(PlayerDefinition definition)
    {
        // Refresh also runs on purchases and wallet changes. Only a new selection starts a new idle.
        if (previewDefinition == definition) return;
        previewDefinition = definition;
        var sprite = definition.BattleCharacterSprite;
        Ground(hero,sprite,0,0,2f);
        contactShadow.gameObject.SetActive(sprite != null);
        if (sprite != null) contactShadow.sizeDelta = new Vector2(sprite.rect.width * .75f,6);
        heroIdle.runtimeAnimatorController = definition.BattleAnimatorController;
    }

    private TMP_FontAsset readingFont;

    private TMP_Text Text(string name, Transform parent, string text, float x, float y, float w, float h,
        int em, TextAlignmentOptions align = TextAlignmentOptions.Midline, bool wrap = false)
    {
        var label = Box(name,parent,x,y,w,h).gameObject.AddComponent<TextMeshProUGUI>();
        bool useReadingFace = em <= 25 && readingFont != null;
        if (useReadingFace)
        {
            label.font = readingFont;
            label.fontSharedMaterial = readingFont.material;
            label.enableAutoSizing = true;
            label.fontSizeMax = Mathf.Round(em * .72f);
            label.fontSizeMin = Mathf.Min(name == "Name" ? 9 : 10, label.fontSizeMax);
            label.fontSize = label.fontSizeMax;
            label.overflowMode = TextOverflowModes.Truncate;
        }
        else GlobalMenuSkin.StyleText(label,em);
        label.raycastTarget = false;
        label.text = text;
        label.color = Cream;
        label.alignment = align;
        label.textWrappingMode = wrap ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
        if (!useReadingFace) PixelTextFitter.ApplyNativeMenu(label,em);
        return label;
    }

    private static Button Control(string name, Transform parent, Sprite sprite, float x, float y, float w, float h, Action action, MenuCue sound = MenuCue.Press)
    {
        var img = Picture(name,parent,sprite,x,y,w,h);
        img.raycastTarget = true;
        var button = img.gameObject.AddComponent<Button>();
        button.targetGraphic = img;
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        var colors = button.colors;
        colors.disabledColor = new Color(.72f,.68f,.75f);
        button.colors = colors;
        MenuButtonFeedback.Attach(button, sound);
        button.onClick.AddListener(() => action());
        return button;
    }

    private void BuildScreen()
    {
        readingFont = Resources.Load<TMP_FontAsset>("UI/CharacterChamber/PixelifyReading");
        var wall = Picture("StoneCourses",transform,Art("Masonry"),0,0,0,0);
        wall.type = Image.Type.Tiled;
        wall.rectTransform.anchorMax = Vector2.one;
        wall.rectTransform.anchorMin = Vector2.zero;
        wall.rectTransform.offsetMin = wall.rectTransform.offsetMax = Vector2.zero;
        safe = Box("CharacterSafeArea",transform,0,0,0,0);
        safe.anchorMin = Vector2.zero; safe.anchorMax = Vector2.one;
        safe.offsetMin = safe.offsetMax = Vector2.zero;
        if (!GetComponentInParent<SafeAreaFitter>()) safe.gameObject.AddComponent<SafeAreaFitter>();
        page = Box("CharacterPage",safe,0,0,360,640);
        page.anchorMin = page.anchorMax = new Vector2(.5f,1);
        page.pivot = new Vector2(.5f,1);
        chamber = Picture("TorchlitChamber",page,Art("Chamber"),0,0,360,288).rectTransform;
        // The walkable top of the dais spans rows 162–179 of the 240 × 192 source.
        // Attach the feet to a point inside it so phone layout changes move the art and actor together.
        footAnchor = Box("DaisFootAnchor",chamber,0,0,0,0);
        footAnchor.anchorMin = footAnchor.anchorMax = new Vector2(48f/240f, 1f-173f/192f);
        contactShadow = Picture("ContactShadow",footAnchor,Art("ContactShadow"),0,0,48,6).rectTransform;
        contactShadow.pivot = new Vector2(.5f,.5f);
        hero = Picture("CharacterPreview",footAnchor,null,0,0,0,0,true);
        heroIdle = hero.gameObject.AddComponent<Animator>();
        heroIdle.updateMode = AnimatorUpdateMode.UnscaledTime;
        figures = Box("CharacterAndStats",page,0,0,360,286);
        var nameplate = Picture("NamePlaque",figures,Art("Plaque"),133,63,222,45);
        nameLabel = Text("CharacterName",nameplate.transform,"",8,11,206,29,29);
        var panel = Picture("StatsPanel",figures,Art("Stats"),136,109,216,151);
        string[] stats = {"Level","HP","Gem Damage","Ability Damage","Energy Cost","Shield Cap"};
        Sprite[] icons = {Art("LevelStar"),Art("Heart"),null,null,Art("Energy"),Art("Shield")};
        for (int i=0; i<stats.Length; i++)
        {
            int y = 8 + i*23;
            if (i>0) {
                var rule = Picture("Rule",panel.transform,null,8,y-1,200,1);
                rule.color = new Color32(78,60,81,255);
            }
            statIcons[i] = Picture("Icon"+i,panel.transform,icons[i],10,y+3,15,15,true);
            Text("StatLabel"+i,panel.transform,stats[i],30,y,80,21,17,TextAlignmentOptions.MidlineLeft);
            currents[i] = Text("Current"+i,panel.transform,"",112,y,39,21,20,TextAlignmentOptions.MidlineRight);
            arrows[i] = Text("Arrow"+i,panel.transform,">",153,y,12,21,20);
            arrows[i].color = new Color32(238,182,96,255);
            nexts[i] = Text("Next"+i,panel.transform,"",166,y,39,21,20,TextAlignmentOptions.MidlineRight);
        }
        affinity = statIcons[2];
        details = Box("AbilityAndUpgrade",page,0,278,360,161);
        var parchment = Picture("AbilityParchment",details,Art("Parchment"),9,0,342,94);
        var badge = Picture("AbilityHeading",parchment.transform,Art("Plaque"),100,-14,142,34);
        Text("Caption",badge.transform,"ACTIVE ABILITY",5,8,132,21,22);
        var socket = Picture("AbilitySocket",parchment.transform,Art("Roster"),12,24,51,52);
        abilityIcon = Picture("AbilityIcon",socket.transform,null,9,9,33,34,true);
        abilityName = Text("AbilityName",parchment.transform,"",73,17,253,23,28,TextAlignmentOptions.MidlineLeft);
        abilityBody = Text("AbilityDescription",parchment.transform,"",73,38,250,48,19,TextAlignmentOptions.MidlineLeft,true);
        abilityBody.color = abilityName.color = Ink;
        var levelPlate = Picture("Progression",details,Art("Stats"),10,106,196,45);
        progress = Text("LevelProgress",levelPlate.transform,"",7,4,182,37,25);
        levelUp = Control("LevelUp",details,Art("LevelUp"),211,102,138,53,LevelUp, MenuCue.None);
        Text("Action",levelUp.transform,"LEVEL UP",8,4,122,24,29);
        var price = Box("UpgradePrice", levelUp.transform, 8, 25, 122, 23);
        costCoin = Picture("GoldCoin", price, ShopArt("Coin"), 0, 0, 13, 13, true);
        costLabel = Text("GoldCost", price, "", 0, 0, 100, 23, 20);
        GlobalMenuSkin.Place(costCoin.rectTransform, price, new Vector2(13,13), Vector2.zero);
        GlobalMenuSkin.Place(costLabel.rectTransform, price, new Vector2(100,23), Vector2.zero);
        price.gameObject.AddComponent<FramedActionContent>().Initialize(costLabel, costCoin, new Rect(0,0,13,13), 5);

        collection = Box("RosterSection",page,0,442,360,143);
        var shelf = Picture("Shelf",collection,ShopArt("Footer"),0,-3,360,10);
        shelf.type = Image.Type.Tiled;
        float cardWidth = (340f - 4*(roster.Length-1)) / Mathf.Max(1,roster.Length);
        for (int i=0;i<roster.Length;i++)
        {
            var entry = roster[i];
            var button = Control(entry.cardName+"Button",collection,Art("Roster"),10+i*(cardWidth+4),10,cardWidth,92,() => Select(entry), MenuCue.None);
            entry.frame = button.image;
            var portrait = Picture("Portrait",button.transform,null,0,0,0,0,true);
            Ground(portrait,entry.definition.BattleCharacterSprite,cardWidth / 2,55,.74f);
            Text("Name",button.transform,entry.cardName,7,56,cardWidth-14,17,17);
            entry.levelLabel = Text("Level",button.transform,"",4,73,cardWidth-8,14,18);
            entry.levelLabel.color = new Color32(240,204,137,255);
        }
        reset = Control("ResetLevel",collection,ShopArt("Wallet"),11,108,64,27,OpenReset);
        Text("ResetLabel",reset.transform,"RESET",4,2,56,22,17);
        feedback = Text("UnlockFeedback",collection,"",81,103,192,36,17,TextAlignmentOptions.Midline,true);
        var play = Control("StartButton",collection,Art("Stats"),278,108,71,27,() => startRequested?.Invoke());
        startLabel = Text("PlayLabel",play.transform,"PLAY",3,2,65,22,19);

        BuildLibrary();
        BuildMastery();
        footer = Box("Navigation",page,0,593,360,46);
        cardsTab = Tab("Cards","CARDS",6,104,GlobalMenuSkin.Load("Challenges"),() => ShowPage(MenuPage.Cards));
        charactersTab = Tab("Characters","CHARACTERS",113,126,GlobalMenuSkin.Load("Characters"),() => ShowPage(MenuPage.Characters));
        gemsTab = Tab("SpecialGems","SPECIAL GEMS",242,112,GlobalMenuSkin.Load("Mastery"),() => { masteryBackToRoster = true; ShowPage(MenuPage.Gems); });

        var back = Control("BackButton",page,ShopArt("Wallet"),7,11,41,34,GoBack);
        Picture("Arrow",back.transform,ShopArt("Arrow"),11,8,19,17,true);
        header = Picture("TitlePlaque",page,Art("Plaque"),118,6,124,47);
        titleLabel = Text("ScreenTitle",header.transform,"CHARACTERS",6,13,112,28,31);
        var purse = Picture("Wallet",page,ShopArt("Wallet"),249,12,103,31);
        Picture("Coin",purse.transform,ShopArt("Coin"),8,6,18,18,true);
        wallet = Text("Gold",purse.transform,"",30,2,63,26,23,TextAlignmentOptions.MidlineRight);
        BuildConfirmation();
    }

    private Button Tab(string name,string label,int x,int width,Sprite icon,Action action)
    {
        var button = Control(name+"Tab",footer,Art("Stats"),x,0,width,42,action,MenuCue.Page);
        Picture("Symbol",button.transform,icon,(width-14)/2f,5,14,14,true);
        Text("Title",button.transform,label,8,21,width-16,15,18);
        return button;
    }

    private void BuildConfirmation()
    {
        modal = Box("ResetLevelConfirmation",page,0,0,360,640);
        var shade = modal.gameObject.AddComponent<Image>();
        shade.color = new Color(0,0,0,.85f);
        var panel = Picture("ConfirmPanel",modal,Art("Stats"),30,217,300,198);
        Text("Title",panel.transform,"RESET LEVEL",15,13,270,30,32);
        resetMessage = Text("Message",panel.transform,"",18,52,264,82,22,TextAlignmentOptions.Midline,true);
        var cancel = Control("CancelResetLevel",panel.transform,Art("Plaque"),17,145,128,36,CloseReset);
        Text("Label",cancel.transform,"CANCEL",6,5,116,25,23);
        var confirm = Control("ConfirmResetLevel",panel.transform,Art("Plaque"),155,145,128,36,ConfirmReset,MenuCue.None);
        Text("Label",confirm.transform,"RESET LEVEL",6,5,116,25,23);
        modal.gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (safe != null && safe.rect.size != lastSize) Layout();
    }

    private void Layout()
    {
        lastSize = safe.rect.size;
        if (lastSize.x < 1 || lastSize.y < 1) return;
        float scale = Mathf.Min(lastSize.x/360f,lastSize.y/640f);
        float height = lastSize.y/scale;
        float extra = height-640;
        page.localScale = new Vector3(scale,scale,1);
        page.sizeDelta = new Vector2(360,height);
        chamber.sizeDelta = new Vector2(360,288+extra*.42f);
        figures.anchoredPosition = new Vector2(0,-extra*.34f);
        details.anchoredPosition = new Vector2(0,-278-extra*.45f);
        collection.anchoredPosition = new Vector2(0,-442-extra*.74f);
        footer.anchoredPosition = new Vector2(0,-593-extra);
        LayoutLibrary(extra);
        modal.sizeDelta = new Vector2(360,height);
        modal.GetChild(0).GetComponent<RectTransform>().anchoredPosition = new Vector2(30,-217-extra*.5f);
    }
}
