public readonly struct SpecialGemCreationRequest
{
    public Gem GemToPreserve { get; }

    public GemSpecialType SpecialType { get; }

    public bool IsValid =>
        GemToPreserve != null &&
        SpecialType != GemSpecialType.None;

    public SpecialGemCreationRequest(
        Gem gemToPreserve,
        GemSpecialType specialType)
    {
        GemToPreserve =
            gemToPreserve;

        SpecialType =
            specialType;
    }
}