using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Approved modular Ironvein scenery; existing live owners retain actors, controls and board rules.</summary>
public sealed class IronveinBattleAssembly : ScriptableObject
{
    [Serializable] public sealed class Piece
    {
        public string name, region;
        public Sprite sprite;
        public Rect rect;
        public Image.Type type;
        public Color color = Color.white;
        public Vector2 reflection = Vector2.one;
    }
    public string approvedScene;
    public Rect portrait, boardOpening;
    public DungeonPixelLabBattleArt frames, sharedControls;
    public Sprite masonry, recess, amberInset;
    [Tooltip("Optional full upper-screen cave canvas, drawn at native environment scale.")]
    public Sprite cavernBackdrop;
    public Piece[] pieces;
    public static IronveinBattleAssembly Current => RunSession.Current?.Zone?.Definition?.zoneId == "ironvein-excavation"
        ? Resources.Load<IronveinBattleAssembly>("UI/BattlePresentation/IronveinAssembly") : null;
}
