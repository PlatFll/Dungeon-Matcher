using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public sealed class ZoneTravelSnapshot
{
    // Zero distinguishes older JSON that materializes a formerly absent object.
    public int version,visit,visitStartWave=1,stage;
    public bool enabled;
    public string zoneId="dungeon",destination;
    public uint random;
    public int completedCourtVisits;
    public List<string> recentCourtRecipes = new List<string>();
}

/// <summary>Owns apex travel and its durable selection; never edits the gem grid.</summary>
public sealed class ZoneTravelController : MonoBehaviour,IWaveProgressionGate
{
    public ZoneTravelSnapshot State { get; private set; }
    public bool IsBlockingWaveProgression => State!=null && State.enabled && State.stage!=0;
    public int LocalWave => Mathf.Max(1,run.Waves.CurrentWave-State.visitStartWave+1);
    private RunSession run;
    private bool processing;
    public void Initialize(RunSession owner,ZoneTravelSnapshot saved,bool enabled,string initialZone)
    {
        run=owner;
        State=saved?.version==1 ? JsonUtility.FromJson<ZoneTravelSnapshot>(JsonUtility.ToJson(saved)) :
            new ZoneTravelSnapshot{version=1,enabled=enabled,zoneId=initialZone};
        run.Waves.RegisterProgressionGate(this);run.Waves.WaveCompleted+=WaveEnded;
    }
    public ZoneTravelSnapshot Capture() => JsonUtility.FromJson<ZoneTravelSnapshot>(JsonUtility.ToJson(State));
    public static string ChooseDestination(string current,IEnumerable<ZoneDefinition> zones,SavedRandom random)
    {
        var eligible=zones.Where(z=>z!=null && z.eligibleForLiveTravel && z.zoneId!=current)
            .OrderBy(z=>z.zoneId,StringComparer.Ordinal).ToArray();
        return eligible.Length==0 ? null : eligible[random.Next(eligible.Length)].zoneId;
    }
    private void WaveEnded(int wave)
    {
        if(!State.enabled || State.stage!=0 || run.IsFinished || run.Zone?.Definition?.apexEnemy==null ||
            !run.Waves.OriginalEncounterDefinitions.Contains(run.Zone.Definition.apexEnemy)) return;
        var random=State.random==0 ? new SavedRandom(run.Waves.EncounterSeed^0x36741) : new SavedRandom(State.random);
        string next=ChooseDestination(State.zoneId,Resources.LoadAll<ZoneDefinition>("Zones"),random);
        if(next==null) return;
        State.random=random.State;State.destination=next;State.stage=1;
    }
    private void Update()
    {
        if(!IsBlockingWaveProgression || processing || run.Continuation==null || run.Continuation.IsRestoring ||
            !run.Continuation.CanCapture || run.NeedsSaveRetry || run.GetComponent<RunControlsUI>()?.IsModalOpen==true) return;
        if(run.Waves.GetComponent<RunUpgradeCoordinator>()?.IsBlockingWaveProgression==true) return;
        if(State.stage==1 && run.Board.IsExternalInputBlocked) return;
        processing=true;StartCoroutine(Travel());
    }
    private IEnumerator Travel()
    {
        try
        {
            if(State.stage==2)
            {
                var active=ZoneTransitionView.Active;
                if(active==null)
                {
                    active=ZoneTransitionView.Create(run);
                    active.TryPlay(run.Zone.Definition.displayName,null,true);
                }
                while(active!=null && active.IsPlaying) yield return null;
                State.stage=0;State.destination=null;
                if(!run.Continuation.SaveNow()) {State.stage=2;yield break;}
                yield break;
            }
            if(!run.Continuation.SaveNow()) yield break; // Persist the choice before any smoke.
            var destination=Resources.Load<ZoneDefinition>("Zones/"+State.destination);
            if(!DestinationReady(destination)) {CancelTravel();yield break;}
            var view=ZoneTransitionView.Create(run);
            if(!view.TryPlay(destination.displayName,()=>Commit(destination))) {Destroy(view.gameObject);yield break;}
            while(view!=null && view.IsPlaying) yield return null;
            if(this!=null && State.stage==1 && !run.NeedsSaveRetry) CancelTravel();
        }
        finally { if(this!=null) processing=false; }
    }
    public static bool DestinationReady(ZoneDefinition zone) => zone!=null && zone.eligibleForLiveTravel &&
        zone.apexEnemy!=null && zone.enemies!=null && zone.enemies.Length>0 &&
        (zone.zoneId=="dungeon" || (zone.theme?.battleEnvironment!=null && zone.liveEncounters?.Length>0));
    private bool Commit(ZoneDefinition destination)
    {
        if(!DestinationReady(destination) || !run.Continuation.TryCommitZoneTravel(destination.zoneId)) return false;
        SceneManager.LoadScene("Game");return true;
    }
    private void CancelTravel()
    {
        State.stage=0;State.destination=null;run.Continuation.SaveNow();
    }
    private void OnDestroy()
    { if(run?.Waves!=null) {run.Waves.WaveCompleted-=WaveEnded;run.Waves.UnregisterProgressionGate(this);} }
}
