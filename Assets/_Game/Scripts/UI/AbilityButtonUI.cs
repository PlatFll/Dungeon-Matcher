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
    private PlayerAbilityEnergy playerAbilityEnergy;

    [Header("Icon Colors")]
    [SerializeField]
    private Color unavailableIconColor =
        new Color(0.35f, 0.35f, 0.35f, 1f);

    [SerializeField]
    private Color readyIconColor =
        Color.white;

    private void Awake()
    {
        if (abilityButton == null)
        {
            abilityButton = GetComponent<Button>();
        }
    }

    private void OnEnable()
    {
        if (playerAbilityEnergy == null)
        {
            return;
        }

        playerAbilityEnergy.EnergyChanged +=
            HandleEnergyChanged;

        SetEnergy(
            playerAbilityEnergy.CurrentEnergy,
            playerAbilityEnergy.MaximumEnergy
        );
    }

    private void OnDisable()
    {
        if (playerAbilityEnergy == null)
        {
            return;
        }

        playerAbilityEnergy.EnergyChanged -=
            HandleEnergyChanged;
    }

    private void HandleEnergyChanged(
        int currentEnergy,
        int maximumEnergy)
    {
        SetEnergy(
            currentEnergy,
            maximumEnergy
        );
    }

    public void SetEnergy(
        float currentEnergy,
        float requiredEnergy)
    {
        float normalizedEnergy =
            requiredEnergy > 0f
                ? Mathf.Clamp01(
                    currentEnergy / requiredEnergy
                )
                : 1f;

        if (energyFill != null)
        {
            energyFill.fillAmount =
                normalizedEnergy;
        }

        bool isReady =
            normalizedEnergy >= 1f;

        if (abilityButton != null)
        {
            abilityButton.interactable =
                isReady;
        }

        if (abilityIcon != null)
        {
            abilityIcon.color =
                isReady
                    ? readyIconColor
                    : unavailableIconColor;
        }
    }
}