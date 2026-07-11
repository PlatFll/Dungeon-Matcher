using UnityEngine;

public abstract class CharacterPassiveDefinition : ScriptableObject
{
    [Header("Identity")]
    [SerializeField]
    [Tooltip("Stable internal identifier. Avoid changing it after release.")]
    private string passiveId = "passive_id";

    [SerializeField]
    private string displayName = "Passive";

    [SerializeField]
    [TextArea(2, 5)]
    private string description;

    [Header("Presentation")]
    [SerializeField]
    private Sprite icon;

    public string PassiveId => passiveId;
    public string DisplayName => displayName;
    public string Description => description;
    public Sprite Icon => icon;

    protected virtual void OnValidate()
    {
        passiveId =
            passiveId
                .Trim()
                .ToLowerInvariant()
                .Replace(" ", "_");

        displayName = displayName.Trim();
    }
}