using UnityEngine;

[DefaultExecutionOrder(110)]
[DisallowMultipleComponent]
public sealed class PlayerAreaFrameSpacingController : MonoBehaviour
{
    private const string GeneratedLayoutName =
        "GeneratedTopBattleLayout";

    private const string PlayerSectionName =
        "PlayerSection";

    private const string ThreeSliceFrameName =
        "PlayerAreaThreeSliceFrame";

    // The arena fitter derives its thickness from the imported sprite. Keep
    // the intended six-reference-pixel gap after that actual border.
    private const float GapInsideArena = 6f;

    private bool applied;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.AfterSceneLoad
    )]
    private static void InstallOnGameScene()
    {
        GameObject topHudObject =
            GameObject.Find("TopHUD");

        if (topHudObject == null)
        {
            return;
        }

        if (!topHudObject.TryGetComponent(
                out PlayerAreaFrameSpacingController _
            ))
        {
            topHudObject.AddComponent<
                PlayerAreaFrameSpacingController
            >();
        }
    }

    private void LateUpdate()
    {
        if (applied)
        {
            return;
        }

        RectTransform topHud =
            transform as RectTransform;

        if (topHud == null)
        {
            return;
        }

        RectTransform generatedLayout =
            topHud.Find(
                GeneratedLayoutName
            ) as RectTransform;

        if (generatedLayout == null)
        {
            return;
        }

        RectTransform playerSection =
            generatedLayout.Find(
                PlayerSectionName
            ) as RectTransform;

        if (playerSection == null ||
            playerSection.Find(
                ThreeSliceFrameName
            ) == null)
        {
            return;
        }

        Vector2 offsetMin =
            playerSection.offsetMin;

        Vector2 offsetMax =
            playerSection.offsetMax;

        RectTransform arenaEdge = generatedLayout.Find("BattleArenaFrame/TopEdge") as RectTransform;
        if (arenaEdge == null) return;
        float verticalInset = arenaEdge.rect.height + GapInsideArena;

        offsetMin.y =
            verticalInset;

        offsetMax.y =
            -verticalInset;

        playerSection.offsetMin =
            offsetMin;

        playerSection.offsetMax =
            offsetMax;

        if (TryGetComponent(
                out TopBattlePresentationController presentation
            ))
        {
            presentation.RefreshPresentation();
        }

        applied = true;
    }
}
