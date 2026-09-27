using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability_ChronoShutter", menuName = "Dungeon Matcher/Abilities/ChronoShutter")]
public sealed class ChronoShutterAbilityDefinition : CharacterAbilityDefinition
{
    [SerializeField, Min(1)] private int energyCost = 100;
    [SerializeField, Min(1)] private int manualMoves = 5;
    public override int EnergyCost => energyCost;
    public int ManualMoves => manualMoves;
    public override bool AllowsMatchEnergyWhileActive => true;
    public override Type RuntimeType => typeof(ChronoShutterRuntime);
}
