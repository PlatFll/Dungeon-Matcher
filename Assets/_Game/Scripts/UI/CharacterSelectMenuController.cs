using System;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class CharacterSelectMenuController : MonoBehaviour
{
    private static readonly Color SelectedColor =
        new Color32(151, 91, 170, 255);

    private static readonly Color UnselectedColor =
        new Color32(91, 46, 105, 255);

    private static readonly Color UnavailableColor =
        new Color32(55, 39, 63, 255);

    [Header("Character Options")]
    [SerializeField]
    private Button rattlebonesButton;

    [SerializeField]
    private Button bardleyButton;

    [Header("Selection Presentation")]
    [SerializeField]
    private Image characterPreview;

    [SerializeField]
    private Text statusText;

    [Header("Navigation")]
    [SerializeField]
    private Button startButton;

    [SerializeField]
    private Button backButton;

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

        if (!HasRequiredReferences())
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

        RefreshCharacterPreview(
            selectedDefinition
        );

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

    private void RefreshCharacterPreview(
        PlayerDefinition definition)
    {
        if (characterPreview == null)
        {
            return;
        }

        Sprite previewSprite = null;

        if (definition != null)
        {
            previewSprite =
                definition.MenuPortrait != null
                    ? definition.MenuPortrait
                    : definition.BattleCharacterSprite;
        }

        characterPreview.sprite =
            previewSprite;

        characterPreview.enabled =
            previewSprite != null;
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

    private bool HasRequiredReferences()
    {
        bool hasAllReferences =
            rattlebonesButton != null &&
            bardleyButton != null &&
            characterPreview != null &&
            statusText != null &&
            startButton != null &&
            backButton != null;

        if (hasAllReferences)
        {
            return true;
        }

        Debug.LogError(
            "CharacterSelectMenuController requires both character buttons, " +
            "preview/status presentation, and Start/Back buttons.",
            this
        );

        return false;
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
