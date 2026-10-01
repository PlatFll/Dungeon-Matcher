using System;
using System.Collections;
using UnityEngine;

public partial class EnemyActor
{
    private int specialMotionId, specialMotionBeat;
    private bool specialMotionComplete;
    private float specialMotionStartedAt;
    public int SpecialMotionId => specialMotionId;
    public string SpecialMotionState { get; private set; }
    public string SpecialIdleState { get; set; }
    public event Action<EnemyActor> SpecialMotionRequested;

    // The ability/board still owns the action. Animation only supplies contact
    // cues; a scaled-time fallback keeps missing presentation from stalling play.
    public int PrepareSpecialMotion(string state = "Ability")
    {
        if (ActiveSpecialAbilityAnimationActionId <= 0 || Definition == null ||
            !Definition.UseAuthoredSpecialAbilityMotion) return 0;
        specialMotionId = ActiveSpecialAbilityAnimationActionId;
        specialMotionBeat = 0;
        specialMotionComplete = false;
        specialMotionStartedAt = Time.time;
        SpecialMotionState = state;
        return specialMotionId;
    }

    public int StartSpecialMotion(string state = "Ability")
    {
        int id = PrepareSpecialMotion(state);
        PlaySpecialMotion(id);
        return id;
    }

    public void PlaySpecialMotion(int id)
    {
        if (!IsSpecialMotionCurrent(id)) return;
        specialMotionBeat = 0;
        specialMotionComplete = false;
        specialMotionStartedAt = Time.time;
        SpecialMotionRequested?.Invoke(this);
    }

    public bool IsSpecialMotionCurrent(int id) => id > 0 && isActiveAndEnabled &&
        !IsDefeated && id == specialMotionId && id == ActiveSpecialAbilityAnimationActionId;

    public void NotifySpecialMotionBeat(int id, int beat)
    {
        if (Time.timeScale > 0f && IsSpecialMotionCurrent(id))
            specialMotionBeat = Mathf.Max(specialMotionBeat, beat);
    }

    public void NotifySpecialMotionComplete(int id)
    {
        if (Time.timeScale > 0f && IsSpecialMotionCurrent(id)) specialMotionComplete = true;
    }

    public IEnumerator WaitForSpecialMotionBeat(int id, int beat = 1)
    {
        while (IsSpecialMotionCurrent(id) && specialMotionBeat < beat &&
               Time.time - specialMotionStartedAt < 3f + (beat - 1) * .45f)
            yield return null;
    }

    public IEnumerator WaitForSpecialMotionComplete(int id)
    {
        while (IsSpecialMotionCurrent(id) && !specialMotionComplete &&
               Time.time - specialMotionStartedAt < 4f) yield return null;
    }
}
