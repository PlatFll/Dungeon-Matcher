public readonly struct BombTriggeredCrystalRequest
{
    public Gem CrystalGem { get; }

    public GemType TriggerGemType { get; }

    public bool IsValid =>
        CrystalGem != null &&
        CrystalGem.SpecialType ==
            GemSpecialType.ColorCrystal;

    public BombTriggeredCrystalRequest(
        Gem crystalGem,
        GemType triggerGemType)
    {
        CrystalGem =
            crystalGem;

        TriggerGemType =
            triggerGemType;
    }
}