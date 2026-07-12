using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerAffinityHealing : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private BoardController boardController;

    [SerializeField]
    private PlayerActor playerActor;

    [Header("Healing Values")]
    [SerializeField, Min(0)]
    private int threeGemHealing = 8;

    [SerializeField, Min(0)]
    private int fourGemHealing = 12;

    [SerializeField, Min(0)]
    private int fiveGemHealing = 18;

    [SerializeField, Min(0)]
    [Tooltip(
        "Healing added for every gem beyond five."
    )]
    private int additionalHealingPerGem = 6;

    [SerializeField, Range(0f, 1f)]
    [Tooltip(
        "Additional healing for each cascade depth. " +
        "0.15 means fifteen percent per cascade."
    )]
    private float cascadeHealingBonusPerDepth =
        0.15f;

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

        boardController.MatchResolved -=
            HandleMatchResolved;

        boardController.MatchResolved +=
            HandleMatchResolved;
    }

    private void UnsubscribeFromBoard()
    {
        if (boardController == null)
        {
            return;
        }

        boardController.MatchResolved -=
            HandleMatchResolved;
    }

    private void HandleMatchResolved(
        GemType gemType,
        int gemCount,
        int cascadeDepth)
    {
        if (playerActor == null ||
            !playerActor.IsInitialized ||
            playerActor.IsDefeated ||
            playerActor.Definition == null)
        {
            return;
        }

        GemType affinityGemType =
            playerActor.Definition.AffinityGemType;

        if (gemType != affinityGemType)
        {
            return;
        }

        int attemptedHealing =
            CalculateHealing(
                gemCount,
                cascadeDepth
            );

        int actualHealing =
            playerActor.Heal(
                attemptedHealing
            );

        AffinityHealingResolved?.Invoke(
            gemType,
            gemCount,
            cascadeDepth,
            actualHealing
        );

        if (actualHealing > 0)
        {
            Debug.Log(
                $"{playerActor.Definition.DisplayName} " +
                $"healed {actualHealing} HP from " +
                $"{gemCount} {gemType} gems. " +
                $"Cascade depth: {cascadeDepth}.",
                playerActor
            );
        }
        else
        {
            Debug.Log(
                $"{gemCount} {gemType} gems matched the " +
                "player affinity, but the player was " +
                "already at full health.",
                playerActor
            );
        }
    }

    public int CalculateHealing(
        int gemCount,
        int cascadeDepth)
    {
        gemCount =
            Mathf.Max(3, gemCount);

        cascadeDepth =
            Mathf.Max(0, cascadeDepth);

        int baseHealing;

        if (gemCount == 3)
        {
            baseHealing =
                threeGemHealing;
        }
        else if (gemCount == 4)
        {
            baseHealing =
                fourGemHealing;
        }
        else
        {
            baseHealing =
                fiveGemHealing +
                Mathf.Max(0, gemCount - 5) *
                additionalHealingPerGem;
        }

        float cascadeMultiplier =
            1f +
            cascadeDepth *
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
        threeGemHealing =
            Mathf.Max(0, threeGemHealing);

        fourGemHealing =
            Mathf.Max(0, fourGemHealing);

        fiveGemHealing =
            Mathf.Max(0, fiveGemHealing);

        additionalHealingPerGem =
            Mathf.Max(
                0,
                additionalHealingPerGem
            );
    }
}