using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class BoardHintController :
    MonoBehaviour
{
    [Header("Board Reference")]
    [SerializeField]
    private BoardController boardController;

    [Header("Hint Timing")]
    [SerializeField, Min(0f)]
    private float delayBeforeHint = 5f;

    [SerializeField, Min(0.01f)]
    private float shakeDuration = 1f;

    [SerializeField, Min(0f)]
    private float pauseBetweenShakes = 0.5f;

    [Header("Hint Movement")]
    [SerializeField, Min(0f)]
    [Tooltip(
        "Maximum movement distance in sprite pixels."
    )]
    private float shakeDistancePixels = 3f;

    [SerializeField, Min(0.1f)]
    [Tooltip(
        "Number of back-and-forth movements per second."
    )]
    private float shakeCyclesPerSecond = 6f;

    [Header("Runtime Debug Information")]
    [SerializeField]
    private float inactivityTime;

    [SerializeField]
    private bool hintIsActive;

    private bool previousBoardBusy;

    private Coroutine hintCoroutine;

    private Gem hintedGem;
    private Vector3 hintedGemRestingPosition;

    private void OnEnable()
    {
        SubscribeToBoard();
    }

    private void Start()
    {
        if (!ValidateReferences())
        {
            enabled = false;
            return;
        }

        previousBoardBusy =
            boardController.IsBusy;

        inactivityTime = 0f;
    }

    private void Update()
    {
        if (boardController == null)
        {
            return;
        }

        bool boardIsBusy =
            boardController.IsBusy;

        if (boardIsBusy)
        {
            if (!previousBoardBusy ||
                hintCoroutine != null)
            {
                ResetHintTimer();
            }

            previousBoardBusy = true;
            return;
        }

        /*
         * A true-to-false transition means swaps,
         * matches, cascades, falling and reshuffling
         * have completely finished.
         */
        if (previousBoardBusy)
        {
            previousBoardBusy = false;
            ResetHintTimer();
        }

        if (hintCoroutine != null)
        {
            return;
        }

        inactivityTime +=
            Time.deltaTime;

        if (inactivityTime <
            delayBeforeHint)
        {
            return;
        }

        if (!boardController.TryGetRandomHintMove(
                out Gem sourceGem,
                out Gem targetGem))
        {
            /*
             * This normally means the board is still
             * initializing or temporarily has no move.
             */
            inactivityTime = 0f;
            return;
        }

        hintCoroutine =
            StartCoroutine(
                HintLoop(
                    sourceGem,
                    targetGem
                )
            );
    }

    private void SubscribeToBoard()
    {
        if (boardController == null)
        {
            return;
        }

        boardController.BoardActivityStarted -=
            HandleBoardActivity;

        boardController.BoardActivityStarted +=
            HandleBoardActivity;

        boardController.BoardMatchResolved -=
            HandleMatchResolved;

        boardController.BoardMatchResolved +=
            HandleMatchResolved;
    }

    private void UnsubscribeFromBoard()
    {
        if (boardController == null)
        {
            return;
        }

        boardController.BoardActivityStarted -=
            HandleBoardActivity;

        boardController.BoardMatchResolved -=
            HandleMatchResolved;
    }

    private void HandleBoardActivity()
    {
        ResetHintTimer();
    }

    private void HandleMatchResolved(
        BoardMatchContext context)
    {
        /*
         * Any normal match or cascade resets the
         * inactivity countdown.
         */
        ResetHintTimer();
    }

    private IEnumerator HintLoop(
        Gem sourceGem,
        Gem targetGem)
    {
        hintedGem =
            sourceGem;

        hintedGemRestingPosition =
            sourceGem.transform.localPosition;

        hintIsActive = true;

        float pixelsPerUnit =
            GetPixelsPerUnit(
                sourceGem
            );

        Vector3 direction =
            (
                targetGem.transform.localPosition -
                hintedGemRestingPosition
            ).normalized;

        float distanceInUnits =
            shakeDistancePixels /
            pixelsPerUnit;

        Vector3 maximumOffset =
            direction *
            distanceInUnits;

        while (CanContinueHint(
                   sourceGem,
                   targetGem))
        {
            float elapsedTime = 0f;

            while (elapsedTime <
                   shakeDuration)
            {
                if (!CanContinueHint(
                        sourceGem,
                        targetGem))
                {
                    FinishHintRoutine();
                    yield break;
                }

                float wave =
                    Mathf.Sin(
                        elapsedTime *
                        shakeCyclesPerSecond *
                        Mathf.PI *
                        2f
                    );

                Vector3 currentOffset =
                    maximumOffset *
                    wave;

                currentOffset =
                    SnapOffsetToPixelGrid(
                        currentOffset,
                        pixelsPerUnit
                    );

                sourceGem.transform.localPosition =
                    hintedGemRestingPosition +
                    currentOffset;

                elapsedTime +=
                    Time.deltaTime;

                yield return null;
            }

            RestoreHintedGemPosition();

            float pauseElapsed = 0f;

            while (pauseElapsed <
                   pauseBetweenShakes)
            {
                if (!CanContinueHint(
                        sourceGem,
                        targetGem))
                {
                    FinishHintRoutine();
                    yield break;
                }

                pauseElapsed +=
                    Time.deltaTime;

                yield return null;
            }
        }

        FinishHintRoutine();
    }

    private bool CanContinueHint(
        Gem sourceGem,
        Gem targetGem)
    {
        if (boardController == null ||
            boardController.IsBusy ||
            sourceGem == null ||
            targetGem == null)
        {
            return false;
        }

        int columnDistance =
            Mathf.Abs(
                sourceGem.Column -
                targetGem.Column
            );

        int rowDistance =
            Mathf.Abs(
                sourceGem.Row -
                targetGem.Row
            );

        return
            columnDistance +
            rowDistance == 1;
    }

    private static float GetPixelsPerUnit(
        Gem gem)
    {
        if (gem == null)
        {
            return 64f;
        }

        SpriteRenderer spriteRenderer =
            gem.GetComponent<SpriteRenderer>();

        if (spriteRenderer == null ||
            spriteRenderer.sprite == null)
        {
            return 64f;
        }

        return Mathf.Max(
            1f,
            spriteRenderer.sprite.pixelsPerUnit
        );
    }

    private static Vector3 SnapOffsetToPixelGrid(
        Vector3 offset,
        float pixelsPerUnit)
    {
        return new Vector3(
            Mathf.Round(
                offset.x *
                pixelsPerUnit
            ) / pixelsPerUnit,

            Mathf.Round(
                offset.y *
                pixelsPerUnit
            ) / pixelsPerUnit,

            0f
        );
    }

    private void ResetHintTimer()
    {
        StopHintAnimation();
        inactivityTime = 0f;
    }

    private void StopHintAnimation()
    {
        if (hintCoroutine != null)
        {
            StopCoroutine(
                hintCoroutine
            );

            hintCoroutine = null;
        }

        RestoreHintedGemPosition();

        hintedGem = null;
        hintIsActive = false;
    }

    private void RestoreHintedGemPosition()
    {
        if (hintedGem != null)
        {
            hintedGem.transform.localPosition =
                hintedGemRestingPosition;
        }
    }

    private void FinishHintRoutine()
    {
        RestoreHintedGemPosition();

        hintedGem = null;
        hintCoroutine = null;
        hintIsActive = false;

        inactivityTime = 0f;
    }

    private bool ValidateReferences()
    {
        if (boardController != null)
        {
            return true;
        }

        Debug.LogError(
            "BoardHintController requires a " +
            "BoardController reference.",
            this
        );

        return false;
    }

    private void OnDisable()
    {
        UnsubscribeFromBoard();
        StopHintAnimation();
    }

    private void OnValidate()
    {
        delayBeforeHint =
            Mathf.Max(
                0f,
                delayBeforeHint
            );

        shakeDuration =
            Mathf.Max(
                0.01f,
                shakeDuration
            );

        pauseBetweenShakes =
            Mathf.Max(
                0f,
                pauseBetweenShakes
            );

        shakeDistancePixels =
            Mathf.Max(
                0f,
                shakeDistancePixels
            );

        shakeCyclesPerSecond =
            Mathf.Max(
                0.1f,
                shakeCyclesPerSecond
            );
    }
}