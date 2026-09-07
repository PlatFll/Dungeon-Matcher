using UnityEngine;

[DefaultExecutionOrder(-90)]
[DisallowMultipleComponent]
[RequireComponent(typeof(BoardController))]
public sealed class TileCanvasBoardFrameController : MonoBehaviour
{
    private const string BoardFrameName =
        "BoardFrame";

    private BoardController board;

    public bool IsUsingTileCanvasFrame
    {
        get;
        private set;
    }

    public Sprite CornerSprite
    {
        get;
        private set;
    }

    public Sprite NormalSprite
    {
        get;
        private set;
    }

    public float OuterLocalWidth
    {
        get;
        private set;
    }

    public float OuterLocalHeight
    {
        get;
        private set;
    }

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.AfterSceneLoad
    )]
    private static void InstallOnBoards()
    {
        BoardController[] boards =
            FindObjectsByType<BoardController>(
                FindObjectsSortMode.None
            );

        foreach (BoardController boardController
                 in boards)
        {
            if (boardController == null ||
                boardController.TryGetComponent(
                    out TileCanvasBoardFrameController _
                ))
            {
                continue;
            }

            boardController.gameObject.AddComponent<
                TileCanvasBoardFrameController
            >();
        }
    }

    private void Awake()
    {
        board =
            GetComponent<BoardController>();

        TryRebuildFrame();
    }

    private void OnEnable()
    {
        if (board == null)
        {
            board =
                GetComponent<BoardController>();
        }

        if (!IsUsingTileCanvasFrame)
        {
            TryRebuildFrame();
        }
    }

    private void TryRebuildFrame()
    {
        if (board == null)
        {
            return;
        }

        Transform existingFrame =
            transform.Find(
                BoardFrameName
            );

        if (existingFrame == null)
        {
            return;
        }

        SpriteRenderer cornerRenderer =
            FindRenderer(
                existingFrame,
                "TopLeftCorner"
            );

        SpriteRenderer normalRenderer =
            FindFirstNormalRenderer(
                existingFrame
            );

        if (cornerRenderer == null ||
            normalRenderer == null)
        {
            return;
        }

        Sprite cornerSprite =
            cornerRenderer.sprite;

        Sprite normalSprite =
            normalRenderer.sprite;

        if (!TileCanvasFrameSpriteUtility
                .IsSquareTileCanvasPair(
                    cornerSprite,
                    normalSprite
                ))
        {
            return;
        }

        Color frameColor =
            normalRenderer.color;

        string sortingLayer =
            normalRenderer.sortingLayerName;

        int sortingOrder =
            normalRenderer.sortingOrder;

        CornerSprite =
            cornerSprite;

        NormalSprite =
            normalSprite;

        existingFrame.name =
            BoardFrameName +
            "_LegacyDisabled";

        existingFrame.gameObject.SetActive(false);

        CreateTileCanvasFrame(
            cornerSprite,
            normalSprite,
            frameColor,
            sortingLayer,
            sortingOrder
        );

        float maximumVisibleMarginRatio =
            Mathf.Max(
                TileCanvasFrameSpriteUtility
                    .GetNormalVisibleThicknessRatio(
                        normalSprite
                    ),
                TileCanvasFrameSpriteUtility
                    .GetMaximumCornerExtentRatio(
                        cornerSprite
                    )
            );

        float visibleMargin =
            board.CellSize *
            maximumVisibleMarginRatio;

        OuterLocalWidth =
            board.LocalBoardWidth +
            visibleMargin *
            2f;

        OuterLocalHeight =
            board.LocalBoardHeight +
            visibleMargin *
            2f;

        IsUsingTileCanvasFrame = true;

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

    private void CreateTileCanvasFrame(
        Sprite cornerSprite,
        Sprite normalSprite,
        Color frameColor,
        string sortingLayer,
        int sortingOrder)
    {
        GameObject frameRoot =
            new GameObject(
                BoardFrameName
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

        float halfCellSize =
            cellSize *
            0.5f;

        float halfBoardWidth =
            board.LocalBoardWidth *
            0.5f;

        float halfBoardHeight =
            board.LocalBoardHeight *
            0.5f;

        float normalScale =
            GetCanvasScale(
                normalSprite,
                cellSize
            );

        float cornerScale =
            GetCanvasScale(
                cornerSprite,
                cellSize
            );

        float normalInnerPadding =
            cellSize *
            TileCanvasFrameSpriteUtility
                .GetNormalInnerPaddingRatio(
                    normalSprite
                );

        float leftCanvasCenterX =
            -halfBoardWidth -
            halfCellSize +
            normalInnerPadding;

        float rightCanvasCenterX =
            halfBoardWidth +
            halfCellSize -
            normalInnerPadding;

        float topCanvasCenterY =
            halfBoardHeight +
            halfCellSize -
            normalInnerPadding;

        float bottomCanvasCenterY =
            -halfBoardHeight -
            halfCellSize +
            normalInnerPadding;

        for (int column = 0;
             column < board.Width;
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
                normalSprite,
                new Vector2(
                    cellCenterX,
                    topCanvasCenterY
                ),
                normalScale,
                -90f,
                frameColor,
                sortingLayer,
                sortingOrder
            );

            CreateFramePiece(
                frameRoot.transform,
                $"BottomEdge_{column}",
                normalSprite,
                new Vector2(
                    cellCenterX,
                    bottomCanvasCenterY
                ),
                normalScale,
                90f,
                frameColor,
                sortingLayer,
                sortingOrder
            );
        }

        for (int row = 0;
             row < board.Height;
             row++)
        {
            float cellCenterY =
                board.GetCellLocalPosition(
                    0,
                    row
                ).y;

            CreateFramePiece(
                frameRoot.transform,
                $"LeftEdge_{row}",
                normalSprite,
                new Vector2(
                    leftCanvasCenterX,
                    cellCenterY
                ),
                normalScale,
                0f,
                frameColor,
                sortingLayer,
                sortingOrder
            );

            CreateFramePiece(
                frameRoot.transform,
                $"RightEdge_{row}",
                normalSprite,
                new Vector2(
                    rightCanvasCenterX,
                    cellCenterY
                ),
                normalScale,
                180f,
                frameColor,
                sortingLayer,
                sortingOrder
            );
        }

        CreateOutsideCorner(
            frameRoot.transform,
            "BottomLeftCorner",
            cornerSprite,
            new Vector2(
                -halfBoardWidth -
                halfCellSize,
                -halfBoardHeight -
                halfCellSize
            ),
            0f,
            cornerScale,
            frameColor,
            sortingLayer,
            sortingOrder
        );

        CreateOutsideCorner(
            frameRoot.transform,
            "BottomRightCorner",
            cornerSprite,
            new Vector2(
                halfBoardWidth +
                halfCellSize,
                -halfBoardHeight -
                halfCellSize
            ),
            90f,
            cornerScale,
            frameColor,
            sortingLayer,
            sortingOrder
        );

        CreateOutsideCorner(
            frameRoot.transform,
            "TopRightCorner",
            cornerSprite,
            new Vector2(
                halfBoardWidth +
                halfCellSize,
                halfBoardHeight +
                halfCellSize
            ),
            180f,
            cornerScale,
            frameColor,
            sortingLayer,
            sortingOrder
        );

        CreateOutsideCorner(
            frameRoot.transform,
            "TopLeftCorner",
            cornerSprite,
            new Vector2(
                -halfBoardWidth -
                halfCellSize,
                halfBoardHeight +
                halfCellSize
            ),
            -90f,
            cornerScale,
            frameColor,
            sortingLayer,
            sortingOrder
        );
    }

    private void CreateOutsideCorner(
        Transform parent,
        string objectName,
        Sprite sprite,
        Vector2 nominalCanvasCenter,
        float rotationDegrees,
        float uniformScale,
        Color frameColor,
        string sortingLayer,
        int sortingOrder)
    {
        float canvasPixels =
            TileCanvasFrameSpriteUtility
                .GetCanvasPixels(
                    sprite
                );

        Vector2 innerPadding =
            Vector2.zero;

        if (canvasPixels > 0f)
        {
            innerPadding =
                new Vector2(
                    Mathf.Max(
                        0f,
                        canvasPixels -
                        sprite.rect.xMax
                    ),
                    Mathf.Max(
                        0f,
                        canvasPixels -
                        sprite.rect.yMax
                    )
                ) /
                canvasPixels *
                board.CellSize;
        }

        Vector2 adjustedCanvasCenter =
            nominalCanvasCenter +
            TileCanvasFrameSpriteUtility
                .Rotate(
                    innerPadding,
                    rotationDegrees
                );

        CreateFramePiece(
            parent,
            objectName,
            sprite,
            adjustedCanvasCenter,
            uniformScale,
            rotationDegrees,
            frameColor,
            sortingLayer,
            sortingOrder
        );
    }

    private static float GetCanvasScale(
        Sprite sprite,
        float targetCanvasSize)
    {
        if (sprite == null ||
            sprite.pixelsPerUnit <= 0f)
        {
            return 1f;
        }

        float canvasPixels =
            TileCanvasFrameSpriteUtility
                .GetCanvasPixels(
                    sprite
                );

        if (canvasPixels <= 0f)
        {
            return 1f;
        }

        float nativeCanvasWorldSize =
            canvasPixels /
            sprite.pixelsPerUnit;

        return
            targetCanvasSize /
            nativeCanvasWorldSize;
    }

    private static void CreateFramePiece(
        Transform parent,
        string objectName,
        Sprite sprite,
        Vector2 logicalCanvasCenter,
        float uniformScale,
        float rotationDegrees,
        Color frameColor,
        string sortingLayer,
        int sortingOrder)
    {
        GameObject pieceObject =
            new GameObject(
                objectName
            );

        pieceObject.transform.SetParent(
            parent,
            false
        );

        Vector2 pivotOffset =
            Vector2.zero;

        if (sprite != null &&
            sprite.pixelsPerUnit > 0f)
        {
            pivotOffset =
                TileCanvasFrameSpriteUtility
                    .GetPivotOffsetPixels(
                        sprite
                    ) /
                sprite.pixelsPerUnit *
                uniformScale;

            pivotOffset =
                TileCanvasFrameSpriteUtility
                    .Rotate(
                        pivotOffset,
                        rotationDegrees
                    );
        }

        pieceObject.transform.localPosition =
            new Vector3(
                logicalCanvasCenter.x +
                pivotOffset.x,
                logicalCanvasCenter.y +
                pivotOffset.y,
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
            pieceObject.AddComponent<
                SpriteRenderer
            >();

        renderer.sprite =
            sprite;

        renderer.color =
            frameColor;

        renderer.sortingLayerName =
            sortingLayer;

        renderer.sortingOrder =
            sortingOrder;

        renderer.maskInteraction =
            SpriteMaskInteraction.None;
    }

    private static SpriteRenderer FindRenderer(
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

    private static SpriteRenderer FindFirstNormalRenderer(
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
                ) ||
                renderer.name.StartsWith(
                    "BottomEdge_"
                ) ||
                renderer.name.StartsWith(
                    "LeftEdge_"
                ) ||
                renderer.name.StartsWith(
                    "RightEdge_"
                ))
            {
                return renderer;
            }
        }

        return null;
    }
}
