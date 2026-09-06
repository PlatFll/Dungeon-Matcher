using UnityEngine;
using UnityEngine.SceneManagement;

public static class RunUpgradeBootstrap
{
    private const string CatalogResourcePath =
        "RunUpgrades/PrototypeRunUpgradeCatalog";

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration
    )]
    private static void ResetSceneLoadedSubscription()
    {
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
    private static void EnsureInitialRunUpgradeSystem()
    {
        EnsureInstalled();
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureInstalled();
    }

    private static void EnsureInstalled()
    {
        WaveController waves =
            Object.FindFirstObjectByType<WaveController>();
        BoardController board =
            Object.FindFirstObjectByType<BoardController>();
        PlayerActor player =
            Object.FindFirstObjectByType<PlayerActor>();
        Canvas canvas = ResolveOverlayCanvas();

        if (waves == null || board == null || player == null || canvas == null)
        {
            return;
        }

        RunUpgradeCatalog catalog =
            Resources.Load<RunUpgradeCatalog>(CatalogResourcePath);

        if (catalog == null)
        {
            Debug.LogError(
                $"Run upgrade catalog Resources/{CatalogResourcePath} " +
                "could not be loaded. Base gameplay will continue.",
                waves
            );
            return;
        }

        RunUpgradeRuntime runtime;

        if (!waves.TryGetComponent(out runtime))
        {
            runtime = waves.gameObject.AddComponent<RunUpgradeRuntime>();
        }

        runtime.Configure(catalog, player, waves);

        UpgradeChoiceUI ui;

        if (!canvas.TryGetComponent(out ui))
        {
            ui = canvas.gameObject.AddComponent<UpgradeChoiceUI>();
        }

        ui.Configure(canvas);

        RunUpgradeCoordinator coordinator;

        if (!waves.TryGetComponent(out coordinator))
        {
            coordinator = waves.gameObject.AddComponent<RunUpgradeCoordinator>();
        }

        coordinator.Configure(runtime, waves, board, player, ui);
    }

    private static Canvas ResolveOverlayCanvas()
    {
        Canvas[] canvases = Object.FindObjectsByType<Canvas>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        for (int index = 0; index < canvases.Length; index++)
        {
            Canvas canvas = canvases[index];

            if (canvas != null && canvas.isRootCanvas &&
                canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                return canvas;
            }
        }

        return canvases.Length > 0 ? canvases[0].rootCanvas : null;
    }
}
