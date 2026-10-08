using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>Read-only presentation records. Runtime owners retain all effect rules.</summary>
public readonly struct CombatStatusDisplay
{
    public readonly string Icon, Name, Counter, Description;
    public CombatStatusDisplay(string icon, string name, string counter, string description)
    { Icon=icon; Name=name; Counter=counter; Description=description; }
    public static string Duration(float value, bool moves) => Mathf.CeilToInt(value) + (moves ? "" : "s");

    public static void Player(PlayerActor player, List<CombatStatusDisplay> output)
    {
        output.Clear();
        if (player == null) return;
        foreach (var kind in player.Statuses.Effects.Select(e=>e.kind).Distinct().OrderBy(k=>k))
            output.Add(new CombatStatusDisplay(kind.ToString(),kind.ToString(),player.Statuses.Remaining(kind).ToString(),"Accepted moves remaining."));
        var decree=player.GetComponent<RoyalDecreeRuntime>();
        if (decree?.IsActive == true) output.Add(new CombatStatusDisplay("RoyalDecree","Royal Decree",
            Duration(decree.RemainingDuration,CombatMoveClock.MoveEffects),"Bonus damage through the complete remaining moves, including cascades."));
    }

    public static void Enemy(EnemyActor actor, List<CombatStatusDisplay> output)
    {
        output.Clear();
        if (actor == null || actor.IsDefeated) return;
        if (actor.IsWarded) output.Add(new CombatStatusDisplay("Warded","Warded","·","25% less damage while a living Warden Root remains."));
        if (actor.FortifiedStacks>0) output.Add(new CombatStatusDisplay("Fortified","Fortified",actor.FortifiedStacks.ToString(),"Each pearl halves one eligible direct hit. Periodic damage does not consume it."));
        if (actor.GetComponent<EnemyOrePower>()?.IsPowered==true) output.Add(new CombatStatusDisplay("OrePowered","Ore-Powered","·","Strengthens the next entire basic sequence once."));
        var poison=actor.GetComponent<EnemyPoisonStatus>();
        if (poison?.IsPoisoned==true) output.Add(new CombatStatusDisplay("Poison","Poison",Duration(poison.RemainingDuration,CombatMoveClock.MoveEffects),"Periodic poison damage remaining."));
        var stagger=actor.GetComponent<EnemyStagger>();
        if (stagger?.IsStaggered==true) output.Add(new CombatStatusDisplay("Stagger","Stagger",Duration(stagger.RemainingStaggerTime,CombatMoveClock.MoveEffects),"Cannot act until these future moves finish."));
        else if (stagger?.RemainingImmunityTime>0) output.Add(new CombatStatusDisplay("StaggerImmunity","Stagger immunity",Duration(stagger.RemainingImmunityTime,CombatMoveClock.MoveEffects),"New Stagger buildup and forced Stagger are blocked."));
        if (actor.GetComponent<ForestCombatAbility>()?.IsEnraged==true || actor.GetComponent<KingEnemyAbility>()?.IsEnraged==true)
            output.Add(new CombatStatusDisplay("Rage","Enrage","·","Stronger, faster basics for the rest of this enemy's life."));
        var ownMarshal=actor.GetComponent<TownMarshalEnemyAbility>();
        if (ownMarshal?.RetreatRemaining>0) output.Add(new CombatStatusDisplay("MarshalRetreat","Retreat",ownMarshal.RetreatRemaining.ToString(),"The designated protector intercepts direct hits. Ends if that protector falls."));
        var run=RunSession.Current;
        if(run==null) return;
        int rhythm=0,rally=0,conch=0;bool blessed=false;
        foreach(var source in run.Waves.ActiveEnemies)
        {
            if(source==null || source.IsDefeated) continue;
            var drummer=source.GetComponent<ForestCombatAbility>();
            if(drummer?.Rallies(actor)==true) rhythm=Mathf.Max(rhythm,drummer.RhythmRemaining);
            var marshal=source.GetComponent<TownMarshalEnemyAbility>();
            if(marshal?.Rallies(actor)==true) rally=Mathf.Max(rally,marshal.RallyRemaining);
            var aquatic=source.GetComponent<AquaticEnemyAbility>();
            if(aquatic?.Rallies(actor)==true) conch=Mathf.Max(conch,aquatic.RallyRemaining);
            blessed|=source.GetComponent<RoyalArchbishopEnemyAbility>()?.Blesses(actor)==true;
        }
        if(rhythm>0) output.Add(new CombatStatusDisplay("WarRhythm","War Rhythm",Duration(rhythm,CombatMoveClock.MoveEffects),"Faster basics; duplicate rhythms do not multiply."));
        if(rally>0) output.Add(new CombatStatusDisplay("MarshalRally","Marshal rally",Duration(rally,CombatMoveClock.MoveEffects),"Faster local allies; source-owned duration."));
        if(conch>0) output.Add(new CombatStatusDisplay("RallyingConch","Rallying Conch",Duration(conch,CombatMoveClock.Unified),"Stronger damage; attack speed is unchanged."));
        if(blessed) output.Add(new CombatStatusDisplay("Benediction","Benediction","·","Strengthens the next whole basic sequence."));
        if(run.Board.GetComponent<RoyalBannerAuraRuntime>()?.Affects(actor)==true)
            output.Add(new CombatStatusDisplay("RoyalStandard","Royal standard","·","Faster Crown basics while any supporting banner survives."));
    }
}
