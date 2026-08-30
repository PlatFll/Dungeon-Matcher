using UnityEngine;

[DisallowMultipleComponent]
public sealed class CrackedGemOverlayView :
    MonoBehaviour
{
    private const int TextureSize = 32;
    private const float PixelsPerUnit = 32f;

    private static Sprite cachedCrackSprite;

    public static void EnsureInstalled(
        Gem gem)
    {
        if (gem == null)
        {
            return;
        }

        CrackedGemOverlayView existing =
            gem.GetComponentInChildren<
                CrackedGemOverlayView
            >(true);

        if (existing != null)
        {
            existing.gameObject.SetActive(true);
            return;
        }

        SpriteRenderer baseRenderer =
            gem.GetComponent<SpriteRenderer>();

        GameObject overlayObject =
            new GameObject(
                "CrackedGemOverlay"
            );

        overlayObject.transform.SetParent(
            gem.transform,
            false
        );

        overlayObject.transform.localPosition =
            Vector3.zero;

        overlayObject.transform.localRotation =
            Quaternion.identity;

        overlayObject.transform.localScale =
            Vector3.one;

        SpriteRenderer overlayRenderer =
            overlayObject.AddComponent<
                SpriteRenderer
            >();

        overlayObject.AddComponent<
            CrackedGemOverlayView
        >();

        overlayRenderer.sprite =
            GetOrCreateCrackSprite();

        overlayRenderer.color =
            Color.white;

        if (baseRenderer != null)
        {
            overlayRenderer.sortingLayerID =
                baseRenderer.sortingLayerID;

            overlayRenderer.sortingOrder =
                baseRenderer.sortingOrder + 2;
        }
    }

    private static Sprite GetOrCreateCrackSprite()
    {
        if (cachedCrackSprite != null)
        {
            return cachedCrackSprite;
        }

        Texture2D texture =
            new Texture2D(
                TextureSize,
                TextureSize,
                TextureFormat.RGBA32,
                false
            );

        texture.name =
            "Runtime_CrackedGemOverlay";

        texture.filterMode =
            FilterMode.Point;

        texture.wrapMode =
            TextureWrapMode.Clamp;

        texture.hideFlags =
            HideFlags.HideAndDontSave;

        Color transparent =
            new Color(0f, 0f, 0f, 0f);

        Color[] pixels =
            new Color[
                TextureSize * TextureSize
            ];

        for (int index = 0;
             index < pixels.Length;
             index++)
        {
            pixels[index] = transparent;
        }

        texture.SetPixels(pixels);

        Color darkCrack =
            new Color(
                0.10f,
                0.075f,
                0.12f,
                0.95f
            );

        Color lightEdge =
            new Color(
                1f,
                0.96f,
                0.82f,
                0.72f
            );

        Vector2Int[] mainCrack =
        {
            new Vector2Int(17, 29),
            new Vector2Int(16, 25),
            new Vector2Int(18, 22),
            new Vector2Int(15, 18),
            new Vector2Int(16, 15),
            new Vector2Int(13, 12),
            new Vector2Int(14, 8),
            new Vector2Int(11, 4)
        };

        Vector2Int[] leftBranch =
        {
            new Vector2Int(15, 18),
            new Vector2Int(11, 17),
            new Vector2Int(8, 14),
            new Vector2Int(5, 13)
        };

        Vector2Int[] rightBranch =
        {
            new Vector2Int(16, 15),
            new Vector2Int(20, 13),
            new Vector2Int(22, 9),
            new Vector2Int(26, 7)
        };

        DrawCrackPolyline(
            texture,
            mainCrack,
            darkCrack,
            lightEdge
        );

        DrawCrackPolyline(
            texture,
            leftBranch,
            darkCrack,
            lightEdge
        );

        DrawCrackPolyline(
            texture,
            rightBranch,
            darkCrack,
            lightEdge
        );

        texture.Apply(false, true);

        cachedCrackSprite =
            Sprite.Create(
                texture,
                new Rect(
                    0f,
                    0f,
                    TextureSize,
                    TextureSize
                ),
                new Vector2(0.5f, 0.5f),
                PixelsPerUnit,
                0,
                SpriteMeshType.FullRect
            );

        cachedCrackSprite.name =
            "Runtime_CrackedGemOverlay";

        cachedCrackSprite.hideFlags =
            HideFlags.HideAndDontSave;

        return cachedCrackSprite;
    }

    private static void DrawCrackPolyline(
        Texture2D texture,
        Vector2Int[] points,
        Color darkCrack,
        Color lightEdge)
    {
        if (texture == null ||
            points == null ||
            points.Length < 2)
        {
            return;
        }

        for (int index = 0;
             index < points.Length - 1;
             index++)
        {
            DrawPixelLine(
                texture,
                points[index],
                points[index + 1],
                darkCrack
            );

            DrawPixelLine(
                texture,
                points[index] +
                    Vector2Int.right,
                points[index + 1] +
                    Vector2Int.right,
                lightEdge
            );
        }
    }

    private static void DrawPixelLine(
        Texture2D texture,
        Vector2Int start,
        Vector2Int end,
        Color color)
    {
        int x0 = start.x;
        int y0 = start.y;
        int x1 = end.x;
        int y1 = end.y;

        int deltaX =
            Mathf.Abs(x1 - x0);

        int stepX =
            x0 < x1
                ? 1
                : -1;

        int deltaY =
            -Mathf.Abs(y1 - y0);

        int stepY =
            y0 < y1
                ? 1
                : -1;

        int error =
            deltaX + deltaY;

        while (true)
        {
            SetPixelSafe(
                texture,
                x0,
                y0,
                color
            );

            if (x0 == x1 &&
                y0 == y1)
            {
                break;
            }

            int doubledError =
                error * 2;

            if (doubledError >= deltaY)
            {
                error += deltaY;
                x0 += stepX;
            }

            if (doubledError <= deltaX)
            {
                error += deltaX;
                y0 += stepY;
            }
        }
    }

    private static void SetPixelSafe(
        Texture2D texture,
        int x,
        int y,
        Color color)
    {
        if (texture == null ||
            x < 0 ||
            x >= TextureSize ||
            y < 0 ||
            y >= TextureSize)
        {
            return;
        }

        texture.SetPixel(
            x,
            y,
            color
        );
    }
}
