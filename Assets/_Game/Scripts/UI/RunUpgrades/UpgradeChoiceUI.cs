using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class UpgradeChoiceUI : MonoBehaviour
{
    private const string OverlayName = "RuntimeUpgradeChoiceOverlay";
    private const float CardGap = 12f;
    private const float CardStep = UpgradeCardView.CardWidth + CardGap;

    private static readonly Color32 BorderColor =
        new Color32(154, 70, 190, 255);
    private static readonly Color32 BodyColor =
        new Color32(43, 20, 59, 255);
    private static readonly Color32 SectionColor =
        new Color32(58, 27, 77, 255);
    private static readonly Color32 HighlightColor =
        new Color32(218, 83, 225, 255);
    private static readonly Color32 PressedColor =
        new Color32(237, 111, 235, 255);
    private static readonly Color32 DisabledColor =
        new Color32(91, 62, 101, 255);
    private static readonly Color32 TextColor =
        new Color32(247, 230, 255, 255);

    private Canvas rootCanvas;
    private RectTransform overlayRect;
    private readonly UpgradeCardView[] cardViews =
        new UpgradeCardView[UpgradeDraftGenerator.DefaultChoiceCount];
    private Func<RunUpgradeDefinition, bool> trySelect;
    private bool selectionPending;
    private TMP_FontAsset uiFont;
    private Sprite fallbackArtwork;
    private Texture2D fallbackArtworkTexture;

    public bool IsOpen => overlayRect != null && overlayRect.gameObject.activeSelf;

    public void Configure(Canvas canvas)
    {
        rootCanvas = canvas;
        BuildIfNeeded();
        Hide();
    }

    public bool Show(
        IReadOnlyList<RunUpgradeDefinition> choices,
        Func<RunUpgradeDefinition, bool> selectionHandler)
    {
        BuildIfNeeded();

        if (overlayRect == null)
        {
            return false;
        }

        trySelect = selectionHandler;
        selectionPending = false;
        int count = Mathf.Min(
            cardViews.Length,
            choices != null ? choices.Count : 0
        );

        for (int index = 0; index < cardViews.Length; index++)
        {
            UpgradeCardView card = cardViews[index];

            if (index < count)
            {
                RectTransform cardRect = card.transform as RectTransform;
                cardRect.anchoredPosition = new Vector2(
                    (index - (count - 1) * 0.5f) * CardStep,
                    -24f
                );
                card.Bind(choices[index], HandleCardSelected);
                ApplyArtwork(card, choices[index]);
            }
            else
            {
                card.Bind(null, null);
            }
        }

        overlayRect.gameObject.SetActive(true);
        overlayRect.SetAsLastSibling();
        return count > 0;
    }

    public void Hide()
    {
        trySelect = null;
        selectionPending = false;

        if (overlayRect != null)
        {
            overlayRect.gameObject.SetActive(false);
        }
    }

    private void HandleCardSelected(RunUpgradeDefinition definition)
    {
        if (selectionPending || definition == null || trySelect == null)
        {
            return;
        }

        selectionPending = true;
        SetCardsInteractable(false);

        if (trySelect(definition))
        {
            Hide();
            return;
        }

        selectionPending = false;
        SetCardsInteractable(true);
    }

    private void SetCardsInteractable(bool interactable)
    {
        for (int index = 0; index < cardViews.Length; index++)
        {
            if (cardViews[index] != null)
            {
                cardViews[index].SetInteractable(interactable);
            }
        }
    }

    private void BuildIfNeeded()
    {
        if (overlayRect != null)
        {
            return;
        }

        if (rootCanvas == null)
        {
            rootCanvas = GetComponent<Canvas>();
        }

        if (rootCanvas == null)
        {
            rootCanvas = GetComponentInParent<Canvas>();
        }

        if (rootCanvas == null)
        {
            Debug.LogError("UpgradeChoiceUI requires a Canvas.", this);
            return;
        }

        Transform existing = rootCanvas.transform.Find(OverlayName);

        if (existing != null)
        {
            Destroy(existing.gameObject);
        }

        uiFont = ResolveUiFont();
        fallbackArtwork = CreateFallbackArtwork();

        GameObject overlayObject = CreateUiObject(
            OverlayName,
            rootCanvas.transform,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );

        overlayRect = overlayObject.GetComponent<RectTransform>();
        StretchToParent(overlayRect, 0f);

        Image dimmer = overlayObject.GetComponent<Image>();
        dimmer.color = new Color32(12, 5, 18, 181);
        dimmer.raycastTarget = true;

        CreateLabel(
            "UpgradeHeading",
            overlayRect,
            "CHOOSE AN UPGRADE",
            28f,
            FontStyles.Bold,
            new Vector2(0f, 190f),
            new Vector2(420f, 52f)
        );

        CreateLabel(
            "PickOneLabel",
            overlayRect,
            "PICK 1",
            13f,
            FontStyles.Bold,
            new Vector2(0f, 153f),
            new Vector2(160f, 24f)
        );

        for (int index = 0; index < cardViews.Length; index++)
        {
            cardViews[index] = CreateCard(index);
        }
    }

    private UpgradeCardView CreateCard(int index)
    {
        GameObject cardObject = CreateUiObject(
            $"UpgradeCard_{index + 1}",
            overlayRect,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Button),
            typeof(UpgradeCardView)
        );

        RectTransform cardRect = cardObject.GetComponent<RectTransform>();
        AnchorAtCenter(
            cardRect,
            Vector2.zero,
            new Vector2(UpgradeCardView.CardWidth, UpgradeCardView.CardHeight)
        );

        Image border = cardObject.GetComponent<Image>();
        border.color = BorderColor;

        Button button = cardObject.GetComponent<Button>();
        button.targetGraphic = border;
        Navigation navigation = button.navigation;
        navigation.mode = Navigation.Mode.None;
        button.navigation = navigation;
        ColorBlock colors = button.colors;
        colors.normalColor = BorderColor;
        colors.highlightedColor = HighlightColor;
        colors.selectedColor = HighlightColor;
        colors.pressedColor = PressedColor;
        colors.disabledColor = DisabledColor;
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.06f;
        button.colors = colors;

        RectTransform body = CreatePanel(
            "CardBody",
            cardRect,
            BodyColor,
            4f
        );

        TMP_Text title = CreateLabel(
            "Title",
            body,
            string.Empty,
            17f,
            FontStyles.Bold,
            new Vector2(0f, 113f),
            new Vector2(136f, 38f)
        );

        RectTransform artworkSection = CreateFixedPanel(
            "ArtworkSection",
            body,
            SectionColor,
            new Vector2(0f, 40f),
            new Vector2(136f, 92f)
        );

        GameObject artObject = CreateUiObject(
            "Artwork",
            artworkSection,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        RectTransform artRect = artObject.GetComponent<RectTransform>();
        AnchorAtCenter(artRect, Vector2.zero, new Vector2(64f, 64f));
        Image art = artObject.GetComponent<Image>();
        art.sprite = fallbackArtwork;
        art.preserveAspect = true;
        art.raycastTarget = false;

        TMP_Text description = CreateLabel(
            "Description",
            body,
            string.Empty,
            14f,
            FontStyles.Normal,
            new Vector2(0f, -70f),
            new Vector2(136f, 96f)
        );
        description.textWrappingMode = TextWrappingModes.Normal;
        description.overflowMode = TextOverflowModes.Ellipsis;

        UpgradeCardView view = cardObject.GetComponent<UpgradeCardView>();
        view.Initialize(button, title, art, description);
        return view;
    }

    private void ApplyArtwork(
        UpgradeCardView card,
        RunUpgradeDefinition definition)
    {
        Image artwork = card.transform.Find("CardBody/ArtworkSection/Artwork")
            ?.GetComponent<Image>();

        if (artwork == null)
        {
            return;
        }

        artwork.sprite = definition != null && definition.Artwork != null
            ? definition.Artwork
            : fallbackArtwork;
        artwork.enabled = true;
    }

    private RectTransform CreatePanel(
        string objectName,
        RectTransform parent,
        Color color,
        float inset)
    {
        GameObject panelObject = CreateUiObject(
            objectName,
            parent,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        RectTransform rect = panelObject.GetComponent<RectTransform>();
        StretchToParent(rect, inset);
        Image image = panelObject.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return rect;
    }

    private RectTransform CreateFixedPanel(
        string objectName,
        RectTransform parent,
        Color color,
        Vector2 position,
        Vector2 size)
    {
        GameObject panelObject = CreateUiObject(
            objectName,
            parent,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        RectTransform rect = panelObject.GetComponent<RectTransform>();
        AnchorAtCenter(rect, position, size);
        Image image = panelObject.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return rect;
    }

    private TMP_Text CreateLabel(
        string objectName,
        RectTransform parent,
        string value,
        float fontSize,
        FontStyles style,
        Vector2 position,
        Vector2 size)
    {
        GameObject labelObject = CreateUiObject(
            objectName,
            parent,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI)
        );
        RectTransform rect = labelObject.GetComponent<RectTransform>();
        AnchorAtCenter(rect, position, size);
        TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
        label.text = value;
        label.fontSize = fontSize;
        label.fontStyle = style;
        label.alignment = TextAlignmentOptions.Center;
        label.color = TextColor;
        label.raycastTarget = false;
        label.overflowMode = TextOverflowModes.Overflow;

        if (uiFont != null)
        {
            label.font = uiFont;
        }

        return label;
    }

    private TMP_FontAsset ResolveUiFont()
    {
        TMP_Text[] labels = UnityEngine.Object.FindObjectsByType<TMP_Text>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        for (int index = 0; index < labels.Length; index++)
        {
            if (labels[index] != null && labels[index].font != null)
            {
                return labels[index].font;
            }
        }

        return null;
    }

    private Sprite CreateFallbackArtwork()
    {
        fallbackArtworkTexture = new Texture2D(
            64,
            64,
            TextureFormat.RGBA32,
            mipChain: false
        );
        fallbackArtworkTexture.name = "Runtime_Upgrade_Placeholder_64x64";
        fallbackArtworkTexture.filterMode = FilterMode.Point;
        fallbackArtworkTexture.wrapMode = TextureWrapMode.Clamp;

        Color32 transparent = new Color32(0, 0, 0, 0);
        Color32 dark = new Color32(78, 31, 105, 255);
        Color32 light = new Color32(218, 83, 225, 255);
        Color32[] pixels = new Color32[64 * 64];

        for (int y = 0; y < 64; y++)
        {
            for (int x = 0; x < 64; x++)
            {
                int distance = Mathf.Abs(x - 31) + Mathf.Abs(y - 31);
                pixels[y * 64 + x] = distance <= 23
                    ? (distance <= 16 ? light : dark)
                    : transparent;
            }
        }

        fallbackArtworkTexture.SetPixels32(pixels);
        fallbackArtworkTexture.Apply(updateMipmaps: false, makeNoLongerReadable: true);

        Sprite sprite = Sprite.Create(
            fallbackArtworkTexture,
            new Rect(0f, 0f, 64f, 64f),
            new Vector2(0.5f, 0.5f),
            64f
        );
        sprite.name = "Runtime_Upgrade_Placeholder_64x64";
        return sprite;
    }

    private void OnDestroy()
    {
        if (fallbackArtwork != null)
        {
            Destroy(fallbackArtwork);
        }

        if (fallbackArtworkTexture != null)
        {
            Destroy(fallbackArtworkTexture);
        }
    }

    private static GameObject CreateUiObject(
        string objectName,
        Transform parent,
        params Type[] componentTypes)
    {
        GameObject created = new GameObject(objectName, componentTypes);
        created.transform.SetParent(parent, false);
        return created;
    }

    private static void AnchorAtCenter(
        RectTransform rect,
        Vector2 position,
        Vector2 size)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private static void StretchToParent(RectTransform rect, float inset)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = new Vector2(inset, inset);
        rect.offsetMax = new Vector2(-inset, -inset);
    }
}
