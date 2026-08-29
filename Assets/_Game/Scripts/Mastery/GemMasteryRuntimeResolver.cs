public static class GemMasteryRuntimeResolver
{
    public static GemSpecialType ResolveSpecialType(
        BoardMatchType matchType)
    {
        GemMasteryShape shape;

        if (!TryGetShape(
                matchType,
                out shape))
        {
            return GemSpecialType.None;
        }

        GemMasteryReward selectedReward =
            GemMasterySettings.GetReward(shape);

        GemSpecialType selectedSpecialType;

        if (TryGetSpecialType(
                selectedReward,
                out selectedSpecialType))
        {
            return selectedSpecialType;
        }

        /*
         * Shield and damage bombs already exist in the mastery data model
         * so save data and UI can be built incrementally. Until their
         * gameplay implementations exist, never let one of those future
         * selections silently remove the special earned by a five-gem shape.
         */
        GemMasteryReward fallbackReward =
            GemMasteryLoadout.Default.GetReward(
                shape
            );

        GemSpecialType fallbackSpecialType;

        return TryGetSpecialType(
                fallbackReward,
                out fallbackSpecialType)
            ? fallbackSpecialType
            : GemSpecialType.None;
    }

    public static bool IsRewardImplemented(
        GemMasteryReward reward)
    {
        GemSpecialType ignoredSpecialType;

        return TryGetSpecialType(
            reward,
            out ignoredSpecialType
        );
    }

    public static bool TryGetShape(
        BoardMatchType matchType,
        out GemMasteryShape shape)
    {
        switch (matchType)
        {
            case BoardMatchType.StraightFive:
                shape =
                    GemMasteryShape.StraightFive;

                return true;

            case BoardMatchType.LShape:
                shape =
                    GemMasteryShape.LShape;

                return true;

            case BoardMatchType.TShape:
                shape =
                    GemMasteryShape.TShape;

                return true;

            case BoardMatchType.CrossShape:
                shape =
                    GemMasteryShape.CrossShape;

                return true;

            default:
                shape = default(GemMasteryShape);
                return false;
        }
    }

    public static bool TryGetSpecialType(
        GemMasteryReward reward,
        out GemSpecialType specialType)
    {
        switch (reward)
        {
            case GemMasteryReward.ColorCrystal:
                specialType =
                    GemSpecialType.ColorCrystal;

                return true;

            case GemMasteryReward.PoisonBomb:
                specialType =
                    GemSpecialType.PoisonBomb;

                return true;

            case GemMasteryReward.HealBomb:
                specialType =
                    GemSpecialType.HealingBomb;

                return true;

            case GemMasteryReward.ShieldBomb:
            case GemMasteryReward.DamageBomb:
            default:
                specialType =
                    GemSpecialType.None;

                return false;
        }
    }
}
