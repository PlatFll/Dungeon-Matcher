using UnityEngine;

public enum CombatSoundCue
{
    GemMatch, GemLand, Explosion, PoisonBurst, Healing, ShieldGain, ShieldHit,
    PlayerHit, EnemyHit, PoisonTick, BardleyAbility, RattlebonesAbility, EnemyAbility
}

/// <summary>Small, fixed mix policy. All clips have the same authored peak ceiling.</summary>
public static class CombatSoundMix
{
    public const int VoiceCount = 6;
    public const float GainBudget = 0.8f;
    public static float Gain(CombatSoundCue cue)
    {
        switch (cue)
        {
            case CombatSoundCue.GemLand: return 0.10f;
            case CombatSoundCue.PoisonTick: return 0.16f;
            case CombatSoundCue.GemMatch: return 0.24f;
            case CombatSoundCue.EnemyHit: return 0.22f;
            case CombatSoundCue.PlayerHit: return 0.36f;
            case CombatSoundCue.Explosion: return 0.46f;
            case CombatSoundCue.BardleyAbility:
            case CombatSoundCue.RattlebonesAbility: return 0.42f;
            default: return 0.28f;
        }
    }
    public static int Priority(CombatSoundCue cue)
    {
        switch (cue)
        {
            case CombatSoundCue.GemLand: return 0;
            case CombatSoundCue.PoisonTick: return 1;
            case CombatSoundCue.GemMatch:
            case CombatSoundCue.EnemyHit: return 2;
            case CombatSoundCue.BardleyAbility:
            case CombatSoundCue.RattlebonesAbility: return 5;
            case CombatSoundCue.PlayerHit:
            case CombatSoundCue.Explosion: return 4;
            default: return 3;
        }
    }
    public static float MinimumInterval(CombatSoundCue cue)
    {
        switch (cue)
        {
            case CombatSoundCue.GemLand: return 0.085f;
            case CombatSoundCue.GemMatch: return 0.07f;
            case CombatSoundCue.PoisonTick: return 0.12f;
            case CombatSoundCue.EnemyHit: return 0.075f;
            case CombatSoundCue.BardleyAbility:
            case CombatSoundCue.RattlebonesAbility: return 0.40f;
            default: return 0.10f;
        }
    }
    public static float HeadroomScale(float requestedGain) =>
        requestedGain > GainBudget ? GainBudget / requestedGain : 1f;
}

/// <summary>Presentation clock only; neither mutates nor consumes gameplay randomness.</summary>
public sealed class FeedbackCooldown
{
    private float nextTime = float.NegativeInfinity;
    public bool TryConsume(float now, float interval)
    {
        if (now < nextTime) return false;
        nextTime = now + Mathf.Max(0, interval);
        return true;
    }
    public void Reset() => nextTime = float.NegativeInfinity;
}
