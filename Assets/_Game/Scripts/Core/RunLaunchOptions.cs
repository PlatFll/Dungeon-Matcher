/// <summary>One scene handoff; durable runs store their challenge in their journal.</summary>
public static class RunLaunchOptions
{
    // Consumed only by an explicit development launch. No public mode setting.
    public static bool ForestPrototype;
    public static int ForestEncounterOffset;
    public static bool Practice;
    public static RunChallenge Challenge;
    public static bool ChangeBuild;
}
