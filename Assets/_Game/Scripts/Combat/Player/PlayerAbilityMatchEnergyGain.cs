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

    [Header("Energy Gain")]
    [SerializeField, Min(0)]
    private int energyPerMatch = 10;

    [SerializeField, Min(0)]
    private int bonusEnergyPerCascadeDepth = 3;

    [SerializeField, Min(0)]
    private int affinityGemBonus = 2;

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

        boardController.BoardMatchResolved -=
            HandleBoardMatchResolved;

        boardController.BoardMatchResolved +=
            HandleBoardMatchResolved;
    }

    private void UnsubscribeFromBoard()
    {
        if (boardController == null)
        {
            return;
        }

        boardController.BoardMatchResolved -=
            HandleBoardMatchResolved;
    }

    private void HandleBoardMatchResolved(
        BoardMatchContext context)
    {
        if (playerActor == null ||
            playerAbilityEnergy == null ||
            playerAbilityController == null ||
            playerAbilityController.IsAbilityActive ||
            !playerActor.IsInitialized ||
            playerActor.IsDefeated ||
            playerActor.Definition == null)
        {
            return;
        }

        int gainedEnergy =
            CalculateEnergyGain(
                context,
                playerActor.Definition
                    .AffinityGemType
            );

        playerAbilityEnergy.AddEnergy(
            gainedEnergy
        );
    }

    public int CalculateEnergyGain(
        BoardMatchContext context,
        GemType affinityGemType)
    {
        int gainedEnergy =
            energyPerMatch;

        gainedEnergy +=
            context.CascadeDepth *
            bonusEnergyPerCascadeDepth;

        if (context.GemType ==
            affinityGemType)
        {
            gainedEnergy +=
                affinityGemBonus;
        }

        return Mathf.Max(
            0,
            gainedEnergy
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
        energyPerMatch =
            Mathf.Max(
                0,
                energyPerMatch
            );

        bonusEnergyPerCascadeDepth =
            Mathf.Max(
                0,
                bonusEnergyPerCascadeDepth
            );

        affinityGemBonus =
            Mathf.Max(
                0,
                affinityGemBonus
            );
    }
}