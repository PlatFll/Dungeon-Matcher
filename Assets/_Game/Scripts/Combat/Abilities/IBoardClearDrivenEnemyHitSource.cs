using System;

public interface IBoardClearDrivenEnemyHitSource
{
    event Action<
        EnemyActor,
        int,
        BoardClearContext
    > HitResolved;
}