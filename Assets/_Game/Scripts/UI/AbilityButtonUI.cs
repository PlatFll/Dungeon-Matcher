using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Button))]
public sealed class AbilityButtonUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Button abilityButton;

    [SerializeField]
    private Image abilityIcon;

    [SerializeField]
    private Image energyFill;

    [SerializeField]
    private PlayerAbilityController
        playerAbilityController;

    [Header("Icon Colors")]
    [SerializeField]
    private Color unavailableIconColor =
        new Color(
            0.35f,
            0.35f,
            0.35f,
            1f
        );

    [SerializeField]
    private Color readyIconColor =
        Color.white;

    private void Awake()
    {
        if (abilityButton == null)
        {
            abilityButton =
                GetComponent<Button>();
        }
    }

    private void OnEnable()
    {
        if (abilityButton != null)
        {
            abilityButton.onClick.RemoveListener(
                HandleButtonClicked
            );

            abilityButton.onClick.AddListener(
                HandleButtonClicked
            );
        }

        if (playerAbilityController != null)
        {
            playerAbilityController.StateChanged -=
                HandleAbilityStateChanged;

            playerAbilityController.StateChanged +=
                HandleAbilityStateChanged;
        }

        RefreshVisuals();
    }

    private void OnDisable()
    {
        if (abilityButton != null)
        {
            abilityButton.onClick.RemoveListener(
                HandleButtonClicked
            );
        }

        if (playerAbilityController != null)
        {
            playerAbilityController.StateChanged -=
                HandleAbilityStateChanged;
        }
    }

    private void HandleButtonClicked()
    {
        if (playerAbilityController == null)
        {
            return;
        }

        playerAbilityController.TryActivate();

        RefreshVisuals();
    }

    private void HandleAbilityStateChanged()
    {
        RefreshVisuals();
    }

    private void RefreshVisuals()
    {
        CharacterAbilityDefinition ability =
            playerAbilityController != null
                ? playerAbilityController.ActiveAbility
                : null;

        float normalizedCharge =
            playerAbilityController != null
                ? playerAbilityController
                    .ChargeNormalized
                : 0f;

        bool canActivate =
            playerAbilityController != null &&
            playerAbilityController.CanActivate;

        if (energyFill != null)
        {
            energyFill.fillAmount =
                normalizedCharge;
        }

        if (abilityButton != null)
        {
            abilityButton.interactable =
                canActivate;
        }

        if (abilityIcon != null)
        {
            if (ability != null &&
                ability.Icon != null)
            {
                abilityIcon.sprite =
                    ability.Icon;
            }

            abilityIcon.color =
                canActivate
                    ? readyIconColor
                    : unavailableIconColor;
        }
    }
}