using UnityEngine;

[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public sealed class WorldSpriteRectFitter :
    MonoBehaviour
{
    [Header("References")]

    [SerializeField]
    private RectTransform targetRect;

    [SerializeField]
    private Camera worldCamera;

    [Header("World Placement")]

    [SerializeField]
    [Tooltip(
        "World Z position used by the fitted sprite. " +
        "The board is at Z 0, so Z 1 places this farther " +
        "from the camera."
    )]
    private float worldZ = 1f;

    private readonly Vector3[] rectCorners =
        new Vector3[4];

    private SpriteRenderer spriteRenderer;
    private Canvas targetCanvas;

    private void Awake()
    {
        CacheReferences();
        FitToTargetRect();
    }

    private void OnEnable()
    {
        CacheReferences();
        FitToTargetRect();
    }

    private void OnValidate()
    {
        CacheReferences();
        FitToTargetRect();
    }

    private void LateUpdate()
    {
        /*
         * The SafeArea and Canvas dimensions can change
         * when testing different phone resolutions.
         *
         * This is only one decorative sprite, so updating
         * it here is inexpensive and keeps it synchronized.
         */
        FitToTargetRect();
    }

    private void CacheReferences()
    {
        spriteRenderer =
            GetComponent<SpriteRenderer>();

        if (targetRect != null)
        {
            targetCanvas =
                targetRect.GetComponentInParent<Canvas>();
        }

        if (worldCamera == null)
        {
            worldCamera =
                Camera.main;
        }
    }

    private void FitToTargetRect()
    {
        if (targetRect == null ||
            worldCamera == null ||
            spriteRenderer == null ||
            spriteRenderer.sprite == null)
        {
            return;
        }

        targetRect.GetWorldCorners(
            rectCorners
        );

        Camera uiCamera =
            GetUICamera();

        Vector2 bottomLeftScreen =
            RectTransformUtility.WorldToScreenPoint(
                uiCamera,
                rectCorners[0]
            );

        Vector2 topRightScreen =
            RectTransformUtility.WorldToScreenPoint(
                uiCamera,
                rectCorners[2]
            );

        Plane fittingPlane =
            new Plane(
                Vector3.forward,
                new Vector3(
                    0f,
                    0f,
                    worldZ
                )
            );

        if (!TryScreenPointToPlane(
                bottomLeftScreen,
                fittingPlane,
                out Vector3 bottomLeftWorld) ||
            !TryScreenPointToPlane(
                topRightScreen,
                fittingPlane,
                out Vector3 topRightWorld))
        {
            return;
        }

        float targetWidth =
            Mathf.Abs(
                topRightWorld.x -
                bottomLeftWorld.x
            );

        float targetHeight =
            Mathf.Abs(
                topRightWorld.y -
                bottomLeftWorld.y
            );

        Vector2 spriteSize =
            spriteRenderer.sprite.bounds.size;

        if (spriteSize.x <= 0f ||
            spriteSize.y <= 0f)
        {
            return;
        }

        Vector3 targetCenter =
            (
                bottomLeftWorld +
                topRightWorld
            ) *
            0.5f;

        transform.position =
            new Vector3(
                targetCenter.x,
                targetCenter.y,
                worldZ
            );

        Vector3 parentScale =
            transform.parent != null
                ? transform.parent.lossyScale
                : Vector3.one;

        float safeParentScaleX =
            Mathf.Max(
                0.0001f,
                Mathf.Abs(parentScale.x)
            );

        float safeParentScaleY =
            Mathf.Max(
                0.0001f,
                Mathf.Abs(parentScale.y)
            );

        transform.localScale =
            new Vector3(
                targetWidth /
                spriteSize.x /
                safeParentScaleX,

                targetHeight /
                spriteSize.y /
                safeParentScaleY,

                1f
            );
    }

    private Camera GetUICamera()
    {
        if (targetCanvas == null ||
            targetCanvas.renderMode ==
                RenderMode.ScreenSpaceOverlay)
        {
            return null;
        }

        return targetCanvas.worldCamera;
    }

    private bool TryScreenPointToPlane(
        Vector2 screenPoint,
        Plane fittingPlane,
        out Vector3 worldPoint)
    {
        Ray screenRay =
            worldCamera.ScreenPointToRay(
                screenPoint
            );

        if (!fittingPlane.Raycast(
                screenRay,
                out float distance))
        {
            worldPoint =
                Vector3.zero;

            return false;
        }

        worldPoint =
            screenRay.GetPoint(
                distance
            );

        return true;
    }
}