using System;
using System.Collections.Generic;
using System.Linq;

internal static class SelectionExpansionRosterImport
{
    internal static EnemyDefinition[] Rebuild(ZoneDefinition zone, EnemyDefinition[] authored)
    {
        // These kits have separate authoring. Preserve an existing registration only.
        var retainedIds = new HashSet<string>(StringComparer.Ordinal);
        if (zone.zoneId == "drowned-court")
            retainedIds.UnionWith(new[] { "nacre_duelist", "nacre_halberdier" });
        else if (zone.zoneId == "frostwake-peaks")
            retainedIds.Add("iceborn_axeman");

        retainedIds.ExceptWith(authored.Select(e => e.EnemyId));
        var result = new List<EnemyDefinition>();
        foreach (var enemy in zone.enemies ?? Array.Empty<EnemyDefinition>())
            if (enemy != null && retainedIds.Remove(enemy.EnemyId))
                result.Add(enemy);

        var importedIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var enemy in authored)
            if (importedIds.Add(enemy.EnemyId))
                result.Add(enemy);
        return result.ToArray();
    }
}
