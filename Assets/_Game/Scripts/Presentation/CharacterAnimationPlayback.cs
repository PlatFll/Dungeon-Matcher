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
    private AnimatorUpdateMode? priorUpdateMode;
    public float AuthoredDeathSeconds {get;private set;}

    public bool TryPlayState(string state,bool unscaled=false)
    {
        ResolveAnimator();
        if(animator==null || animator.runtimeAnimatorController==null || !animator.isActiveAndEnabled ||
            !animator.HasState(0,Animator.StringToHash("Base Layer."+state)))return false;
        ResumeAtDefaultSpeed();
        if(unscaled)
        {
            if(!priorUpdateMode.HasValue)priorUpdateMode=animator.updateMode;
            animator.updateMode=AnimatorUpdateMode.UnscaledTime;
        }
        animator.Play("Base Layer."+state,0,0);animator.Update(0);
        AuthoredDeathSeconds=state=="Death"?animator.GetCurrentAnimatorStateInfo(0).length:0;
        return true;
    }
    public void RestoreAuthoredClock()
    {
        if(animator!=null && priorUpdateMode.HasValue)animator.updateMode=priorUpdateMode.Value;
        priorUpdateMode=null;AuthoredDeathSeconds=0;
    }

    public event Action AutoAttackImpactReached;
    public event Action AutoAttackCompleted;
    public event Action AbilityImpactReached;
    public event Action AbilityCompleted;
    public event Action<int> AbilityBeatReached;

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

    public void AutoAttackComplete()
    {
        AutoAttackCompleted?.Invoke();
    }

    public void AbilityBeat(int beat) => AbilityBeatReached?.Invoke(beat);

    public void AbilityComplete()
    {
        AbilityCompleted?.Invoke();
    }

    private void OnDisable()
    {
        RestoreAuthoredClock();
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
