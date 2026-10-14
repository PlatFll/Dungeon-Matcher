using UnityEngine;
using UnityEngine.UI;

/// <summary>Optional battle artwork. Existing layout, input and combat owners remain authoritative.</summary>
public static class BattlePresentationSkin
{
    // Only the explicitly approved Dungeon kit is live. Forest/Court rebuilds
    // remain source-review candidates until the user approves them.
    public static bool UsesReferenceKit => Folder == "Dungeon" && Reference("FarWall") != null;
    public static Sprite Reference(string name) => Resources.Load<Sprite>("UI/BattlePresentation/ReferenceDungeon/"+name);
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
