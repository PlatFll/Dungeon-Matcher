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

    private int displayedWave;

    private void Awake()
    {
        var plaque = FinalizedUiSkin.Load("WavePlaque");
        if (plaque != null && TryGetComponent<UnityEngine.UI.Image>(out var image))
        {
            image.sprite = plaque;
            image.color = Color.white;
            image.type = UnityEngine.UI.Image.Type.Simple;
        }
        RefreshFromController();
    }

    private void OnEnable()
    {
        SubscribeToWaveController();
        RefreshFromController();
    }

    private void Start()
    {
        if (DungeonPixelLabBattleArt.Controls is DungeonPixelLabBattleArt nativeArt)
        {
            var top=FindFirstObjectByType<GameplayPixelLayoutController>().TopHud;
            nativeArt.SkinControl((RectTransform)transform,"Wave",top);
            if(waveText) { waveText.rectTransform.offsetMin=waveText.rectTransform.offsetMax=Vector2.zero; waveText.enableAutoSizing=true; waveText.fontSizeMin=16; waveText.fontSizeMax=32; }
            return;
        }
        var plaque=BattlePresentationSkin.ZoneSprite("Plaque") ?? RunSession.Current?.Zone?.Definition?.theme?.wavePlaque;
        if(plaque!=null && TryGetComponent<UnityEngine.UI.Image>(out var image))
        {
            image.sprite=plaque;
            if (BattlePresentationSkin.Folder != null)
            {
                GameplayPixelGrid.FitImage(image, new Vector2(264,54));
                if (waveText != null)
                {
                    waveText.rectTransform.offsetMin = new Vector2(12,11);
                    waveText.rectTransform.offsetMax = new Vector2(-12,0);
                }
            }
            if(BattlePresentationSkin.UsesReferenceKit)
            {
                if(TryGetComponent<NativePixelUiSurface>(out var native))native.enabled=false;
                image.raycastTarget=false;
                gameObject.AddComponent<ReferenceKitSurface>().Initialize(BattlePresentationSkin.ReferenceWaveRect,new Vector2(470,BattlePresentationSkin.ReferenceTopHeight));
                if(waveText!=null)
                {
                    waveText.rectTransform.offsetMin=new Vector2(18,18);
                    waveText.rectTransform.offsetMax=new Vector2(-13,-9);
                    waveText.enableAutoSizing=true;waveText.fontSizeMin=16;waveText.fontSizeMax=28;
                }
                ApprovedDungeonBattleArt.Current?.WaveFrame(image);
            }
        }
    }

    private void OnDisable()
    {
        UnsubscribeFromWaveController();
    }

    private void LateUpdate()
    {
        // Continue restores an existing encounter without firing WaveStarted.
        // Read its depth for presentation without replaying gameplay events.
        if (waveController != null && displayedWave != waveController.CurrentWave)
            RefreshFromController();
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
        displayedWave = safeWaveNumber;
    }
}
