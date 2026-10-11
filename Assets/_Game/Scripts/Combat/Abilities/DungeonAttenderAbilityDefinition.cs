using System;
using UnityEngine;

[CreateAssetMenu(fileName="Ability_DungeonAttender", menuName="Dungeon Matcher/Abilities/Dungeon Attender")]
public sealed class DungeonAttenderAbilityDefinition : CharacterAbilityDefinition
{
    [SerializeField,Min(1)] private int energyCost=90;
    [SerializeField,Range(1,32)] private int movementBudget=7;
    [SerializeField,Min(0)] private float jumpSeconds=.4f;
    [SerializeField,Min(0)] private float stepSeconds=.16f;
    [SerializeField] private Sprite boardMarker;
    [SerializeField,Min(0)] private float landingSeconds=.3f;
    [SerializeField,Min(0)] private float recoverySeconds=.32f;
    [SerializeField] private Sprite[] impactFrames;
    public override int EnergyCost => Mathf.Max(1,energyCost);
    public override float MaximumSelfRefundFraction => 0;
    public override Type RuntimeType => typeof(DungeonAttenderRuntime);
    public int MovementBudget => Mathf.Clamp(movementBudget,1,32);
    public float JumpSeconds => Mathf.Max(0,jumpSeconds);
    public float StepSeconds => Mathf.Max(0,stepSeconds);
    public Sprite BoardMarker => boardMarker;
    public float LandingSeconds => Mathf.Max(0,landingSeconds);
    public float RecoverySeconds => Mathf.Max(0,recoverySeconds);
    public Sprite[] ImpactFrames => impactFrames;
}
