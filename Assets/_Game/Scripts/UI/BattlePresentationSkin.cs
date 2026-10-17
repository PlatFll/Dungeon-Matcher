using UnityEngine;
using UnityEngine.UI;

/// <summary>Optional battle artwork. Existing layout, input and combat owners remain authoritative.</summary>
public static class BattlePresentationSkin
{
    public static bool UsesReferenceKit => Folder != null && Reference(Folder == "Dungeon" ? "FarWall" : "FarScenery") != null;
    public static Sprite Reference(string name)
    {
        // Controls and quiet board cells deliberately use one global family.
        string folder = name.StartsWith("Shared", System.StringComparison.Ordinal) || name.StartsWith("EmptyCell", System.StringComparison.Ordinal)
            ? "Dungeon" : Folder;
        return folder == null ? null : Resources.Load<Sprite>("UI/BattlePresentation/Reference"+folder+"/"+name);
    }
    // Native registrations from the approved kits' layout.json, in top-left coordinates.
    // Forest/Court's board rim begins at y=288; its overlap is excluded from the arena.
    public static float ReferenceTopHeight => Folder == "Dungeon" ? 298 : 288;
    public static Rect ReferencePlayerRect => Folder == "Dungeon" ? new Rect(11,28,129,252) : new Rect(8,24,135,258);
    public static Rect ReferenceWaveRect => Folder == "Dungeon" ? new Rect(169,21,135,47) : new Rect(157,20,157,48);
    public static Rect ReferenceBoardOpening => Folder == "Dungeon" ? new Rect(16,18,364,369) : new Rect(23,25,364,369);
    public static Vector2 ReferenceBoardSize => Folder == "Dungeon" ? new Vector2(400,406) : new Vector2(410,414);
    private static readonly System.Collections.Generic.Dictionary<string, Sprite[]> frameAtlases = new();
    public static Sprite FramePiece(string name)
    {
        if (Folder == null) return null;
        if (!frameAtlases.TryGetValue(Folder, out var pieces))
        {
            pieces=Resources.LoadAll<Sprite>("UI/BattlePresentation/"+Folder+"/FrameAtlas");
            frameAtlases[Folder]=pieces;
        }
        return System.Array.Find(pieces,p=>p.name==name);
    }
    public static string Folder => RunSession.Current?.Zone?.Definition?.zoneId switch
    {
        "dungeon" => "Dungeon", "magical-forest" => "Forest", "drowned-court" => "Court", _ => null
    };
    public static Sprite Shared(string name)
    {
        if(Folder==null)return null;
        string reference = name switch {"Guide"=>"SharedGuide", "Supply"=>"SharedPotionHolder",
            "Ability"=>"SharedAbility", "Energy"=>"SharedEnergy", _=>null};
        return (reference==null?null:Reference(reference)) ?? Resources.Load<Sprite>("UI/BattlePresentation/Shared/"+name);
    }
    public static Sprite ZoneSprite(string name)
    {
        if(Folder==null)return null;
        if(UsesReferenceKit)
        {
            string reference=name switch {"Plaque"=>"WaveHeader", "CellA"=>"EmptyCell0",
                "CellB"=>"EmptyCell1", "CellC"=>"EmptyCell2", _=>null};
            if(reference!=null)return Reference(reference);
        }
        return Resources.Load<Sprite>("UI/BattlePresentation/"+Folder+"/"+name);
    }
    public static PlayerAreaFrameProfile PlayerFrame => Folder==null ? null : Resources.Load<PlayerAreaFrameProfile>("UI/BattlePresentation/"+Folder+"/PlayerFrame");
    public static GameObject Environment => Folder==null ? null : Resources.Load<GameObject>("BattleEnvironments/Presentation_"+Folder);
    public static bool Button(Button button,string family)
    {
        var sprite=Shared(family);if(sprite==null||button==null||button.image==null)return false;
        button.image.sprite=sprite;button.image.type=Image.Type.Simple;button.image.color=Color.white;
        if(Reference("SharedAbility")!=null)
        {
            // Tint the actual approved plate; an old alternate sprite would
            // visibly change its shape whenever a control is pressed/disabled.
            button.transition=Selectable.Transition.ColorTint;button.spriteState=default;
            var colors=ColorBlock.defaultColorBlock;colors.highlightedColor=new Color(.95f,.95f,1);
            colors.pressedColor=new Color(.72f,.68f,.82f);colors.disabledColor=new Color(.45f,.45f,.5f);
            button.colors=colors;button.image.CrossFadeColor(Color.white,0,true,true);return true;
        }
        button.transition=Selectable.Transition.SpriteSwap;
        button.spriteState=new SpriteState{highlightedSprite=Shared(family+"Hover")??sprite,
            selectedSprite=Shared(family+"Hover")??sprite,pressedSprite=Shared(family+"Pressed")??sprite,
            disabledSprite=Shared(family+"Disabled")??sprite};
        button.image.CrossFadeColor(Color.white,0,true,true);return true;
    }
}
