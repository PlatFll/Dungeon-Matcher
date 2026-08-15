using UnityEngine;

[CreateAssetMenu(
    fileName = "TopBattlePresentationProfile",
    menuName = "Dungeon Matcher/UI/Top Battle Presentation Profile"
)]
public sealed class TopBattlePresentationProfile : ScriptableObject
{
    [Header("Battle Area")]
    [SerializeField, Min(200f)]
    [Tooltip(
        "Height of the complete top battle arena in reference-resolution pixels."
    )]
    private float battleAreaHeight = 290f;

    [SerializeField, Min(0f)]
    [Tooltip(
        "Reference-pixel gap kept between the battle arena and the board area."
    )]
    private float gapBelowBattleArea = 15f;

    [SerializeField, Min(0f)]
    [Tooltip(
        "Reference-pixel gap kept between the board area and the BottomHUD."
    )]
    private float gapAboveBottomHud = 7f;

    [SerializeField, Min(0f)]
    [Tooltip(
        "Horizontal inset used by the board layout area from the safe-area edges."
    )]
    private float horizontalGameAreaInset = 16f;

    [Header("Enemy Presentation")]
    [SerializeField, Range(0.5f, 1.5f)]
    [Tooltip(
        "Uniform scale applied to enemy visuals. 1 is authored size; values below " +
        "1 make enemies smaller without changing their slots, bases, or health bars."
    )]
    private float enemyVisualScale = 0.9f;

    public float BattleAreaHeight => battleAreaHeight;
    public float GapBelowBattleArea => gapBelowBattleArea;
    public float GapAboveBottomHud => gapAboveBottomHud;
    public float HorizontalGameAreaInset => horizontalGameAreaInset;
    public float EnemyVisualScale => enemyVisualScale;

    private void OnValidate()
    {
        battleAreaHeight = Mathf.Max(200f, battleAreaHeight);
        gapBelowBattleArea = Mathf.Max(0f, gapBelowBattleArea);
        gapAboveBottomHud = Mathf.Max(0f, gapAboveBottomHud);
        horizontalGameAreaInset = Mathf.Max(0f, horizontalGameAreaInset);
        enemyVisualScale = Mathf.Clamp(enemyVisualScale, 0.5f, 1.5f);
    }
}
