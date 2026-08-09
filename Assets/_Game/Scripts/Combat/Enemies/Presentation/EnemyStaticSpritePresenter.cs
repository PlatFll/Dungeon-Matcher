using UnityEngine;

/*
 * Compatibility wrapper for the current WaveController spawn path.
 * Final per-enemy visual configuration is applied by EnemyVisualPresenter
 * from EnemyActor.Initialize, where the full EnemyDefinition is available.
 */
public static class EnemyStaticSpritePresenter
{
    public static bool TryApply(
        GameObject enemyObject,
        Sprite staticSprite)
    {
        return EnemyVisualPresenter
            .TryApplyFallbackOnly(
                enemyObject,
                staticSprite
            );
    }
}
