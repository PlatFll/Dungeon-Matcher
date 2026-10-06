using System;

public partial class EnemyActor
{
    public event Action<EnemyActor,string> AbilityCastCommitted;
    // Called once by the gameplay owner after its cast/telegraph is accepted,
    // never by the ready counter or an animation-start event.
    public void AnnounceCommittedCast(string displayName=null)
    {
        if(!IsInitialized || IsDefeated) return;
        displayName=displayName ?? EnemyAbilityNames.Primary(Definition);
        if(string.IsNullOrWhiteSpace(displayName)) return;
        AbilityCastCommitted?.Invoke(this,displayName);
        (GetComponent<EnemyCastAnnouncement>() ?? gameObject.AddComponent<EnemyCastAnnouncement>()).Show(displayName);
    }
}
