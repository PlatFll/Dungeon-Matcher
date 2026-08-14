using UnityEngine;
using UnityEngine.Serialization;

[DefaultExecutionOrder(-50)]
[RequireComponent(typeof(BoardController))]
public sealed class BoardLayoutController : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Camera worldCamera;

    [SerializeField]
    private RectTransform boardArea;

    [Header("Sizing")]
    [SerializeField, Range(0.1f, 2.5f)]
    private float maximumScale = 2f;

    [FormerlySerializedAs("areaPaddingPixels")]
    [SerializeField, Min(0f)]
    [Tooltip(
        "Vertical breathing room, in reference UI pixels, kept between the " +
        "board and the top/bottom of its Board Area."
    )]
    private float verticalAreaPaddingPixels = 8f;

    [SerializeField, Min(0f)]
    [Tooltip(
        "Horizontal breathing room, in reference UI pixels, between the outer " +
        "board frame and the phone safe-area edges."
    )]
    private float horizontalScreenPaddingPixels = 10f;

    [SerializeField, Range(0f, 0.25f)]
    [Tooltip(
        "Allows the board to grow slightly beyond the Board Area's vertical " +
        "fit when that is needed to reach the desired near-edge-to-edge width. " +
        "This is useful for tall boards while keeping the board visually large."
    )]
    private float maximumVerticalOverflowFraction = 0.12f;

    private BoardController board;
    private BoardVisuals boardVisuals;
    private Canvas parentCanvas;

    private readonly Vector3[] areaCorners =
        new Vector3[4];

    private void Awake()
    {
        board = GetComponent<BoardController>();
        boardVisuals = GetComponent<BoardVisuals>();

        if (worldCamera == null)
        {
            worldCamera = Camera.main;
        }

        if (boardArea != null)
        {
            parentCanvas =
                boardArea.GetComponentInParent<Canvas>();
        }
    }

    private void LateUpdate()
    {
        ApplyLayout();
    }

    private void ApplyLayout()
    {
        if (worldCamera == null ||
            boardArea == null ||
            board == null)
        {
            return;
        }

        boardArea.GetWorldCorners(areaCorners);

        Camera uiCamera = null;

        if (parentCanvas != null &&
            parentCanvas.renderMode !=
            RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = parentCanvas.worldCamera;
        }

        Vector2 areaScreenBottomLeft =
            RectTransformUtility.WorldToScreenPoint(
                uiCamera,
                areaCorners[0]
            );

        Vector2 areaScreenTopRight =
            RectTransformUtility.WorldToScreenPoint(
                uiCamera,
                areaCorners[2]
            );

        float referenceToScreenScale =
            GetReferenceToScreenPixelScale();

        float verticalPadding =
            verticalAreaPaddingPixels *
            referenceToScreenScale;

        float horizontalPadding =
            horizontalScreenPaddingPixels *
            referenceToScreenScale;

        Rect safeArea =
            Screen.safeArea;

        if (safeArea.width <= 0f ||
            safeArea.height <= 0f)
        {
            safeArea =
                new Rect(
                    0f,
                    0f,
                    Screen.width,
                    Screen.height
                );
        }

        /*
         * Width is intentionally based on the full phone safe area rather than
         * the narrower Board Area RectTransform. This lets the match board read
         * as the main interaction surface and brings its frame close to the
         * screen edges like the final mockup.
         */
        float screenLeft =
            safeArea.xMin +
            horizontalPadding;

        float screenRight =
            safeArea.xMax -
            horizontalPadding;

        /*
         * The Board Area still controls vertical placement. Its padding keeps a
         * small visual gap from the battle area and Bottom HUD.
         */
        float screenBottom =
            Mathf.Max(
                areaScreenBottomLeft.y +
                verticalPadding,
                safeArea.yMin
            );

        float screenTop =
            Mathf.Min(
                areaScreenTopRight.y -
                verticalPadding,
                safeArea.yMax
            );

        if (screenRight <= screenLeft ||
            screenTop <= screenBottom)
        {
            return;
        }

        float distanceFromCamera =
            Mathf.Abs(
                transform.position.z -
                worldCamera.transform.position.z
            );

        Vector3 worldBottomLeft =
            worldCamera.ScreenToWorldPoint(
                new Vector3(
                    screenLeft,
                    screenBottom,
                    distanceFromCamera
                )
            );

        Vector3 worldTopRight =
            worldCamera.ScreenToWorldPoint(
                new Vector3(
                    screenRight,
                    screenTop,
                    distanceFromCamera
                )
            );

        float availableWidth =
            Mathf.Abs(
                worldTopRight.x -
                worldBottomLeft.x
            );

        float availableHeight =
            Mathf.Abs(
                worldTopRight.y -
                worldBottomLeft.y
            );

        float localBoardWidth =
            boardVisuals != null
                ? boardVisuals.OuterLocalWidth
                : board.LocalBoardWidth;

        float localBoardHeight =
            boardVisuals != null
                ? boardVisuals.OuterLocalHeight
                : board.LocalBoardHeight;

        if (localBoardWidth <= 0f ||
            localBoardHeight <= 0f)
        {
            return;
        }

        float widthScale =
            availableWidth /
            localBoardWidth;

        float heightScale =
            availableHeight /
            localBoardHeight;

        /*
         * Prefer the large, near-edge-to-edge width. For taller board shapes we
         * allow a modest amount of vertical bleed beyond the nominal Board Area
         * instead of shrinking the entire board and leaving large side gutters.
         */
        float heightScaleWithBleed =
            heightScale *
            (1f +
             maximumVerticalOverflowFraction);

        float targetScale =
            Mathf.Min(
                widthScale,
                heightScaleWithBleed
            );

        targetScale =
            Mathf.Min(
                targetScale,
                maximumScale
            );

        targetScale =
            Mathf.Max(
                targetScale,
                0.01f
            );

        transform.localScale =
            new Vector3(
                targetScale,
                targetScale,
                1f
            );

        /*
         * Center horizontally in the safe area and vertically in the Board Area.
         * The modular frame grows with the board because BoardVisuals reports its
         * full outer dimensions, including the frame thickness.
         */
        Vector3 targetPosition =
            new Vector3(
                (
                    worldBottomLeft.x +
                    worldTopRight.x
                ) * 0.5f,
                (
                    worldBottomLeft.y +
                    worldTopRight.y
                ) * 0.5f,
                transform.position.z
            );

        transform.position =
            targetPosition;
    }

    private float GetReferenceToScreenPixelScale()
    {
        if (parentCanvas == null ||
            parentCanvas.scaleFactor <= 0f)
        {
            return 1f;
        }

        return parentCanvas.scaleFactor;
    }
}
