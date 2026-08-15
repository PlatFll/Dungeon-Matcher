using UnityEngine;

[DisallowMultipleComponent]
public sealed class ResponsiveModularFrameFitter : MonoBehaviour
{
    private RectTransform frameRoot;
    private RectTransform framedTarget;
    private RectTransform topLeftCorner;
    private RectTransform topEdge;
    private RectTransform bottomEdge;
    private RectTransform leftEdge;
    private RectTransform rightEdge;

    private Vector2 lastTargetSize =
        new Vector2(float.NaN, float.NaN);

    private void Awake()
    {
        ResolveReferences();
        RefreshFrame();
    }

    private void OnEnable()
    {
        ResolveReferences();
        RefreshFrame();
    }

    private void LateUpdate()
    {
        if (framedTarget == null)
        {
            ResolveReferences();
        }

        if (framedTarget == null)
        {
            return;
        }

        Vector2 targetSize =
            framedTarget.rect.size;

        if (!Approximately(
                targetSize,
                lastTargetSize
            ))
        {
            RefreshFrame();
        }
    }

    public void RefreshFrame()
    {
        ResolveReferences();

        if (frameRoot == null ||
            framedTarget == null ||
            topLeftCorner == null)
        {
            return;
        }

        float cornerWidth =
            Mathf.Abs(topLeftCorner.rect.width);

        float cornerHeight =
            Mathf.Abs(topLeftCorner.rect.height);

        float horizontalThickness =
            topEdge != null
                ? Mathf.Abs(topEdge.rect.height)
                : 0f;

        float verticalLength =
            Mathf.Max(
                0f,
                framedTarget.rect.height -
                cornerHeight * 2f
            );

        if (leftEdge != null)
        {
            leftEdge.sizeDelta =
                new Vector2(
                    verticalLength,
                    horizontalThickness
                );
        }

        if (rightEdge != null)
        {
            rightEdge.sizeDelta =
                new Vector2(
                    verticalLength,
                    horizontalThickness
                );
        }

        /*
         * Horizontal edges are anchor-stretched by TopBattleLayoutController.
         * Reassert the corner offsets so their tiled middle always starts and
         * ends at the corner artwork after an aspect-ratio change.
         */
        if (topEdge != null)
        {
            topEdge.offsetMin =
                new Vector2(
                    cornerWidth,
                    topEdge.offsetMin.y
                );
            topEdge.offsetMax =
                new Vector2(
                    -cornerWidth,
                    topEdge.offsetMax.y
                );
        }

        if (bottomEdge != null)
        {
            bottomEdge.offsetMin =
                new Vector2(
                    cornerWidth,
                    bottomEdge.offsetMin.y
                );
            bottomEdge.offsetMax =
                new Vector2(
                    -cornerWidth,
                    bottomEdge.offsetMax.y
                );
        }

        lastTargetSize =
            framedTarget.rect.size;
    }

    private void ResolveReferences()
    {
        frameRoot =
            transform as RectTransform;

        framedTarget =
            transform.parent as RectTransform;

        if (frameRoot == null)
        {
            return;
        }

        topLeftCorner =
            FindDirectChild("TopLeftCorner");
        topEdge =
            FindDirectChild("TopEdge");
        bottomEdge =
            FindDirectChild("BottomEdge");
        leftEdge =
            FindDirectChild("LeftEdge");
        rightEdge =
            FindDirectChild("RightEdge");
    }

    private RectTransform FindDirectChild(
        string childName)
    {
        Transform child =
            frameRoot.Find(childName);

        return child as RectTransform;
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
