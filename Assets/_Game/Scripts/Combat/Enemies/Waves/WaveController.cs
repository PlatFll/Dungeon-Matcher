using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class WaveController : MonoBehaviour
{
    [Header("Enemy Data")]
    [SerializeField]
    private EnemyDatabase enemyDatabase;

    [SerializeField]
    private DifficultyProfile difficultyProfile;

    [SerializeField]
    private WaveSpawnProfile waveSpawnProfile;

    [Header("Player Target")]
    [SerializeField]
    private PlayerActor playerActor;

    [Header("Board Synchronization")]
    [SerializeField]
    [Tooltip(
    "Used to wait until matches and cascades have completely " +
    "finished before the next wave spawns."
)]
    private BoardController boardController;

    [Header("Automatic Wave Progression")]
    [SerializeField]
    private bool advanceWavesAutomatically = true;

    [SerializeField, Min(0f)]
    [Tooltip(
        "Delay after the board settles before the next wave begins. " +
        "Keep this at zero until a proper wave transition UI is added."
    )]
    private float delayBeforeNextWave;

    [Header("Enemy Slots")]
    [SerializeField]
    private EnemySlotUI[] enemySlots =
        new EnemySlotUI[3];

    [Header("Current Run")]
    [SerializeField, Min(1)]
    private int currentWave = 1;

    [SerializeField, Min(0.01f)]
    [Tooltip(
        "Keep this at 1 for now. The small capped " +
        "player-power correction will be connected later."
    )]
    private float playerPowerRatio = 1f;

    [SerializeField]
    private bool spawnWaveOnStart = true;

    [Header("Spawn Timing")]
    [SerializeField, Min(0f)]
    private float delayBetweenEnemySpawns = 0.5f;

    [Header("Selection Rules")]
    [SerializeField]
    [Tooltip(
        "Avoids spawning duplicate enemy types when enough " +
        "different eligible definitions are available. " +
        "Duplicates are still allowed when necessary."
    )]
    private bool avoidDuplicateEnemyTypesWhenPossible = true;

    [SerializeField]
    [Tooltip(
        "If a requested category has no available enemy, " +
        "the controller temporarily substitutes another category."
    )]
    private bool allowCategoryFallback = true;

    [Header("Prototype Testing")]
    [SerializeField, Min(1)]
    private int debugDamageAmount = 25;

    public event Action<int> WaveStarted;
    public event Action<int> WaveCompleted;

    public event Action<EnemyActor> EnemySpawned;

    public int CurrentWave => currentWave;

    public bool IsWaveActive { get; private set; }

    public WaveSpawnPlan CurrentPlan
    {
        get;
        private set;
    }

    public IReadOnlyList<EnemyActor> ActiveEnemies =>
        activeEnemies;

    private readonly List<EnemyActor> activeEnemies =
        new List<EnemyActor>();

    private int pendingDeathEffects;

    private bool waitingForDeathEffects;

    private Coroutine advanceWaveCoroutine;
    private Coroutine waveSpawnCoroutine;

    private static readonly EnemyCategory[]
        fallbackCategoryOrder =
        {
            EnemyCategory.Normal,
            EnemyCategory.Special,
            EnemyCategory.Miniboss,
            EnemyCategory.Boss
        };

    private void OnEnable()
    {
        SubscribeToSlots();
    }

    private void Start()
    {
        if (!ValidateReferences())
        {
            enabled = false;
            return;
        }

        if (spawnWaveOnStart)
        {
            SpawnCurrentWave();
        }
    }

    private void OnDisable()
    {
        CancelPendingWaveAdvance();
        UnsubscribeFromSlots();
    }

    public void SpawnCurrentWave()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning(
                "Waves can only spawn during Play mode.",
                this
            );

            return;
        }

        if (waveSpawnCoroutine != null)
        {
            StopCoroutine(
                waveSpawnCoroutine
            );

            waveSpawnCoroutine = null;
        }

        waveSpawnCoroutine =
            StartCoroutine(
                SpawnCurrentWaveRoutine()
            );
    }

    private IEnumerator SpawnCurrentWaveRoutine()
    {
        CancelPendingWaveAdvance();

        if (!ValidateReferences())
        {
            waveSpawnCoroutine = null;
            yield break;
        }

        ClearCurrentWave();

        CurrentPlan =
            waveSpawnProfile.CreatePlan(currentWave);

        if (CurrentPlan == null ||
            CurrentPlan.EnemyCount == 0)
        {
            Debug.LogError(
                $"Wave {currentWave} produced an empty spawn plan.",
                this
            );

            waveSpawnCoroutine = null;
            yield break;
        }

        List<GemType> availableGemTypes =
            CreateShuffledGemTypeList();

        int plannedEnemyCount =
            Mathf.Min(
                CurrentPlan.EnemyCount,
                enemySlots.Length,
                availableGemTypes.Count
            );

        HashSet<EnemyDefinition>
            selectedDefinitions =
                new HashSet<EnemyDefinition>();

        int spawnedEnemyCount = 0;

        for (int slotIndex = 0;
             slotIndex < enemySlots.Length;
             slotIndex++)
        {
            EnemySlotUI slot =
                enemySlots[slotIndex];

            if (slot == null)
            {
                continue;
            }

            if (slotIndex >= plannedEnemyCount)
            {
                slot.ClearSlot(
                    destroyEnemyObject: true,
                    notifyListeners: false
                );

                continue;
            }

            EnemyCategory requestedCategory =
                CurrentPlan.Categories[slotIndex];

            bool foundEnemy =
                TrySelectEnemyDefinition(
                    requestedCategory,
                    selectedDefinitions,
                    out EnemyDefinition definition,
                    out EnemyCategory selectedCategory
                );

            if (!foundEnemy ||
                definition == null)
            {
                Debug.LogError(
                    $"Wave {currentWave} could not find an " +
                    $"enemy for slot {slotIndex + 1}. " +
                    $"Requested category: {requestedCategory}.",
                    this
                );

                slot.ClearSlot(
                    destroyEnemyObject: true,
                    notifyListeners: false
                );

                continue;
            }

            if (selectedCategory != requestedCategory)
            {
                Debug.LogWarning(
                    $"Wave {currentWave} requested a " +
                    $"{requestedCategory} enemy for " +
                    $"{slot.name}, but none were available. " +
                    $"{selectedCategory} was substituted.",
                    this
                );
            }

            GemType assignedGemType =
                availableGemTypes[spawnedEnemyCount];

            EnemyActor enemy =
                CreateEnemy(
                    definition,
                    slot,
                    assignedGemType
                );

            if (enemy == null)
            {
                continue;
            }

            selectedDefinitions.Add(definition);
            activeEnemies.Add(enemy);

            EnemySpawned?.Invoke(enemy);

            spawnedEnemyCount++;

            if (slotIndex < plannedEnemyCount - 1 &&
                delayBetweenEnemySpawns > 0f)
            {
                yield return
                    new WaitForSeconds(
                        delayBetweenEnemySpawns
                    );
            }
        }

        IsWaveActive =
            spawnedEnemyCount > 0;

        if (!IsWaveActive)
        {
            Debug.LogError(
                $"Wave {currentWave} could not spawn any enemies.",
                this
            );

            waveSpawnCoroutine = null;
            yield break;
        }

        Debug.Log(
            $"Wave {currentWave} started using rule " +
            $"'{CurrentPlan.SourceRuleName}'. " +
            $"Planned: {CurrentPlan.EnemyCount}. " +
            $"Spawned: {spawnedEnemyCount}.",
            this
        );

        waveSpawnCoroutine = null;

        WaveStarted?.Invoke(currentWave);
    }

    private bool TrySelectEnemyDefinition(
        EnemyCategory requestedCategory,
        HashSet<EnemyDefinition> selectedDefinitions,
        out EnemyDefinition selectedDefinition,
        out EnemyCategory selectedCategory)
    {
        if (TrySelectFromCategory(
                requestedCategory,
                selectedDefinitions,
                out selectedDefinition))
        {
            selectedCategory =
                requestedCategory;

            return true;
        }

        if (!allowCategoryFallback)
        {
            selectedDefinition = null;
            selectedCategory = requestedCategory;

            return false;
        }

        foreach (
            EnemyCategory fallbackCategory
            in fallbackCategoryOrder)
        {
            if (fallbackCategory ==
                requestedCategory)
            {
                continue;
            }

            if (TrySelectFromCategory(
                    fallbackCategory,
                    selectedDefinitions,
                    out selectedDefinition))
            {
                selectedCategory =
                    fallbackCategory;

                return true;
            }
        }

        selectedDefinition = null;
        selectedCategory = requestedCategory;

        return false;
    }

    private bool TrySelectFromCategory(
        EnemyCategory category,
        HashSet<EnemyDefinition> selectedDefinitions,
        out EnemyDefinition selectedDefinition)
    {
        /*
         * First attempt: avoid duplicate enemy definitions.
         */
        if (avoidDuplicateEnemyTypesWhenPossible &&
            selectedDefinitions != null &&
            selectedDefinitions.Count > 0)
        {
            List<EnemyDefinition> uniqueCandidates =
                enemyDatabase.GetEligibleEnemies(
                    category,
                    currentWave,
                    selectedDefinitions
                );

            if (uniqueCandidates.Count > 0 &&
                enemyDatabase.TryGetRandomWeightedEnemy(
                    category,
                    currentWave,
                    out selectedDefinition,
                    selectedDefinitions
                ))
            {
                return true;
            }
        }

        /*
         * Second attempt: duplicates are allowed.
         * This is required while the database contains
         * only one Knight definition.
         */
        List<EnemyDefinition> allCandidates =
            enemyDatabase.GetEligibleEnemies(
                category,
                currentWave
            );

        if (allCandidates.Count == 0)
        {
            selectedDefinition = null;
            return false;
        }

        return enemyDatabase.TryGetRandomWeightedEnemy(
            category,
            currentWave,
            out selectedDefinition
        );
    }

    private EnemyActor CreateEnemy(
        EnemyDefinition definition,
        EnemySlotUI slot,
        GemType assignedGemType)
    {
        if (definition.EnemyPrefab == null)
        {
            Debug.LogError(
                $"{definition.name} has no enemy prefab assigned.",
                definition
            );

            return null;
        }

        if (slot.EnemySpawnAnchor == null)
        {
            Debug.LogError(
                $"{slot.name} has no spawn anchor assigned.",
                slot
            );

            return null;
        }

        EnemyRuntimeStats runtimeStats =
            difficultyProfile.CalculateStats(
                definition,
                currentWave,
                playerPowerRatio
            );

        GameObject enemyObject = Instantiate(
            definition.EnemyPrefab,
            slot.EnemySpawnAnchor,
            false
        );

        EnemyActor enemy =
            enemyObject.GetComponent<EnemyActor>();

        if (enemy == null)
        {
            Debug.LogError(
                $"The prefab {definition.EnemyPrefab.name} " +
                "does not contain an EnemyActor component.",
                definition.EnemyPrefab
            );

            Destroy(enemyObject);
            return null;
        }


        enemy.Initialize(
            definition,
            runtimeStats,
            assignedGemType
        );

        bool successfullyBound =
            slot.BindEnemy(enemy);

        if (!successfullyBound)
        {
            Destroy(enemyObject);
            return null;
        }

        EnemyAutoAttack autoAttack =
        enemyObject.GetComponent<EnemyAutoAttack>();

            if (autoAttack != null)
            {
                autoAttack.Initialize(
                    enemy,
                    playerActor
                );
            }
            else
            {
                Debug.LogWarning(
                    $"The prefab {definition.EnemyPrefab.name} " +
                    "does not contain an EnemyAutoAttack component.",
                    enemyObject
                );
        }

        EnemyLifecycleVFX lifecycleVFX =
            enemyObject.GetComponent<
                EnemyLifecycleVFX
            >();

        if (lifecycleVFX != null)
        {
            lifecycleVFX.PlaySpawnEffect();
        }
        else
        {
            Debug.LogWarning(
                $"The prefab {definition.EnemyPrefab.name} " +
                "does not contain an EnemyLifecycleVFX component.",
                enemyObject
            );
        }

        Debug.Log(
            $"Spawned {definition.DisplayName} " +
            $"in {slot.name}. " +
            $"Category: {definition.Category}. " +
            $"Gem type: {assignedGemType}. " +
            $"{runtimeStats}",
            enemy
        );

        return enemy;
    }

    private List<GemType>
        CreateShuffledGemTypeList()
    {
        Array enumValues =
            Enum.GetValues(typeof(GemType));

        List<GemType> gemTypes =
            new List<GemType>(
                enumValues.Length
            );

        foreach (GemType gemType in enumValues)
        {
            gemTypes.Add(gemType);
        }

        for (int index = gemTypes.Count - 1;
             index > 0;
             index--)
        {
            int randomIndex =
                UnityEngine.Random.Range(
                    0,
                    index + 1
                );

            GemType temporary =
                gemTypes[index];

            gemTypes[index] =
                gemTypes[randomIndex];

            gemTypes[randomIndex] =
                temporary;
        }

        return gemTypes;
    }

    private void HandleEnemyDefeated(
        EnemySlotUI slot,
        EnemyActor enemy)
    {
        if (enemy != null)
        {
            activeEnemies.Remove(enemy);

            EnemyLifecycleVFX lifecycleVFX =
                enemy.GetComponent<
                    EnemyLifecycleVFX
                >();

            if (lifecycleVFX != null)
            {
                pendingDeathEffects++;

                bool effectStarted =
                    lifecycleVFX.PlayDeathEffect(
                        () =>
                        {
                            pendingDeathEffects =
                                Mathf.Max(
                                    0,
                                    pendingDeathEffects - 1
                                );

                            if (enemy != null)
                            {
                                Destroy(
                                    enemy.gameObject
                                );
                            }

                            TryCompleteWaveAfterDeaths();
                        }
                    );

                if (!effectStarted)
                {
                    pendingDeathEffects =
                        Mathf.Max(
                            0,
                            pendingDeathEffects - 1
                        );

                    Destroy(
                        enemy.gameObject
                    );
                }
            }
            else
            {
                Destroy(
                    enemy.gameObject
                );
            }
        }

        Debug.Log(
            $"Enemy defeated in {slot.name}. " +
            $"{activeEnemies.Count} enemies remain.",
            slot
        );

        if (activeEnemies.Count == 0)
        {
            waitingForDeathEffects = true;

            TryCompleteWaveAfterDeaths();
        }
    }

    private void TryCompleteWaveAfterDeaths()
    {
        if (!waitingForDeathEffects)
        {
            return;
        }

        if (activeEnemies.Count > 0 ||
            pendingDeathEffects > 0)
        {
            return;
        }

        waitingForDeathEffects = false;

        CompleteCurrentWave();
    }

    private void CompleteCurrentWave()
    {
        if (!IsWaveActive)
        {
            return;
        }

        IsWaveActive = false;

        int completedWave =
            currentWave;

        Debug.Log(
            $"Wave {completedWave} completed.",
            this
        );

        WaveCompleted?.Invoke(completedWave);

        if (!advanceWavesAutomatically)
        {
            return;
        }

        if (playerActor == null ||
            playerActor.IsDefeated)
        {
            return;
        }

        if (advanceWaveCoroutine != null)
        {
            return;
        }

        advanceWaveCoroutine =
            StartCoroutine(
                AdvanceToNextWaveWhenReady(
                    completedWave
                )
            );
    }

    private IEnumerator AdvanceToNextWaveWhenReady(
    int completedWave)
    {
        /*
         * The final enemy may die while BoardController is still
         * clearing gems, dropping new gems, or resolving cascades.
         *
         * Waiting here prevents those old cascades from attacking
         * enemies belonging to the next wave.
         */
        while (boardController != null &&
               boardController.IsBusy)
        {
            yield return null;
        }

        if (delayBeforeNextWave > 0f)
        {
            yield return new WaitForSeconds(
                delayBeforeNextWave
            );
        }
        else
        {
            /*
             * Give the defeated enemy objects and UI one frame
             * to finish cleaning themselves up.
             */
            yield return null;
        }

        if (!isActiveAndEnabled ||
            playerActor == null ||
            playerActor.IsDefeated)
        {
            advanceWaveCoroutine = null;
            yield break;
        }

        /*
         * Stop this old transition if something else already
         * changed the wave or spawned enemies.
         */
        if (currentWave != completedWave ||
            IsWaveActive)
        {
            advanceWaveCoroutine = null;
            yield break;
        }

        currentWave =
            completedWave + 1;

        advanceWaveCoroutine = null;

        SpawnCurrentWave();
    }

    private void CancelPendingWaveAdvance()
    {
        if (advanceWaveCoroutine == null)
        {
            return;
        }

        StopCoroutine(
            advanceWaveCoroutine
        );

        advanceWaveCoroutine = null;
    }

    public void ClearCurrentWave()
    {
        CancelPendingWaveAdvance();

        pendingDeathEffects = 0;
        waitingForDeathEffects = false;

        IsWaveActive = false;
        activeEnemies.Clear();

        if (enemySlots == null)
        {
            return;
        }

        foreach (EnemySlotUI slot in enemySlots)
        {
            if (slot == null)
            {
                continue;
            }

            slot.ClearSlot(
                destroyEnemyObject: true,
                notifyListeners: false
            );
        }
    }

    private void SubscribeToSlots()
    {
        if (enemySlots == null)
        {
            return;
        }

        foreach (EnemySlotUI slot in enemySlots)
        {
            if (slot == null)
            {
                continue;
            }

            slot.EnemyDefeated -=
                HandleEnemyDefeated;

            slot.EnemyDefeated +=
                HandleEnemyDefeated;
        }
    }

    private void UnsubscribeFromSlots()
    {
        if (enemySlots == null)
        {
            return;
        }

        foreach (EnemySlotUI slot in enemySlots)
        {
            if (slot == null)
            {
                continue;
            }

            slot.EnemyDefeated -=
                HandleEnemyDefeated;
        }
    }

    private bool ValidateReferences()
    {
        bool isValid = true;

        if (enemyDatabase == null)
        {
            Debug.LogError(
                "WaveController requires an EnemyDatabase.",
                this
            );

            isValid = false;
        }

        if (difficultyProfile == null)
        {
            Debug.LogError(
                "WaveController requires a DifficultyProfile.",
                this
            );

            isValid = false;
        }

        if (waveSpawnProfile == null)
        {
            Debug.LogError(
                "WaveController requires a WaveSpawnProfile.",
                this
            );

            isValid = false;
        }

        if (playerActor == null)
        {
            Debug.LogError(
                "WaveController requires a PlayerActor.",
                this
            );

            isValid = false;
        }

        if (boardController == null)
        {
            Debug.LogError(
                "WaveController requires a BoardController.",
                this
            );

            isValid = false;
        }

        if (enemySlots == null ||
            enemySlots.Length == 0)
        {
            Debug.LogError(
                "WaveController requires enemy slots.",
                this
            );

            isValid = false;
        }
        else
        {
            for (int index = 0;
                 index < enemySlots.Length;
                 index++)
            {
                if (enemySlots[index] != null)
                {
                    continue;
                }

                Debug.LogError(
                    $"Enemy Slots element {index} is empty.",
                    this
                );

                isValid = false;
            }
        }

        return isValid;
    }

    [ContextMenu("Prototype/Spawn Current Wave")]
    private void DebugSpawnCurrentWave()
    {
        SpawnCurrentWave();
    }

    [ContextMenu("Prototype/Advance and Spawn Next Wave")]
    private void DebugAdvanceWave()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        currentWave++;
        SpawnCurrentWave();
    }

    [ContextMenu("Prototype/Damage All Enemies")]
    private void DebugDamageAllEnemies()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        List<EnemyActor> enemySnapshot =
            new List<EnemyActor>(
                activeEnemies
            );

        foreach (
            EnemyActor enemy
            in enemySnapshot)
        {
            if (enemy != null)
            {
                enemy.TryTakeDamage(
                    debugDamageAmount
                );
            }
        }
    }

    [ContextMenu("Prototype/Defeat All Enemies")]
    private void DebugDefeatAllEnemies()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        List<EnemyActor> enemySnapshot =
            new List<EnemyActor>(
                activeEnemies
            );

        foreach (
            EnemyActor enemy
            in enemySnapshot)
        {
            if (enemy != null)
            {
                enemy.TryTakeDamage(
                    enemy.CurrentHealth
                );
            }
        }
    }

    [ContextMenu("Prototype/Clear Wave")]
    private void DebugClearWave()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        ClearCurrentWave();
    }
}