using System;
using UnityEngine;

public static class GemMasterySettings
{
    private const string KeyPrefix =
        "DungeonMatcher.GemMastery.v1.";

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

    public static GemMasteryReward GetReward(
        GemMasteryShape shape)
    {
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

        return (GemMasteryReward)storedValue;
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
