using System.Collections;
using System.Collections.Generic;
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
        new Color32(49, 126, 230, 255);

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
        PositionShieldBar();
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

            if (playerShieldBar != null)
            {
                playerShieldBar.SetActive(true);
            }

            if (playerShieldFill != null)
            {
                playerShieldFill.sprite = null;
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

            if (playerHealthText != null)
            {
                playerHealthText.enabled = false;
            }

            PositionShieldBar();
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

        if (playerHealthText != null)
        {
            playerHealthText.enabled = true;
        }
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

        string fillPath =
            GetRelativePath(
                playerHealthBar.transform,
                playerHealthFill.transform
            );

        string textPath =
            GetRelativePath(
                playerHealthBar.transform,
                playerHealthText.transform
            );

        if (fillPath == null ||
            textPath == null)
        {
            Debug.LogError(
                "Player shield bar could not mirror the health bar because " +
                "its fill or text is not a child of the health bar root.",
                this
            );

            return;
        }

        playerShieldBar =
            Instantiate(
                playerHealthBar,
                playerHealthBar.transform.parent
            );

        playerShieldBar.name =
            "PlayerShieldBar";

        Transform shieldFillTransform =
            playerShieldBar.transform.Find(
                fillPath
            );

        Transform shieldTextTransform =
            playerShieldBar.transform.Find(
                textPath
            );

        playerShieldFill =
            shieldFillTransform != null
                ? shieldFillTransform.GetComponent<Image>()
                : null;

        playerShieldText =
            shieldTextTransform != null
                ? shieldTextTransform.GetComponent<TMP_Text>()
                : null;

        if (playerShieldFill == null ||
            playerShieldText == null)
        {
            Debug.LogError(
                "Player shield bar clone is missing its expected fill or text.",
                this
            );

            DestroyShieldBarImmediately();
            return;
        }

        /*
         * The HP fill sprite contains red artwork, so tinting that sprite blue
         * still leaves red/magenta pixels visible. Shield uses the exact same
         * cloned frame and fill RectTransform, but a sprite-free solid fill.
         */
        playerShieldFill.sprite = null;
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

        playerShieldText.enableAutoSizing = true;
        playerShieldText.fontSize = HealthTextMaximumFontSize;
        playerShieldText.fontSizeMin = HealthTextMinimumFontSize;
        playerShieldText.fontSizeMax = HealthTextMaximumFontSize;
        playerShieldText.alignment =
            TextAlignmentOptions.Center;
        playerShieldText.raycastTarget = false;

        int healthSiblingIndex =
            playerHealthBar.transform.GetSiblingIndex();

        playerShieldBar.transform.SetSiblingIndex(
            healthSiblingIndex + 1
        );

        PositionShieldBar();
    }

    private void PositionShieldBar()
    {
        if (playerShieldBar == null ||
            playerHealthBar == null ||
            playerShieldBar.transform is not RectTransform shieldRect ||
            playerHealthBar.transform is not RectTransform healthRect)
        {
            return;
        }

        shieldRect.anchorMin =
            healthRect.anchorMin;

        shieldRect.anchorMax =
            healthRect.anchorMax;

        shieldRect.pivot =
            healthRect.pivot;

        shieldRect.anchoredPosition =
            healthRect.anchoredPosition;

        shieldRect.sizeDelta =
            healthRect.sizeDelta;

        shieldRect.localScale =
            healthRect.localScale;

        shieldRect.localRotation =
            healthRect.localRotation;
    }

    private IEnumerator PlayShieldBreakAndDestroy()
    {
        if (playerShieldBar == null)
        {
            shieldBreakRoutine = null;
            yield break;
        }

        /*
         * A depleted shield gets one very short full-white frame-state before
         * the cloned bar is destroyed. This reads like a crisp shield break
         * instead of the blue fill simply vanishing.
         */
        if (playerShieldFill != null)
        {
            playerShieldFill.sprite = null;
            playerShieldFill.material = null;
            playerShieldFill.fillAmount = 1f;
            playerShieldFill.color = Color.white;
        }

        if (playerShieldText != null)
        {
            playerShieldText.text =
                string.Empty;
            playerShieldText.enabled = false;
        }

        if (playerHealthText != null)
        {
            playerHealthText.enabled = false;
        }

        yield return new WaitForSecondsRealtime(
            ShieldBreakFlashDuration
        );

        shieldBreakRoutine = null;
        DestroyShieldBarImmediately();

        if (playerHealthText != null)
        {
            playerHealthText.enabled = true;
        }
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

    private bool ShouldHideHealthText()
    {
        return shieldBreakRoutine != null ||
               (
                   boundPlayer != null &&
                   boundPlayer.HasShield
               );
    }

    private static string GetRelativePath(
        Transform root,
        Transform target)
    {
        if (root == null ||
            target == null)
        {
            return null;
        }

        if (target == root)
        {
            return string.Empty;
        }

        Stack<string> pathParts =
            new Stack<string>();

        Transform current =
            target;

        while (current != null &&
               current != root)
        {
            pathParts.Push(
                current.name
            );

            current =
                current.parent;
        }

        if (current != root)
        {
            return null;
        }

        return string.Join(
            "/",
            pathParts
        );
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
