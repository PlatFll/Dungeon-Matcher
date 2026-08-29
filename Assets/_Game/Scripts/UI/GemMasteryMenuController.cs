using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class GemMasteryMenuController : MonoBehaviour
{
    private static readonly Color NormalButtonColor =
        new Color32(91, 46, 105, 255);

    private static readonly Color SelectedButtonColor =
        new Color32(151, 91, 170, 255);

    private static readonly Color LockedButtonColor =
        new Color32(72, 57, 82, 255);

    [Header("Shape Selection")]
    [SerializeField]
    private Button[] shapeButtons;

    [Header("Reward Selection")]
    [SerializeField]
    private Button[] rewardButtons;

    [Header("Status")]
    [SerializeField]
    private Text selectionText;

    private GemMasteryShape selectedShape =
        GemMasteryShape.StraightFive;

    private bool isInitialized;

    private void Awake()
    {
        if (!HasRequiredReferences())
        {
            enabled = false;
            return;
        }

        AddButtonListeners();
        isInitialized = true;
        Refresh();
    }

    private void OnEnable()
    {
        GemMasterySettings.Changed +=
            HandleMasteryChanged;

        if (isInitialized)
        {
            Refresh();
        }
    }

    private void OnDisable()
    {
        GemMasterySettings.Changed -=
            HandleMasteryChanged;
    }

    private void OnDestroy()
    {
        if (!isInitialized)
        {
            return;
        }

        RemoveButtonListeners();
    }

    private void AddButtonListeners()
    {
        shapeButtons[0].onClick.AddListener(
            SelectStraightFive
        );

        shapeButtons[1].onClick.AddListener(
            SelectLShape
        );

        shapeButtons[2].onClick.AddListener(
            SelectTShape
        );

        shapeButtons[3].onClick.AddListener(
            SelectCrossShape
        );

        rewardButtons[0].onClick.AddListener(
            SelectColorCrystal
        );

        rewardButtons[1].onClick.AddListener(
            SelectPoisonBomb
        );

        rewardButtons[2].onClick.AddListener(
            SelectShieldBomb
        );

        rewardButtons[3].onClick.AddListener(
            SelectHealBomb
        );

        rewardButtons[4].onClick.AddListener(
            SelectDamageBomb
        );
    }

    private void RemoveButtonListeners()
    {
        shapeButtons[0].onClick.RemoveListener(
            SelectStraightFive
        );

        shapeButtons[1].onClick.RemoveListener(
            SelectLShape
        );

        shapeButtons[2].onClick.RemoveListener(
            SelectTShape
        );

        shapeButtons[3].onClick.RemoveListener(
            SelectCrossShape
        );

        rewardButtons[0].onClick.RemoveListener(
            SelectColorCrystal
        );

        rewardButtons[1].onClick.RemoveListener(
            SelectPoisonBomb
        );

        rewardButtons[2].onClick.RemoveListener(
            SelectShieldBomb
        );

        rewardButtons[3].onClick.RemoveListener(
            SelectHealBomb
        );

        rewardButtons[4].onClick.RemoveListener(
            SelectDamageBomb
        );
    }

    private void SelectStraightFive()
    {
        SelectShape(
            GemMasteryShape.StraightFive
        );
    }

    private void SelectLShape()
    {
        SelectShape(
            GemMasteryShape.LShape
        );
    }

    private void SelectTShape()
    {
        SelectShape(
            GemMasteryShape.TShape
        );
    }

    private void SelectCrossShape()
    {
        SelectShape(
            GemMasteryShape.CrossShape
        );
    }

    private void SelectColorCrystal()
    {
        SelectReward(
            GemMasteryReward.ColorCrystal
        );
    }

    private void SelectPoisonBomb()
    {
        SelectReward(
            GemMasteryReward.PoisonBomb
        );
    }

    private void SelectShieldBomb()
    {
        SelectReward(
            GemMasteryReward.ShieldBomb
        );
    }

    private void SelectHealBomb()
    {
        SelectReward(
            GemMasteryReward.HealBomb
        );
    }

    private void SelectDamageBomb()
    {
        SelectReward(
            GemMasteryReward.DamageBomb
        );
    }

    private void SelectShape(
        GemMasteryShape shape)
    {
        selectedShape = shape;
        Refresh();
    }

    private void SelectReward(
        GemMasteryReward reward)
    {
        if (!GemMasteryRuntimeResolver
                .IsRewardImplemented(reward))
        {
            return;
        }

        GemMasterySettings.SetReward(
            selectedShape,
            reward
        );

        Refresh();
    }

    private void HandleMasteryChanged(
        GemMasteryShape shape,
        GemMasteryReward reward)
    {
        Refresh();
    }

    private void Refresh()
    {
        if (!isInitialized)
        {
            return;
        }

        GemMasteryReward effectiveReward =
            GetEffectiveReward(
                selectedShape
            );

        selectionText.text =
            GetShapeDisplayName(selectedShape) +
            "  >  " +
            GetRewardDisplayName(effectiveReward);

        for (int index = 0;
             index < shapeButtons.Length;
             index++)
        {
            Button button =
                shapeButtons[index];

            bool isSelected =
                index == (int)selectedShape;

            SetButtonColor(
                button,
                isSelected
                    ? SelectedButtonColor
                    : NormalButtonColor
            );
        }

        for (int index = 0;
             index < rewardButtons.Length;
             index++)
        {
            Button button =
                rewardButtons[index];

            GemMasteryReward reward =
                (GemMasteryReward)index;

            bool isImplemented =
                GemMasteryRuntimeResolver
                    .IsRewardImplemented(reward);

            button.interactable =
                isImplemented;

            Text label =
                button.GetComponentInChildren<Text>(
                    true
                );

            if (label != null)
            {
                label.text =
                    GetRewardDisplayName(reward) +
                    (
                        isImplemented
                            ? string.Empty
                            : " (LOCKED)"
                    );
            }

            bool isSelected =
                reward == effectiveReward;

            Color buttonColor =
                !isImplemented
                    ? LockedButtonColor
                    : isSelected
                        ? SelectedButtonColor
                        : NormalButtonColor;

            SetButtonColor(
                button,
                buttonColor
            );
        }
    }

    private static GemMasteryReward
        GetEffectiveReward(
            GemMasteryShape shape)
    {
        GemMasteryReward reward =
            GemMasterySettings.GetReward(shape);

        if (GemMasteryRuntimeResolver
                .IsRewardImplemented(reward))
        {
            return reward;
        }

        return GemMasteryLoadout.Default
            .GetReward(shape);
    }

    private static void SetButtonColor(
        Button button,
        Color color)
    {
        if (button == null ||
            button.targetGraphic == null)
        {
            return;
        }

        button.targetGraphic.color = color;
    }

    private bool HasRequiredReferences()
    {
        if (shapeButtons == null ||
            shapeButtons.Length != 4 ||
            rewardButtons == null ||
            rewardButtons.Length != 5 ||
            selectionText == null)
        {
            Debug.LogError(
                "GemMasteryMenuController requires exactly four shape " +
                "buttons, five reward buttons, and one selection Text.",
                this
            );

            return false;
        }

        foreach (Button button in shapeButtons)
        {
            if (button == null)
            {
                Debug.LogError(
                    "Gem Mastery shape button reference is missing.",
                    this
                );

                return false;
            }
        }

        foreach (Button button in rewardButtons)
        {
            if (button == null)
            {
                Debug.LogError(
                    "Gem Mastery reward button reference is missing.",
                    this
                );

                return false;
            }
        }

        return true;
    }

    private static string GetShapeDisplayName(
        GemMasteryShape shape)
    {
        switch (shape)
        {
            case GemMasteryShape.StraightFive:
                return "STRAIGHT 5";

            case GemMasteryShape.LShape:
                return "L SHAPE";

            case GemMasteryShape.TShape:
                return "T SHAPE";

            case GemMasteryShape.CrossShape:
                return "CROSS";

            default:
                return shape.ToString().ToUpperInvariant();
        }
    }

    private static string GetRewardDisplayName(
        GemMasteryReward reward)
    {
        switch (reward)
        {
            case GemMasteryReward.ColorCrystal:
                return "COLOR CRYSTAL";

            case GemMasteryReward.PoisonBomb:
                return "POISON BOMB";

            case GemMasteryReward.ShieldBomb:
                return "SHIELD BOMB";

            case GemMasteryReward.HealBomb:
                return "HEAL BOMB";

            case GemMasteryReward.DamageBomb:
                return "DAMAGE BOMB";

            default:
                return reward.ToString().ToUpperInvariant();
        }
    }
}
