using UnityEngine;
using UnityEngine.UI;

/// <summary>Explicit gameplay-only art bindings; never themes opened settings or menus.</summary>
public static class GameplayThemeSkin
{
    public static GameplayThemeDefinition Current => RunSession.Current?.Zone?.Definition?.theme;
    public static void Button(Button button,bool settings=false)
    {
        var theme=Current;
        if(theme==null || button==null || button.targetGraphic is not Image image) return;
        Sprite normal=settings?theme.settingsNormal:theme.buttonNormal;
        if(normal==null) return;
        image.sprite=normal;image.color=Color.white;
        image.type=settings?Image.Type.Simple:Image.Type.Tiled;
        if(!image.TryGetComponent<NativePixelUiSurface>(out _)) image.gameObject.AddComponent<NativePixelUiSurface>();
        button.transition=Selectable.Transition.SpriteSwap;
        button.spriteState=new SpriteState {
            highlightedSprite=(settings?theme.settingsHighlighted:theme.buttonHighlighted)??normal,
            selectedSprite=(settings?theme.settingsHighlighted:theme.buttonHighlighted)??normal,
            pressedSprite=(settings?theme.settingsPressed:theme.buttonPressed)??normal,
            disabledSprite=(settings?theme.settingsDisabled:theme.buttonDisabled)??normal };
        image.CrossFadeColor(Color.white,0,true,true);
    }
    public static void Panel(Image image)
    {
        if(Current?.panelShell==null || image==null) return;
        image.sprite=Current.panelShell;image.type=Image.Type.Tiled;image.color=Color.white;
        if(!image.TryGetComponent<NativePixelUiSurface>(out _)) image.gameObject.AddComponent<NativePixelUiSurface>();
        if(image.TryGetComponent<Outline>(out var outline)) outline.enabled=false;
    }
    public static void Supply(Button button)
    {
        var theme=Current;
        if(theme?.supplyNormal==null || button==null) return;
        button.image.sprite=theme.supplyNormal;button.image.type=Image.Type.Simple;button.image.color=Color.white;
        button.transition=Selectable.Transition.SpriteSwap;
        button.spriteState=new SpriteState{highlightedSprite=theme.supplyHighlighted,selectedSprite=theme.supplyHighlighted,
            pressedSprite=theme.supplyPressed,disabledSprite=theme.supplyDisabled};
        button.image.CrossFadeColor(Color.white,0,true,true);
    }
    public static void Screen(GameObject screen)
    {
        if(screen==null || Current==null) return;
        Panel(screen.GetComponent<Image>());
        foreach(var button in screen.GetComponentsInChildren<Button>(true)) Button(button);
    }
}
