using UnityEngine;

[CreateAssetMenu(
    fileName = "Player_",
    menuName = "Dungeon Matcher/Players/Player Definition"
)]
public sealed class PlayerDefinition : ScriptableObject
{
    [Header("Identity")]
    [SerializeField]
    [Tooltip(
        "Stable internal identifier. Avoid changing it after release."
    )]
    private string playerId = "player_id";

    [SerializeField]
    private string displayName = "Player";

    [SerializeField]
    [TextArea(2, 5)]
    private string description;

    [Header("Battle Presentation")]
    [SerializeField]
    [Tooltip(
        "Optional prefab for the player's runtime visual, " +
        "effects, animation helpers, or future logic."
    )]
    private GameObject playerPrefab;

    [SerializeField]
    [Tooltip(
        "Decorative frame shown in the battle HUD around " +
        "the player's character."
    )]
    private Sprite battleFrameSprite;

    [SerializeField]
    [Tooltip(
        "Full character sprite displayed inside the battle frame."
    )]
    private Sprite battleCharacterSprite;

    [SerializeField]
    [Tooltip(
        "Optional separate portrait for menus or future " +
        "character selection screens."
    )]
    private Sprite menuPortrait;

    [Header("Base Stats")]
    [SerializeField, Min(1)]
    private int baseMaxHealth = 100;

    [Header("Gem Affinity")]
    [SerializeField]
    [Tooltip(
        "Matching this gem type heals this character. " +
        "The player and an enemy may share the same color."
    )]
    private GemType affinityGemType =
        GemType.Emerald;

    [Header("Character Features")]
    [SerializeField]
    private CharacterAbilityDefinition activeAbility;

    [SerializeField]
    private CharacterPassiveDefinition passiveAbility;

    public string PlayerId => playerId;
    public string DisplayName => displayName;
    public string Description => description;

    public GameObject PlayerPrefab => playerPrefab;
    public Sprite BattleFrameSprite => battleFrameSprite;
    public Sprite BattleCharacterSprite =>
        battleCharacterSprite;

    public Sprite MenuPortrait => menuPortrait;

    public int BaseMaxHealth => baseMaxHealth;

    public GemType AffinityGemType =>
        affinityGemType;

    public CharacterAbilityDefinition ActiveAbility =>
        activeAbility;

    public CharacterPassiveDefinition PassiveAbility =>
        passiveAbility;

    private void OnValidate()
    {
        playerId =
            playerId
                .Trim()
                .ToLowerInvariant()
                .Replace(" ", "_");

        displayName = displayName.Trim();
        baseMaxHealth = Mathf.Max(1, baseMaxHealth);
    }
}