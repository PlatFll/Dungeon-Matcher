using System;
using UnityEngine;

public static class PlayerDefinitionRegistry
{
    private const string ResourcesPath =
        "Players";

    private static PlayerDefinition[] cachedDefinitions;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration
    )]
    private static void ResetCache()
    {
        cachedDefinitions = null;
    }

    public static bool TryGetDefinition(
        string playerId,
        out PlayerDefinition definition)
    {
        definition = null;

        if (string.IsNullOrWhiteSpace(playerId))
        {
            return false;
        }

        PlayerDefinition[] definitions =
            GetDefinitions();

        foreach (PlayerDefinition candidate
                 in definitions)
        {
            if (candidate == null)
            {
                continue;
            }

            if (!string.Equals(
                    candidate.PlayerId,
                    playerId,
                    StringComparison.Ordinal
                ))
            {
                continue;
            }

            definition = candidate;
            return true;
        }

        return false;
    }

    public static bool IsAvailable(
        string playerId)
    {
        return TryGetDefinition(
            playerId,
            out _
        );
    }

    public static PlayerDefinition ResolveSelectedOrFallback(
        PlayerDefinition fallbackDefinition)
    {
        if (TryGetDefinition(
                CharacterSelectionSettings.SelectedPlayerId,
                out PlayerDefinition selectedDefinition
            ))
        {
            return selectedDefinition;
        }

        if (fallbackDefinition != null)
        {
            return fallbackDefinition;
        }

        TryGetDefinition(
            CharacterSelectionSettings.RattlebonesPlayerId,
            out PlayerDefinition rattlebonesDefinition
        );

        return rattlebonesDefinition;
    }

    private static PlayerDefinition[] GetDefinitions()
    {
        if (cachedDefinitions == null)
        {
            cachedDefinitions =
                Resources.LoadAll<PlayerDefinition>(
                    ResourcesPath
                );
        }

        return cachedDefinitions;
    }
}
