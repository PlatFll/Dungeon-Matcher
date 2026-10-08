using UnityEngine;

public enum PlayerStatusKind { Weakened, Burn, Sapped, Wounded, Fear, Frostbite, Slippery, Rattled }

[CreateAssetMenu(menuName = "Dungeon Matcher/Combat/Player Status")]
public sealed class PlayerStatusDefinition : ScriptableObject
{
    public PlayerStatusKind kind;
    public string displayName;
    [TextArea] public string description;
    [Min(1)] public int durationMoves = 3;
    [Min(0)] public float multiplier = 1f;
    [Min(0)] public int damagePerMove = 5;
    public Sprite icon;
}
