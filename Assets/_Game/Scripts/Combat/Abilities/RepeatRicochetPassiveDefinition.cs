using System;
using UnityEngine;

[CreateAssetMenu(menuName="Dungeon Matcher/Passives/Repeat Ricochet")]
public sealed class RepeatRicochetPassiveDefinition : CharacterPassiveDefinition
{
    [SerializeField,Min(0)] private int damagePerPriorHit=5;
    [SerializeField,Min(0)] private int maximumBonus=15;
    public int Bonus(int priorHits) => (int)Math.Min(Math.Max(0,maximumBonus),
        (long)Math.Max(0,priorHits)*Math.Max(0,damagePerPriorHit));
}
