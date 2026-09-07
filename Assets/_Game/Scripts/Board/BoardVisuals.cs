using UnityEngine;
using UnityEngine.Serialization;

[DefaultExecutionOrder(-100)]
[RequireComponent(typeof(BoardController))]
public sealed class BoardVisuals : MonoBehaviour
{
    [Header("Board Frame")]

    [SerializeField]
    [Tooltip(
        "Top-left L-shaped corner sprite. Designed for the new 80x80 board " +
        "corner art. It is mirrored automatically for the other three corners."
    )]
    private Sprite cornerPiece;

    [SerializeField]
    [Tooltip(
        "Straight top-edge sprite. Designed for the new 64x16 board frame " +
        "piece. It is repeated without stretching and rotated/flipped for the " +
        "other three sides."
    )]
    private Sprite normalPiece;

    /*
     * Preserve the old full-frame assignment when older scenes deserialize.
     * Once Corner Piece + Normal Piece are assigned, the modular frame is used
     * and this legacy sprite is ignored.
     */
    [FormerlySerializedAs("boardFrameSprite")]
    [SerializeField, HideInInspector]
    private Sprite legacyBoardFrameSprite;

    [SerializeField]
    private Color boardFrameColor =
        Color.white;

    [SerializeField]
    private string boardFrameSortingLayer =
        "Gems";

    [SerializeField]
    private int boardFrameSortingOrder =
        100;

    [Header("Board Background")]

    [SerializeField, Min(0f)]
    private float backgroundPadding = 0f;

    [SerializeField]
    private Color backgroundColor =
        new Color32(19, 23, 31, 255);

    [Header("Cell Tiles")]

    [SerializeField]
    [Tooltip(
        "Visual tile variations randomly assigned across " +
        "the board."
    )]
    private Sprite[] cellTileSprites;

    [SerializeField, Range(0.1f, 1.2f)]
    [Tooltip(
        "Size of each tile relative to one board cell."
    )]
    private float cellTileScale = 1f;

    [SerializeField]
    private Color cellTileColor =
        Color.white;

    [Header("Cell Tile Randomization")]

    [SerializeField]
    [Tooltip(
        "When enabled, a different tile arrangement is " +
        "generated each time the scene starts."
    )]
    private bool randomizeTileLayoutEachRun =
        true;

    [SerializeField]
    [Tooltip(
        "Used when Randomize Tile Layout Each Run is disabled."
    )]
    private int tileLayoutSeed =
        12345;

    [SerializeField]
    [Tooltip(
        "Tries to prevent identical tiles from appearing " +
        "directly beside or underneath one another."
    )]
    private bool avoidAdjacentDuplicateTiles =
        true;

    private BoardController board;

    private const string BoardFrameContainerName =
        "BoardFrame";

    private const string CellTileContainerName =
        "CellTiles";

    private Texture2D runtimeTexture;
    private Sprite runtimeSquareSprite;

    public float OuterLocalWidth
    {
        get
        {
            EnsureBoardReference();

            if (board == null)
            {
                return 0f;
            }

            if (HasValidModularFrame())
            {
                return
                    board.LocalBoardWidth +
                    GetFrameThickness() * 2f;
            }

            return legacyBoardFrameSprite != null
                ? legacyBoardFrameSprite.bounds.size.x
                : board.LocalBoardWidth;
        }
    }

    public float OuterLocalHeight
    {
        get
        {
            EnsureBoardReference();

            if (board == null)
            {
                return 0f;
            }

            if (HasValidModularFrame())
            {
                return
                    board.LocalBoardHeight +
                    GetFrameThickness() * 2f;
            }

            return legacyBoardFrameSprite != null
                ? legacyBoardFrameSprite.bounds.size.y
                : board.LocalBoardHeight;
        }
    }

    private void Awake()
    {
        EnsureBoardReference();

        CreateSquareSprite();
        CreateBoardFrame();
        CreateBoardMask();
        CreateCellTiles();
    }

    private void EnsureBoardReference()
    {
        if (board == null)
        {
            board = GetComponent<BoardController>();
        }
    }

    private void CreateSquareSprite()
    {
        runtimeTexture = new Texture2D(
            1,
            1,
            TextureFormat.RGBA32,
            false
        );

        runtimeTexture.name =
            "Runtime Board Square Texture";

        runtimeTexture.filterMode =
            FilterMode.Point;

        runtimeTexture.wrapMode =
            TextureWrapMode.Clamp;

        runtimeTexture.SetPixel(
            0,
            0,
            Color.white
        );

        runtimeTexture.Apply();

        runtimeSquareSprite = Sprite.Create(
            runtimeTexture,
            new Rect(0f, 0f, 1f, 1f),
            new Vector2(0.5f, 0.5f),
            1f
        );

        runtimeSquareSprite.name =
            "Runtime Board Square Sprite";
    }

    private void CreateBoardFrame()
    {
        /*
         * Dark backing behind tiles. This covers the small transparent gaps
         * that may exist between tile sprites.
         */
        CreateSpriteObject(
            "BoardBackground",
            new Vector2(
                board.LocalBoardWidth +
                backgroundPadding * 2f,

                board.LocalBoardHeight +
                backgroundPadding * 2f
            ),
            backgroundColor,
            1
        );

        RemoveExistingBoardFrame();

        if (HasValidModularFrame())
        {
            CreateModularBoardFrame();
            return;
        }

        if (legacyBoardFrameSprite != null)
        {
            CreateLegacyBoardFrame();
            return;
        }

        Debug.LogWarning(
            "BoardVisuals needs both Corner Piece and Normal Piece for the " +
            "modular board frame.",
            this
        );
    }

    private bool HasValidModularFrame()
    {
        if (cornerPiece == null ||
            normalPiece == null)
        {
            return false;
        }

        Vector2 normalSize =
            normalPiece.bounds.size;

        Vector2 cornerSize =
            cornerPiece.bounds.size;

        return
            normalSize.x > 0f &&
            normalSize.y > 0f &&
            cornerSize.x > 0f &&
            cornerSize.y > 0f;
    }

    private float GetFrameThickness()
    {
        if (board == null ||
            normalPiece == null)
        {
            return 0f;
        }

        Vector2 normalSize =
            normalPiece.bounds.size;

        if (normalSize.x <= 0f)
        {
            return 0f;
        }

        /*
         * The normal piece spans exactly one board cell horizontally. Its
         * aspect ratio therefore defines the frame thickness. With the intended
         * 64x16 art this becomes 16/64 = one quarter of a cell.
         *
         * Deriving the value from the sprite ratio also keeps the layout correct
         * if the import PPU is accidentally changed, although 64 PPU is still
         * recommended for pixel-perfect rendering.
         */
        return
            board.CellSize *
            (normalSize.y / normalSize.x);
    }

    private void CreateModularBoardFrame()
    {
        if (board.Width < 2 ||
            board.Height < 2)
        {
            Debug.LogWarning(
                "The modular board frame is designed for boards at least 2x2. " +
                "The current board will still be drawn, but corner pieces may " +
                "overlap.",
                this
            );
        }

        GameObject frameRoot =
            new GameObject(
                BoardFrameContainerName
            );

        frameRoot.transform.SetParent(
            transform,
            false
        );

        frameRoot.transform.localPosition =
            Vector3.zero;

        frameRoot.transform.localScale =
            Vector3.one;

        float cellSize =
            board.CellSize;

        float frameThickness =
            GetFrameThickness();

        float cornerTargetSize =
            cellSize +
            frameThickness;

        float normalScale =
            cellSize /
            normalPiece.bounds.size.x;

        float cornerScale =
            cornerTargetSize /
            cornerPiece.bounds.size.x;

        float halfBoardWidth =
            board.LocalBoardWidth * 0.5f;

        float halfBoardHeight =
            board.LocalBoardHeight * 0.5f;

        float halfFrameThickness =
            frameThickness * 0.5f;

        float leftCellCenterX =
            -halfBoardWidth +
            cellSize * 0.5f;

        float rightCellCenterX =
            halfBoardWidth -
            cellSize * 0.5f;

        float bottomCellCenterY =
            -halfBoardHeight +
            cellSize * 0.5f;

        float topCellCenterY =
            halfBoardHeight -
            cellSize * 0.5f;

        /*
         * Each 80x80 corner contains the entire top/side L shape around one
         * 64x64 cell span. Its center is therefore shifted half the exterior
         * frame thickness away from that corner cell's center.
         */
        CreateFramePiece(
            frameRoot.transform,
            "TopLeftCorner",
            cornerPiece,
            new Vector2(
                leftCellCenterX -
                halfFrameThickness,

                topCellCenterY +
                halfFrameThickness
            ),
            cornerScale,
            0f,
            false,
            false
        );

        CreateFramePiece(
            frameRoot.transform,
            "TopRightCorner",
            cornerPiece,
            new Vector2(
                rightCellCenterX +
                halfFrameThickness,

                topCellCenterY +
                halfFrameThickness
            ),
            cornerScale,
            0f,
            true,
            false
        );

        CreateFramePiece(
            frameRoot.transform,
            "BottomLeftCorner",
            cornerPiece,
            new Vector2(
                leftCellCenterX -
                halfFrameThickness,

                bottomCellCenterY -
                halfFrameThickness
            ),
            cornerScale,
            0f,
            false,
            true
        );

        CreateFramePiece(
            frameRoot.transform,
            "BottomRightCorner",
            cornerPiece,
            new Vector2(
                rightCellCenterX +
                halfFrameThickness,

                bottomCellCenterY -
                halfFrameThickness
            ),
            cornerScale,
            0f,
            true,
            true
        );

        /*
         * Corners already cover the first and last cell span on each side, so
         * straight pieces are only needed for cells between those corners.
         */
        for (int column = 1;
             column < board.Width - 1;
             column++)
        {
            float cellCenterX =
                board.GetCellLocalPosition(
                    column,
                    0
                ).x;

            CreateFramePiece(
                frameRoot.transform,
                $"TopEdge_{column}",
                normalPiece,
                new Vector2(
                    cellCenterX,
                    halfBoardHeight +
                    halfFrameThickness
                ),
                normalScale,
                0f,
                false,
                false
            );

            CreateFramePiece(
                frameRoot.transform,
                $"BottomEdge_{column}",
                normalPiece,
                new Vector2(
                    cellCenterX,
                    -halfBoardHeight -
                    halfFrameThickness
                ),
                normalScale,
                0f,
                false,
                true
            );
        }

        for (int row = 1;
             row < board.Height - 1;
             row++)
        {
            float cellCenterY =
                board.GetCellLocalPosition(
                    0,
                    row
                ).y;

            /*
             * The original normal sprite is the top edge. +90 degrees maps its
             * outward-facing top to the left side; -90 maps it to the right.
             */
            CreateFramePiece(
                frameRoot.transform,
                $"LeftEdge_{row}",
                normalPiece,
                new Vector2(
                    -halfBoardWidth -
                    halfFrameThickness,
                    cellCenterY
                ),
                normalScale,
                90f,
                false,
                false
            );

            CreateFramePiece(
                frameRoot.transform,
                $"RightEdge_{row}",
                normalPiece,
                new Vector2(
                    halfBoardWidth +
                    halfFrameThickness,
                    cellCenterY
                ),
                normalScale,
                -90f,
                false,
                false
            );
        }
    }

    private void CreateLegacyBoardFrame()
    {
        GameObject frameObject =
            new GameObject(
                BoardFrameContainerName
            );

        frameObject.transform.SetParent(
            transform,
            false
        );

        frameObject.transform.localPosition =
            Vector3.zero;

        frameObject.transform.localScale =
            Vector3.one;

        SpriteRenderer frameRenderer =
            frameObject.AddComponent<SpriteRenderer>();

        ConfigureFrameRenderer(
            frameRenderer,
            legacyBoardFrameSprite
        );
    }

    private void CreateFramePiece(
        Transform parent,
        string objectName,
        Sprite sprite,
        Vector2 localPosition,
        float uniformScale,
        float rotationDegrees,
        bool flipX,
        bool flipY)
    {
        GameObject pieceObject =
            new GameObject(objectName);

        pieceObject.transform.SetParent(
            parent,
            false
        );

        pieceObject.transform.localPosition =
            new Vector3(
                localPosition.x,
                localPosition.y,
                0f
            );

        pieceObject.transform.localRotation =
            Quaternion.Euler(
                0f,
                0f,
                rotationDegrees
            );

        pieceObject.transform.localScale =
            new Vector3(
                uniformScale,
                uniformScale,
                1f
            );

        SpriteRenderer renderer =
            pieceObject.AddComponent<SpriteRenderer>();

        ConfigureFrameRenderer(
            renderer,
            sprite
        );

        renderer.flipX = flipX;
        renderer.flipY = flipY;
    }

    private void ConfigureFrameRenderer(
        SpriteRenderer renderer,
        Sprite sprite)
    {
        renderer.sprite = sprite;
        renderer.color = boardFrameColor;

        /*
         * The frame renders above gems. Its transparent interior leaves the
         * playable board visible while the decorative border stays on top.
         */
        renderer.sortingLayerName =
            boardFrameSortingLayer;

        renderer.sortingOrder =
            boardFrameSortingOrder;

        /*
         * Frame pieces extend outside the board mask and must never be clipped.
         */
        renderer.maskInteraction =
            SpriteMaskInteraction.None;
    }

    private void RemoveExistingBoardFrame()
    {
        Transform existingFrame =
            transform.Find(
                BoardFrameContainerName
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

    private void RemoveExistingCellTileContainers()
    {
        Transform[] boardDescendants =
            GetComponentsInChildren<Transform>(
                true
            );

        foreach (Transform descendant
                 in boardDescendants)
        {
            if (descendant == transform ||
                descendant.name !=
                    CellTileContainerName)
            {
                continue;
            }

            /*
             * Disable immediately so an older tile layer cannot remain visible
             * until Destroy finishes at the end of the frame.
             */
            descendant.gameObject.SetActive(false);

            if (Application.isPlaying)
            {
                Destroy(
                    descendant.gameObject
                );
            }
            else
            {
                DestroyImmediate(
                    descendant.gameObject
                );
            }
        }
    }

    private void CreateCellTiles()
    {
        /*
         * Remove any tile grid left behind by an earlier initialization or a
         * Play Mode script reload.
         */
        RemoveExistingCellTileContainers();

        if (!HasValidCellTileSprite())
        {
            Debug.LogWarning(
                "BoardVisuals has no valid cell tile sprites.",
                this
            );

            return;
        }

        GameObject tileContainer =
            new GameObject(
                CellTileContainerName
            );

        tileContainer.transform.SetParent(
            transform,
            false
        );

        int selectedSeed =
            randomizeTileLayoutEachRun
                ? System.Environment.TickCount
                : tileLayoutSeed;

        /*
         * Use a separate random generator so visual tile selection cannot change
         * gameplay randomness.
         */
        System.Random tileRandom =
            new System.Random(selectedSeed);

        Sprite[,] assignedSprites =
            new Sprite[
                board.Width,
                board.Height
            ];

        float targetTileSize =
            board.CellSize *
            cellTileScale;

        for (int row = 0;
             row < board.Height;
             row++)
        {
            for (int column = 0;
                 column < board.Width;
                 column++)
            {
                Sprite selectedSprite =
                    SelectCellTileSprite(
                        tileRandom,
                        assignedSprites,
                        column,
                        row
                    );

                if (selectedSprite == null)
                {
                    continue;
                }

                Vector2 spriteSize =
                    selectedSprite.bounds.size;

                if (spriteSize.x <= 0f ||
                    spriteSize.y <= 0f)
                {
                    Debug.LogWarning(
                        $"Cell tile sprite " +
                        $"'{selectedSprite.name}' has " +
                        $"invalid bounds.",
                        this
                    );

                    continue;
                }

                assignedSprites[
                    column,
                    row
                ] = selectedSprite;

                GameObject tileObject =
                    new GameObject(
                        $"CellTile_{column}_{row}"
                    );

                tileObject.transform.SetParent(
                    tileContainer.transform,
                    false
                );

                tileObject.transform.localPosition =
                    board.GetCellLocalPosition(
                        column,
                        row
                    );

                tileObject.transform.localScale =
                    new Vector3(
                        targetTileSize /
                        spriteSize.x,

                        targetTileSize /
                        spriteSize.y,

                        1f
                    );

                SpriteRenderer tileRenderer =
                    tileObject.AddComponent<
                        SpriteRenderer
                    >();

                tileRenderer.sprite =
                    selectedSprite;

                tileRenderer.color =
                    cellTileColor;

                tileRenderer.sortingLayerName =
                    "BoardBackground";

                tileRenderer.sortingOrder = 10;

                tileRenderer.maskInteraction =
                    SpriteMaskInteraction.None;
            }
        }
    }

    private bool HasValidCellTileSprite()
    {
        if (cellTileSprites == null ||
            cellTileSprites.Length == 0)
        {
            return false;
        }

        foreach (Sprite sprite
                 in cellTileSprites)
        {
            if (sprite != null)
            {
                return true;
            }
        }

        return false;
    }

    private Sprite SelectCellTileSprite(
        System.Random tileRandom,
        Sprite[,] assignedSprites,
        int column,
        int row)
    {
        Sprite leftSprite =
            column > 0
                ? assignedSprites[
                    column - 1,
                    row
                ]
                : null;

        Sprite lowerSprite =
            row > 0
                ? assignedSprites[
                    column,
                    row - 1
                ]
                : null;

        /*
         * Multiple attempts reduce visible clusters while still allowing layouts
         * to remain naturally random.
         */
        const int maximumSelectionAttempts = 12;

        Sprite fallbackSprite = null;

        for (int attempt = 0;
             attempt < maximumSelectionAttempts;
             attempt++)
        {
            Sprite candidate =
                cellTileSprites[
                    tileRandom.Next(
                        cellTileSprites.Length
                    )
                ];

            if (candidate == null)
            {
                continue;
            }

            fallbackSprite ??=
                candidate;

            if (!avoidAdjacentDuplicateTiles)
            {
                return candidate;
            }

            bool matchesLeft =
                candidate == leftSprite;

            bool matchesBelow =
                candidate == lowerSprite;

            if (!matchesLeft &&
                !matchesBelow)
            {
                return candidate;
            }
        }

        /*
         * This can happen when there is only one valid variation or when every
         * alternative conflicts.
         */
        if (fallbackSprite != null)
        {
            return fallbackSprite;
        }

        foreach (Sprite sprite
                 in cellTileSprites)
        {
            if (sprite != null)
            {
                return sprite;
            }
        }

        return null;
    }

    private void CreateSpriteObject(
        string objectName,
        Vector2 size,
        Color color,
        int sortingOrder)
    {
        GameObject visualObject =
            new GameObject(objectName);

        visualObject.transform.SetParent(
            transform,
            false
        );

        visualObject.transform.localPosition =
            Vector3.zero;

        visualObject.transform.localScale =
            new Vector3(
                size.x,
                size.y,
                1f
            );

        SpriteRenderer renderer =
            visualObject.AddComponent<SpriteRenderer>();

        renderer.sprite = runtimeSquareSprite;
        renderer.color = color;

        renderer.sortingLayerName =
            "BoardBackground";

        renderer.sortingOrder = sortingOrder;
    }

    private void CreateBoardMask()
    {
        GameObject maskObject =
            new GameObject("BoardClipMask");

        maskObject.transform.SetParent(
            transform,
            false
        );

        maskObject.transform.localPosition =
            Vector3.zero;

        maskObject.transform.localScale =
            new Vector3(
                board.LocalBoardWidth,
                board.LocalBoardHeight,
                1f
            );

        SpriteMask spriteMask =
            maskObject.AddComponent<SpriteMask>();

        spriteMask.sprite =
            runtimeSquareSprite;

        spriteMask.alphaCutoff = 0.01f;
    }

    private void OnDestroy()
    {
        if (runtimeSquareSprite != null)
        {
            Destroy(runtimeSquareSprite);
        }

        if (runtimeTexture != null)
        {
            Destroy(runtimeTexture);
        }
    }
}
