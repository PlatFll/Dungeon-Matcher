using UnityEngine;

public enum AbilityMeterDisplay
{
    Custom = 0,
    Charge = 1,
    Cooldown = 2
}

public abstract class CharacterAbilityDefinition : ScriptableObject
{
    [Header("Identity")]
    [SerializeField]
    [Tooltip("Stable internal identifier. Avoid changing it after release.")]
    private string abilityId = "ability_id";

    [SerializeField]
    private string displayName = "Ability";

    [SerializeField]
    [TextArea(2, 5)]
    private string description;

    [Header("Ability Button")]
    [SerializeField]
    private Sprite icon;

    [SerializeField]
    [Tooltip(
        "Optional short text displayed on the ability button. " +
        "Leave empty to use the ability name."
    )]
    private string buttonLabel;

    [SerializeField]
    [Tooltip(
        "Controls how the ability button represents readiness. " +
        "The actual behavior will be implemented by each ability."
    )]
    private AbilityMeterDisplay meterDisplay =
        AbilityMeterDisplay.Custom;

    public string AbilityId => abilityId;
    public string DisplayName => displayName;
    public string Description => description;
    public Sprite Icon => icon;

    public string ButtonLabel =>
        string.IsNullOrWhiteSpace(buttonLabel)
            ? displayName
            : buttonLabel;

    public AbilityMeterDisplay MeterDisplay =>
        meterDisplay;

    protected virtual void OnValidate()
    {
        abilityId =
            abilityId
                .Trim()
                .ToLowerInvariant()
                .Replace(" ", "_");

        displayName = displayName.Trim();
        buttonLabel = buttonLabel.Trim();
    }
}