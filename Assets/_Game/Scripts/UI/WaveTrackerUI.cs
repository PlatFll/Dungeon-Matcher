using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class WaveTrackerUI :
    MonoBehaviour
{
    [Header("References")]

    [SerializeField]
    private WaveController waveController;

    [SerializeField]
    private TMP_Text waveText;

    [Header("Display")]

    [SerializeField]
    private string waveLabelFormat =
        "WAVE {0}";

    private void Awake()
    {
        RefreshFromController();
    }

    private void OnEnable()
    {
        SubscribeToWaveController();
        RefreshFromController();
    }

    private void OnDisable()
    {
        UnsubscribeFromWaveController();
    }

    private void SubscribeToWaveController()
    {
        if (waveController == null)
        {
            return;
        }

        /*
         * Remove first to prevent an accidental duplicate
         * subscription after disabling and enabling the UI.
         */
        waveController.WaveStarted -=
            HandleWaveStarted;

        waveController.WaveStarted +=
            HandleWaveStarted;
    }

    private void UnsubscribeFromWaveController()
    {
        if (waveController == null)
        {
            return;
        }

        waveController.WaveStarted -=
            HandleWaveStarted;
    }

    private void HandleWaveStarted(
        int waveNumber)
    {
        SetWaveNumber(waveNumber);
    }

    private void RefreshFromController()
    {
        if (waveController == null)
        {
            SetWaveNumber(1);
            return;
        }

        SetWaveNumber(
            waveController.CurrentWave
        );
    }

    private void SetWaveNumber(
        int waveNumber)
    {
        if (waveText == null)
        {
            return;
        }

        int safeWaveNumber =
            Mathf.Max(
                1,
                waveNumber
            );

        waveText.text =
            string.Format(
                waveLabelFormat,
                safeWaveNumber
            );
    }
}