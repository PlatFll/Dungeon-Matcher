using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class EnemySlotUI : MonoBehaviour
{
    [Header("Spawn Anchor")]
    [SerializeField]
    private RectTransform enemySpawnAnchor;

    [Header("Slot UI")]
    [SerializeField]
    private Image enemyBase;

    [SerializeField]
    private GameObject enemyHealthBar;

    [SerializeField]
    private Image enemyHealthFill;

    [SerializeField]
    private TMP_Text enemyHealthText;

    [Header("Shared Visual Data")]
    [SerializeField]
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

    private void Awake()
    {
        ShowEmptyState();
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

        CurrentEnemy = enemy;

        PlaceEnemyAtAnchor(enemy);
        SubscribeToEnemy(enemy);

        enemy.gameObject.SetActive(true);

        ShowOccupiedState();

        UpdateHealthDisplay(
            enemy.CurrentHealth,
            enemy.MaxHealth
        );

        UpdateGemColor(
            enemy.AssignedGemType
        );

        EnemyBound?.Invoke(this, enemy);

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

        ShowEmptyState();

        if (notifyListeners)
        {
            SlotCleared?.Invoke(this);
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

        UpdateHealthDisplay(
            currentHealth,
            maximumHealth
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

        UpdateGemColor(gemType);
    }

    private void HandleEnemyDefeated(
        EnemyActor enemy)
    {
        if (enemy != CurrentEnemy)
        {
            return;
        }

        UnsubscribeFromEnemy(enemy);

        CurrentEnemy = null;

        SetBaseToEmptyColor();

        if (enemyHealthBar != null)
        {
            enemyHealthBar.SetActive(false);
        }

        if (hideEnemyImmediatelyOnDefeat)
        {
            enemy.gameObject.SetActive(false);
        }

        EnemyDefeated?.Invoke(
            this,
            enemy
        );
    }

    private void ShowOccupiedState()
    {
        if (enemyHealthBar != null)
        {
            enemyHealthBar.SetActive(true);
        }
    }

    private void ShowEmptyState()
    {
        SetBaseToEmptyColor();

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

    private void UpdateGemColor(
        GemType gemType)
    {
        if (enemyBase == null)
        {
            return;
        }

        enemyBase.color =
            gemColorPalette != null
                ? gemColorPalette.GetColor(gemType)
                : Color.white;
    }

    private void SetBaseToEmptyColor()
    {
        if (enemyBase == null)
        {
            return;
        }

        enemyBase.color =
            gemColorPalette != null
                ? gemColorPalette.EmptyColor
                : Color.gray;
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