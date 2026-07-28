using UnityEngine;

[DefaultExecutionOrder(10)]
[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public sealed class BoardTopUIFollower :
    MonoBehaviour
{
    [Header("References")]

    [SerializeField]
    private BoardVisuals boardVisuals;

    [SerializeField]
    private Camera worldCamera;

    [Header("Positioning")]

    [SerializeField, Min(0f)]
    [Tooltip(
        "Distance between the top of the board frame " +
        "and the bottom of this UI element."
    )]
    private float gapAboveBoard = 4f;

    private RectTransform targetRect;
    private RectTransform parentRect;
    private Canvas parentCanvas;

    private void Awake()
    {
        CacheReferences();
    }

    private void OnEnable()
    {
        CacheReferences();
    }

    private void LateUpdate()
    {
        FollowBoardTop();
    }

    private void CacheReferences()
    {
        targetRect =
            GetComponent<RectTransform>();

        if (targetRect != null)
        {
            parentRect =
                targetRect.parent as RectTransform;
        }

        parentCanvas =
            GetComponentInParent<Canvas>();

        if (worldCamera == null)
        {
            worldCamera =
                Camera.main;
        }
    }

    private void FollowBoardTop()
    {
        if (boardVisuals == null ||
            worldCamera == null ||
            targetRect == null ||
            parentRect == null)
        {
            return;
        }

        /*
         * OuterLocalHeight includes the complete board
         * frame, not just the playable gem area.
         */
        Vector3 boardTopLocalPosition =
            new Vector3(
                0f,
                boardVisuals.OuterLocalHeight * 0.5f,
                0f
            );

        Vector3 boardTopWorldPosition =
            boardVisuals.transform.TransformPoint(
                boardTopLocalPosition
            );

        Vector2 boardTopScreenPosition =
            RectTransformUtility.WorldToScreenPoint(
                worldCamera,
                boardTopWorldPosition
            );

        Camera uiCamera = null;

        if (parentCanvas != null &&
            parentCanvas.renderMode !=
            RenderMode.ScreenSpaceOverlay)
        {
            uiCamera =
                parentCanvas.worldCamera;
        }

        if (!RectTransformUtility
                .ScreenPointToLocalPointInRectangle(
                    parentRect,
                    boardTopScreenPosition,
                    uiCamera,
                    out Vector2 boardTopInParent
                ))
        {
            return;
        }

        /*
         * Position the bottom of the plaque above the
         * board instead of positioning its center there.
         */
        float halfPlaqueHeight =
            targetRect.rect.height * 0.5f;

        Vector3 newLocalPosition =
            targetRect.localPosition;

        newLocalPosition.x =
            boardTopInParent.x;

        newLocalPosition.y =
            boardTopInParent.y +
            gapAboveBoard +
            halfPlaqueHeight;

        targetRect.localPosition =
            newLocalPosition;
    }
}