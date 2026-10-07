using System;
using System.Collections.Generic;

public partial class EnemyActor
{
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
