using UnityEngine;

/// <summary>Additive gameplay materials. Opened menus/settings keep their own skin.</summary>
[CreateAssetMenu(menuName="Dungeon Matcher/Zones/Gameplay theme")]
public sealed class GameplayThemeDefinition : ScriptableObject
{
    public Sprite generalBackground;
    public Sprite panelBackground;
    public Sprite[] boardCells;
    public GameObject battleEnvironment;
    public Sprite frameCorner, frameEdge, wavePlaque;
    public PlayerAreaFrameProfile playerFrame;
    public ModularHealthBarStyle[] healthStyles;
    public ModularHealthBarStyle HealthStyle(EnemyCategory? rank)
    {
        int index=rank.HasValue?(int)rank.Value:4;
        return healthStyles!=null && index>=0 && index<healthStyles.Length?healthStyles[index]:null;
    }
}
