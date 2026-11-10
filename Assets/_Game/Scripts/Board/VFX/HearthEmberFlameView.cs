using System;
using UnityEngine;

/// <summary>Native flame cels only. Board ownership, healing and lifetime stay external.</summary>
[DisallowMultipleComponent]
public sealed class HearthEmberFlameView : MonoBehaviour
{
    private const float FrameSeconds = .12f;
    private SpriteRenderer target;
    private Sprite[] frames;
    private MaterialPropertyBlock properties;
    private float elapsed;

    public void Bind(SpriteRenderer renderer)
    {
        target = renderer;
        if (frames == null)
        {
            frames = Resources.LoadAll<Sprite>("Combat/ApprovedRevisions/HearthFlame");
            Array.Sort(frames, (a,b) => string.CompareOrdinal(a.name,b.name));
            properties = new MaterialPropertyBlock();
        }
        // The existing approved still remains the fallback when optional cels
        // are absent. Never make an art import responsible for board behavior.
        enabled = target != null && frames.Length > 0;
        if (enabled) Present();
    }

    private void LateUpdate()
    {
        if (target == null || frames == null || frames.Length == 0) return;
        if (!PresentationPreferences.ReducedMotion)
            elapsed = (elapsed + Time.deltaTime) % (frames.Length * FrameSeconds);
        Present();
    }

    private void Present()
    {
        int index = PresentationPreferences.ReducedMotion ? 0 : (int)(elapsed / FrameSeconds);
        target.sprite = frames[index];
        // Keep the barricade's existing hit-flash values; only bind the cel
        // texture explicitly, as with the AIR/net overlay presentation.
        target.GetPropertyBlock(properties);
        properties.SetTexture("_MainTex",target.sprite.texture);
        target.SetPropertyBlock(properties);
    }
}
