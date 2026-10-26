using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Approved Court modules and sampled cosmetic poses; no actors, controls or gameplay state.</summary>
public sealed class DrownedCourtBattleAssembly : ScriptableObject
{
    [Serializable] public sealed class Pose { public Sprite sprite; public Vector2 offset; public float alpha; }
    [Serializable] public sealed class Piece
    {
        public string name, region;
        public Sprite sprite;
        public Rect rect;
        public Image.Type type;
        public Color color=Color.white;
        public Pose[] poses;
    }
    public string approvedScene;
    public Rect portrait, boardOpening;
    public DungeonPixelLabBattleArt frames;
    public Sprite masonry, recess, innerShadow;
    public Piece[] pieces;
    public static DrownedCourtBattleAssembly Current => BattlePresentationSkin.Folder=="Court"
        ? Resources.Load<DrownedCourtBattleAssembly>("UI/BattlePresentation/CourtAssembly") : null;
}
