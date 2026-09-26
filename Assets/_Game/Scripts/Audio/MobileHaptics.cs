using System;
using UnityEngine;
#if UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

public enum HapticStrength { None, Tick, Impact, Cast }

/// <summary>One short device-tuned pulse per batch. No generic long vibrate fallback.</summary>
public static class MobileHaptics
{
    private static HapticStrength pending;
    private static readonly FeedbackCooldown cooldown = new FeedbackCooldown();
    private static volatile int generation;
    private static volatile bool unavailable;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset()
    {
        pending = HapticStrength.None; cooldown.Reset(); unavailable = false; generation++;
    }
    public static void Request(HapticStrength strength)
    {
        if (!AudioPreferences.VibrationMuted && CombatAudioController.FeedbackAllowed && strength > pending)
            pending = strength;
    }
    public static void Cancel()
    {
        pending = HapticStrength.None; generation++;
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
        HapticStrength strength = pending; pending = HapticStrength.None;
        if (strength == HapticStrength.None || unavailable || AudioPreferences.VibrationMuted ||
            !CombatAudioController.FeedbackAllowed || !cooldown.TryConsume(Time.unscaledTime,
                strength == HapticStrength.Tick ? 0.20f : 0.13f)) return;
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
