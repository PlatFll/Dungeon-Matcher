using System;

public interface IMatchDrivenEnemyHitSource
{
    event Action<
        EnemyActor,
        int,
        BoardMatchContext
    > HitResolved;
}