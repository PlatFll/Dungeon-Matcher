using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerAbilityController))]
public sealed class PlayerAbilityMatchEnergyGain :
    MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private BoardController boardController;

    [SerializeField]
    private PlayerActor playerActor;

    [SerializeField]
    private PlayerAbilityEnergy playerAbilityEnergy;

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

        boardController.BoardMatchOutcomeResolved -=
            HandleBoardMatchOutcomeResolved;

        boardController.BoardMatchOutcomeResolved +=
            HandleBoardMatchOutcomeResolved;
    }

    private void UnsubscribeFromBoard()
    {
        if (boardController == null)
        {
            return;
        }

        boardController.BoardMatchOutcomeResolved -=
            HandleBoardMatchOutcomeResolved;
    }

    private void HandleBoardMatchOutcomeResolved(
        BoardMatchOutcome outcome)
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
            CalculateEnergyGain(outcome);

        playerAbilityEnergy.AddEnergy(
            gainedEnergy
        );
    }

    public int CalculateEnergyGain(
        BoardMatchOutcome outcome)
    {
        bool damagedMatchingEnemy =
            outcome.DamagedMatchingEnemy;

        switch (outcome.MatchContext.MatchType)
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
                GetComponent<PlayerAbilityEnergy>();
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
            Mathf.Max(0, damagingThreeEnergy);

        nonDamagingThreeEnergy =
            Mathf.Max(0, nonDamagingThreeEnergy);

        damagingFourEnergy =
            Mathf.Max(0, damagingFourEnergy);

        nonDamagingFourEnergy =
            Mathf.Max(0, nonDamagingFourEnergy);

        damagingFiveEnergy =
            Mathf.Max(0, damagingFiveEnergy);

        nonDamagingFiveEnergy =
            Mathf.Max(0, nonDamagingFiveEnergy);

        damagingLShapeEnergy =
            Mathf.Max(0, damagingLShapeEnergy);

        nonDamagingLShapeEnergy =
            Mathf.Max(0, nonDamagingLShapeEnergy);

        damagingTShapeEnergy =
            Mathf.Max(0, damagingTShapeEnergy);

        nonDamagingTShapeEnergy =
            Mathf.Max(0, nonDamagingTShapeEnergy);

        damagingOtherEnergy =
            Mathf.Max(0, damagingOtherEnergy);

        nonDamagingOtherEnergy =
            Mathf.Max(0, nonDamagingOtherEnergy);
    }
}