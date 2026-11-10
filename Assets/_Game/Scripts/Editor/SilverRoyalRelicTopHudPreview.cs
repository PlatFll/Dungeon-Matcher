using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Stage 2 only: reversible art substitution on the real Game controls.
/// Editor assembly only; never saves a scene or changes production skin routing.</summary>
public sealed class SilverRoyalRelicTopHudPreview : IDisposable
{
    const string Root = "Assets/_Game/Art/UI/SilverRoyalRelic/Stage02/";
    readonly Image wave, settings, pendant;
    readonly Sprite oldWave, oldSettings, oldPendant;
    readonly Vector2 pendantSize, pendantPosition, pendantMin, pendantMax, pendantPivot;
    readonly Image.Type waveType;
    readonly List<GameObject> created = new();
    readonly Sprite body, diamondSprite, pendantSprite, settingsFrame, gearSprite;
    readonly Image diamond, gear;
    bool disposed;

    public SilverRoyalRelicTopHudPreview(GameplayPixelLayoutController layout, RunControlsUI controls)
    {
        wave = layout.TopHud.Find("WaveTracker").GetComponent<Image>();
        settings = controls.SettingsButton.image;
        pendant = wave.transform.Find("SharedWavePendant").GetComponent<Image>();
        oldWave = wave.sprite; oldSettings = settings.sprite; oldPendant = pendant.sprite; waveType = wave.type;
        var pr = pendant.rectTransform;
        pendantSize = pr.sizeDelta; pendantPosition = pr.anchoredPosition;
        pendantMin = pr.anchorMin; pendantMax = pr.anchorMax; pendantPivot = pr.pivot;
        body = Load("WaveBody"); diamondSprite = Load("WaveDiamond"); pendantSprite = Load("WavePendant");
        settingsFrame = Load("SettingsFrame"); gearSprite = Load("SettingsGear");
        diamond = Child("RelicPreviewDiamond", wave.rectTransform);
        gear = Child("RelicPreviewGear", settings.rectTransform);
        Canvas.willRenderCanvases += Apply;
        Apply();
    }

    static Sprite Load(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(Root + name + ".png")
        ?? throw new InvalidOperationException("Missing Stage 2 sprite: " + name);
    Image Child(string name, RectTransform parent)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false); created.Add(go);
        var image = go.GetComponent<Image>(); image.raycastTarget = false; return image;
    }
    static void Ornament(Image image, Sprite sprite, float top)
    {
        image.sprite = sprite; image.type = Image.Type.Simple; image.color = Color.white;
        image.raycastTarget = false; image.preserveAspect = true;
        var r = image.rectTransform;
        r.localScale = Vector3.one; r.anchorMin = r.anchorMax = r.pivot = new Vector2(.5f, 1);
        r.sizeDelta = sprite.rect.size; r.anchoredPosition = new Vector2(0, -top);
        GameplayPixelGrid.Snap(r);
    }
    public void Apply()
    {
        if (disposed || !wave || !settings) return;
        // Existing layout remains the only owner of the control rectangles.
        wave.sprite = body; wave.type = Image.Type.Tiled; wave.pixelsPerUnitMultiplier = 1;
        settings.sprite = settingsFrame;
        Ornament(diamond, diamondSprite, -8);
        Ornament(pendant, pendantSprite, 32);
        Ornament(gear, gearSprite, 12);
    }
    public void Dispose()
    {
        if (disposed) return; disposed = true; Canvas.willRenderCanvases -= Apply;
        if (wave) { wave.sprite = oldWave; wave.type = waveType; }
        if (settings) settings.sprite = oldSettings;
        if (pendant)
        {
            pendant.sprite = oldPendant; var r = pendant.rectTransform;
            r.anchorMin = pendantMin; r.anchorMax = pendantMax; r.pivot = pendantPivot;
            r.sizeDelta = pendantSize; r.anchoredPosition = pendantPosition;
        }
        foreach (var go in created) if (go) UnityEngine.Object.DestroyImmediate(go);
    }
}
