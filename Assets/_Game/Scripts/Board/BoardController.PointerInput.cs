using UnityEngine;

public partial class BoardController
{
    /// <summary>
    /// Resolves a swipe as soon as the pointer crosses the configured
    /// threshold instead of waiting for pointer-up. This keeps tap selection
    /// unchanged while making touch input feel immediate on mobile.
    /// </summary>
    public void UpdatePointerDrag(
        Gem gem,
        Vector2 screenPosition)
    {
        if (isBusy ||
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
