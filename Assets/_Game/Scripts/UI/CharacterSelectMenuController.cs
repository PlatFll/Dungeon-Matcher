using System;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class CharacterSelectMenuController : MonoBehaviour
{
    private static readonly Color SelectedColor =
        new Color32(240, 177, 255, 255);

    private static readonly Color UnselectedColor =
        new Color32(244, 231, 246, 255);

    private static readonly Color UnavailableColor =
        new Color32(139, 116, 146, 255);

    private static readonly Color SecondaryColor =
        new Color32(204, 184, 212, 255);

    private Button rattlebonesButton;
    private Button bardleyButton;
    private Button startButton;
    private Button backButton;
    private Text statusText;

    private Action startRequested;
    private Action backRequested;
    private string selectedPlayerId;
    private bool initialized;

    public void Initialize(
        Action onStartRequested,
        Action onBackRequested)
    {
        startRequested = onStartRequested;
        backRequested = onBackRequested;

        EnsureInitialControls();

        if (!ResolveReferences())
        {
            enabled = false;
            return;
        }

        if (!initialized)
        {
            rattlebonesButton.onClick.AddListener(
                SelectRattlebones
            );

            bardleyButton.onClick.AddListener(
                SelectBardley
            );

            startButton.onClick.AddListener(
                HandleStartRequested
            );

            backButton.onClick.AddListener(
                HandleBackRequested
            );

            initialized = true;
        }

        selectedPlayerId =
            CharacterSelectionSettings.SelectedPlayerId;

        Refresh();
    }

    private void OnEnable()
    {
        if (!initialized)
        {
            return;
        }

        selectedPlayerId =
            CharacterSelectionSettings.SelectedPlayerId;

        Refresh();
    }

    private void OnDestroy()
    {
        if (!initialized)
        {
            return;
        }

        rattlebonesButton.onClick.RemoveListener(
            SelectRattlebones
        );

        bardleyButton.onClick.RemoveListener(
            SelectBardley
        );

        startButton.onClick.RemoveListener(
            HandleStartRequested
        );

        backButton.onClick.RemoveListener(
            HandleBackRequested
        );
    }

    private void SelectRattlebones()
    {
        SelectCharacter(
            CharacterSelectionSettings.RattlebonesPlayerId
        );
    }

    private void SelectBardley()
    {
        SelectCharacter(
            CharacterSelectionSettings.BardleyPlayerId
        );
    }

    private void SelectCharacter(
        string playerId)
    {
        selectedPlayerId = playerId;

        CharacterSelectionSettings.SetSelectedPlayerId(
            playerId
        );

        Refresh();
    }

    private void HandleStartRequested()
    {
        if (!PlayerDefinitionRegistry.IsAvailable(
                selectedPlayerId
            ))
        {
            Refresh();
            return;
        }

        startRequested?.Invoke();
    }

    private void HandleBackRequested()
    {
        backRequested?.Invoke();
    }

    private void Refresh()
    {
        bool rattlebonesAvailable =
            PlayerDefinitionRegistry.IsAvailable(
                CharacterSelectionSettings.RattlebonesPlayerId
            );

        bool bardleyAvailable =
            PlayerDefinitionRegistry.IsAvailable(
                CharacterSelectionSettings.BardleyPlayerId
            );

        bool selectedAvailable =
            PlayerDefinitionRegistry.TryGetDefinition(
                selectedPlayerId,
                out PlayerDefinition selectedDefinition
            );

        SetButtonColor(
            rattlebonesButton,
            CharacterSelectionSettings.RattlebonesPlayerId,
            rattlebonesAvailable
        );

        SetButtonColor(
            bardleyButton,
            CharacterSelectionSettings.BardleyPlayerId,
            bardleyAvailable
        );

        if (startButton != null)
        {
            startButton.interactable =
                selectedAvailable;
        }

        if (statusText == null)
        {
            return;
        }

        if (!selectedAvailable)
        {
            statusText.text =
                "BARDLEY\nCHARACTER DATA COMING NEXT\nSTART LOCKED UNTIL BARDLEY IS BUILT";
            return;
        }

        string activeAbilityName =
            selectedDefinition.ActiveAbility != null
                ? selectedDefinition.ActiveAbility.DisplayName
                : "NONE";

        string passiveAbilityName =
            selectedDefinition.PassiveAbility != null
                ? selectedDefinition.PassiveAbility.DisplayName
                : "NONE";

        statusText.text =
            $"{GetMenuDisplayName(selectedPlayerId)}\n" +
            $"HP {selectedDefinition.BaseMaxHealth}   " +
            $"AFFINITY {selectedDefinition.AffinityGemType}\n" +
            $"ACTIVE {activeAbilityName}\n" +
            $"PASSIVE {passiveAbilityName}";
    }

    private void SetButtonColor(
        Button button,
        string playerId,
        bool available)
    {
        if (button == null ||
            button.targetGraphic == null)
        {
            return;
        }

        if (string.Equals(
                selectedPlayerId,
                playerId,
                StringComparison.Ordinal
            ))
        {
            button.targetGraphic.color =
                SelectedColor;
            return;
        }

        button.targetGraphic.color =
            available
                ? UnselectedColor
                : UnavailableColor;
    }

    private void EnsureInitialControls()
    {
        if (transform.Find("RattlebonesButton") != null)
        {
            return;
        }

        CreateText(
            "Title",
            "CHOOSE YOUR CHARACTER",
            new Vector2(0.08f, 0.85f),
            new Vector2(0.92f, 0.96f),
            32,
            FontStyle.Bold,
            UnselectedColor,
            false
        );

        CreateText(
            "Instruction",
            "WHO ENTERS THE DUNGEON?",
            new Vector2(0.08f, 0.77f),
            new Vector2(0.92f, 0.84f),
            17,
            FontStyle.Normal,
            SecondaryColor,
            false
        );

        CreateButton(
            "RattlebonesButton",
            "RATTLEBONES",
            new Vector2(0.08f, 0.56f),
            new Vector2(0.47f, 0.72f),
            22,
            UnselectedColor
        );

        CreateButton(
            "BardleyButton",
            "BARDLEY",
            new Vector2(0.53f, 0.56f),
            new Vector2(0.92f, 0.72f),
            22,
            UnavailableColor
        );

        statusText =
            CreateText(
                "Status",
                "RATTLEBONES",
                new Vector2(0.08f, 0.31f),
                new Vector2(0.92f, 0.52f),
                18,
                FontStyle.Bold,
                UnselectedColor,
                false
            );

        CreateButton(
            "StartButton",
            "START",
            new Vector2(0.25f, 0.18f),
            new Vector2(0.75f, 0.28f),
            22,
            UnselectedColor
        );

        CreateButton(
            "BackButton",
            "BACK",
            new Vector2(0.25f, 0.07f),
            new Vector2(0.75f, 0.15f),
            18,
            SecondaryColor
        );
    }

    private Text CreateText(
        string objectName,
        string textValue,
        Vector2 anchorMin,
        Vector2 anchorMax,
        int fontSize,
        FontStyle fontStyle,
        Color color,
        bool raycastTarget)
    {
        GameObject textObject =
            new GameObject(
                objectName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Text)
            );

        textObject.layer = gameObject.layer;
        textObject.transform.SetParent(
            transform,
            false
        );

        RectTransform rect =
            textObject.GetComponent<RectTransform>();

        ConfigureRect(
            rect,
            anchorMin,
            anchorMax
        );

        Text text =
            textObject.GetComponent<Text>();

        text.font =
            Resources.GetBuiltinResource<Font>(
                "LegacyRuntime.ttf"
            );

        text.text = textValue;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.alignment = TextAnchor.MiddleCenter;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 10;
        text.resizeTextMaxSize = fontSize;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.color = color;
        text.raycastTarget = raycastTarget;

        return text;
    }

    private Button CreateButton(
        string objectName,
        string label,
        Vector2 anchorMin,
        Vector2 anchorMax,
        int fontSize,
        Color color)
    {
        Text text =
            CreateText(
                objectName,
                label,
                anchorMin,
                anchorMax,
                fontSize,
                FontStyle.Bold,
                color,
                true
            );

        Button button =
            text.gameObject.AddComponent<Button>();

        button.targetGraphic = text;

        ColorBlock colors =
            button.colors;

        colors.normalColor = Color.white;
        colors.highlightedColor = Color.white;
        colors.selectedColor = Color.white;
        colors.pressedColor =
            new Color(0.7f, 0.7f, 0.7f, 1f);
        colors.disabledColor =
            new Color(0.45f, 0.45f, 0.45f, 0.7f);
        colors.fadeDuration = 0.08f;

        button.colors = colors;

        return button;
    }

    private static void ConfigureRect(
        RectTransform rect,
        Vector2 anchorMin,
        Vector2 anchorMax)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot =
            new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;
        rect.localScale = Vector3.one;
    }

    private bool ResolveReferences()
    {
        rattlebonesButton =
            FindComponent<Button>(
                "RattlebonesButton"
            );

        bardleyButton =
            FindComponent<Button>(
                "BardleyButton"
            );

        startButton =
            FindComponent<Button>(
                "StartButton"
            );

        backButton =
            FindComponent<Button>(
                "BackButton"
            );

        statusText =
            FindComponent<Text>(
                "Status"
            );

        bool hasAllReferences =
            rattlebonesButton != null &&
            bardleyButton != null &&
            startButton != null &&
            backButton != null &&
            statusText != null;

        if (!hasAllReferences)
        {
            Debug.LogError(
                "CharacterSelectMenuController could not resolve its character-select controls.",
                this
            );
        }

        return hasAllReferences;
    }

    private T FindComponent<T>(
        string path)
        where T : Component
    {
        Transform child =
            transform.Find(path);

        return child != null
            ? child.GetComponent<T>()
            : null;
    }

    private static string GetMenuDisplayName(
        string playerId)
    {
        return string.Equals(
                playerId,
                CharacterSelectionSettings.BardleyPlayerId,
                StringComparison.Ordinal
            )
            ? "BARDLEY"
            : "RATTLEBONES";
    }
}
