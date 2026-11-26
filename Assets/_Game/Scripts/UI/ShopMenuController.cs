using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ShopMenuController : MonoBehaviour
{
    private const string Folder = "UI/MerchantStall/";
    private const float Width = 480, HeaderHeight = 246, FooterHeight = 26;
    private static readonly Color Cream = new Color32(255, 239, 207, 255);
    private static readonly Color Ink = new Color32(43, 28, 43, 255);

    private sealed class Row
    {
        public ConsumableKind kind;
        public RectTransform root, body;
        public Image card;
        public TMP_Text owned, equipCaption;
        public Button buy, equip;
    }

    private readonly List<Row> rows = new List<Row>();
    private readonly Dictionary<string, Sprite> art = new Dictionary<string, Sprite>();
    private RectTransform safe, page, viewport, content, rail, footer;
    private ScrollRect scroll;
    private Image mouse;
    private TMP_FontAsset font;
    private TMP_Text wallet, comment;
    private RectTransform walletCoin;
    private ConsumableKind featured = ConsumableKind.ShieldPotion;
    private int commentIndex;
    private bool initialized;
    private Vector2 lastSize;
    private AccountProgression account;

    public void Initialize(Action back)
    {
        if (initialized) return;
        account = AccountProgression.Current;
        font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        var background = gameObject.AddComponent<Image>();
        background.color = new Color32(29, 22, 36, 255);
        safe = Box("ShopSafeArea", transform, 0, 0, 0, 0);
        Stretch(safe);
        if (transform.GetComponentInParent<SafeAreaFitter>() == null) safe.gameObject.AddComponent<SafeAreaFitter>();
        page = Box("ShopPage", safe, 0, 0, Width, 854);
        // The page is measured in one common portrait width; only the list changes height.
        BuildList();
        BuildHeader(back);
        footer = Picture("BottomTrim", page, "Footer", 0, 0, Width, FooterHeight, sliced: true).rectTransform;
        footer.GetComponent<Image>().type = Image.Type.Tiled;
        footer.GetComponent<Image>().pixelsPerUnitMultiplier = .58f;
        Picture("CenterJewel", footer, "FooterJewel", 227, 0, 26, 26, preserve:true);
        initialized = true;
        Layout();
        Refresh();
    }

    private void OnEnable()
    {
        AccountProgression.Current.Changed += Refresh;
        Refresh();
    }

    private void OnDisable() => AccountProgression.Current.Changed -= Refresh;

    private void LateUpdate()
    {
        if (initialized && (safe.rect.size - lastSize).sqrMagnitude > .01f) Layout();
    }

    private void BuildHeader(Action back)
    {
        var header = Box("Merchant", page, 0, 0, Width, HeaderHeight);
        Picture("StoneAlcove", header, "Alcove", 0, 0, Width, 219);
        mouse = Picture("Shopkeeper", header, "MouseHappy", 101, 88, 143, 137, preserve: true);
        Picture("CounterEdge", header, "Counter", 0, 219, Width, 27);
        var title = Picture("TitlePlaque", header, "Title", 134, 1, 212, 90, preserve: true);
        // The crest rises above the lettering face. Center SHOP between the two laurels.
        Label("Title", title.transform, "SHOP", 36, 27, 140, 44, 32, TextAlignmentOptions.Midline, true);
        var exit = Control("Back", header, "Wallet", 8, 19, 61, 50, () => back?.Invoke());
        Picture("Arrow", exit.transform, "Arrow", 16, 12.5f, 29, 25, preserve: true);
        var purse = Picture("CurrencyPlaque", header, "Wallet", 352, 21, 118, 43, sliced: true);
        wallet = MakeAmount(purse.rectTransform, "Gold", 24, 21, out walletCoin);
        wallet.fontSizeMin = 10;
        var bubble = Picture("SpeechBubble", header, "Speech", 244, 98, 172, 75, sliced: true);
        // The tail sits below the speech face.
        comment = Label("MerchantComment", bubble.transform, "Great for tricky situations!", 9, 5, 154, 59, 17, TextAlignmentOptions.Midline, true);
        comment.color = Ink;
    }

    private void BuildList()
    {
        viewport = Box("ItemViewport", page, 8, HeaderHeight + 3, 449, 550);
        var hit = viewport.gameObject.AddComponent<Image>();
        hit.color = new Color(0,0,0,0);
        viewport.gameObject.AddComponent<RectMask2D>();
        content = Box("Items", viewport, 0, 0, 449, 1);
        scroll = viewport.gameObject.AddComponent<ScrollRect>();
        scroll.viewport = viewport;
        scroll.content = content;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 44;
        foreach (var kind in SupplyCatalog.DisplayOrder) MakeRow(kind);

        rail = Picture("ScrollRail", page, "Rail", 461, HeaderHeight + 10, 11, 540, sliced:true).rectTransform;
        rail.GetComponent<Image>().raycastTarget = true;
        var slide = Box("Slider", rail, 0, 0, 0, 0);
        Stretch(slide);
        var handle = Picture("Handle", slide, "Thumb", 0, 0, 0, 0, sliced:true);
        handle.raycastTarget = true;
        Stretch(handle.rectTransform);
        var bar = rail.gameObject.AddComponent<Scrollbar>();
        bar.handleRect = handle.rectTransform;
        bar.targetGraphic = handle;
        bar.direction = Scrollbar.Direction.BottomToTop;
        scroll.verticalScrollbar = bar;
    }

    private void MakeRow(ConsumableKind kind)
    {
        var card = Picture(kind.ToString(), content, "Card", 0, 0, 449, 120, sliced: true);
        var row = new Row {kind = kind, card = card, root = card.rectTransform};
        rows.Add(row);
        var parent = row.body = Box("Contents", row.root, 0, 0, 449, 116);
        var well = Picture("ItemSocket", parent, "Socket", 12, 14, 83, 88, sliced: true);
        Picture("ItemIcon", well.transform, kind.ToString(), 10.5f, 12, 62, 64, preserve: true);

        var details = Box("Details", parent, 106, 8, 191, 100);
        var stack = details.gameObject.AddComponent<VerticalLayoutGroup>();
        stack.childAlignment = TextAnchor.MiddleLeft;
        stack.spacing = 4;
        stack.childForceExpandHeight = false;
        stack.childForceExpandWidth = true;
        stack.childControlWidth = stack.childControlHeight = true;
        Label("ItemName", details, SupplyCatalog.Name(kind), 0, 0, 191, 32, 22, TextAlignmentOptions.MidlineLeft, true);
        Label("Description", details, SupplyCatalog.Description(kind), 0, 0, 191, 42, 16, TextAlignmentOptions.Left);
        row.owned = Label("Owned", details, "", 0, 0, 191, 24, 14, TextAlignmentOptions.MidlineLeft);

        var priceArea = Box("PriceGroup", parent, 306, 7, 126, 26);
        var price = MakeAmount(priceArea, "Price", 20, 19, out var coin);
        CenterAmount(coin, price, SupplyCatalog.Price(kind).ToString("N0"));
        row.buy = Control("Buy", parent, "Buy", 306, 35, 126, 39, () => Purchase(row));
        ButtonLabel(row.buy, "Buy", 19).color = Ink;
        row.equip = Control("Equip", parent, "Equip", 312, 78, 114, 29, () => Equip(row));
        row.equipCaption = ButtonLabel(row.equip, "Equip", 14);
        row.equipCaption.color = Ink;
    }

    private TMP_Text MakeAmount(RectTransform region, string name, float iconSize, float fontSize, out RectTransform coin)
    {
        float height = region.rect.height;
        coin = Picture("Coin", region, "Coin", 0, (height - iconSize) / 2, iconSize, iconSize, preserve: true).rectTransform;
        var amount = Label(name, region, "", 0, 0, region.rect.width, height, fontSize, TextAlignmentOptions.Midline, true);
        amount.textWrappingMode = TextWrappingModes.NoWrap;
        return amount;
    }

    private static void CenterAmount(RectTransform coin, TMP_Text label, string value)
    {
        label.text = value;
        var region = (RectTransform)coin.parent;
        const float gap = 5;
        float iconSize = coin.rect.width;
        float textWidth = Mathf.Min(Mathf.Ceil(label.GetPreferredValues(value).x) + 2, region.rect.width - iconSize - gap - 12);
        float left = (region.rect.width - iconSize - gap - textWidth) / 2;
        coin.anchoredPosition = new Vector2(left + iconSize / 2, -region.rect.height / 2);
        label.rectTransform.sizeDelta = new Vector2(textWidth, region.rect.height);
        label.rectTransform.anchoredPosition = new Vector2(left + iconSize + gap, 0);
    }

    private TMP_Text ButtonLabel(Button button, string caption, float size)
    {
        var rect = (RectTransform)button.transform;
        return Label("Label", rect, caption, 5, 2, rect.rect.width - 10, rect.rect.height - 6,
            size, TextAlignmentOptions.Midline, true);
    }

    private void Layout()
    {
        lastSize = safe.rect.size;
        if (lastSize.x <= 0 || lastSize.y <= 0) return;
        float scale = lastSize.x / Width;
        float height = lastSize.y / scale;
        page.localScale = new Vector3(scale, scale, 1);
        page.sizeDelta = new Vector2(Width, height);
        float viewHeight = Mathf.Max(1, height - HeaderHeight - FooterHeight - 6);
        viewport.sizeDelta = new Vector2(449, viewHeight);
        rail.sizeDelta = new Vector2(11, Mathf.Max(12, viewHeight - 14));
        footer.anchoredPosition = new Vector2(0, -(height - FooterHeight));
        float pitch = Mathf.Max(122, Mathf.Floor(viewHeight / 4.25f));
        for (int i = 0; i < rows.Count; i++)
        {
            rows[i].root.anchoredPosition = new Vector2(0, -(i * pitch + 3));
            rows[i].root.sizeDelta = new Vector2(449, pitch - 4);
            rows[i].body.anchoredPosition = new Vector2(0, -Mathf.Floor((pitch - 120) / 2));
        }
        content.sizeDelta = new Vector2(449, rows.Count * pitch + 6);
        rail.gameObject.SetActive(content.sizeDelta.y > viewHeight);
    }

    private void Purchase(Row row)
    {
        if (!account.TryPurchase(row.kind)) { Refresh(); return; }
        featured = row.kind;
        string line = row.kind switch {
            ConsumableKind.HealthPotion => "For a rainy dungeon day!",
            ConsumableKind.ShieldPotion => "Great for tricky situations!",
            ConsumableKind.Bomb => "Mind that little fuse!",
            ConsumableKind.MegaPotion => "That's my finest bottle!",
            _ => "Pick a color. Any color!"
        };
        comment.text = commentIndex++ % 3 == 2 ? "I'll wrap that for you!" : line;
        mouse.sprite = Art("MouseHappy");
        Refresh();
    }

    private void Equip(Row row)
    {
        bool equip = !account.Equipped(row.kind);
        if (!account.SetEquipped(row.kind, equip)) { Refresh(); return; }
        featured = row.kind;
        comment.text = equip ? (SupplyCatalog.IsPotion(row.kind) ? "Within paw's reach!" : "Packed for a big entrance!") : "Back on the shelf!";
        mouse.sprite = Art(equip ? "MouseHelpful" : "MousePlayful");
        Refresh();
    }

    private void Refresh()
    {
        if (!initialized) return;
        CenterAmount(walletCoin, wallet, account.Gold.ToString("N0"));
        bool locked = account.ActiveRun != null;
        if (locked) comment.text = "Finish your run to change supplies.";
        else if (account.LastError != null) comment.text = "Couldn't save. Try again?";
        foreach (var row in rows)
        {
            int count = account.Owned(row.kind);
            bool equipped = account.Equipped(row.kind);
            row.owned.text = $"Owned: {count:N0}";
            row.card.sprite = Art(row.kind == featured ? "SelectedCard" : "Card");
            row.buy.interactable = !locked && count < 9999 && account.Gold >= SupplyCatalog.Price(row.kind);
            row.equip.interactable = !locked && (count > 0 || equipped);
            row.equipCaption.text = equipped ? "Unequip" : "Equip";
            row.buy.image.sprite = Art(row.buy.interactable ? "Buy" : "Disabled");
            row.equip.image.sprite = Art(row.equip.interactable ? "Equip" : "Disabled");
        }
    }

    private Sprite Art(string name)
    {
        if (!art.TryGetValue(name, out var sprite))
            art[name] = sprite = Resources.Load<Sprite>(Folder + name);
        return sprite;
    }

    private static RectTransform Box(string name, Transform parent, float x, float y, float w, float h)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
        rect.sizeDelta = new Vector2(w,h);
        rect.anchoredPosition = new Vector2(x,-y);
        return rect;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    private Image Picture(string name, Transform parent, string sprite, float x, float y, float w, float h, bool sliced = false, bool preserve = false)
    {
        var image = Box(name,parent,x,y,w,h).gameObject.AddComponent<Image>();
        image.sprite = Art(sprite);
        image.type = sliced ? Image.Type.Sliced : Image.Type.Simple;
        if (sliced) image.pixelsPerUnitMultiplier = .7f;
        image.preserveAspect = preserve;
        if (preserve)
        {
            // Unity fits a Simple Image around its pivot, including any letterboxing.
            image.rectTransform.pivot = new Vector2(.5f, .5f);
            image.rectTransform.anchoredPosition = new Vector2(x + w / 2, -y - h / 2);
        }
        image.raycastTarget = false;
        return image;
    }

    private Button Control(string name, Transform parent, string sprite, float x, float y, float w, float h, UnityEngine.Events.UnityAction action)
    {
        var image = Picture(name,parent,sprite,x,y,w,h,sliced:true);
        image.raycastTarget = true;
        var button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.ColorTint;
        var colors = button.colors;
        colors.pressedColor = new Color(.78f,.72f,.8f);
        colors.disabledColor = new Color(.9f,.87f,.92f);
        button.colors = colors;
        button.onClick.AddListener(action);
        return button;
    }

    private TMP_Text Label(string name, Transform parent, string value, float x, float y, float w, float h, float size, TextAlignmentOptions alignment, bool bold = false)
    {
        var label = Box(name,parent,x,y,w,h).gameObject.AddComponent<TextMeshProUGUI>();
        label.font = font;
        label.text = value; label.fontSize = size;
        label.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        label.color = Cream; label.alignment = alignment;
        label.textWrappingMode = TextWrappingModes.Normal;
        label.enableAutoSizing = true; label.fontSizeMin = size - 3; label.fontSizeMax = size;
        label.raycastTarget = false;
        return label;
    }
}
