using UnityEngine;

public static class TileCanvasFrameSpriteUtility
{
    public static bool IsSquareTileCanvas(
        Sprite sprite)
    {
        if (sprite == null ||
            sprite.texture == null ||
            sprite.texture.width <= 0 ||
            sprite.texture.height <= 0)
        {
            return false;
        }

        return
            sprite.texture.width ==
            sprite.texture.height;
    }

    public static bool IsSquareTileCanvasPair(
        Sprite cornerSprite,
        Sprite normalSprite)
    {
        if (!IsSquareTileCanvas(cornerSprite) ||
            !IsSquareTileCanvas(normalSprite))
        {
            return false;
        }

        return
            cornerSprite.texture.width ==
            normalSprite.texture.width &&
            cornerSprite.texture.height ==
            normalSprite.texture.height;
    }

    public static float GetCanvasPixels(
        Sprite sprite)
    {
        return IsSquareTileCanvas(sprite)
            ? sprite.texture.width
            : 0f;
    }

    public static Vector2 GetVisibleSizePixels(
        Sprite sprite)
    {
        return sprite != null
            ? sprite.rect.size
            : Vector2.zero;
    }

    public static Vector2 GetVisibleCenterOffsetPixels(
        Sprite sprite)
    {
        if (!IsSquareTileCanvas(sprite))
        {
            return Vector2.zero;
        }

        Vector2 textureCenter =
            new Vector2(
                sprite.texture.width * 0.5f,
                sprite.texture.height * 0.5f
            );

        return
            sprite.rect.center -
            textureCenter;
    }

    public static Vector2 GetPivotOffsetPixels(
        Sprite sprite)
    {
        if (!IsSquareTileCanvas(sprite))
        {
            return Vector2.zero;
        }

        Vector2 textureCenter =
            new Vector2(
                sprite.texture.width * 0.5f,
                sprite.texture.height * 0.5f
            );

        Vector2 pivotInTexture =
            sprite.rect.position +
            sprite.pivot;

        return
            pivotInTexture -
            textureCenter;
    }

    public static float GetNormalVisibleThicknessRatio(
        Sprite normalSprite)
    {
        float canvasPixels =
            GetCanvasPixels(normalSprite);

        if (canvasPixels <= 0f)
        {
            return 0f;
        }

        return
            normalSprite.rect.width /
            canvasPixels;
    }

    public static float GetNormalInnerPaddingRatio(
        Sprite normalSprite)
    {
        float canvasPixels =
            GetCanvasPixels(normalSprite);

        if (canvasPixels <= 0f)
        {
            return 0f;
        }

        return
            Mathf.Max(
                0f,
                canvasPixels -
                normalSprite.rect.xMax
            ) /
            canvasPixels;
    }

    public static float GetMaximumCornerExtentRatio(
        Sprite cornerSprite)
    {
        float canvasPixels =
            GetCanvasPixels(cornerSprite);

        if (canvasPixels <= 0f)
        {
            return 0f;
        }

        return
            Mathf.Max(
                cornerSprite.rect.width,
                cornerSprite.rect.height
            ) /
            canvasPixels;
    }

    public static Vector2 Rotate(
        Vector2 value,
        float degrees)
    {
        float radians =
            degrees *
            Mathf.Deg2Rad;

        float cosine =
            Mathf.Cos(radians);

        float sine =
            Mathf.Sin(radians);

        return new Vector2(
            value.x * cosine -
            value.y * sine,
            value.x * sine +
            value.y * cosine
        );
    }

    public static Rect GetRotatedVisibleBoundsRelativeToCanvas(
        Sprite sprite,
        float pixelsToUnits,
        float rotationDegrees)
    {
        if (!IsSquareTileCanvas(sprite) ||
            pixelsToUnits <= 0f)
        {
            return new Rect();
        }

        float halfTextureWidth =
            sprite.texture.width * 0.5f;

        float halfTextureHeight =
            sprite.texture.height * 0.5f;

        Vector2 bottomLeft =
            new Vector2(
                sprite.rect.xMin -
                halfTextureWidth,
                sprite.rect.yMin -
                halfTextureHeight
            ) *
            pixelsToUnits;

        Vector2 topLeft =
            new Vector2(
                sprite.rect.xMin -
                halfTextureWidth,
                sprite.rect.yMax -
                halfTextureHeight
            ) *
            pixelsToUnits;

        Vector2 topRight =
            new Vector2(
                sprite.rect.xMax -
                halfTextureWidth,
                sprite.rect.yMax -
                halfTextureHeight
            ) *
            pixelsToUnits;

        Vector2 bottomRight =
            new Vector2(
                sprite.rect.xMax -
                halfTextureWidth,
                sprite.rect.yMin -
                halfTextureHeight
            ) *
            pixelsToUnits;

        bottomLeft =
            Rotate(
                bottomLeft,
                rotationDegrees
            );

        topLeft =
            Rotate(
                topLeft,
                rotationDegrees
            );

        topRight =
            Rotate(
                topRight,
                rotationDegrees
            );

        bottomRight =
            Rotate(
                bottomRight,
                rotationDegrees
            );

        float minimumX =
            Mathf.Min(
                bottomLeft.x,
                topLeft.x,
                topRight.x,
                bottomRight.x
            );

        float maximumX =
            Mathf.Max(
                bottomLeft.x,
                topLeft.x,
                topRight.x,
                bottomRight.x
            );

        float minimumY =
            Mathf.Min(
                bottomLeft.y,
                topLeft.y,
                topRight.y,
                bottomRight.y
            );

        float maximumY =
            Mathf.Max(
                bottomLeft.y,
                topLeft.y,
                topRight.y,
                bottomRight.y
            );

        return Rect.MinMaxRect(
            minimumX,
            minimumY,
            maximumX,
            maximumY
        );
    }
}
