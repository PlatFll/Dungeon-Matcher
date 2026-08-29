using System;
using UnityEngine;

public static class CharacterSelectionSettings
{
    public const string RattlebonesPlayerId = "skeleton";
    public const string BardleyPlayerId = "bardley";

    private const string SelectedPlayerKey =
        "DungeonMatcher.CharacterSelection.v1.SelectedPlayer";

    public static event Action Changed;

    public static string SelectedPlayerId
    {
        get
        {
            string storedPlayerId =
                PlayerPrefs.GetString(
                    SelectedPlayerKey,
                    RattlebonesPlayerId
                );

            return IsKnownCharacter(storedPlayerId)
                ? storedPlayerId
                : RattlebonesPlayerId;
        }
    }

    public static bool SetSelectedPlayerId(
        string playerId)
    {
        if (!IsKnownCharacter(playerId))
        {
            Debug.LogError(
                $"Cannot select unknown player id '{playerId}'."
            );

            return false;
        }

        if (string.Equals(
                SelectedPlayerId,
                playerId,
                StringComparison.Ordinal))
        {
            return false;
        }

        if (string.Equals(
                playerId,
                RattlebonesPlayerId,
                StringComparison.Ordinal))
        {
            PlayerPrefs.DeleteKey(
                SelectedPlayerKey
            );
        }
        else
        {
            PlayerPrefs.SetString(
                SelectedPlayerKey,
                playerId
            );
        }

        PlayerPrefs.Save();
        Changed?.Invoke();

        return true;
    }

    public static bool IsKnownCharacter(
        string playerId)
    {
        return string.Equals(
                   playerId,
                   RattlebonesPlayerId,
                   StringComparison.Ordinal
               ) ||
               string.Equals(
                   playerId,
                   BardleyPlayerId,
                   StringComparison.Ordinal
               );
    }
}
