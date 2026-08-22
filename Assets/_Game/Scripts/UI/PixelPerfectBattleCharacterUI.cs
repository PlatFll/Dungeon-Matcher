using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Keeps battle character Images on an integer physical-pixel magnification.
/// The responsive layout is still free to request any logical scale; this
/// component snaps that request to the nearest scale at which one source-art
/// pixel maps to exactly 1, 2, 3... screen pixels.
/// </summary>
[DefaultExecutionOrder(10000)]
[DisallowMultipleComponent]
public sealed class PixelPerfectBattleCharacterUI : MonoBehaviour
{
    private const string PlayerCharacterName =
        "PlayerCharacter";

    private const string EnemySpawnAnchorName =
        "EnemySpawnAnchor";

    private const string EnemyVisualRootName =
        "VisualRoot";

    private const float MinimumScale = 0.0001f;

    private RectTransform scaleRoot;
    private Image characterImage;
    private Canvas rootCanvas;

    private float requestedUniformScale = 1f;
    private float lastAppliedUniformScale = float.NaN;
    private float lastCanvasScaleFactor = float.NaN;
    private float lastHierarchyScale = float.NaN;

    private Vector2 lastSourcePixelSize =
        new Vector2(float.NaN, float.NaN);

    private Vector2 lastImageRectSize =
        new Vector2(float.NaN, float.NaN);

    private bool presentationDirty = true;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.AfterSceneLoad
    )]
    private static void InstallForBattleCharacters()
    {
        RectTransform[] rects =
            Object.FindObjectsByType<RectTransform>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (RectTransform rect in rects)
        {
            if (rect == null ||
                (rect.name != PlayerCharacterName &&
                 rect.name != EnemySpawnAnchorName))
            {
                continue;
            }

            if (!rect.TryGetComponent(
                    out PixelPerfectBattleCharacterUI _
                ))
            {
                rect.gameObject.AddComponent<
                    PixelPerfectBattleCharacterUI
                >();
            }
        }
    }

    private void Awake()
    {
        scaleRoot =
            transform as RectTransform;

        CaptureRequestedScaleFromLayout();
        ResolveCharacterImage();
        ResolveCanvas();
        EnsurePixelPerfectCanvas();
        presentationDirty = true;
    }

    private void OnEnable()
    {
        if (scaleRoot == null)
        {
            scaleRoot =
                transform as RectTransform;
        }

        CaptureRequestedScaleFromLayout();
        ResolveCharacterImage();
        ResolveCanvas();
        EnsurePixelPerfectCanvas();
        presentationDirty = true;
    }

    private void LateUpdate()
    {
        if (scaleRoot == null)
        {
            return;
        }

        CaptureExternalScaleRequest();

        if (characterImage == null ||
            !characterImage.transform.IsChildOf(scaleRoot))
        {
            ResolveCharacterImage();
            presentationDirty = true;
        }

        if (rootCanvas == null)
        {
            ResolveCanvas();
            presentationDirty = true;
        }

        EnsurePixelPerfectCanvas();

        if (characterImage == null ||
            characterImage.sprite == null ||
            rootCanvas == null)
        {
            return;
        }

        Vector2 sourcePixelSize =
            characterImage.sprite.rect.size;

        Vector2 imageRectSize =
            characterImage.rectTransform.rect.size;

        float canvasScaleFactor =
            Mathf.Max(
                MinimumScale,
                rootCanvas.scaleFactor
            );

        float hierarchyScale =
            GetHierarchyScaleExcludingRoot();

        if (!Approximately(
                sourcePixelSize,
                lastSourcePixelSize
            ) ||
            !Approximately(
                imageRectSize,
                lastImageRectSize
            ) ||
            !Mathf.Approximately(
                canvasScaleFactor,
                lastCanvasScaleFactor
            ) ||
            !Mathf.Approximately(
                hierarchyScale,
                lastHierarchyScale
            ))
        {
            presentationDirty = true;
        }

        if (!presentationDirty)
        {
            return;
        }

        ApplyIntegerPixelMagnification(
            sourcePixelSize,
            imageRectSize,
            canvasScaleFactor,
            hierarchyScale
        );

        lastSourcePixelSize =
            sourcePixelSize;
        lastImageRectSize =
            imageRectSize;
        lastCanvasScaleFactor =
            canvasScaleFactor;
        lastHierarchyScale =
            hierarchyScale;
        presentationDirty = false;
    }

    private void CaptureRequestedScaleFromLayout()
    {
        if (scaleRoot == null)
        {
            return;
        }

        requestedUniformScale =
            GetUniformMagnitude(
                scaleRoot.localScale
            );
    }

    private void CaptureExternalScaleRequest()
    {
        float currentScale =
            GetUniformMagnitude(
                scaleRoot.localScale
            );

        if (float.IsNaN(
                lastAppliedUniformScale
            ) ||
            !Mathf.Approximately(
                currentScale,
                lastAppliedUniformScale
            ))
        {
            requestedUniformScale =
                currentScale;
            presentationDirty = true;
        }
    }

    private void ResolveCharacterImage()
    {
        characterImage =
            GetComponent<Image>();

        if (characterImage != null)
        {
            return;
        }

        Transform visualRoot =
            FindDescendantByName(
                transform,
                EnemyVisualRootName
            );

        if (visualRoot != null)
        {
            characterImage =
                visualRoot.GetComponent<Image>();
        }
    }

    private void ResolveCanvas()
    {
        rootCanvas =
            characterImage != null
                ? characterImage.canvas
                : GetComponentInParent<Canvas>();
    }

    private void EnsurePixelPerfectCanvas()
    {
        if (rootCanvas == null ||
            rootCanvas.renderMode ==
            RenderMode.WorldSpace)
        {
            return;
        }

        if (!rootCanvas.pixelPerfect)
        {
            rootCanvas.pixelPerfect = true;
        }
    }

    private void ApplyIntegerPixelMagnification(
        Vector2 sourcePixelSize,
        Vector2 imageRectSize,
        float canvasScaleFactor,
        float hierarchyScale)
    {
        if (sourcePixelSize.x <= 0f ||
            sourcePixelSize.y <= 0f ||
            imageRectSize.x <= 0f ||
            imageRectSize.y <= 0f)
        {
            return;
        }

        /*
         * Character Images use Preserve Aspect. The smaller axis ratio therefore
         * describes the actual source-pixel magnification inside the Image rect.
         * This is the piece the previous 112x112 presentation was missing: a
         * 47x58 or 54x61 sprite otherwise lands on a fractional magnification.
         */
        characterImage.preserveAspect = true;

        float logicalSpriteMagnification =
            Mathf.Min(
                imageRectSize.x /
                sourcePixelSize.x,
                imageRectSize.y /
                sourcePixelSize.y
            );

        float physicalMagnificationWithoutRoot =
            logicalSpriteMagnification *
            Mathf.Max(
                MinimumScale,
                hierarchyScale
            ) *
            canvasScaleFactor;

        if (physicalMagnificationWithoutRoot <=
            MinimumScale)
        {
            return;
        }

        float idealPhysicalMagnification =
            physicalMagnificationWithoutRoot *
            Mathf.Max(
                MinimumScale,
                requestedUniformScale
            );

        int integerPhysicalMagnification =
            Mathf.Max(
                1,
                Mathf.RoundToInt(
                    idealPhysicalMagnification
                )
            );

        float snappedRootScale =
            integerPhysicalMagnification /
            physicalMagnificationWithoutRoot;

        Vector3 currentScale =
            scaleRoot.localScale;

        float signX =
            currentScale.x < 0f
                ? -1f
                : 1f;

        float signY =
            currentScale.y < 0f
                ? -1f
                : 1f;

        scaleRoot.localScale =
            new Vector3(
                snappedRootScale * signX,
                snappedRootScale * signY,
                Mathf.Approximately(
                    currentScale.z,
                    0f
                )
                    ? 1f
                    : currentScale.z
            );

        lastAppliedUniformScale =
            snappedRootScale;
    }

    private float GetHierarchyScaleExcludingRoot()
    {
        if (scaleRoot == null ||
            characterImage == null)
        {
            return 1f;
        }

        float scale = 1f;

        Transform current =
            characterImage.rectTransform;

        while (current != null &&
               current != scaleRoot)
        {
            scale *=
                GetUniformMagnitude(
                    current.localScale
                );

            current =
                current.parent;
        }

        if (current != scaleRoot)
        {
            return 1f;
        }

        current =
            scaleRoot.parent;

        Transform canvasTransform =
            rootCanvas != null
                ? rootCanvas.transform
                : null;

        while (current != null &&
               current != canvasTransform)
        {
            scale *=
                GetUniformMagnitude(
                    current.localScale
                );

            current =
                current.parent;
        }

        return Mathf.Max(
            MinimumScale,
            scale
        );
    }

    private static float GetUniformMagnitude(
        Vector3 scale)
    {
        float x =
            Mathf.Abs(scale.x);

        float y =
            Mathf.Abs(scale.y);

        return Mathf.Max(
            MinimumScale,
            Mathf.Min(x, y)
        );
    }

    private static Transform FindDescendantByName(
        Transform root,
        string objectName)
    {
        if (root == null)
        {
            return null;
        }

        for (int index = 0;
             index < root.childCount;
             index++)
        {
            Transform child =
                root.GetChild(index);

            if (child.name == objectName)
            {
                return child;
            }

            Transform nested =
                FindDescendantByName(
                    child,
                    objectName
                );

            if (nested != null)
            {
                return nested;
            }
        }

        return null;
    }

    private static bool Approximately(
        Vector2 left,
        Vector2 right)
    {
        return
            Mathf.Approximately(
                left.x,
                right.x
            ) &&
            Mathf.Approximately(
                left.y,
                right.y
            );
    }

    private void OnRectTransformDimensionsChange()
    {
        presentationDirty = true;
    }

    private void OnTransformParentChanged()
    {
        rootCanvas = null;
        characterImage = null;
        presentationDirty = true;
    }
}
