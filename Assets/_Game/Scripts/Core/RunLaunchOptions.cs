/// <summary>One scene handoff; durable runs store their challenge in their journal.</summary>
public static class RunLaunchOptions
{
    // Permanent player rule: every fresh run and retry begins in the Dungeon.
    public static bool TestingZonePickerEnabled => false;
#if UNITY_EDITOR
    // Explicit Editor diagnostics only. This field does not exist in player builds.
    public static string StartingZone;
#endif
    public static string ConsumeDevelopmentZone()
    {
#if UNITY_EDITOR
        string zone = StartingZone;
        StartingZone = null;
        return zone;
#else
        return null;
#endif
    }
    public static void ClearDevelopmentLaunch()
    {
        ConsumeDevelopmentZone();
        ForestPrototype = false;
        ForestClockProfile = CombatClockSnapshot.HybridProfile;
        ForestEncounterOffset = 0;
    }

    // Consumed only by an explicit Editor launch. No public mode setting.
    public static bool ForestPrototype;
    public static string ForestClockProfile = CombatClockSnapshot.HybridProfile;
    public static int ForestEncounterOffset;
    public static bool Practice;
    public static RunChallenge Challenge;
    public static bool ChangeBuild;
}
