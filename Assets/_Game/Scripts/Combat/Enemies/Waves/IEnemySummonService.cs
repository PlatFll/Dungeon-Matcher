public interface IEnemySummonService
{
    bool HasFreeEnemySlot { get; }

    bool TrySummonEnemy(
        EnemyDefinition definition,
        out EnemyActor summonedEnemy
    );
}

// Optional fixed-slot support for a telegraphed summon. Existing immediate
// summoners keep their original API and free-slot policy.
public interface IEnemyFixedSlotSummonService : IEnemySummonService
{
    int FirstFreeSummonSlot { get; }
    bool TrySummonEnemyAt(EnemyDefinition definition, int slotIndex, out EnemyActor summonedEnemy);
}
