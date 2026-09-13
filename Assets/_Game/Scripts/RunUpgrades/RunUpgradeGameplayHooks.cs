using UnityEngine;

[DisallowMultipleComponent]
public sealed class RunUpgradeGameplayHooks : MonoBehaviour
{
    private const int PreparedCastingEnergy = 15;
    private const int EmergencyPlatingShield = 10;
    private const int ResonantCracksFrequency = 3;
    private const int ResonantCracksEnergy = 5;

    private RunUpgradeRuntime runtime;
    private BoardController board;
    private CombatController combat;
    private PlayerActor player;
    private WaveController waves;
    private PlayerAbilityEnergy energy;

    private int baseMaximumHealth;
    private int observedShield;
    private int resonantCrackCount;
    private bool emergencyPlatingUsedThisWave;
    private bool isConfigured;
    private int observedRunRevision;
    private int observedWave;

    public static RunUpgradeGameplayHooks Current { get; private set; }

    public static int CurrentSpecialClearCount =>
        Current != null ? Current.currentSpecialClearCount : 0;

    public static int CurrentDirectionalBombCount =>
        Current != null ? Current.currentDirectionalBombCount : 0;

    public static bool HasAnyPoisonedEnemy =>
        Current != null && Current.CheckAnyPoisonedEnemy();

    private int currentSpecialClearCount;
    private int currentDirectionalBombCount;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration
    )]
    private static void ResetStaticState()
    {
        Current = null;
    }

    private void OnEnable()
    {
        if (Current != null && Current != this)
        {
            enabled = false;
            return;
        }

        Current = this;
        SynchronizeTrackingAfterEnable();
        Subscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
        HandleSpecialClearFinished();

        if (Current == this)
        {
            Current = null;
        }
    }

    public void Configure(
        RunUpgradeRuntime runRuntime,
        BoardController boardController,
        CombatController combatController,
        PlayerActor runPlayer,
        WaveController waveController)
    {
        if (isConfigured && runtime == runRuntime && board == boardController &&
            combat == combatController && player == runPlayer && waves == waveController)
        {
            Subscribe();
            return;
        }

        Unsubscribe();

        runtime = runRuntime;
        board = boardController;
        combat = combatController;
        player = runPlayer;
        waves = waveController;
        energy = player != null
            ? player.GetComponent<PlayerAbilityEnergy>()
            : null;
        isConfigured = true;

        ResetRunTracking();
        Subscribe();
    }

    private void ResetRunTracking()
    {
        baseMaximumHealth = runtime != null && runtime.BaseMaximumHealth > 0
            ? runtime.BaseMaximumHealth
            : player != null && player.IsInitialized ? player.MaximumHealth : 0;
        observedRunRevision = runtime != null ? runtime.RunRevision : 0;
        observedWave = waves != null ? waves.CurrentWave : 0;
        observedShield = player != null ? player.CurrentShield : 0;
        resonantCrackCount = 0;
        emergencyPlatingUsedThisWave = false;
        HandleSpecialClearFinished();
    }

    private void SynchronizeTrackingAfterEnable()
    {
        if (runtime != null && observedRunRevision != runtime.RunRevision)
        {
            // ResetRun may have happened while this observer was disabled.
            ResetRunTracking();
            return;
        }

        int wave = waves != null ? waves.CurrentWave : 0;
        if (observedWave != wave)
        {
            observedWave = wave;
            resonantCrackCount = 0;
            emergencyPlatingUsedThisWave = false;
        }

        // A temporary disable is NOT a fresh wave or a shield break. Preserve
        // same-wave usage counters and never replay energy/shield rewards.
        observedShield = player != null ? player.CurrentShield : 0;
        HandleSpecialClearFinished();
    }

    private void Subscribe()
    {
        if (!isActiveAndEnabled)
        {
            return;
        }

        if (runtime != null)
        {
            runtime.UpgradeChanged -= HandleUpgradeChanged;
            runtime.UpgradeChanged += HandleUpgradeChanged;
            runtime.RunReset -= ResetRunTracking;
            runtime.RunReset += ResetRunTracking;
        }

        if (board != null)
        {
            board.RunUpgradeSpecialClearPrepared -=
                HandleSpecialClearPrepared;
            board.RunUpgradeSpecialClearPrepared +=
                HandleSpecialClearPrepared;
            board.RunUpgradeSpecialClearFinished -=
                HandleSpecialClearFinished;
            board.RunUpgradeSpecialClearFinished +=
                HandleSpecialClearFinished;
            board.BoardClearOutcomeResolved -=
                HandleBoardClearOutcomeResolved;
            board.BoardClearOutcomeResolved +=
                HandleBoardClearOutcomeResolved;
        }

        if (combat != null)
        {
            combat.BeforeGemDamage -= HandleBeforeGemDamage;
            combat.BeforeGemDamage += HandleBeforeGemDamage;
        }

        if (player != null)
        {
            player.Initialized -= HandlePlayerInitialized;
            player.Initialized += HandlePlayerInitialized;
            player.ShieldChanged -= HandleShieldChanged;
            player.ShieldChanged += HandleShieldChanged;
        }

        if (waves != null)
        {
            waves.WaveStarted -= HandleWaveStarted;
            waves.WaveStarted += HandleWaveStarted;
        }
    }

    private void Unsubscribe()
    {
        if (runtime != null)
        {
            runtime.UpgradeChanged -= HandleUpgradeChanged;
            runtime.RunReset -= ResetRunTracking;
        }

        if (board != null)
        {
            board.RunUpgradeSpecialClearPrepared -=
                HandleSpecialClearPrepared;
            board.RunUpgradeSpecialClearFinished -=
                HandleSpecialClearFinished;
            board.BoardClearOutcomeResolved -=
                HandleBoardClearOutcomeResolved;
        }

        if (combat != null)
        {
            combat.BeforeGemDamage -= HandleBeforeGemDamage;
        }

        if (player != null)
        {
            player.Initialized -= HandlePlayerInitialized;
            player.ShieldChanged -= HandleShieldChanged;
        }

        if (waves != null)
        {
            waves.WaveStarted -= HandleWaveStarted;
        }
    }

    private void HandleSpecialClearPrepared(
        RunUpgradeSpecialClearContext context)
    {
        currentSpecialClearCount = context.SpecialCount;
        currentDirectionalBombCount = context.DirectionalBombCount;
    }

    private void HandleSpecialClearFinished()
    {
        currentSpecialClearCount = 0;
        currentDirectionalBombCount = 0;
    }

    private void HandleUpgradeChanged(
        RunUpgradeDefinition definition,
        int stacks)
    {
        if (player == null ||
            !player.IsInitialized ||
            baseMaximumHealth <= 0)
        {
            return;
        }

        int desiredMaximumHealth =
            RunUpgradeResolver.ResolveMaximumHealth(
                baseMaximumHealth,
                runtime
            );

        player.SetMaximumHealth(
            desiredMaximumHealth,
            healAddedAmount: desiredMaximumHealth > player.MaximumHealth
        );
    }

    private void HandlePlayerInitialized(PlayerActor initializedPlayer)
    {
        if (initializedPlayer != player)
        {
            return;
        }

        ResetRunTracking();
        // Player.Initialized and runtime.RunReset subscription order may vary.
        // The actor has already installed its new unmodified baseline here.
        baseMaximumHealth = player.MaximumHealth;
    }

    private void HandleWaveStarted(int wave)
    {
        observedWave = wave;
        emergencyPlatingUsedThisWave = false;
        resonantCrackCount = 0;

        if (energy != null &&
            RunUpgradeResolver.HasMechanic(
                RunUpgradeMechanic.PreparedCasting,
                runtime
            ))
        {
            energy.AddEnergy(PreparedCastingEnergy);
        }
    }

    private void HandleShieldChanged(
        PlayerActor changedPlayer,
        int currentShield,
        int maximumShield)
    {
        int previousShield = observedShield;
        observedShield = currentShield;

        if (changedPlayer != player ||
            previousShield <= 0 ||
            currentShield != 0 ||
            emergencyPlatingUsedThisWave ||
            waves == null ||
            !waves.IsWaveActive ||
            !RunUpgradeResolver.HasMechanic(
                RunUpgradeMechanic.EmergencyPlating,
                runtime
            ))
        {
            return;
        }

        emergencyPlatingUsedThisWave = true;
        player.GrantShield(EmergencyPlatingShield);
    }

    private void HandleBoardClearOutcomeResolved(BoardClearOutcome outcome)
    {
        if (energy == null ||
            player == null ||
            !player.IsInitialized ||
            player.IsDefeated ||
            outcome.ClearContext.Source != BoardClearSource.ColorCrystal ||
            outcome.ClearContext.GemCount <= 0 ||
            !RunUpgradeResolver.HasMechanic(
                RunUpgradeMechanic.ChromaticConductor,
                runtime
            ))
        {
            return;
        }

        energy.AddEnergy(outcome.ClearContext.GemCount);
    }

    private void HandleBeforeGemDamage(GemDamageContext context)
    {
        if (context == null ||
            player == null ||
            context.Player != player ||
            !(player.ActiveAbility is CrackedGemsAbilityDefinition cracked) ||
            context.ClearSource != BoardClearSource.Ability ||
            !context.ClearContext.GrantsSpecialEnergy ||
            context.GemCount != 1 ||
            context.OriginalDamage != cracked.CrackedGemDamage)
        {
            return;
        }

        context.Damage =
            RunUpgradeResolver.ResolveCrackedGemDamage(
                context.Damage,
                cracked,
                runtime
            );

        if (!RunUpgradeResolver.HasMechanic(
                RunUpgradeMechanic.ResonantCracks,
                runtime
            ))
        {
            return;
        }

        resonantCrackCount++;

        if (resonantCrackCount >= ResonantCracksFrequency)
        {
            resonantCrackCount = 0;

            if (energy != null)
            {
                energy.AddEnergy(ResonantCracksEnergy);
            }
        }
    }

    private bool CheckAnyPoisonedEnemy()
    {
        if (waves == null)
        {
            return false;
        }

        for (int index = 0; index < waves.ActiveEnemies.Count; index++)
        {
            EnemyActor enemy = waves.ActiveEnemies[index];

            if (enemy == null || enemy.IsDefeated)
            {
                continue;
            }

            EnemyPoisonStatus poison =
                enemy.GetComponent<EnemyPoisonStatus>();

            if (poison != null && poison.IsPoisoned)
            {
                return true;
            }
        }

        return false;
    }
}
