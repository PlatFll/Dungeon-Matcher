using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class StaggerCombatBridge :
    MonoBehaviour
{
    [Header("Combat Reference")]
    [SerializeField]
    private CombatController combatController;

    [Header("Stagger Durations")]
    [SerializeField, Min(0f)]
    private float initialStaggerDuration =
        1f;

    [SerializeField, Min(0f)]
    private float additionalStaggerDuration =
        0.75f;

    [SerializeField, Min(0f)]
    private float cascadeStaggerBonusPerDepth =
        0.25f;

    [SerializeField, Min(0f)]
    private float maximumStoredStaggerDuration =
        3f;

    private readonly
        List<IBoardClearDrivenEnemyHitSource>
        boardClearDrivenHitSources =
            new List<
                IBoardClearDrivenEnemyHitSource
            >();

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
            combatController
                .EnemyDamagedByGemClear -=
                    HandleEnemyDamagedByClear;

            combatController
                .EnemyDamagedByGemClear +=
                    HandleEnemyDamagedByClear;
        }

        SubscribeToBoardClearDrivenHitSources();
    }

    private void Unsubscribe()
    {
        if (combatController != null)
        {
            combatController
                .EnemyDamagedByGemClear -=
                    HandleEnemyDamagedByClear;
        }

        UnsubscribeFromBoardClearDrivenHitSources();
    }

    private void
        SubscribeToBoardClearDrivenHitSources()
    {
        UnsubscribeFromBoardClearDrivenHitSources();

        MonoBehaviour[] components =
            GetComponentsInChildren<
                MonoBehaviour
            >(true);

        foreach (MonoBehaviour component
                 in components)
        {
            if (!(component is
                    IBoardClearDrivenEnemyHitSource
                        hitSource))
            {
                continue;
            }

            hitSource.HitResolved -=
                HandleBoardClearDrivenAbilityHit;

            hitSource.HitResolved +=
                HandleBoardClearDrivenAbilityHit;

            boardClearDrivenHitSources.Add(
                hitSource
            );
        }
    }

    private void
        UnsubscribeFromBoardClearDrivenHitSources()
    {
        foreach (
            IBoardClearDrivenEnemyHitSource
                hitSource
            in boardClearDrivenHitSources)
        {
            hitSource.HitResolved -=
                HandleBoardClearDrivenAbilityHit;
        }

        boardClearDrivenHitSources.Clear();
    }

    private void HandleEnemyDamagedByClear(
        EnemyActor enemy,
        GemDamageContext damageContext,
        int actualDamage)
    {
        int cascadeDepth =
            damageContext != null
                ? damageContext.CascadeDepth
                : 0;

        ApplyClearDrivenStagger(
            enemy,
            actualDamage,
            cascadeDepth
        );
    }

    private void
        HandleBoardClearDrivenAbilityHit(
            EnemyActor enemy,
            int actualDamage,
            BoardClearContext clearContext)
    {
        ApplyClearDrivenStagger(
            enemy,
            actualDamage,
            clearContext.CascadeDepth
        );
    }

    private void ApplyClearDrivenStagger(
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
            enemy.GetComponent<
                EnemyStagger
            >();

        if (stagger == null)
        {
            Debug.LogWarning(
                $"{enemy.name} received clear-driven " +
                "damage but has no EnemyStagger component.",
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
            $"Stored: " +
            $"{stagger.RemainingStaggerTime:0.00}s.",
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
            "StaggerCombatBridge requires a " +
            "CombatController.",
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