using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(
    fileName = "TopBattlePresentationProfile",
    menuName = "Dungeon Matcher/UI/Top Battle Presentation Profile"
)]
public sealed class TopBattlePresentationProfile : ScriptableObject
{
    [Header("Responsive Stack")]
    [FormerlySerializedAs("battleAreaHeight")]
    [SerializeField, Min(1f)]
    [Tooltip(
        "Battle-area height used as the authored/reference composition. It is " +
        "also the baseline used by responsive character scaling."
    )]
    private float referenceBattleAreaHeight = 290f;

    [SerializeField, Min(1f)]
    [Tooltip(
        "Smallest battle-area height the layout will try to preserve before the " +
        "board is allowed to become height-limited on unusually short screens."
    )]
    private float minimumBattleAreaHeight = 220f;

    [SerializeField, Min(0f)]
    [Tooltip(
        "Fixed reference-pixel gap between the bottom of the battle area and the board."
    )]
    private float gapBelowBattleArea = 8f;

    [SerializeField, Min(0f)]
    [Tooltip(
        "Fixed reference-pixel gap between the board and the fixed BottomHUD."
    )]
    private float gapAboveBottomHud = 8f;

    [FormerlySerializedAs("horizontalGameAreaInset")]
    [SerializeField, Min(0f)]
    [Tooltip(
        "Horizontal safe-area inset used to determine the board's width-driven size."
    )]
    private float boardHorizontalInset = 10f;

    [Header("Responsive Character Presentation")]
    [SerializeField, Range(0.5f, 1.5f)]
    [Tooltip(
        "Authored player multiplier. The responsive screen scale is applied on top of this value."
    )]
    private float playerVisualScale = 1f;

    [SerializeField, Range(0.5f, 1.5f)]
    [Tooltip(
        "Authored enemy multiplier. The responsive screen scale is applied on top of this value."
    )]
    private float enemyVisualScale = 0.802f;

    [SerializeField, Range(0f, 1f)]
    [Tooltip(
        "How strongly extra battle-area height affects character size. Zero keeps " +
        "authored size; one follows the battle-height ratio directly."
    )]
    private float characterScaleResponse = 0.45f;

    [SerializeField, Range(0.5f, 1.5f)]
    private float minimumResponsiveCharacterScale = 0.88f;

    [SerializeField, Range(0.5f, 1.75f)]
    private float maximumResponsiveCharacterScale = 1.25f;

    public float ReferenceBattleAreaHeight => referenceBattleAreaHeight;
    public float MinimumBattleAreaHeight => minimumBattleAreaHeight;
    public float GapBelowBattleArea => gapBelowBattleArea;
    public float GapAboveBottomHud => gapAboveBottomHud;
    public float BoardHorizontalInset => boardHorizontalInset;
    public float PlayerVisualScale => playerVisualScale;
    public float EnemyVisualScale => enemyVisualScale;
    public float CharacterScaleResponse => characterScaleResponse;
    public float MinimumResponsiveCharacterScale => minimumResponsiveCharacterScale;
    public float MaximumResponsiveCharacterScale => maximumResponsiveCharacterScale;

    private void OnValidate()
    {
        referenceBattleAreaHeight = Mathf.Max(1f, referenceBattleAreaHeight);
        minimumBattleAreaHeight = Mathf.Clamp(
            minimumBattleAreaHeight,
            1f,
            referenceBattleAreaHeight
        );

        gapBelowBattleArea = Mathf.Max(0f, gapBelowBattleArea);
        gapAboveBottomHud = Mathf.Max(0f, gapAboveBottomHud);
        boardHorizontalInset = Mathf.Max(0f, boardHorizontalInset);

        playerVisualScale = Mathf.Clamp(playerVisualScale, 0.5f, 1.5f);
        enemyVisualScale = Mathf.Clamp(enemyVisualScale, 0.5f, 1.5f);
        characterScaleResponse = Mathf.Clamp01(characterScaleResponse);
        minimumResponsiveCharacterScale = Mathf.Clamp(
            minimumResponsiveCharacterScale,
            0.5f,
            1.5f
        );
        maximumResponsiveCharacterScale = Mathf.Max(
            minimumResponsiveCharacterScale,
            Mathf.Clamp(maximumResponsiveCharacterScale, 0.5f, 1.75f)
        );
    }
}
