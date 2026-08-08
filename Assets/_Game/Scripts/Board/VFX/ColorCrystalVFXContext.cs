using UnityEngine;

public readonly struct ColorCrystalVFXContext
{
    public Gem CrystalGem { get; }

    public Gem[] TargetGems { get; }

    public bool IsValid =>
        CrystalGem != null &&
        TargetGems != null &&
        TargetGems.Length > 0;

    public ColorCrystalVFXContext(
        Gem crystalGem,
        Gem[] targetGems)
    {
        CrystalGem = crystalGem;
        TargetGems = targetGems;
    }
}
