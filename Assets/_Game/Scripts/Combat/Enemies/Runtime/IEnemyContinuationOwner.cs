using System;

public interface IEnemyContinuationOwner
{
    void CaptureContinuation(EnemyCombatSnapshot saved, Func<EnemyActor,int> slotOf);
    void RestoreContinuation(EnemyCombatSnapshot saved, Func<int,EnemyActor> enemyAt);
}
