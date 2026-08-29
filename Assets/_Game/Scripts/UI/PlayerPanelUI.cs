using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class PlayerPanelUI : MonoBehaviour
{
    private const string GemIndicatorConfigPath =
        "UI/EnemyWeaknessIndicatorConfig";

    private const string TopBattlePresentationProfilePath =
        "UI/TopBattlePresentationProfile";

    private const float FallbackAffinityGapAboveCharacter = 8f;
    private const float FallbackAffinityIconSize = 16f;
    private const float HealthTextMaximumFontSize = 10f;
    private const float HealthTextMinimumFontSize = 6f;
    private const float ShieldBreakFlashDuration = 0.1f;

    private static readonly Color ShieldFillColor =
        new Color32(39, 124, 255, 255);

    private static Sprite shieldSolidSprite;

    [Header("Runtime Player")]
    [SerializeField]
    private PlayerActor playerActor;

    [Header("Character Presentation")]
    [SerializeField]
    private Image playerFrame;

    [SerializeField]
    private Image playerCharacter;

    [SerializeField]
    [Tooltip(
        "Legacy colored player affinity circle. It is retained only so old " +
        "scene references migrate safely and is disabled at runtime."
    )]
    private Image playerBase;

    [Header("Legacy Shared Visual Data")]
    [SerializeField]
    [Tooltip(
        "Legacy palette used by the retired colored player base. The player " +
        "affinity is now shown with the matching 16x16 gem sprite."
    )]
    private GemColorPalette gemColorPalette;

    [Header("Health Bar")]
    [SerializeField]
    private GameObject playerHealthBar;

    [SerializeField]
    private Image playerHealthFill;

    [SerializeField]
    private TMP_Text playerHealthText;

    private PlayerActor boundPlayer;
    private EnemyWeaknessIndicatorConfig gemIndicatorConfig;
    private TopBattlePresentationProfile presentationProfile;
    private RectTransform affinityGemRect;
    private Image affinityGemImage;

    private GameObject playerShieldBar;
    private Image playerShieldFill;
    private TMP_Text playerShieldText;
    private Coroutine shieldBreakRoutine;

    public PlayerActor BoundPlayer =>
        boundPlayer;

    private void Awake()
    {
        gemIndicatorConfig =
            Resources.Load<EnemyWeaknessIndicatorConfig>(
                GemIndicatorConfigPath
            );

        presentationProfile =
            Resources.Load<TopBattlePresentationProfile>(
                TopBattlePresentationProfilePath
            );

        ApplyHealthTextPresentation();
        DisableLegacyPlayerBase();
        CreateAffinityGemIndicator();
    }

    private void OnEnable()
    {
        ApplyHealthTextPresentation();
        DisableLegacyPlayerBase();
        CreateAffinityGemIndicator();
        BindPlayer(playerActor);
    }

    private void Start()
    {
        RefreshAll();
    }

    private void LateUpdate()
    {
        DisableLegacyPlayerBase();
        FollowPlayerCharacter();
    }

    private void OnDisable()
    {
        UnsubscribeFromPlayer(boundPlayer);
        boundPlayer = null;

        StopShieldBreakRoutine();
        DestroyShieldBarImmediately();
    }

    public void BindPlayer(
        PlayerActor newPlayer)
    {
        if (boundPlayer == newPlayer)
        {
            RefreshAll();
            return;
        }

        UnsubscribeFromPlayer(boundPlayer);

        boundPlayer = newPlayer;
        playerActor = newPlayer;

        SubscribeToPlayer(boundPlayer);
        RefreshAll();
    }

    private void SubscribeToPlayer(
        PlayerActor player)
    {
        if (player == null)
        {
            return;
        }

        player.Initialized +=
            HandlePlayerInitialized;

        player.HealthChanged +=
            HandleHealthChanged;

        player.MaximumHealthChanged +=
            HandleMaximumHealthChanged;

        player.ShieldChanged +=
            HandleShieldChanged;

        player.Defeated +=
            HandlePlayerDefeated;

        player.Revived +=
            HandlePlayerRevived;
    }

    private void UnsubscribeFromPlayer(
        PlayerActor player)
    {
        if (player == null)
        {
            return;
        }

        player.Initialized -=
            HandlePlayerInitialized;

        player.HealthChanged -=
            HandleHealthChanged;

        player.MaximumHealthChanged -=
            HandleMaximumHealthChanged;

        player.ShieldChanged -=
            HandleShieldChanged;

        player.Defeated -=
            HandlePlayerDefeated;

        player.Revived -=
            HandlePlayerRevived;
    }

    private void HandlePlayerInitialized(
        PlayerActor player)
    {
        if (player != boundPlayer)
        {
            return;
        }

        ApplyDefinition(
            player.Definition
        );

        UpdateHealthDisplay(
            player.CurrentHealth,
            player.MaximumHealth
        );

        UpdateShieldDisplay(
            player.CurrentShield,
            player.MaximumShield
        );
    }

    private void HandleHealthChanged(
        PlayerActor player,
        int currentHealth,
        int maximumHealth)
    {
        if (player != boundPlayer)
        {
            return;
        }

        UpdateHealthDisplay(
            currentHealth,
            maximumHealth
        );
    }

    private void HandleMaximumHealthChanged(
        PlayerActor player,
        int previousMaximumHealth,
        int newMaximumHealth)
    {
        if (player != boundPlayer)
        {
            return;
        }

        UpdateHealthDisplay(
            player.CurrentHealth,
            newMaximumHealth
        );
    }

    private void HandleShieldChanged(
        PlayerActor player,
        int currentShield,
        int maximumShield)
    {
        if (player != boundPlayer)
        {
            return;
        }

        UpdateShieldDisplay(
            currentShield,
            maximumShield
        );
    }

    private void HandlePlayerDefeated(
        PlayerActor player)
    {
        if (player != boundPlayer)
        {
            return;
        }

        UpdateHealthDisplay(
            0,
            player.MaximumHealth
        );

        UpdateShieldDisplay(
            0,
            player.MaximumShield
        );
    }

    private void HandlePlayerRevived(
        PlayerActor player,
        int revivalCount)
    {
        if (player != boundPlayer)
        {
            return;
        }

        UpdateHealthDisplay(
            player.CurrentHealth,
            player.MaximumHealth
        );

        UpdateShieldDisplay(
            player.CurrentShield,
            player.MaximumShield
        );
    }

    private void RefreshAll()
    {
        DisableLegacyPlayerBase();

        if (boundPlayer == null ||
            !boundPlayer.IsInitialized)
        {
            ShowUninitializedState();
            return;
        }

        ApplyDefinition(
            boundPlayer.Definition
        );

        UpdateHealthDisplay(
            boundPlayer.CurrentHealth,
            boundPlayer.MaximumHealth
        );

        UpdateShieldDisplay(
            boundPlayer.CurrentShield,
            boundPlayer.MaximumShield
        );
    }

    private void ApplyDefinition(
        PlayerDefinition definition)
    {
        if (definition == null)
        {
            ShowUninitializedState();
            return;
        }

        SetImageSprite(
            playerFrame,
            definition.BattleFrameSprite,
            preserveAspect: true
        );

        SetImageSprite(
            playerCharacter,
            definition.BattleCharacterSprite,
            preserveAspect: true
        );

        DisableLegacyPlayerBase();

        ShowAffinityGem(
            definition.AffinityGemType
        );

        if (playerHealthBar != null)
        {
            playerHealthBar.SetActive(true);
        }
    }

    private void UpdateHealthDisplay(
        int currentHealth,
        int maximumHealth)
    {
        maximumHealth =
            Mathf.Max(1, maximumHealth);

        currentHealth =
            Mathf.Clamp(
                currentHealth,
                0,
                maximumHealth
            );

        if (playerHealthBar != null)
        {
            playerHealthBar.SetActive(true);
        }

        if (playerHealthFill != null)
        {
            playerHealthFill.fillAmount =
                (float)currentHealth /
                maximumHealth;
        }

        if (playerHealthText != null)
        {
            playerHealthText.text =
                $"{currentHealth} / {maximumHealth}";

            playerHealthText.enabled =
                !ShouldHideHealthText();
        }
    }

    private void UpdateShieldDisplay(
        int currentShield,
        int maximumShield)
    {
        maximumShield =
            Mathf.Max(1, maximumShield);

        currentShield =
            Mathf.Clamp(
                currentShield,
                0,
                maximumShield
            );

        if (currentShield > 0)
        {
            StopShieldBreakRoutine();
            CreateShieldBar();

            if (playerHealthFill != null)
            {
                playerHealthFill.enabled = false;
            }

            if (playerHealthText != null)
            {
                playerHealthText.enabled = false;
            }

            if (playerShieldBar != null)
            {
                playerShieldBar.SetActive(true);
            }

            if (playerShieldFill != null)
            {
                playerShieldFill.sprite =
                    GetShieldSolidSprite();

                playerShieldFill.material = null;
                playerShieldFill.color =
                    ShieldFillColor;

                playerShieldFill.fillAmount =
                    (float)currentShield /
                    maximumShield;
            }

            if (playerShieldText != null)
            {
                playerShieldText.enabled = true;
                playerShieldText.text =
                    $"{currentShield} / {maximumShield}";
            }

            return;
        }

        if (playerShieldBar != null &&
            playerShieldBar.activeSelf)
        {
            if (shieldBreakRoutine == null)
            {
                shieldBreakRoutine =
                    StartCoroutine(
                        PlayShieldBreakAndDestroy()
                    );
            }

            return;
        }

        DestroyShieldBarImmediately();
        RestoreHealthBarPresentation();
    }

    private void CreateShieldBar()
    {
        if (playerShieldBar != null ||
            playerHealthBar == null ||
            playerHealthFill == null ||
            playerHealthText == null)
        {
            return;
        }

        GameObject shieldRoot =
            new GameObject(
                "PlayerShieldBar",
                typeof(RectTransform)
            );

        shieldRoot.layer =
            playerHealthBar.layer;

        shieldRoot.transform.SetParent(
            playerHealthBar.transform,
            false
        );

        RectTransform shieldRootRect =
            shieldRoot.transform as RectTransform;

        if (shieldRootRect == null)
        {
            Destroy(shieldRoot);
            return;
        }

        shieldRootRect.anchorMin =
            Vector2.zero;

        shieldRootRect.anchorMax =
            Vector2.one;

        shieldRootRect.pivot =
            new Vector2(0.5f, 0.5f);

        shieldRootRect.anchoredPosition =
            Vector2.zero;

        shieldRootRect.sizeDelta =
            Vector2.zero;

        shieldRootRect.localScale =
            Vector3.one;

        GameObject shieldFillObject =
            Instantiate(
                playerHealthFill.gameObject,
                shieldRoot.transform,
                false
            );

        shieldFillObject.name =
            "PlayerShieldBarFill";

        GameObject shieldTextObject =
            Instantiate(
                playerHealthText.gameObject,
                shieldRoot.transform,
                false
            );

        shieldTextObject.name =
            "PlayerShieldBarText";

        playerShieldBar =
            shieldRoot;

        playerShieldFill =
            shieldFillObject.GetComponent<Image>();

        playerShieldText =
            shieldTextObject.GetComponent<TMP_Text>();

        if (playerShieldFill == null ||
            playerShieldText == null)
        {
            Debug.LogError(
                "Player shield overlay could not clone the HP fill/text presentation.",
                this
            );

            DestroyShieldBarImmediately();
            return;
        }

        playerShieldFill.sprite =
            GetShieldSolidSprite();

        playerShieldFill.material = null;
        playerShieldFill.color =
            ShieldFillColor;

        playerShieldFill.type =
            Image.Type.Filled;

        playerShieldFill.fillMethod =
            Image.FillMethod.Horizontal;

        playerShieldFill.fillOrigin =
            (int)Image.OriginHorizontal.Left;

        playerShieldFill.fillClockwise = true;
        playerShieldFill.raycastTarget = false;
        playerShieldFill.enabled = true;

        playerShieldText.enableAutoSizing = true;
        playerShieldText.fontSize = HealthTextMaximumFontSize;
        playerShieldText.fontSizeMin = HealthTextMinimumFontSize;
        playerShieldText.fontSizeMax = HealthTextMaximumFontSize;
        playerShieldText.alignment =
            TextAlignmentOptions.Center;
        playerShieldText.raycastTarget = false;
        playerShieldText.enabled = true;

        shieldRoot.transform.SetAsLastSibling();
    }

    private IEnumerator PlayShieldBreakAndDestroy()
    {
        if (playerShieldBar == null)
        {
            shieldBreakRoutine = null;
            RestoreHealthBarPresentation();
            yield break;
        }

        if (playerHealthFill != null)
        {
            playerHealthFill.enabled = false;
        }

        if (playerHealthText != null)
        {
            playerHealthText.enabled = false;
        }

        if (playerShieldFill != null)
        {
            playerShieldFill.sprite =
                GetShieldSolidSprite();

            playerShieldFill.material = null;
            playerShieldFill.fillAmount = 1f;
            playerShieldFill.color = Color.white;
            playerShieldFill.enabled = true;
        }

        if (playerShieldText != null)
        {
            playerShieldText.text =
                string.Empty;

            playerShieldText.enabled = false;
        }

        yield return new WaitForSecondsRealtime(
            ShieldBreakFlashDuration
        );

        shieldBreakRoutine = null;
        DestroyShieldBarImmediately();
        RestoreHealthBarPresentation();
    }

    private void StopShieldBreakRoutine()
    {
        if (shieldBreakRoutine == null)
        {
            return;
        }

        StopCoroutine(
            shieldBreakRoutine
        );

        shieldBreakRoutine = null;
    }

    private void DestroyShieldBarImmediately()
    {
        if (playerShieldBar != null)
        {
            Destroy(
                playerShieldBar
            );
        }

        playerShieldBar = null;
        playerShieldFill = null;
        playerShieldText = null;
    }

    private void RestoreHealthBarPresentation()
    {
        if (playerHealthFill != null)
        {
            playerHealthFill.enabled = true;
        }

        if (playerHealthText != null)
        {
            playerHealthText.enabled = true;
        }
    }

    private bool ShouldHideHealthText()
    {
        return shieldBreakRoutine != null ||
               (
                   boundPlayer != null &&
                   boundPlayer.HasShield
               );
    }

    private static Sprite GetShieldSolidSprite()
    {
        if (shieldSolidSprite != null)
        {
            return shieldSolidSprite;
        }

        Texture2D texture =
            new Texture2D(
                1,
                1,
                TextureFormat.RGBA32,
                false
            );

        texture.name =
            "ShieldBarSolidTexture";

        texture.filterMode =
            FilterMode.Point;

        texture.wrapMode =
            TextureWrapMode.Clamp;

        texture.SetPixel(
            0,
            0,
            Color.white
        );

        texture.Apply(
            false,
            true
        );

        texture.hideFlags =
            HideFlags.HideAndDontSave;

        shieldSolidSprite =
            Sprite.Create(
                texture,
                new Rect(
                    0f,
                    0f,
                    1f,
                    1f
                ),
                new Vector2(0.5f, 0.5f),
                1f
            );

        shieldSolidSprite.name =
            "ShieldBarSolidSprite";

        shieldSolidSprite.hideFlags =
            HideFlags.HideAndDontSave;

        return shieldSolidSprite;
    }

    private void ShowUninitializedState()
    {
        DisableLegacyPlayerBase();
        StopShieldBreakRoutine();
        DestroyShieldBarImmediately();

        if (affinityGemImage != null)
        {
            affinityGemImage.enabled = false;
        }

        if (playerHealthBar != null)
        {
            playerHealthBar.SetActive(false);
        }

        if (playerHealthFill != null)
        {
            playerHealthFill.enabled = true;
            playerHealthFill.fillAmount = 0f;
        }

        if (playerHealthText != null)
        {
            playerHealthText.text =
                string.Empty;

            playerHealthText.enabled = true;
        }
    }

    private void ApplyHealthTextPresentation()
    {
        if (playerHealthText == null)
        {
            return;
        }

        playerHealthText.enableAutoSizing = true;
        playerHealthText.fontSize = HealthTextMaximumFontSize;
        playerHealthText.fontSizeMin = HealthTextMinimumFontSize;
        playerHealthText.fontSizeMax = HealthTextMaximumFontSize;
        playerHealthText.alignment =
            TextAlignmentOptions.Center;
        playerHealthText.raycastTarget = false;

        if (playerHealthText.transform is not RectTransform textRect)
        {
            return;
        }

        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.pivot =
            new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = Vector2.zero;
        textRect.sizeDelta = Vector2.zero;
        textRect.localScale = Vector3.one;
    }

    private void CreateAffinityGemIndicator()
    {
        if (affinityGemRect != null ||
            affinityGemImage != null)
        {
            return;
        }

        RectTransform panelRect =
            transform as RectTransform;

        if (panelRect == null)
        {
            return;
        }

        if (gemIndicatorConfig == null)
        {
            gemIndicatorConfig =
                Resources.Load<EnemyWeaknessIndicatorConfig>(
                    GemIndicatorConfigPath
                );
        }

        if (presentationProfile == null)
        {
            presentationProfile =
                Resources.Load<TopBattlePresentationProfile>(
                    TopBattlePresentationProfilePath
                );
        }

        if (gemIndicatorConfig == null)
        {
            Debug.LogError(
                $"Could not load {GemIndicatorConfigPath}; player affinity gem " +
                "cannot be displayed.",
                this
            );

            return;
        }

        GameObject indicatorObject =
            new GameObject(
                "PlayerAffinityGem",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image)
            );

        indicatorObject.layer =
            gameObject.layer;

        indicatorObject.transform.SetParent(
            transform,
            false
        );

        indicatorObject.transform.SetAsLastSibling();

        affinityGemRect =
            indicatorObject.transform as RectTransform;

        affinityGemImage =
            indicatorObject.GetComponent<Image>();

        affinityGemRect.anchorMin =
            new Vector2(0.5f, 0.5f);

        affinityGemRect.anchorMax =
            new Vector2(0.5f, 0.5f);

        affinityGemRect.pivot =
            new Vector2(0.5f, 0.5f);

        float iconSize =
            gemIndicatorConfig != null
                ? gemIndicatorConfig.IconSize
                : FallbackAffinityIconSize;

        affinityGemRect.sizeDelta =
            Vector2.one * iconSize;

        affinityGemImage.raycastTarget = false;
        affinityGemImage.preserveAspect = true;
        affinityGemImage.enabled = false;
    }

    private void ShowAffinityGem(
        GemType affinityGemType)
    {
        CreateAffinityGemIndicator();

        if (affinityGemImage == null ||
            gemIndicatorConfig == null)
        {
            return;
        }

        Sprite affinitySprite =
            gemIndicatorConfig.GetSprite(
                affinityGemType
            );

        affinityGemImage.sprite =
            affinitySprite;

        affinityGemImage.enabled =
            affinitySprite != null;

        FollowPlayerCharacter();
    }

    private void FollowPlayerCharacter()
    {
        if (affinityGemRect == null ||
            affinityGemImage == null ||
            !affinityGemImage.enabled ||
            playerCharacter == null)
        {
            return;
        }

        RectTransform characterRect =
            playerCharacter.rectTransform;

        RectTransform panelRect =
            transform as RectTransform;

        if (characterRect == null ||
            panelRect == null)
        {
            return;
        }

        Vector3 characterTopCenterWorld =
            characterRect.TransformPoint(
                new Vector3(
                    characterRect.rect.center.x,
                    characterRect.rect.yMax,
                    0f
                )
            );

        Vector3 panelLocalPosition =
            panelRect.InverseTransformPoint(
                characterTopCenterWorld
            );

        float iconSize =
            gemIndicatorConfig != null
                ? gemIndicatorConfig.IconSize
                : FallbackAffinityIconSize;

        float gap =
            presentationProfile != null
                ? presentationProfile.PlayerAffinityGapAboveCharacter
                : FallbackAffinityGapAboveCharacter;

        affinityGemRect.localPosition =
            new Vector3(
                panelLocalPosition.x,
                panelLocalPosition.y +
                gap +
                iconSize * 0.5f,
                0f
            );
    }

    private void DisableLegacyPlayerBase()
    {
        if (playerBase == null)
        {
            return;
        }

        playerBase.gameObject.SetActive(false);
    }

    private static void SetImageSprite(
        Image image,
        Sprite sprite,
        bool preserveAspect)
    {
        if (image == null)
        {
            return;
        }

        image.sprite = sprite;
        image.preserveAspect = preserveAspect;
        image.enabled = sprite != null;
    }

    private void OnValidate()
    {
        if (playerHealthFill != null)
        {
            playerHealthFill.type =
                Image.Type.Filled;

            playerHealthFill.fillMethod =
                Image.FillMethod.Horizontal;

            playerHealthFill.fillOrigin =
                (int)Image.OriginHorizontal.Left;
        }

        ApplyHealthTextPresentation();
    }
}
