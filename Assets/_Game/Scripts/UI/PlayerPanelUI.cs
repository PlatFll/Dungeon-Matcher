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

        DisableLegacyPlayerBase();
        CreateAffinityGemIndicator();
    }

    private void OnEnable()
    {
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
        }
    }

    private void ShowUninitializedState()
    {
        DisableLegacyPlayerBase();

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
        }
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
    }
}
