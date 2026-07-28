using UnityEngine;

[DefaultExecutionOrder(-100)]
[RequireComponent(typeof(BoardController))]
public sealed class BoardVisuals : MonoBehaviour
{
    [Header("Board Frame")]

    [SerializeField]
    private Sprite boardFrameSprite;

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

    private Texture2D runtimeTexture;
    private Sprite runtimeSquareSprite;


    public float OuterLocalWidth =>
        boardFrameSprite != null
            ? boardFrameSprite.bounds.size.x
            : board.LocalBoardWidth;

    public float OuterLocalHeight =>
        boardFrameSprite != null
            ? boardFrameSprite.bounds.size.y
            : board.LocalBoardHeight;


    private void Awake()
    {
        board = GetComponent<BoardController>();

        CreateSquareSprite();
        CreateBoardFrame();
        CreateCellTiles();
        CreateBoardMask();
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
         * Dark backing behind tiles. This covers the small
         * transparent gaps that may exist between tile sprites.
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

        if (boardFrameSprite == null)
        {
            Debug.LogWarning(
                "BoardVisuals has no board frame sprite.",
                this
            );

            return;
        }

        GameObject frameObject =
            new GameObject("BoardFrame");

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

        frameRenderer.sprite =
            boardFrameSprite;

        frameRenderer.color =
            boardFrameColor;

        /*
         * The frame renders above gems. Its transparent
         * center means it only visually covers the border.
         */
        frameRenderer.sortingLayerName =
            boardFrameSortingLayer;

        frameRenderer.sortingOrder =
            boardFrameSortingOrder;

        /*
         * The frame extends outside the board mask, so it
         * must not be restricted by that mask.
         */
        frameRenderer.maskInteraction =
            SpriteMaskInteraction.None;
    }

    private void CreateCellTiles()
    {
        if (!HasValidCellTileSprite())
        {
            Debug.LogWarning(
                "BoardVisuals has no valid cell tile sprites.",
                this
            );

            return;
        }

        GameObject tileContainer =
            new GameObject("CellTiles");

        tileContainer.transform.SetParent(
            transform,
            false
        );

        int selectedSeed =
            randomizeTileLayoutEachRun
                ? System.Environment.TickCount
                : tileLayoutSeed;

        /*
         * Use a separate random generator so visual tile
         * selection cannot change gameplay randomness.
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

                tileRenderer.sortingOrder = 2;

                tileRenderer.maskInteraction =
                    SpriteMaskInteraction
                        .VisibleInsideMask;
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
         * Multiple attempts reduce visible clusters while
         * still allowing layouts to remain naturally random.
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
         * This can happen when there is only one valid
         * variation or when every alternative conflicts.
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