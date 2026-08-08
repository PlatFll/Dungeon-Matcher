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
     * New scalable event.
     *
     * Future passives, VFX and cards can inspect exactly
     * what kind of clear caused the healing.
     */
    public event Action<
        BoardClearContext,
        int
    > AffinityClearHealingResolved;

    /*
     * Legacy compatibility event.
     *
     * Keep this for now in case an existing UI or effect
     * listens to the old signature.
     */
    public event Action<
        GemType,
        int,
        int,
        int
    > AffinityHealingResolved;

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

        /*
         * New context-rich event.
         */
        AffinityClearHealingResolved?.Invoke(
            context,
            actualHealing
        );

        /*
         * Old event remains temporarily compatible.
         */
        AffinityHealingResolved?.Invoke(
            context.GemType,
            context.GemCount,
            context.CascadeDepth,
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

    /*
     * Compatibility overload for any old caller that
     * directly calculates healing.
     */
    public int CalculateHealing(
        int gemCount,
        int cascadeDepth)
    {
        BoardClearContext context =
            new BoardClearContext(
                default(GemType),
                gemCount,
                cascadeDepth,
                BoardClearSource.Match,
                BoardMatchType.Other
            );

        return CalculateHealing(
            context
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