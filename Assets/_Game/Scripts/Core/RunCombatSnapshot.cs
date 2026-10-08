using System;
using System.Collections.Generic;
using UnityEngine;

// Versioned value data only. Unity instance IDs, scene objects, delegates and
// coroutine stacks never cross a process boundary. Each owner restores itself.
[Serializable]
public sealed class RunCombatSnapshot
{
    public int version = 1;
    public CombatClockSnapshot clock;
    public ZoneTravelSnapshot travel;
    public int decreeAppliedMove;
    public long sequence;
    public int wave;
    public long nextLegacyEnemyId = 1;
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
    public BoardMemoryAbilitySnapshot boardMemory;
    public int completedWaves, milestones, potionCharges, bombCharges;
    public bool kingCleared, refinementUsed;
    public int draftWave;
    public List<string> draft = new List<string>();
}
[Serializable] public sealed class OwnedCardSnapshot { public string id; public int stacks; }
[Serializable] public sealed class PlayerCombatSnapshot
{
    public PlayerStatusSnapshot statuses;
    public int health, maximumHealth, shield, maximumShield, revivalCount, energy;
    public string lastDamage;
}
[Serializable] public sealed class BoardCombatSnapshot
{
    public AquaticEnvironmentState aquatic;
    public MineEnvironmentState mine;
    public int width, height, moves, nextBanner, nextGem;
    public uint refillRandom;
    public int forestRulesVersion, nextRootId, nextVineGrowthMove;
    public int dungeonRulesVersion, nextCrumbleMove;
    public List<BoardCellSnapshot> cells = new List<BoardCellSnapshot>();
    public List<BoardWarningSnapshot> warnings = new List<BoardWarningSnapshot>();
    public List<VineNodeSnapshot> vines = new List<VineNodeSnapshot>();
}
[Serializable] public sealed class BoardCellSnapshot
{
    public int x,y,identity;
    public bool hasGem;
    public GemType type;
    public GemSpecialType special;
    public int pinOwner = -1, mineOwner = -1, barricadeOwner = -1, bannerOwner = -1;
    public bool pinned, frozen, movable, mined, barricade, banner, reachedBottom;
    public int durability, maximumDurability, bannerId;
    public EnemyBarricadeStyle barricadeStyle;
    public int rootId, openRootSides;
    public int thornSafeSide, thornDamage;
    public long rootOwnerId;
    public bool rootSpreading;
    public int crumbleRestoreMove;
    public MineStoneState mineStone;
}
[Serializable] public sealed class BoardWarningSnapshot
{
    // 0 pair / 1 set / 2 lanes / 3 fixed response cells. Target indices refer to saved cells.
    public int kind, owner, dueMove, row, column;
    public bool restoration, vine, environmental, nonSpreading;
    public int vineLimit, parentGemId;
    public int rootDurability;
    public bool requiresVine, answered;
    public EnemyBarricadeStyle rootStyle;
    public bool cancelOnAnyTargetLost;
    public string threatLabel;
    public bool rootSpreading, playerInterrupted;
    public List<int> targets = new List<int>();
}
[Serializable] public sealed class EnemyCombatSnapshot
{
    public float oreNextMultiplier;
    public bool oreBurstConsumed;
    public string definition;
    public long persistentId;
    public int staggerHitMove, staggerAppliedMove, immunityAppliedMove;
    public int poisonMoveTicks, poisonNextMove, rallyExpiryMove;
    public EnemyChannelSnapshot channel;
    public ForestMilestoneSnapshot forestMilestone;
    public ForestRosterSnapshot forestRoster;
    public AquaticEnemySnapshot aquaticEnemy;
    public string rootbinderOutcome;
    public int slot, health, shield, specialTurns, specialRequirement, fortifiedStacks;
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
