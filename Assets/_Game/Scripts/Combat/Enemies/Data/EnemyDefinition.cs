using UnityEngine;

[CreateAssetMenu(
    fileName = "Enemy_",
    menuName = "Dungeon Matcher/Enemies/Enemy Definition"
)]
public sealed class EnemyDefinition : ScriptableObject
{
    [Header("Identity")]

    [SerializeField]
    [Tooltip("Stable internal identifier. Avoid changing it after release.")]
    private string enemyId = "enemy_id";

    [SerializeField]
    private string displayName = "Enemy";

    [SerializeField]
    private EnemyCategory category = EnemyCategory.Normal;

    [SerializeField]
    [TextArea(2, 4)]
    private string description;

    [Header("Prefab")]

    [SerializeField]
    [Tooltip("Prefab spawned by the wave controller.")]
    private GameObject enemyPrefab;

    [Header("Base Combat Stats")]

    [SerializeField, Min(1)]
    private int baseMaxHealth = 100;

    [SerializeField, Min(0)]
    private int baseDamage = 10;

    [SerializeField, Min(0.1f)]
    [Tooltip("Seconds between automatic attacks.")]
    private float baseAttackInterval = 3f;

    [Header("Special Ability")]

    [SerializeField]
    private bool hasSpecialAbility;

    [SerializeField, Min(1)]
    [Tooltip(
        "Number of valid player turns required before " +
        "the enemy uses its special ability."
    )]
    private int baseSpecialTurnRequirement = 5;

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

    public string EnemyId => enemyId;
    public string DisplayName => displayName;
    public EnemyCategory Category => category;
    public string Description => description;
    public GameObject EnemyPrefab => enemyPrefab;

    public int BaseMaxHealth => baseMaxHealth;
    public int BaseDamage => baseDamage;
    public float BaseAttackInterval => baseAttackInterval;

    public bool HasSpecialAbility => hasSpecialAbility;

    public int BaseSpecialTurnRequirement =>
        baseSpecialTurnRequirement;

    public int MinimumWave => minimumWave;
    public float SpawnWeight => spawnWeight;

    public float HealthMultiplier => healthMultiplier;
    public float DamageMultiplier => damageMultiplier;

    public float AttackSpeedMultiplier =>
        attackSpeedMultiplier;

    private void OnValidate()
    {
        enemyId = enemyId.Trim().ToLowerInvariant();
        displayName = displayName.Trim();

        baseMaxHealth = Mathf.Max(1, baseMaxHealth);
        baseDamage = Mathf.Max(0, baseDamage);
        baseAttackInterval = Mathf.Max(0.1f, baseAttackInterval);

        baseSpecialTurnRequirement =
            Mathf.Max(1, baseSpecialTurnRequirement);

        minimumWave = Mathf.Max(1, minimumWave);
        spawnWeight = Mathf.Max(0.01f, spawnWeight);

        healthMultiplier = Mathf.Max(0.1f, healthMultiplier);
        damageMultiplier = Mathf.Max(0.1f, damageMultiplier);

        attackSpeedMultiplier =
            Mathf.Max(0.1f, attackSpeedMultiplier);
    }
}