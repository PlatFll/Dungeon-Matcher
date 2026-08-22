using System;
using UnityEngine;

/*
 * Compatibility wrapper for the current WaveController spawn path.
 * Final per-enemy visual configuration is applied by EnemyVisualPresenter
 * from EnemyActor.Initialize, where the full EnemyDefinition is available.
 *
 * Presentation is deliberately fail-soft: a visual problem must never stop
 * the gameplay enemy from being created or the wave from becoming active.
 */
public static class EnemyStaticSpritePresenter
{
    public static bool TryApply(
        GameObject enemyObject,
        Sprite staticSprite)
    {
        try
        {
            return EnemyVisualPresenter
                .TryApplyFallbackOnly(
                    enemyObject,
                    staticSprite
                );
        }
        catch (Exception exception)
        {
            Debug.LogError(
                "Enemy fallback presentation failed. " +
                "Enemy spawning will continue."
            );

            Debug.LogException(
                exception
            );

            return false;
        }
    }
}
