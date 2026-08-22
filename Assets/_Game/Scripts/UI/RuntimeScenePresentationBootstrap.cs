using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Re-applies presentation components that are intentionally created at runtime
/// whenever a scene is loaded or reloaded.
///
/// RuntimeInitializeOnLoadMethod(AfterSceneLoad) is a play-session startup hook;
/// it is not a reliable per-SceneManager.LoadScene callback. Retry reloads the
/// serialized Game scene, so without this bootstrap the scene comes back with
/// the old authored hierarchy and none of the runtime battle-layout components.
/// </summary>
public static class RuntimeScenePresentationBootstrap
{
    private const string TopHudName = "TopHUD";
    private const string PlayerHealthBarName = "PlayerHPBarBackground";
    private const string EnemyHealthBarName = "EnemyHPBarBackground";
    private const string PlayerCharacterName = "PlayerCharacter";
    private const string EnemySpawnAnchorName = "EnemySpawnAnchor";

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration
    )]
    private static void ResetSceneLoadedSubscription()
    {
        /*
         * This also protects projects using Enter Play Mode Options with domain
         * reload disabled. A previous play session must never leave another copy
         * of our sceneLoaded delegate registered.
         */
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.BeforeSceneLoad
    )]
    private static void RegisterForSceneLoads()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.AfterSceneLoad
    )]
    private static void EnsureInitialScenePresentation()
    {
        /*
         * The sceneLoaded callback normally covers the first scene as well. This
         * idempotent fallback also covers editor play configurations that keep the
         * currently-open scene instead of performing a normal scene load.
         */
        EnsureRuntimePresentationInstalled();
    }

    private static void HandleSceneLoaded(
        Scene scene,
        LoadSceneMode loadMode)
    {
        EnsureRuntimePresentationInstalled();
    }

    private static void EnsureRuntimePresentationInstalled()
    {
        InstallTopBattleControllers();
        InstallModularHealthBars();
        InstallPixelPerfectBattleCharacters();
        InstallGameOverController();
    }

    private static void InstallTopBattleControllers()
    {
        GameObject topHud =
            GameObject.Find(TopHudName);

        if (topHud == null)
        {
            return;
        }

        /*
         * Add the structural controller first. It builds the generated split
         * battle hierarchy in Start; TopBattlePresentationController then applies
         * the responsive dimensions/floor presentation in LateUpdate.
         */
        if (!topHud.TryGetComponent(
                out TopBattleLayoutController _
            ))
        {
            topHud.AddComponent<TopBattleLayoutController>();
        }

        if (!topHud.TryGetComponent(
                out TopBattlePresentationController _
            ))
        {
            topHud.AddComponent<TopBattlePresentationController>();
        }
    }

    private static void InstallModularHealthBars()
    {
        RectTransform[] rects =
            Object.FindObjectsByType<RectTransform>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (RectTransform rect in rects)
        {
            if (rect == null ||
                (rect.name != PlayerHealthBarName &&
                 rect.name != EnemyHealthBarName))
            {
                continue;
            }

            if (!rect.TryGetComponent(
                    out ModularHealthBarUI _
                ))
            {
                rect.gameObject.AddComponent<ModularHealthBarUI>();
            }
        }
    }

    private static void InstallPixelPerfectBattleCharacters()
    {
        RectTransform[] rects =
            Object.FindObjectsByType<RectTransform>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (RectTransform rect in rects)
        {
            if (rect == null ||
                (rect.name != PlayerCharacterName &&
                 rect.name != EnemySpawnAnchorName))
            {
                continue;
            }

            if (!rect.TryGetComponent(
                    out PixelPerfectBattleCharacterUI _
                ))
            {
                rect.gameObject.AddComponent<PixelPerfectBattleCharacterUI>();
            }
        }
    }

    private static void InstallGameOverController()
    {
        PlayerActor player =
            Object.FindFirstObjectByType<PlayerActor>();

        if (player == null)
        {
            return;
        }

        if (!player.TryGetComponent(
                out GameOverPresentationController _
            ))
        {
            player.gameObject.AddComponent<GameOverPresentationController>();
        }
    }
}
