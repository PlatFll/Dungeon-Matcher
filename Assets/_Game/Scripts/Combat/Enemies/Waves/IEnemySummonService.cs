public interface IEnemySummonService
{
    bool HasFreeEnemySlot { get; }

    bool TrySummonEnemy(
        EnemyDefinition definition,
        out EnemyActor summonedEnemy
    );
}
