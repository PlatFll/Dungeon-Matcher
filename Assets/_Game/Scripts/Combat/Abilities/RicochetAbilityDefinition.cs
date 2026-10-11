using System;
using UnityEngine;

[CreateAssetMenu(fileName="Ability_Ricochet", menuName="Dungeon Matcher/Abilities/Ricochet")]
public sealed class RicochetAbilityDefinition : CharacterAbilityDefinition
{
    [SerializeField,Min(1)] private int energyCost=90;
    [SerializeField,Range(1,32)] private int shots=6;
    [SerializeField,Min(0)] private int baseEnemyDamage=5;
    [SerializeField,Min(0)] private float spinSeconds=.3f;
    [SerializeField,Min(0)] private float shotSeconds=.13f;
    [SerializeField,Min(0)] private float recoverySeconds=.18f;
    [SerializeField] private Sprite[] impactFrames;
    public override int EnergyCost => Mathf.Max(1,energyCost);
    public override float MaximumSelfRefundFraction => 0;
    public override Type RuntimeType => typeof(RicochetRuntime);
    public int Shots => Mathf.Clamp(shots,1,32);
    public float SpinSeconds => Mathf.Max(0,spinSeconds);
    public float ShotSeconds => Mathf.Max(0,shotSeconds);
    public int BaseEnemyDamage => Mathf.Max(0,baseEnemyDamage);
    public float RecoverySeconds => Mathf.Max(0,recoverySeconds);
    public Sprite[] ImpactFrames => impactFrames;
}
