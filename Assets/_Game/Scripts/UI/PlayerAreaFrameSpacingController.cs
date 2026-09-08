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

    /*
     * BattleArenaFrame uses a 10-reference-pixel edge. Keeping the player frame
     * 16 pixels in from the arena edge leaves a deliberate 6-pixel visual gap
     * above and below it instead of letting the two borders touch or overlap.
     */
    private const float VerticalInsetFromArenaEdge =
        16f;

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

        offsetMin.y =
            VerticalInsetFromArenaEdge;

        offsetMax.y =
            -VerticalInsetFromArenaEdge;

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
