using System;
using UnityEngine;
#if UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

public enum HapticStrength { None, Tick, Impact, Cast }

/// <summary>Bounded promotion: an important contact can follow a detail tick,
/// but never sooner than 40ms or more than six requests in a one-second window.</summary>
public sealed class HapticBudget
{
    private float lastTime = float.NegativeInfinity, windowStart = float.NegativeInfinity;
    private int lastPriority = -1, count;
    public bool TryConsume(float now, int priority)
    {
        if (now - windowStart >= 1f) { windowStart = now; count = 0; }
        float interval = priority > lastPriority ? .04f : priority == 0 ? .20f : .13f;
        if (now - lastTime < interval || count >= 6) return false;
        lastTime = now; lastPriority = priority; count++;
        return true;
    }
    public void Reset() { lastTime = windowStart = float.NegativeInfinity; lastPriority = -1; count = 0; }
}

/// <summary>One short device-tuned pulse per batch. No generic long vibrate fallback.</summary>
public static class MobileHaptics
{
    private static HapticStrength pending;
    private static int pendingPriority = -1;
    private static readonly HapticBudget cooldown = new HapticBudget();
    public static int Requests { get; private set; }
    public static int Coalesced { get; private set; }
    public static int Suppressed { get; private set; }
    public static int BudgetDrops { get; private set; }
    public static int DispatchAttempts { get; private set; }
    public static HapticStrength LastDispatched { get; private set; }
    private static volatile int generation;
    private static volatile bool unavailable;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset()
    {
        pending = HapticStrength.None; pendingPriority = -1; cooldown.Reset(); unavailable = false; generation++;
        Requests = Coalesced = Suppressed = BudgetDrops = DispatchAttempts = 0;
        LastDispatched = HapticStrength.None;
    }
    public static void Request(HapticStrength strength, int priority = -1)
    {
        if (strength == HapticStrength.None) return;
        Requests++;
        if (AudioPreferences.VibrationMuted || !CombatAudioController.FeedbackAllowed || unavailable)
        { Suppressed++; return; }
        if (priority < 0) priority = (int)strength;
        if (pending != HapticStrength.None) Coalesced++;
        if (priority > pendingPriority || priority == pendingPriority && strength > pending)
        { pending = strength; pendingPriority = priority; }
    }
    public static void Cancel()
    {
        pending = HapticStrength.None; pendingPriority = -1; generation++;
#if UNITY_IOS && !UNITY_EDITOR
        if (!unavailable)
        {
            try { DMCancelHaptic(); }
            catch (Exception) { unavailable = true; }
        }
#endif
    }
    public static void Flush()
    {
        HapticStrength strength = pending; int priority = pendingPriority;
        pending = HapticStrength.None; pendingPriority = -1;
        if (strength == HapticStrength.None) return;
        if (unavailable || AudioPreferences.VibrationMuted || !CombatAudioController.FeedbackAllowed)
        { Suppressed++; return; }
        if (!cooldown.TryConsume(Time.unscaledTime, priority)) { BudgetDrops++; return; }
        DispatchAttempts++; LastDispatched = strength;
        Play(strength);
    }
    private static void Play(HapticStrength strength)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        int requestGeneration = generation;
        try
        {
            using (var unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = unity.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                // Android's View API honors system touch-feedback preferences without a permission.
                // Constants are available from API 21: CLOCK_TICK, KEYBOARD_TAP and LONG_PRESS.
                int effect = strength == HapticStrength.Tick ? 4 : strength == HapticStrength.Cast ? 0 : 3;
                activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    if (requestGeneration != generation) return;
                    try
                    {
                        using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                        using (var current = player.GetStatic<AndroidJavaObject>("currentActivity"))
                        using (var window = current.Call<AndroidJavaObject>("getWindow"))
                        using (var view = window.Call<AndroidJavaObject>("getDecorView"))
                            view.Call<bool>("performHapticFeedback", effect);
                    }
                    catch (Exception) { unavailable = true; }
                }));
            }
        }
        catch (Exception) { unavailable = true; }
#elif UNITY_IOS && !UNITY_EDITOR
        try { DMPlayHaptic((int)strength); }
        catch (Exception) { unavailable = true; }
#endif
    }
#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void DMPlayHaptic(int strength);
    [DllImport("__Internal")] private static extern void DMCancelHaptic();
#endif
}
