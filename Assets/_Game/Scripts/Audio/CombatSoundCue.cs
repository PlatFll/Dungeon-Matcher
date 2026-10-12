using UnityEngine;

public enum CombatSoundCue
{
    GemMatch, GemLand, Explosion, PoisonBurst, Healing, ShieldGain, ShieldHit,
    PlayerHit, EnemyHit, PoisonTick, BardleyAbility, RattlebonesAbility, EnemyAbility,
    AirPickup, CofferBreak, AirWarning,
    MineRail, MineSmallDrill, MineLargeDrill, MineStoneHarden, MineStoneBreak,
    MineFuse, MineBlast, MineOre, MineRivet, MineHammer, MinePiston, MineMechBreak,
    AttenderJump, AttenderStep, AttenderCrash, KoboldSpin, KoboldShot, KoboldRicochet,
    Swap, InvalidSwap, SpecialCreated, BlockerHit, BlockerBreak, CrystalCharge, CrystalImpact
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
            case CombatSoundCue.Swap: return .09f;
            case CombatSoundCue.InvalidSwap: return .12f;
            case CombatSoundCue.SpecialCreated: return .29f;
            case CombatSoundCue.BlockerHit: return .14f;
            case CombatSoundCue.BlockerBreak: return .25f;
            case CombatSoundCue.CrystalCharge: return .18f;
            case CombatSoundCue.CrystalImpact: return .48f;
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
            case CombatSoundCue.Swap:
            case CombatSoundCue.BlockerHit: return 1;
            case CombatSoundCue.InvalidSwap: return 2;
            case CombatSoundCue.CrystalImpact: return 5;
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

    // One grouped voice gains at most 18%, never a voice per cleared gem.
    public static float CountWeight(CombatSoundCue cue, int count) =>
        cue == CombatSoundCue.GemMatch || cue == CombatSoundCue.Explosion
            ? 1f + Mathf.Clamp(count - 3, 0, 6) * .03f : 1f;
}

/// <summary>Private presentation RNG; a draw cannot advance board/encounter RNG.</summary>
public sealed class SoundVariantSelector
{
    private readonly System.Random random = new System.Random();
    private int previous = -1;
    public int Next(int count)
    {
        if (count <= 1) { previous = 0; return 0; }
        int index = random.Next(previous >= 0 && previous < count ? count - 1 : count);
        if (previous >= 0 && previous < count && index >= previous) index++;
        return previous = index;
    }
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
