using UnityEngine;
using UnityEngine.Serialization;

[DisallowMultipleComponent]
[RequireComponent(
    typeof(PlayerAbilityController)
)]
public sealed class PlayerAbilityMatchEnergyGain :
    MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private BoardController boardController;

    [SerializeField]
    private PlayerActor playerActor;

    [SerializeField]
    private PlayerAbilityEnergy
        playerAbilityEnergy;

    [SerializeField]
    private PlayerAbilityController
        playerAbilityController;

    [Header("Normal Three Energy")]
    [SerializeField, Min(0)]
    private int damagingThreeEnergy = 5;

    [SerializeField, Min(0)]
    private int nonDamagingThreeEnergy = 1;

    [Header("Straight Four Energy")]
    [SerializeField, Min(0)]
    private int damagingFourEnergy = 7;

    [SerializeField, Min(0)]
    private int nonDamagingFourEnergy = 3;

    [Header("Straight Five Energy")]
    [SerializeField, Min(0)]
    private int damagingFiveEnergy = 10;

    [SerializeField, Min(0)]
    private int nonDamagingFiveEnergy = 5;

    [Header("L Shape Energy")]
    [SerializeField, Min(0)]
    private int damagingLShapeEnergy = 10;

    [SerializeField, Min(0)]
    private int nonDamagingLShapeEnergy = 5;

    [Header("T Shape Energy")]
    [SerializeField, Min(0)]
    private int damagingTShapeEnergy = 10;

    [SerializeField, Min(0)]
    private int nonDamagingTShapeEnergy = 5;

    [Header("Unclassified Match Energy")]
    [SerializeField, Min(0)]
    private int damagingOtherEnergy = 5;

    [SerializeField, Min(0)]
    private int nonDamagingOtherEnergy = 1;

    [Header("Special Clear Energy")]

    /*
     * FormerlySerializedAs preserves the existing Inspector
     * values currently stored under the bomb-only names.
     */
    [FormerlySerializedAs(
        "damagingBombEnergyPerGem"
    )]
    [SerializeField, Min(0)]
    [Tooltip(
        "Energy per special-cleared gem when that " +
        "gem damages a matching-color enemy."
    )]
    private int damagingSpecialEnergyPerGem = 2;

    [FormerlySerializedAs(
        "nonDamagingBombEnergyPerGem"
    )]
    [SerializeField, Min(0)]
    [Tooltip(
        "Energy per special-cleared gem when no " +
        "matching-color enemy is damaged."
    )]
    private int nonDamagingSpecialEnergyPerGem = 1;

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnEnable()
    {
        ResolveReferences();
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

        /*
         * One unified subscription now handles matches,
         * bombs and every crystal clear.
         */
        boardController.BoardClearOutcomeResolved -=
            HandleBoardClearOutcomeResolved;

        boardController.BoardClearOutcomeResolved +=
            HandleBoardClearOutcomeResolved;
    }

    private void UnsubscribeFromBoard()
    {
        if (boardController == null)
        {
            return;
        }

        boardController.BoardClearOutcomeResolved -=
            HandleBoardClearOutcomeResolved;
    }

    private void HandleBoardClearOutcomeResolved(
        BoardClearOutcome outcome)
    {
        if (playerActor == null ||
            playerAbilityEnergy == null ||
            playerAbilityController == null ||
            playerAbilityController.IsAbilityActive ||
            !playerActor.IsInitialized ||
            playerActor.IsDefeated)
        {
            return;
        }

        int gainedEnergy =
            CalculateEnergyGain(
                outcome
            );

        if (gainedEnergy <= 0)
        {
            return;
        }

        playerAbilityEnergy.AddEnergy(
            gainedEnergy
        );
    }

    public int CalculateEnergyGain(
        BoardClearOutcome outcome)
    {
        BoardClearContext context =
            outcome.ClearContext;

        if (context.GemCount <= 0)
        {
            return 0;
        }

        switch (context.Source)
        {
            /*
             * Ordinary matches retain the intentional
             * shape-based energy design.
             */
            case BoardClearSource.Match:
                return CalculateMatchEnergy(
                    context.MatchType,
                    outcome.DamagedMatchingEnemy
                );

            /*
             * Every gem destroyed by board specials uses
             * the same per-gem rule.
             */
            case BoardClearSource.Bomb:
            case BoardClearSource.ColorCrystal:
            case BoardClearSource.DoubleColorCrystal:
                return CalculateSpecialClearEnergy(
                    context.GemCount,
                    outcome.DamagedMatchingEnemy
                );

            /*
             * Ability-generated clears currently grant no
             * energy by default. This prevents future
             * abilities from refunding themselves or
             * creating infinite energy loops.
             */
            case BoardClearSource.Ability:
                return 0;

            default:
                return 0;
        }
    }

    private int CalculateMatchEnergy(
        BoardMatchType matchType,
        bool damagedMatchingEnemy)
    {
        switch (matchType)
        {
            case BoardMatchType.NormalThree:
                return damagedMatchingEnemy
                    ? damagingThreeEnergy
                    : nonDamagingThreeEnergy;

            case BoardMatchType.StraightFour:
                return damagedMatchingEnemy
                    ? damagingFourEnergy
                    : nonDamagingFourEnergy;

            case BoardMatchType.StraightFive:
                return damagedMatchingEnemy
                    ? damagingFiveEnergy
                    : nonDamagingFiveEnergy;

            case BoardMatchType.LShape:
                return damagedMatchingEnemy
                    ? damagingLShapeEnergy
                    : nonDamagingLShapeEnergy;

            case BoardMatchType.TShape:
                return damagedMatchingEnemy
                    ? damagingTShapeEnergy
                    : nonDamagingTShapeEnergy;

            default:
                return damagedMatchingEnemy
                    ? damagingOtherEnergy
                    : nonDamagingOtherEnergy;
        }
    }

    private int CalculateSpecialClearEnergy(
        int gemCount,
        bool damagedMatchingEnemy)
    {
        int energyPerGem =
            damagedMatchingEnemy
                ? damagingSpecialEnergyPerGem
                : nonDamagingSpecialEnergyPerGem;

        return Mathf.Max(
            0,
            Mathf.Max(
                0,
                gemCount
            ) *
            energyPerGem
        );
    }

    /*
     * Compatibility method for any old code that directly
     * calls CalculateEnergyGain with BoardMatchOutcome.
     */
    public int CalculateEnergyGain(
        BoardMatchOutcome outcome)
    {
        BoardMatchContext oldContext =
            outcome.MatchContext;

        BoardClearContext clearContext =
            new BoardClearContext(
                oldContext.GemType,
                oldContext.GemCount,
                oldContext.CascadeDepth,
                BoardClearSource.Match,
                oldContext.MatchType
            );

        return CalculateEnergyGain(
            new BoardClearOutcome(
                clearContext,
                outcome.DamagedMatchingEnemy
            )
        );
    }

    private void ResolveReferences()
    {
        if (playerActor == null)
        {
            playerActor =
                GetComponent<PlayerActor>();
        }

        if (playerAbilityEnergy == null)
        {
            playerAbilityEnergy =
                GetComponent<
                    PlayerAbilityEnergy
                >();
        }

        if (playerAbilityController == null)
        {
            playerAbilityController =
                GetComponent<
                    PlayerAbilityController
                >();
        }
    }

    private bool ValidateReferences()
    {
        bool isValid = true;

        if (boardController == null)
        {
            Debug.LogError(
                "PlayerAbilityMatchEnergyGain " +
                "requires a BoardController.",
                this
            );

            isValid = false;
        }

        if (playerActor == null)
        {
            Debug.LogError(
                "PlayerAbilityMatchEnergyGain " +
                "requires a PlayerActor.",
                this
            );

            isValid = false;
        }

        if (playerAbilityEnergy == null)
        {
            Debug.LogError(
                "PlayerAbilityMatchEnergyGain " +
                "requires PlayerAbilityEnergy.",
                this
            );

            isValid = false;
        }

        if (playerAbilityController == null)
        {
            Debug.LogError(
                "PlayerAbilityMatchEnergyGain " +
                "requires a PlayerAbilityController.",
                this
            );

            isValid = false;
        }

        return isValid;
    }

    private void OnValidate()
    {
        damagingThreeEnergy =
            Mathf.Max(
                0,
                damagingThreeEnergy
            );

        nonDamagingThreeEnergy =
            Mathf.Max(
                0,
                nonDamagingThreeEnergy
            );

        damagingFourEnergy =
            Mathf.Max(
                0,
                damagingFourEnergy
            );

        nonDamagingFourEnergy =
            Mathf.Max(
                0,
                nonDamagingFourEnergy
            );

        damagingFiveEnergy =
            Mathf.Max(
                0,
                damagingFiveEnergy
            );

        nonDamagingFiveEnergy =
            Mathf.Max(
                0,
                nonDamagingFiveEnergy
            );

        damagingLShapeEnergy =
            Mathf.Max(
                0,
                damagingLShapeEnergy
            );

        nonDamagingLShapeEnergy =
            Mathf.Max(
                0,
                nonDamagingLShapeEnergy
            );

        damagingTShapeEnergy =
            Mathf.Max(
                0,
                damagingTShapeEnergy
            );

        nonDamagingTShapeEnergy =
            Mathf.Max(
                0,
                nonDamagingTShapeEnergy
            );

        damagingOtherEnergy =
            Mathf.Max(
                0,
                damagingOtherEnergy
            );

        nonDamagingOtherEnergy =
            Mathf.Max(
                0,
                nonDamagingOtherEnergy
            );

        damagingSpecialEnergyPerGem =
            Mathf.Max(
                0,
                damagingSpecialEnergyPerGem
            );

        nonDamagingSpecialEnergyPerGem =
            Mathf.Max(
                0,
                nonDamagingSpecialEnergyPerGem
            );
    }
}