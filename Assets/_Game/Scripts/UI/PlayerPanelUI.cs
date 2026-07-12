using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class PlayerPanelUI : MonoBehaviour
{
    [Header("Runtime Player")]
    [SerializeField]
    private PlayerActor playerActor;

    [Header("Character Presentation")]
    [SerializeField]
    private Image playerFrame;

    [SerializeField]
    private Image playerCharacter;

    [SerializeField]
    private Image playerBase;

    [Header("Shared Visual Data")]
    [SerializeField]
    private GemColorPalette gemColorPalette;

    [Header("Health Bar")]
    [SerializeField]
    private GameObject playerHealthBar;

    [SerializeField]
    private Image playerHealthFill;

    [SerializeField]
    private TMP_Text playerHealthText;

    private PlayerActor boundPlayer;

    public PlayerActor BoundPlayer =>
        boundPlayer;

    private void OnEnable()
    {
        BindPlayer(playerActor);
    }

    private void Start()
    {
        RefreshAll();
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

        if (playerBase != null)
        {
            playerBase.gameObject.SetActive(true);

            playerBase.color =
                gemColorPalette != null
                    ? gemColorPalette.GetColor(
                        definition.AffinityGemType
                    )
                    : Color.white;
        }

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
        if (playerBase != null)
        {
            playerBase.color =
                gemColorPalette != null
                    ? gemColorPalette.EmptyColor
                    : Color.gray;
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