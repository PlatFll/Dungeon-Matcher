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
    private Sprite cellTileSprite;

    [SerializeField, Range(0.1f, 1.2f)]
    [Tooltip(
        "Size of each tile relative to one board cell."
    )]
    private float cellTileScale = 1f;

    [SerializeField]
    private Color cellTileColor =
        Color.white;

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

        /*
         * The frame is imported at 64 PPU, so its native
         * size is already exactly 7.5 by 8.5 world units.
         */
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
        if (cellTileSprite == null)
        {
            Debug.LogWarning(
                "BoardVisuals has no cell tile sprite.",
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

        Vector2 spriteSize =
            cellTileSprite.bounds.size;

        if (spriteSize.x <= 0f ||
            spriteSize.y <= 0f)
        {
            Debug.LogError(
                "The cell tile sprite has invalid bounds.",
                this
            );

            return;
        }

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
                    cellTileSprite;

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