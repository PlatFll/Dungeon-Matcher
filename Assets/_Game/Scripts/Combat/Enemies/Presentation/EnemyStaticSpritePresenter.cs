using UnityEngine;
using UnityEngine.UI;

public static class EnemyStaticSpritePresenter
{
    private const string VisualRootName =
        "VisualRoot";

    public static bool TryApply(
        GameObject enemyObject,
        Sprite staticSprite)
    {
        if (enemyObject == null ||
            staticSprite == null)
        {
            return false;
        }

        Transform visualRoot =
            enemyObject.transform.Find(
                VisualRootName
            );

        if (visualRoot == null)
        {
            Debug.LogError(
                $"{enemyObject.name} has no '{VisualRootName}' child " +
                "for its static enemy sprite.",
                enemyObject
            );

            return false;
        }

        Image enemyImage =
            visualRoot.GetComponent<Image>();

        if (enemyImage == null)
        {
            Debug.LogError(
                $"{enemyObject.name}'s '{VisualRootName}' has no Image.",
                enemyObject
            );

            return false;
        }

        /*
         * Early enemies reuse the polished enemy shell: health display,
         * affinity color, attack timer, hit shake, spawn/death VFX and lunge.
         * Only the character artwork is replaced.
         */
        enemyImage.sprite = staticSprite;
        enemyImage.preserveAspect = true;
        enemyImage.enabled = true;

        /*
         * The shared prototype prefab currently contains the Knight animator.
         * Disable character animation when a definition supplies a static
         * sprite so an animation frame can never overwrite that artwork.
         */
        Animator animator =
            visualRoot.GetComponent<Animator>();

        if (animator != null)
        {
            animator.enabled = false;
        }

        CharacterAnimationPlayback playback =
            visualRoot.GetComponent<
                CharacterAnimationPlayback
            >();

        if (playback != null)
        {
            playback.enabled = false;
        }

        return true;
    }
}
