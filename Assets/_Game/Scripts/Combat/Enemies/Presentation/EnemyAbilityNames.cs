/// <summary>Shared display vocabulary for inspection and committed-cast announcements.</summary>
public static class EnemyAbilityNames
{
    public static string Primary(EnemyDefinition definition)
    {
        if(definition==null) return "";
        if(definition.SpecialAbilityKind==EnemySpecialAbilityKind.ApplyPlayerStatus)
            return definition.appliedPlayerStatus?.displayName ?? "Status";
        if(definition.SpecialAbilityKind==EnemySpecialAbilityKind.Barricade)
            return definition.BarricadeStyle==EnemyBarricadeStyle.Thorn ? "Bramble Barricade" : "Barricade";
        if(definition.SpecialAbilityKind==EnemySpecialAbilityKind.ChannelHeal)
            return definition.canFightFlooded ? "Pearl Hymn" : "Mend";
        if(definition.SpecialAbilityKind==EnemySpecialAbilityKind.CrossbowGuardBolt)
            return definition.ChainsPerUse>1 ? "Royal Chain Shot" : "Chain Shot";
        return All(definition.SpecialAbilityKind).Split('|')[0];
    }
    public static string All(EnemySpecialAbilityKind kind)
    {
        switch(kind)
        {
            case EnemySpecialAbilityKind.Miner:return "Mine";
            case EnemySpecialAbilityKind.CrossbowGuardBolt:return "Chain Shot";
            case EnemySpecialAbilityKind.Barricade:return "Barricade";
            case EnemySpecialAbilityKind.ShieldingAllies:return "Shielding Allies";
            case EnemySpecialAbilityKind.TownMarshal:return "Ring the Bell|Citizens, Seize Him!";
            case EnemySpecialAbilityKind.SiegeSergeant:return "Fortification|Hammer Strike";
            case EnemySpecialAbilityKind.KnightCaptain:return "Chain Volley|Royal Command";
            case EnemySpecialAbilityKind.RoyalStandardBearer:return "Royal Standard";
            case EnemySpecialAbilityKind.CourtMage:return "Frozen Gems";
            case EnemySpecialAbilityKind.RoyalArchbishop:return "Restoration|Benediction";
            case EnemySpecialAbilityKind.King:return "Judgment|Royal Assault|Bombardment|Reinforcements";
            case EnemySpecialAbilityKind.ChannelHeal:return "Mend";
            case EnemySpecialAbilityKind.SpreadingVines:return "Binding Roots";
            case EnemySpecialAbilityKind.GuardingRoots:return "Guarding Roots";
            case EnemySpecialAbilityKind.GroveRenewal:return "Renew the Grove|Verdant Surge|Thorn Harvest";
            case EnemySpecialAbilityKind.Bloodrage:return "Bloodrage";
            case EnemySpecialAbilityKind.CallSnapvine:return "Call Snapvine";
            case EnemySpecialAbilityKind.WarRhythm:return "War Rhythm";
            case EnemySpecialAbilityKind.ThornVolley:return "Thorn Volley";
            case EnemySpecialAbilityKind.AncientBough:return "Bark Armor|Falling Bough";
            case EnemySpecialAbilityKind.PearlTheft:return "Pearl Theft";
            case EnemySpecialAbilityKind.RallyingConch:return "Rallying Conch";
            case EnemySpecialAbilityKind.MoraySiphon:return "Siphon";
            case EnemySpecialAbilityKind.ThornySnare:return "Thorny Snare";
            case EnemySpecialAbilityKind.SpineGuard:return "Spine Guard";
            case EnemySpecialAbilityKind.BreakwaterCommand:return "Shellguard|Boarding Order";
            case EnemySpecialAbilityKind.LanternPressure:return "Air Levy|Deepguard|Pressure Lance";
            case EnemySpecialAbilityKind.AbyssalRegent:return "Royal Seizure|Royal Ward|Crushing Depths|Court Muster|Royal Guard";
            case EnemySpecialAbilityKind.ApplyPlayerStatus:return "Status";
            default:return "";
        }
    }
    public static string Aquatic(string action)
    {
        switch(action)
        {
            case "THEFT":return "Pearl Theft";case "RALLY":return "Rallying Conch";
            case "SNARE":return "Thorny Snare";case "SPINES":return "Spine Guard";
            case "SIPHON":return "Siphon";case "SHELLGUARD":return "Shellguard";
            case "BOARDING":return "Boarding Order";case "AIR LEVY":return "Air Levy";
            case "DEEPGUARD":return "Deepguard";case "PRESSURE":return "Pressure Lance";
            case "SEIZURE":return "Royal Seizure";case "DEPTHS":return "Crushing Depths";
            case "MUSTER":return "Court Muster";case "ROYAL WARD":return "Royal Ward";
            case "ROYAL GUARD":return "Royal Guard";default:return action;
        }
    }
}
