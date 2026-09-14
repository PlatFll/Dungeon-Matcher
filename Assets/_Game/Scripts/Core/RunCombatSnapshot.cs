using System;
using System.Collections.Generic;
using UnityEngine;

// Versioned value data only. Unity instance IDs, scene objects, delegates and
// coroutine stacks never cross a process boundary. Each owner restores itself.
[Serializable]
public sealed class RunCombatSnapshot
{
    public int version = 1;
    public long sequence;
    public int wave;
    public bool waveActive;
    public string plan;
    public int encounterSeed, cardSeed;
    public uint encounterRandom, cardRandom, gameplayRandom;
    public List<string> seenEnemies = new List<string>();
    public List<string> originalEnemies = new List<string>();
    public List<string> previousLeaders = new List<string>();
    public string previousRecipe;
    public PlayerCombatSnapshot player = new PlayerCombatSnapshot();
    public BoardCombatSnapshot board = new BoardCombatSnapshot();
    public List<EnemyCombatSnapshot> enemies = new List<EnemyCombatSnapshot>();
    public List<OwnedCardSnapshot> cards = new List<OwnedCardSnapshot>();
    public int baseHealth, resonantCount;
    public bool emergencyUsed;
    public float potionCooldown, bombCooldown;
    public float decreeRemaining;
    public int decreeTarget = -1;
    public int completedWaves, milestones, potionCharges, bombCharges;
    public bool kingCleared, refinementUsed;
    public int draftWave;
    public List<string> draft = new List<string>();
}
[Serializable] public sealed class OwnedCardSnapshot { public string id; public int stacks; }
[Serializable] public sealed class PlayerCombatSnapshot
{
    public int health, maximumHealth, shield, maximumShield, revivalCount, energy;
    public string lastDamage;
}
[Serializable] public sealed class BoardCombatSnapshot
{
    public int width, height, moves, nextBanner;
    public List<BoardCellSnapshot> cells = new List<BoardCellSnapshot>();
    public List<BoardWarningSnapshot> warnings = new List<BoardWarningSnapshot>();
}
[Serializable] public sealed class BoardCellSnapshot
{
    public int x,y;
    public bool hasGem;
    public GemType type;
    public GemSpecialType special;
    public int pinOwner = -1, mineOwner = -1, barricadeOwner = -1, bannerOwner = -1;
    public bool pinned, frozen, movable, mined, barricade, banner, reachedBottom;
    public int durability, maximumDurability, bannerId;
    public EnemyBarricadeStyle barricadeStyle;
}
[Serializable] public sealed class BoardWarningSnapshot
{
    // 0 pair / 1 set / 2 lanes. Target indices refer to saved cells.
    public int kind, owner, dueMove, row, column;
    public bool restoration;
    public List<int> targets = new List<int>();
}
[Serializable] public sealed class EnemyCombatSnapshot
{
    public string definition;
    public int slot, health, shield, specialTurns, specialRequirement;
    public GemType weakness;
    public float attackRemaining, attackSpeed;
    public bool attackRunning;
    public float staggerMeter, staggerRemaining, staggerDuration, staggerImmunity, staggerGrace;
    public float poisonRemaining, poisonNextTick, poisonInterval;
    public int poisonDamage;
    public bool poisoned;
    public bool preferPrimary, crossedHalf, crossedQuarter;
    public int cycle, retryAfterMove = -1, protector = -1, retreatMoves;
    public List<int> thresholds = new List<int>();
    public float rallyRemaining;
    public List<int> rallyTargets = new List<int>();
    public List<int> blessingTargets = new List<int>();
}
