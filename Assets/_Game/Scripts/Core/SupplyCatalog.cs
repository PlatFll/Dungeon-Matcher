using System;

public static class SupplyCatalog
{
    public static readonly ConsumableKind[] DisplayOrder = {
        ConsumableKind.HealthPotion, ConsumableKind.ShieldPotion, ConsumableKind.Bomb,
        ConsumableKind.MegaPotion, ConsumableKind.ColorBomb
    };

    public static bool IsPotion(ConsumableKind kind) => kind == ConsumableKind.HealthPotion ||
        kind == ConsumableKind.MegaPotion || kind == ConsumableKind.ShieldPotion;
    public static ConsumableKind PotionOrDefault(ConsumableKind kind) =>
        IsPotion(kind) ? kind : ConsumableKind.HealthPotion;
    public static ConsumableKind BombOrDefault(ConsumableKind kind) =>
        kind == ConsumableKind.ColorBomb ? kind : ConsumableKind.Bomb;

    public static string Name(ConsumableKind kind) => kind switch {
        ConsumableKind.HealthPotion => "Health Potion", ConsumableKind.ShieldPotion => "Shield Potion",
        ConsumableKind.MegaPotion => "Mega Potion", ConsumableKind.ColorBomb => "Color Bomb",
        ConsumableKind.Bomb => "Bomb", _ => throw new ArgumentOutOfRangeException(nameof(kind))
    };

    public static int Price(ConsumableKind kind) => kind switch {
        ConsumableKind.HealthPotion => BalanceV1.Current.potionPrice,
        ConsumableKind.ShieldPotion => BalanceV1.Current.shieldPotionPrice,
        ConsumableKind.MegaPotion => BalanceV1.Current.megaPotionPrice,
        ConsumableKind.ColorBomb => BalanceV1.Current.colorBombPrice,
        ConsumableKind.Bomb => BalanceV1.Current.bombPrice, _ => throw new ArgumentOutOfRangeException(nameof(kind))
    };

    public static string Description(ConsumableKind kind) => kind switch {
        ConsumableKind.HealthPotion => $"Restores {BalanceV1.Current.potionHealthFraction * 100:0}% of max HP.",
        ConsumableKind.ShieldPotion => $"Gain {BalanceV1.Current.shieldPotionAmount} shield for {BalanceV1.Current.shieldPotionMoves} moves.",
        ConsumableKind.MegaPotion => "Fully restores your HP.",
        ConsumableKind.ColorBomb => "Clears gems of one chosen color.",
        ConsumableKind.Bomb => $"Clears a {2 * BalanceV1.Current.consumableBombRadius + 1} × {2 * BalanceV1.Current.consumableBombRadius + 1} area.",
        _ => throw new ArgumentOutOfRangeException(nameof(kind))
    };
}
