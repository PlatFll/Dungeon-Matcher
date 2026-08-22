using System.Collections;
using UnityEngine;

/*
 * Small process-wide runtime bootstrap. Keep device-level configuration here
 * instead of coupling it to the board or combat systems.
 */
public sealed class GameManager : MonoBehaviour
{
    private const int MobileTargetFrameRate = 60;
    private const float InitialWaveRecoveryDelay = 0.75f;

    private static GameManager instance;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.BeforeSceneLoad
    )]
    private static void Install()
    {
        if (!Application.isPlaying ||
            instance != null)
        {
            return;
        }

        GameObject runtimeObject =
            new GameObject(
                "GameRuntime"
            );

        instance =
            runtimeObject.AddComponent<
                GameManager
            >();

        DontDestroyOnLoad(
            runtimeObject
        );
    }

    private void Awake()
    {
        if (instance != null &&
            instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        ConfigureFramePacing();

        if (Application.isMobilePlatform)
        {
            StartCoroutine(
                RecoverInitialWaveIfNeeded()
            );
        }
    }

    private static void ConfigureFramePacing()
    {
        if (!Application.isMobilePlatform)
        {
            return;
        }

        /*
         * Mobile platforms otherwise commonly fall back to 30 FPS. The board
         * is intentionally tuned around responsive 60 FPS touch interaction.
         */
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate =
            MobileTargetFrameRate;
    }

    private IEnumerator RecoverInitialWaveIfNeeded()
    {
        yield return new WaitForSecondsRealtime(
            InitialWaveRecoveryDelay
        );

        WaveController waveController =
            FindFirstObjectByType<WaveController>();

        if (waveController == null ||
            waveController.CurrentWave != 1 ||
            waveController.IsWaveActive ||
            waveController.ActiveEnemies.Count > 0)
        {
            yield break;
        }

        if (!waveController.enabled)
        {
            Debug.LogError(
                "WaveController is disabled on mobile after startup. " +
                "Check the player log for its reference-validation error.",
                waveController
            );

            yield break;
        }

        /*
         * A presentation/runtime exception can stop the first spawn coroutine
         * before it marks the wave active. Retry once after the scene and
         * responsive UI hierarchy have had time to settle.
         */
        Debug.LogWarning(
            "Initial mobile wave did not become active. Retrying wave 1 once.",
            waveController
        );

        waveController.SpawnCurrentWave();
    }
}
