using UnityEngine;

[DisallowMultipleComponent]
public sealed class CrackedGemOverlayPresenter :
    MonoBehaviour
{
    private BoardController boardController;
    private float presentationUntilTime;

    public static CrackedGemOverlayPresenter
        EnsureInstalled(
            BoardController board)
    {
        if (board == null)
        {
            return null;
        }

        CrackedGemOverlayPresenter presenter =
            board.GetComponent<
                CrackedGemOverlayPresenter
            >();

        if (presenter == null)
        {
            presenter =
                board.gameObject.AddComponent<
                    CrackedGemOverlayPresenter
                >();
        }

        presenter.Configure(board);
        return presenter;
    }

    private void OnEnable()
    {
        ResolveReferences();
        Subscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void LateUpdate()
    {
        if (boardController == null ||
            Time.time > presentationUntilTime)
        {
            return;
        }

        Gem[] boardGems =
            boardController.GetComponentsInChildren<
                Gem
            >(true);

        for (int index = 0;
             index < boardGems.Length;
             index++)
        {
            Gem gem =
                boardGems[index];

            if (gem == null ||
                gem.SpecialType !=
                    GemSpecialType.Cracked)
            {
                continue;
            }

            CrackedGemOverlayView
                .EnsureInstalled(gem);
        }
    }

    public void Configure(
        BoardController board)
    {
        if (boardController == board)
        {
            return;
        }

        Unsubscribe();
        boardController = board;
        Subscribe();
    }

    private void ResolveReferences()
    {
        if (boardController == null)
        {
            boardController =
                GetComponent<BoardController>();
        }
    }

    private void Subscribe()
    {
        if (boardController == null)
        {
            return;
        }

        boardController.CrackedGemTargetsSelected -=
            HandleTargetsSelected;

        boardController.CrackedGemTargetsSelected +=
            HandleTargetsSelected;
    }

    private void Unsubscribe()
    {
        if (boardController == null)
        {
            return;
        }

        boardController.CrackedGemTargetsSelected -=
            HandleTargetsSelected;
    }

    private void HandleTargetsSelected(
        System.Collections.Generic
            .IReadOnlyList<Vector3> targetPositions,
        float travelDuration,
        float hoverDuration)
    {
        /*
         * The crack shake lasts one second by default. Keep the presenter
         * alive slightly beyond that so cracks created by the Color Crystal
         * interaction are picked up during the same resolution wave.
         */
        presentationUntilTime =
            Time.time +
            Mathf.Max(0f, travelDuration) +
            Mathf.Max(0f, hoverDuration) +
            1.5f;
    }
}
