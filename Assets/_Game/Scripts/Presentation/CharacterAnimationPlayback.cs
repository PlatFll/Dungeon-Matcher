using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Animator))]
public sealed class CharacterAnimationPlayback : MonoBehaviour
{
    [SerializeField]
    private Animator animator;

    [SerializeField, Min(0.01f)]
    private float defaultPlaybackSpeed = 1f;

    private float speedBeforePause = 1f;
    private bool isPaused;

    public event Action AutoAttackImpactReached;
    public event Action AbilityImpactReached;

    public bool IsPaused => isPaused;

    private void Awake()
    {
        ResolveAnimator();
    }

    private void ResolveAnimator()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    public void Pause()
    {
        ResolveAnimator();

        if (animator == null || isPaused)
        {
            return;
        }

        speedBeforePause =
            animator.speed > 0f
                ? animator.speed
                : defaultPlaybackSpeed;

        animator.speed = 0f;
        isPaused = true;
    }

    public void Resume()
    {
        ResolveAnimator();

        if (animator == null || !isPaused)
        {
            return;
        }

        animator.speed = Mathf.Max(0.01f, speedBeforePause);
        isPaused = false;
    }

    public void ResumeAtDefaultSpeed()
    {
        ResolveAnimator();

        if (animator == null)
        {
            return;
        }

        animator.speed = defaultPlaybackSpeed;
        speedBeforePause = defaultPlaybackSpeed;
        isPaused = false;
    }

    /*
     * Unity Animation Events call these methods on the VisualRoot object.
     * They intentionally contain no combat logic; they only relay the exact
     * impact frame to the enemy runtime components on the parent object.
     */
    public void AutoAttackImpact()
    {
        AutoAttackImpactReached?.Invoke();
    }

    public void AbilityImpact()
    {
        AbilityImpactReached?.Invoke();
    }

    private void OnDisable()
    {
        if (animator != null && isPaused)
        {
            animator.speed = Mathf.Max(0.01f, speedBeforePause);
        }

        isPaused = false;
    }

    private void OnValidate()
    {
        defaultPlaybackSpeed = Mathf.Max(0.01f, defaultPlaybackSpeed);
    }
}
