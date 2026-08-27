using System;

public readonly struct GemMasteryLoadout
{
    public GemMasteryReward StraightFive { get; }
    public GemMasteryReward LShape { get; }
    public GemMasteryReward TShape { get; }
    public GemMasteryReward CrossShape { get; }

    public static GemMasteryLoadout Default =>
        new GemMasteryLoadout(
            GemMasteryReward.ColorCrystal,
            GemMasteryReward.PoisonBomb,
            GemMasteryReward.PoisonBomb,
            GemMasteryReward.PoisonBomb
        );

    public GemMasteryLoadout(
        GemMasteryReward straightFive,
        GemMasteryReward lShape,
        GemMasteryReward tShape,
        GemMasteryReward crossShape)
    {
        StraightFive = straightFive;
        LShape = lShape;
        TShape = tShape;
        CrossShape = crossShape;
    }

    public GemMasteryReward GetReward(
        GemMasteryShape shape)
    {
        switch (shape)
        {
            case GemMasteryShape.StraightFive:
                return StraightFive;

            case GemMasteryShape.LShape:
                return LShape;

            case GemMasteryShape.TShape:
                return TShape;

            case GemMasteryShape.CrossShape:
                return CrossShape;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(shape),
                    shape,
                    "Unsupported Gem Mastery shape."
                );
        }
    }

    public GemMasteryLoadout WithReward(
        GemMasteryShape shape,
        GemMasteryReward reward)
    {
        switch (shape)
        {
            case GemMasteryShape.StraightFive:
                return new GemMasteryLoadout(
                    reward,
                    LShape,
                    TShape,
                    CrossShape
                );

            case GemMasteryShape.LShape:
                return new GemMasteryLoadout(
                    StraightFive,
                    reward,
                    TShape,
                    CrossShape
                );

            case GemMasteryShape.TShape:
                return new GemMasteryLoadout(
                    StraightFive,
                    LShape,
                    reward,
                    CrossShape
                );

            case GemMasteryShape.CrossShape:
                return new GemMasteryLoadout(
                    StraightFive,
                    LShape,
                    TShape,
                    reward
                );

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(shape),
                    shape,
                    "Unsupported Gem Mastery shape."
                );
        }
    }
}
