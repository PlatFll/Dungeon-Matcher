using UnityEngine;
using UnityEngine.UI;

/// <summary>Optional battle artwork. Existing layout, input and combat owners remain authoritative.</summary>
public static class BattlePresentationSkin
{
    public static string Folder => RunSession.Current?.Zone?.Definition?.zoneId switch
    {
        "dungeon" => "Dungeon", "magical-forest" => "Forest", "drowned-court" => "Court", _ => null
    };
    public static Sprite Shared(string name) => Folder==null ? null : Resources.Load<Sprite>("UI/BattlePresentation/Shared/"+name);
    public static Sprite ZoneSprite(string name) => Folder==null ? null : Resources.Load<Sprite>("UI/BattlePresentation/"+Folder+"/"+name);
    public static PlayerAreaFrameProfile PlayerFrame => Folder==null ? null : Resources.Load<PlayerAreaFrameProfile>("UI/BattlePresentation/"+Folder+"/PlayerFrame");
    public static GameObject Environment => Folder==null ? null : Resources.Load<GameObject>("BattleEnvironments/Presentation_"+Folder);
    public static bool Button(Button button,string family)
    {
        var sprite=Shared(family);if(sprite==null||button==null||button.image==null)return false;
        button.image.sprite=sprite;button.image.type=Image.Type.Simple;button.image.color=Color.white;
        button.transition=Selectable.Transition.SpriteSwap;
        button.spriteState=new SpriteState{highlightedSprite=Shared(family+"Hover")??sprite,
            selectedSprite=Shared(family+"Hover")??sprite,pressedSprite=Shared(family+"Pressed")??sprite,
            disabledSprite=Shared(family+"Disabled")??sprite};
        button.image.CrossFadeColor(Color.white,0,true,true);return true;
    }
}
