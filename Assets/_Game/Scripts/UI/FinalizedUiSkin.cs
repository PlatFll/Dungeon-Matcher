using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>The shared optional sprite family for controls and panels.</summary>
public static class FinalizedUiSkin
{
    private static readonly Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();
    public static Sprite Load(string name)
    {
        if (Sprites.TryGetValue(name, out var sprite) && sprite != null) return sprite;
        sprite = Resources.Load<Sprite>("UI/Finalized/" + name);
        if (sprite != null) Sprites[name] = sprite;
        return sprite;
    }

    public static void Button(Button button, bool small = false, bool selected = false, bool gear = false)
    {
        if (button == null || button.targetGraphic is not Image image) return;
        string family = gear ? "Settings" : small ? "ButtonSmall" : "ButtonLarge";
        var normal = Load(family + (selected ? "Highlighted" : "Normal"));
        if (normal == null) normal = Load("ButtonLargeNormal");
        if (normal == null) return;
        image.sprite = normal; image.color = Color.white; image.type = gear ? Image.Type.Simple : Image.Type.Tiled;
        if (!image.TryGetComponent<NativePixelUiSurface>(out _)) image.gameObject.AddComponent<NativePixelUiSurface>();
        button.transition = Selectable.Transition.SpriteSwap;
        image.CrossFadeColor(Color.white, 0f, true, true);
        button.spriteState = new SpriteState
        {
            highlightedSprite = Load(family + "Highlighted") ?? normal,
            selectedSprite = Load(family + "Highlighted") ?? normal,
            pressedSprite = Load(family + "Pressed") ?? normal,
            disabledSprite = Load(family + "Disabled") ?? normal
        };
    }

    public static void Panel(Image image, bool tall = false)
    {
        var sprite = Load(tall ? "PanelTall" : "PanelInset");
        if (image == null || sprite == null) return;
        image.sprite = sprite; image.type = Image.Type.Tiled; image.color = Color.white;
        if (!image.TryGetComponent<NativePixelUiSurface>(out _)) image.gameObject.AddComponent<NativePixelUiSurface>();
        var outline = image.GetComponent<Outline>(); if (outline != null) outline.enabled = false;
    }

    public static void ApplyScreen(GameObject screen)
    {
        if (screen == null) return;
        foreach (var button in screen.GetComponentsInChildren<Button>(true))
            Button(button, ((RectTransform)button.transform).rect.width < 180);
        Panel(screen.GetComponent<Image>(), true);
    }
}
