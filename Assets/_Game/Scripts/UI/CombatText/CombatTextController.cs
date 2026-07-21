using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class CombatTextController :
    MonoBehaviour
{
    [Header("UI References")]
    [SerializeField]
    private Canvas rootCanvas;

    [SerializeField]
    private RectTransform combatTextLayer;

    [SerializeField]
    private FloatingCombatText floatingTextPrefab;

    [SerializeField]
    private CombatTextStyleLibrary styleLibrary;

    [Header("Player")]
    [SerializeField]
    private PlayerActor playerActor;

    [SerializeField]
    [Tooltip(
        "Usually the PlayerCharacter RectTransform."
    )]
    private RectTransform playerTextAnchor;

    [SerializeField]
    private Vector2 playerTextOffset =
        new Vector2(0f, 45f);

    [Header("Enemies")]
    [SerializeField]
    private WaveController waveController;

    [SerializeField]
    private Vector2 enemyTextOffset =
        new Vector2(0f, 40f);

    [Header("Pooling")]
    [SerializeField, Min(0)]
    private int prewarmCount = 8;

    private readonly Queue<FloatingCombatText>
        availableTexts =
            new Queue<FloatingCombatText>();

    private readonly HashSet<EnemyActor>
        boundEnemies =
            new HashSet<EnemyActor>();

    private void Awake()
    {
        ResolveCanvas();
    }

    private void OnEnable()
    {
        SubscribeToPlayer();
        SubscribeToWaves();
    }

    private void Start()
    {
        if (!ValidateReferences())
        {
            enabled = false;
            return;
        }

        PrewarmPool();
        BindCurrentEnemies();
    }

    private void OnDisable()
    {
        UnsubscribeFromPlayer();
        UnsubscribeFromWaves();
        UnbindAllEnemies();
    }

    private void ResolveCanvas()
    {
        if (rootCanvas == null &&
            combatTextLayer != null)
        {
            rootCanvas =
                combatTextLayer
                    .GetComponentInParent<Canvas>();
        }
    }

    private void SubscribeToPlayer()
    {
        if (playerActor == null)
        {
            return;
        }

        playerActor.DamageTaken -=
            HandlePlayerDamaged;

        playerActor.DamageTaken +=
            HandlePlayerDamaged;

        playerActor.Healed -=
            HandlePlayerHealed;

        playerActor.Healed +=
            HandlePlayerHealed;
    }

    private void UnsubscribeFromPlayer()
    {
        if (playerActor == null)
        {
            return;
        }

        playerActor.DamageTaken -=
            HandlePlayerDamaged;

        playerActor.Healed -=
            HandlePlayerHealed;
    }

    private void SubscribeToWaves()
    {
        if (waveController == null)
        {
            return;
        }

        waveController.EnemySpawned -=
            HandleEnemySpawned;

        waveController.EnemySpawned +=
            HandleEnemySpawned;

        waveController.WaveStarted -=
            HandleWaveStarted;

        waveController.WaveStarted +=
            HandleWaveStarted;
    }

    private void UnsubscribeFromWaves()
    {
        if (waveController == null)
        {
            return;
        }

        waveController.EnemySpawned -=
            HandleEnemySpawned;

        waveController.WaveStarted -=
            HandleWaveStarted;
    }

    private void HandleEnemySpawned(
    EnemyActor spawnedEnemy)
    {
        BindEnemy(spawnedEnemy);
    }

    private void HandleWaveStarted(
        int waveNumber)
    {
        BindCurrentEnemies();
    }

    private void BindCurrentEnemies()
    {
        UnbindAllEnemies();

        if (waveController == null)
        {
            return;
        }

        foreach (
            EnemyActor enemy
            in waveController.ActiveEnemies)
        {
            BindEnemy(enemy);
        }
    }

    private void BindEnemy(
        EnemyActor enemy)
    {
        if (enemy == null ||
            !boundEnemies.Add(enemy))
        {
            return;
        }

        enemy.DamageReceived +=
            HandleEnemyDamaged;

        enemy.Defeated +=
            HandleEnemyDefeated;
    }

    private void UnbindEnemy(
        EnemyActor enemy)
    {
        if (enemy == null)
        {
            return;
        }

        enemy.DamageReceived -=
            HandleEnemyDamaged;

        enemy.Defeated -=
            HandleEnemyDefeated;

        boundEnemies.Remove(enemy);
    }

    private void UnbindAllEnemies()
    {
        List<EnemyActor> snapshot =
            new List<EnemyActor>(
                boundEnemies
            );

        foreach (
            EnemyActor enemy
            in snapshot)
        {
            if (enemy != null)
            {
                enemy.DamageReceived -=
                    HandleEnemyDamaged;

                enemy.Defeated -=
                    HandleEnemyDefeated;
            }
        }

        boundEnemies.Clear();
    }

    private void HandlePlayerDamaged(
        PlayerActor player,
        int actualDamage)
    {
        if (actualDamage <= 0 ||
            playerTextAnchor == null)
        {
            return;
        }

        ShowText(
            $"-{actualDamage}",
            CombatTextKind.Damage,
            playerTextAnchor,
            playerTextOffset
        );
    }

    private void HandlePlayerHealed(
        PlayerActor player,
        int actualHealing)
    {
        if (actualHealing <= 0 ||
            playerTextAnchor == null)
        {
            return;
        }

        ShowText(
            $"+{actualHealing}",
            CombatTextKind.Healing,
            playerTextAnchor,
            playerTextOffset
        );
    }

    private void HandleEnemyDamaged(
        EnemyActor enemy,
        int actualDamage)
    {
        if (enemy == null ||
            actualDamage <= 0)
        {
            return;
        }

        ShowText(
            $"-{actualDamage}",
            CombatTextKind.Damage,
            enemy.transform,
            enemyTextOffset
        );
    }

    private void HandleEnemyDefeated(
        EnemyActor enemy)
    {
        UnbindEnemy(enemy);
    }

    public void ShowText(
        string displayedText,
        CombatTextKind kind,
        Transform anchor,
        Vector2 localOffset)
    {
        if (anchor == null ||
            string.IsNullOrWhiteSpace(
                displayedText
            ))
        {
            return;
        }

        ShowTextAtWorldPosition(
            displayedText,
            kind,
            anchor.position,
            localOffset
        );
    }

    public void ShowTextAtWorldPosition(
        string displayedText,
        CombatTextKind kind,
        Vector3 worldPosition,
        Vector2 localOffset)
    {
        if (!TryConvertToLayerPosition(
                worldPosition,
                out Vector2 layerPosition))
        {
            return;
        }

        CombatTextStyle style =
            styleLibrary.GetStyle(kind);

        if (style == null)
        {
            Debug.LogError(
                $"No combat-text style exists for {kind}.",
                styleLibrary
            );

            return;
        }

        FloatingCombatText instance =
            GetTextInstance();

        instance.Play(
            displayedText,
            style,
            layerPosition + localOffset,
            ReleaseText
        );
    }

    private bool TryConvertToLayerPosition(
        Vector3 worldPosition,
        out Vector2 localPosition)
    {
        localPosition =
            Vector2.zero;

        if (combatTextLayer == null)
        {
            return false;
        }

        Vector3 localPoint =
            combatTextLayer.InverseTransformPoint(
                worldPosition
            );

        localPosition =
            new Vector2(
                localPoint.x,
                localPoint.y
            );

        return true;
    }

    private FloatingCombatText
        GetTextInstance()
    {
        while (availableTexts.Count > 0)
        {
            FloatingCombatText existing =
                availableTexts.Dequeue();

            if (existing != null)
            {
                return existing;
            }
        }

        return CreateTextInstance();
    }

    private FloatingCombatText
        CreateTextInstance()
    {
        FloatingCombatText instance =
            Instantiate(
                floatingTextPrefab,
                combatTextLayer,
                false
            );

        instance.gameObject.SetActive(false);

        return instance;
    }

    private void ReleaseText(
        FloatingCombatText instance)
    {
        if (instance == null)
        {
            return;
        }

        instance.gameObject.SetActive(false);

        instance.transform.SetParent(
            combatTextLayer,
            false
        );

        availableTexts.Enqueue(
            instance
        );
    }

    private void PrewarmPool()
    {
        for (int index = 0;
             index < prewarmCount;
             index++)
        {
            FloatingCombatText instance =
                CreateTextInstance();

            availableTexts.Enqueue(
                instance
            );
        }
    }

    private bool ValidateReferences()
    {
        ResolveCanvas();

        bool isValid = true;

        if (rootCanvas == null)
        {
            Debug.LogError(
                "CombatTextController requires a Canvas.",
                this
            );

            isValid = false;
        }

        if (combatTextLayer == null)
        {
            Debug.LogError(
                "CombatTextController requires a " +
                "CombatTextLayer.",
                this
            );

            isValid = false;
        }

        if (floatingTextPrefab == null)
        {
            Debug.LogError(
                "CombatTextController requires a " +
                "FloatingCombatText prefab.",
                this
            );

            isValid = false;
        }

        if (styleLibrary == null)
        {
            Debug.LogError(
                "CombatTextController requires a " +
                "CombatTextStyleLibrary.",
                this
            );

            isValid = false;
        }

        if (playerActor == null)
        {
            Debug.LogError(
                "CombatTextController requires a PlayerActor.",
                this
            );

            isValid = false;
        }

        if (playerTextAnchor == null)
        {
            Debug.LogError(
                "CombatTextController requires a " +
                "Player Text Anchor.",
                this
            );

            isValid = false;
        }

        if (waveController == null)
        {
            Debug.LogError(
                "CombatTextController requires a " +
                "WaveController.",
                this
            );

            isValid = false;
        }

        return isValid;
    }
}