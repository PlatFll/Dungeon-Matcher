using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class EnemySlotUI : MonoBehaviour
{
    private const string WeaknessIndicatorConfigPath =
        "UI/EnemyWeaknessIndicatorConfig";

    private const string TopBattlePresentationProfilePath =
        "UI/TopBattlePresentationProfile";

    private const float FallbackEnemyHealthBarBottomOffset = 14f;

    [Header("Spawn Anchor")]
    [SerializeField]
    private RectTransform enemySpawnAnchor;

    [Header("Slot UI")]
    [SerializeField]
    [Tooltip(
        "Legacy colored weakness circle. It is kept only so existing scenes " +
        "migrate safely and is disabled at runtime."
    )]
    private Image enemyBase;

    [SerializeField]
    private GameObject enemyHealthBar;

    [SerializeField]
    private Image enemyHealthFill;

    [SerializeField]
    private TMP_Text enemyHealthText;

    [Header("Legacy Shared Visual Data")]
    [SerializeField]
    [Tooltip(
        "Legacy palette used by the old colored weakness circles. " +
        "The new weakness indicator uses 16x16 gem sprites instead."
    )]
    private GemColorPalette gemColorPalette;

    [Header("Defeat Prototype")]
    [SerializeField]
    [Tooltip(
        "Legacy fallback that immediately hides defeated enemies. " +
        "Keep disabled when using EnemyLifecycleVFX."
    )]
    private bool hideEnemyImmediatelyOnDefeat = false;

    public event Action<EnemySlotUI, EnemyActor> EnemyBound;
    public event Action<EnemySlotUI, EnemyActor> EnemyDefeated;
    public event Action<EnemySlotUI> SlotCleared;

    public EnemyActor CurrentEnemy { get; private set; }

    public RectTransform EnemySpawnAnchor =>
        enemySpawnAnchor;

    public bool IsOccupied =>
        CurrentEnemy != null &&
        CurrentEnemy.IsInitialized &&
        !CurrentEnemy.IsDefeated;

    private EnemyWeaknessIndicatorUI weaknessIndicator;
    private TopBattlePresentationProfile presentationProfile;
    private RectTransform enemyHealthBarRect;

    private void Awake()
    {
        /*
         * EnemySlotUI is part of the authoritative wave-spawn path. Optional
         * UI setup must never be able to prevent WaveController from using the
         * slot, especially on device builds where a shader/material/resource
         * can behave differently from the Editor.
         */
        RunPresentationSafely(
            "initial slot presentation",
            () =>
            {
                presentationProfile =
                    Resources.Load<TopBattlePresentationProfile>(
                        TopBattlePresentationProfilePath
                    );

                enemyHealthBarRect =
                    enemyHealthBar != null
                        ? enemyHealthBar.transform as RectTransform
                        : null;

                DisableLegacyWeaknessCircle();
                CreateWeaknessIndicator();
                ShowEmptyState();
            }
        );

        /*
         * This reference is harmless to resolve even if presentation setup
         * above failed before reaching it. LateUpdate simply no-ops if null.
         */
        if (enemyHealthBarRect == null &&
            enemyHealthBar != null)
        {
            enemyHealthBarRect =
                enemyHealthBar.transform as RectTransform;
        }
    }

    private void LateUpdate()
    {
        ApplyHealthBarPosition();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEnemy(CurrentEnemy);
    }

    public bool BindEnemy(EnemyActor enemy)
    {
        if (enemy == null)
        {
            Debug.LogError(
                $"{name} cannot bind a null EnemyActor.",
                this
            );

            return false;
        }

        if (!enemy.IsInitialized)
        {
            Debug.LogError(
                $"{name} cannot bind {enemy.name} because " +
                "the enemy has not been initialized.",
                this
            );

            return false;
        }

        if (enemySpawnAnchor == null)
        {
            Debug.LogError(
                $"{name} has no Enemy Spawn Anchor assigned.",
                this
            );

            return false;
        }

        if (CurrentEnemy != null)
        {
            ClearSlot(
                destroyEnemyObject: true,
                notifyListeners: false
            );
        }

        /*
         * Everything above this line is validation/cleanup. Everything below
         * establishes gameplay ownership before optional presentation runs.
         * A health-bar or weakness-indicator exception therefore cannot turn a
         * successfully-created EnemyActor into a failed wave spawn.
         */
        CurrentEnemy = enemy;

        try
        {
            PlaceEnemyAtAnchor(enemy);
            SubscribeToEnemy(enemy);
            enemy.gameObject.SetActive(true);
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"{name} failed to establish core binding for {enemy.name}.",
                this
            );

            Debug.LogException(
                exception,
                this
            );

            UnsubscribeFromEnemy(enemy);
            CurrentEnemy = null;
            return false;
        }

        RunPresentationSafely(
            $"occupied presentation for {enemy.name}",
            () =>
            {
                ShowOccupiedState();

                UpdateHealthDisplay(
                    enemy.CurrentHealth,
                    enemy.MaxHealth
                );

                ShowWeaknessIndicator(
                    enemy.AssignedGemType,
                    animate: true
                );
            }
        );

        InvokeEnemyBoundSafely(enemy);

        return true;
    }

    public void ClearSlot(
        bool destroyEnemyObject = true,
        bool notifyListeners = true)
    {
        EnemyActor previousEnemy =
            CurrentEnemy;

        if (previousEnemy != null)
        {
            UnsubscribeFromEnemy(previousEnemy);
        }

        /*
         * Clear gameplay ownership before touching any UI. SpawnCurrentWave
         * calls this during its first setup step, so presentation must not be
         * able to abort the wave coroutine here.
         */
        CurrentEnemy = null;

        if (previousEnemy != null)
        {
            if (destroyEnemyObject)
            {
                Destroy(previousEnemy.gameObject);
            }
            else
            {
                previousEnemy.gameObject.SetActive(false);
            }
        }

        RunPresentationSafely(
            "empty slot presentation",
            ShowEmptyState
        );

        if (notifyListeners)
        {
            InvokeSlotClearedSafely();
        }
    }

    private void PlaceEnemyAtAnchor(
        EnemyActor enemy)
    {
        Transform enemyTransform =
            enemy.transform;

        enemyTransform.SetParent(
            enemySpawnAnchor,
            false
        );

        enemyTransform.localPosition =
            Vector3.zero;

        enemyTransform.localRotation =
            Quaternion.identity;

        enemyTransform.localScale =
            Vector3.one;

        if (enemyTransform is RectTransform enemyRect)
        {
            enemyRect.anchorMin =
                new Vector2(0.5f, 0.5f);

            enemyRect.anchorMax =
                new Vector2(0.5f, 0.5f);

            enemyRect.pivot =
                new Vector2(0.5f, 0.5f);

            enemyRect.anchoredPosition =
                Vector2.zero;
        }
    }

    private void SubscribeToEnemy(
        EnemyActor enemy)
    {
        enemy.HealthChanged +=
            HandleHealthChanged;

        enemy.GemTypeChanged +=
            HandleGemTypeChanged;

        enemy.Defeated +=
            HandleEnemyDefeated;
    }

    private void UnsubscribeFromEnemy(
        EnemyActor enemy)
    {
        if (enemy == null)
        {
            return;
        }

        enemy.HealthChanged -=
            HandleHealthChanged;

        enemy.GemTypeChanged -=
            HandleGemTypeChanged;

        enemy.Defeated -=
            HandleEnemyDefeated;
    }

    private void HandleHealthChanged(
        EnemyActor enemy,
        int currentHealth,
        int maximumHealth)
    {
        if (enemy != CurrentEnemy)
        {
            return;
        }

        RunPresentationSafely(
            $"health display update for {enemy.name}",
            () =>
            {
                UpdateHealthDisplay(
                    currentHealth,
                    maximumHealth
                );
            }
        );
    }

    private void HandleGemTypeChanged(
        EnemyActor enemy,
        GemType gemType)
    {
        if (enemy != CurrentEnemy)
        {
            return;
        }

        RunPresentationSafely(
            $"weakness display update for {enemy.name}",
            () =>
            {
                ShowWeaknessIndicator(
                    gemType,
                    animate: true
                );
            }
        );
    }

    private void HandleEnemyDefeated(
        EnemyActor enemy)
    {
        if (enemy != CurrentEnemy)
        {
            return;
        }

        /*
         * Gameplay lifecycle comes first. In particular, WaveController listens
         * to EnemyDefeated and must always receive that notification even if a
         * weakness-gem death pop or health-bar operation fails on a device.
         */
        UnsubscribeFromEnemy(enemy);
        CurrentEnemy = null;

        RunPresentationSafely(
            $"defeat presentation for {enemy.name}",
            () =>
            {
                /*
                 * Start the weakness gem's white-pop burst before the health
                 * bar is hidden. The indicator is parented to the slot rather
                 * than the bar so its short death effect can finish separately.
                 */
                if (weaknessIndicator != null)
                {
                    weaknessIndicator.PlayDefeat();
                }

                DisableLegacyWeaknessCircle();

                if (enemyHealthBar != null)
                {
                    enemyHealthBar.SetActive(false);
                }

                if (hideEnemyImmediatelyOnDefeat)
                {
                    enemy.gameObject.SetActive(false);
                }
            }
        );

        InvokeEnemyDefeatedSafely(enemy);
    }

    private void ShowOccupiedState()
    {
        DisableLegacyWeaknessCircle();

        if (enemyHealthBar != null)
        {
            enemyHealthBar.SetActive(true);
        }
    }

    private void ShowEmptyState()
    {
        DisableLegacyWeaknessCircle();

        if (enemyHealthBar != null)
        {
            enemyHealthBar.SetActive(false);
        }

        if (enemyHealthFill != null)
        {
            enemyHealthFill.fillAmount = 0f;
        }

        if (enemyHealthText != null)
        {
            enemyHealthText.text = string.Empty;
        }

        /*
         * A defeated indicator is allowed to finish its tiny burst even if
         * the slot is cleared as part of wave cleanup. A future BindEnemy call
         * will immediately replace it with the new enemy's weakness sprite.
         */
        if (weaknessIndicator != null &&
            !weaknessIndicator.IsDefeating)
        {
            weaknessIndicator.HideImmediate();
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

        if (enemyHealthFill != null)
        {
            enemyHealthFill.fillAmount =
                (float)currentHealth /
                maximumHealth;
        }

        if (enemyHealthText != null)
        {
            enemyHealthText.text =
                $"{currentHealth} / {maximumHealth}";
        }
    }

    private void ApplyHealthBarPosition()
    {
        if (enemyHealthBarRect == null)
        {
            return;
        }

        float bottomOffset =
            presentationProfile != null
                ? presentationProfile.EnemyHealthBarBottomOffset
                : FallbackEnemyHealthBarBottomOffset;

        Vector2 position =
            enemyHealthBarRect.anchoredPosition;

        if (Mathf.Approximately(
                position.y,
                bottomOffset))
        {
            return;
        }

        position.y = bottomOffset;
        enemyHealthBarRect.anchoredPosition =
            position;
    }

    private void CreateWeaknessIndicator()
    {
        if (weaknessIndicator != null)
        {
            return;
        }

        RectTransform slotRect =
            transform as RectTransform;

        RectTransform healthBarRect =
            enemyHealthBar != null
                ? enemyHealthBar.transform as RectTransform
                : null;

        if (slotRect == null ||
            healthBarRect == null)
        {
            Debug.LogError(
                $"{name} needs RectTransform-based slot and health bar " +
                "objects to place its weakness gem indicator.",
                this
            );

            return;
        }

        EnemyWeaknessIndicatorConfig config =
            Resources.Load<EnemyWeaknessIndicatorConfig>(
                WeaknessIndicatorConfigPath
            );

        if (config == null)
        {
            Debug.LogError(
                $"Could not load {WeaknessIndicatorConfigPath} from Resources.",
                this
            );

            return;
        }

        GameObject indicatorObject =
            new GameObject(
                "EnemyWeaknessIndicator",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(CanvasGroup)
            );

        indicatorObject.layer =
            gameObject.layer;

        indicatorObject.transform.SetParent(
            transform,
            false
        );

        indicatorObject.transform.SetAsLastSibling();

        weaknessIndicator =
            indicatorObject.AddComponent<
                EnemyWeaknessIndicatorUI
            >();

        weaknessIndicator.Initialize(
            slotRect,
            healthBarRect,
            config
        );
    }

    private void ShowWeaknessIndicator(
        GemType gemType,
        bool animate)
    {
        DisableLegacyWeaknessCircle();

        if (weaknessIndicator == null)
        {
            return;
        }

        weaknessIndicator.Show(
            gemType,
            animate
        );
    }

    private void DisableLegacyWeaknessCircle()
    {
        if (enemyBase == null)
        {
            return;
        }

        enemyBase.gameObject.SetActive(false);
    }

    private void RunPresentationSafely(
        string operation,
        Action presentationAction)
    {
        if (presentationAction == null)
        {
            return;
        }

        try
        {
            presentationAction();
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"{name} failed during {operation}. " +
                "Gameplay will continue without that presentation step.",
                this
            );

            Debug.LogException(
                exception,
                this
            );
        }
    }

    private void InvokeEnemyBoundSafely(
        EnemyActor enemy)
    {
        Action<EnemySlotUI, EnemyActor> handlers =
            EnemyBound;

        if (handlers == null)
        {
            return;
        }

        foreach (Delegate callback
                 in handlers.GetInvocationList())
        {
            try
            {
                ((Action<EnemySlotUI, EnemyActor>)callback)(
                    this,
                    enemy
                );
            }
            catch (Exception exception)
            {
                Debug.LogException(
                    exception,
                    this
                );
            }
        }
    }

    private void InvokeEnemyDefeatedSafely(
        EnemyActor enemy)
    {
        Action<EnemySlotUI, EnemyActor> handlers =
            EnemyDefeated;

        if (handlers == null)
        {
            return;
        }

        foreach (Delegate callback
                 in handlers.GetInvocationList())
        {
            try
            {
                ((Action<EnemySlotUI, EnemyActor>)callback)(
                    this,
                    enemy
                );
            }
            catch (Exception exception)
            {
                Debug.LogException(
                    exception,
                    this
                );
            }
        }
    }

    private void InvokeSlotClearedSafely()
    {
        Action<EnemySlotUI> handlers =
            SlotCleared;

        if (handlers == null)
        {
            return;
        }

        foreach (Delegate callback
                 in handlers.GetInvocationList())
        {
            try
            {
                ((Action<EnemySlotUI>)callback)(
                    this
                );
            }
            catch (Exception exception)
            {
                Debug.LogException(
                    exception,
                    this
                );
            }
        }
    }

    private void OnValidate()
    {
        if (enemyHealthFill != null)
        {
            enemyHealthFill.type =
                Image.Type.Filled;

            enemyHealthFill.fillMethod =
                Image.FillMethod.Horizontal;

            enemyHealthFill.fillOrigin =
                (int)Image.OriginHorizontal.Left;
        }
    }
}
