using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class GemDamageContext
{
    public PlayerActor Player { get; }

    public BoardClearContext ClearContext
    {
        get;
    }

    public GemType GemType =>
        ClearContext.GemType;

    public int GemCount =>
        ClearContext.GemCount;

    public int CascadeDepth =>
        ClearContext.CascadeDepth;

    public BoardClearSource ClearSource =>
        ClearContext.Source;

    public BoardMatchType MatchType =>
        ClearContext.MatchType;

    public int OriginalDamage { get; }

    public int Damage { get; set; }

    public bool IsCancelled
    {
        get;
        private set;
    }

    public GemDamageContext(
        PlayerActor player,
        BoardClearContext clearContext,
        int damage)
    {
        Player =
            player;

        ClearContext =
            clearContext;

        OriginalDamage =
            Mathf.Max(
                0,
                damage
            );

        Damage =
            OriginalDamage;
    }

    public void Cancel()
    {
        IsCancelled = true;
        Damage = 0;
    }
}

[DisallowMultipleComponent]
public sealed class CombatController :
    MonoBehaviour
{
    [Header("Combat References")]
    [SerializeField]
    private PlayerActor playerActor;

    [SerializeField]
    private WaveController waveController;

    [Header("Per-Gem Damage")]
    [SerializeField, Min(0)]
    [Tooltip(
        "Base damage caused by each rewardable colored " +
        "gem cleared through any source."
    )]
    private int damagePerGem = 10;

    [SerializeField, Range(0f, 1f)]
    [Tooltip(
        "Extra damage for each cascade depth. " +
        "0.20 means twenty percent per depth."
    )]
    private float cascadeDamageBonusPerDepth =
        0.20f;

    [Header("Poison")]
    [SerializeField, Min(0.05f)]
    [Tooltip(
        "How long Poison Bomb poison remains active."
    )]
    private float poisonDuration = 7f;

    [SerializeField, Min(0.05f)]
    [Tooltip(
        "Seconds between poison damage ticks."
    )]
    private float poisonTickInterval = 1f;

    [SerializeField, Min(1)]
    [Tooltip(
        "Prototype damage dealt by each poison tick. " +
        "This is intentionally data-tunable for later balance passes."
    )]
    private int poisonTickDamage = 5;

    [Header("Prototype Debugging")]
    [SerializeField]
    private GemType debugGemType;

    [SerializeField, Min(1)]
    private int debugGemCount = 3;

    [SerializeField, Min(0)]
    private int debugCascadeDepth;

    public event Action<GemDamageContext>
        BeforeGemDamage;

    public event Action<
        GemDamageContext,
        int
    > GemDamageResolved;

    public event Action<
        EnemyActor,
        GemDamageContext,
        int
    > EnemyDamagedByGemClear;

    public PlayerActor PlayerActor =>
        playerActor;

    public WaveController WaveController =>
        waveController;

    public bool ResolveGemClear(
        BoardClearContext clearContext)
    {
        if (!CanResolveCombat() ||
            clearContext.GemCount <= 0)
        {
            return false;
        }

        int calculatedDamage =
            CalculateGemClearDamage(
                clearContext
            );

        if (calculatedDamage <= 0)
        {
            return false;
        }

        GemDamageContext damageContext =
            new GemDamageContext(
                playerActor,
                clearContext,
                calculatedDamage
            );

        /*
         * Cards, passives, buffs and debuffs can inspect:
         *
         * damageContext.ClearSource
         * damageContext.MatchType
         * damageContext.GemType
         * damageContext.GemCount
         * damageContext.CascadeDepth
         *
         * They may modify Damage or cancel the result.
         */
        BeforeGemDamage?.Invoke(
            damageContext
        );

        if (damageContext.IsCancelled)
        {
            return false;
        }

        damageContext.Damage =
            Mathf.Max(
                0,
                damageContext.Damage
            );

        if (damageContext.Damage == 0)
        {
            return false;
        }

        List<EnemyActor> enemySnapshot =
            new List<EnemyActor>(
                waveController.ActiveEnemies
            );

        int enemiesHit = 0;

        foreach (EnemyActor enemy
                 in enemySnapshot)
        {
            if (enemy == null ||
                enemy.IsDefeated ||
                !enemy.IsInitialized ||
                enemy.AssignedGemType !=
                    clearContext.GemType)
            {
                continue;
            }

            int healthBeforeDamage =
                enemy.CurrentHealth;

            bool damageApplied =
                enemy.TryTakeDamage(
                    damageContext.Damage
                );

            if (!damageApplied)
            {
                continue;
            }

            int actualDamage =
                Mathf.Max(
                    0,
                    healthBeforeDamage -
                    enemy.CurrentHealth
                );

            enemiesHit++;

            EnemyDamagedByGemClear?.Invoke(
                enemy,
                damageContext,
                actualDamage
            );

            Debug.Log(
                $"{clearContext.GemCount} " +
                $"{clearContext.GemType} gem(s) from " +
                $"{clearContext.Source} dealt " +
                $"{actualDamage} damage to " +
                $"{enemy.Definition.DisplayName}.",
                enemy
            );
        }

        GemDamageResolved?.Invoke(
            damageContext,
            enemiesHit
        );

        if (enemiesHit == 0)
        {
            Debug.Log(
                $"{clearContext.GemCount} " +
                $"{clearContext.GemType} gem(s) were " +
                $"cleared through {clearContext.Source}, " +
                "but no active enemy had that weakness.",
                this
            );
        }

        return enemiesHit > 0;
    }

    public int ApplyPoisonToGemType(
        GemType gemType)
    {
        if (!CanResolveCombat())
        {
            return 0;
        }

        List<EnemyActor> enemySnapshot =
            new List<EnemyActor>(
                waveController.ActiveEnemies
            );

        int enemiesPoisoned = 0;

        foreach (EnemyActor enemy
                 in enemySnapshot)
        {
            if (enemy == null ||
                !enemy.IsInitialized ||
                enemy.IsDefeated ||
                enemy.AssignedGemType !=
                    gemType)
            {
                continue;
            }

            EnemyPoisonStatus poisonStatus =
                enemy.GetComponent<
                    EnemyPoisonStatus
                >();

            if (poisonStatus == null)
            {
                poisonStatus =
                    enemy.gameObject.AddComponent<
                        EnemyPoisonStatus
                    >();
            }

            poisonStatus.Apply(
                poisonDuration,
                poisonTickInterval,
                poisonTickDamage
            );

            if (!poisonStatus.IsPoisoned)
            {
                continue;
            }

            enemiesPoisoned++;
        }

        if (enemiesPoisoned > 0)
        {
            Debug.Log(
                $"Poison applied to {enemiesPoisoned} " +
                $"active {gemType}-weak enemy/enemies for " +
                $"{poisonDuration:0.##}s.",
                this
            );
        }

        return enemiesPoisoned;
    }

    public int CalculateGemClearDamage(
        BoardClearContext clearContext)
    {
        int safeGemCount =
            Mathf.Max(
                0,
                clearContext.GemCount
            );

        int baseDamage =
            safeGemCount *
            damagePerGem;

        float cascadeMultiplier =
            1f +
            Mathf.Max(
                0,
                clearContext.CascadeDepth
            ) *
            cascadeDamageBonusPerDepth;

        return Mathf.Max(
            0,
            Mathf.RoundToInt(
                baseDamage *
                cascadeMultiplier
            )
        );
    }

    private bool CanResolveCombat()
    {
        if (playerActor == null)
        {
            Debug.LogError(
                "CombatController requires a PlayerActor.",
                this
            );

            return false;
        }

        if (waveController == null)
        {
            Debug.LogError(
                "CombatController requires a WaveController.",
                this
            );

            return false;
        }

        if (!playerActor.IsInitialized ||
            playerActor.IsDefeated)
        {
            return false;
        }

        return waveController.IsWaveActive;
    }

    private void OnValidate()
    {
        damagePerGem =
            Mathf.Max(
                0,
                damagePerGem
            );

        cascadeDamageBonusPerDepth =
            Mathf.Max(
                0f,
                cascadeDamageBonusPerDepth
            );

        poisonDuration =
            Mathf.Max(
                0.05f,
                poisonDuration
            );

        poisonTickInterval =
            Mathf.Max(
                0.05f,
                poisonTickInterval
            );

        poisonTickDamage =
            Mathf.Max(
                1,
                poisonTickDamage
            );

        debugGemCount =
            Mathf.Max(
                1,
                debugGemCount
            );

        debugCascadeDepth =
            Mathf.Max(
                0,
                debugCascadeDepth
            );
    }

    [ContextMenu(
        "Prototype/Resolve Debug Gem Clear"
    )]
    private void DebugResolveGemClear()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        BoardClearContext clearContext =
            new BoardClearContext(
                debugGemType,
                debugGemCount,
                debugCascadeDepth,
                BoardClearSource.Match,
                BoardMatchType.Other
            );

        ResolveGemClear(
            clearContext
        );
    }

    [ContextMenu(
        "Prototype/Match First Enemy Weakness"
    )]
    private void DebugMatchFirstEnemyWeakness()
    {
        if (!Application.isPlaying ||
            waveController == null)
        {
            return;
        }

        foreach (
            EnemyActor enemy
            in waveController.ActiveEnemies)
        {
            if (enemy == null ||
                enemy.IsDefeated)
            {
                continue;
            }

            BoardClearContext clearContext =
                new BoardClearContext(
                    enemy.AssignedGemType,
                    debugGemCount,
                    debugCascadeDepth,
                    BoardClearSource.Match,
                    BoardMatchType.Other
                );

            ResolveGemClear(
                clearContext
            );

            return;
        }
    }
}
