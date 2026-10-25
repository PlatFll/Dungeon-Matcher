using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Approved Forest ambient motion, shared by native runtime sprites. No gameplay ownership.</summary>
public sealed class ForestAmbientMotion : MonoBehaviour
{
    public enum MotionKind { Light, Foliage, Water }
    [Serializable]
    public sealed class Track
    {
        public Image image;
        public Sprite[] frames;
        public MotionKind kind;
        public Vector2 origin, amplitude;
        public float period = 6, phase, onFraction = .7f, peak = .85f;
        public float driftPeriod = 7, frameRate = 5, gustDuration = 2.8f;
        public float unit = 1;
        public Track Bind(Image target, Vector2 position, float pixelUnit) => new Track {
            image=target, origin=position, frames=frames, kind=kind, amplitude=amplitude, unit=pixelUnit,
            period=period, phase=phase, onFraction=onFraction, peak=peak, driftPeriod=driftPeriod,
            frameRate=frameRate, gustDuration=gustDuration };
    }
    public Track[] tracks;
    public bool automatic = true;
    float clock;

    void Update()
    {
        if (!Application.isPlaying || !automatic) return;
        bool reduced = PresentationPreferences.ReducedMotion;
        if (!reduced) clock += Time.deltaTime;
        Evaluate(clock, reduced);
    }

    public void Evaluate(float time, bool reduced)
    {
        if (tracks == null) return;
        foreach (var track in tracks)
        {
            if (track.image == null || track.frames == null || track.frames.Length == 0) continue;
            Vector2 position = track.origin;
            float alpha = 1;
            int frame = 0;
            if (track.kind == MotionKind.Light)
            {
                if (reduced) alpha = track.peak * .65f;
                else
                {
                    float q = Mathf.Repeat((time + track.phase) / track.period, 1);
                    float active = q / track.onFraction;
                    float envelope = active >= 1 ? 0 : active < .3f
                        ? Mathf.SmoothStep(0, 1, active / .3f)
                        : active > .62f ? Mathf.SmoothStep(1, 0, (active - .62f) / .38f) : 1;
                    alpha = envelope * track.peak;
                    float angle = (time + track.phase) / track.driftPeriod * Mathf.PI * 2;
                    position += new Vector2(Mathf.Round(Mathf.Sin(angle) * track.amplitude.x),
                        Mathf.Round(Mathf.Sin(angle * 1.3f + .7f) * track.amplitude.y)) * track.unit;
                    frame = Mathf.FloorToInt((time + track.phase) * track.frameRate) % track.frames.Length;
                }
            }
            else if (track.kind == MotionKind.Foliage && !reduced)
            {
                float q = Mathf.Repeat(time + track.phase, track.period) / track.gustDuration;
                int bend = q >= 1 ? 0 : Mathf.RoundToInt(2.4f * Mathf.Sin(q * Mathf.PI * 2) * Mathf.Sin(q * Mathf.PI));
                frame = bend >= 0 ? bend : 2 - bend;
            }
            else if (track.kind == MotionKind.Water && !reduced)
                frame = Mathf.FloorToInt(time * track.frameRate) % track.frames.Length;

            track.image.rectTransform.anchoredPosition = position;
            track.image.sprite = track.frames[Mathf.Clamp(frame, 0, track.frames.Length - 1)];
            track.image.color = new Color(1, 1, 1, alpha);
        }
    }
}
