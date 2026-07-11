using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    [Header("Board Size")]
    [SerializeField, Min(1)]
    private int width = 7;

    [SerializeField, Min(1)]
    private int height = 8;

    [Header("Gem Layout")]
    [SerializeField, Min(0.1f)]
    private float cellSize = 1f;

    [Header("Gem Assets")]
    [SerializeField]
    private Gem gemPrefab;

    [SerializeField]
    private Sprite[] gemSprites = new Sprite[6];

    [SerializeField, Range(0.1f, 1f)]
    private float gemScale = 0.86f;

    [Header("Input")]
    [SerializeField, Min(1f)]
    private float swipeMinDistance = 40f;

    [Header("Swap Animation")]
    [SerializeField, Min(0.01f)]
    private float swapDuration = 0.15f;

    [SerializeField, Min(0f)]
    private float invalidSwapPause = 0.08f;

    [Header("Board Entrance")]
    [SerializeField, Min(0f)]
    private float initialDropExtraRows = 2f;

    [SerializeField, Min(0.01f)]
    private float initialDropDuration = 0.6f;

    [SerializeField, Min(0f)]
    private float initialRowStagger = 0.025f;

    [SerializeField, Min(0f)]
    private float initialColumnStagger = 0.015f;

    [Header("Match Animation")]
    [SerializeField, Min(0.01f)]
    private float clearDuration = 0.18f;

    [SerializeField, Min(0f)]
    private float cascadePause = 0.06f;

    [Header("Falling Animation")]
    [SerializeField, Min(0.01f)]
    private float fallDurationPerCell = 0.075f;

    [SerializeField, Min(0.01f)]
    private float minimumFallDuration = 0.14f;

    [SerializeField, Min(0.01f)]
    private float maximumFallDuration = 0.45f;

    [Header("Reshuffle")]
    [SerializeField, Min(0f)]
    private float reshufflePause = 0.25f;

    [SerializeField, Min(0.01f)]
    private float reshuffleDuration = 0.45f;

    [SerializeField, Min(0f)]
    private float reshuffleStagger = 0.04f;

    [SerializeField, Min(1)]
    private int maximumReshuffleAttempts = 250;

    private Gem[,] gems;
    private Gem selectedGem;

    private Gem pointerStartGem;
    private Vector2 pointerStartPosition;

    private bool isBusy;

    private Texture2D temporaryTexture;
    private Sprite temporarySprite;

    private class GemMove
    {
        public Gem Gem;
        public Vector3 StartPosition;
        public Vector3 TargetPosition;
        public float Delay;
        public float Duration;
    }

    private class ClearVisual
    {
        public Gem Gem;
        public SpriteRenderer SpriteRenderer;
        public Vector3 StartScale;
        public Color StartColor;
    }

    private void Start()
    {
        if (!ValidateGemAssets())
        {
            enabled = false;
            return;
        }

        StartCoroutine(InitializeBoard());
    }

    private bool ValidateGemAssets()
    {
        int requiredSpriteCount =
            System.Enum.GetValues(typeof(GemType)).Length;

        if (gemPrefab == null)
        {
            Debug.LogError(
                "BoardController requires a Gem prefab.",
                this
            );

            return false;
        }

        if (gemSprites == null ||
            gemSprites.Length != requiredSpriteCount)
        {
            Debug.LogError(
                $"Gem Sprites must contain exactly " +
                $"{requiredSpriteCount} sprites.",
                this
            );

            return false;
        }

        for (int index = 0;
             index < gemSprites.Length;
             index++)
        {
            if (gemSprites[index] != null)
            {
                continue;
            }

            Debug.LogError(
                $"Gem sprite element {index} is empty.",
                this
            );

            return false;
        }

        return true;
    }

    private IEnumerator InitializeBoard()
    {
        isBusy = true;

        List<GemMove> entranceMoves =
            GenerateStartingBoard();

        yield return AnimateGemMoves(entranceMoves);

        if (!HasAvailableMove())
        {
            yield return ReshuffleBoard();
        }

        isBusy = false;

        Debug.Log("Board entrance complete.");
    }



    private List<GemMove> GenerateStartingBoard()
    {
        gems = new Gem[width, height];

        List<GemMove> entranceMoves =
            new List<GemMove>();

        float dropDistance =
            (height + initialDropExtraRows) *
            cellSize;

        for (int row = 0; row < height; row++)
        {
            for (int column = 0;
                 column < width;
                 column++)
            {
                GemType gemType =
                    GetStartingGemType(
                        column,
                        row
                    );

                Vector3 targetPosition =
                    GetLocalPosition(
                        column,
                        row
                    );

                Vector3 startingPosition =
                    targetPosition +
                    Vector3.up * dropDistance;

                Gem gem = CreateGem(
                    column,
                    row,
                    gemType,
                    startingPosition
                );

                entranceMoves.Add(
                    new GemMove
                    {
                        Gem = gem,
                        StartPosition =
                            startingPosition,
                        TargetPosition =
                            targetPosition,
                        Delay =
                            row *
                            initialRowStagger +
                            column *
                            initialColumnStagger,
                        Duration =
                            initialDropDuration
                    }
                );
            }
        }

        return entranceMoves;
    }

    private Gem CreateGem(
        int column,
        int row,
        GemType gemType,
        Vector3 localPosition)
    {
        Gem gem = Instantiate(
            gemPrefab,
            transform
        );

        gem.gameObject.layer =
            LayerMask.NameToLayer("Default");

        gem.transform.localPosition =
            localPosition;

        SpriteRenderer spriteRenderer;

        if (!gem.TryGetComponent(out spriteRenderer))
        {
            spriteRenderer =
                gem.gameObject.AddComponent<SpriteRenderer>();
        }

        spriteRenderer.sortingLayerName = "Gems";
        spriteRenderer.sortingOrder = 0;

        BoxCollider2D boxCollider;

        if (!gem.TryGetComponent(out boxCollider))
        {
            boxCollider =
                gem.gameObject.AddComponent<BoxCollider2D>();
        }

        boxCollider.enabled = true;
        boxCollider.isTrigger = false;
        boxCollider.offset = Vector2.zero;
        boxCollider.size = Vector2.one;

        gem.Initialize(
            this,
            column,
            row,
            gemType,
            gemSprites[(int)gemType],
            gemScale
        );

        gems[column, row] = gem;

        return gem;
    }

    private Vector3 GetLocalPosition(
        int column,
        int row)
    {
        float startX =
            -(width - 1) *
            cellSize *
            0.5f;

        float startY =
            -(height - 1) *
            cellSize *
            0.5f;

        return new Vector3(
            startX + column * cellSize,
            startY + row * cellSize,
            0f
        );
    }

    private GemType GetStartingGemType(
        int column,
        int row)
    {
        List<GemType> allowedTypes =
            new List<GemType>();

        for (int typeIndex = 0;
             typeIndex < gemSprites.Length;
             typeIndex++)
        {
            GemType candidateType =
                (GemType)typeIndex;

            bool horizontalMatch =
                column >= 2 &&
                gems[column - 1, row].Type ==
                candidateType &&
                gems[column - 2, row].Type ==
                candidateType;

            bool verticalMatch =
                row >= 2 &&
                gems[column, row - 1].Type ==
                candidateType &&
                gems[column, row - 2].Type ==
                candidateType;

            if (!horizontalMatch &&
                !verticalMatch)
            {
                allowedTypes.Add(
                    candidateType
                );
            }
        }

        int randomIndex =
            Random.Range(
                0,
                allowedTypes.Count
            );

        return allowedTypes[randomIndex];
    }

    private GemType GetRandomGemType()
    {
        int typeIndex =
            Random.Range(
                0,
                gemSprites.Length
            );

        return (GemType)typeIndex;
    }

    public void BeginPointer(
        Gem gem,
        Vector2 screenPosition)
    {
        if (isBusy)
        {
            return;
        }

        pointerStartGem = gem;
        pointerStartPosition =
            screenPosition;
    }

    public void EndPointer(
        Gem gem,
        Vector2 screenPosition)
    {
        if (isBusy ||
            pointerStartGem != gem)
        {
            pointerStartGem = null;
            return;
        }

        Vector2 pointerDelta =
            screenPosition -
            pointerStartPosition;

        pointerStartGem = null;

        if (pointerDelta.magnitude >=
            swipeMinDistance)
        {
            TrySwapFromSwipe(
                gem,
                pointerDelta
            );
        }
        else
        {
            SelectGem(gem);
        }
    }

    public void SelectGem(Gem gem)
    {
        if (isBusy)
        {
            return;
        }

        if (selectedGem == gem)
        {
            ClearSelection();
            return;
        }

        if (selectedGem == null)
        {
            selectedGem = gem;
            selectedGem.SetSelected(true);
            return;
        }

        if (AreAdjacent(
            selectedGem,
            gem))
        {
            Gem firstGem =
                selectedGem;

            ClearSelection();

            StartCoroutine(
                TrySwap(
                    firstGem,
                    gem
                )
            );

            return;
        }

        selectedGem.SetSelected(false);

        selectedGem = gem;
        selectedGem.SetSelected(true);
    }

    private void TrySwapFromSwipe(
        Gem startingGem,
        Vector2 swipeDelta)
    {
        int columnDirection = 0;
        int rowDirection = 0;

        if (Mathf.Abs(swipeDelta.x) >
            Mathf.Abs(swipeDelta.y))
        {
            columnDirection =
                swipeDelta.x > 0f
                    ? 1
                    : -1;
        }
        else
        {
            rowDirection =
                swipeDelta.y > 0f
                    ? 1
                    : -1;
        }

        int targetColumn =
            startingGem.Column +
            columnDirection;

        int targetRow =
            startingGem.Row +
            rowDirection;

        Gem targetGem =
            GetGem(
                targetColumn,
                targetRow
            );

        if (targetGem == null)
        {
            return;
        }

        ClearSelection();

        StartCoroutine(
            TrySwap(
                startingGem,
                targetGem
            )
        );
    }

    private IEnumerator TrySwap(
        Gem first,
        Gem second)
    {
        isBusy = true;
        pointerStartGem = null;

        yield return AnimateSwap(
            first,
            second
        );

        HashSet<Gem> matches =
            FindMatchesFrom(
                first,
                second
            );

        if (matches.Count == 0)
        {
            yield return new WaitForSeconds(
                invalidSwapPause
            );

            yield return AnimateSwap(
                first,
                second
            );

            Debug.Log(
                "No match created. " +
                "Swap reversed."
            );
        }
        else
        {
            yield return ResolveCascades(
                matches
            );
        }

        isBusy = false;
    }

    private IEnumerator ResolveCascades(
        HashSet<Gem> matches)
    {
        int cascadeNumber = 1;

        while (matches.Count > 0)
        {
            Debug.Log(
                $"Cascade {cascadeNumber}: " +
                $"clearing " +
                $"{matches.Count} gems."
            );

            yield return ClearMatches(
                matches
            );

            if (cascadePause > 0f)
            {
                yield return
                    new WaitForSeconds(
                        cascadePause
                    );
            }

            yield return
                CollapseAndRefillBoard();

            if (cascadePause > 0f)
            {
                yield return
                    new WaitForSeconds(
                        cascadePause
                    );
            }

            matches = FindAllMatches();
            cascadeNumber++;
        }

        if (!HasAvailableMove())
        {
            yield return ReshuffleBoard();
        }

        Debug.Log("Board settled.");
    }

    private IEnumerator ClearMatches(
        HashSet<Gem> matches)
    {
        List<ClearVisual> visuals =
            new List<ClearVisual>();

        foreach (Gem gem in matches)
        {
            if (gem == null)
            {
                continue;
            }

            int column = gem.Column;
            int row = gem.Row;

            if (GetGem(column, row) == gem)
            {
                gems[column, row] = null;
            }

            BoxCollider2D collider =
                gem.GetComponent
                    <BoxCollider2D>();

            if (collider != null)
            {
                collider.enabled = false;
            }

            SpriteRenderer renderer =
                gem.GetComponent
                    <SpriteRenderer>();

            visuals.Add(
                new ClearVisual
                {
                    Gem = gem,
                    SpriteRenderer =
                        renderer,
                    StartScale =
                        gem.transform
                            .localScale,
                    StartColor =
                        renderer.color
                }
            );
        }

        float elapsedTime = 0f;

        while (elapsedTime <
               clearDuration)
        {
            float progress =
                Mathf.Clamp01(
                    elapsedTime /
                    clearDuration
                );

            float easedProgress =
                SmoothStep(progress);

            foreach (ClearVisual visual
                     in visuals)
            {
                if (visual.Gem == null)
                {
                    continue;
                }

                visual.Gem.transform
                    .localScale =
                    Vector3.Lerp(
                        visual.StartScale,
                        Vector3.zero,
                        easedProgress
                    );

                Color currentColor =
                    visual.StartColor;

                currentColor.a =
                    1f - easedProgress;

                visual.SpriteRenderer.color =
                    currentColor;
            }

            elapsedTime +=
                Time.deltaTime;

            yield return null;
        }

        foreach (ClearVisual visual
                 in visuals)
        {
            if (visual.Gem != null)
            {
                Destroy(
                    visual.Gem.gameObject
                );
            }
        }

        yield return null;
    }

    private IEnumerator
        CollapseAndRefillBoard()
    {
        List<GemMove> fallingMoves =
            new List<GemMove>();

        for (int column = 0;
             column < width;
             column++)
        {
            int targetRow = 0;

            for (int row = 0;
                 row < height;
                 row++)
            {
                Gem gem =
                    gems[column, row];

                if (gem == null)
                {
                    continue;
                }

                if (row != targetRow)
                {
                    Vector3 startPosition =
                        gem.transform
                            .localPosition;

                    Vector3 targetPosition =
                        GetLocalPosition(
                            column,
                            targetRow
                        );

                    gems[column, targetRow] =
                        gem;

                    gems[column, row] =
                        null;

                    gem.SetGridPosition(
                        column,
                        targetRow
                    );

                    fallingMoves.Add(
                        new GemMove
                        {
                            Gem = gem,
                            StartPosition =
                                startPosition,
                            TargetPosition =
                                targetPosition,
                            Delay = 0f,
                            Duration =
                                CalculateFallDuration(
                                    startPosition,
                                    targetPosition
                                )
                        }
                    );
                }

                targetRow++;
            }

            int newGemIndex = 0;

            for (int row = targetRow;
                 row < height;
                 row++)
            {
                Vector3 targetPosition =
                    GetLocalPosition(
                        column,
                        row
                    );

                Vector3 topPosition =
                    GetLocalPosition(
                        column,
                        height - 1
                    );

                Vector3 startPosition =
                    topPosition +
                    Vector3.up *
                    ((newGemIndex + 1) *
                     cellSize);

                Gem gem = CreateGem(
                    column,
                    row,
                    GetRandomGemType(),
                    startPosition
                );

                fallingMoves.Add(
                    new GemMove
                    {
                        Gem = gem,
                        StartPosition =
                            startPosition,
                        TargetPosition =
                            targetPosition,
                        Delay = 0f,
                        Duration =
                            CalculateFallDuration(
                                startPosition,
                                targetPosition
                            )
                    }
                );

                newGemIndex++;
            }
        }

        yield return AnimateGemMoves(
            fallingMoves
        );
    }

    private float CalculateFallDuration(
        Vector3 startPosition,
        Vector3 targetPosition)
    {
        float distanceInCells =
            Mathf.Abs(
                startPosition.y -
                targetPosition.y
            ) / cellSize;

        float duration =
            distanceInCells *
            fallDurationPerCell;

        return Mathf.Clamp(
            duration,
            minimumFallDuration,
            maximumFallDuration
        );
    }

    private IEnumerator AnimateGemMoves(
        List<GemMove> moves)
    {
        if (moves.Count == 0)
        {
            yield break;
        }

        float totalDuration = 0f;

        foreach (GemMove move in moves)
        {
            totalDuration = Mathf.Max(
                totalDuration,
                move.Delay +
                move.Duration
            );

            move.Gem.transform
                .localPosition =
                move.StartPosition;
        }

        float elapsedTime = 0f;

        while (elapsedTime <
               totalDuration)
        {
            foreach (GemMove move in moves)
            {
                if (move.Gem == null)
                {
                    continue;
                }

                float moveElapsed =
                    elapsedTime -
                    move.Delay;

                if (moveElapsed < 0f)
                {
                    move.Gem.transform
                        .localPosition =
                        move.StartPosition;

                    continue;
                }

                float progress =
                    Mathf.Clamp01(
                        moveElapsed /
                        move.Duration
                    );

                float easedProgress =
                    SmoothStep(progress);

                move.Gem.transform
                    .localPosition =
                    Vector3.Lerp(
                        move.StartPosition,
                        move.TargetPosition,
                        easedProgress
                    );
            }

            elapsedTime +=
                Time.deltaTime;

            yield return null;
        }

        foreach (GemMove move in moves)
        {
            if (move.Gem != null)
            {
                move.Gem.transform
                    .localPosition =
                    move.TargetPosition;
            }
        }
    }

    private float SmoothStep(
        float progress)
    {
        return progress *
               progress *
               (3f - 2f * progress);
    }

    private IEnumerator AnimateSwap(
        Gem first,
        Gem second)
    {
        int firstColumn =
            first.Column;

        int firstRow =
            first.Row;

        int secondColumn =
            second.Column;

        int secondRow =
            second.Row;

        Vector3 firstStartPosition =
            first.transform.localPosition;

        Vector3 secondStartPosition =
            second.transform.localPosition;

        gems[firstColumn, firstRow] =
            second;

        gems[secondColumn, secondRow] =
            first;

        first.SetGridPosition(
            secondColumn,
            secondRow
        );

        second.SetGridPosition(
            firstColumn,
            firstRow
        );

        float elapsedTime = 0f;

        while (elapsedTime <
               swapDuration)
        {
            float progress =
                Mathf.Clamp01(
                    elapsedTime /
                    swapDuration
                );

            float easedProgress =
                SmoothStep(progress);

            first.transform.localPosition =
                Vector3.Lerp(
                    firstStartPosition,
                    secondStartPosition,
                    easedProgress
                );

            second.transform.localPosition =
                Vector3.Lerp(
                    secondStartPosition,
                    firstStartPosition,
                    easedProgress
                );

            elapsedTime +=
                Time.deltaTime;

            yield return null;
        }

        first.transform.localPosition =
            secondStartPosition;

        second.transform.localPosition =
            firstStartPosition;
    }

    private IEnumerator ReshuffleBoard()
    {
        ClearSelection();

        Debug.Log(
            "No valid moves remain. " +
            "Reshuffling board."
        );

        if (reshufflePause > 0f)
        {
            yield return new WaitForSeconds(
                reshufflePause
            );
        }

        List<Gem> shuffledLayout;

        GemType[,] replacementTypes =
            null;

        bool foundExistingLayout =
            TryCreateShuffledLayout(
                out shuffledLayout
            );

        if (!foundExistingLayout)
        {
            Debug.LogWarning(
                "Could not find a valid " +
                "arrangement using the " +
                "existing gem counts. " +
                "Generating new gem types."
            );

            shuffledLayout =
                GetAllGems();

            ShuffleList(shuffledLayout);

            replacementTypes =
                GeneratePlayableTypeGrid();
        }

        Gem[,] newGrid =
            new Gem[width, height];

        List<GemMove> reshuffleMoves =
            new List<GemMove>();

        int index = 0;

        for (int row = 0;
             row < height;
             row++)
        {
            for (int column = 0;
                 column < width;
                 column++)
            {
                Gem gem =
                    shuffledLayout[index];

                if (replacementTypes != null)
                {
                    GemType newType =
                        replacementTypes[
                            column,
                            row
                        ];

                    gem.SetType(
                        newType,
                        gemSprites[
                            (int)newType
                        ]
                    );
                }

                Vector3 startPosition =
                    gem.transform
                        .localPosition;

                Vector3 targetPosition =
                    GetLocalPosition(
                        column,
                        row
                    );

                gem.SetGridPosition(
                    column,
                    row
                );

                newGrid[column, row] =
                    gem;

                reshuffleMoves.Add(
                    new GemMove
                    {
                        Gem = gem,
                        StartPosition =
                            startPosition,
                        TargetPosition =
                            targetPosition,
                        Delay =
                            Random.Range(
                                0f,
                                reshuffleStagger
                            ),
                        Duration =
                            reshuffleDuration
                    }
                );

                index++;
            }
        }

        gems = newGrid;

        yield return AnimateGemMoves(
            reshuffleMoves
        );

        Debug.Log(
            "Reshuffle complete. " +
            "A valid move is available."
        );
    }

    private bool TryCreateShuffledLayout(
        out List<Gem> validLayout)
    {
        List<Gem> allGems =
            GetAllGems();

        for (int attempt = 0;
             attempt <
             maximumReshuffleAttempts;
             attempt++)
        {
            List<Gem> candidate =
                new List<Gem>(allGems);

            ShuffleList(candidate);

            GemType[,] candidateGrid =
                BuildTypeGrid(candidate);

            if (HasAnyMatches(
                    candidateGrid))
            {
                continue;
            }

            if (!HasAvailableMove(
                    candidateGrid))
            {
                continue;
            }

            validLayout = candidate;
            return true;
        }

        validLayout = null;
        return false;
    }

    private List<Gem> GetAllGems()
    {
        List<Gem> allGems =
            new List<Gem>(
                width * height
            );

        for (int row = 0;
             row < height;
             row++)
        {
            for (int column = 0;
                 column < width;
                 column++)
            {
                Gem gem =
                    gems[column, row];

                if (gem != null)
                {
                    allGems.Add(gem);
                }
            }
        }

        return allGems;
    }

    private void ShuffleList<T>(
        List<T> list)
    {
        for (int i = list.Count - 1;
             i > 0;
             i--)
        {
            int randomIndex =
                Random.Range(0, i + 1);

            T temporary =
                list[i];

            list[i] =
                list[randomIndex];

            list[randomIndex] =
                temporary;
        }
    }

    private GemType[,] BuildCurrentTypeGrid()
    {
        GemType[,] typeGrid =
            new GemType[width, height];

        for (int row = 0;
             row < height;
             row++)
        {
            for (int column = 0;
                 column < width;
                 column++)
            {
                typeGrid[column, row] =
                    gems[column, row].Type;
            }
        }

        return typeGrid;
    }

    private GemType[,] BuildTypeGrid(
        List<Gem> layout)
    {
        GemType[,] typeGrid =
            new GemType[width, height];

        int index = 0;

        for (int row = 0;
             row < height;
             row++)
        {
            for (int column = 0;
                 column < width;
                 column++)
            {
                typeGrid[column, row] =
                    layout[index].Type;

                index++;
            }
        }

        return typeGrid;
    }

    private GemType[,]
        GeneratePlayableTypeGrid()
    {
        for (int attempt = 0;
             attempt < 500;
             attempt++)
        {
            GemType[,] typeGrid =
                new GemType[
                    width,
                    height
                ];

            for (int row = 0;
                 row < height;
                 row++)
            {
                for (int column = 0;
                     column < width;
                     column++)
                {
                    List<GemType>
                        allowedTypes =
                            new List<GemType>();

                    for (int typeIndex = 0;
                         typeIndex <
                         gemSprites.Length;
                         typeIndex++)
                    {
                        GemType candidate =
                            (GemType)typeIndex;

                        bool horizontalMatch =
                            column >= 2 &&
                            typeGrid[
                                column - 1,
                                row
                            ] == candidate &&
                            typeGrid[
                                column - 2,
                                row
                            ] == candidate;

                        bool verticalMatch =
                            row >= 2 &&
                            typeGrid[
                                column,
                                row - 1
                            ] == candidate &&
                            typeGrid[
                                column,
                                row - 2
                            ] == candidate;

                        if (!horizontalMatch &&
                            !verticalMatch)
                        {
                            allowedTypes.Add(
                                candidate
                            );
                        }
                    }

                    typeGrid[column, row] =
                        allowedTypes[
                            Random.Range(
                                0,
                                allowedTypes.Count
                            )
                        ];
                }
            }

            if (HasAvailableMove(typeGrid))
            {
                return typeGrid;
            }
        }

        Debug.LogError(
            "Failed to generate a " +
            "playable board."
        );

        return BuildCurrentTypeGrid();
    }

    private bool HasAvailableMove()
    {
        return HasAvailableMove(
            BuildCurrentTypeGrid()
        );
    }

    private bool HasAvailableMove(
        GemType[,] typeGrid)
    {
        for (int row = 0;
             row < height;
             row++)
        {
            for (int column = 0;
                 column < width;
                 column++)
            {
                if (column + 1 < width &&
                    SwapCreatesMatch(
                        typeGrid,
                        column,
                        row,
                        column + 1,
                        row
                    ))
                {
                    return true;
                }

                if (row + 1 < height &&
                    SwapCreatesMatch(
                        typeGrid,
                        column,
                        row,
                        column,
                        row + 1
                    ))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool SwapCreatesMatch(
        GemType[,] typeGrid,
        int firstColumn,
        int firstRow,
        int secondColumn,
        int secondRow)
    {
        GemType firstType =
            typeGrid[
                firstColumn,
                firstRow
            ];

        GemType secondType =
            typeGrid[
                secondColumn,
                secondRow
            ];

        if (firstType == secondType)
        {
            return false;
        }

        typeGrid[
            firstColumn,
            firstRow
        ] = secondType;

        typeGrid[
            secondColumn,
            secondRow
        ] = firstType;

        bool createsMatch =
            HasMatchAt(
                typeGrid,
                firstColumn,
                firstRow
            ) ||
            HasMatchAt(
                typeGrid,
                secondColumn,
                secondRow
            );

        typeGrid[
            firstColumn,
            firstRow
        ] = firstType;

        typeGrid[
            secondColumn,
            secondRow
        ] = secondType;

        return createsMatch;
    }

    private bool HasAnyMatches(
        GemType[,] typeGrid)
    {
        for (int row = 0;
             row < height;
             row++)
        {
            for (int column = 0;
                 column < width;
                 column++)
            {
                if (HasMatchAt(
                        typeGrid,
                        column,
                        row))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool HasMatchAt(
        GemType[,] typeGrid,
        int column,
        int row)
    {
        GemType type =
            typeGrid[column, row];

        int horizontalCount =
            1 +
            CountMatchingTypes(
                typeGrid,
                column,
                row,
                -1,
                0,
                type
            ) +
            CountMatchingTypes(
                typeGrid,
                column,
                row,
                1,
                0,
                type
            );

        if (horizontalCount >= 3)
        {
            return true;
        }

        int verticalCount =
            1 +
            CountMatchingTypes(
                typeGrid,
                column,
                row,
                0,
                -1,
                type
            ) +
            CountMatchingTypes(
                typeGrid,
                column,
                row,
                0,
                1,
                type
            );

        return verticalCount >= 3;
    }

    private int CountMatchingTypes(
        GemType[,] typeGrid,
        int startingColumn,
        int startingRow,
        int columnDirection,
        int rowDirection,
        GemType type)
    {
        int count = 0;

        int column =
            startingColumn +
            columnDirection;

        int row =
            startingRow +
            rowDirection;

        while (column >= 0 &&
               column < width &&
               row >= 0 &&
               row < height &&
               typeGrid[column, row] ==
               type)
        {
            count++;

            column +=
                columnDirection;

            row +=
                rowDirection;
        }

        return count;
    }

    private HashSet<Gem> FindMatchesFrom(
        Gem first,
        Gem second)
    {
        HashSet<Gem> matchedGems =
            new HashSet<Gem>();

        AddMatchesAt(
            first,
            matchedGems
        );

        AddMatchesAt(
            second,
            matchedGems
        );

        return matchedGems;
    }

    private HashSet<Gem> FindAllMatches()
    {
        HashSet<Gem> matchedGems =
            new HashSet<Gem>();

        for (int row = 0;
             row < height;
             row++)
        {
            for (int column = 0;
                 column < width;
                 column++)
            {
                Gem gem =
                    gems[column, row];

                if (gem != null)
                {
                    AddMatchesAt(
                        gem,
                        matchedGems
                    );
                }
            }
        }

        return matchedGems;
    }

    private void AddMatchesAt(
        Gem origin,
        HashSet<Gem> matchedGems)
    {
        if (origin == null)
        {
            return;
        }

        List<Gem> horizontalMatch =
            CollectLine(
                origin,
                1,
                0
            );

        if (horizontalMatch.Count >= 3)
        {
            foreach (Gem gem
                     in horizontalMatch)
            {
                matchedGems.Add(gem);
            }
        }

        List<Gem> verticalMatch =
            CollectLine(
                origin,
                0,
                1
            );

        if (verticalMatch.Count >= 3)
        {
            foreach (Gem gem
                     in verticalMatch)
            {
                matchedGems.Add(gem);
            }
        }
    }

    private List<Gem> CollectLine(
        Gem origin,
        int columnDirection,
        int rowDirection)
    {
        List<Gem> line =
            new List<Gem>
            {
                origin
            };

        int column =
            origin.Column -
            columnDirection;

        int row =
            origin.Row -
            rowDirection;

        while (true)
        {
            Gem nextGem =
                GetGem(
                    column,
                    row
                );

            if (nextGem == null ||
                nextGem.Type !=
                origin.Type)
            {
                break;
            }

            line.Add(nextGem);

            column -=
                columnDirection;

            row -=
                rowDirection;
        }

        column =
            origin.Column +
            columnDirection;

        row =
            origin.Row +
            rowDirection;

        while (true)
        {
            Gem nextGem =
                GetGem(
                    column,
                    row
                );

            if (nextGem == null ||
                nextGem.Type !=
                origin.Type)
            {
                break;
            }

            line.Add(nextGem);

            column +=
                columnDirection;

            row +=
                rowDirection;
        }

        return line;
    }

    private Gem GetGem(
        int column,
        int row)
    {
        if (column < 0 ||
            column >= width ||
            row < 0 ||
            row >= height)
        {
            return null;
        }

        return gems[column, row];
    }

    private bool AreAdjacent(
        Gem first,
        Gem second)
    {
        int columnDistance =
            Mathf.Abs(
                first.Column -
                second.Column
            );

        int rowDistance =
            Mathf.Abs(
                first.Row -
                second.Row
            );

        return columnDistance +
               rowDistance == 1;
    }

    private void ClearSelection()
    {
        if (selectedGem == null)
        {
            return;
        }

        selectedGem.SetSelected(false);
        selectedGem = null;
    }

    [ContextMenu("Force Reshuffle")]
    private void ForceReshuffle()
    {
        if (!Application.isPlaying ||
            isBusy ||
            gems == null)
        {
            return;
        }

        StartCoroutine(
            ForceReshuffleRoutine()
        );
    }

    private IEnumerator
        ForceReshuffleRoutine()
    {
        isBusy = true;

        yield return ReshuffleBoard();

        isBusy = false;
    }

}