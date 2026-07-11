using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class GemDamageContext
{
    public PlayerActor Player { get; }

    public GemType GemType { get; }

    public int GemCount { get; }

    public int CascadeDepth { get; }

    public int OriginalDamage { get; }

    public int Damage { get; set; }

    public bool IsCancelled { get; private set; }

    public GemDamageContext(
        PlayerActor player,
        GemType gemType,
        int gemCount,
        int cascadeDepth,
        int damage)
    {
        Player = player;
        GemType = gemType;
        GemCount = Mathf.Max(0, gemCount);
        CascadeDepth = Mathf.Max(0, cascadeDepth);

        OriginalDamage = Mathf.Max(0, damage);
        Damage = OriginalDamage;
    }

    public void Cancel()
    {
        IsCancelled = true;
        Damage = 0;
    }
}

[DisallowMultipleComponent]
public sealed class CombatController : MonoBehaviour
{
    [Header("Combat References")]
    [SerializeField]
    private PlayerActor playerActor;

    [SerializeField]
    private WaveController waveController;

    [Header("Gem Match Damage")]
    [SerializeField, Min(1)]
    private int minimumMatchSize = 3;

    [SerializeField, Min(0)]
    [Tooltip("Damage caused by a normal three-gem match.")]
    private int threeGemMatchDamage = 30;

    [SerializeField, Min(0)]
    [Tooltip(
        "Additional damage for every gem beyond " +
        "the minimum match size."
    )]
    private int additionalDamagePerGem = 15;

    [SerializeField, Range(0f, 1f)]
    [Tooltip(
        "Extra damage for each cascade level. " +
        "0.20 means twenty percent per cascade."
    )]
    private float cascadeDamageBonusPerDepth = 0.20f;

    [Header("Prototype Debugging")]
    [SerializeField]
    private GemType debugGemType;

    [SerializeField, Min(3)]
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
    > EnemyDamagedByGemMatch;

    public PlayerActor PlayerActor =>
        playerActor;

    public WaveController WaveController =>
        waveController;

    public bool ResolveGemMatch(
        GemType gemType,
        int gemCount,
        int cascadeDepth = 0)
    {
        if (!CanResolveCombat())
        {
            return false;
        }

        if (gemCount < minimumMatchSize)
        {
            Debug.LogWarning(
                $"A match of {gemCount} gems is below the " +
                $"minimum size of {minimumMatchSize}.",
                this
            );

            return false;
        }

        int calculatedDamage =
            CalculateGemMatchDamage(
                gemCount,
                cascadeDepth
            );

        GemDamageContext context =
            new GemDamageContext(
                playerActor,
                gemType,
                gemCount,
                cascadeDepth,
                calculatedDamage
            );

        /*
         * Future passives, upgrades and temporary effects
         * can subscribe here and modify context.Damage.
         *
         * Example:
         * Skeleton passive could increase context.Damage.
         */
        BeforeGemDamage?.Invoke(context);

        if (context.IsCancelled)
        {
            return false;
        }

        context.Damage =
            Mathf.Max(0, context.Damage);

        if (context.Damage == 0)
        {
            return false;
        }

        List<EnemyActor> enemySnapshot =
            new List<EnemyActor>(
                waveController.ActiveEnemies
            );

        int enemiesHit = 0;

        foreach (EnemyActor enemy in enemySnapshot)
        {
            if (enemy == null ||
                enemy.IsDefeated ||
                !enemy.IsInitialized)
            {
                continue;
            }

            /*
             * Only the enemy whose weakness color matches
             * the cleared gem type receives damage.
             */
            if (enemy.AssignedGemType != gemType)
            {
                continue;
            }

            int previousHealth =
                enemy.CurrentHealth;

            bool damageApplied =
                enemy.TryTakeDamage(
                    context.Damage
                );

            if (!damageApplied)
            {
                continue;
            }

            int actualDamage =
                previousHealth -
                enemy.CurrentHealth;

            enemiesHit++;

            EnemyDamagedByGemMatch?.Invoke(
                enemy,
                context,
                actualDamage
            );

            Debug.Log(
                $"{gemCount} {gemType} gems dealt " +
                $"{actualDamage} damage to " +
                $"{enemy.Definition.DisplayName}.",
                enemy
            );
        }

        GemDamageResolved?.Invoke(
            context,
            enemiesHit
        );

        if (enemiesHit == 0)
        {
            Debug.Log(
                $"{gemCount} {gemType} gems were cleared, " +
                "but no active enemy had that weakness.",
                this
            );
        }

        return enemiesHit > 0;
    }

    public int CalculateGemMatchDamage(
        int gemCount,
        int cascadeDepth)
    {
        gemCount =
            Mathf.Max(
                minimumMatchSize,
                gemCount
            );

        cascadeDepth =
            Mathf.Max(
                0,
                cascadeDepth
            );

        int extraGemCount =
            gemCount - minimumMatchSize;

        int baseDamage =
            threeGemMatchDamage +
            extraGemCount *
            additionalDamagePerGem;

        float cascadeMultiplier =
            1f +
            cascadeDepth *
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

        if (!waveController.IsWaveActive)
        {
            return false;
        }

        return true;
    }

    private void OnValidate()
    {
        minimumMatchSize =
            Mathf.Max(
                1,
                minimumMatchSize
            );

        threeGemMatchDamage =
            Mathf.Max(
                0,
                threeGemMatchDamage
            );

        additionalDamagePerGem =
            Mathf.Max(
                0,
                additionalDamagePerGem
            );

        debugGemCount =
            Mathf.Max(
                minimumMatchSize,
                debugGemCount
            );

        debugCascadeDepth =
            Mathf.Max(
                0,
                debugCascadeDepth
            );
    }

    [ContextMenu("Prototype/Resolve Debug Gem Match")]
    private void DebugResolveGemMatch()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        ResolveGemMatch(
            debugGemType,
            debugGemCount,
            debugCascadeDepth
        );
    }

    [ContextMenu("Prototype/Match First Enemy Weakness")]
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

            ResolveGemMatch(
                enemy.AssignedGemType,
                debugGemCount,
                debugCascadeDepth
            );

            return;
        }
    }
}