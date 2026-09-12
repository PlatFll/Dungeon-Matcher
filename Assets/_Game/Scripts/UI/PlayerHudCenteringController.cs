using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Keeps the visible player HUD stack centered inside the PlayerPanel after all
/// responsive battle-layout and pixel-perfect character sizing has finished.
/// </summary>
[DefaultExecutionOrder(10020)]
[DisallowMultipleComponent]
public sealed class PlayerHudCenteringController : MonoBehaviour
{
    private const string CharacterName = "PlayerCharacter";
    private const string AffinityGemName = "PlayerAffinityGem";
    private const string HealthBarName = "PlayerHPBarBackground";
    private const float Epsilon = 0.01f;

    private RectTransform panel;
    private RectTransform character;
    private RectTransform affinityGem;
    private RectTransform healthBar;
    private readonly Vector3[] worldCorners = new Vector3[4];

    private void Awake()
    {
        panel = transform as RectTransform;
        ResolveReferences();
    }

    private void OnEnable()
    {
        if (panel == null)
        {
            panel = transform as RectTransform;
        }

        ResolveReferences();
    }

    private void LateUpdate()
    {
        if (panel == null)
        {
            return;
        }

        ResolveReferences();

        if (!TryGetStackLocalBounds(out Rect bounds))
        {
            return;
        }

        float logicalShift = Mathf.Round(
            panel.rect.center.y - bounds.center.y
        );

        if (Mathf.Abs(logicalShift) < Epsilon)
        {
            return;
        }

        Vector3 worldShift = panel.TransformVector(
            new Vector3(0f, logicalShift, 0f)
        );

        Shift(character, worldShift);
        Shift(affinityGem, worldShift);
        Shift(healthBar, worldShift);
    }

    /// <summary>
    /// Returns the combined visible bounds of the player sprite, affinity gem,
    /// and health bar in PlayerPanel-local logical pixels.
    /// </summary>
    public bool TryGetStackLocalBounds(out Rect bounds)
    {
        bounds = default;

        if (panel == null)
        {
            return false;
        }

        bool hasBounds = false;
        float xMin = 0f;
        float xMax = 0f;
        float yMin = 0f;
        float yMax = 0f;

        IncludeVisibleRect(
            character,
            ref hasBounds,
            ref xMin,
            ref xMax,
            ref yMin,
            ref yMax
        );

        IncludeVisibleRect(
            affinityGem,
            ref hasBounds,
            ref xMin,
            ref xMax,
            ref yMin,
            ref yMax
        );

        IncludeVisibleRect(
            healthBar,
            ref hasBounds,
            ref xMin,
            ref xMax,
            ref yMin,
            ref yMax
        );

        if (!hasBounds)
        {
            return false;
        }

        bounds = Rect.MinMaxRect(
            xMin,
            yMin,
            xMax,
            yMax
        );

        return true;
    }

    private void ResolveReferences()
    {
        if (panel == null)
        {
            return;
        }

        if (character == null)
        {
            character = FindDescendant(
                panel,
                CharacterName
            );
        }

        if (affinityGem == null)
        {
            affinityGem = FindDescendant(
                panel,
                AffinityGemName
            );
        }

        if (healthBar == null)
        {
            healthBar = FindDescendant(
                panel,
                HealthBarName
            );
        }
    }

    private void IncludeVisibleRect(
        RectTransform rect,
        ref bool hasBounds,
        ref float xMin,
        ref float xMax,
        ref float yMin,
        ref float yMax)
    {
        if (!IsVisible(rect))
        {
            return;
        }

        rect.GetWorldCorners(worldCorners);

        for (int index = 0; index < worldCorners.Length; index++)
        {
            Vector3 local = panel.InverseTransformPoint(
                worldCorners[index]
            );

            if (!hasBounds)
            {
                xMin = xMax = local.x;
                yMin = yMax = local.y;
                hasBounds = true;
                continue;
            }

            xMin = Mathf.Min(xMin, local.x);
            xMax = Mathf.Max(xMax, local.x);
            yMin = Mathf.Min(yMin, local.y);
            yMax = Mathf.Max(yMax, local.y);
        }
    }

    private static bool IsVisible(
        RectTransform rect)
    {
        if (rect == null ||
            !rect.gameObject.activeInHierarchy)
        {
            return false;
        }

        if (rect.TryGetComponent(
                out Graphic graphic
            ))
        {
            return graphic.enabled;
        }

        return true;
    }

    private static void Shift(
        RectTransform rect,
        Vector3 worldShift)
    {
        if (!IsVisible(rect))
        {
            return;
        }

        rect.position += worldShift;
    }

    private static RectTransform FindDescendant(
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
            RectTransform match = FindDescendant(
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
}
