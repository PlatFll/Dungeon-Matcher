using System.Collections.Generic;

public static class CounterplayGuard
{
    public static bool HasUsefulResponse(IReadOnlyList<BoardController.ResponseOption> options, PlayerActor player, WaveController waves)
    {
        if (player == null || !player.IsInitialized || player.IsDefeated || waves == null || !waves.IsWaveActive) return true;
        foreach (var option in options)
        {
            if (option.UsesSpecial || option.BreaksObstacle || option.CreatesSpecial) return true;
            foreach (var gem in option.Clears)
            {
                if (gem == null) continue;
                if (player.CurrentHealth < player.MaximumHealth && gem.Type == player.Definition.AffinityGemType) return true;
                foreach (var enemy in waves.ActiveEnemies)
                    if (enemy != null && !enemy.IsDefeated && enemy.AssignedGemType == gem.Type) return true;
            }
        }
        return false;
    }
}
