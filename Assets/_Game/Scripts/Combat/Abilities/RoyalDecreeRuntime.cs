using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class RoyalDecreeRuntime :
    MonoBehaviour,
    IPlayerAbilityRuntime
{
    [Header("References")]
    [SerializeField]
    private BoardController boardController;

    [SerializeField]
    private WaveController waveController;

    private RoyalDecreeAbilityDefinition activeDefinition;
    private EnemyActor currentTarget;
    private Coroutine durationCoroutine;
    private float abilityEndTime;

    public event Action StateChanged;

    public event Action<EnemyActor>
        TargetChanged;

    public event Action<
        EnemyActor,
        int,
        BoardMatchContext
    > HitResolved;

    public bool IsActive { get; private set; }

    public EnemyActor CurrentTarget =>
        currentTarget;

    public float RemainingDuration =>
        IsActive
            ? Mathf.Max(
                0f,
                abilityEndTime - Time.time
            )
            : 0f;

    private void OnEnable()
    {
        ResolveReferences();
        Subscribe();
    }

    private void Start()
    {
        if (!ValidateReferences())
        {
            enabled = false;
        }
    }

    private void OnDisable()
    {
        Cancel();
        Unsubscribe();
    }

    public bool Supports(
        CharacterAbilityDefinition definition)
    {
        return definition is
            RoyalDecreeAbilityDefinition;
    }

    public bool CanActivate(
        CharacterAbilityDefinition definition)
    {
        return
            !IsActive &&
            definition is
                RoyalDecreeAbilityDefinition &&
            boardController != null &&
            waveController != null &&
            waveController.IsWaveActive &&
            HasAliveEnemy();
    }

    public bool TryActivate(
        CharacterAbilityDefinition definition)
    {
        if (!(definition is
                RoyalDecreeAbilityDefinition
                royalDecreeDefinition) ||
            !CanActivate(definition))
        {
            return false;
        }

        EnemyActor selectedTarget =
            FindRandomAliveEnemy();

        if (selectedTarget == null)
        {
            return false;
        }

        activeDefinition =
            royalDecreeDefinition;

        IsActive = true;

        abilityEndTime =
            Time.time +
            activeDefinition.Duration;

        SetTarget(selectedTarget);

        if (durationCoroutine != null)
        {
            StopCoroutine(
                durationCoroutine
            );
        }

        durationCoroutine =
            StartCoroutine(
                EndAfterDuration(
                    activeDefinition.Duration
                )
            );

        StateChanged?.Invoke();

        return true;
    }

    public void Cancel()
    {
        if (durationCoroutine != null)
        {
            StopCoroutine(
                durationCoroutine
            );

            durationCoroutine = null;
        }

        FinishAbility();
    }

    private IEnumerator EndAfterDuration(
        float duration)
    {
        yield return new WaitForSeconds(
            Mathf.Max(
                0.1f,
                duration
            )
        );

        durationCoroutine = null;

        FinishAbility();
    }

    private void FinishAbility()
    {
        bool wasActive =
            IsActive;

        IsActive = false;
        abilityEndTime = 0f;
        activeDefinition = null;

        SetTarget(null);

        if (wasActive)
        {
            StateChanged?.Invoke();
        }
    }

    private void Subscribe()
    {
        if (boardController != null)
        {
            boardController.BoardMatchResolved -=
                HandleBoardMatchResolved;

            boardController.BoardMatchResolved +=
                HandleBoardMatchResolved;
        }

        if (waveController != null)
        {
            waveController.EnemySpawned -=
                HandleEnemySpawned;

            waveController.EnemySpawned +=
                HandleEnemySpawned;

            waveController.WaveStarted -=
                HandleWaveStarted;

            waveController.WaveStarted +=
                HandleWaveStarted;

            waveController.WaveCompleted -=
                HandleWaveCompleted;

            waveController.WaveCompleted +=
                HandleWaveCompleted;
        }
    }

    private void Unsubscribe()
    {
        if (boardController != null)
        {
            boardController.BoardMatchResolved -=
                HandleBoardMatchResolved;
        }

        if (waveController != null)
        {
            waveController.EnemySpawned -=
                HandleEnemySpawned;

            waveController.WaveStarted -=
                HandleWaveStarted;

            waveController.WaveCompleted -=
                HandleWaveCompleted;
        }
    }

    private void HandleBoardMatchResolved(
        BoardMatchContext context)
    {
        if (!IsActive ||
            activeDefinition == null)
        {
            return;
        }

        EnsureValidTarget();

        if (currentTarget == null)
        {
            return;
        }

        int requestedDamage =
            activeDefinition.CalculateDamage(
                context
            );

        if (requestedDamage <= 0)
        {
            return;
        }

        EnemyActor damagedTarget =
            currentTarget;

        int healthBeforeDamage =
            damagedTarget.CurrentHealth;

        bool damageSucceeded =
            damagedTarget.TryTakeDamage(
                requestedDamage
            );

        if (!damageSucceeded)
        {
            return;
        }

        int actualDamage =
            Mathf.Max(
                0,
                healthBeforeDamage -
                damagedTarget.CurrentHealth
            );

        HitResolved?.Invoke(
            damagedTarget,
            actualDamage,
            context
        );
    }

    private void HandleWaveStarted(
    int startedWave)
    {
        StateChanged?.Invoke();
    }

    private void HandleWaveCompleted(
        int completedWave)
    {
        StateChanged?.Invoke();
    }

    private void HandleEnemySpawned(
        EnemyActor spawnedEnemy)
    {
        if (!IsActive ||
            currentTarget != null ||
            spawnedEnemy == null ||
            !spawnedEnemy.CanReceiveDamage)
        {
            return;
        }

        SetTarget(spawnedEnemy);
    }

    private void HandleTargetDefeated(
        EnemyActor defeatedEnemy)
    {
        if (defeatedEnemy != currentTarget)
        {
            return;
        }

        if (!IsActive)
        {
            SetTarget(null);
            return;
        }

        SetTarget(
            FindRandomAliveEnemy()
        );
    }

    private void EnsureValidTarget()
    {
        if (currentTarget != null &&
            currentTarget.CanReceiveDamage)
        {
            return;
        }

        SetTarget(
            FindRandomAliveEnemy()
        );
    }

    private void SetTarget(
        EnemyActor newTarget)
    {
        if (currentTarget == newTarget)
        {
            return;
        }

        if (currentTarget != null)
        {
            currentTarget.Defeated -=
                HandleTargetDefeated;
        }

        currentTarget = newTarget;

        if (currentTarget != null)
        {
            currentTarget.Defeated -=
                HandleTargetDefeated;

            currentTarget.Defeated +=
                HandleTargetDefeated;
        }

        TargetChanged?.Invoke(
            currentTarget
        );
    }

    private bool HasAliveEnemy()
    {
        if (waveController == null)
        {
            return false;
        }

        IReadOnlyList<EnemyActor> enemies =
            waveController.ActiveEnemies;

        for (int index = 0;
             index < enemies.Count;
             index++)
        {
            EnemyActor enemy =
                enemies[index];

            if (enemy != null &&
                enemy.CanReceiveDamage)
            {
                return true;
            }
        }

        return false;
    }

    private EnemyActor FindRandomAliveEnemy()
    {
        if (waveController == null)
        {
            return null;
        }

        IReadOnlyList<EnemyActor> enemies =
            waveController.ActiveEnemies;

        int aliveEnemyCount = 0;

        for (int index = 0;
             index < enemies.Count;
             index++)
        {
            EnemyActor enemy =
                enemies[index];

            if (enemy != null &&
                enemy.CanReceiveDamage)
            {
                aliveEnemyCount++;
            }
        }

        if (aliveEnemyCount == 0)
        {
            return null;
        }

        int selectedAliveIndex =
            UnityEngine.Random.Range(
                0,
                aliveEnemyCount
            );

        for (int index = 0;
             index < enemies.Count;
             index++)
        {
            EnemyActor enemy =
                enemies[index];

            if (enemy == null ||
                !enemy.CanReceiveDamage)
            {
                continue;
            }

            if (selectedAliveIndex == 0)
            {
                return enemy;
            }

            selectedAliveIndex--;
        }

        return null;
    }

    private void ResolveReferences()
    {
        if (waveController == null)
        {
            waveController =
                GetComponentInParent<
                    WaveController
                >();
        }
    }

    private bool ValidateReferences()
    {
        bool isValid = true;

        if (boardController == null)
        {
            Debug.LogError(
                "RoyalDecreeRuntime requires " +
                "a BoardController.",
                this
            );

            isValid = false;
        }

        if (waveController == null)
        {
            Debug.LogError(
                "RoyalDecreeRuntime requires " +
                "a WaveController.",
                this
            );

            isValid = false;
        }

        return isValid;
    }
}