using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pixel-perfect presentation for the player and enemy character artwork.
///
/// The layout remains the authority for character scale. Designers can keep
/// tuning PlayerVisualScale, EnemyVisualScale, per-enemy VisualSize, and the
/// responsive battle scale normally. This component never replaces those
/// values with coarse integer scale tiers.
///
/// Instead, it treats those values as the requested presentation and snaps the
/// final rendered character rectangle to whole physical screen pixels. With
/// Point-filtered sprites this removes fractional UI bounds/sub-pixel placement
/// while preserving the requested player/enemy size relationship.
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

    private const float MinimumPhysicalScale = 0.0001f;
    private const float SizeComparisonEpsilon = 0.01f;

    /*
     * A small dead-band around the currently selected physical size prevents
     * +/- one-pixel chatter if a device reports tiny floating-point scale
     * changes around a rounding boundary.
     */
    private const float PhysicalPixelHysteresis = 0.60f;

    private RectTransform scaleRoot;
    private RectTransform movingVisualRoot;
    private Image characterImage;
    private Canvas rootCanvas;

    private RectTransform observedVisualRoot;
    private Image observedImage;

    private Vector2 requestedVisualBoxSize;
    private Vector2 lastAppliedVisualSize;
    private Vector2 lastSourceSpriteSize;
    private Vector2Int lastAppliedPhysicalSize;

    private bool hasRequestedVisualBox;
    private bool hasAppliedGeometry;
    private bool isPlayerRoot;

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
        ResolveReferences();
        EnsurePixelPerfectCanvas();
    }

    private void OnEnable()
    {
        ResolveReferences();
        EnsurePixelPerfectCanvas();
    }

    private void LateUpdate()
    {
        ResolveReferences();

        if (scaleRoot == null ||
            movingVisualRoot == null ||
            characterImage == null ||
            rootCanvas == null)
        {
            return;
        }

        EnsurePixelPerfectCanvas();
        CaptureExternalVisualBoxRequest();

        /*
         * The gameplay/layout root owns the character's floor position. Snap it
         * first, then quantize/snap the actual artwork inside that root.
         */
        SnapRectPivotToPhysicalPixelGrid(
            scaleRoot
        );

        ApplyPixelPerfectVisualGeometry();

        if (movingVisualRoot != scaleRoot)
        {
            SnapRectPivotToPhysicalPixelGrid(
                movingVisualRoot
            );
        }
    }

    private void ResolveReferences()
    {
        scaleRoot =
            transform as RectTransform;

        isPlayerRoot =
            scaleRoot != null &&
            scaleRoot.name == PlayerCharacterName;

        RectTransform resolvedVisualRoot = null;
        Image resolvedImage = null;

        if (scaleRoot != null)
        {
            if (isPlayerRoot)
            {
                resolvedVisualRoot =
                    scaleRoot;

                resolvedImage =
                    scaleRoot.GetComponent<Image>();
            }
            else
            {
                Transform visualTransform =
                    FindDescendantByName(
                        scaleRoot,
                        EnemyVisualRootName
                    );

                resolvedVisualRoot =
                    visualTransform as RectTransform;

                resolvedImage =
                    visualTransform != null
                        ? visualTransform.GetComponent<Image>()
                        : null;
            }
        }

        if (resolvedVisualRoot != observedVisualRoot ||
            resolvedImage != observedImage)
        {
            observedVisualRoot =
                resolvedVisualRoot;

            observedImage =
                resolvedImage;

            movingVisualRoot =
                resolvedVisualRoot;

            characterImage =
                resolvedImage;

            ResetVisualGeometryState();
        }
        else
        {
            movingVisualRoot =
                resolvedVisualRoot;

            characterImage =
                resolvedImage;
        }

        rootCanvas =
            characterImage != null
                ? characterImage.canvas
                : GetComponentInParent<Canvas>();
    }

    private void ResetVisualGeometryState()
    {
        requestedVisualBoxSize =
            Vector2.zero;

        lastAppliedVisualSize =
            Vector2.zero;

        lastSourceSpriteSize =
            Vector2.zero;

        lastAppliedPhysicalSize =
            Vector2Int.zero;

        hasRequestedVisualBox = false;
        hasAppliedGeometry = false;
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

    private void CaptureExternalVisualBoxRequest()
    {
        if (movingVisualRoot == null ||
            characterImage == null)
        {
            return;
        }

        Vector2 currentSize =
            movingVisualRoot.rect.size;

        if (currentSize.x <= 0f ||
            currentSize.y <= 0f)
        {
            return;
        }

        bool externalSizeChange =
            hasAppliedGeometry &&
            !Approximately(
                currentSize,
                lastAppliedVisualSize,
                SizeComparisonEpsilon
            );

        /*
         * EnemyVisualPresenter intentionally sets preserveAspect=true whenever
         * it applies a fresh EnemyDefinition. After this component has already
         * taken ownership of the final geometry, seeing that flag become true
         * again is also a reliable signal that a new authored presentation was
         * supplied, even when its box happens to match the previous dimensions.
         */
        bool externalAspectFitRequest =
            hasAppliedGeometry &&
            characterImage.preserveAspect;

        if (!hasRequestedVisualBox ||
            externalSizeChange ||
            externalAspectFitRequest)
        {
            requestedVisualBoxSize =
                currentSize;

            hasRequestedVisualBox = true;
            hasAppliedGeometry = false;
            lastAppliedPhysicalSize =
                Vector2Int.zero;
        }
    }

    private void ApplyPixelPerfectVisualGeometry()
    {
        if (!hasRequestedVisualBox ||
            movingVisualRoot == null ||
            characterImage == null ||
            characterImage.sprite == null)
        {
            return;
        }

        Vector2 sourceSpriteSize =
            characterImage.sprite.rect.size;

        if (sourceSpriteSize.x <= 0f ||
            sourceSpriteSize.y <= 0f)
        {
            return;
        }

        Vector2 idealLocalDrawSize =
            FitInside(
                sourceSpriteSize,
                requestedVisualBoxSize
            );

        if (!TryGetPhysicalPixelsPerLocalUnit(
                movingVisualRoot,
                out Vector2 physicalScale
            ))
        {
            return;
        }

        Vector2 idealPhysicalSize =
            new Vector2(
                idealLocalDrawSize.x *
                physicalScale.x,
                idealLocalDrawSize.y *
                physicalScale.y
            );

        bool sourceGeometryChanged =
            !Approximately(
                sourceSpriteSize,
                lastSourceSpriteSize,
                SizeComparisonEpsilon
            );

        int physicalWidth =
            QuantizePhysicalDimension(
                idealPhysicalSize.x,
                sourceGeometryChanged
                    ? 0
                    : lastAppliedPhysicalSize.x
            );

        int physicalHeight =
            QuantizePhysicalDimension(
                idealPhysicalSize.y,
                sourceGeometryChanged
                    ? 0
                    : lastAppliedPhysicalSize.y
            );

        Vector2 appliedLocalSize =
            new Vector2(
                physicalWidth /
                Mathf.Max(
                    MinimumPhysicalScale,
                    physicalScale.x
                ),
                physicalHeight /
                Mathf.Max(
                    MinimumPhysicalScale,
                    physicalScale.y
                )
            );

        if (!IsFinitePositive(appliedLocalSize))
        {
            return;
        }

        movingVisualRoot.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal,
            appliedLocalSize.x
        );

        movingVisualRoot.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            appliedLocalSize.y
        );

        /*
         * We already performed the aspect fit above. Disabling Image's own
         * preserveAspect pass prevents it from reintroducing a fractional inner
         * rectangle after the outer RectTransform has been quantized.
         */
        characterImage.preserveAspect = false;

        lastAppliedVisualSize =
            movingVisualRoot.rect.size;

        lastAppliedPhysicalSize =
            new Vector2Int(
                physicalWidth,
                physicalHeight
            );

        lastSourceSpriteSize =
            sourceSpriteSize;

        hasAppliedGeometry = true;
    }

    private static Vector2 FitInside(
        Vector2 sourceSize,
        Vector2 boxSize)
    {
        if (sourceSize.x <= 0f ||
            sourceSize.y <= 0f ||
            boxSize.x <= 0f ||
            boxSize.y <= 0f)
        {
            return Vector2.zero;
        }

        float fitScale =
            Mathf.Min(
                boxSize.x /
                sourceSize.x,
                boxSize.y /
                sourceSize.y
            );

        return sourceSize *
               Mathf.Max(
                   0f,
                   fitScale
               );
    }

    private static int QuantizePhysicalDimension(
        float idealPhysicalSize,
        int previousPhysicalSize)
    {
        if (!IsFinite(idealPhysicalSize) ||
            idealPhysicalSize <= 0f)
        {
            return 1;
        }

        int nearestPhysicalSize =
            Mathf.Max(
                1,
                Mathf.RoundToInt(
                    idealPhysicalSize
                )
            );

        if (previousPhysicalSize > 0 &&
            Mathf.Abs(
                idealPhysicalSize -
                previousPhysicalSize
            ) < PhysicalPixelHysteresis)
        {
            return previousPhysicalSize;
        }

        return nearestPhysicalSize;
    }

    private bool TryGetPhysicalPixelsPerLocalUnit(
        RectTransform rect,
        out Vector2 physicalScale)
    {
        physicalScale =
            Vector2.zero;

        if (rect == null ||
            rootCanvas == null)
        {
            return false;
        }

        Camera canvasCamera =
            rootCanvas.renderMode ==
            RenderMode.ScreenSpaceOverlay
                ? null
                : rootCanvas.worldCamera;

        Vector3 originWorld =
            rect.TransformPoint(
                Vector3.zero
            );

        Vector3 xWorld =
            rect.TransformPoint(
                Vector3.right
            );

        Vector3 yWorld =
            rect.TransformPoint(
                Vector3.up
            );

        Vector2 originScreen =
            RectTransformUtility.WorldToScreenPoint(
                canvasCamera,
                originWorld
            );

        Vector2 xScreen =
            RectTransformUtility.WorldToScreenPoint(
                canvasCamera,
                xWorld
            );

        Vector2 yScreen =
            RectTransformUtility.WorldToScreenPoint(
                canvasCamera,
                yWorld
            );

        physicalScale =
            new Vector2(
                Vector2.Distance(
                    originScreen,
                    xScreen
                ),
                Vector2.Distance(
                    originScreen,
                    yScreen
                )
            );

        return
            IsFinite(physicalScale.x) &&
            IsFinite(physicalScale.y) &&
            physicalScale.x >= MinimumPhysicalScale &&
            physicalScale.y >= MinimumPhysicalScale;
    }

    private void SnapRectPivotToPhysicalPixelGrid(
        RectTransform rect)
    {
        if (rect == null ||
            rootCanvas == null ||
            rect.parent is not RectTransform parentRect)
        {
            return;
        }

        Camera canvasCamera =
            rootCanvas.renderMode ==
            RenderMode.ScreenSpaceOverlay
                ? null
                : rootCanvas.worldCamera;

        Vector2 currentScreenPoint =
            RectTransformUtility.WorldToScreenPoint(
                canvasCamera,
                rect.position
            );

        Vector2 snappedScreenPoint =
            new Vector2(
                Mathf.Round(
                    currentScreenPoint.x
                ),
                Mathf.Round(
                    currentScreenPoint.y
                )
            );

        if (Approximately(
                currentScreenPoint,
                snappedScreenPoint,
                0.001f
            ))
        {
            return;
        }

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                currentScreenPoint,
                canvasCamera,
                out Vector2 currentParentPoint
            ) ||
            !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                snappedScreenPoint,
                canvasCamera,
                out Vector2 snappedParentPoint
            ))
        {
            return;
        }

        rect.anchoredPosition +=
            snappedParentPoint -
            currentParentPoint;
    }

    private static bool IsFinitePositive(
        Vector2 value)
    {
        return
            IsFinite(value.x) &&
            IsFinite(value.y) &&
            value.x > 0f &&
            value.y > 0f;
    }

    private static bool IsFinite(
        float value)
    {
        return
            !float.IsNaN(value) &&
            !float.IsInfinity(value);
    }

    private static bool Approximately(
        Vector2 left,
        Vector2 right,
        float epsilon)
    {
        return
            Mathf.Abs(left.x - right.x) <= epsilon &&
            Mathf.Abs(left.y - right.y) <= epsilon;
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

    private void OnTransformParentChanged()
    {
        ResolveReferences();
    }
}
