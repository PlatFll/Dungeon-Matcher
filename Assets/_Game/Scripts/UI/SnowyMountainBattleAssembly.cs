using System.Linq;
using UnityEngine;

/// <summary>Approved Frostwake art selected by the existing zone presentation owner.</summary>
public sealed class SnowyMountainBattleAssembly : ScriptableObject
{
    public DungeonPixelLabBattleArt frames;
    public Sprite[] modules;
    public Sprite boardCell;
    public Sprite Get(string name) => modules.FirstOrDefault(s => s && s.name == name);
    public static SnowyMountainBattleAssembly Current => RunSession.Current?.Zone?.Definition?.zoneId == "frostwake-peaks"
        ? Resources.Load<SnowyMountainBattleAssembly>("UI/BattlePresentation/SnowyMountainAssembly") : null;
}
