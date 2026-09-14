using System;
using UnityEngine;

public static class GemMasterySettings
{
    private const string KeyPrefix =
        "DungeonMatcher.GemMastery.v1.";
    private static GemMasteryLoadout? temporaryLoadout;

    // Scoped non-persistent configuration for diagnostic Play Mode runs.
    public static IDisposable UseTemporaryLoadout(GemMasteryLoadout loadout)
    {
        var prior = temporaryLoadout; temporaryLoadout = loadout;
        return new TemporarySelection(() => temporaryLoadout = prior);
    }
    private sealed class TemporarySelection : IDisposable
    {
        private Action restore;
        public TemporarySelection(Action action) { restore = action; }
        public void Dispose() { restore?.Invoke(); restore = null; }
    }

    public static event Action<
        GemMasteryShape,
        GemMasteryReward
    > Changed;

    public static GemMasteryLoadout Current =>
        new GemMasteryLoadout(
            GetReward(
                GemMasteryShape.StraightFive
            ),
            GetReward(
                GemMasteryShape.LShape
            ),
            GetReward(
                GemMasteryShape.TShape
            ),
            GetReward(
                GemMasteryShape.CrossShape
            )
        );

    public static bool IsAvailableInRun(GemSpecialType special)
    {
        if (!AccountProgression.Current.IsUnlocked(special)) return false;
        if (special == GemSpecialType.RowBomb || special == GemSpecialType.ColumnBomb) return true;
        foreach (GemMasteryShape shape in Enum.GetValues(typeof(GemMasteryShape)))
            if (GemMasteryRuntimeResolver.TryGetSpecialType(GetReward(shape), out var selected) && selected == special) return true;
        return false;
    }

    public static GemMasteryReward GetReward(
        GemMasteryShape shape)
    {
        if (temporaryLoadout.HasValue)
        {
            var selected = temporaryLoadout.Value.GetReward(shape);
            return AccountProgression.Current.IsUnlocked(selected) ? selected : GemMasteryReward.ColorCrystal;
        }
        string key =
            GetKey(shape);

        GemMasteryReward defaultReward =
            GemMasteryLoadout.Default.GetReward(
                shape
            );

        if (!PlayerPrefs.HasKey(key))
        {
            return defaultReward;
        }

        int storedValue =
            PlayerPrefs.GetInt(
                key,
                (int)defaultReward
            );

        if (!Enum.IsDefined(
                typeof(GemMasteryReward),
                storedValue))
        {
            return defaultReward;
        }

        var reward = (GemMasteryReward)storedValue;
        return AccountProgression.Current.IsUnlocked(reward) ? reward : GemMasteryReward.ColorCrystal;
    }

    public static bool SetReward(
        GemMasteryShape shape,
        GemMasteryReward reward)
    {
        if (!Enum.IsDefined(
                typeof(GemMasteryReward),
                reward))
        {
            throw new ArgumentOutOfRangeException(
                nameof(reward),
                reward,
                "Unsupported Gem Mastery reward."
            );
        }

        string key =
            GetKey(shape);

        GemMasteryReward currentReward =
            GetReward(shape);

        if (!AccountProgression.Current.IsUnlocked(reward)) return false;

        if (temporaryLoadout.HasValue)
        {
            temporaryLoadout = temporaryLoadout.Value.WithReward(shape, reward);
            Changed?.Invoke(shape, reward);
            return currentReward != reward;
        }

        if (currentReward == reward)
        {
            return false;
        }

        GemMasteryReward defaultReward =
            GemMasteryLoadout.Default.GetReward(
                shape
            );

        if (reward == defaultReward)
        {
            PlayerPrefs.DeleteKey(key);
        }
        else
        {
            PlayerPrefs.SetInt(
                key,
                (int)reward
            );
        }

        PlayerPrefs.Save();

        Changed?.Invoke(
            shape,
            reward
        );

        return true;
    }

    public static void ResetToDefaults()
    {
        GemMasteryLoadout previousLoadout =
            Current;

        bool deletedAnySavedValue = false;

        foreach (
            GemMasteryShape shape
            in Enum.GetValues(
                typeof(GemMasteryShape)))
        {
            string key =
                GetKey(shape);

            if (!PlayerPrefs.HasKey(key))
            {
                continue;
            }

            PlayerPrefs.DeleteKey(key);
            deletedAnySavedValue = true;
        }

        if (deletedAnySavedValue)
        {
            PlayerPrefs.Save();
        }

        GemMasteryLoadout defaultLoadout =
            GemMasteryLoadout.Default;

        foreach (
            GemMasteryShape shape
            in Enum.GetValues(
                typeof(GemMasteryShape)))
        {
            GemMasteryReward previousReward =
                previousLoadout.GetReward(shape);

            GemMasteryReward defaultReward =
                defaultLoadout.GetReward(shape);

            if (previousReward == defaultReward)
            {
                continue;
            }

            Changed?.Invoke(
                shape,
                defaultReward
            );
        }
    }

    private static string GetKey(
        GemMasteryShape shape)
    {
        switch (shape)
        {
            case GemMasteryShape.StraightFive:
                return KeyPrefix +
                       "StraightFive";

            case GemMasteryShape.LShape:
                return KeyPrefix +
                       "LShape";

            case GemMasteryShape.TShape:
                return KeyPrefix +
                       "TShape";

            case GemMasteryShape.CrossShape:
                return KeyPrefix +
                       "CrossShape";

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(shape),
                    shape,
                    "Unsupported Gem Mastery shape."
                );
        }
    }
}
