using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[Serializable]
public sealed class CombatTextStyle
{
    [SerializeField]
    private CombatTextKind kind;

    [SerializeField]
    private Color color = Color.white;

    [SerializeField, Min(1f)]
    private float fontSize = 28f;

    [SerializeField]
    private FontStyles fontStyle =
        FontStyles.Bold;

    [SerializeField, Min(0.05f)]
    private float lifetime = 0.75f;

    [SerializeField]
    [Tooltip(
        "Distance travelled in CombatTextLayer UI pixels."
    )]
    private Vector2 travelDistance =
        new Vector2(0f, 25f);

    [SerializeField, Min(0f)]
    private float horizontalJitter = 4f;

    [SerializeField, Range(0f, 1f)]
    [Tooltip(
        "At what percentage of the animation fading begins."
    )]
    private float fadeStartNormalized = 0.55f;

    [SerializeField, Min(0.01f)]
    private float startScale = 0.75f;

    [SerializeField, Min(0.01f)]
    private float peakScale = 1.15f;

    public CombatTextKind Kind =>
        kind;

    public Color Color =>
        color;

    public float FontSize =>
        fontSize;

    public FontStyles FontStyle =>
        fontStyle;

    public float Lifetime =>
        Mathf.Max(0.05f, lifetime);

    public Vector2 TravelDistance =>
        travelDistance;

    public float HorizontalJitter =>
        Mathf.Max(0f, horizontalJitter);

    public float FadeStartNormalized =>
        Mathf.Clamp01(
            fadeStartNormalized
        );

    public float StartScale =>
        Mathf.Max(0.01f, startScale);

    public float PeakScale =>
        Mathf.Max(0.01f, peakScale);
}

[CreateAssetMenu(
    fileName = "CombatTextStyles_",
    menuName =
        "Dungeon Matcher/UI/Combat Text Style Library"
)]
public sealed class CombatTextStyleLibrary :
    ScriptableObject
{
    [Header("Fallback Style")]
    [SerializeField]
    private CombatTextStyle fallbackStyle =
        new CombatTextStyle();

    [Header("Type Styles")]
    [SerializeField]
    private List<CombatTextStyle> styles =
        new List<CombatTextStyle>();

    public CombatTextStyle GetStyle(
        CombatTextKind kind)
    {
        if (styles != null)
        {
            foreach (
                CombatTextStyle style
                in styles)
            {
                if (style != null &&
                    style.Kind == kind)
                {
                    return style;
                }
            }
        }

        return fallbackStyle;
    }
}