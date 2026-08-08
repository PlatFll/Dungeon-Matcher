public readonly struct ColorCrystalVFXContext
{
    public Gem CrystalGem { get; }

    public Gem[] TargetGems { get; }

    public bool IsValid =>
        CrystalGem != null &&
        TargetGems != null;

    public ColorCrystalVFXContext(
        Gem crystalGem,
        Gem[] targetGems)
    {
        CrystalGem = crystalGem;
        TargetGems = targetGems;
    }
}
