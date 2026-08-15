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
        backgroundRect.anchoredPosition =
            Vector2.zero;
        backgroundRect.localScale =
            Vector3.one;

        if (sprite == null ||
            sprite.rect.width <= 0f ||
            sprite.rect.height <= 0f)
        {
            backgroundRect.sizeDelta =
                viewportSize;
        }
        else
        {
            /*
             * Width is the primary constraint so taller phones reveal more of
             * the authored dungeon above the floor instead of stretching it.
             * If a viewport is ever taller than the available overscan art, the
             * image falls back to a standard cover fit and crops the sides.
             */
            float spriteAspect =
                sprite.rect.width /
                sprite.rect.height;

            float fittedWidth =
                viewportSize.x;

            float fittedHeight =
                fittedWidth /
                spriteAspect;

            if (fittedHeight < viewportSize.y)
            {
                fittedHeight = viewportSize.y;
                fittedWidth =
                    fittedHeight *
                    spriteAspect;
            }

            backgroundRect.sizeDelta =
                new Vector2(
                    Mathf.Ceil(fittedWidth),
                    Mathf.Ceil(fittedHeight)
                );
        }

        backgroundImage.preserveAspect = false;
        backgroundImage.raycastTarget = false;

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
