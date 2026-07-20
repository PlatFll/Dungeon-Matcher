using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class StaggerCombatBridge : MonoBehaviour
{
    [Header("Combat Reference")]
    [SerializeField]
    private CombatController combatController;

    [Header("Stagger Durations")]
    [SerializeField, Min(0f)]
    [Tooltip(
        "Stagger applied when an enemy was not already staggered."
    )]
    private float initialStaggerDuration = 1f;

    [SerializeField, Min(0f)]
    [Tooltip(
        "Extra stagger added when an already-staggered " +
        "enemy receives another damaging hit."
    )]
    private float additionalStaggerDuration = 0.75f;

    [SerializeField, Min(0f)]
    [Tooltip(
        "Additional stagger for each cascade depth."
    )]
    private float cascadeStaggerBonusPerDepth = 0.25f;

    [SerializeField, Min(0f)]
    [Tooltip(
        "Maximum stagger time that can be stored at once."
    )]
    private float maximumStoredStaggerDuration = 3f;

    private readonly List<IMatchDrivenEnemyHitSource>
        matchDrivenHitSources =
            new List<IMatchDrivenEnemyHitSource>();

    public event Action<
        EnemyActor,
        float,
        float
    > EnemyStaggered;

    private void OnEnable()
    {
        Subscribe();
    }

    private void Start()
    {
        if (!ValidateReferences())
        {
            enabled = false;
        }
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Subscribe()
    {
        if (combatController != null)
        {
            combatController.EnemyDamagedByGemMatch -=
                HandleEnemyDamaged;

            combatController.EnemyDamagedByGemMatch +=
                HandleEnemyDamaged;
        }

        SubscribeToMatchDrivenHitSources();
    }

    private void Unsubscribe()
    {
        if (combatController != null)
        {
            combatController.EnemyDamagedByGemMatch -=
                HandleEnemyDamaged;
        }

        UnsubscribeFromMatchDrivenHitSources();
    }

    private void SubscribeToMatchDrivenHitSources()
    {
        UnsubscribeFromMatchDrivenHitSources();

        MonoBehaviour[] components =
            GetComponentsInChildren<MonoBehaviour>(
                true
            );

        foreach (MonoBehaviour component in components)
        {
            if (!(component is
                    IMatchDrivenEnemyHitSource hitSource))
            {
                continue;
            }

            hitSource.HitResolved -=
                HandleMatchDrivenAbilityHit;

            hitSource.HitResolved +=
                HandleMatchDrivenAbilityHit;

            matchDrivenHitSources.Add(
                hitSource
            );
        }
    }

    private void UnsubscribeFromMatchDrivenHitSources()
    {
        foreach (
            IMatchDrivenEnemyHitSource hitSource
            in matchDrivenHitSources)
        {
            hitSource.HitResolved -=
                HandleMatchDrivenAbilityHit;
        }

        matchDrivenHitSources.Clear();
    }

    private void HandleEnemyDamaged(
        EnemyActor enemy,
        GemDamageContext damageContext,
        int actualDamage)
    {
        int cascadeDepth =
            damageContext != null
                ? damageContext.CascadeDepth
                : 0;

        ApplyMatchStagger(
            enemy,
            actualDamage,
            cascadeDepth
        );
    }

    private void HandleMatchDrivenAbilityHit(
        EnemyActor enemy,
        int actualDamage,
        BoardMatchContext matchContext)
    {
        ApplyMatchStagger(
            enemy,
            actualDamage,
            matchContext.CascadeDepth
        );
    }

    private void ApplyMatchStagger(
        EnemyActor enemy,
        int actualDamage,
        int cascadeDepth)
    {
        if (enemy == null ||
            enemy.IsDefeated ||
            actualDamage <= 0)
        {
            return;
        }

        EnemyStagger stagger =
            enemy.GetComponent<EnemyStagger>();

        if (stagger == null)
        {
            Debug.LogWarning(
                $"{enemy.name} received match-driven damage " +
                "but has no EnemyStagger component.",
                enemy
            );

            return;
        }

        float durationToAdd =
            stagger.IsStaggered
                ? additionalStaggerDuration
                : initialStaggerDuration;

        durationToAdd +=
            Mathf.Max(
                0,
                cascadeDepth
            ) *
            cascadeStaggerBonusPerDepth;

        float actualAddedDuration =
            stagger.ApplyStagger(
                durationToAdd,
                maximumStoredStaggerDuration
            );

        if (actualAddedDuration <= 0f)
        {
            return;
        }

        EnemyStaggered?.Invoke(
            enemy,
            actualAddedDuration,
            stagger.RemainingStaggerTime
        );

        Debug.Log(
            $"{enemy.Definition.DisplayName} was staggered. " +
            $"Added: {actualAddedDuration:0.00}s. " +
            $"Stored: {stagger.RemainingStaggerTime:0.00}s.",
            enemy
        );
    }

    private bool ValidateReferences()
    {
        if (combatController != null)
        {
            return true;
        }

        Debug.LogError(
            "StaggerCombatBridge requires a CombatController.",
            this
        );

        return false;
    }

    private void OnValidate()
    {
        initialStaggerDuration =
            Mathf.Max(
                0f,
                initialStaggerDuration
            );

        additionalStaggerDuration =
            Mathf.Max(
                0f,
                additionalStaggerDuration
            );

        cascadeStaggerBonusPerDepth =
            Mathf.Max(
                0f,
                cascadeStaggerBonusPerDepth
            );

        maximumStoredStaggerDuration =
            Mathf.Max(
                0f,
                maximumStoredStaggerDuration
            );
    }
}