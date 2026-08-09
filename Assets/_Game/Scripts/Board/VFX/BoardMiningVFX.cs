using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoardController))]
public sealed class BoardMiningVFX :
    MonoBehaviour
{
    private const string CellTileContainerName =
        "CellTiles";

    private const string FlashObjectName =
        "MiningTileFlash";

    private BoardController boardController;

    private Texture2D runtimeTexture;
    private Sprite runtimeSquareSprite;

    private readonly Dictionary<Vector2Int, Coroutine>
        activeFlashRoutines =
            new Dictionary<Vector2Int, Coroutine>();

    private readonly Dictionary<Vector2Int, GameObject>
        activeFlashObjects =
            new Dictionary<Vector2Int, GameObject>();

    private void Awake()
    {
        boardController =
            GetComponent<BoardController>();

        CreateRuntimeSquareSprite();
    }

    private void OnEnable()
    {
        Subscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
        StopAllFlashes();
    }

    private void Subscribe()
    {
        if (boardController == null)
        {
            return;
        }

        boardController.CellMiningStarted -=
            HandleCellMiningStarted;

        boardController.CellMiningStarted +=
            HandleCellMiningStarted;

        boardController.CellRestored -=
            HandleCellRestored;

        boardController.CellRestored +=
            HandleCellRestored;
    }

    private void Unsubscribe()
    {
        if (boardController == null)
        {
            return;
        }

        boardController.CellMiningStarted -=
            HandleCellMiningStarted;

        boardController.CellRestored -=
            HandleCellRestored;
    }

    private void HandleCellMiningStarted(
        int column,
        int row,
        float flashDuration)
    {
        Vector2Int cell =
            new Vector2Int(
                column,
                row
            );

        StopFlash(cell);

        Coroutine routine =
            StartCoroutine(
                PlayMineFlash(
                    cell,
                    Mathf.Max(
                        0.02f,
                        flashDuration
                    )
                )
            );

        activeFlashRoutines[cell] =
            routine;
    }

    private IEnumerator PlayMineFlash(
        Vector2Int cell,
        float duration)
    {
        GameObject flashObject =
            CreateFlashObject(cell);

        if (flashObject == null)
        {
            SetCellTileVisible(
                cell,
                false
            );

            activeFlashRoutines.Remove(cell);
            yield break;
        }

        activeFlashObjects[cell] =
            flashObject;

        SpriteRenderer renderer =
            flashObject.GetComponent<
                SpriteRenderer
            >();

        float elapsedTime = 0f;

        while (elapsedTime < duration &&
               flashObject != null)
        {
            float progress =
                Mathf.Clamp01(
                    elapsedTime /
                    duration
                );

            Color color = Color.white;

            /*
             * Hit hard immediately, then fade only near the end. This reads
             * like the tile itself was struck rather than a soft UI fade.
             */
            float fadeProgress =
                Mathf.InverseLerp(
                    0.55f,
                    1f,
                    progress
                );

            color.a =
                1f -
                fadeProgress *
                fadeProgress;

            renderer.color = color;

            elapsedTime +=
                Time.deltaTime;

            yield return null;
        }

        SetCellTileVisible(
            cell,
            false
        );

        activeFlashRoutines.Remove(cell);
        activeFlashObjects.Remove(cell);

        if (flashObject != null)
        {
            Destroy(flashObject);
        }
    }

    private void HandleCellRestored(
        int column,
        int row)
    {
        Vector2Int cell =
            new Vector2Int(
                column,
                row
            );

        /*
         * A Miner can theoretically die while its final mine flash is still
         * alive. Cancel that flash so it cannot disable the restored tile a
         * few frames after restoration.
         */
        StopFlash(cell);

        SetCellTileVisible(
            cell,
            true
        );
    }

    private GameObject CreateFlashObject(
        Vector2Int cell)
    {
        if (boardController == null ||
            runtimeSquareSprite == null)
        {
            return null;
        }

        GameObject flashObject =
            new GameObject(
                $"{FlashObjectName}_{cell.x}_{cell.y}"
            );

        flashObject.transform.SetParent(
            boardController.transform,
            false
        );

        flashObject.transform.localPosition =
            boardController.GetCellLocalPosition(
                cell.x,
                cell.y
            );

        flashObject.transform.localScale =
            new Vector3(
                boardController.CellSize,
                boardController.CellSize,
                1f
            );

        SpriteRenderer renderer =
            flashObject.AddComponent<
                SpriteRenderer
            >();

        renderer.sprite =
            runtimeSquareSprite;

        renderer.color = Color.white;
        renderer.sortingLayerName = "Gems";
        renderer.sortingOrder = 20;
        renderer.maskInteraction =
            SpriteMaskInteraction
                .VisibleInsideMask;

        return flashObject;
    }

    private void SetCellTileVisible(
        Vector2Int cell,
        bool visible)
    {
        Transform tileTransform =
            transform.Find(
                $"{CellTileContainerName}/" +
                $"CellTile_{cell.x}_{cell.y}"
            );

        if (tileTransform == null)
        {
            Debug.LogWarning(
                $"Could not find board tile ({cell.x}, {cell.y}) " +
                "for Miner presentation.",
                this
            );

            return;
        }

        SpriteRenderer tileRenderer =
            tileTransform.GetComponent<
                SpriteRenderer
            >();

        if (tileRenderer != null)
        {
            tileRenderer.enabled = visible;
        }
    }

    private void StopFlash(
        Vector2Int cell)
    {
        if (activeFlashRoutines.TryGetValue(
                cell,
                out Coroutine routine))
        {
            if (routine != null)
            {
                StopCoroutine(routine);
            }

            activeFlashRoutines.Remove(cell);
        }

        if (activeFlashObjects.TryGetValue(
                cell,
                out GameObject flashObject))
        {
            activeFlashObjects.Remove(cell);

            if (flashObject != null)
            {
                Destroy(flashObject);
            }
        }
    }

    private void StopAllFlashes()
    {
        List<Vector2Int> cells =
            new List<Vector2Int>(
                activeFlashRoutines.Keys
            );

        foreach (Vector2Int cell in cells)
        {
            StopFlash(cell);
        }

        activeFlashRoutines.Clear();
        activeFlashObjects.Clear();
    }

    private void CreateRuntimeSquareSprite()
    {
        runtimeTexture =
            new Texture2D(
                1,
                1,
                TextureFormat.RGBA32,
                false
            );

        runtimeTexture.name =
            "Runtime Mining Tile Flash Texture";

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

        runtimeSquareSprite =
            Sprite.Create(
                runtimeTexture,
                new Rect(
                    0f,
                    0f,
                    1f,
                    1f
                ),
                new Vector2(
                    0.5f,
                    0.5f
                ),
                1f
            );

        runtimeSquareSprite.name =
            "Runtime Mining Tile Flash Sprite";
    }

    private void OnDestroy()
    {
        Unsubscribe();
        StopAllFlashes();

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
