using UnityEngine;

[DefaultExecutionOrder(25)]
[DisallowMultipleComponent]
public sealed class BottomHudTileCanvasFrameAdapter : MonoBehaviour
{
    private const string BottomHudName =
        "BottomHUD";

    private const string FrameName =
        "GeneratedBottomHudFrame";

    private bool hasRebuiltFrame;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.AfterSceneLoad
    )]
    private static void InstallOnBottomHud()
    {
        GameObject bottomHud =
            GameObject.Find(
                BottomHudName
            );

        if (bottomHud == null ||
            bottomHud.TryGetComponent(
                out BottomHudTileCanvasFrameAdapter _
            ))
        {
            return;
        }

        bottomHud.AddComponent<
            BottomHudTileCanvasFrameAdapter
        >();
    }

    private void Start()
    {
        TryRebuildFrame();
    }

    private void LateUpdate()
    {
        if (!hasRebuiltFrame)
        {
            TryRebuildFrame();
        }
    }

    private void TryRebuildFrame()
    {
        RectTransform bottomHud =
            transform as RectTransform;

        if (bottomHud == null)
        {
            return;
        }

        TileCanvasBoardFrameController boardFrame =
            FindFirstObjectByType<
                TileCanvasBoardFrameController
            >();

        if (boardFrame == null ||
            !boardFrame.IsUsingTileCanvasFrame ||
            !TileCanvasFrameSpriteUtility
                .IsSquareTileCanvasPair(
                    boardFrame.CornerSprite,
                    boardFrame.NormalSprite
                ))
        {
            return;
        }

        Transform existingFrame =
            bottomHud.Find(
                FrameName
            );

        if (existingFrame != null)
        {
            existingFrame.name =
                FrameName +
                "_LegacyDisabled";

            existingFrame.gameObject.SetActive(false);
        }

        RectTransform frameRoot =
            CreateRectTransform(
                FrameName,
                bottomHud
            );

        StretchToParent(
            frameRoot
        );

        frameRoot.SetAsFirstSibling();

        ResponsiveModularFrameFitter fitter =
            frameRoot.gameObject.AddComponent<
                ResponsiveModularFrameFitter
            >();

        fitter.ConfigureTileCanvas(
            boardFrame.CornerSprite,
            boardFrame.NormalSprite,
            Color.white
        );

        hasRebuiltFrame = true;

        if (existingFrame == null)
        {
            return;
        }

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

        return rect;
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
