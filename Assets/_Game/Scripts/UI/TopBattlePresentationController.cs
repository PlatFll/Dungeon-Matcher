using UnityEngine;

[DefaultExecutionOrder(-75)]
[DisallowMultipleComponent]
public sealed class TopBattlePresentationController : MonoBehaviour
{
    private const string DefaultProfileResourcePath =
        "UI/TopBattlePresentationProfile";


    private const string GameAreaName = "GameArea";
    private const string BottomHudName = "BottomHUD";
    private const string GeneratedLayoutName =
        "GeneratedTopBattleLayout";


    private const float FallbackBattleFloorOffsetFromBottom = 58f;
    private const float FallbackCharacterFeetOffsetFromFloor = 0f;
    private const float FallbackBaseCenterOffsetFromFloor = -3f;

    [Header("Presentation Profile")]
    [SerializeField]
    [Tooltip(
        "Optional profile override. When empty, Resources/UI/" +
        "TopBattlePresentationProfile is loaded automatically."
    )]
    private TopBattlePresentationProfile profileOverride;

    [Header("Battle Floor")]
    [SerializeField, Tooltip("Authoritative floor. Defaults to the serialized TopHUD/BattleFloorAnchor child.")]
    private RectTransform battleFloorAnchor;
    private Vector3 lastFloorWorld = new Vector3(float.NaN, float.NaN, float.NaN);

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

    private bool battlePresentationDirty = true;

    private void Awake()
    {
        ResolveProfile();
        ResolveReferences();
        CacheAssignedGeometry();
    }

    private void OnEnable()
    {
        ResolveProfile();
        ResolveReferences();
        CacheAssignedGeometry();
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

        if (HasResponsiveLayoutChanged() ||
            (battleFloorAnchor != null && battleFloorAnchor.position != lastFloorWorld))
        {
            CacheAssignedGeometry();
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
        CacheAssignedGeometry();
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

        if (battleFloorAnchor == null && topHud != null)
            battleFloorAnchor = topHud.Find("BattleFloorAnchor") as RectTransform;

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

    // Major rectangles belong exclusively to GameplayPixelLayoutController.
    private void CacheAssignedGeometry()
    {
        if (topHud == null || safeArea == null || bottomHud == null) return;

        lastSafeAreaSize = safeArea.rect.size;
        lastBottomHudHeight = bottomHud.rect.height;
        lastBoardOuterSize = GetBoardOuterSize();
        if (battleFloorAnchor != null) lastFloorWorld = battleFloorAnchor.position;
    }
    private bool TryApplyBattlePresentation()
    {
        if (topHud == null)
        {
            return false;
        }

        RectTransform generatedLayout =
            topHud.Find(GeneratedLayoutName)
                as RectTransform;

        if (generatedLayout == null)
        {
            return false;
        }

        if (TryGetComponent(out TopBattleLayoutController structure)) structure.LayoutEnemyArea();
        float floorOffsetFromBattleBottom =
            profile != null
                ? profile.BattleFloorOffsetFromBottom
                : FallbackBattleFloorOffsetFromBottom;

        float feetOffsetFromFloor =
            profile != null
                ? profile.CharacterFeetOffsetFromFloor
                : FallbackCharacterFeetOffsetFromFloor;

        float baseOffsetFromFloor =
            profile != null
                ? profile.BaseCenterOffsetFromFloor
                : FallbackBaseCenterOffsetFromFloor;

        Vector3 sharedFloorWorld =
            battleFloorAnchor != null ? battleFloorAnchor.position : GetSharedFloorWorldPosition(
                generatedLayout,
                floorOffsetFromBattleBottom
            );

        Vector3 playerFloorWorld = sharedFloorWorld + generatedLayout.TransformVector(
            Vector3.up * Mathf.Ceil(Mathf.Max(0, generatedLayout.rect.height - 290f) / 2f));

        RectTransform playerCharacter =
            FindRectTransform(
                generatedLayout,
                "PlayerCharacter"
            );

        if (playerCharacter != null)
        {
            AnchorVisualToSharedFloor(
                playerCharacter,
                playerFloorWorld,
                feetOffsetFromFloor,
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
            AnchorVisualToSharedFloor(
                playerBase,
                playerFloorWorld,
                baseOffsetFromFloor,
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
                AnchorVisualToSharedFloor(
                    spawnAnchor,
                    sharedFloorWorld,
                    feetOffsetFromFloor,
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
                AnchorVisualToSharedFloor(
                    enemyBase,
                    sharedFloorWorld,
                    baseOffsetFromFloor,
                    useBottomPivot: false
                );
            }
        }

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

    private static Vector3 GetSharedFloorWorldPosition(
        RectTransform battleRoot,
        float floorOffsetFromBottom)
    {
        float clampedOffset =
            Mathf.Clamp(
                floorOffsetFromBottom,
                0f,
                Mathf.Max(
                    0f,
                    battleRoot.rect.height
                )
            );

        float localFloorY =
            battleRoot.rect.yMin +
            clampedOffset;

        return battleRoot.TransformPoint(
            new Vector3(
                0f,
                localFloorY,
                0f
            )
        );
    }

    private static void AnchorVisualToSharedFloor(
        RectTransform visual,
        Vector3 sharedFloorWorld,
        float visualOffsetFromFloor,
        bool useBottomPivot)
    {
        if (visual == null ||
            visual.parent is not RectTransform parent)
        {
            return;
        }

        Vector3 localFloor =
            parent.InverseTransformPoint(
                sharedFloorWorld
            );

        float floorFromParentBottom =
            localFloor.y -
            parent.rect.yMin;

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
                Mathf.Round(
                    floorFromParentBottom +
                    visualOffsetFromFloor
                )
            );

        visual.localScale = Vector3.one;
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
