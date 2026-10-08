using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(
    fileName = "Enemy_",
    menuName = "Dungeon Matcher/Enemies/Enemy Definition"
)]
public sealed class EnemyDefinition : ScriptableObject
{
    [Header("Ironvein ore network")]
    public bool oreWeaponEligible;
    public bool releasesOreOnDefeat;
    public CombatSoundCue mineBasicSound = CombatSoundCue.MineHammer;
    [Min(1f)] public float oreAttackMultiplier = 1.3f;
    [Min(1)] public int mineWarningMoves = 2;
    [Min(0)] public int mineAbilityDamage = 20;
    public EnemyDefinition mineTurret;
    [Range(1,2)] public int maximumMineTurrets = 2;
    public Vector3 mineExtractionMultipliers = new Vector3(1.3f,1.6f,2f);
    [Min(1f)] public float mineObsidianSlamMultiplier = 1.5f;
    [Header("Grand Delver experimental remount (pending design review)")]
    public bool mineEnableRemount;
    [Tooltip("Optional native phase art. Missing presentation never changes boss rules.")]
    public RuntimeAnimatorController minePilotController, mineReserveController;
    public Sprite minePilotSprite, mineReserveSprite;
    [Min(1)] public int minePilotMoves = 3;
    [Range(.05f,.5f)] public float minePilotHealthFraction = .2f;
    [Range(.1f,.75f)] public float mineReserveHealthFraction = .5f;
    [Header("Optional shared player debuff (unassigned in the production roster)")]
    public PlayerStatusDefinition appliedPlayerStatus;
    [Tooltip("The complete formation must opt in before the court floods. Unknown/global enemies stay dry-only.")]
    public bool canFightFlooded;
    public EnemyDefinition aquaticSummon;
    [Min(1)] public int aquaticChannelMoves = 2;
    [Min(0)] public int aquaticAbilityDamage = 20;
    [Min(0)] public int aquaticShield = 25;
    [Min(1)] public float aquaticRallyDamage = 1.3f;
    [Min(0)] public float aquaticRallySeconds = 5f;
    [Range(1,3)] public int tributeTargetCount=3;
    [Min(1)] public int tributeChannelMoves=3;
    [Range(1,2)] public int tributeStackCap=2;
    [Header("Identity")]

    [SerializeField]
    [Tooltip(
        "Stable internal identifier. Avoid changing it after release."
    )]
    private string enemyId = "enemy_id";

    [Header("Content taxonomy (independent of weakness and rank)")]
    [SerializeField] private string race;
    [SerializeField] private string faction;
    [SerializeField] private string combatRole;
    [SerializeField] private string[] eligibleZones = new string[0];
    public string Race => race;
    public string Faction => faction;
    public string CombatRole => combatRole;
    public System.Collections.Generic.IReadOnlyList<string> EligibleZones => eligibleZones;

    [Header("Encounter Budget")]
    [SerializeField, Min(0.1f)] private float threatCost = 1f;
    [SerializeField] private bool isBoardDisruptor;
    [SerializeField] private bool isSupport;
    [SerializeField, Range(1, 3)] private int chainCap = 2;
    [SerializeField, Range(1, 3)] private int chainsPerUse = 1;
    public float ThreatCost => Mathf.Max(0.1f, threatCost);
    public bool IsBoardDisruptor => isBoardDisruptor;
    public bool IsSupport => isSupport;
    public int ChainCap => chainCap;
    public int ChainsPerUse => Mathf.Clamp(chainsPerUse, 1, ChainCap);

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
    [Tooltip("The sprite clip supplies the attack movement and AutoAttackComplete event. Suppresses the generic UI lunge.")]
    private bool useAuthoredAutoAttackMotion;

    [SerializeField]
    [Tooltip(
        "When enabled, the special ability waits for the AbilityImpact " +
        "Animation Event before applying its gameplay effect."
    )]
    private bool timeSpecialAbilityFromAnimation;

    [SerializeField]
    [Tooltip("The authored Ability clip owns recovery until AbilityComplete. Missing events retain a bounded gameplay fallback.")]
    private bool useAuthoredSpecialAbilityMotion;

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

    [Header("Accepted-move prototype")]
    [SerializeField, Min(1)] private int firstAttackMoves = 3;
    [SerializeField, Min(1)] private int attackMoves = 3;
    public int FirstAttackMoves => Mathf.Max(1, firstAttackMoves);
    public int AttackMoves => Mathf.Max(1, attackMoves);
    [Header("Unified move combat (experimental)")]
    [SerializeField, Min(2)] private int unifiedFirstAttackMoves = 4;
    [SerializeField, Min(2)] private int unifiedAttackMoves = 4;
    public int UnifiedFirstAttackMoves => Mathf.Max(2, unifiedFirstAttackMoves);
    public int UnifiedAttackMoves => Mathf.Max(2, unifiedAttackMoves);

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

    [Header("Town Marshal Ability")]

    [SerializeField]
    [Tooltip(
        "Independent local enemies that Ring the Bell may summon into a free " +
        "enemy slot. They remain in the wave if the Marshal is defeated."
    )]
    private EnemyDefinition[] townMarshalSummonCandidates =
        new EnemyDefinition[0];

    [SerializeField, Min(1)]
    [Tooltip(
        "Accepted player moves that Big Man in Town can keep the Marshal " +
        "retreated behind the newly summoned protector."
    )]
    private int townMarshalRetreatMoveCount = 2;

    [SerializeField, Min(1f)]
    [Tooltip(
        "Real-time auto-attack speed multiplier applied by Citizens, Seize Him!"
    )]
    private float townMarshalRallyAttackSpeedMultiplier = 1.4f;

    [SerializeField, Min(0.1f)]
    [Tooltip(
        "Real-time duration in seconds of Citizens, Seize Him!"
    )]
    private float townMarshalRallyDuration = 5f;

    [Header("Siege Sergeant Ability")]
    [SerializeField, Min(1)]
    private int hammerWarningMoves = 2;

    [SerializeField, Min(0)]
    private int hammerBaseDamage = 12;

    [SerializeField, Range(0f, 0.9f)]
    private float barricadeDamageReduction = 0.2f;

    public int HammerWarningMoves => Mathf.Max(1, hammerWarningMoves);
    public int HammerBaseDamage => Mathf.Max(0, hammerBaseDamage);
    public float BarricadeDamageReduction => Mathf.Clamp(barricadeDamageReduction, 0f, 0.9f);

    [Header("Royal Milestone Abilities (first-pass tuning)")]
    [SerializeField] private bool royalAssaultParticipant;
    [SerializeField, Min(1)] private int royalMarkCount = 3;
    [SerializeField, Min(1)] private int royalMarkMoves = 3;
    [SerializeField, Range(0f, 0.1f)] private float restorationHealFraction = 0.033f;
    [SerializeField] private Vector4 triageRankWeights = new Vector4(1f, 1.15f, 1.35f, 1.6f);
    [SerializeField, Range(0f, 1f)] private float triageSelfWeightWithWoundedAllies = 0.5f;
    [SerializeField, Range(0f, 1f)] private float triageMeaningfulWound = 0.1f;
    [SerializeField, Min(1)] private int benedictionTargets = 2;
    [SerializeField, Min(1f)] private float benedictionDamageMultiplier = 1.4f;
    [SerializeField] private Sprite benedictionHaloSprite;
    [SerializeField, Min(0)] private int judgmentBaseDamage = 12;
    [SerializeField, Min(0)] private int judgmentFinisherBaseDamage = 18;
    [SerializeField, Min(1)] private int bombardmentWarningMoves = 2;
    [SerializeField, Min(0)] private int bombardmentBaseDamage = 6;
    [SerializeField, Min(1f)] private float assaultDamageMultiplier = 1.1f;
    [SerializeField, Min(0f)] private float royalCommandWindup = 0.6f;
    [SerializeField, Min(0f)] private float royalCommandSpacing = 0.12f;
    [SerializeField, Min(1f)] private float enrageDamageMultiplier = 1.2f;
    [SerializeField, Min(1f)] private float enrageSpeedMultiplier = 1.25f;
    [SerializeField, Min(1)] private int enragedSpecialMoves = 3;
    [SerializeField] private EnemyDefinition[] royalReinforcements = new EnemyDefinition[0];
    [SerializeField] private EnemyDefinition requiredBossEscort;
    public bool RoyalAssaultParticipant => royalAssaultParticipant;
    public int RoyalMarkCount => Mathf.Max(1, royalMarkCount);
    public int RoyalMarkMoves => Mathf.Max(1, royalMarkMoves);
    public float RestorationHealFraction => Mathf.Clamp01(restorationHealFraction);
    public Vector4 TriageRankWeights => triageRankWeights;
    public float TriageSelfWeightWithWoundedAllies => Mathf.Clamp01(triageSelfWeightWithWoundedAllies);
    public float TriageMeaningfulWound => Mathf.Clamp01(triageMeaningfulWound);
    public int BenedictionTargets => Mathf.Max(1, benedictionTargets);
    public float BenedictionDamageMultiplier => Mathf.Max(1f, benedictionDamageMultiplier);
    public Sprite BenedictionHaloSprite => benedictionHaloSprite;
    public int JudgmentBaseDamage => Mathf.Max(0, judgmentBaseDamage);
    public int JudgmentFinisherBaseDamage => Mathf.Max(0, judgmentFinisherBaseDamage);
    public int BombardmentWarningMoves => Mathf.Max(1, bombardmentWarningMoves);
    public int BombardmentBaseDamage => Mathf.Max(0, bombardmentBaseDamage);
    public float AssaultDamageMultiplier => Mathf.Max(1f, assaultDamageMultiplier);
    public float RoyalCommandWindup => Mathf.Max(0f, royalCommandWindup);
    public float RoyalCommandSpacing => Mathf.Max(0f, royalCommandSpacing);
    public float EnrageDamageMultiplier => Mathf.Max(1f, enrageDamageMultiplier);
    public float EnrageSpeedMultiplier => Mathf.Max(1f, enrageSpeedMultiplier);
    public int EnragedSpecialMoves => Mathf.Max(1, enragedSpecialMoves);
    public EnemyDefinition[] RoyalReinforcements => royalReinforcements;
    public EnemyDefinition RequiredBossEscort => requiredBossEscort;

    [Header("Forest expanded roster")]
    [SerializeField] private EnemyDefinition forestSummon;
    [SerializeField, Min(0)] private int thornRetaliationDamage = 10;
    [SerializeField, Min(1)] private float warRhythmSpeed = 1.4f;
    [SerializeField, Min(.1f)] private float warRhythmSeconds = 5f;
    [SerializeField, Min(1)] private int warRhythmMoves = 3;
    [SerializeField, Min(0)] private int barkArmorShield = 30;
    [SerializeField, Min(0)] private int thornVolleyDamage = 5;
    [SerializeField, Min(0)] private int fallingBoughDamage = 30;
    public EnemyDefinition ForestSummon => forestSummon;
    public int ThornRetaliationDamage => CombatAmounts.Round(thornRetaliationDamage);
    public float WarRhythmSpeed => Mathf.Max(1, warRhythmSpeed);
    public float WarRhythmSeconds => Mathf.Max(.1f, warRhythmSeconds);
    public int WarRhythmMoves => Mathf.Max(1, warRhythmMoves);
    public int BarkArmorShield => CombatAmounts.Round(barkArmorShield);
    public int ThornVolleyDamage => CombatAmounts.Round(thornVolleyDamage);
    public int FallingBoughDamage => CombatAmounts.Round(fallingBoughDamage);

    [Header("Forest ritual tuning")]
    [SerializeField,Min(0)] private int forestRenewalBaseHeal=20;
    [SerializeField,Min(0)] private int forestHeartrootHealBonus=20;
    [SerializeField,Min(0)] private int forestHarvestBaseDamage=20;
    [SerializeField,Min(0)] private int forestHarvestDamagePerVine=5;
    [SerializeField,Min(1)] private int forestRenewalChannelMoves=2;
    [SerializeField,Min(1)] private int forestHarvestChannelMoves=3;
    public int ForestRenewalBaseHeal=>forestRenewalBaseHeal;
    public int ForestHeartrootHealBonus=>forestHeartrootHealBonus;
    public int ForestHarvestBaseDamage=>forestHarvestBaseDamage;
    public int ForestHarvestDamagePerVine=>forestHarvestDamagePerVine;
    public int ForestRenewalChannelMoves=>forestRenewalChannelMoves;
    public int ForestHarvestChannelMoves=>forestHarvestChannelMoves;

    [Header("Spawn Rules")]

    [SerializeField, Min(1)]
    private int minimumWave = 1;

    [SerializeField, Min(0), Tooltip("Last eligible wave; zero keeps the enemy eligible indefinitely.")]
    private int maximumWave;
    [SerializeField, Tooltip("Weight multiplier by waves since Minimum Wave. Zero disables selection.")]
    private AnimationCurve progressionWeight = AnimationCurve.Linear(0, 1, 8, 1);
    [SerializeField] private bool crownSoldier;
    [SerializeField] private EnemyDefinition[] encounterEscorts = new EnemyDefinition[0];
    [SerializeField, Range(0, 2)] private int maximumSpecialEscorts = 1;
    public bool CrownSoldier => crownSoldier;
    public EnemyDefinition[] EncounterEscorts => encounterEscorts ?? System.Array.Empty<EnemyDefinition>();
    public int MaximumSpecialEscorts => Mathf.Clamp(maximumSpecialEscorts, 0, 2);
    public float GetSpawnWeight(int wave) => wave < minimumWave ||
        (maximumWave > 0 && wave > maximumWave) ? 0f :
        Mathf.Max(0f, spawnWeight * (progressionWeight == null ? 1f : progressionWeight.Evaluate(wave - minimumWave)));

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

    public bool UseAuthoredAutoAttackMotion =>
        timeAutoAttackFromAnimation && useAuthoredAutoAttackMotion;

    public bool TimeSpecialAbilityFromAnimation =>
        timeSpecialAbilityFromAnimation;

    public bool UseAuthoredSpecialAbilityMotion =>
        timeSpecialAbilityFromAnimation && useAuthoredSpecialAbilityMotion;

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

    public EnemyDefinition[] TownMarshalSummonCandidates =>
        townMarshalSummonCandidates;

    public int TownMarshalRetreatMoveCount =>
        townMarshalRetreatMoveCount;

    public float TownMarshalRallyAttackSpeedMultiplier =>
        townMarshalRallyAttackSpeedMultiplier;

    public float TownMarshalRallyDuration =>
        townMarshalRallyDuration;

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

        townMarshalRetreatMoveCount =
            Mathf.Max(
                1,
                townMarshalRetreatMoveCount
            );

        townMarshalRallyAttackSpeedMultiplier =
            Mathf.Max(
                1f,
                townMarshalRallyAttackSpeedMultiplier
            );

        townMarshalRallyDuration =
            Mathf.Max(
                0.1f,
                townMarshalRallyDuration
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
