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
                "BARDLEY\nCHARACTER DATA COMING NEXT";
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
            button.targetGraphic is not Image image)
        {
            return;
        }

        if (string.Equals(
                selectedPlayerId,
                playerId,
                StringComparison.Ordinal
            ))
        {
            image.color = SelectedColor;
            return;
        }

        image.color =
            available
                ? UnselectedColor
                : UnavailableColor;
    }

    private bool ResolveReferences()
    {
        rattlebonesButton =
            FindComponent<Button>(
                "Panel/RattlebonesButton"
            );

        bardleyButton =
            FindComponent<Button>(
                "Panel/BardleyButton"
            );

        startButton =
            FindComponent<Button>(
                "Panel/StartButton"
            );

        backButton =
            FindComponent<Button>(
                "Panel/BackButton"
            );

        statusText =
            FindComponent<Text>(
                "Panel/Status"
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
                "CharacterSelectMenuController could not resolve its authored UI hierarchy.",
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
