using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(200)]
[DisallowMultipleComponent]
public sealed class BottomHudPresentationTuner : MonoBehaviour
{
    private const string BottomHudName = "BottomHUD";
    private const string GeneratedFrameName = "GeneratedBottomHudFrame";

    // Keep the larger 104px BottomHUD and 50px corners, but render the straight
    // pieces slightly thinner so their visible band lines up with the outgoing
    // arms of the shared corner art. This tuning is BottomHUD-only; the board
    // and battle arena keep their existing modular frame sizing.
    private const float TargetHudHeight = 104f;
    private const float FrameCornerSize = 50f;
    private const float FrameThickness = 8f;
    private const float EdgePixelsPerUnitMultiplier = 1f;

    private RectTransform bottomHud;
    private bool applied;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InstallOnGameScene()
    {
        GameObject bottomHudObject = GameObject.Find(BottomHudName);

        if (bottomHudObject == null)
        {
            return;
        }

        if (!bottomHudObject.TryGetComponent(out BottomHudPresentationTuner _))
        {
            bottomHudObject.AddComponent<BottomHudPresentationTuner>();
        }
    }

    private void Awake()
    {
        bottomHud = transform as RectTransform;
    }

    private void LateUpdate()
    {
        if (!applied)
        {
            TryApply();
        }
    }

    private void TryApply()
    {
        if (bottomHud == null)
        {
            bottomHud = transform as RectTransform;
        }

        if (bottomHud == null)
        {
            return;
        }

        Transform frameTransform = bottomHud.Find(GeneratedFrameName);

        if (frameTransform is not RectTransform frameRoot)
        {
            return;
        }

        bottomHud.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            TargetHudHeight
        );

        ConfigureCorner(frameRoot, "TopLeftCorner", new Vector2(0f, 1f),
            new Vector2(FrameCornerSize * 0.5f, -FrameCornerSize * 0.5f));
        ConfigureCorner(frameRoot, "TopRightCorner", new Vector2(1f, 1f),
            new Vector2(-FrameCornerSize * 0.5f, -FrameCornerSize * 0.5f));
        ConfigureCorner(frameRoot, "BottomLeftCorner", new Vector2(0f, 0f),
            new Vector2(FrameCornerSize * 0.5f, FrameCornerSize * 0.5f));
        ConfigureCorner(frameRoot, "BottomRightCorner", new Vector2(1f, 0f),
            new Vector2(-FrameCornerSize * 0.5f, FrameCornerSize * 0.5f));

        ConfigureHorizontalEdge(frameRoot, "TopEdge", true);
        ConfigureHorizontalEdge(frameRoot, "BottomEdge", false);
        ConfigureVerticalEdge(frameRoot, "LeftEdge", true);
        ConfigureVerticalEdge(frameRoot, "RightEdge", false);

        CenterAbilityContent();
        applied = true;
    }

    private static void ConfigureCorner(
        RectTransform frameRoot,
        string objectName,
        Vector2 anchor,
        Vector2 anchoredPosition)
    {
        RectTransform rect = FindChildRect(frameRoot, objectName);

        if (rect == null)
        {
            return;
        }

        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(FrameCornerSize, FrameCornerSize);
    }

    private static void ConfigureHorizontalEdge(
        RectTransform frameRoot,
        string objectName,
        bool top)
    {
        RectTransform rect = FindChildRect(frameRoot, objectName);

        if (rect == null)
        {
            return;
        }

        float yAnchor = top ? 1f : 0f;
        rect.anchorMin = new Vector2(0f, yAnchor);
        rect.anchorMax = new Vector2(1f, yAnchor);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = new Vector2(
            FrameCornerSize,
            top ? -FrameThickness : 0f
        );
        rect.offsetMax = new Vector2(
            -FrameCornerSize,
            top ? 0f : FrameThickness
        );

        if (rect.TryGetComponent(out Image image))
        {
            image.type = Image.Type.Tiled;
            image.pixelsPerUnitMultiplier = EdgePixelsPerUnitMultiplier;
        }
    }

    private static void ConfigureVerticalEdge(
        RectTransform frameRoot,
        string objectName,
        bool left)
    {
        RectTransform rect = FindChildRect(frameRoot, objectName);

        if (rect == null)
        {
            return;
        }

        rect.anchorMin = new Vector2(left ? 0f : 1f, 0.5f);
        rect.anchorMax = rect.anchorMin;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(
            left ? FrameThickness * 0.5f : -FrameThickness * 0.5f,
            0f
        );

        float verticalLength = Mathf.Max(
            1f,
            TargetHudHeight - FrameCornerSize * 2f
        );

        rect.sizeDelta = new Vector2(verticalLength, FrameThickness);
        rect.localRotation = Quaternion.Euler(
            0f,
            0f,
            left ? 90f : -90f
        );

        if (rect.TryGetComponent(out Image image))
        {
            image.type = Image.Type.Tiled;
            image.pixelsPerUnitMultiplier = EdgePixelsPerUnitMultiplier;
        }
    }

    private void CenterAbilityContent()
    {
        AbilityButtonUI abilityUi =
            bottomHud.GetComponentInChildren<AbilityButtonUI>(true);

        if (abilityUi == null ||
            abilityUi.transform is not RectTransform abilityRect)
        {
            return;
        }

        Vector2 position = abilityRect.anchoredPosition;
        position.y = 0f;
        abilityRect.anchoredPosition = position;
    }

    private static RectTransform FindChildRect(
        RectTransform parent,
        string objectName)
    {
        Transform child = parent.Find(objectName);
        return child as RectTransform;
    }
}
