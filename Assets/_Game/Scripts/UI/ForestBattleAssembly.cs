using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>References the approved modular Forest artwork; contains no gameplay or copied actors.</summary>
public sealed class ForestBattleAssembly : ScriptableObject
{
    [Serializable] public sealed class Module { public string key; public Sprite sprite; }
    [Serializable] public sealed class Piece
    {
        public string name, region;
        public Sprite sprite;
        public Rect rect;
        public Image.Type type;
        public Color color = Color.white;
        public ForestAmbientMotion.Track motion;
    }
    public string approvedScene;
    public Rect portrait, boardOpening;
    public Module[] modules;
    public Piece[] pieces;
}
