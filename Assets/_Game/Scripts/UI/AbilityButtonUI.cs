using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Button))]
public sealed class AbilityButtonUI : MonoBehaviour
{
    private const float AbilityButtonWidth = 176f;
    private const float AbilityButtonHeight = 64f;
    private const string AbilityEnergyBarName =
        "AbilityEnergyBar";

    [Header("References")]
    [SerializeField]
    private Button abilityButton;

    [SerializeField]
    private Image abilityIcon;

    [SerializeField]
    private Image energyFill;

    [SerializeField]
    private RectTransform energyBarFillMask;

    [SerializeField]
    private PlayerAbilityController
        playerAbilityController;

    [Header("Energy Animation")]
    [SerializeField]
    [Min(0.01f)]
    private float energyFillSmoothTime = 0.18f;

    [Header("Icon Colors")]
    [SerializeField]
    private Color unavailableIconColor =
        new Color(
            0.35f,
            0.35f,
            0.35f,
            1f
        );

    [SerializeField]
    private Color readyIconColor =
        Color.white;

    private float energyBarMaximumWidth;
    private float displayedCharge;
    private float targetCharge;
    private float chargeVelocity;
    private bool hasInitializedCharge;
    private TMP_Text energyAmount;

    private void Awake()
    {
        if (abilityButton == null)
        {
            abilityButton =
                GetComponent<Button>();
        }

        if (energyBarFillMask != null)
        {
            energyBarMaximumWidth =
                energyBarFillMask.rect.width;
        }

        ApplyBottomHudLayout();
        energyBarMaximumWidth = 64f;
        BuildFinalizedEnergyBar();
        energyAmount=GameUi.Label("EnergyAmount",transform,"",new Vector2(176,22),new Vector2(0,86),17);
    }

    private void OnEnable()
    {
        if (abilityButton != null)
        {
            abilityButton.onClick.RemoveListener(
                HandleButtonClicked
            );

            abilityButton.onClick.AddListener(
                HandleButtonClicked
            );
        }

        if (playerAbilityController != null)
        {
            playerAbilityController.StateChanged -=
                HandleAbilityStateChanged;

            playerAbilityController.StateChanged +=
                HandleAbilityStateChanged;
        }

        hasInitializedCharge = false;
        chargeVelocity = 0f;

        RefreshVisuals();
    }

    private void OnDisable()
    {
        if (abilityButton != null)
        {
            abilityButton.onClick.RemoveListener(
                HandleButtonClicked
            );
        }

        if (playerAbilityController != null)
        {
            playerAbilityController.StateChanged -=
                HandleAbilityStateChanged;
        }
    }

    private void OnValidate()
    {
        ApplyBottomHudLayout();
    }

    private void Update()
    {
        RefreshAvailability();
        if(energyAmount!=null&&playerAbilityController!=null)
            energyAmount.text=$"Energy {playerAbilityController.CurrentEnergy} / {playerAbilityController.RequiredEnergy}";

        if (!hasInitializedCharge)
        {
            return;
        }

        displayedCharge =
            Mathf.SmoothDamp(
                displayedCharge,
                targetCharge,
                ref chargeVelocity,
                Mathf.Max(
                    0.01f,
                    energyFillSmoothTime
                ),
                Mathf.Infinity,
                Time.unscaledDeltaTime
            );

        if (Mathf.Abs(
                displayedCharge -
                targetCharge
            ) < 0.001f)
        {
            displayedCharge =
                targetCharge;

            chargeVelocity = 0f;
        }

        ApplyEnergyVisual(
            displayedCharge
        );
    }

    private void HandleButtonClicked()
    {
        if (playerAbilityController == null)
        {
            return;
        }

        playerAbilityController.TryActivate();

        RefreshVisuals();
    }

    private void HandleAbilityStateChanged()
    {
        RefreshVisuals();
    }

    private void RefreshVisuals()
    {
        CharacterAbilityDefinition ability =
            playerAbilityController != null
                ? playerAbilityController.ActiveAbility
                : null;

        float normalizedCharge =
            playerAbilityController != null
                ? playerAbilityController
                    .ChargeNormalized
                : 0f;

        normalizedCharge =
            Mathf.Clamp01(
                normalizedCharge
            );

        if (!hasInitializedCharge)
        {
            displayedCharge =
                normalizedCharge;

            targetCharge =
                normalizedCharge;

            hasInitializedCharge = true;

            ApplyEnergyVisual(
                displayedCharge
            );
        }
        else
        {
            targetCharge =
                normalizedCharge;
        }

        if (abilityIcon != null &&
            ability != null &&
            ability.Icon != null)
        {
            abilityIcon.sprite =
                ability.Icon;
            GameplayPixelGrid.FitImage(abilityIcon, abilityIcon.rectTransform.rect.size);
        }

        RefreshAvailability();
    }

    private void RefreshAvailability()
    {
        bool canActivate =
            playerAbilityController != null &&
            playerAbilityController.CanActivate;

        if (abilityButton != null)
        {
            abilityButton.interactable =
                canActivate;
        }

        if (abilityIcon != null)
        {
            abilityIcon.color =
                canActivate
                    ? readyIconColor
                    : unavailableIconColor;
        }
    }

    private void ApplyEnergyVisual(
        float normalizedCharge)
    {
        normalizedCharge =
            Mathf.Clamp01(
                normalizedCharge
            );

        if (energyFill != null)
        {
            energyFill.fillAmount =
                Mathf.Round(normalizedCharge * energyBarMaximumWidth) / Mathf.Max(1, energyBarMaximumWidth);
        }

        if (energyBarFillMask == null)
        {
            return;
        }

        if (energyBarMaximumWidth <= 0f)
        {
            energyBarMaximumWidth =
                energyBarFillMask.rect.width;
        }

        energyBarFillMask
            .SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                Mathf.Round(energyBarMaximumWidth * normalizedCharge)
            );
    }

    private void ApplyBottomHudLayout()
    {
        ApplyAbilityButtonSize();
        RepositionAbilityEnergyBar();
    }

    private void BuildFinalizedEnergyBar()
    {
        var frame = FinalizedUiSkin.Load("EnergyFrame");
        var track = FinalizedUiSkin.Load("EnergyTrack");
        var fill = FinalizedUiSkin.Load("EnergyFill");
        if (frame == null || track == null || fill == null) return;
        if (transform.Find(AbilityEnergyBarName) is not RectTransform root) return;
        foreach (Transform child in root) child.gameObject.SetActive(false);
        if (root.TryGetComponent<Image>(out var oldImage)) oldImage.enabled = false;
        root.localRotation = Quaternion.identity; root.sizeDelta = new Vector2(144,32);
        root.anchoredPosition = new Vector2(0,58);
        energyBarMaximumWidth = 107;
        var empty = EnergyImage("EmptyEnergyTrack", root, track, new Vector2(107,8), new Vector2(-.5f,-1));
        empty.rectTransform.anchorMin = empty.rectTransform.anchorMax = new Vector2(0,.5f);
        empty.rectTransform.pivot = new Vector2(0,.5f); empty.rectTransform.anchoredPosition = new Vector2(18,-1);
        energyBarFillMask = GameUi.Rect("EnergyFillMask", root, new Vector2(107,8), Vector2.zero);
        energyBarFillMask.anchorMin = energyBarFillMask.anchorMax = new Vector2(0,.5f);
        energyBarFillMask.pivot = new Vector2(0,.5f); energyBarFillMask.anchoredPosition = new Vector2(18,-1);
        energyBarFillMask.gameObject.AddComponent<RectMask2D>();
        energyFill = EnergyImage("EnergyFill", energyBarFillMask, fill, new Vector2(107,8), Vector2.zero);
        energyFill.rectTransform.anchorMin = energyFill.rectTransform.anchorMax = new Vector2(0,.5f);
        energyFill.rectTransform.pivot = new Vector2(0,.5f);
        EnergyImage("EnergyFrame", root, frame, new Vector2(144,32), Vector2.zero).type = Image.Type.Simple;
    }

    private static Image EnergyImage(string name, Transform parent, Sprite sprite, Vector2 size, Vector2 position)
    {
        var rect = GameUi.Rect(name,parent,size,position);
        var image = rect.gameObject.AddComponent<Image>();
        image.sprite = sprite; image.type = Image.Type.Tiled; image.color = Color.white; image.raycastTarget = false;
        return image;
    }

    private void ApplyAbilityButtonSize()
    {
        RectTransform buttonRect =
            transform as RectTransform;

        if (buttonRect == null)
        {
            return;
        }

        buttonRect.localScale = Vector3.one;
        buttonRect.anchorMin = buttonRect.anchorMax = buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = new Vector2(0f, -24f);
        buttonRect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal,
            AbilityButtonWidth
        );

        buttonRect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            AbilityButtonHeight
        );
    }

    private void RepositionAbilityEnergyBar()
    {
        Transform energyBarTransform =
            transform.Find(
                AbilityEnergyBarName
            );

        if (energyBarTransform is not RectTransform energyBarRect)
        {
            return;
        }

        energyBarRect.localScale = Vector3.one;
        energyBarRect.sizeDelta = new Vector2(64f, 64f);
        foreach (RectTransform child in energyBarRect)
        {
            child.localScale = Vector3.one;
            if (child.anchorMin == child.anchorMax) child.sizeDelta = new Vector2(64f, 64f);
        }
        energyBarRect.anchorMin = energyBarRect.anchorMax = energyBarRect.pivot = new Vector2(.5f,.5f);
        energyBarRect.anchoredPosition = new Vector2(0f,64f);
        energyBarRect.localRotation = Quaternion.Euler(0,0,-90);
    }

}
