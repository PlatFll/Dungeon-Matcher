using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(
    fileName = "Enemy_",
    menuName = "Dungeon Matcher/Enemies/Enemy Definition"
)]
public sealed class EnemyDefinition : ScriptableObject
{
    [Header("Identity")]

    [SerializeField]
    [Tooltip(
        "Stable internal identifier. Avoid changing it after release."
    )]
    private string enemyId = "enemy_id";

    [SerializeField]
    private string displayName = "Enemy";

    [SerializeField]
    private EnemyCategory category =
        EnemyCategory.Normal;

    [SerializeField]
    [TextArea(2, 4)]
    private string description;

    [Header("Prefab")]

    [SerializeField]
    [Tooltip(
        "Shared enemy shell prefab in most cases. Use a dedicated prefab only " +
        "when the enemy has genuinely different hierarchy/UI requirements."
    )]
    private GameObject enemyPrefab;

    [Header("Visuals")]

    [SerializeField]
    [FormerlySerializedAs("staticVisualSprite")]
    [Tooltip(
        "Single-frame fallback/preview artwork. This is used when no animation " +
        "controller override has been assigned yet, and also gives the Image a " +
        "safe frame before an Animator begins driving it."
    )]
    private Sprite fallbackVisualSprite;

    [SerializeField]
    [Tooltip(
        "Optional per-enemy Runtime Animator Controller. An Animator Override " +
        "Controller is recommended when enemies share the same states but use " +
        "different clips. When assigned, it replaces the shared prefab's " +
        "controller and animation playback remains enabled."
    )]
    private RuntimeAnimatorController animationControllerOverride;

    [SerializeField]
    [Tooltip(
        "Display size of this enemy's VisualRoot. " +
        "Use a larger size for animation canvases that need extra room."
    )]
    private Vector2 visualSize =
        new Vector2(112f, 112f);

    [Header("Animation Impact Timing")]

    [SerializeField]
    [Tooltip(
        "When enabled, auto-attack damage waits for the AutoAttackImpact " +
        "Animation Event instead of resolving when the attack animation starts."
    )]
    private bool timeAutoAttackFromAnimation;

    [SerializeField]
    [Tooltip(
        "When enabled, the special ability waits for the AbilityImpact " +
        "Animation Event before applying its gameplay effect."
    )]
    private bool timeSpecialAbilityFromAnimation;

    [Header("Base Combat Stats")]

    [SerializeField, Min(1)]
    private int baseMaxHealth = 100;

    [SerializeField, Min(0)]
    private int baseDamage = 10;

    [SerializeField, Min(0)]
    [Tooltip(
        "Optional second automatic-attack hit. Keep at zero for the " +
        "established single-hit behavior."
    )]
    private int baseFollowUpDamage;

    [SerializeField, Min(0f)]
    [Tooltip(
        "Optional delay after the first automatic-attack presentation has " +
        "returned to rest and before the follow-up attack begins."
    )]
    private float followUpAttackDelay;

    [SerializeField, Min(0.1f)]
    [Tooltip("Seconds between automatic attacks.")]
    private float baseAttackInterval = 3f;

    [Header("Special Ability")]

    [SerializeField]
    private bool hasSpecialAbility;

    [SerializeField]
    [Tooltip(
        "Runtime behavior attached for this enemy's special ability. " +
        "Keep None for enemies without a special."
    )]
    private EnemySpecialAbilityKind specialAbilityKind =
        EnemySpecialAbilityKind.None;

    [SerializeField, Min(1)]
    [Tooltip(
        "Number of valid player turns required before " +
        "the enemy uses its special ability."
    )]
    private int baseSpecialTurnRequirement = 5;

    [SerializeField]
    [Tooltip(
        "When enabled, this enemy always uses the exact base special-turn " +
        "requirement. Global difficulty scaling cannot shorten its cadence."
    )]
    private bool lockSpecialTurnRequirement;

    [Header("Shielding Allies Ability")]

    [SerializeField, Min(1)]
    [Tooltip(
        "Shield granted to each other living enemy by one successful cast."
    )]
    private int allyShieldAmount = 10;

    [SerializeField, Min(1)]
    [Tooltip(
        "Shield granted to the casting enemy by one successful cast."
    )]
    private int selfShieldAmount = 15;

    [Header("Barricade Ability")]

    [SerializeField, Min(1)]
    [Tooltip(
        "Number of barricades placed by one accepted barricade ability use."
    )]
    private int barricadesPerUse = 2;

    [SerializeField, Min(1)]
    [Tooltip(
        "Maximum number of barricades this enemy may own on the board at once."
    )]
    private int maximumOwnedBarricades = 6;

    [SerializeField, Min(1)]
    [Tooltip(
        "Number of adjacent clear hits required to break each barricade."
    )]
    private int barricadeDurability = 1;

    [SerializeField]
    [Tooltip(
        "Visual/material family used by barricades placed by this enemy."
    )]
    private EnemyBarricadeStyle barricadeStyle =
        EnemyBarricadeStyle.Wood;

    [Header("Spawn Rules")]

    [SerializeField, Min(1)]
    private int minimumWave = 1;

    [SerializeField, Min(0.01f)]
    [Tooltip(
        "Relative selection weight among other eligible " +
        "enemies in the same category."
    )]
    private float spawnWeight = 1f;

    [Header("Individual Scaling Modifiers")]

    [SerializeField, Min(0.1f)]
    [Tooltip(
        "Multiplies this enemy's health after global " +
        "wave scaling is calculated."
    )]
    private float healthMultiplier = 1f;

    [SerializeField, Min(0.1f)]
    [Tooltip(
        "Multiplies this enemy's damage after global " +
        "wave scaling is calculated."
    )]
    private float damageMultiplier = 1f;

    [SerializeField, Min(0.1f)]
    [Tooltip(
        "Values above 1 make this enemy attack faster. " +
        "Values below 1 make it attack slower."
    )]
    private float attackSpeedMultiplier = 1f;

    [Header("Status Resistance")]

    [SerializeField, Range(0f, 2f)]
    [Tooltip(
        "Multiplies stagger duration. " +
        "1 means normal stagger, 0.5 means half duration, " +
        "and 0 makes this enemy immune."
    )]
    private float staggerDurationMultiplier = 1f;

    public string EnemyId =>
        enemyId;

    public string DisplayName =>
        displayName;

    public EnemyCategory Category =>
        category;

    public string Description =>
        description;

    public GameObject EnemyPrefab =>
        enemyPrefab;

    public Sprite FallbackVisualSprite =>
        fallbackVisualSprite;

    public RuntimeAnimatorController
        AnimationControllerOverride =>
            animationControllerOverride;

    public Vector2 VisualSize =>
        visualSize.x > 0f &&
        visualSize.y > 0f
            ? visualSize
            : new Vector2(112f, 112f);

    public bool TimeAutoAttackFromAnimation =>
        timeAutoAttackFromAnimation;

    public bool TimeSpecialAbilityFromAnimation =>
        timeSpecialAbilityFromAnimation;

    /*
     * Compatibility property for the current WaveController spawn path.
     * New visual setup is finalized by EnemyVisualPresenter from EnemyActor.
     */
    public Sprite StaticVisualSprite =>
        fallbackVisualSprite;

    public int BaseMaxHealth =>
        baseMaxHealth;

    public int BaseDamage =>
        baseDamage;

    public int BaseFollowUpDamage =>
        baseFollowUpDamage;

    public float FollowUpAttackDelay =>
        followUpAttackDelay;

    public float BaseAttackInterval =>
        baseAttackInterval;

    public bool HasSpecialAbility =>
        hasSpecialAbility;

    public EnemySpecialAbilityKind SpecialAbilityKind =>
        specialAbilityKind;

    public int BaseSpecialTurnRequirement =>
        baseSpecialTurnRequirement;

    public bool LockSpecialTurnRequirement =>
        lockSpecialTurnRequirement;

    public int AllyShieldAmount =>
        allyShieldAmount;

    public int SelfShieldAmount =>
        selfShieldAmount;

    public int BarricadesPerUse =>
        barricadesPerUse;

    public int MaximumOwnedBarricades =>
        maximumOwnedBarricades;

    public int BarricadeDurability =>
        barricadeDurability;

    public EnemyBarricadeStyle BarricadeStyle =>
        barricadeStyle;

    public int MinimumWave =>
        minimumWave;

    public float SpawnWeight =>
        spawnWeight;

    public float HealthMultiplier =>
        healthMultiplier;

    public float DamageMultiplier =>
        damageMultiplier;

    public float AttackSpeedMultiplier =>
        attackSpeedMultiplier;

    public float StaggerDurationMultiplier =>
        staggerDurationMultiplier;

    private void OnValidate()
    {
        enemyId =
            string.IsNullOrWhiteSpace(enemyId)
                ? "enemy_id"
                : enemyId
                    .Trim()
                    .ToLowerInvariant()
                    .Replace(" ", "_");

        displayName =
            string.IsNullOrWhiteSpace(displayName)
                ? "Enemy"
                : displayName.Trim();

        baseMaxHealth =
            Mathf.Max(
                1,
                baseMaxHealth
            );

        baseDamage =
            Mathf.Max(
                0,
                baseDamage
            );

        baseFollowUpDamage =
            Mathf.Max(
                0,
                baseFollowUpDamage
            );

        followUpAttackDelay =
            Mathf.Max(
                0f,
                followUpAttackDelay
            );

        baseAttackInterval =
            Mathf.Max(
                0.1f,
                baseAttackInterval
            );

        baseSpecialTurnRequirement =
            Mathf.Max(
                1,
                baseSpecialTurnRequirement
            );

        allyShieldAmount =
            Mathf.Max(
                1,
                allyShieldAmount
            );

        selfShieldAmount =
            Mathf.Max(
                1,
                selfShieldAmount
            );

        barricadesPerUse =
            Mathf.Max(
                1,
                barricadesPerUse
            );

        maximumOwnedBarricades =
            Mathf.Max(
                1,
                maximumOwnedBarricades
            );

        barricadesPerUse =
            Mathf.Min(
                barricadesPerUse,
                maximumOwnedBarricades
            );

        barricadeDurability =
            Mathf.Max(
                1,
                barricadeDurability
            );

        minimumWave =
            Mathf.Max(
                1,
                minimumWave
            );

        spawnWeight =
            Mathf.Max(
                0.01f,
                spawnWeight
            );

        healthMultiplier =
            Mathf.Max(
                0.1f,
                healthMultiplier
            );

        damageMultiplier =
            Mathf.Max(
                0.1f,
                damageMultiplier
            );

        attackSpeedMultiplier =
            Mathf.Max(
                0.1f,
                attackSpeedMultiplier
            );

        staggerDurationMultiplier =
            Mathf.Clamp(
                staggerDurationMultiplier,
                0f,
                2f
            );

        if (!hasSpecialAbility)
        {
            specialAbilityKind =
                EnemySpecialAbilityKind.None;

            lockSpecialTurnRequirement = false;
            timeSpecialAbilityFromAnimation = false;
        }
    }
}
