using UnityEngine;

public partial class BoardController
{
    // Valid only while pointerStartGem is non-null. Existing cancellation,
    // external input blocks and accepted swaps already clear that authority.
    private int gesturePointerId;

    public void BeginPointerGesture(
        Gem gem,
        Vector2 screenPosition,
        int pointerId)
    {
        if (targetSelection != null)
        {
            if (!IsBusy && !IsExternalInputBlocked && Time.timeScale > 0)
                targetSelection.Invoke(gem);
            return;
        }
        // BeginPointer has no acceptance return value. Mirror its guards so a
        // rejected contact cannot steal the ID of an existing gesture.
        if (IsExternalInputBlocked ||
            isBusy ||
            HasPendingBoardMutation ||
            gem == null ||
            IsGemPinned(gem))
        {
            return;
        }

        // Preserve the existing latest-accepted-down behavior, including its
        // origin and activity notification. Older fingers may no longer drive
        // or cancel this newly accepted gesture, even on the very same gem.
        BeginPointer(gem, screenPosition);
        gesturePointerId = pointerId;
    }

    public void UpdatePointerGesture(
        Gem gem,
        Vector2 screenPosition,
        int pointerId)
    {
        if (OwnsPointerGesture(gem, pointerId))
        {
            UpdatePointerDrag(gem, screenPosition);
        }
    }

    public void EndPointerGesture(
        Gem gem,
        Vector2 screenPosition,
        int pointerId)
    {
        if (OwnsPointerGesture(gem, pointerId))
        {
            EndPointer(gem, screenPosition);
        }
    }

    private bool OwnsPointerGesture(Gem gem, int pointerId)
    {
        return gem != null &&
               pointerStartGem == gem &&
               gesturePointerId == pointerId;
    }

    /// <summary>
    /// Resolves a swipe as soon as the pointer crosses the configured
    /// threshold instead of waiting for pointer-up. This keeps tap selection
    /// unchanged while making touch input feel immediate on mobile.
    /// </summary>
    public void UpdatePointerDrag(
        Gem gem,
        Vector2 screenPosition)
    {
        if (IsExternalInputBlocked ||
            isBusy ||
            HasPendingBoardMutation ||
            gem == null ||
            IsGemPinned(gem) ||
            pointerStartGem != gem)
        {
            return;
        }

        Vector2 pointerDelta =
            screenPosition -
            pointerStartPosition;

        float minimumDistance =
            Mathf.Max(
                1f,
                swipeMinDistance
            );

        if (pointerDelta.sqrMagnitude <
            minimumDistance *
            minimumDistance)
        {
            return;
        }

        /*
         * Consume the gesture before starting the swap. Pointer-up will then
         * harmlessly no-op instead of attempting a second swap or selection.
         */
        pointerStartGem = null;

        TrySwapFromSwipe(
            gem,
            pointerDelta
        );
    }
}
