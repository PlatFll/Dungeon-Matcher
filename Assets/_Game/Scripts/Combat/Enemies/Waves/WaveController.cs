using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed partial class WaveController :
    MonoBehaviour,
    IEnemySummonService
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

    [SerializeField] private int encounterSeed;
    private System.Random encounterRandom;
    private readonly HashSet<EnemyDefinition> seenMilestoneLeaders = new HashSet<EnemyDefinition>();
    private EnemyDefinition selectedMilestoneLeader;
    private readonly List<EnemyDefinition> originalEncounterDefinitions = new List<EnemyDefinition>();
    public IReadOnlyList<EnemyDefinition> OriginalEncounterDefinitions => originalEncounterDefinitions;
    public int EncounterSeed => encounterSeed;
    private System.Random EncounterRandom
    {
        get
        {
            if (encounterRandom == null)
            {
                if (encounterSeed == 0) encounterSeed = Environment.TickCount;
                encounterRandom = new SavedRandom(encounterSeed);
            }
            return encounterRandom;
        }
    }

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

    private bool isSpawningWave;
    private object encounterIdentity = new object();

    private readonly HashSet<IWaveProgressionGate> progressionGates =
        new HashSet<IWaveProgressionGate>();

    private void OnEnable()
    {
        SubscribeToSlots();
        SubscribeToBoard();
    }

    private void Start()
    {
        if (!ValidateReferences())
        {
            enabled = false;
            return;
        }

        /*
         * OnEnable runs before Start. Subscribe again after reference
         * validation so runtime-assigned references are also covered.
         */
        SubscribeToBoard();

        if (spawnWaveOnStart)
        {
            SpawnCurrentWave();
        }
    }

    private void OnDisable()
    {
        CancelPendingWaveAdvance();
        UnsubscribeFromSlots();
        UnsubscribeFromBoard();
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

        if (!isActiveAndEnabled)
        {
            return;
        }

        // Clear before starting, not from inside the new coroutine: a public
        // clear must also cancel an old spawn loop waiting between enemies.
        ClearCurrentWave();
        object identity = encounterIdentity;
        Coroutine started = StartCoroutine(SpawnCurrentWaveRoutine());

        // The routine may finish synchronously, or a spawn callback may clear
        // or replace the encounter before StartCoroutine returns.
        if (ReferenceEquals(identity, encounterIdentity) && isSpawningWave)
        {
            waveSpawnCoroutine = started;
        }
    }

    public void RegisterProgressionGate(IWaveProgressionGate gate)
    {
        if (gate != null)
        {
            progressionGates.Add(gate);
        }
    }

    public void UnregisterProgressionGate(IWaveProgressionGate gate)
    {
        if (gate != null)
        {
            progressionGates.Remove(gate);
        }
    }

    private bool IsWaveProgressionBlocked()
    {
        progressionGates.RemoveWhere(
            gate => gate == null ||
                    (gate is UnityEngine.Object unityObject &&
                     unityObject == null)
        );

        foreach (IWaveProgressionGate gate in progressionGates)
        {
            if (gate.IsBlockingWaveProgression)
            {
                return true;
            }
        }

        return false;
    }

    private IEnumerator SpawnCurrentWaveRoutine()
    {
        CancelPendingWaveAdvance();

        if (!ValidateReferences())
        {
            isSpawningWave = false;
            waveSpawnCoroutine = null;
            yield break;
        }

        object identity = encounterIdentity;
        int spawningWave = currentWave;
        isSpawningWave = true;

        selectedRecipeMembers = null;
        CurrentPlan =
            waveSpawnProfile.CreatePlan(currentWave, EncounterRandom);
        selectedMilestoneLeader = waveSpawnProfile.SelectMilestone(currentWave, EncounterRandom,
            seenMilestoneLeaders, out int milestoneCount);
        if (selectedMilestoneLeader != null && GetRepeatExclusions().Contains(selectedMilestoneLeader))
            selectedMilestoneLeader = null;
        if (selectedMilestoneLeader != null)
        {
            var categories = new List<EnemyCategory> { selectedMilestoneLeader.Category };
            while (categories.Count < milestoneCount) categories.Add(EnemyCategory.Normal);
            CurrentPlan = new WaveSpawnPlan(currentWave, "Weighted milestone opportunity", categories);
        }

        if (selectedMilestoneLeader == null && waveSpawnProfile.GetFixedEnemy(currentWave, 0) == null &&
            waveSpawnProfile.TrySelectRecipe(currentWave, enemySlots.Length, enemyDatabase, EncounterRandom,
                GetRepeatExclusions(), previousRecipeId, out var recipe, out var recipeMembers))
        {
            selectedRecipeMembers = recipeMembers;
            previousRecipeId = recipe.id;
            var categories = new List<EnemyCategory>();
            foreach (var member in recipeMembers) categories.Add(member.Category);
            CurrentPlan = new WaveSpawnPlan(currentWave, recipe.id + ": " + recipe.purpose, categories);
        }

        if (CurrentPlan == null ||
            CurrentPlan.EnemyCount == 0)
        {
            Debug.LogError(
                $"Wave {currentWave} produced an empty spawn plan.",
                this
            );

            isSpawningWave = false;
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

        int spawnedEnemyCount = 0;
        List<EnemyDefinition> encounter = BuildEncounter(plannedEnemyCount);
        originalEncounterDefinitions.Clear();
        plannedEnemyCount = Mathf.Min(plannedEnemyCount, encounter.Count);
        previousEncounterLeaders.Clear();

        for (int slotIndex = 0;
             slotIndex < enemySlots.Length;
             slotIndex++)
        {
            if (!IsCurrentSpawn(identity, spawningWave)) yield break;

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

            EnemyDefinition definition = encounter[slotIndex];
            EnemyCategory selectedCategory = requestedCategory;
            bool foundEnemy = definition != null;
            if (foundEnemy) selectedCategory = definition.Category;

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
                    $"{slot.name}; composition constraints selected " +
                    $"{selectedCategory} instead.",
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

            if (!IsCurrentSpawn(identity, spawningWave))
            {
                if (enemy != null) Destroy(enemy.gameObject);
                yield break;
            }

            if (enemy == null)
            {
                continue;
            }

            activeEnemies.Add(enemy);
            originalEncounterDefinitions.Add(definition);
            seenMilestoneLeaders.Add(definition);
            if (definition.Category == EnemyCategory.Miniboss || definition.Category == EnemyCategory.Boss)
                previousEncounterLeaders.Add(definition);

            spawnedEnemyCount++;

            /*
             * Combat becomes active as soon as the first enemy
             * successfully enters the wave. The remaining enemies
             * may continue spawning afterward.
             */
            IsWaveActive = true;

            EnemySpawned?.Invoke(enemy);
            if (!IsCurrentSpawn(identity, spawningWave)) yield break;

            if (slotIndex < plannedEnemyCount - 1 &&
                delayBetweenEnemySpawns > 0f)
            {
                yield return
                    new WaitForSeconds(
                        delayBetweenEnemySpawns
                    );
            }
        }

        if (!IsCurrentSpawn(identity, spawningWave)) yield break;

        /*
         * The complete spawn loop has now finished.
         */
        isSpawningWave = false;

        IsWaveActive =
            spawnedEnemyCount > 0;

        /*
         * This is the no-enemies-spawned block.
         *
         * isSpawningWave was already set to false directly
         * above, so this failed wave cannot remain marked
         * as spawning.
         */
        if (!IsWaveActive)
        {
            Debug.LogError(
                $"Wave {currentWave} could not spawn any enemies.",
                this
            );

            isSpawningWave = false;
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
        WaveStarted?.Invoke(spawningWave);

        // Fast kills during spawning may have queued completion already.
        // Publish start first, and never finalize a replacement encounter.
        if (ReferenceEquals(identity, encounterIdentity) && currentWave == spawningWave)
        {
            TryCompleteWaveAfterDeaths();
        }
    }

    private bool IsCurrentSpawn(object identity, int spawningWave)
    {
        return ReferenceEquals(identity, encounterIdentity) &&
               currentWave == spawningWave && isSpawningWave;
    }

    private bool TrySelectFromCategory(
        EnemyCategory category,
        HashSet<EnemyDefinition> selectedDefinitions,
        out EnemyDefinition selectedDefinition)
    {
        var excluded = GetRepeatExclusions();
        var uniqueExcluded = new HashSet<EnemyDefinition>(excluded);
        if (selectedDefinitions != null) uniqueExcluded.UnionWith(selectedDefinitions);
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
                    uniqueExcluded
                );

            if (uniqueCandidates.Count > 0 &&
                enemyDatabase.TryGetRandomWeightedEnemy(
                    category,
                    currentWave,
                    out selectedDefinition,
                    uniqueExcluded,
                    EncounterRandom
                ))
            {
                return true;
            }
        }

        /*
         * Second attempt: duplicates are allowed when the available content
         * for a requested category is smaller than the wave composition.
         */
        List<EnemyDefinition> allCandidates =
            enemyDatabase.GetEligibleEnemies(
                category,
                currentWave,
                excluded
            );

        if (allCandidates.Count == 0)
        {
            selectedDefinition = null;
            return false;
        }

        return enemyDatabase.TryGetRandomWeightedEnemy(
            category,
            currentWave,
            out selectedDefinition,
            excludedEnemies: excluded,
            deterministicRandom: EncounterRandom
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

        enemyObject.name =
            $"Enemy_{definition.DisplayName}";

        if (definition.StaticVisualSprite != null)
        {
            EnemyStaticSpritePresenter.TryApply(
                enemyObject,
                definition.StaticVisualSprite
            );
        }

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
            enemyObject.GetComponent<
                EnemyAutoAttack
            >();

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

        if (definition.HasSpecialAbility)
        {
            if (definition.SpecialAbilityKind ==
                EnemySpecialAbilityKind.None)
            {
                Debug.LogWarning(
                    $"{definition.DisplayName} enables a special ability " +
                    "but has no special ability kind assigned.",
                    definition
                );
            }
            else
            {
                EnemySpecialAbilityRuntimeFactory
                    .CreateAndInitialize(
                        definition.SpecialAbilityKind,
                        enemyObject,
                        enemy,
                        boardController,
                        activeEnemies,
                        this
                    );
            }
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
                EncounterRandom.Next(
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
        // An old slot notification or duplicate delivery cannot count a death
        // twice, or complete a wave to which the enemy never belonged.
        if (enemy == null || !activeEnemies.Remove(enemy)) return;
        object identity = encounterIdentity;

        EnemyLifecycleVFX lifecycleVFX =
            enemy.GetComponent<EnemyLifecycleVFX>();

        if (lifecycleVFX != null)
        {
            Action completeDeath = TrackEnemyDeathCompletion(enemy);
            if (!lifecycleVFX.PlayDeathEffect(completeDeath))
            {
                completeDeath();
            }
        }
        else
        {
            Destroy(enemy.gameObject);
        }

        if (!ReferenceEquals(identity, encounterIdentity)) return;

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

    private Action TrackEnemyDeathCompletion(EnemyActor enemy)
    {
        object identity = encounterIdentity;
        bool completed = false;
        pendingDeathEffects++;
        return () =>
        {
            if (completed) return;
            completed = true;

            // Old presentation may still finish after its slot was released.
            // Clean up its own object, but never decrement a newer wave's debt.
            if (enemy != null) Destroy(enemy.gameObject);
            if (this == null || !ReferenceEquals(identity, encounterIdentity)) return;

            pendingDeathEffects = Mathf.Max(0, pendingDeathEffects - 1);
            TryCompleteWaveAfterDeaths();
        };
    }

    private void HandleValidPlayerMoveCompleted(
        int completedMoveNumber)
    {
        if (!IsWaveActive ||
            activeEnemies.Count == 0)
        {
            return;
        }

        /*
         * Snapshot the list because a special becoming ready is allowed to
         * queue board work immediately. Each active special enemy receives
         * exactly one turn here, regardless of how many cascades the move had.
         */
        List<EnemyActor> enemySnapshot =
            new List<EnemyActor>(
                activeEnemies
            );

        foreach (EnemyActor enemy
                 in enemySnapshot)
        {
            if (enemy == null ||
                enemy.IsDefeated)
            {
                continue;
            }

            enemy.RegisterValidPlayerTurn();
        }
    }

    private void TryCompleteWaveAfterDeaths()
    {
        if (!waitingForDeathEffects)
        {
            return;
        }

        /*
         * Do not complete the wave while later planned enemies
         * are still waiting to spawn.
         */
        if (isSpawningWave)
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
        object identity = encounterIdentity;

        Debug.Log(
            $"Wave {completedWave} completed.",
            this
        );

        WaveCompleted?.Invoke(completedWave);

        // Completion listeners may clear/restart even the same wave number.
        if (!ReferenceEquals(identity, encounterIdentity)) return;

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
        object identity = encounterIdentity;
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

        // A new swap or ability can acquire the board during the delay or
        // while an intermission gate is held. Both conditions must be clear
        // in the same frame immediately before spawning the next encounter.
        while ((boardController != null && boardController.IsBusy) ||
               IsWaveProgressionBlocked())
        {
            if (!isActiveAndEnabled ||
                playerActor == null ||
                playerActor.IsDefeated)
            {
                advanceWaveCoroutine = null;
                yield break;
            }

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
        if (!ReferenceEquals(identity, encounterIdentity) ||
            currentWave != completedWave ||
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
        // Reusing a wave number is still a new encounter identity. Invalidate
        // callbacks before stopping work or destroying any bound objects.
        encounterIdentity = new object();
        CancelPendingWaveAdvance();
        if (waveSpawnCoroutine != null)
        {
            StopCoroutine(waveSpawnCoroutine);
            waveSpawnCoroutine = null;
        }
        isSpawningWave = false;

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

    private void SubscribeToBoard()
    {
        if (boardController == null)
        {
            return;
        }

        boardController.ValidPlayerMoveCompleted -=
            HandleValidPlayerMoveCompleted;

        boardController.ValidPlayerMoveCompleted +=
            HandleValidPlayerMoveCompleted;
    }

    private void UnsubscribeFromBoard()
    {
        if (boardController == null)
        {
            return;
        }

        boardController.ValidPlayerMoveCompleted -=
            HandleValidPlayerMoveCompleted;
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
