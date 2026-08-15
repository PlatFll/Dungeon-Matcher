using UnityEngine;

[DefaultExecutionOrder(25)]
[DisallowMultipleComponent]
public sealed class TopBattlePresentationController : MonoBehaviour
{
    private const string DefaultProfileResourcePath =
        "UI/TopBattlePresentationProfile";

    private const string TopHudName = "TopHUD";
    private const string GameAreaName = "GameArea";
    private const string BottomHudName = "BottomHUD";

    private const float FallbackBattleAreaHeight = 290f;
    private const float FallbackGapBelowBattleArea = 15f;
    private const float FallbackGapAboveBottomHud = 7f;
    private const float FallbackHorizontalGameAreaInset = 16f;
    private const float FallbackEnemyVisualScale = 0.9f;

    [Header("Presentation Profile")]
    [SerializeField]
    [Tooltip(
        "Optional profile override. When empty, the shared Resources/UI/" +
        "TopBattlePresentationProfile asset is loaded automatically."
    )]
    private TopBattlePresentationProfile profileOverride;

    private TopBattlePresentationProfile profile;
    private RectTransform topHud;
    private RectTransform safeArea;
    private RectTransform gameArea;
    private RectTransform bottomHud;
    private EnemySlotUI[] enemySlots;

    private Vector2 lastSafeAreaSize =
        new Vector2(float.NaN, float.NaN);

    private float lastBottomHudHeight = float.NaN;
    private bool enemyScaleApplied;

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
        ResolveReferences();
        ResolveProfile();
        ApplyLayoutMetrics();
    }

    private void OnEnable()
    {
        ResolveReferences();
        ResolveProfile();
        ApplyLayoutMetrics();
        enemyScaleApplied = false;
    }

    private void LateUpdate()
    {
        if (topHud == null ||
            gameArea == null ||
            bottomHud == null)
        {
            ResolveReferences();
        }

        if (HasResponsiveLayoutChanged())
        {
            ApplyLayoutMetrics();
        }

        /*
         * TopBattleLayoutController lays out the enemy spawn anchors in Start.
         * Apply the presentation scale once in LateUpdate so this controller
         * owns the final visual scale without affecting slot geometry, bases,
         * health bars, targeting, or combat logic.
         */
        if (!enemyScaleApplied)
        {
            ApplyEnemyVisualScale();
            enemyScaleApplied = true;
        }
    }

    public void RefreshPresentation()
    {
        ResolveReferences();
        ResolveProfile();
        ApplyLayoutMetrics();
        RefreshEnemySlots();
        ApplyEnemyVisualScale();
        enemyScaleApplied = true;
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

        RefreshEnemySlots();
    }

    private void RefreshEnemySlots()
    {
        if (topHud == null)
        {
            enemySlots = null;
            return;
        }

        enemySlots =
            topHud.GetComponentsInChildren<
                EnemySlotUI
            >(true);
    }

    private void ApplyLayoutMetrics()
    {
        if (topHud == null ||
            gameArea == null ||
            bottomHud == null)
        {
            return;
        }

        float battleAreaHeight =
            profile != null
                ? profile.BattleAreaHeight
                : FallbackBattleAreaHeight;

        float gapBelowBattleArea =
            profile != null
                ? profile.GapBelowBattleArea
                : FallbackGapBelowBattleArea;

        float gapAboveBottomHud =
            profile != null
                ? profile.GapAboveBottomHud
                : FallbackGapAboveBottomHud;

        float horizontalInset =
            profile != null
                ? profile.HorizontalGameAreaInset
                : FallbackHorizontalGameAreaInset;

        battleAreaHeight =
            Mathf.Max(200f, battleAreaHeight);

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
            battleAreaHeight
        );

        /*
         * Define the board's usable area by semantic edge offsets instead of
         * magic anchored-position/size pairs. This keeps the board between the
         * top battle arena and BottomHUD when either section changes size and
         * remains stable under CanvasScaler/SafeArea scaling.
         */
        gameArea.anchorMin =
            Vector2.zero;
        gameArea.anchorMax =
            Vector2.one;
        gameArea.pivot =
            new Vector2(0.5f, 0.5f);
        gameArea.offsetMin =
            new Vector2(
                horizontalInset,
                bottomHud.rect.height +
                gapAboveBottomHud
            );
        gameArea.offsetMax =
            new Vector2(
                -horizontalInset,
                -(battleAreaHeight +
                  gapBelowBattleArea)
            );

        lastSafeAreaSize =
            safeArea != null
                ? safeArea.rect.size
                : Vector2.zero;

        lastBottomHudHeight =
            bottomHud.rect.height;
    }

    private void ApplyEnemyVisualScale()
    {
        if (enemySlots == null ||
            enemySlots.Length == 0)
        {
            RefreshEnemySlots();
        }

        if (enemySlots == null)
        {
            return;
        }

        float scale =
            profile != null
                ? profile.EnemyVisualScale
                : FallbackEnemyVisualScale;

        scale =
            Mathf.Clamp(scale, 0.5f, 1.5f);

        Vector3 visualScale =
            new Vector3(scale, scale, 1f);

        foreach (EnemySlotUI slot in enemySlots)
        {
            if (slot == null ||
                slot.EnemySpawnAnchor == null)
            {
                continue;
            }

            slot.EnemySpawnAnchor.localScale =
                visualScale;
        }
    }

    private bool HasResponsiveLayoutChanged()
    {
        if (safeArea == null ||
            bottomHud == null)
        {
            return false;
        }

        Vector2 safeAreaSize =
            safeArea.rect.size;

        float bottomHudHeight =
            bottomHud.rect.height;

        return
            !Approximately(
                safeAreaSize,
                lastSafeAreaSize
            ) ||
            !Mathf.Approximately(
                bottomHudHeight,
                lastBottomHudHeight
            );
    }

    private static bool Approximately(
        Vector2 left,
        Vector2 right)
    {
        return
            Mathf.Approximately(
                left.x,
                right.x
            ) &&
            Mathf.Approximately(
                left.y,
                right.y
            );
    }

    private static RectTransform FindDirectChildRect(
        Transform parent,
        string childName)
    {
        if (parent == null)
        {
            return null;
        }

        Transform child =
            parent.Find(childName);

        return child as RectTransform;
    }
}
