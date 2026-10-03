using UnityEngine;

/// <summary>Additive gameplay materials. Opened menus/settings keep their own skin.</summary>
[CreateAssetMenu(menuName="Dungeon Matcher/Zones/Gameplay theme")]
public sealed class GameplayThemeDefinition : ScriptableObject
{
    [Min(0)] public int minimumBattleHeight;
    public Sprite generalBackground;
    public Sprite panelBackground;
    public Sprite[] boardCells;
    public GameObject battleEnvironment;
    public Sprite frameCorner, frameEdge, wavePlaque;
    public Sprite buttonNormal, buttonHighlighted, buttonPressed, buttonDisabled;
    public Sprite settingsNormal, settingsHighlighted, settingsPressed, settingsDisabled;
    public Sprite panelShell, energyFrame, vineOverlay, anchorOverlay, vineWarning;
    public Sprite rootLevelOne, rootLevelTwo;
    public Sprite[] vineSpreadFrames, vineHitFrames;
    public Sprite healEffect, interruptEffect, resonanceIcon;
    public Sprite supplyNormal,supplyHighlighted,supplyPressed,supplyDisabled;
    [System.Serializable] public struct IconReplacement { public Sprite source, themed; }
    public IconReplacement[] abilityIcons;
    public Sprite AbilityIcon(Sprite original)
    {
        if(abilityIcons!=null) foreach(var icon in abilityIcons)
            if(icon.source==original && icon.themed!=null) return icon.themed;
        return original;
    }
    public PlayerAreaFrameProfile playerFrame;
    public ModularHealthBarStyle[] healthStyles;
    public ModularHealthBarStyle HealthStyle(EnemyCategory? rank)
    {
        int index=rank.HasValue?(int)rank.Value:4;
        return healthStyles!=null && index>=0 && index<healthStyles.Length?healthStyles[index]:null;
    }
}
