using UnityEngine;

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
            uiCamera =
                parentCanvas.worldCamera;
        }

        Vector2 screenBottomLeft =
            RectTransformUtility.WorldToScreenPoint(
                uiCamera,
                areaCorners[0]
            );

        Vector2 screenTopRight =
            RectTransformUtility.WorldToScreenPoint(
                uiCamera,
                areaCorners[2]
            );

        if (screenTopRight.x <= screenBottomLeft.x ||
            screenTopRight.y <= screenBottomLeft.y)
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
                    screenBottomLeft.x,
                    screenBottomLeft.y,
                    distanceFromCamera
                )
            );

        Vector3 worldTopRight =
            worldCamera.ScreenToWorldPoint(
                new Vector3(
                    screenTopRight.x,
                    screenTopRight.y,
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

        if (availableWidth <= 0f ||
            availableHeight <= 0f ||
            localBoardWidth <= 0f ||
            localBoardHeight <= 0f)
        {
            return;
        }

        /*
         * GameplayScreen/TopBattlePresentationController already computes an
         * aspect-correct BoardArea from the phone safe area. Fit uniformly into
         * that exact slot: no independent X/Y scaling, no deliberate overflow,
         * and no second set of layout paddings competing with the UI stack.
         */
        float widthScale =
            availableWidth /
            localBoardWidth;

        float heightScale =
            availableHeight /
            localBoardHeight;

        float targetScale =
            Mathf.Min(
                widthScale,
                heightScale,
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

        transform.position =
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
    }
}
