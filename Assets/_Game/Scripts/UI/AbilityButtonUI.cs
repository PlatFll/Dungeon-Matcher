using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Button))]
public sealed class AbilityButtonUI : MonoBehaviour
{
    private const float BottomHudHeight = 88f;
    private const float BottomHudPieceSize = 44f;
    private const string GeneratedBottomHudFrameName =
        "GeneratedBottomHudFrame";

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

    [Header("Bottom HUD Frame")]
    [SerializeField]
    [Tooltip(
        "44x44 top-left corner sprite. The runtime mirrors this one sprite " +
        "to create all four corners of the 540x88 BottomHUD."
    )]
    private Sprite cornerPiece;

    [SerializeField]
    [Tooltip(
        "44x44 straight top-edge sprite. The runtime tiles it between the " +
        "corners and mirrors it vertically for the bottom half."
    )]
    private Sprite normalPiece;

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

        ApplyBottomHudHeight();
        BuildBottomHudFrame();
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
        ApplyBottomHudHeight();
    }

    private void Update()
    {
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

        bool canActivate =
            playerAbilityController != null &&
            playerAbilityController.CanActivate;

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

        if (abilityButton != null)
        {
            abilityButton.interactable =
                canActivate;
        }

        if (abilityIcon != null)
        {
            if (ability != null &&
                ability.Icon != null)
            {
                abilityIcon.sprite =
                    ability.Icon;
            }

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
                normalizedCharge;
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
                energyBarMaximumWidth *
                normalizedCharge
            );
    }

    private void ApplyBottomHudHeight()
    {
        RectTransform bottomHud =
            FindBottomHud();

        if (bottomHud == null)
        {
            return;
        }

        bottomHud.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            BottomHudHeight
        );
    }

    private void BuildBottomHudFrame()
    {
        RectTransform bottomHud =
            FindBottomHud();

        if (bottomHud == null ||
            cornerPiece == null ||
            normalPiece == null)
        {
            return;
        }

        Transform existingFrame =
            bottomHud.Find(
                GeneratedBottomHudFrameName
            );

        if (existingFrame != null)
        {
            Destroy(
                existingFrame.gameObject
            );
        }

        RectTransform frameRoot =
            CreateRectTransform(
                GeneratedBottomHudFrameName,
                bottomHud
            );

        StretchToParent(
            frameRoot
        );

        frameRoot.SetAsFirstSibling();

        CreateNormalPiece(
            frameRoot,
            "TopEdge",
            false
        );

        CreateNormalPiece(
            frameRoot,
            "BottomEdge",
            true
        );

        CreateCornerPiece(
            frameRoot,
            "TopLeftCorner",
            new Vector2(0f, 1f),
            new Vector2(
                BottomHudPieceSize * 0.5f,
                -BottomHudPieceSize * 0.5f
            ),
            new Vector3(1f, 1f, 1f)
        );

        CreateCornerPiece(
            frameRoot,
            "TopRightCorner",
            new Vector2(1f, 1f),
            new Vector2(
                -BottomHudPieceSize * 0.5f,
                -BottomHudPieceSize * 0.5f
            ),
            new Vector3(-1f, 1f, 1f)
        );

        CreateCornerPiece(
            frameRoot,
            "BottomLeftCorner",
            new Vector2(0f, 0f),
            new Vector2(
                BottomHudPieceSize * 0.5f,
                BottomHudPieceSize * 0.5f
            ),
            new Vector3(1f, -1f, 1f)
        );

        CreateCornerPiece(
            frameRoot,
            "BottomRightCorner",
            new Vector2(1f, 0f),
            new Vector2(
                -BottomHudPieceSize * 0.5f,
                BottomHudPieceSize * 0.5f
            ),
            new Vector3(-1f, -1f, 1f)
        );
    }

    private void CreateNormalPiece(
        RectTransform parent,
        string objectName,
        bool flipVertically)
    {
        Image image =
            CreateImage(
                objectName,
                parent,
                normalPiece
            );

        RectTransform imageRect =
            image.rectTransform;

        if (flipVertically)
        {
            imageRect.anchorMin =
                new Vector2(0f, 0f);

            imageRect.anchorMax =
                new Vector2(1f, 0f);

            imageRect.offsetMin =
                new Vector2(
                    BottomHudPieceSize,
                    0f
                );

            imageRect.offsetMax =
                new Vector2(
                    -BottomHudPieceSize,
                    BottomHudPieceSize
                );

            imageRect.localScale =
                new Vector3(1f, -1f, 1f);
        }
        else
        {
            imageRect.anchorMin =
                new Vector2(0f, 1f);

            imageRect.anchorMax =
                new Vector2(1f, 1f);

            imageRect.offsetMin =
                new Vector2(
                    BottomHudPieceSize,
                    -BottomHudPieceSize
                );

            imageRect.offsetMax =
                new Vector2(
                    -BottomHudPieceSize,
                    0f
                );
        }

        imageRect.pivot =
            new Vector2(0.5f, 0.5f);

        image.type =
            Image.Type.Tiled;
    }

    private void CreateCornerPiece(
        RectTransform parent,
        string objectName,
        Vector2 anchor,
        Vector2 anchoredPosition,
        Vector3 localScale)
    {
        Image image =
            CreateImage(
                objectName,
                parent,
                cornerPiece
            );

        RectTransform imageRect =
            image.rectTransform;

        imageRect.anchorMin = anchor;
        imageRect.anchorMax = anchor;
        imageRect.pivot =
            new Vector2(0.5f, 0.5f);

        imageRect.anchoredPosition =
            anchoredPosition;

        imageRect.sizeDelta =
            new Vector2(
                BottomHudPieceSize,
                BottomHudPieceSize
            );

        imageRect.localScale =
            localScale;

        image.type =
            Image.Type.Simple;
    }

    private static Image CreateImage(
        string objectName,
        RectTransform parent,
        Sprite sprite)
    {
        GameObject imageObject =
            new GameObject(
                objectName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image)
            );

        imageObject.layer =
            parent.gameObject.layer;

        RectTransform imageRect =
            imageObject.GetComponent<RectTransform>();

        imageRect.SetParent(
            parent,
            false
        );

        Image image =
            imageObject.GetComponent<Image>();

        image.sprite = sprite;
        image.color = Color.white;
        image.raycastTarget = false;
        image.preserveAspect = false;
        image.pixelsPerUnitMultiplier = 1f;

        return image;
    }

    private static RectTransform CreateRectTransform(
        string objectName,
        RectTransform parent)
    {
        GameObject rectObject =
            new GameObject(
                objectName,
                typeof(RectTransform)
            );

        rectObject.layer =
            parent.gameObject.layer;

        RectTransform rectTransform =
            rectObject.GetComponent<RectTransform>();

        rectTransform.SetParent(
            parent,
            false
        );

        return rectTransform;
    }

    private static void StretchToParent(
        RectTransform rectTransform)
    {
        rectTransform.anchorMin =
            Vector2.zero;

        rectTransform.anchorMax =
            Vector2.one;

        rectTransform.offsetMin =
            Vector2.zero;

        rectTransform.offsetMax =
            Vector2.zero;

        rectTransform.localScale =
            Vector3.one;
    }

    private RectTransform FindBottomHud()
    {
        Transform current =
            transform.parent;

        while (current != null)
        {
            if (current.name == "BottomHUD")
            {
                return
                    current as RectTransform;
            }

            current = current.parent;
        }

        return null;
    }
}
