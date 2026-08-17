using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Image))]
public sealed class BottomAnchoredBackgroundFitter : MonoBehaviour
{
    private Image backgroundImage;
    private RectTransform backgroundRect;
    private RectTransform viewportRect;

    private Sprite lastSprite;
    private Vector2 lastViewportSize =
        new Vector2(float.NaN, float.NaN);

    private float sourceFloorPixelsFromBottom;
    private float targetFloorOffsetFromBottom;

    private void Awake()
    {
        ResolveReferences();
        RefreshLayout();
    }

    private void OnEnable()
    {
        ResolveReferences();
        RefreshLayout();
    }

    private void LateUpdate()
    {
        if (backgroundImage == null ||
            backgroundRect == null ||
            viewportRect == null)
        {
            ResolveReferences();
        }

        if (backgroundImage == null ||
            backgroundRect == null ||
            viewportRect == null)
        {
            return;
        }

        if (backgroundImage.sprite != lastSprite ||
            !Approximately(
                viewportRect.rect.size,
                lastViewportSize
            ))
        {
            RefreshLayout();
        }
    }

    public void ConfigureFloorAlignment(
        float sourceFloorPixels,
        float targetFloorOffset)
    {
        sourceFloorPixelsFromBottom =
            Mathf.Max(0f, sourceFloorPixels);

        targetFloorOffsetFromBottom =
            Mathf.Max(0f, targetFloorOffset);

        RefreshLayout();
    }

    public void RefreshLayout()
    {
        ResolveReferences();

        if (backgroundImage == null ||
            backgroundRect == null ||
            viewportRect == null)
        {
            return;
        }

        EnsureViewportMask();

        Sprite sprite =
            backgroundImage.sprite;

        Vector2 viewportSize =
            viewportRect.rect.size;

        if (viewportSize.x <= 0f ||
            viewportSize.y <= 0f)
        {
            return;
        }

        backgroundRect.anchorMin =
            new Vector2(0.5f, 0f);
        backgroundRect.anchorMax =
            new Vector2(0.5f, 0f);
        backgroundRect.pivot =
            new Vector2(0.5f, 0f);
        backgroundRect.localScale =
            Vector3.one;

        if (sprite == null ||
            sprite.rect.width <= 0f ||
            sprite.rect.height <= 0f)
        {
            backgroundRect.anchoredPosition =
                Vector2.zero;
            backgroundRect.sizeDelta =
                viewportSize;

            backgroundImage.preserveAspect = false;
            backgroundImage.raycastTarget = false;

            CacheCurrentState(sprite, viewportSize);
            return;
        }

        float sourceWidth =
            sprite.rect.width;

        float sourceHeight =
            sprite.rect.height;

        float floorPixels =
            Mathf.Clamp(
                sourceFloorPixelsFromBottom,
                0f,
                sourceHeight
            );

        float targetFloor =
            Mathf.Clamp(
                targetFloorOffsetFromBottom,
                0f,
                viewportSize.y
            );

        if (floorPixels > 0f &&
            floorPixels < sourceHeight)
        {
            FitToSemanticFloor(
                viewportSize,
                sourceWidth,
                sourceHeight,
                floorPixels,
                targetFloor
            );
        }
        else
        {
            FitBottomAnchoredCover(
                viewportSize,
                sourceWidth,
                sourceHeight
            );
        }

        backgroundImage.preserveAspect = false;
        backgroundImage.raycastTarget = false;

        CacheCurrentState(sprite, viewportSize);
    }

    private void FitToSemanticFloor(
        Vector2 viewportSize,
        float sourceWidth,
        float sourceHeight,
        float floorPixels,
        float targetFloor)
    {
        /*
         * One uniform source-to-UI scale must satisfy three constraints:
         *   1) cover the viewport width,
         *   2) provide enough source art below the authored floor to reach the
         *      viewport bottom, and
         *   3) provide enough source art above the floor to reach the viewport top.
         *
         * This is equivalent to an aspect-preserving "cover" fit whose semantic
         * anchor is the dungeon floor rather than the image's geometric center.
         */
        float widthScale =
            viewportSize.x /
            sourceWidth;

        float belowFloorScale =
            targetFloor > 0f
                ? targetFloor /
                  floorPixels
                : 0f;

        float sourcePixelsAboveFloor =
            sourceHeight -
            floorPixels;

        float viewportPixelsAboveFloor =
            Mathf.Max(
                0f,
                viewportSize.y -
                targetFloor
            );

        float aboveFloorScale =
            sourcePixelsAboveFloor > 0f
                ? viewportPixelsAboveFloor /
                  sourcePixelsAboveFloor
                : 0f;

        float sourceToUiScale =
            Mathf.Max(
                widthScale,
                belowFloorScale,
                aboveFloorScale
            );

        sourceToUiScale =
            Mathf.Max(
                0.0001f,
                sourceToUiScale
            );

        float fittedWidth =
            sourceWidth *
            sourceToUiScale;

        float fittedHeight =
            sourceHeight *
            sourceToUiScale;

        float renderedFloorOffset =
            floorPixels *
            sourceToUiScale;

        float imageBottom =
            targetFloor -
            renderedFloorOffset;

        backgroundRect.sizeDelta =
            new Vector2(
                fittedWidth,
                fittedHeight
            );

        /*
         * Snap only the semantic vertical anchor to the reference UI pixel grid.
         * Width/height keep one shared scale so the background is never distorted.
         */
        backgroundRect.anchoredPosition =
            new Vector2(
                0f,
                Mathf.Round(imageBottom)
            );
    }

    private void FitBottomAnchoredCover(
        Vector2 viewportSize,
        float sourceWidth,
        float sourceHeight)
    {
        float spriteAspect =
            sourceWidth /
            sourceHeight;

        float fittedWidth =
            viewportSize.x;

        float fittedHeight =
            fittedWidth /
            spriteAspect;

        if (fittedHeight < viewportSize.y)
        {
            fittedHeight =
                viewportSize.y;

            fittedWidth =
                fittedHeight *
                spriteAspect;
        }

        backgroundRect.anchoredPosition =
            Vector2.zero;

        backgroundRect.sizeDelta =
            new Vector2(
                fittedWidth,
                fittedHeight
            );
    }

    private void CacheCurrentState(
        Sprite sprite,
        Vector2 viewportSize)
    {
        lastSprite = sprite;
        lastViewportSize = viewportSize;
    }

    private void ResolveReferences()
    {
        if (backgroundImage == null)
        {
            backgroundImage =
                GetComponent<Image>();
        }

        if (backgroundRect == null)
        {
            backgroundRect =
                transform as RectTransform;
        }

        if (viewportRect == null &&
            transform.parent != null)
        {
            viewportRect =
                transform.parent as RectTransform;
        }
    }

    private void EnsureViewportMask()
    {
        if (viewportRect == null)
        {
            return;
        }

        if (!viewportRect.TryGetComponent(
                out RectMask2D _
            ))
        {
            viewportRect.gameObject.AddComponent<
                RectMask2D
            >();
        }
    }

    private static bool Approximately(
        Vector2 left,
        Vector2 right)
    {
        return
            Mathf.Approximately(left.x, right.x) &&
            Mathf.Approximately(left.y, right.y);
    }
}
