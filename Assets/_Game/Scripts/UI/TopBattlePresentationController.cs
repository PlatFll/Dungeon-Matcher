using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-75)]
[DisallowMultipleComponent]
public sealed class TopBattlePresentationController : MonoBehaviour
{
    private const string DefaultProfileResourcePath =
        "UI/TopBattlePresentationProfile";

    private const string TopHudName = "TopHUD";
    private const string GameAreaName = "GameArea";
    private const string BottomHudName = "BottomHUD";
    private const string GeneratedLayoutName =
        "GeneratedTopBattleLayout";

    private const float FallbackReferenceBattleHeight = 290f;
    private const float FallbackMinimumBattleHeight = 220f;
    private const float FallbackGapBelowBattleArea = 8f;
    private const float FallbackGapAboveBottomHud = 8f;
    private const float FallbackBoardHorizontalInset = 10f;
    private const float FallbackPlayerVisualScale = 1f;
    private const float FallbackEnemyVisualScale = 0.802f;
    private const float FallbackCharacterScaleResponse = 0.45f;
    private const float FallbackMinimumResponsiveScale = 0.88f;
    private const float FallbackMaximumResponsiveScale = 1.25f;

    private const float PlayerCharacterFloorOffset = 43f;
    private const float PlayerBaseFloorOffset = 48f;
    private const float EnemyCharacterFloorOffset = 43f;
    private const float EnemyBaseFloorOffset = 48f;

    [Header("Presentation Profile")]
    [SerializeField]
    [Tooltip(
        "Optional profile override. When empty, Resources/UI/" +
        "TopBattlePresentationProfile is loaded automatically."
    )]
    private TopBattlePresentationProfile profileOverride;

    private TopBattlePresentationProfile profile;
    private RectTransform topHud;
    private RectTransform safeArea;
    private RectTransform gameArea;
    private RectTransform bottomHud;
    private BoardVisuals boardVisuals;

    private Vector2 lastSafeAreaSize =
        new Vector2(float.NaN, float.NaN);

    private float lastBottomHudHeight = float.NaN;
    private Vector2 lastBoardOuterSize =
        new Vector2(float.NaN, float.NaN);

    private float currentBattleHeight =
        FallbackReferenceBattleHeight;

    private bool battlePresentationDirty = true;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.AfterSceneLoad
    )]
    private static void InstallOnGameScene()
    {
        GameObject topHudObject =
            GameObject.Find(TopHudName);

        if (topHudObject == null)
        {
            return;
        }

        if (!topHudObject.TryGetComponent(
                out TopBattlePresentationController _
            ))
        {
            topHudObject.AddComponent<
                TopBattlePresentationController
            >();
        }
    }

    private void Awake()
    {
        ResolveProfile();
        ResolveReferences();
        ApplyResponsiveStackLayout();
    }

    private void OnEnable()
    {
        ResolveProfile();
        ResolveReferences();
        ApplyResponsiveStackLayout();
        battlePresentationDirty = true;
    }

    private void LateUpdate()
    {
        if (topHud == null ||
            safeArea == null ||
            gameArea == null ||
            bottomHud == null ||
            boardVisuals == null)
        {
            ResolveReferences();
        }

        if (HasResponsiveLayoutChanged())
        {
            ApplyResponsiveStackLayout();
            battlePresentationDirty = true;
        }

        if (battlePresentationDirty &&
            TryApplyBattlePresentation())
        {
            battlePresentationDirty = false;
        }
    }

    public void RefreshPresentation()
    {
        ResolveProfile();
        ResolveReferences();
        ApplyResponsiveStackLayout();
        battlePresentationDirty = true;
    }

    private void ResolveProfile()
    {
        profile =
            profileOverride != null
                ? profileOverride
                : Resources.Load<TopBattlePresentationProfile>(
                    DefaultProfileResourcePath
                );
    }

    private void ResolveReferences()
    {
        topHud =
            transform as RectTransform;

        safeArea =
            topHud != null
                ? topHud.parent as RectTransform
                : null;

        if (safeArea == null)
        {
            return;
        }

        gameArea =
            FindDirectChildRect(
                safeArea,
                GameAreaName
            );

        bottomHud =
            FindDirectChildRect(
                safeArea,
                BottomHudName
            );

        boardVisuals =
            FindFirstObjectByType<BoardVisuals>();
    }

    private void ApplyResponsiveStackLayout()
    {
        if (topHud == null ||
            safeArea == null ||
            gameArea == null ||
            bottomHud == null)
        {
            return;
        }

        Vector2 safeSize =
            safeArea.rect.size;

        if (safeSize.x <= 0f ||
            safeSize.y <= 0f)
        {
            return;
        }

        float bottomHudHeight =
            Mathf.Max(
                0f,
                bottomHud.rect.height
            );

        float referenceBattleHeight =
            profile != null
                ? profile.ReferenceBattleAreaHeight
                : FallbackReferenceBattleHeight;

        float minimumBattleHeight =
            profile != null
                ? profile.MinimumBattleAreaHeight
                : FallbackMinimumBattleHeight;

        float gapBelowBattle =
            profile != null
                ? profile.GapBelowBattleArea
                : FallbackGapBelowBattleArea;

        float gapAboveBottom =
            profile != null
                ? profile.GapAboveBottomHud
                : FallbackGapAboveBottomHud;

        float horizontalInset =
            profile != null
                ? profile.BoardHorizontalInset
                : FallbackBoardHorizontalInset;

        referenceBattleHeight =
            Mathf.Max(1f, referenceBattleHeight);

        minimumBattleHeight =
            Mathf.Clamp(
                minimumBattleHeight,
                1f,
                referenceBattleHeight
            );

        horizontalInset =
            Mathf.Clamp(
                horizontalInset,
                0f,
                safeSize.x * 0.45f
            );

        float boardAspect =
            GetBoardOuterAspectRatio();

        float preferredBoardWidth =
            Mathf.Max(
                1f,
                safeSize.x -
                horizontalInset * 2f
            );

        float preferredBoardHeight =
            preferredBoardWidth *
            boardAspect;

        float availableForBattleAndBoard =
            Mathf.Max(
                1f,
                safeSize.y -
                bottomHudHeight -
                gapBelowBattle -
                gapAboveBottom
            );

        /*
         * The board is width-driven on normal portrait phones. Only unusually
         * short windows can make height the limiting constraint, and even then
         * the battle area keeps a defined minimum before the board is reduced.
         */
        float maximumBoardHeight =
            Mathf.Max(
                1f,
                availableForBattleAndBoard -
                minimumBattleHeight
            );

        float boardHeight =
            Mathf.Min(
                preferredBoardHeight,
                maximumBoardHeight
            );

        float boardWidth =
            boardAspect > 0f
                ? boardHeight /
                  boardAspect
                : preferredBoardWidth;

        boardWidth =
            Mathf.Min(
                boardWidth,
                preferredBoardWidth
            );

        currentBattleHeight =
            Mathf.Max(
                1f,
                availableForBattleAndBoard -
                boardHeight
            );

        topHud.anchorMin =
            new Vector2(0f, 1f);
        topHud.anchorMax =
            new Vector2(1f, 1f);
        topHud.pivot =
            new Vector2(0.5f, 1f);
        topHud.anchoredPosition =
            Vector2.zero;
        topHud.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            currentBattleHeight
        );

        float horizontalBoardMargin =
            Mathf.Max(
                0f,
                (safeSize.x - boardWidth) *
                0.5f
            );

        /*
         * GameArea becomes the exact slot reserved for the width-driven board.
         * This prevents extra device height from turning into dead space around
         * the board; all remaining height is intentionally given to the battle
         * presentation above it.
         */
        gameArea.anchorMin =
            Vector2.zero;
        gameArea.anchorMax =
            Vector2.one;
        gameArea.pivot =
            new Vector2(0.5f, 0.5f);
        gameArea.offsetMin =
            new Vector2(
                horizontalBoardMargin,
                bottomHudHeight +
                gapAboveBottom
            );
        gameArea.offsetMax =
            new Vector2(
                -horizontalBoardMargin,
                -(currentBattleHeight +
                  gapBelowBattle)
            );

        lastSafeAreaSize = safeSize;
        lastBottomHudHeight = bottomHudHeight;
        lastBoardOuterSize =
            GetBoardOuterSize();
    }

    private bool TryApplyBattlePresentation()
    {
        if (topHud == null)
        {
            return false;
        }

        Transform generatedLayout =
            topHud.Find(GeneratedLayoutName);

        if (generatedLayout == null)
        {
            return false;
        }

        float responsiveScale =
            CalculateResponsiveCharacterScale();

        float playerMultiplier =
            profile != null
                ? profile.PlayerVisualScale
                : FallbackPlayerVisualScale;

        float enemyMultiplier =
            profile != null
                ? profile.EnemyVisualScale
                : FallbackEnemyVisualScale;

        float playerScale =
            responsiveScale *
            playerMultiplier;

        float enemyScale =
            responsiveScale *
            enemyMultiplier;

        RectTransform playerCharacter =
            FindRectTransform(
                generatedLayout,
                "PlayerCharacter"
            );

        if (playerCharacter != null)
        {
            AnchorVisualToFloor(
                playerCharacter,
                PlayerCharacterFloorOffset,
                playerScale,
                useBottomPivot: true
            );
        }

        RectTransform playerBase =
            FindRectTransform(
                generatedLayout,
                "PlayerBase"
            );

        if (playerBase != null)
        {
            AnchorVisualToFloor(
                playerBase,
                PlayerBaseFloorOffset,
                playerScale,
                useBottomPivot: false
            );
        }

        EnemySlotUI[] slots =
            topHud.GetComponentsInChildren<
                EnemySlotUI
            >(true);

        foreach (EnemySlotUI slot in slots)
        {
            if (slot == null)
            {
                continue;
            }

            RectTransform spawnAnchor =
                slot.EnemySpawnAnchor;

            if (spawnAnchor != null)
            {
                AnchorVisualToFloor(
                    spawnAnchor,
                    EnemyCharacterFloorOffset,
                    enemyScale,
                    useBottomPivot: true
                );
            }

            RectTransform enemyBase =
                FindRectTransform(
                    slot.transform,
                    "EnemyBase"
                );

            if (enemyBase != null)
            {
                AnchorVisualToFloor(
                    enemyBase,
                    EnemyBaseFloorOffset,
                    enemyScale,
                    useBottomPivot: false
                );
            }
        }

        EnsureBackgroundFitter(
            generatedLayout,
            "PlayerSectionBackground"
        );

        EnsureBackgroundFitter(
            generatedLayout,
            "EnemySectionBackground"
        );

        EnsureFrameFitter(
            generatedLayout,
            "BattleArenaFrame"
        );

        EnsureFrameFitter(
            generatedLayout,
            "PlayerSectionFrame"
        );

        return true;
    }

    private float CalculateResponsiveCharacterScale()
    {
        float referenceHeight =
            profile != null
                ? profile.ReferenceBattleAreaHeight
                : FallbackReferenceBattleHeight;

        float response =
            profile != null
                ? profile.CharacterScaleResponse
                : FallbackCharacterScaleResponse;

        float minimumScale =
            profile != null
                ? profile.MinimumResponsiveCharacterScale
                : FallbackMinimumResponsiveScale;

        float maximumScale =
            profile != null
                ? profile.MaximumResponsiveCharacterScale
                : FallbackMaximumResponsiveScale;

        referenceHeight =
            Mathf.Max(1f, referenceHeight);

        float heightRatio =
            currentBattleHeight /
            referenceHeight;

        float responsiveScale =
            Mathf.Lerp(
                1f,
                heightRatio,
                Mathf.Clamp01(response)
            );

        return Mathf.Clamp(
            responsiveScale,
            minimumScale,
            Mathf.Max(
                minimumScale,
                maximumScale
            )
        );
    }

    private float GetBoardOuterAspectRatio()
    {
        Vector2 size =
            GetBoardOuterSize();

        if (size.x <= 0f ||
            size.y <= 0f)
        {
            return 1f;
        }

        return size.y /
               size.x;
    }

    private Vector2 GetBoardOuterSize()
    {
        if (boardVisuals == null)
        {
            return Vector2.zero;
        }

        return new Vector2(
            boardVisuals.OuterLocalWidth,
            boardVisuals.OuterLocalHeight
        );
    }

    private bool HasResponsiveLayoutChanged()
    {
        if (safeArea == null ||
            bottomHud == null)
        {
            return false;
        }

        Vector2 safeSize =
            safeArea.rect.size;

        float bottomHeight =
            bottomHud.rect.height;

        Vector2 boardOuterSize =
            GetBoardOuterSize();

        return
            !Approximately(
                safeSize,
                lastSafeAreaSize
            ) ||
            !Mathf.Approximately(
                bottomHeight,
                lastBottomHudHeight
            ) ||
            !Approximately(
                boardOuterSize,
                lastBoardOuterSize
            );
    }

    private static void AnchorVisualToFloor(
        RectTransform visual,
        float floorOffset,
        float uniformScale,
        bool useBottomPivot)
    {
        if (visual == null)
        {
            return;
        }

        visual.anchorMin =
            new Vector2(0.5f, 0f);
        visual.anchorMax =
            new Vector2(0.5f, 0f);
        visual.pivot =
            useBottomPivot
                ? new Vector2(0.5f, 0f)
                : new Vector2(0.5f, 0.5f);
        visual.anchoredPosition =
            new Vector2(
                0f,
                floorOffset
            );
        visual.localScale =
            new Vector3(
                uniformScale,
                uniformScale,
                1f
            );
    }

    private static void EnsureBackgroundFitter(
        Transform root,
        string objectName)
    {
        RectTransform rect =
            FindRectTransform(
                root,
                objectName
            );

        if (rect == null ||
            !rect.TryGetComponent(
                out Image image
            ))
        {
            return;
        }

        BottomAnchoredBackgroundFitter fitter;

        if (!rect.TryGetComponent(
                out fitter
            ))
        {
            fitter =
                rect.gameObject.AddComponent<
                    BottomAnchoredBackgroundFitter
                >();
        }

        image.raycastTarget = false;
        fitter.RefreshLayout();
    }

    private static void EnsureFrameFitter(
        Transform root,
        string objectName)
    {
        RectTransform frame =
            FindRectTransform(
                root,
                objectName
            );

        if (frame == null)
        {
            return;
        }

        ResponsiveModularFrameFitter fitter;

        if (!frame.TryGetComponent(
                out fitter
            ))
        {
            fitter =
                frame.gameObject.AddComponent<
                    ResponsiveModularFrameFitter
                >();
        }

        fitter.RefreshFrame();
    }

    private static RectTransform FindDirectChildRect(
        Transform parent,
        string childName)
    {
        if (parent == null)
        {
            return null;
        }

        return parent.Find(childName) as RectTransform;
    }

    private static RectTransform FindRectTransform(
        Transform root,
        string objectName)
    {
        if (root == null)
        {
            return null;
        }

        if (root.name == objectName)
        {
            return root as RectTransform;
        }

        for (int index = 0;
             index < root.childCount;
             index++)
        {
            RectTransform match =
                FindRectTransform(
                    root.GetChild(index),
                    objectName
                );

            if (match != null)
            {
                return match;
            }
        }

        return null;
    }

    private static bool Approximately(
        Vector2 left,
        Vector2 right)
    {
        return
            Mathf.Approximately(left.x, right.x) &&
            Mathf.Approximately(left.y, right.y);
    }
}
