using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Owns the complete presentation-only game-over sequence:
/// lethal player hit -> white silhouette -> square-pixel burst -> dimmer ->
/// descending Game Over panel -> scene retry.
///
/// The component auto-installs on the scene PlayerActor so the Game scene does
/// not need hand-authored overlay objects or extra Inspector wiring.
/// </summary>
[DisallowMultipleComponent]
public sealed class GameOverPresentationController : MonoBehaviour
{
    private const string OverlayName =
        "RuntimeGameOverOverlay";

    private const string AffinityGemName =
        "PlayerAffinityGem";

    private const int BurstParticleCount = 18;

    private const float WhiteSilhouetteHold = 0.10f;
    private const float ExplosionReadTime = 0.12f;
    private const float DimDuration = 0.24f;
    private const float MenuDropDuration = 0.48f;
    private const float DimTargetAlpha = 0.72f;

    private const float PanelWidth = 300f;
    private const float PanelHeight = 178f;
    private const float PanelTopClearance = 24f;

    private const float ParticleMinimumSpeed = 72f;
    private const float ParticleMaximumSpeed = 150f;
    private const float ParticleGravity = 170f;
    private const float ParticleMinimumLifetime = 0.34f;
    private const float ParticleMaximumLifetime = 0.54f;

    private static readonly Color32 PanelOuterColor =
        new Color32(39, 19, 49, 255);

    private static readonly Color32 PanelAccentColor =
        new Color32(139, 65, 148, 255);

    private static readonly Color32 PanelFaceColor =
        new Color32(25, 20, 32, 255);

    private static readonly Color32 ButtonColor =
        new Color32(91, 40, 105, 255);

    private static readonly Color32 ButtonHighlightColor =
        new Color32(121, 55, 137, 255);

    private static readonly Color32 ButtonPressedColor =
        new Color32(68, 31, 78, 255);

    private sealed class BurstParticle
    {
        public RectTransform Rect;
        public Image Image;
        public Vector2 Velocity;
        public float Lifetime;
        public float Age;
    }

    private readonly List<BurstParticle>
        activeParticles = new();

    private PlayerActor playerActor;
    private PlayerCombatFeedback combatFeedback;
    private PlayerPanelUI playerPanel;
    private Canvas rootCanvas;

    private RectTransform overlayRect;
    private RectTransform particleLayer;
    private Image dimmerImage;
    private RectTransform gameOverPanel;
    private Button retryButton;

    private Coroutine sequenceCoroutine;
    private Coroutine particleCoroutine;

    private float previousTimeScale = 1f;
    private bool timeFrozen;
    private bool sequenceStarted;
    private bool retryRequested;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.AfterSceneLoad
    )]
    private static void InstallForGameScene()
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
            player.gameObject.AddComponent<
                GameOverPresentationController
            >();
        }
    }

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnEnable()
    {
        ResolveReferences();
        Subscribe();
    }

    private void Start()
    {
        if (playerActor != null &&
            playerActor.IsDefeated)
        {
            HandlePlayerDefeated(
                playerActor
            );
        }
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void ResolveReferences()
    {
        if (playerActor == null)
        {
            playerActor =
                GetComponent<PlayerActor>();
        }

        if (playerActor == null)
        {
            playerActor =
                Object.FindFirstObjectByType<
                    PlayerActor
                >();
        }

        if (combatFeedback == null)
        {
            combatFeedback =
                Object.FindFirstObjectByType<
                    PlayerCombatFeedback
                >();
        }

        if (playerPanel == null)
        {
            playerPanel =
                Object.FindFirstObjectByType<
                    PlayerPanelUI
                >();
        }

        if (rootCanvas == null &&
            combatFeedback != null &&
            combatFeedback.PlayerImage != null)
        {
            rootCanvas =
                combatFeedback.PlayerImage.canvas;
        }

        if (rootCanvas == null &&
            playerPanel != null)
        {
            rootCanvas =
                playerPanel.GetComponentInParent<Canvas>();
        }

        if (rootCanvas == null)
        {
            rootCanvas =
                Object.FindFirstObjectByType<Canvas>();
        }
    }

    private void Subscribe()
    {
        if (playerActor == null)
        {
            return;
        }

        playerActor.Defeated -=
            HandlePlayerDefeated;

        playerActor.Defeated +=
            HandlePlayerDefeated;
    }

    private void Unsubscribe()
    {
        if (playerActor == null)
        {
            return;
        }

        playerActor.Defeated -=
            HandlePlayerDefeated;
    }

    private void HandlePlayerDefeated(
        PlayerActor defeatedPlayer)
    {
        if (defeatedPlayer != playerActor ||
            sequenceStarted)
        {
            return;
        }

        ResolveReferences();

        if (rootCanvas == null)
        {
            Debug.LogError(
                "Game Over presentation requires a UI Canvas.",
                this
            );

            return;
        }

        sequenceStarted = true;
        FreezeGameplay();
        BuildOverlay();

        sequenceCoroutine =
            StartCoroutine(
                GameOverSequence()
            );
    }

    private IEnumerator GameOverSequence()
    {
        Image playerImage =
            combatFeedback != null
                ? combatFeedback.PlayerImage
                : null;

        if (combatFeedback != null)
        {
            combatFeedback.EnterDefeatedVisualState();
        }
        else
        {
            Debug.LogError(
                "Game Over presentation could not find PlayerCombatFeedback; " +
                "the menu will still appear, but the white silhouette flash " +
                "cannot be guaranteed.",
                this
            );
        }

        if (playerImage != null)
        {
            playerImage.enabled = true;
        }

        yield return
            new WaitForSecondsRealtime(
                WhiteSilhouetteHold
            );

        Rect playerBounds =
            GetPlayerBoundsInOverlay(
                playerImage
            );

        SpawnBurstParticles(
            playerBounds
        );

        if (playerImage != null)
        {
            playerImage.enabled = false;
        }

        HidePlayerAffinityGem();

        if (particleCoroutine != null)
        {
            StopCoroutine(
                particleCoroutine
            );
        }

        particleCoroutine =
            StartCoroutine(
                AnimateBurstParticles()
            );

        yield return
            new WaitForSecondsRealtime(
                ExplosionReadTime
            );

        yield return
            AnimateDimmerAndMenu();

        if (retryButton != null)
        {
            retryButton.interactable = true;
        }

        sequenceCoroutine = null;
    }

    private void FreezeGameplay()
    {
        if (timeFrozen)
        {
            return;
        }

        previousTimeScale =
            Time.timeScale;

        Time.timeScale = 0f;
        timeFrozen = true;
    }

    private void RestoreGameplayTime()
    {
        if (!timeFrozen)
        {
            return;
        }

        Time.timeScale =
            previousTimeScale > 0f
                ? previousTimeScale
                : 1f;

        timeFrozen = false;
    }

    private void BuildOverlay()
    {
        if (overlayRect != null)
        {
            overlayRect.SetAsLastSibling();
            return;
        }

        GameObject overlayObject =
            CreateUiObject(
                OverlayName,
                rootCanvas.transform,
                typeof(RectTransform)
            );

        overlayRect =
            overlayObject.GetComponent<RectTransform>();

        StretchToParent(
            overlayRect,
            0f
        );

        overlayRect.SetAsLastSibling();

        GameObject dimmerObject =
            CreateUiObject(
                "Dimmer",
                overlayRect,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image)
            );

        RectTransform dimmerRect =
            dimmerObject.GetComponent<RectTransform>();

        StretchToParent(
            dimmerRect,
            0f
        );

        dimmerImage =
            dimmerObject.GetComponent<Image>();

        dimmerImage.color =
            new Color(
                0f,
                0f,
                0f,
                0f
            );

        /*
         * The invisible dimmer begins blocking raycasts immediately on death,
         * preventing board/ability input while the death VFX is playing.
         */
        dimmerImage.raycastTarget = true;

        GameObject particlesObject =
            CreateUiObject(
                "DeathParticleLayer",
                overlayRect,
                typeof(RectTransform)
            );

        particleLayer =
            particlesObject.GetComponent<RectTransform>();

        StretchToParent(
            particleLayer,
            0f
        );

        CreateGameOverPanel();
    }

    private void CreateGameOverPanel()
    {
        GameObject panelObject =
            CreateUiObject(
                "GameOverPanel",
                overlayRect,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image)
            );

        gameOverPanel =
            panelObject.GetComponent<RectTransform>();

        gameOverPanel.anchorMin =
            new Vector2(0.5f, 0.5f);
        gameOverPanel.anchorMax =
            new Vector2(0.5f, 0.5f);
        gameOverPanel.pivot =
            new Vector2(0.5f, 0.5f);
        gameOverPanel.sizeDelta =
            new Vector2(
                PanelWidth,
                PanelHeight
            );

        panelObject.GetComponent<Image>().color =
            PanelOuterColor;

        RectTransform accentRect =
            CreateInsetPanel(
                "AccentBorder",
                gameOverPanel,
                PanelAccentColor,
                4f
            );

        RectTransform faceRect =
            CreateInsetPanel(
                "PanelFace",
                accentRect,
                PanelFaceColor,
                4f
            );

        TMP_FontAsset font =
            ResolveUiFont();

        CreateLabel(
            "GameOverTitle",
            faceRect,
            "GAME OVER",
            font,
            34f,
            new Vector2(0f, 37f),
            new Vector2(250f, 54f)
        );

        retryButton =
            CreateRetryButton(
                faceRect,
                font
            );

        retryButton.interactable = false;
        retryButton.onClick.AddListener(
            RetryCurrentGame
        );

        float canvasHeight =
            rootCanvas.transform is RectTransform canvasRect
                ? canvasRect.rect.height
                : 960f;

        float startY =
            Mathf.Max(
                PanelHeight,
                canvasHeight * 0.5f +
                PanelHeight * 0.5f +
                PanelTopClearance
            );

        gameOverPanel.anchoredPosition =
            new Vector2(
                0f,
                Mathf.Round(startY)
            );
    }

    private RectTransform CreateInsetPanel(
        string objectName,
        RectTransform parent,
        Color color,
        float inset)
    {
        GameObject panelObject =
            CreateUiObject(
                objectName,
                parent,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image)
            );

        RectTransform rect =
            panelObject.GetComponent<RectTransform>();

        StretchToParent(
            rect,
            inset
        );

        Image image =
            panelObject.GetComponent<Image>();

        image.color = color;
        image.raycastTarget = false;

        return rect;
    }

    private Button CreateRetryButton(
        RectTransform parent,
        TMP_FontAsset font)
    {
        GameObject buttonObject =
            CreateUiObject(
                "RetryButton",
                parent,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Button)
            );

        RectTransform buttonRect =
            buttonObject.GetComponent<RectTransform>();

        buttonRect.anchorMin =
            new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax =
            new Vector2(0.5f, 0.5f);
        buttonRect.pivot =
            new Vector2(0.5f, 0.5f);
        buttonRect.sizeDelta =
            new Vector2(158f, 50f);
        buttonRect.anchoredPosition =
            new Vector2(0f, -43f);

        Image buttonImage =
            buttonObject.GetComponent<Image>();

        buttonImage.color =
            ButtonColor;

        Button button =
            buttonObject.GetComponent<Button>();

        button.targetGraphic =
            buttonImage;

        ColorBlock colors =
            button.colors;

        colors.normalColor =
            ButtonColor;
        colors.highlightedColor =
            ButtonHighlightColor;
        colors.selectedColor =
            ButtonHighlightColor;
        colors.pressedColor =
            ButtonPressedColor;
        colors.disabledColor =
            new Color32(58, 42, 62, 255);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.05f;

        button.colors = colors;

        Navigation navigation =
            button.navigation;

        navigation.mode =
            Navigation.Mode.None;

        button.navigation = navigation;

        RectTransform innerFace =
            CreateInsetPanel(
                "RetryFace",
                buttonRect,
                PanelFaceColor,
                4f
            );

        CreateLabel(
            "RetryLabel",
            innerFace,
            "RETRY",
            font,
            20f,
            Vector2.zero,
            new Vector2(140f, 38f)
        );

        return button;
    }

    private TMP_Text CreateLabel(
        string objectName,
        RectTransform parent,
        string text,
        TMP_FontAsset font,
        float fontSize,
        Vector2 anchoredPosition,
        Vector2 size)
    {
        GameObject labelObject =
            CreateUiObject(
                objectName,
                parent,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(TextMeshProUGUI)
            );

        RectTransform labelRect =
            labelObject.GetComponent<RectTransform>();

        labelRect.anchorMin =
            new Vector2(0.5f, 0.5f);
        labelRect.anchorMax =
            new Vector2(0.5f, 0.5f);
        labelRect.pivot =
            new Vector2(0.5f, 0.5f);
        labelRect.anchoredPosition =
            anchoredPosition;
        labelRect.sizeDelta =
            size;

        TextMeshProUGUI label =
            labelObject.GetComponent<TextMeshProUGUI>();

        label.text = text;
        label.fontSize = fontSize;
        label.fontStyle = FontStyles.Bold;
        label.alignment =
            TextAlignmentOptions.Center;
        label.color = Color.white;
        label.raycastTarget = false;
        label.overflowMode =
            TextOverflowModes.Overflow;

        if (font != null)
        {
            label.font = font;
        }

        return label;
    }

    private TMP_FontAsset ResolveUiFont()
    {
        TMP_Text[] existingLabels =
            Object.FindObjectsByType<TMP_Text>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (TMP_Text label in
                 existingLabels)
        {
            if (label != null &&
                label.font != null)
            {
                return label.font;
            }
        }

        return null;
    }

    private Rect GetPlayerBoundsInOverlay(
        Image playerImage)
    {
        if (playerImage == null ||
            overlayRect == null)
        {
            return Rect.MinMaxRect(
                -18f,
                -24f,
                18f,
                24f
            );
        }

        Vector3[] worldCorners =
            new Vector3[4];

        playerImage.rectTransform.GetWorldCorners(
            worldCorners
        );

        Vector3 firstLocal =
            overlayRect.InverseTransformPoint(
                worldCorners[0]
            );

        float minimumX = firstLocal.x;
        float maximumX = firstLocal.x;
        float minimumY = firstLocal.y;
        float maximumY = firstLocal.y;

        for (int index = 1;
             index < worldCorners.Length;
             index++)
        {
            Vector3 local =
                overlayRect.InverseTransformPoint(
                    worldCorners[index]
                );

            minimumX =
                Mathf.Min(
                    minimumX,
                    local.x
                );

            maximumX =
                Mathf.Max(
                    maximumX,
                    local.x
                );

            minimumY =
                Mathf.Min(
                    minimumY,
                    local.y
                );

            maximumY =
                Mathf.Max(
                    maximumY,
                    local.y
                );
        }

        return Rect.MinMaxRect(
            minimumX,
            minimumY,
            maximumX,
            maximumY
        );
    }

    private void SpawnBurstParticles(
        Rect playerBounds)
    {
        if (particleLayer == null)
        {
            return;
        }

        ClearBurstParticles();

        Vector2 center =
            playerBounds.center;

        for (int index = 0;
             index < BurstParticleCount;
             index++)
        {
            GameObject particleObject =
                CreateUiObject(
                    $"PlayerDeathPixel_{index:00}",
                    particleLayer,
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image)
                );

            RectTransform rect =
                particleObject.GetComponent<RectTransform>();

            rect.anchorMin =
                new Vector2(0.5f, 0.5f);
            rect.anchorMax =
                new Vector2(0.5f, 0.5f);
            rect.pivot =
                new Vector2(0.5f, 0.5f);

            float particleSize =
                Random.Range(0, 3) switch
                {
                    0 => 3f,
                    1 => 4f,
                    _ => 5f
                };

            rect.sizeDelta =
                Vector2.one * particleSize;

            Vector2 spawnPosition =
                new Vector2(
                    Random.Range(
                        playerBounds.xMin,
                        playerBounds.xMax
                    ),
                    Random.Range(
                        playerBounds.yMin,
                        playerBounds.yMax
                    )
                );

            rect.anchoredPosition =
                RoundVector(
                    spawnPosition
                );

            Image image =
                particleObject.GetComponent<Image>();

            image.color = Color.white;
            image.raycastTarget = false;

            Vector2 direction =
                spawnPosition - center;

            if (direction.sqrMagnitude < 0.01f)
            {
                direction =
                    Random.insideUnitCircle;
            }

            direction =
                (direction.normalized +
                 Random.insideUnitCircle * 0.35f)
                .normalized;

            if (direction.sqrMagnitude < 0.01f)
            {
                direction = Vector2.up;
            }

            float speed =
                Random.Range(
                    ParticleMinimumSpeed,
                    ParticleMaximumSpeed
                );

            activeParticles.Add(
                new BurstParticle
                {
                    Rect = rect,
                    Image = image,
                    Velocity = direction * speed,
                    Lifetime = Random.Range(
                        ParticleMinimumLifetime,
                        ParticleMaximumLifetime
                    ),
                    Age = 0f
                }
            );
        }
    }

    private IEnumerator AnimateBurstParticles()
    {
        while (activeParticles.Count > 0)
        {
            float deltaTime =
                Time.unscaledDeltaTime;

            for (int index =
                     activeParticles.Count - 1;
                 index >= 0;
                 index--)
            {
                BurstParticle particle =
                    activeParticles[index];

                if (particle == null ||
                    particle.Rect == null ||
                    particle.Image == null)
                {
                    activeParticles.RemoveAt(index);
                    continue;
                }

                particle.Age += deltaTime;

                if (particle.Age >=
                    particle.Lifetime)
                {
                    Destroy(
                        particle.Rect.gameObject
                    );

                    activeParticles.RemoveAt(index);
                    continue;
                }

                particle.Velocity +=
                    Vector2.down *
                    ParticleGravity *
                    deltaTime;

                Vector2 position =
                    particle.Rect.anchoredPosition +
                    particle.Velocity *
                    deltaTime;

                particle.Rect.anchoredPosition =
                    RoundVector(
                        position
                    );

                float normalizedAge =
                    Mathf.Clamp01(
                        particle.Age /
                        particle.Lifetime
                    );

                float alpha =
                    normalizedAge < 0.40f
                        ? 1f
                        : 1f -
                          Mathf.InverseLerp(
                              0.40f,
                              1f,
                              normalizedAge
                          );

                Color color =
                    particle.Image.color;

                color.a = alpha;
                particle.Image.color = color;
            }

            yield return null;
        }

        particleCoroutine = null;
    }

    private IEnumerator AnimateDimmerAndMenu()
    {
        if (dimmerImage == null ||
            gameOverPanel == null)
        {
            yield break;
        }

        float startY =
            gameOverPanel.anchoredPosition.y;

        float elapsed = 0f;
        float duration =
            Mathf.Max(
                DimDuration,
                MenuDropDuration
            );

        while (elapsed < duration)
        {
            elapsed +=
                Time.unscaledDeltaTime;

            float dimProgress =
                Mathf.Clamp01(
                    elapsed /
                    DimDuration
                );

            float dimEase =
                dimProgress *
                dimProgress *
                (3f - 2f * dimProgress);

            dimmerImage.color =
                new Color(
                    0f,
                    0f,
                    0f,
                    DimTargetAlpha *
                    dimEase
                );

            float menuProgress =
                Mathf.Clamp01(
                    elapsed /
                    MenuDropDuration
                );

            float menuEase =
                EaseOutBack(
                    menuProgress
                );

            float y =
                Mathf.LerpUnclamped(
                    startY,
                    0f,
                    menuEase
                );

            gameOverPanel.anchoredPosition =
                new Vector2(
                    0f,
                    Mathf.Round(y)
                );

            yield return null;
        }

        dimmerImage.color =
            new Color(
                0f,
                0f,
                0f,
                DimTargetAlpha
            );

        gameOverPanel.anchoredPosition =
            Vector2.zero;
    }

    private void HidePlayerAffinityGem()
    {
        if (playerPanel == null)
        {
            return;
        }

        Transform affinityGem =
            playerPanel.transform.Find(
                AffinityGemName
            );

        if (affinityGem != null)
        {
            affinityGem.gameObject.SetActive(
                false
            );
        }
    }

    private void RetryCurrentGame()
    {
        if (retryRequested)
        {
            return;
        }

        retryRequested = true;

        if (retryButton != null)
        {
            retryButton.interactable = false;
        }

        RestoreGameplayTime();

        Scene activeScene =
            SceneManager.GetActiveScene();

        if (!activeScene.IsValid() ||
            string.IsNullOrWhiteSpace(
                activeScene.name
            ))
        {
            Debug.LogError(
                "Cannot retry because the active scene is invalid.",
                this
            );

            retryRequested = false;
            return;
        }

        SceneManager.LoadScene(
            activeScene.name,
            LoadSceneMode.Single
        );
    }

    private static float EaseOutBack(
        float progress)
    {
        progress =
            Mathf.Clamp01(progress);

        const float overshoot = 1.70158f;
        const float overshootPlusOne =
            overshoot + 1f;

        float shifted =
            progress - 1f;

        return 1f +
               overshootPlusOne *
               shifted *
               shifted *
               shifted +
               overshoot *
               shifted *
               shifted;
    }

    private GameObject CreateUiObject(
        string objectName,
        Transform parent,
        params System.Type[] componentTypes)
    {
        GameObject uiObject =
            new GameObject(
                objectName,
                componentTypes
            );

        uiObject.layer =
            rootCanvas != null
                ? rootCanvas.gameObject.layer
                : gameObject.layer;

        uiObject.transform.SetParent(
            parent,
            false
        );

        return uiObject;
    }

    private static void StretchToParent(
        RectTransform rect,
        float inset)
    {
        if (rect == null)
        {
            return;
        }

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot =
            new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.offsetMin =
            new Vector2(inset, inset);
        rect.offsetMax =
            new Vector2(-inset, -inset);
        rect.localScale = Vector3.one;
    }

    private static Vector2 RoundVector(
        Vector2 value)
    {
        return new Vector2(
            Mathf.Round(value.x),
            Mathf.Round(value.y)
        );
    }

    private void ClearBurstParticles()
    {
        for (int index =
                 activeParticles.Count - 1;
             index >= 0;
             index--)
        {
            BurstParticle particle =
                activeParticles[index];

            if (particle != null &&
                particle.Rect != null)
            {
                Destroy(
                    particle.Rect.gameObject
                );
            }
        }

        activeParticles.Clear();
    }

    private void OnDestroy()
    {
        Unsubscribe();

        if (retryButton != null)
        {
            retryButton.onClick.RemoveListener(
                RetryCurrentGame
            );
        }

        RestoreGameplayTime();
        ClearBurstParticles();
    }
}