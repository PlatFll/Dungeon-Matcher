/// <summary>One scene handoff; durable runs store their challenge in their journal.</summary>
public static class RunLaunchOptions
{
    // Temporary menu entry for zone testing. Disable this for the full release.
    public static bool TestingZonePickerEnabled => true;
    public static string StartingZone;

    // Consumed only by an explicit development launch. No public mode setting.
    public static bool ForestPrototype;
    // Internal regression fixture override; never exposed in player settings.
    public static string ForestClockProfile = CombatClockSnapshot.HybridProfile;
    public static int ForestEncounterOffset;
    public static bool Practice;
    public static RunChallenge Challenge;
    public static bool ChangeBuild;
}
