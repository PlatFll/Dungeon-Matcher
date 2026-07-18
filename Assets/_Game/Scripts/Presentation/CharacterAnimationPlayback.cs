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