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

    private RectTransform topHud;
    private RectTransform playerSection;
    private RectTransform enemySection;
    private PlayerAreaFrameProfile profile;
    private bool configured;
    private float lastLayoutHeight, lastLayoutWidth;
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
        if (configured && playerSection != null && playerSection.parent is RectTransform root && (root.rect.height != lastLayoutHeight || root.rect.width != lastLayoutWidth))
            ApplySectionGeometry(root);
    }

    private void TryConfigure()
    {
        profile=BattlePresentationSkin.PlayerFrame ?? RunSession.Current?.Zone?.Definition?.theme?.playerFrame ?? profile;
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
        lastLayoutHeight = generatedLayout.rect.height;
        lastLayoutWidth = generatedLayout.rect.width;
        var owner = GetComponentInParent<GameplayPixelLayoutController>();
        if (owner == null) return;
        GameplayPixelLayoutController.SetRect(playerSection, owner.SharedPlayerRect(generatedLayout.rect.size));
        float x = generatedLayout.rect.width / 470f;
        GameplayPixelLayoutController.SetRect(enemySection, new Rect(Mathf.Round(140 * x), 6,
            Mathf.Round(330 * x) - 6, lastLayoutHeight - 12));
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
        if (IronveinBattleAssembly.Current is IronveinBattleAssembly mine)
        { if (!playerSection.Find("PixelLabPortraitFrame")) mine.frames.Frame(playerSection, "Portrait", 50, BattleFloorAnchor.Unit(playerSection.GetComponentInParent<Canvas>())); return; }
        if (DrownedCourtBattleAssembly.Current is DrownedCourtBattleAssembly court)
        { if (!playerSection.Find("PixelLabPortraitFrame")) court.frames.Frame(playerSection, "Portrait", 45, BattleFloorAnchor.Unit(playerSection.GetComponentInParent<Canvas>())); return; }
        if (DungeonPixelLabBattleArt.Current is DungeonPixelLabBattleArt nativeArt)
        { nativeArt.Frame(playerSection, "Portrait", 40, DungeonPixelLabBattleArt.Unit(playerSection.GetComponentInParent<Canvas>())); return; }
        if(ApprovedDungeonBattleArt.Current is ApprovedDungeonBattleArt approved)
        {
            approved.PlayerFrame(playerSection);return;
        }
        if(BattlePresentationSkin.UsesReferenceKit)
        {
            Vector2 size=BattlePresentationSkin.ReferencePlayerRect.size;
            DungeonPresentationArt.ReferenceLayer(playerSection,"PlayerFrame",new Rect(Vector2.zero,size),size).SetAsLastSibling();
            return;
        }

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

        float frameWidth = profile.PlayerSectionWidth;

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

        ConfigureResizableStrip(topImage, profile.TopPiece);

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

        ConfigureResizableStrip(bottomImage, profile.BottomPiece);

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

        ConfigureResizableStrip(middleImage, profile.MiddlePiece);

        middleRect.SetAsFirstSibling();
    }

    private readonly System.Collections.Generic.List<Sprite> resizedStripSources = new System.Collections.Generic.List<Sprite>();
    private void ConfigureResizableStrip(Image image, Sprite source)
    {
        // Runtime nine-slice registration uses the original texture unchanged.
        // Native endcaps remain intact, and Image.Tiled repeats the straight rail.
        var sliced = Sprite.Create(source.texture, source.rect, source.pivot / source.rect.size,
            source.pixelsPerUnit, 0, SpriteMeshType.FullRect, new Vector4(16, 0, 16, 0));
        sliced.name = source.name + "_Responsive";
        resizedStripSources.Add(sliced); image.sprite = sliced; image.type = Image.Type.Tiled;
        var rect = image.rectTransform;
        rect.anchorMin = new Vector2(0, rect.anchorMin.y);
        rect.anchorMax = new Vector2(1, rect.anchorMax.y);
        rect.offsetMin = new Vector2(0, rect.offsetMin.y);
        rect.offsetMax = new Vector2(0, rect.offsetMax.y);
    }
    private void OnDestroy()
    {
        foreach (var sprite in resizedStripSources) if (sprite) Destroy(sprite);
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
