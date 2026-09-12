using UnityEngine;


[CreateAssetMenu(
    fileName = "TopBattlePresentationProfile",
    menuName = "Dungeon Matcher/UI/Top Battle Presentation Profile"
)]
public sealed class TopBattlePresentationProfile : ScriptableObject
{
    [Header("Shared Battle Floor")]
    [SerializeField, Min(0f)]
    [Tooltip(
        "Migration/fallback floor height in reference UI pixels above the battle " +
        "area bottom. Used only when TopHUD has no BattleFloorAnchor. " +
        "Edit the scene anchor to position the battle floor."
    )]
    private float battleFloorOffsetFromBottom = 58f;

    [SerializeField]
    [Tooltip(
        "Small visual correction applied equally to the feet of player and enemy " +
        "character anchors after they are aligned to the shared floor."
    )]
    private float characterFeetOffsetFromFloor = 0f;

    [SerializeField]
    [Tooltip(
        "Vertical offset of the legacy colored character/enemy base center relative " +
        "to the shared floor. Kept for migration while those bases are retired."
    )]
    private float baseCenterOffsetFromFloor = -3f;

    [Header("Battle HUD Positioning")]
    [SerializeField]
    [Tooltip(
        "Vertical position of every enemy health bar's bottom edge, measured in " +
        "reference UI pixels upward from the bottom of its enemy slot. Change " +
        "this to move all enemy health bars up or down without editing the scene."
    )]
    private float enemyHealthBarBottomOffset = 14f;

    [SerializeField, Min(0f)]
    [Tooltip(
        "Clear-space gap between the top of the player's character rect and the " +
        "bottom of the 16x16 affinity gem."
    )]
    private float playerAffinityGapAboveCharacter = 8f;

    public float BattleFloorOffsetFromBottom => battleFloorOffsetFromBottom;
    public float CharacterFeetOffsetFromFloor => characterFeetOffsetFromFloor;
    public float BaseCenterOffsetFromFloor => baseCenterOffsetFromFloor;
    public float EnemyHealthBarBottomOffset => enemyHealthBarBottomOffset;
    public float PlayerAffinityGapAboveCharacter => playerAffinityGapAboveCharacter;
    private void OnValidate()
    {
        battleFloorOffsetFromBottom = Mathf.Clamp(
            battleFloorOffsetFromBottom,
            0f,
            GameplayPixelLayoutController.MaximumBattleHeight
        );

        characterFeetOffsetFromFloor = Mathf.Clamp(
            characterFeetOffsetFromFloor,
            -64f,
            64f
        );

        baseCenterOffsetFromFloor = Mathf.Clamp(
            baseCenterOffsetFromFloor,
            -64f,
            64f
        );

        enemyHealthBarBottomOffset = Mathf.Clamp(
            enemyHealthBarBottomOffset,
            -128f,
            256f
        );

        playerAffinityGapAboveCharacter = Mathf.Clamp(
            playerAffinityGapAboveCharacter,
            0f,
            128f
        );

    }
}
