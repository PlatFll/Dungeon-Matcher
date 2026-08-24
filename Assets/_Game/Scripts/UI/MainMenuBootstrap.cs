using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class MainMenuBootstrap
{
    private const string MainMenuSceneName = "MainMenu";
    private const string GameSceneName = "Game";
    private const string MenuCanvasName = "MainMenuCanvas";

    private static readonly Color BackgroundColor =
        new Color32(20, 14, 27, 255);

    private static readonly Color FrameColor =
        new Color32(91, 50, 103, 255);

    private static readonly Color FrameHighlightColor =
        new Color32(151, 91, 158, 255);

    private static readonly Color PanelColor =
        new Color32(34, 22, 43, 255);

    private static readonly Color ButtonColor =
        new Color32(91, 46, 105, 255);

    private static readonly Color ButtonHighlightedColor =
        new Color32(119, 63, 133, 255);

    private static readonly Color ButtonPressedColor =
        new Color32(62, 31, 73, 255);

    private static readonly Color TextColor =
        new Color32(244, 231, 246, 255);

    private static bool isLoadingGame;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.BeforeSceneLoad
    )]
    private static void Install()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private static void HandleSceneLoaded(
        Scene scene,
        LoadSceneMode loadSceneMode
    )
    {
        if (scene.name != MainMenuSceneName)
        {
            isLoadingGame = false;
            return;
        }

        BuildMenuIfNeeded();
    }

    private static void BuildMenuIfNeeded()
    {
        if (GameObject.Find(MenuCanvasName) != null)
        {
            return;
        }

        EnsureAudioListener();
        EnsureEventSystem();

        Canvas canvas = CreateCanvas();
        CreateBackdrop(canvas.transform);
        CreateMenuFrame(canvas.transform);
    }

    private static Canvas CreateCanvas()
    {
        GameObject canvasObject = new GameObject(
            MenuCanvasName,
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster)
        );

        canvasObject.layer = LayerMask.NameToLayer("UI");

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = true;

        CanvasScaler scaler =
            canvasObject.GetComponent<CanvasScaler>();

        scaler.uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(480f, 854f);
        scaler.screenMatchMode =
            CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        scaler.referencePixelsPerUnit = 100f;

        return canvas;
    }

    private static void CreateBackdrop(Transform parent)
    {
        Image backdrop = CreateImage(
            "Backdrop",
            parent,
            BackgroundColor
        );

        StretchToParent(backdrop.rectTransform, 0f);
    }

    private static void CreateMenuFrame(Transform parent)
    {
        Image outerFrame = CreateImage(
            "MenuFrame",
            parent,
            FrameColor
        );

        RectTransform outerRect = outerFrame.rectTransform;
        outerRect.anchorMin = new Vector2(0.055f, 0.065f);
        outerRect.anchorMax = new Vector2(0.945f, 0.935f);
        outerRect.offsetMin = Vector2.zero;
        outerRect.offsetMax = Vector2.zero;

        Image highlight = CreateImage(
            "FrameHighlight",
            outerFrame.transform,
            FrameHighlightColor
        );
        StretchToParent(highlight.rectTransform, 4f);

        Image panel = CreateImage(
            "InnerPanel",
            highlight.transform,
            PanelColor
        );
        StretchToParent(panel.rectTransform, 5f);

        CreateTitle(panel.transform);
        CreatePlayButton(panel.transform);
    }

    private static void CreateTitle(Transform parent)
    {
        Text title = CreateText(
            "Title",
            parent,
            "DUNGEON\nMATCHER",
            48,
            FontStyle.Bold
        );

        RectTransform rect = title.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 0.72f);
        rect.anchorMax = new Vector2(0.5f, 0.72f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(390f, 150f);

        title.alignment = TextAnchor.MiddleCenter;
        title.color = TextColor;
        title.horizontalOverflow = HorizontalWrapMode.Overflow;
        title.verticalOverflow = VerticalWrapMode.Overflow;
    }

    private static void CreatePlayButton(Transform parent)
    {
        Image buttonBorder = CreateImage(
            "PlayButtonBorder",
            parent,
            FrameHighlightColor
        );

        RectTransform borderRect = buttonBorder.rectTransform;
        borderRect.anchorMin = new Vector2(0.5f, 0.43f);
        borderRect.anchorMax = new Vector2(0.5f, 0.43f);
        borderRect.pivot = new Vector2(0.5f, 0.5f);
        borderRect.anchoredPosition = Vector2.zero;
        borderRect.sizeDelta = new Vector2(250f, 84f);

        Image buttonImage = CreateImage(
            "PlayButton",
            buttonBorder.transform,
            ButtonColor
        );

        StretchToParent(buttonImage.rectTransform, 5f);
        buttonImage.raycastTarget = true;

        Button button =
            buttonImage.gameObject.AddComponent<Button>();

        button.targetGraphic = buttonImage;
        button.transition = Selectable.Transition.ColorTint;

        ColorBlock colors = button.colors;
        colors.normalColor = ButtonColor;
        colors.highlightedColor = ButtonHighlightedColor;
        colors.pressedColor = ButtonPressedColor;
        colors.selectedColor = ButtonHighlightedColor;
        colors.disabledColor = new Color32(55, 45, 61, 255);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.08f;
        button.colors = colors;

        button.onClick.AddListener(PlayGame);

        Text label = CreateText(
            "Label",
            buttonImage.transform,
            "PLAY",
            34,
            FontStyle.Bold
        );

        StretchToParent(label.rectTransform, 0f);
        label.alignment = TextAnchor.MiddleCenter;
        label.color = TextColor;
        label.raycastTarget = false;
    }

    private static void PlayGame()
    {
        if (isLoadingGame)
        {
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(GameSceneName))
        {
            Debug.LogError(
                $"Main menu could not load scene '{GameSceneName}'. " +
                "Make sure it is enabled in Build Settings."
            );
            return;
        }

        isLoadingGame = true;
        SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);
    }

    private static void EnsureEventSystem()
    {
        if (EventSystem.current != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject(
            "EventSystem",
            typeof(EventSystem),
            typeof(InputSystemUIInputModule)
        );

        Object.DontDestroyOnLoad(eventSystemObject);
    }

    private static void EnsureAudioListener()
    {
        if (Object.FindFirstObjectByType<AudioListener>() != null)
        {
            return;
        }

        GameObject listenerObject =
            new GameObject("MainMenuAudioListener");

        listenerObject.AddComponent<AudioListener>();
    }

    private static Image CreateImage(
        string name,
        Transform parent,
        Color color
    )
    {
        GameObject imageObject = new GameObject(
            name,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );

        imageObject.layer = LayerMask.NameToLayer("UI");
        imageObject.transform.SetParent(parent, false);

        Image image = imageObject.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;

        return image;
    }

    private static Text CreateText(
        string name,
        Transform parent,
        string value,
        int fontSize,
        FontStyle fontStyle
    )
    {
        GameObject textObject = new GameObject(
            name,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Text)
        );

        textObject.layer = LayerMask.NameToLayer("UI");
        textObject.transform.SetParent(parent, false);

        Text text = textObject.GetComponent<Text>();
        text.text = value;
        text.font = Resources.GetBuiltinResource<Font>(
            "LegacyRuntime.ttf"
        );
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.color = TextColor;
        text.raycastTarget = false;

        return text;
    }

    private static void StretchToParent(
        RectTransform rect,
        float inset
    )
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = new Vector2(inset, inset);
        rect.offsetMax = new Vector2(-inset, -inset);
    }
}
