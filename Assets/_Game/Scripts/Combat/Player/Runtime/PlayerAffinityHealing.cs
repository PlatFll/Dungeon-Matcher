using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerAffinityHealing :
    MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private BoardController boardController;

    [SerializeField]
    private PlayerActor playerActor;

    [Header("Per-Gem Healing")]
    [SerializeField, Min(0)]
    [Tooltip(
        "Base HP restored for every affinity-color gem " +
        "that is genuinely destroyed."
    )]
    private int healingPerGem = 3;

    [SerializeField, Range(0f, 1f)]
    [Tooltip(
        "Additional healing for each cascade depth. " +
        "0.15 means fifteen percent per cascade."
    )]
    private float cascadeHealingBonusPerDepth =
        0.15f;

    /*
     * Future passives, VFX and cards can inspect exactly
     * what kind of clear caused the healing.
     */
    public event Action<
        BoardClearContext,
        int
    > AffinityClearHealingResolved;

    private void OnEnable()
    {
        SubscribeToBoard();
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
        UnsubscribeFromBoard();
    }

    private void SubscribeToBoard()
    {
        if (boardController == null)
        {
            return;
        }

        boardController.BoardClearResolved -=
            HandleBoardClearResolved;

        boardController.BoardClearResolved +=
            HandleBoardClearResolved;
    }

    private void UnsubscribeFromBoard()
    {
        if (boardController == null)
        {
            return;
        }

        boardController.BoardClearResolved -=
            HandleBoardClearResolved;
    }

    private void HandleBoardClearResolved(
        BoardClearContext context)
    {
        if (playerActor == null ||
            !playerActor.IsInitialized ||
            playerActor.IsDefeated ||
            playerActor.Definition == null ||
            context.GemCount <= 0)
        {
            return;
        }

        GemType affinityGemType =
            playerActor.Definition
                .AffinityGemType;

        if (context.GemType !=
            affinityGemType)
        {
            return;
        }

        int attemptedHealing =
            CalculateHealing(
                context
            );

        if (attemptedHealing <= 0)
        {
            return;
        }

        int actualHealing =
            playerActor.Heal(
                attemptedHealing
            );

        AffinityClearHealingResolved?.Invoke(
            context,
            actualHealing
        );

        if (actualHealing > 0)
        {
            Debug.Log(
                $"{playerActor.Definition.DisplayName} " +
                $"healed {actualHealing} HP from " +
                $"{context.GemCount} " +
                $"{context.GemType} gem(s) cleared by " +
                $"{context.Source}. " +
                $"Cascade depth: " +
                $"{context.CascadeDepth}.",
                playerActor
            );
        }
    }

    public int CalculateHealing(
        BoardClearContext context)
    {
        int safeGemCount =
            Mathf.Max(
                0,
                context.GemCount
            );

        if (safeGemCount == 0)
        {
            return 0;
        }

        int baseHealing =
            safeGemCount *
            healingPerGem;

        float cascadeMultiplier =
            1f +
            Mathf.Max(
                0,
                context.CascadeDepth
            ) *
            cascadeHealingBonusPerDepth;

        return Mathf.Max(
            0,
            Mathf.RoundToInt(
                baseHealing *
                cascadeMultiplier
            )
        );
    }

    private bool ValidateReferences()
    {
        bool isValid = true;

        if (boardController == null)
        {
            Debug.LogError(
                "PlayerAffinityHealing requires a " +
                "BoardController.",
                this
            );

            isValid = false;
        }

        if (playerActor == null)
        {
            Debug.LogError(
                "PlayerAffinityHealing requires a " +
                "PlayerActor.",
                this
            );

            isValid = false;
        }

        return isValid;
    }

    private void OnValidate()
    {
        healingPerGem =
            Mathf.Max(
                0,
                healingPerGem
            );

        cascadeHealingBonusPerDepth =
            Mathf.Max(
                0f,
                cascadeHealingBonusPerDepth
            );
    }
}
