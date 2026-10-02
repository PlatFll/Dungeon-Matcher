using System;
using UnityEngine;

public sealed class ZoneRuntimeContext : MonoBehaviour
{
    public ZoneDefinition Definition { get; private set; }
    public GemType CurrentGem => Definition.affiliatedGem;
    public event Action<BoardClearContext> AffiliatedGemCleared;
    private RunSession run;
    public void Initialize(RunSession owner,string zoneId)
    {
        run=owner;
        Definition=Resources.Load<ZoneDefinition>("Zones/"+zoneId);
        if(Definition==null) throw new InvalidOperationException("Saved zone is unavailable: "+zoneId);
        run.Board.BoardClearResolved+=ObserveClear;
        run.Waves.WaveCompleted+=WaveEnded;
    }
    private void Start()
    {
        if(Definition?.theme==null) return;
        run.Board.GetComponent<BoardVisuals>()?.ApplyGameplayTheme(Definition.theme);
        FindFirstObjectByType<BattleBackgroundTilemapController>()?.ApplyGameplayTheme(Definition.theme);
    }
    public ZoneTestEncounter TestEncounter(int wave)
    {
        var fixtures=Definition.developmentEncounters;
        if(fixtures==null || fixtures.Length==0) throw new InvalidOperationException("Development zone has no test encounters.");
        return fixtures[(Mathf.Max(1,wave)-1+Mathf.Max(0,run.MoveClock.TestEncounterOffset))%fixtures.Length];
    }
    public EnemyDefinition FindEnemy(string name)
    {
        foreach(var enemy in Definition.enemies) if(enemy!=null && enemy.name==name) return enemy;
        return null;
    }
    public float EligibleDamageMultiplier(BoardClearContext context) =>
        context.GemCount>0 && context.GemType==CurrentGem && EligibleSource(context) ? 1.15f : 1f;
    public static bool EligibleSource(BoardClearContext context) => context.IsMatchClear || context.IsSpecialClear || context.Source==BoardClearSource.Ability;
    private void ObserveClear(BoardClearContext context)
    {
        if(context.GemCount>0 && context.GemType==CurrentGem && EligibleSource(context)) AffiliatedGemCleared?.Invoke(context);
    }
    private void WaveEnded(int wave) => run.Board.RemoveVineSource(null);
    private void OnDestroy()
    {
        if(run==null) return;
        if(run.Board!=null) { run.Board.BoardClearResolved-=ObserveClear;run.Board.RemoveVineSource(null); }
        if(run.Waves!=null) run.Waves.WaveCompleted-=WaveEnded;
    }
}
