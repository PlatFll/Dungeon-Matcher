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
        Picture("TitlePlaque", header, "Title", 123, 2, 223, 84, preserve: true);
        Label("Title", header, "SHOP", 148, 24, 173, 45, 32, TextAlignmentOptions.Center, true);
        var exit = Control("Back", header, "Wallet", 8, 19, 61, 50, () => back?.Invoke());
        Picture("Arrow", exit.transform, "Arrow", 16, 12, 29, 25, preserve: true);
        Picture("CurrencyPlaque", header, "Wallet", 352, 21, 118, 43, sliced: true);
        Picture("GoldCoin", header, "Coin", 361, 30, 24, 24, preserve: true);
        wallet = Label("Gold", header, "", 386, 25, 76, 33, 21, TextAlignmentOptions.Center, true);
        wallet.fontSizeMin = 10;
        wallet.textWrappingMode = TextWrappingModes.NoWrap;
        Picture("SpeechBubble", header, "Speech", 244, 98, 172, 75, sliced: true);
        comment = Label("MerchantComment", header, "Great for tricky situations!", 255, 102, 150, 52, 17, TextAlignmentOptions.Center, true);
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
        Picture("ItemSocket", parent, "Socket", 12, 14, 83, 88, sliced: true);
        Picture("ItemIcon", parent, kind.ToString(), 22, 23, 63, 68, preserve: true);
        Label("ItemName", parent, SupplyCatalog.Name(kind), 106, 12, 207, 32, 22, TextAlignmentOptions.MidlineLeft, true);
        Label("Description", parent, SupplyCatalog.Description(kind), 107, 44, 185, 43, 16, TextAlignmentOptions.TopLeft);
        row.owned = Label("Owned", parent, "", 107, 85, 190, 25, 14, TextAlignmentOptions.MidlineLeft);
        Picture("Coin", parent, "Coin", 331, 10, 20, 20, preserve: true);
        Label("Price", parent, SupplyCatalog.Price(kind).ToString("N0"), 357, 7, 63, 26, 19, TextAlignmentOptions.MidlineLeft, true);
        row.buy = Control("Buy", parent, "Buy", 306, 35, 126, 39, () => Purchase(row));
        Label("Label", row.buy.transform, "Buy", 5, 2, 116, 32, 19, TextAlignmentOptions.Center, true).color = Ink;
        row.equip = Control("Equip", parent, "Equip", 312, 78, 114, 29, () => Equip(row));
        row.equipCaption = Label("Label", row.equip.transform, "Equip", 4, 1, 106, 24, 14, TextAlignmentOptions.Center, true);
        row.equipCaption.color = Ink;
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
        wallet.text = account.Gold.ToString("N0");
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
