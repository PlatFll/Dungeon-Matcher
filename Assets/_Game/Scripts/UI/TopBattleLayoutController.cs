using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(50)]
[DisallowMultipleComponent]
public sealed class TopBattleLayoutController : MonoBehaviour
{
    private const string GeneratedLayoutName =
        "GeneratedTopBattleLayout";

    private const float PlayerSectionRatio =
        0.31f;

    private const float SectionInset =
        6f;

    private const float FrameThickness =
        10f;

    private const float FrameCornerSize =
        50f;

    private const float HealthBarHeight =
        20f;

    private const float HealthBarBottomGap =
        14f;

    [Header("Optional Overrides")]
    [SerializeField]
    [Tooltip(
        "Optional frame corner override. When empty, the controller reuses " +
        "the board modular frame and then falls back to the BottomHUD frame."
    )]
    private Sprite cornerPieceOverride;

    [SerializeField]
    [Tooltip(
        "Optional frame edge override. When empty, the controller reuses " +
        "the board modular frame and then falls back to the BottomHUD frame."
    )]
    private Sprite normalPieceOverride;

    [SerializeField]
    [Tooltip(
        "Optional player-side background. This is intentionally independent " +
        "from the enemy-side background so each character can later have its " +
        "own presentation."
    )]
    private Sprite playerBackgroundSprite;

    [SerializeField]
    [Tooltip(
        "Optional enemy-side background. When empty, the current dungeon " +
        "battle background is reused for this prototype layout."
    )]
    private Sprite enemyBackgroundSprite;

    [Header("Temporary Background Colors")]
    [SerializeField]
    private Color playerBackgroundColor =
        new Color32(25, 21, 31, 255);

    [SerializeField]
    private Color enemyBackgroundColor =
        Color.white;

    private RectTransform topHud;
    private RectTransform playerPanel;
    private RectTransform enemyArea;
    private Sprite cornerPiece;
    private Sprite normalPiece;
    private Sprite temporaryDungeonBackground;
    private bool layoutBuilt;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.AfterSceneLoad
    )]
    private static void InstallOnGameScene()
    {
        GameObject topHudObject =
            GameObject.Find("TopHUD");

        if (topHudObject == null)
        {
            return;
        }

        if (!topHudObject.TryGetComponent(
                out TopBattleLayoutController _
            ))
        {
            topHudObject.AddComponent<
                TopBattleLayoutController
            >();
        }
    }

    private void Start()
    {
        BuildLayout();
    }

    private void BuildLayout()
    {
        if (layoutBuilt)
        {
            return;
        }

        topHud =
            transform as RectTransform;

        playerPanel =
            FindRectTransform(
                transform,
                "PlayerPanel"
            );

        enemyArea =
            FindRectTransform(
                transform,
                "EnemyArea"
            );

        if (topHud == null ||
            playerPanel == null ||
            enemyArea == null)
        {
            Debug.LogWarning(
                "TopBattleLayoutController could not find TopHUD, " +
                "PlayerPanel, or EnemyArea. The old battle layout was left " +
                "unchanged.",
                this
            );

            return;
        }

        ResolveTemporaryBackground();
        ResolveFrameSprites();

        RectTransform layoutRoot =
            CreateRectTransform(
                GeneratedLayoutName,
                topHud
            );

        StretchToParent(
            layoutRoot,
            Vector2.zero,
            Vector2.zero
        );

        layoutRoot.SetAsFirstSibling();

        RectTransform playerSection =
            CreateSection(
                "PlayerSection",
                layoutRoot,
                new Vector2(0f, 0f),
                new Vector2(
                    PlayerSectionRatio,
                    1f
                ),
                playerBackgroundSprite,
                playerBackgroundColor
            );

        RectTransform enemySection =
            CreateSection(
                "EnemySection",
                layoutRoot,
                new Vector2(
                    PlayerSectionRatio,
                    0f
                ),
                new Vector2(1f, 1f),
                enemyBackgroundSprite != null
                    ? enemyBackgroundSprite
                    : temporaryDungeonBackground,
                enemyBackgroundColor
            );

        ReparentAndStretchContent(
            playerPanel,
            playerSection
        );

        ReparentAndStretchContent(
            enemyArea,
            enemySection
        );

        LayoutPlayerPanel();
        LayoutEnemyArea();

        /*
         * Match the mockup hierarchy: one frame surrounds the entire battle
         * arena, then the player receives an additional inner frame that visually
         * separates Rattlebones from the enemy side. The enemy side uses the
         * outer frame plus the player's right-hand divider rather than a second
         * nested box.
         */
        BuildFrame(
            layoutRoot,
            "BattleArenaFrame"
        );

        BuildFrame(
            playerSection,
            "PlayerSectionFrame"
        );

        layoutBuilt = true;
    }

    private RectTransform CreateSection(
        string objectName,
        RectTransform parent,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Sprite backgroundSprite,
        Color backgroundColor)
    {
        RectTransform section =
            CreateRectTransform(
                objectName,
                parent
            );

        section.anchorMin = anchorMin;
        section.anchorMax = anchorMax;
        section.offsetMin =
            new Vector2(
                SectionInset,
                SectionInset
            );
        section.offsetMax =
            new Vector2(
                -SectionInset,
                -SectionInset
            );

        Image background =
            CreateImage(
                objectName + "Background",
                section,
                backgroundSprite
            );

        StretchToParent(
            background.rectTransform,
            Vector2.zero,
            Vector2.zero
        );

        background.color =
            backgroundColor;

        background.preserveAspect = false;
        background.raycastTarget = false;
        background.rectTransform.SetAsFirstSibling();

        return section;
    }

    private static void ReparentAndStretchContent(
        RectTransform content,
        RectTransform parent)
    {
        content.SetParent(
            parent,
            false
        );

        StretchToParent(
            content,
            new Vector2(
                FrameThickness + 3f,
                FrameThickness + 3f
            ),
            new Vector2(
                -(FrameThickness + 3f),
                -(FrameThickness + 3f)
            )
        );

        content.SetAsLastSibling();
    }

    private void LayoutPlayerPanel()
    {
        RectTransform oldPlayerFrame =
            FindRectTransform(
                playerPanel,
                "PlayerFrame"
            );

        if (oldPlayerFrame != null)
        {
            oldPlayerFrame.gameObject.SetActive(false);
        }

        RectTransform playerCharacter =
            FindRectTransform(
                playerPanel,
                "PlayerCharacter"
            );

        if (playerCharacter != null)
        {
            AnchorAtPoint(
                playerCharacter,
                new Vector2(0.5f, 0.56f),
                new Vector2(104f, 118f),
                Vector2.zero
            );
        }

        RectTransform playerBase =
            FindRectTransform(
                playerPanel,
                "PlayerBase"
            );

        if (playerBase != null)
        {
            AnchorAtPoint(
                playerBase,
                new Vector2(0.5f, 0.27f),
                new Vector2(102f, 30f),
                Vector2.zero
            );
        }

        RectTransform healthBar =
            FindRectTransform(
                playerPanel,
                "PlayerHPBarBackground"
            );

        if (healthBar != null)
        {
            PlaceBottomHealthBar(
                healthBar,
                10f
            );
        }
    }

    private void LayoutEnemyArea()
    {
        EnemySlotUI[] slots =
            enemyArea.GetComponentsInChildren<
                EnemySlotUI
            >(true);

        if (slots == null ||
            slots.Length == 0)
        {
            return;
        }

        System.Array.Sort(
            slots,
            (left, right) =>
                string.CompareOrdinal(
                    left.name,
                    right.name
                )
        );

        int slotCount =
            slots.Length;

        for (int index = 0;
             index < slotCount;
             index++)
        {
            RectTransform slotRect =
                slots[index].transform as RectTransform;

            if (slotRect == null)
            {
                continue;
            }

            float start =
                (float)index /
                slotCount;

            float end =
                (float)(index + 1) /
                slotCount;

            slotRect.anchorMin =
                new Vector2(start, 0f);

            slotRect.anchorMax =
                new Vector2(end, 1f);

            slotRect.pivot =
                new Vector2(0.5f, 0.5f);

            slotRect.offsetMin =
                new Vector2(3f, 0f);

            slotRect.offsetMax =
                new Vector2(-3f, 0f);

            slotRect.localScale =
                Vector3.one;

            LayoutEnemySlot(
                slotRect
            );
        }
    }

    private void LayoutEnemySlot(
        RectTransform slotRect)
    {
        RectTransform spawnAnchor =
            FindRectTransform(
                slotRect,
                "EnemySpawnAnchor"
            );

        if (spawnAnchor != null)
        {
            AnchorAtPoint(
                spawnAnchor,
                new Vector2(0.5f, 0.57f),
                new Vector2(106f, 124f),
                Vector2.zero
            );

            spawnAnchor.localScale =
                new Vector3(1.08f, 1.08f, 1f);
        }

        RectTransform enemyBase =
            FindRectTransform(
                slotRect,
                "EnemyBase"
            );

        if (enemyBase != null)
        {
            AnchorAtPoint(
                enemyBase,
                new Vector2(0.5f, 0.28f),
                new Vector2(84f, 25f),
                Vector2.zero
            );
        }

        RectTransform healthBar =
            FindRectTransform(
                slotRect,
                "EnemyHPBarBackground"
            );

        if (healthBar != null)
        {
            PlaceBottomHealthBar(
                healthBar,
                7f
            );
        }
    }

    private static void PlaceBottomHealthBar(
        RectTransform healthBar,
        float horizontalInset)
    {
        healthBar.anchorMin =
            new Vector2(0f, 0f);

        healthBar.anchorMax =
            new Vector2(1f, 0f);

        healthBar.pivot =
            new Vector2(0.5f, 0f);

        healthBar.anchoredPosition =
            new Vector2(
                0f,
                HealthBarBottomGap
            );

        healthBar.sizeDelta =
            new Vector2(
                -(horizontalInset * 2f),
                HealthBarHeight
            );

        healthBar.localScale =
            Vector3.one;
    }

    private void ResolveTemporaryBackground()
    {
        GameObject backgroundObject =
            GameObject.Find(
                "BattleBackgroundWorld"
            );

        if (backgroundObject == null)
        {
            return;
        }

        SpriteRenderer renderer =
            backgroundObject.GetComponent<
                SpriteRenderer
            >();

        if (renderer == null)
        {
            return;
        }

        temporaryDungeonBackground =
            renderer.sprite;

        /*
         * The new battle arena owns its two backgrounds. Leaving the old world
         * background active would make the prototype look like one continuous
         * scene behind both framed sections.
         */
        renderer.enabled = false;
    }

    private void ResolveFrameSprites()
    {
        cornerPiece =
            cornerPieceOverride;

        normalPiece =
            normalPieceOverride;

        if (cornerPiece != null &&
            normalPiece != null)
        {
            return;
        }

        BoardVisuals boardVisuals =
            FindFirstObjectByType<
                BoardVisuals
            >();

        if (boardVisuals != null)
        {
            Transform frameRoot =
                boardVisuals.transform.Find(
                    "BoardFrame"
                );

            if (frameRoot != null)
            {
                SpriteRenderer cornerRenderer =
                    FindSpriteRenderer(
                        frameRoot,
                        "TopLeftCorner"
                    );

                SpriteRenderer normalRenderer =
                    FindFirstEdgeRenderer(
                        frameRoot
                    );

                if (cornerPiece == null &&
                    cornerRenderer != null)
                {
                    cornerPiece =
                        cornerRenderer.sprite;
                }

                if (normalPiece == null &&
                    normalRenderer != null)
                {
                    normalPiece =
                        normalRenderer.sprite;
                }
            }
        }

        if (cornerPiece != null &&
            normalPiece != null)
        {
            return;
        }

        /*
         * Clean-checkout fallback: the current scene already has the modular
         * BottomHUD frame assigned. Reusing it keeps this layout immediately
         * testable even before the new board-frame assignments are committed.
         */
        GameObject bottomHud =
            GameObject.Find("BottomHUD");

        if (bottomHud == null)
        {
            return;
        }

        Transform generatedFrame =
            bottomHud.transform.Find(
                "GeneratedBottomHudFrame"
            );

        if (generatedFrame == null)
        {
            return;
        }

        Image cornerImage =
            FindImage(
                generatedFrame,
                "TopLeftCorner"
            );

        Image normalImage =
            FindImage(
                generatedFrame,
                "TopEdge"
            );

        if (cornerPiece == null &&
            cornerImage != null)
        {
            cornerPiece =
                cornerImage.sprite;
        }

        if (normalPiece == null &&
            normalImage != null)
        {
            normalPiece =
                normalImage.sprite;
        }
    }

    private void BuildFrame(
        RectTransform target,
        string frameName)
    {
        if (cornerPiece == null ||
            normalPiece == null)
        {
            CreateFallbackFrame(
                target,
                frameName
            );
            return;
        }

        RectTransform frameRoot =
            CreateRectTransform(
                frameName,
                target
            );

        StretchToParent(
            frameRoot,
            Vector2.zero,
            Vector2.zero
        );

        frameRoot.SetAsLastSibling();

        CreateFrameCorner(
            frameRoot,
            "TopLeftCorner",
            new Vector2(0f, 1f),
            new Vector2(
                FrameCornerSize * 0.5f,
                -FrameCornerSize * 0.5f
            ),
            new Vector3(1f, 1f, 1f)
        );

        CreateFrameCorner(
            frameRoot,
            "TopRightCorner",
            new Vector2(1f, 1f),
            new Vector2(
                -FrameCornerSize * 0.5f,
                -FrameCornerSize * 0.5f
            ),
            new Vector3(-1f, 1f, 1f)
        );

        CreateFrameCorner(
            frameRoot,
            "BottomLeftCorner",
            new Vector2(0f, 0f),
            new Vector2(
                FrameCornerSize * 0.5f,
                FrameCornerSize * 0.5f
            ),
            new Vector3(1f, -1f, 1f)
        );

        CreateFrameCorner(
            frameRoot,
            "BottomRightCorner",
            new Vector2(1f, 0f),
            new Vector2(
                -FrameCornerSize * 0.5f,
                FrameCornerSize * 0.5f
            ),
            new Vector3(-1f, -1f, 1f)
        );

        CreateHorizontalFrameEdge(
            frameRoot,
            "TopEdge",
            true
        );

        CreateHorizontalFrameEdge(
            frameRoot,
            "BottomEdge",
            false
        );

        CreateVerticalFrameEdge(
            frameRoot,
            "LeftEdge",
            true
        );

        CreateVerticalFrameEdge(
            frameRoot,
            "RightEdge",
            false
        );
    }

    private void CreateFrameCorner(
        RectTransform parent,
        string objectName,
        Vector2 anchor,
        Vector2 position,
        Vector3 scale)
    {
        Image image =
            CreateImage(
                objectName,
                parent,
                cornerPiece
            );

        RectTransform rect =
            image.rectTransform;

        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot =
            new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta =
            new Vector2(
                FrameCornerSize,
                FrameCornerSize
            );
        rect.localScale = scale;

        image.type =
            Image.Type.Simple;
        image.raycastTarget = false;
    }

    private void CreateHorizontalFrameEdge(
        RectTransform parent,
        string objectName,
        bool top)
    {
        Image image =
            CreateImage(
                objectName,
                parent,
                normalPiece
            );

        RectTransform rect =
            image.rectTransform;

        float yAnchor =
            top
                ? 1f
                : 0f;

        rect.anchorMin =
            new Vector2(0f, yAnchor);
        rect.anchorMax =
            new Vector2(1f, yAnchor);
        rect.pivot =
            new Vector2(0.5f, 0.5f);
        rect.offsetMin =
            new Vector2(
                FrameCornerSize,
                top
                    ? -FrameThickness
                    : 0f
            );
        rect.offsetMax =
            new Vector2(
                -FrameCornerSize,
                top
                    ? 0f
                    : FrameThickness
            );

        if (!top)
        {
            rect.localScale =
                new Vector3(1f, -1f, 1f);
        }

        image.type =
            Image.Type.Tiled;
        image.raycastTarget = false;
    }

    private void CreateVerticalFrameEdge(
        RectTransform parent,
        string objectName,
        bool left)
    {
        Image image =
            CreateImage(
                objectName,
                parent,
                normalPiece
            );

        RectTransform rect =
            image.rectTransform;

        rect.anchorMin =
            new Vector2(
                left ? 0f : 1f,
                0.5f
            );
        rect.anchorMax =
            rect.anchorMin;
        rect.pivot =
            new Vector2(0.5f, 0.5f);
        rect.anchoredPosition =
            new Vector2(
                left
                    ? FrameThickness * 0.5f
                    : -FrameThickness * 0.5f,
                0f
            );

        Rect targetRect =
            parent.rect;

        float verticalLength =
            Mathf.Max(
                1f,
                targetRect.height -
                FrameCornerSize * 2f
            );

        rect.sizeDelta =
            new Vector2(
                verticalLength,
                FrameThickness
            );
        rect.localRotation =
            Quaternion.Euler(
                0f,
                0f,
                left
                    ? 90f
                    : -90f
            );

        image.type =
            Image.Type.Tiled;
        image.raycastTarget = false;
    }

    private static void CreateFallbackFrame(
        RectTransform target,
        string frameName)
    {
        RectTransform frame =
            CreateRectTransform(
                frameName,
                target
            );

        StretchToParent(
            frame,
            Vector2.zero,
            Vector2.zero
        );

        frame.SetAsLastSibling();

        Color fallbackColor =
            new Color32(93, 50, 104, 255);

        CreateSolidEdge(
            frame,
            "Top",
            new Vector2(0f, 1f),
            new Vector2(1f, 1f),
            new Vector2(0f, -FrameThickness),
            Vector2.zero,
            fallbackColor
        );

        CreateSolidEdge(
            frame,
            "Bottom",
            new Vector2(0f, 0f),
            new Vector2(1f, 0f),
            Vector2.zero,
            new Vector2(0f, FrameThickness),
            fallbackColor
        );

        CreateSolidEdge(
            frame,
            "Left",
            new Vector2(0f, 0f),
            new Vector2(0f, 1f),
            Vector2.zero,
            new Vector2(FrameThickness, 0f),
            fallbackColor
        );

        CreateSolidEdge(
            frame,
            "Right",
            new Vector2(1f, 0f),
            new Vector2(1f, 1f),
            new Vector2(-FrameThickness, 0f),
            Vector2.zero,
            fallbackColor
        );
    }

    private static void CreateSolidEdge(
        RectTransform parent,
        string objectName,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 offsetMin,
        Vector2 offsetMax,
        Color color)
    {
        Image image =
            CreateImage(
                objectName,
                parent,
                null
            );

        RectTransform rect =
            image.rectTransform;
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
        image.color = color;
        image.raycastTarget = false;
    }

    private static void AnchorAtPoint(
        RectTransform rect,
        Vector2 anchor,
        Vector2 size,
        Vector2 offset)
    {
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot =
            new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = offset;
        rect.sizeDelta = size;
        rect.localScale =
            Vector3.one;
    }

    private static RectTransform FindRectTransform(
        Transform root,
        string objectName)
    {
        if (root == null)
        {
            return null;
        }

        RectTransform[] descendants =
            root.GetComponentsInChildren<
                RectTransform
            >(true);

        foreach (RectTransform descendant
                 in descendants)
        {
            if (descendant.name == objectName)
            {
                return descendant;
            }
        }

        return null;
    }

    private static SpriteRenderer FindSpriteRenderer(
        Transform root,
        string objectName)
    {
        SpriteRenderer[] renderers =
            root.GetComponentsInChildren<
                SpriteRenderer
            >(true);

        foreach (SpriteRenderer renderer
                 in renderers)
        {
            if (renderer.name == objectName)
            {
                return renderer;
            }
        }

        return null;
    }

    private static SpriteRenderer FindFirstEdgeRenderer(
        Transform root)
    {
        SpriteRenderer[] renderers =
            root.GetComponentsInChildren<
                SpriteRenderer
            >(true);

        foreach (SpriteRenderer renderer
                 in renderers)
        {
            if (renderer.name.StartsWith(
                    "TopEdge_"
                ))
            {
                return renderer;
            }
        }

        return null;
    }

    private static Image FindImage(
        Transform root,
        string objectName)
    {
        Image[] images =
            root.GetComponentsInChildren<
                Image
            >(true);

        foreach (Image image
                 in images)
        {
            if (image.name == objectName)
            {
                return image;
            }
        }

        return null;
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
            rectObject.GetComponent<
                RectTransform
            >();

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
            imageObject.GetComponent<
                RectTransform
            >();

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
        RectTransform rect,
        Vector2 offsetMin,
        Vector2 offsetMax)
    {
        rect.anchorMin =
            Vector2.zero;
        rect.anchorMax =
            Vector2.one;
        rect.offsetMin =
            offsetMin;
        rect.offsetMax =
            offsetMax;
        rect.localScale =
            Vector3.one;
    }
}
