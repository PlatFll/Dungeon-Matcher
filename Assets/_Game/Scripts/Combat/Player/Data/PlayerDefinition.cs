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
        "Optional Animator Controller used by this character in battle. " +
        "Leave empty for a static battle character sprite."
    )]
    private RuntimeAnimatorController battleAnimatorController;

    [SerializeField]
    [Tooltip(
        "Optional separate portrait for menus or future " +
        "character selection screens."
    )]
    private Sprite menuPortrait;

    [Header("Base Stats")]
    [SerializeField, Min(1)]
    private int baseMaxHealth = 100;

    [SerializeField, Min(0)] private int healthPerLevel = 10;
    [SerializeField, Min(1)] private float baseGemDamage = 11f;
    [SerializeField, Min(0)] private float gemDamagePerLevel = 0.55f;
    [SerializeField, Min(0)] private float abilityDamageGrowth = 0.06f;
    [SerializeField, Min(1)] private int baseShieldCap = 45;
    [SerializeField, Min(0)] private int shieldCapPerLevel = 3;

    public int HealthAtLevel(int level) => baseMaxHealth + healthPerLevel * (Mathf.Clamp(level, 1, BalanceV1.Current.levelCap) - 1);
    public float GemDamageAtLevel(int level) => baseGemDamage + gemDamagePerLevel * (Mathf.Clamp(level, 1, BalanceV1.Current.levelCap) - 1);
    public float AbilityMultiplierAtLevel(int level) => 1f + abilityDamageGrowth * (Mathf.Clamp(level, 1, BalanceV1.Current.levelCap) - 1);
    public int ShieldCapAtLevel(int level) => baseShieldCap + shieldCapPerLevel * (Mathf.Clamp(level, 1, BalanceV1.Current.levelCap) - 1);

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

    public RuntimeAnimatorController BattleAnimatorController =>
        battleAnimatorController;

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
