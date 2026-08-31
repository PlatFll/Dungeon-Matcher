using System.Collections.Generic;

public interface IEnemySpecialAbilityRuntime
{
    void InitializeSpecialAbility(
        EnemyActor enemyActor,
        BoardController boardController,
        IReadOnlyList<EnemyActor> activeEnemies
    );
}
