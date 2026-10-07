using System;
using System.Collections.Generic;
using UnityEngine;

public partial class EnemyActor
{
    private int fortifiedStacks;
    public int FortifiedStacks => IsDefeated?0:fortifiedStacks;
    public event Action<EnemyActor> FortifiedConsumed;
    public int GrantFortified(int amount,int cap=2)
    {
        if(!IsInitialized || IsDefeated || amount<=0)return 0;
        int before=fortifiedStacks;
        fortifiedStacks=Mathf.Min(Mathf.Clamp(cap,1,2),fortifiedStacks+Mathf.Min(amount,2));
        if(fortifiedStacks<before)fortifiedStacks=before; // A smaller grant cap never removes an existing stack.
        if(fortifiedStacks>0 && GetComponent<EnemyFortifiedView>()==null)gameObject.AddComponent<EnemyFortifiedView>();
        return fortifiedStacks-before;
    }
    // Sources are evaluated at impact and presentation time. Duplicate Wardens
    // supply one named effect; loss of one source cannot remove another's buff.
    private readonly Dictionary<object,Func<bool>> wardedSources=new Dictionary<object,Func<bool>>();
    public bool IsWarded
    {
        get
        {
            if(IsDefeated) return false;
            foreach(var source in wardedSources.Values) if(source!=null && source()) return true;
            return false;
        }
    }
    public void SetWardedSource(object source,Func<bool> active)
    { if(source!=null) wardedSources[source]=active; }
    public void RemoveWardedSource(object source)
    { if(source!=null) wardedSources.Remove(source); }
}
