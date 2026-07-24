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
    private RectTransform energyBarFillMask;

    [SerializeField]
    private PlayerAbilityController
        playerAbilityController;

    [Header("Energy Animation")]
    [SerializeField]
    [Min(0.01f)]
    private float energyFillSmoothTime = 0.18f;

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

    private float energyBarMaximumWidth;
    private float displayedCharge;
    private float targetCharge;
    private float chargeVelocity;
    private bool hasInitializedCharge;

    private void Awake()
    {
        if (abilityButton == null)
        {
            abilityButton =
                GetComponent<Button>();
        }

        if (energyBarFillMask != null)
        {
            energyBarMaximumWidth =
                energyBarFillMask.rect.width;
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

        hasInitializedCharge = false;
        chargeVelocity = 0f;

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

    private void Update()
    {
        if (!hasInitializedCharge)
        {
            return;
        }

        displayedCharge =
            Mathf.SmoothDamp(
                displayedCharge,
                targetCharge,
                ref chargeVelocity,
                Mathf.Max(
                    0.01f,
                    energyFillSmoothTime
                ),
                Mathf.Infinity,
                Time.unscaledDeltaTime
            );

        if (Mathf.Abs(
                displayedCharge -
                targetCharge
            ) < 0.001f)
        {
            displayedCharge =
                targetCharge;

            chargeVelocity = 0f;
        }

        ApplyEnergyVisual(
            displayedCharge
        );
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

        normalizedCharge =
            Mathf.Clamp01(
                normalizedCharge
            );

        if (!hasInitializedCharge)
        {
            displayedCharge =
                normalizedCharge;

            targetCharge =
                normalizedCharge;

            hasInitializedCharge = true;

            ApplyEnergyVisual(
                displayedCharge
            );
        }
        else
        {
            targetCharge =
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

    private void ApplyEnergyVisual(
        float normalizedCharge)
    {
        normalizedCharge =
            Mathf.Clamp01(
                normalizedCharge
            );

        if (energyFill != null)
        {
            energyFill.fillAmount =
                normalizedCharge;
        }

        if (energyBarFillMask == null)
        {
            return;
        }

        if (energyBarMaximumWidth <= 0f)
        {
            energyBarMaximumWidth =
                energyBarFillMask.rect.width;
        }

        energyBarFillMask
            .SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                energyBarMaximumWidth *
                normalizedCharge
            );
    }
}