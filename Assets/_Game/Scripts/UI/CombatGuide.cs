using System.Text;
using UnityEngine;

/// <summary>Inspection copy only; the actor, board and combat systems remain authoritative.</summary>
public static class CombatGuide
{
    public static string Basics => CombatMoveClock.MoveBasics ? LegacyBasics.Replace("Enemy attacks count seconds. Special counters and warning deadlines count completed valid moves.", "LEGACY FOREST TEST: attacks, channels, poison and supplies count accepted moves. Thinking is safe. Emerald damage gains 15%; affinity and weakness remain separate.") : CombatMoveClock.MoveEffects ? LegacyBasics.Replace("Enemy attacks count seconds. Special counters and warning deadlines count completed valid moves.", "FOREST: basic attacks count seconds. Special abilities and channels count accepted moves. Basics pause during board resolution and owned ability holds. Buffs, stagger, poison and supply cooldowns retain their move durations. Emerald damage gains 15%; affinity and weakness remain separate.") : LegacyBasics;
    private const string LegacyBasics = "COMBAT BASICS\n\nMatch an enemy's weakness to damage and stagger it. A shield absorbs a whole hit, including the hit that breaks it. Later hits reach HP. Combat amounts use steps of five. Defeating the King continues the run; survive as long as you can. Affinity matches heal you. Make four or more to prepare specials.\n\nEnemy attacks count seconds. Special counters and warning deadlines count completed valid moves. Cascades, invalid swaps and inspection do not add moves.\n\nTap an enemy portrait to pause and inspect its current threat, marked cells and counter.\n\nAfter wave 2, choose a Board, Ability or Survival direction. One free Refine per run offers new cards in a chosen theme.\n\nBomb: preview the highlighted area, then confirm. Chains and ice break when their gem clears. Barricades take one hit per blast, including adjacent hits; banners fall when gems beneath them clear. Specials can extend the blast. Empty mined cells are not restored by a Bomb. Cancelling spends nothing.\n\nSuspend to Menu keeps this attempt. End Run pays completed waves once and starts over.\n\nFONT CREDIT\nThaleah by Rick Hoppmann (Tiny Worlds). CC BY 4.0.\ntinyworlds.itch.io/free-pixel-font-thaleah\ncreativecommons.org/licenses/by/4.0/";
    public static string Enemy(EnemyActor actor)
    {
        if (actor == null || actor.Definition == null) return "This slot is empty.";
        var attack = actor.GetComponent<EnemyAutoAttack>();
        string basic = $"{actor.Definition.DisplayName}\nHP {actor.CurrentHealth}/{actor.MaxHealth}   Shield {actor.CurrentShield}\n" +
            $"Weakness: {actor.AssignedGemType}\nNormal hit: {actor.Damage}" + (actor.FollowUpDamage > 0 ? $" + {actor.FollowUpDamage}" : "") +
            (CombatMoveClock.MoveBasics ? $"   Every {actor.Definition.AttackMoves} moves\n" : $"   Every {actor.AttackInterval:0.#} seconds\n") +
            (attack != null ? $"Next attack: {attack.RemainingAttackTime:0.#} {(CombatMoveClock.MoveBasics ? "moves" : "seconds")}\n" : "");
        if (actor.HasSpecialAbility) basic += $"Special: {Mathf.Max(0,actor.SpecialTurnRequirement-actor.CurrentSpecialTurnCount)} valid moves to ready\n";
        return basic + "\n" + (actor.Definition.canFightFlooded ? actor.Definition.Description : Counter(actor.Definition.SpecialAbilityKind)) +
            (RunSession.Current?.Board != null ? "\n\n"+RunSession.Current.Board.DescribeOwnedBoardThreats(actor) : "") +
            (CombatMoveClock.Active ? "\n\nOnly accepted manual actions advance special deadlines. Interrupt channels by staggering or defeating their caster. Free skills can solve threats without spending a move." : "\n\nSeconds run during combat. Only completed valid swaps/taps advance move counters; cascades and invalid swaps do not.");
    }

    public static string Counter(EnemySpecialAbilityKind kind)
    {
        switch (kind)
        {
            case EnemySpecialAbilityKind.GuardingRoots: return "Warns a cell, then replaces its gem with a 2-hit Root and four spreading vines. While the Root lives, all enemies take 25% less damage. First clear a vine beside the Root; later adjacent clears through an open side damage it. Stagger or clear the warning to interrupt preparation.";
            case EnemySpecialAbilityKind.GroveRenewal: return "Plants two linked 2-hit Heartroots with spreading vines. Clear side vines, then hit through open sides. Destroy both Heartroots to stagger her. Renew the Grove: 2 moves, heals herself and all surviving allies for 20 plus 20 per living Heartroot. Verdant Surge advances existing vines without changing normal growth timing. Thorn Harvest: 3 moves, hits for 20 plus 5 per vine-covered gem, then consumes all vines. Heartroots survive. No basics during channels or recovery.";
            case EnemySpecialAbilityKind.ChannelHeal: return "Heals a fixed wounded ally by 20 after two future moves. Stagger or defeat the Mender, or defeat its target, to cancel. Two recovery moves follow. No basics during channel or recovery.";
            case EnemySpecialAbilityKind.SpreadingVines: return "Warns one gem, then replaces it with a 1-hit Root and four nonspreading vines. First clear a vine beside the Root; a later adjacent clear through that open side destroys it. Vines only obscure gems: swapping, falling, matching and bombs still work normally. Roots and their vines leave with their owner.";
            case EnemySpecialAbilityKind.Miner: return "Mines remove board cells. Defeat their Miner to restore the holes he owns. Specials are protected from mining.";
            case EnemySpecialAbilityKind.CrossbowGuardBolt: return "Chains stop manual swaps, but fall with gravity. Clear the chained gem with a match, special or ability. Adjacent clears do not break chains; defeating the owner releases them.";
            case EnemySpecialAbilityKind.Barricade: return "Barricades occupy cells and block gravity. Damage them with adjacent matches or special blasts. They remain when their owner falls.";
            case EnemySpecialAbilityKind.ShieldingAllies: return "Grants separate shields to allies and itself. Target the support to stop repeated shielding. The hit that breaks a shield stops there; later hits reach HP.";
            case EnemySpecialAbilityKind.TownMarshal: return "Summons a protector and retreats behind it. The protector intercepts direct hits; defeating it opens the Marshal. Rally speeds living locals for " + (CombatMoveClock.MoveEffects ? "3 future moves." : "5 seconds.") + " Summoned locals survive him.";
            case EnemySpecialAbilityKind.SiegeSergeant: return "Alternates blockades and a hammer warning with at least two moves to respond. Clear either marked gem to cancel the hammer. Marks follow gems. Break his last owned blockade to remove his defence.";
            case EnemySpecialAbilityKind.KnightCaptain: return "Chains up to three gems, then commands Crown allies. Clear chained gems or defeat the Captain. Defeating him cancels unfinished command strikes; escorts remain valid targets.";
            case EnemySpecialAbilityKind.RoyalStandardBearer: return "His banner speeds Crown forces. Clear gems beneath it so gravity carries it to the bottom. Defeating the bearer stops new placements; the existing banner and its aura remain until it reaches the bottom.";
            case EnemySpecialAbilityKind.CourtMage: return "Frozen gems cannot be moved manually and stay fixed during gravity. Clear the frozen gems or defeat their Mage. Freezing and falling chains are different obstacles.";
            case EnemySpecialAbilityKind.RoyalArchbishop: return "Heals wounded allies, marks Restoration runes and blesses a normal attack sequence. Clear individual runes before their move countdown ends to prevent their healing.";
            case EnemySpecialAbilityKind.King: return "Cycles Judgment, coordinated Assault and Bombardment. Clear Judgment marks; clearing ordinary gems cannot cancel a lane bombardment. Defeat or stagger the threat, plan healing/shielding and target remaining escorts. Reinforcements trigger once at half and quarter HP.";
            default: return "Match this enemy's weakness to damage and stagger it. Prioritize the most urgent attack or build a special for the next threat.";
        }
    }

    public static string Ability(PlayerActor player)
    {
        var controller = player != null ? player.GetComponent<PlayerAbilityController>() : null;
        if (controller == null || player.ActiveAbility == null) return "No ability available.";
        var definition = player.ActiveAbility;
        if(RunSession.Current!=null && RunSession.Current.Challenge==RunChallenge.BoardOnly)
            return "Board Only challenge\n\nAbilities and supplies are disabled for this attempt. Use weakness matches, affinity healing and specials to progress.";
        return $"{definition.DisplayName}\nEnergy {controller.CurrentEnergy}/{controller.RequiredEnergy}\n\n{(CombatMoveClock.MoveEffects && definition is RoyalDecreeAbilityDefinition ? "Decree lasts 3 full manual actions, or 4 with Longer Reign. Adds 5 base damage per cleared gem; normal match energy is suppressed for those actions." : definition.Description)}\n\n" +
            (definition.MaximumSelfRefundFraction >= 0 ? $"This cast can return at most {Mathf.FloorToInt(controller.RequiredEnergy*definition.MaximumSelfRefundFraction)} energy, shared by explosions, chains and cards.\n\n" : "") +
            "Matching an enemy weakness gives more energy. Affinity matches heal. Energy is spent only when activation is accepted.";
    }

    public static string Build(RunUpgradeRuntime runtime)
    {
        var text = new StringBuilder("CURRENT BUILD\n\n");
        if (runtime != null && runtime.Catalog != null)
            foreach (var card in runtime.Catalog.Upgrades)
            {
                int stacks = runtime.GetStackCount(card.UpgradeId);
                if (stacks > 0) text.AppendLine($"{card.DisplayTitle} x{stacks}: {card.Description}");
            }
        if (text.Length < 20) text.Append("Your first build choice arrives after wave 2. Board, Ability and Survival offers give three different directions.");
        return text.ToString();
    }
    public static string Continue(RunSession run)
    {
        var text=new StringBuilder($"Continue wave {run.Waves.CurrentWave}\n{run.Player.Definition.DisplayName}: {run.Player.CurrentHealth} HP / {run.Player.CurrentShield} shield\n");
        text.AppendLine($"Supplies: {run.Charges(ConsumableKind.HealthPotion)} Potions / {run.Charges(ConsumableKind.Bomb)} Bombs\n");
        foreach(var enemy in run.Waves.ActiveEnemies)
            text.AppendLine($"{enemy.Definition.DisplayName}: next attack in {enemy.GetComponent<EnemyAutoAttack>()?.RemainingAttackTime:0.#} {(CombatMoveClock.MoveBasics ? "moves" : "seconds")}");
        text.AppendLine("\nNo combat time passed while you were away. Resume when ready.\n");
        text.Append(Build(RunUpgradeRuntime.Current));return text.ToString();
    }
}
