using System;
using UnityEngine;

public static class GemMasterySettings
{
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

    internal static GemMasteryLoadout ForCharacter(AccountProgression account, string id) =>
        temporaryLoadout ?? account.CharacterMastery(id);

    public static GemMasteryLoadout Current => ForCharacter(AccountProgression.Current, CharacterSelectionSettings.SelectedPlayerId);

    public static bool IsAvailableInRun(GemSpecialType special)
    {
        if (!AccountProgression.Current.IsUnlocked(special)) return false;
        if (special == GemSpecialType.RowBomb || special == GemSpecialType.ColumnBomb) return true;
        foreach (GemMasteryShape shape in Enum.GetValues(typeof(GemMasteryShape)))
            if (GemMasteryRuntimeResolver.TryGetSpecialType(GetReward(shape), out var selected) && selected == special) return true;
        return false;
    }

    public static GemMasteryReward GetReward(GemMasteryShape shape)
    {
        if (temporaryLoadout.HasValue)
        {
            var reward = temporaryLoadout.Value.GetReward(shape);
            return AccountProgression.Current.IsUnlocked(reward) ? reward : GemMasteryReward.ColorCrystal;
        }
        return Current.GetReward(shape);
    }

    public static bool SetReward(GemMasteryShape shape, GemMasteryReward reward)
    {
        if (!Enum.IsDefined(typeof(GemMasteryReward), reward)) throw new ArgumentOutOfRangeException(nameof(reward));
        if (!AccountProgression.Current.IsUnlocked(reward) || GetReward(shape) == reward) return false;
        var build = Current.WithReward(shape, reward);
        if (temporaryLoadout.HasValue) temporaryLoadout = build;
        else if (!AccountProgression.Current.SaveMastery(CharacterSelectionSettings.SelectedPlayerId, build)) return false;
        Changed?.Invoke(shape, reward);
        return true;
    }

    public static void ResetToDefaults()
    {
        var previous = Current;
        if (temporaryLoadout.HasValue) temporaryLoadout = GemMasteryLoadout.Default;
        else if (!AccountProgression.Current.SaveMastery(CharacterSelectionSettings.SelectedPlayerId, GemMasteryLoadout.Default)) return;
        foreach (GemMasteryShape shape in Enum.GetValues(typeof(GemMasteryShape)))
            if (previous.GetReward(shape) != GemMasteryReward.ColorCrystal) Changed?.Invoke(shape, GemMasteryReward.ColorCrystal);
    }
}
