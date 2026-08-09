using UnityEngine;
using UnityEngine.UI;

public static class EnemyVisualPresenter
{
    private const string VisualRootName =
        "VisualRoot";

    public static bool TryApply(
        GameObject enemyObject,
        EnemyDefinition definition)
    {
        if (enemyObject == null ||
            definition == null)
        {
            return false;
        }

        if (!TryResolveVisualComponents(
                enemyObject,
                out Image enemyImage,
                out Animator animator,
                out CharacterAnimationPlayback playback))
        {
            return false;
        }

        Sprite fallbackSprite =
            definition.FallbackVisualSprite;

        if (fallbackSprite != null)
        {
            enemyImage.sprite =
                fallbackSprite;

            enemyImage.preserveAspect = true;
            enemyImage.enabled = true;
        }

        RuntimeAnimatorController controller =
            definition.AnimationControllerOverride;

        if (controller != null)
        {
            if (animator == null)
            {
                Debug.LogError(
                    $"{definition.DisplayName} has an animation controller " +
                    "override, but its enemy prefab VisualRoot has no Animator.",
                    enemyObject
                );

                return false;
            }

            animator.runtimeAnimatorController =
                controller;

            animator.enabled = true;

            /*
             * Evaluate the newly assigned controller immediately so the spawn
             * VFX never exposes a stale frame from the shared prefab or the
             * fallback sprite for one rendered frame.
             */
            animator.Rebind();
            animator.Update(0f);

            if (playback != null)
            {
                playback.enabled = true;
                playback.ResumeAtDefaultSpeed();
            }

            return true;
        }

        /*
         * Current early-enemy art only contains one imported sprite each.
         * Until an Animator Controller/Animator Override Controller is assigned,
         * keep the correct fallback frame visible and prevent the shared
         * prefab's Knight controller from leaking Knight animation frames.
         */
        if (fallbackSprite != null)
        {
            if (animator != null)
            {
                animator.enabled = false;
            }

            if (playback != null)
            {
                playback.enabled = false;
            }

            return true;
        }

        /*
         * No per-enemy visual override was supplied. Leave the prefab-native
         * visual setup untouched. This preserves the Knight's existing
         * controller and also supports future dedicated enemy prefabs.
         */
        return true;
    }

    public static bool TryApplyFallbackOnly(
        GameObject enemyObject,
        Sprite fallbackSprite)
    {
        if (enemyObject == null ||
            fallbackSprite == null)
        {
            return false;
        }

        if (!TryResolveVisualComponents(
                enemyObject,
                out Image enemyImage,
                out Animator animator,
                out CharacterAnimationPlayback playback))
        {
            return false;
        }

        enemyImage.sprite =
            fallbackSprite;

        enemyImage.preserveAspect = true;
        enemyImage.enabled = true;

        if (animator != null)
        {
            animator.enabled = false;
        }

        if (playback != null)
        {
            playback.enabled = false;
        }

        return true;
    }

    private static bool TryResolveVisualComponents(
        GameObject enemyObject,
        out Image enemyImage,
        out Animator animator,
        out CharacterAnimationPlayback playback)
    {
        enemyImage = null;
        animator = null;
        playback = null;

        Transform visualRoot =
            enemyObject.transform.Find(
                VisualRootName
            );

        if (visualRoot == null)
        {
            Debug.LogError(
                $"{enemyObject.name} has no '{VisualRootName}' child " +
                "for enemy presentation.",
                enemyObject
            );

            return false;
        }

        enemyImage =
            visualRoot.GetComponent<Image>();

        if (enemyImage == null)
        {
            Debug.LogError(
                $"{enemyObject.name}'s '{VisualRootName}' has no Image.",
                enemyObject
            );

            return false;
        }

        animator =
            visualRoot.GetComponent<Animator>();

        playback =
            visualRoot.GetComponent<
                CharacterAnimationPlayback
            >();

        return true;
    }
}
