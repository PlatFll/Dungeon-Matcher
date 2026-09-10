using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(100)]
[DisallowMultipleComponent]
public sealed class PlayerAreaThreeSliceFrameController : MonoBehaviour
{
    private const string ProfileResourcePath =
        "UI/PlayerAreaFrameProfile";

    private const string GeneratedLayoutName =
        "GeneratedTopBattleLayout";

    private const string PlayerSectionName =
        "PlayerSection";

    private const string EnemySectionName =
        "EnemySection";

    private const string LegacyPlayerFrameName =
        "PlayerSectionFrame";

    private const string GeneratedFrameName =
        "PlayerAreaThreeSliceFrame";

    private const float FallbackPlayerSectionWidth =
        146f;

    private const float SectionOuterInset =
        6f;

    private const float SectionGap =
        12f;

    private const float PlayerFrameLeftInset =
        16f;

    private RectTransform topHud;
    private RectTransform playerSection;
    private RectTransform enemySection;
    private PlayerAreaFrameProfile profile;
    private bool configured;
    private bool missingArtWarningShown;

    private void Awake()
    {
        topHud =
            transform as RectTransform;

        profile =
            Resources.Load<PlayerAreaFrameProfile>(
                ProfileResourcePath
            );
    }

    private void Start()
    {
        TryConfigure();
    }

    private void LateUpdate()
    {
        if (!configured)
        {
            TryConfigure();
        }
    }

    private void TryConfigure()
    {
        if (topHud == null)
        {
            topHud =
                transform as RectTransform;
        }

        if (topHud == null)
        {
            return;
        }

        RectTransform generatedLayout =
            topHud.Find(
                GeneratedLayoutName
            ) as RectTransform;

        if (generatedLayout == null)
        {
            return;
        }

        playerSection =
            generatedLayout.Find(
                PlayerSectionName
            ) as RectTransform;

        enemySection =
            generatedLayout.Find(
                EnemySectionName
            ) as RectTransform;

        if (playerSection == null ||
            enemySection == null)
        {
            return;
        }

        ApplySectionGeometry(
            generatedLayout
        );

        RemoveLegacyPlayerFrame();
        BuildThreeSliceFrame();

        configured = true;
    }

    private void ApplySectionGeometry(
        RectTransform generatedLayout)
    {
        float playerWidth =
            profile != null
                ? profile.PlayerSectionWidth
                : FallbackPlayerSectionWidth;

        playerWidth =
            Mathf.Max(
                1f,
                playerWidth
            );

        float maximumPlayerWidth =
            Mathf.Max(
                1f,
                generatedLayout.rect.width -
                SectionOuterInset * 2f -
                SectionGap -
                120f
            );

        playerWidth =
            Mathf.Min(
                playerWidth,
                maximumPlayerWidth
            );

        playerSection.anchorMin =
            new Vector2(0f, 0f);
        playerSection.anchorMax =
            new Vector2(0f, 1f);
        playerSection.pivot =
            new Vector2(0.5f, 0.5f);
        playerSection.offsetMin =
            new Vector2(
                PlayerFrameLeftInset,
                16f + 6f
            );
        playerSection.offsetMax =
            new Vector2(
                PlayerFrameLeftInset +
                playerWidth,
                -(16f + 6f)
            );

        enemySection.anchorMin =
            new Vector2(0f, 0f);
        enemySection.anchorMax =
            new Vector2(1f, 1f);
        enemySection.pivot =
            new Vector2(0.5f, 0.5f);
        enemySection.offsetMin =
            new Vector2(
                SectionOuterInset +
                playerWidth +
                SectionGap,
                SectionOuterInset
            );
        enemySection.offsetMax =
            new Vector2(
                -SectionOuterInset,
                -SectionOuterInset
            );
    }

    private void RemoveLegacyPlayerFrame()
    {
        Transform legacyFrame =
            playerSection.Find(
                LegacyPlayerFrameName
            );

        if (legacyFrame == null)
        {
            return;
        }

        legacyFrame.gameObject.SetActive(false);

        if (Application.isPlaying)
        {
            Destroy(
                legacyFrame.gameObject
            );
        }
        else
        {
            DestroyImmediate(
                legacyFrame.gameObject
            );
        }
    }

    private void BuildThreeSliceFrame()
    {
        RemoveGeneratedFrame();

        if (profile == null ||
            profile.TopPiece == null ||
            profile.MiddlePiece == null ||
            profile.BottomPiece == null)
        {
            if (!missingArtWarningShown)
            {
                Debug.LogWarning(
                    "PlayerAreaThreeSliceFrameController is active, but the " +
                    "PlayerAreaFrameProfile still needs Top, Middle, and Bottom " +
                    "frame sprites assigned.",
                    this
                );

                missingArtWarningShown = true;
            }

            return;
        }

        float frameWidth =
            profile.PlayerSectionWidth;

        float topHeight =
            GetNativeUiHeight(
                profile.TopPiece
            );

        float bottomHeight =
            GetNativeUiHeight(
                profile.BottomPiece
            );

        ValidateAuthoredWidth(
            profile.TopPiece,
            frameWidth,
            "Top"
        );

        ValidateAuthoredWidth(
            profile.MiddlePiece,
            frameWidth,
            "Middle"
        );

        ValidateAuthoredWidth(
            profile.BottomPiece,
            frameWidth,
            "Bottom"
        );

        RectTransform frameRoot =
            CreateRectTransform(
                GeneratedFrameName,
                playerSection
            );

        StretchToParent(
            frameRoot
        );

        frameRoot.SetAsLastSibling();

        Image topImage =
            CreateImage(
                "TopPiece",
                frameRoot,
                profile.TopPiece
            );

        RectTransform topRect =
            topImage.rectTransform;

        topRect.anchorMin =
            new Vector2(0.5f, 1f);
        topRect.anchorMax =
            topRect.anchorMin;
        topRect.pivot =
            new Vector2(0.5f, 1f);
        topRect.anchoredPosition =
            Vector2.zero;
        topRect.sizeDelta =
            new Vector2(
                frameWidth,
                topHeight
            );

        topImage.type =
            Image.Type.Simple;

        Image bottomImage =
            CreateImage(
                "BottomPiece",
                frameRoot,
                profile.BottomPiece
            );

        RectTransform bottomRect =
            bottomImage.rectTransform;

        bottomRect.anchorMin =
            new Vector2(0.5f, 0f);
        bottomRect.anchorMax =
            bottomRect.anchorMin;
        bottomRect.pivot =
            new Vector2(0.5f, 0f);
        bottomRect.anchoredPosition =
            Vector2.zero;
        bottomRect.sizeDelta =
            new Vector2(
                frameWidth,
                bottomHeight
            );

        bottomImage.type =
            Image.Type.Simple;

        Image middleImage =
            CreateImage(
                "MiddlePiece",
                frameRoot,
                profile.MiddlePiece
            );

        RectTransform middleRect =
            middleImage.rectTransform;

        middleRect.anchorMin =
            new Vector2(0.5f, 0f);
        middleRect.anchorMax =
            new Vector2(0.5f, 1f);
        middleRect.pivot =
            new Vector2(0.5f, 0.5f);
        middleRect.offsetMin =
            new Vector2(
                -frameWidth * 0.5f,
                bottomHeight
            );
        middleRect.offsetMax =
            new Vector2(
                frameWidth * 0.5f,
                -topHeight
            );

        middleImage.type =
            Image.Type.Tiled;

        middleRect.SetAsFirstSibling();
    }

    private void RemoveGeneratedFrame()
    {
        if (playerSection == null)
        {
            return;
        }

        Transform existingFrame =
            playerSection.Find(
                GeneratedFrameName
            );

        if (existingFrame == null)
        {
            return;
        }

        existingFrame.gameObject.SetActive(false);

        if (Application.isPlaying)
        {
            Destroy(
                existingFrame.gameObject
            );
        }
        else
        {
            DestroyImmediate(
                existingFrame.gameObject
            );
        }
    }

    private float GetNativeUiHeight(
        Sprite sprite)
    {
        if (sprite == null ||
            sprite.pixelsPerUnit <= 0f)
        {
            return 0f;
        }

        return
            sprite.rect.height *
            GetReferencePixelsPerSpritePixel(
                sprite
            );
    }

    private float GetNativeUiWidth(
        Sprite sprite)
    {
        if (sprite == null ||
            sprite.pixelsPerUnit <= 0f)
        {
            return 0f;
        }

        return
            sprite.rect.width *
            GetReferencePixelsPerSpritePixel(
                sprite
            );
    }

    private float GetReferencePixelsPerSpritePixel(
        Sprite sprite)
    {
        if (sprite == null ||
            sprite.pixelsPerUnit <= 0f)
        {
            return 1f;
        }

        CanvasScaler scaler =
            GetComponentInParent<CanvasScaler>();

        float referencePixelsPerUnit =
            scaler != null
                ? scaler.referencePixelsPerUnit
                : sprite.pixelsPerUnit;

        return
            referencePixelsPerUnit /
            sprite.pixelsPerUnit;
    }

    private void ValidateAuthoredWidth(
        Sprite sprite,
        float expectedWidth,
        string pieceName)
    {
        float nativeWidth =
            GetNativeUiWidth(
                sprite
            );

        if (Mathf.Abs(
                nativeWidth -
                expectedWidth
            ) <= 0.01f)
        {
            return;
        }

        Debug.LogWarning(
            $"Player frame {pieceName} piece is {nativeWidth:0.##} reference " +
            $"pixels wide, but the player frame is authored for " +
            $"{expectedWidth:0.##}. Keep all three source sprites at the exact " +
            "profile width to avoid horizontal pixel-art scaling.",
            this
        );
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

        RectTransform rect =
            rectObject.GetComponent<RectTransform>();

        rect.SetParent(
            parent,
            false
        );

        rect.localScale =
            Vector3.one;

        return rect;
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

        RectTransform rect =
            imageObject.GetComponent<RectTransform>();

        rect.SetParent(
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

    private static void StretchToParent(
        RectTransform rect)
    {
        rect.anchorMin =
            Vector2.zero;
        rect.anchorMax =
            Vector2.one;
        rect.offsetMin =
            Vector2.zero;
        rect.offsetMax =
            Vector2.zero;
        rect.localScale =
            Vector3.one;
    }
}
