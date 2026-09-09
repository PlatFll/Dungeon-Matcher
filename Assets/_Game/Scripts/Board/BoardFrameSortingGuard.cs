using UnityEngine;

[DefaultExecutionOrder(-90)]
[DisallowMultipleComponent]
public sealed class BoardFrameSortingGuard : MonoBehaviour
{
    private const string BoardFrameContainerName =
        "BoardFrame";

    private const string BoardFrameSortingLayerName =
        "BoardFrame";

    private const int BoardFrameSortingOrder =
        0;

    private Transform frameRoot;
    private SpriteRenderer[] frameRenderers =
        System.Array.Empty<SpriteRenderer>();

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.AfterSceneLoad
    )]
    private static void InstallOnBoards()
    {
        BoardVisuals[] boardVisuals =
            Object.FindObjectsByType<BoardVisuals>(
                FindObjectsSortMode.None
            );

        foreach (BoardVisuals visuals in boardVisuals)
        {
            if (visuals == null ||
                visuals.TryGetComponent(
                    out BoardFrameSortingGuard _
                ))
            {
                continue;
            }

            visuals.gameObject.AddComponent<
                BoardFrameSortingGuard
            >();
        }
    }

    private void Awake()
    {
        CacheAndApply();
    }

    private void OnEnable()
    {
        CacheAndApply();
    }

    private void LateUpdate()
    {
        /*
         * BoardVisuals normally creates the frame once in Awake. Keeping this
         * guard active makes the contract explicit: no gem, board tile, VFX,
         * or later presentation code can accidentally move a frame renderer
         * back underneath the playable board.
         */
        if (frameRoot == null)
        {
            CacheAndApply();
            return;
        }

        bool needsRecache = false;

        foreach (SpriteRenderer renderer in frameRenderers)
        {
            if (renderer == null)
            {
                needsRecache = true;
                break;
            }

            ApplySorting(renderer);
        }

        if (needsRecache)
        {
            CacheAndApply();
        }
    }

    private void CacheAndApply()
    {
        frameRoot =
            transform.Find(
                BoardFrameContainerName
            );

        if (frameRoot == null)
        {
            frameRenderers =
                System.Array.Empty<SpriteRenderer>();
            return;
        }

        frameRenderers =
            frameRoot.GetComponentsInChildren<
                SpriteRenderer
            >(true);

        foreach (SpriteRenderer renderer in frameRenderers)
        {
            if (renderer != null)
            {
                ApplySorting(renderer);
            }
        }
    }

    private static void ApplySorting(
        SpriteRenderer renderer)
    {
        renderer.sortingLayerName =
            BoardFrameSortingLayerName;

        renderer.sortingOrder =
            BoardFrameSortingOrder;

        renderer.maskInteraction =
            SpriteMaskInteraction.None;
    }
}
