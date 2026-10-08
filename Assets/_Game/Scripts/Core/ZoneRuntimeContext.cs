using System;
using UnityEngine;

public sealed class ZoneRuntimeContext : MonoBehaviour
{
    public ZoneDefinition Definition { get; private set; }
    public GemType CurrentGem => Definition.affiliatedGem;
    public event Action<BoardClearContext> AffiliatedGemCleared;
    private RunSession run;
    private int environmentVariant = -1;
    public void Initialize(RunSession owner,string zoneId)
    {
        run=owner;
        Definition=Resources.Load<ZoneDefinition>("Zones/"+zoneId);
        if(Definition==null) throw new InvalidOperationException("Saved zone is unavailable: "+zoneId);
        run.Board.BoardClearResolved+=ObserveClear;
        run.Board.ValidPlayerMoveCompleted+=MoveEnded;
        run.Waves.WaveStarted+=EncounterScenery;
    }
    private void Start()
    {
        run.Board.InitializeMine();
        if (Definition.maturesStone && run.Board.GetComponent<MineEnvironmentView>() == null)
            run.Board.gameObject.AddComponent<MineEnvironmentView>();
        if(Definition.periodicallyFloods && run.Board.GetComponent<AquaticEnvironmentView>()==null)
            run.Board.gameObject.AddComponent<AquaticEnvironmentView>();
        BackgroundMusicPlayer.Instance?.SetZoneMusic(Definition?.music);
        if(Definition?.theme==null) return;
        run.Board.GetComponent<BoardVisuals>()?.ApplyGameplayTheme(Definition.theme);
        EncounterScenery(run.Waves.CurrentWave);
    }
    private void EncounterScenery(int wave)
    {
        if (Definition?.theme == null) return;
        int local = run.Travel?.LocalWave ?? wave;
        int variant = Definition.periodicallyFloods ? (local >= 16 ? 2 : local >= 8 ? 1 : 0) : 0;
        if (environmentVariant == variant) return;
        environmentVariant = variant;
        FindFirstObjectByType<BattleBackgroundTilemapController>()?.ApplyGameplayTheme(Definition.theme, variant);
    }
    public ZoneTestEncounter TestEncounter(int wave)
    {
        var fixtures=Definition.developmentEncounters;
        if(fixtures==null || fixtures.Length==0) throw new InvalidOperationException("Development zone has no test encounters.");
        return fixtures[(Mathf.Max(1,wave)-1+Mathf.Max(0,run.MoveClock.TestEncounterOffset))%fixtures.Length];
    }
    public ZoneTestEncounter Encounter(int wave,System.Random random)
    {
        if(run.Travel?.State.enabled!=true) return Definition.zoneId=="dungeon" ? null : TestEncounter(wave);
        // Preserve the existing opening kingdom progression through its first King.
        if(Definition.zoneId=="dungeon" && run.Travel.State.visit==0) return null;
        int local=run.Travel.LocalWave;
        var choices=new System.Collections.Generic.List<ZoneTestEncounter>();
        foreach(var entry in Definition.liveEncounters)
            if(local>=entry.firstLocalWave && local<=entry.lastLocalWave) choices.Add(entry);
        if(Definition.periodicallyFloods && choices.Count>0)
        {
            bool wet=run.Board.IsFlooded;
            bool pending=run.Board.Aquatic?.phase==TidePhase.Pending;
            choices.RemoveAll(e=>(e.requiredTide==1&&wet)||(e.requiredTide==2&&!wet)||
                ((wet||pending)&&!run.Board.FormationCanFlood(e.members)));
            if ((run.Board.Aquatic?.floodCount ?? 0) == 0 && pending && local < Definition.apexLocalWave)
                choices.RemoveAll(e => !BoardController.FirstFloodLessonSafe(e.members));
            if (run.Travel.State.completedCourtVisits == 0)
                choices.RemoveAll(e => e.label.StartsWith("R31:") || e.label.StartsWith("R32:"));
            var recent=run.Travel.State.recentCourtRecipes;
            var fresh=choices.FindAll(e=>!recent.Contains(e.label));
            if(fresh.Count>0) choices=fresh;
            if(choices.Count>0)
            {
                int total=0;foreach(var e in choices) total+=Mathf.Max(1,e.weight);
                int choice=random.Next(total);var picked=choices[0];
                foreach(var e in choices) { choice-=Mathf.Max(1,e.weight);if(choice<0){picked=e;break;} }
                recent.Add(picked.label);while(recent.Count>4) recent.RemoveAt(0);
                return picked;
            }
            // A tide constraint cannot fall through to the unrelated dungeon pool.
            foreach(var e in Definition.liveEncounters)
                if(e.firstLocalWave<=3 && e.requiredTide==0 && run.Board.FormationCanFlood(e.members)) return e;
            throw new InvalidOperationException("The aquatic zone has no compatible relief formation.");
        }
        return choices.Count==0 ? null : choices[random.Next(choices.Count)];
    }
    public EnemyDefinition FindEnemy(string name)
    {
        foreach(var enemy in Definition.enemies) if(enemy!=null && enemy.name==name) return enemy;
        return null;
    }
    public float EligibleDamageMultiplier(BoardClearContext context) =>
        context.GemCount>0 && context.GemType==CurrentGem && EligibleSource(context) ? Definition.affiliatedDamageMultiplier : 1f;
    public static bool EligibleSource(BoardClearContext context) => context.IsMatchClear || context.IsSpecialClear || context.Source==BoardClearSource.Ability;
    private void ObserveClear(BoardClearContext context)
    {
        if(context.GemCount>0 && context.GemType==CurrentGem && EligibleSource(context)) AffiliatedGemCleared?.Invoke(context);
    }
    private void MoveEnded(int move)
    {
        // Move-profile runs advance the environment before actor opportunities.
        // The original dungeon seconds profile has no combat move coordinator.
        if(!CombatMoveClock.Active) run.Board.QueueZoneEnvironment(move);
    }
    private void OnDestroy()
    {
        if(run==null) return;
        if(run.Waves!=null) run.Waves.WaveStarted-=EncounterScenery;
        if(run.Board!=null) { run.Board.BoardClearResolved-=ObserveClear;run.Board.ValidPlayerMoveCompleted-=MoveEnded; }
    }
}
