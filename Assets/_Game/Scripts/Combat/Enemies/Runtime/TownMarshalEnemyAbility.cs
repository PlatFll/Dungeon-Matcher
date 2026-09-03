using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(EnemyActor))]
public sealed class TownMarshalEnemyAbility :
    MonoBehaviour,
    IEnemySpecialAbilityRuntime
{
    private const float RetreatVisualScale = 0.88f;
    private const float RetreatVisualYOffset = 12f;
    private const float RetreatVisualBrightness = 0.65f;

    private enum PreferredAbility
    {
        RingTheBell,
        CitizensSeizeHim
    }

    private EnemyActor enemyActor;
    private BoardController boardController;
    private IReadOnlyList<EnemyActor> activeEnemies;
    private IEnemySummonService summonService;

    private PreferredAbility preferredAbility =
        PreferredAbility.RingTheBell;

    private EnemyActor currentProtector;
    private int retreatMovesRemaining;

    private Coroutine readyAbilityCoroutine;
    private Coroutine rallyCoroutine;
    private bool isAttemptingReadyAbility;
    private bool isRallyActive;

    private readonly List<EnemyAutoAttack>
        ralliedAutoAttacks =
            new List<EnemyAutoAttack>();

    private Transform visualRoot;
    private Image visualImage;
    private Vector3 normalVisualPosition;
    private Vector3 normalVisualScale;
    private Color normalVisualColor = Color.white;
    private bool visualStateCaptured;

    public void ConfigureSummonService(
        IEnemySummonService configuredSummonService)
    {
        summonService = configuredSummonService;
    }

    public void InitializeSpecialAbility(
        EnemyActor initializedEnemy,
        BoardController initializedBoard,
        IReadOnlyList<EnemyActor> initializedActiveEnemies)
    {
        CancelReadyAbilityCoroutine();
        StopRally();
        EndRetreat();
        Unsubscribe();

        enemyActor = initializedEnemy;
        boardController = initializedBoard;
        activeEnemies = initializedActiveEnemies;
        preferredAbility = PreferredAbility.RingTheBell;
        isAttemptingReadyAbility = false;

        if (enemyActor == null ||
            boardController == null ||
            activeEnemies == null ||
            summonService == null ||
            enemyActor.Definition == null)
        {
            Debug.LogError(
                "TownMarshalEnemyAbility requires an initialized EnemyActor, " +
                "BoardController, active-enemy roster, summon service, and " +
                "EnemyDefinition.",
                this
            );

            return;
        }

        CaptureVisualState();
        Subscribe();
    }

    private void Subscribe()
    {
        if (enemyActor == null)
        {
            return;
        }

        enemyActor.SpecialBecameReady -=
            HandleSpecialBecameReady;
        enemyActor.SpecialBecameReady +=
            HandleSpecialBecameReady;

        enemyActor.SpecialCounterChanged -=
            HandleSpecialCounterChanged;
        enemyActor.SpecialCounterChanged +=
            HandleSpecialCounterChanged;

        enemyActor.AnimationActionReleased -=
            HandleAnimationActionReleased;
        enemyActor.AnimationActionReleased +=
            HandleAnimationActionReleased;

        enemyActor.Defeated -=
            HandleMarshalDefeated;
        enemyActor.Defeated +=
            HandleMarshalDefeated;
    }

    private void Unsubscribe()
    {
        if (enemyActor == null)
        {
            return;
        }

        enemyActor.SpecialBecameReady -=
            HandleSpecialBecameReady;
        enemyActor.SpecialCounterChanged -=
            HandleSpecialCounterChanged;
        enemyActor.AnimationActionReleased -=
            HandleAnimationActionReleased;
        enemyActor.Defeated -=
            HandleMarshalDefeated;
    }

    private void HandleSpecialBecameReady(
        EnemyActor readyEnemy)
    {
        if (readyEnemy != enemyActor ||
            readyEnemy == null ||
            readyEnemy.IsDefeated)
        {
            return;
        }

        TryUseReadyAbility();
    }

    private void HandleAnimationActionReleased(
        EnemyActor releasedEnemy)
    {
        if (releasedEnemy == enemyActor &&
            releasedEnemy != null &&
            !releasedEnemy.IsDefeated &&
            releasedEnemy.IsSpecialReady)
        {
            TryUseReadyAbility();
        }
    }

    private void HandleSpecialCounterChanged(
        EnemyActor changedEnemy,
        int currentCount,
        int requiredCount)
    {
        if (changedEnemy != enemyActor ||
            currentProtector == null ||
            retreatMovesRemaining <= 0 ||
            currentCount <= 0)
        {
            return;
        }

        retreatMovesRemaining--;

        if (retreatMovesRemaining <= 0)
        {
            EndRetreat();
        }
    }

    private void TryUseReadyAbility()
    {
        if (isAttemptingReadyAbility ||
            enemyActor == null ||
            boardController == null ||
            enemyActor.Definition == null ||
            enemyActor.IsDefeated ||
            !enemyActor.IsSpecialReady)
        {
            return;
        }

        if (boardController.IsBusy ||
            enemyActor.HasAnimationActionInProgress ||
            !CanUseAnyAbility())
        {
            EnsureReadyAbilityCoroutine();
            return;
        }

        CancelReadyAbilityCoroutine();

        if (!enemyActor
                .TryBeginSpecialAbilityAnimationAction())
        {
            EnsureReadyAbilityCoroutine();
            return;
        }

        isAttemptingReadyAbility = true;
        bool abilityUsed = false;

        try
        {
            if (preferredAbility ==
                PreferredAbility.RingTheBell)
            {
                abilityUsed =
                    TryUseRingTheBell();

                if (!abilityUsed)
                {
                    abilityUsed =
                        TryUseCitizensSeizeHim();
                }
            }
            else
            {
                abilityUsed =
                    TryUseCitizensSeizeHim();

                if (!abilityUsed)
                {
                    abilityUsed =
                        TryUseRingTheBell();
                }
            }

            if (abilityUsed)
            {
                enemyActor.NotifySpecialAbilityUsed();
                enemyActor.ResetSpecialCounter();
            }
        }
        finally
        {
            enemyActor.EndSpecialAbilityAnimationAction();
            isAttemptingReadyAbility = false;

            if (enemyActor != null &&
                !enemyActor.IsDefeated &&
                enemyActor.IsSpecialReady)
            {
                EnsureReadyAbilityCoroutine();
            }
        }
    }

    private bool TryUseRingTheBell()
    {
        if (!CanUseRingTheBell())
        {
            return false;
        }

        EnemyDefinition[] candidates =
            enemyActor.Definition
                .TownMarshalSummonCandidates;

        int startIndex =
            Random.Range(
                0,
                candidates.Length
            );

        for (int offset = 0;
             offset < candidates.Length;
             offset++)
        {
            int candidateIndex =
                (startIndex + offset) %
                candidates.Length;

            EnemyDefinition candidate =
                candidates[candidateIndex];

            if (candidate == null)
            {
                continue;
            }

            if (!summonService.TrySummonEnemy(
                    candidate,
                    out EnemyActor summonedEnemy))
            {
                continue;
            }

            BeginRetreat(
                summonedEnemy
            );

            preferredAbility =
                PreferredAbility.CitizensSeizeHim;

            Debug.Log(
                $"{enemyActor.Definition.DisplayName} used Ring the Bell " +
                $"and summoned {candidate.DisplayName}.",
                this
            );

            return true;
        }

        return false;
    }

    private bool TryUseCitizensSeizeHim()
    {
        if (!CanUseCitizensSeizeHim())
        {
            return false;
        }

        float speedMultiplier =
            enemyActor.Definition
                .TownMarshalRallyAttackSpeedMultiplier;

        float duration =
            enemyActor.Definition
                .TownMarshalRallyDuration;

        ralliedAutoAttacks.Clear();

        for (int index = 0;
             index < activeEnemies.Count;
             index++)
        {
            EnemyActor ally =
                activeEnemies[index];

            if (!IsLivingLocalAlly(ally))
            {
                continue;
            }

            EnemyAutoAttack autoAttack =
                ally.GetComponent<EnemyAutoAttack>();

            if (autoAttack == null)
            {
                continue;
            }

            autoAttack.SetRuntimeAttackSpeedMultiplier(
                speedMultiplier
            );

            ralliedAutoAttacks.Add(
                autoAttack
            );
        }

        if (ralliedAutoAttacks.Count == 0)
        {
            return false;
        }

        isRallyActive = true;
        rallyCoroutine =
            StartCoroutine(
                RallyDurationRoutine(
                    duration,
                    speedMultiplier
                )
            );

        preferredAbility =
            PreferredAbility.RingTheBell;

        Debug.Log(
            $"{enemyActor.Definition.DisplayName} used Citizens, Seize Him! " +
            $"on {ralliedAutoAttacks.Count} local allies.",
            this
        );

        return true;
    }

    private bool CanUseAnyAbility()
    {
        return CanUseRingTheBell() ||
               CanUseCitizensSeizeHim();
    }

    private bool CanUseRingTheBell()
    {
        if (summonService == null ||
            !summonService.HasFreeEnemySlot ||
            enemyActor == null ||
            enemyActor.Definition == null)
        {
            return false;
        }

        EnemyDefinition[] candidates =
            enemyActor.Definition
                .TownMarshalSummonCandidates;

        if (candidates == null ||
            candidates.Length == 0)
        {
            return false;
        }

        for (int index = 0;
             index < candidates.Length;
             index++)
        {
            if (candidates[index] != null)
            {
                return true;
            }
        }

        return false;
    }

    private bool CanUseCitizensSeizeHim()
    {
        if (isRallyActive ||
            activeEnemies == null)
        {
            return false;
        }

        for (int index = 0;
             index < activeEnemies.Count;
             index++)
        {
            if (IsLivingLocalAlly(
                    activeEnemies[index]))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsLivingLocalAlly(
        EnemyActor candidate)
    {
        if (candidate == null ||
            candidate == enemyActor ||
            !candidate.IsInitialized ||
            candidate.IsDefeated ||
            candidate.Definition == null ||
            enemyActor == null ||
            enemyActor.Definition == null)
        {
            return false;
        }

        EnemyDefinition[] localDefinitions =
            enemyActor.Definition
                .TownMarshalSummonCandidates;

        if (localDefinitions == null)
        {
            return false;
        }

        for (int index = 0;
             index < localDefinitions.Length;
             index++)
        {
            if (localDefinitions[index] ==
                candidate.Definition)
            {
                return true;
            }
        }

        return false;
    }

    private void BeginRetreat(
        EnemyActor protector)
    {
        EndRetreat();

        if (protector == null ||
            protector.IsDefeated ||
            enemyActor == null ||
            enemyActor.Definition == null)
        {
            return;
        }

        int retreatMoves =
            Mathf.Max(
                1,
                enemyActor.Definition
                    .TownMarshalRetreatMoveCount
            );

        if (!enemyActor.SetDamageRedirectTarget(
                protector))
        {
            return;
        }

        currentProtector = protector;
        retreatMovesRemaining = retreatMoves;

        currentProtector.Defeated -=
            HandleProtectorDefeated;
        currentProtector.Defeated +=
            HandleProtectorDefeated;

        ApplyRetreatVisual(
            true
        );
    }

    private void EndRetreat()
    {
        if (currentProtector != null)
        {
            currentProtector.Defeated -=
                HandleProtectorDefeated;
        }

        if (enemyActor != null)
        {
            enemyActor.ClearDamageRedirectTarget(
                currentProtector
            );
        }

        currentProtector = null;
        retreatMovesRemaining = 0;

        ApplyRetreatVisual(
            false
        );
    }

    private void HandleProtectorDefeated(
        EnemyActor defeatedProtector)
    {
        if (defeatedProtector ==
            currentProtector)
        {
            EndRetreat();
        }
    }

    private IEnumerator RallyDurationRoutine(
        float duration,
        float appliedMultiplier)
    {
        yield return
            new WaitForSeconds(
                Mathf.Max(
                    0.1f,
                    duration
                )
            );

        ClearRallyAttackSpeed(
            appliedMultiplier
        );

        isRallyActive = false;
        rallyCoroutine = null;

        if (enemyActor != null &&
            !enemyActor.IsDefeated &&
            enemyActor.IsSpecialReady)
        {
            TryUseReadyAbility();
        }
    }

    private void StopRally()
    {
        float expectedMultiplier =
            enemyActor != null &&
            enemyActor.Definition != null
                ? enemyActor.Definition
                    .TownMarshalRallyAttackSpeedMultiplier
                : -1f;

        if (rallyCoroutine != null)
        {
            StopCoroutine(
                rallyCoroutine
            );

            rallyCoroutine = null;
        }

        ClearRallyAttackSpeed(
            expectedMultiplier
        );

        isRallyActive = false;
    }

    private void ClearRallyAttackSpeed(
        float expectedMultiplier)
    {
        for (int index = 0;
             index < ralliedAutoAttacks.Count;
             index++)
        {
            EnemyAutoAttack autoAttack =
                ralliedAutoAttacks[index];

            if (autoAttack == null)
            {
                continue;
            }

            if (expectedMultiplier <= 0f ||
                Mathf.Approximately(
                    autoAttack.RuntimeAttackSpeedMultiplier,
                    expectedMultiplier
                ))
            {
                autoAttack.ResetRuntimeAttackSpeedMultiplier();
            }
        }

        ralliedAutoAttacks.Clear();
    }

    private void EnsureReadyAbilityCoroutine()
    {
        if (readyAbilityCoroutine != null ||
            !isActiveAndEnabled)
        {
            return;
        }

        readyAbilityCoroutine =
            StartCoroutine(
                WaitUntilReadyAbilityCanExecute()
            );
    }

    private IEnumerator
        WaitUntilReadyAbilityCanExecute()
    {
        while (enemyActor != null &&
               boardController != null &&
               !enemyActor.IsDefeated &&
               enemyActor.IsSpecialReady)
        {
            if (!boardController.IsBusy &&
                !enemyActor.HasAnimationActionInProgress &&
                CanUseAnyAbility())
            {
                readyAbilityCoroutine = null;
                TryUseReadyAbility();
                yield break;
            }

            yield return null;
        }

        readyAbilityCoroutine = null;
    }

    private void CancelReadyAbilityCoroutine()
    {
        if (readyAbilityCoroutine == null)
        {
            return;
        }

        StopCoroutine(
            readyAbilityCoroutine
        );

        readyAbilityCoroutine = null;
    }

    private void CaptureVisualState()
    {
        visualStateCaptured = false;
        visualRoot = null;
        visualImage = null;

        if (enemyActor == null)
        {
            return;
        }

        visualRoot =
            enemyActor.transform.Find(
                "VisualRoot"
            );

        if (visualRoot == null)
        {
            Image fallbackImage =
                enemyActor.GetComponentInChildren<Image>();

            if (fallbackImage != null)
            {
                visualRoot =
                    fallbackImage.transform;
            }
        }

        if (visualRoot == null)
        {
            return;
        }

        visualImage =
            visualRoot.GetComponent<Image>();

        normalVisualPosition =
            visualRoot.localPosition;

        normalVisualScale =
            visualRoot.localScale;

        if (visualImage != null)
        {
            normalVisualColor =
                visualImage.color;
        }

        visualStateCaptured = true;
    }

    private void ApplyRetreatVisual(
        bool retreated)
    {
        if (!visualStateCaptured ||
            visualRoot == null)
        {
            return;
        }

        visualRoot.localPosition =
            retreated
                ? normalVisualPosition +
                  Vector3.up *
                  RetreatVisualYOffset
                : normalVisualPosition;

        visualRoot.localScale =
            retreated
                ? normalVisualScale *
                  RetreatVisualScale
                : normalVisualScale;

        if (visualImage != null)
        {
            visualImage.color =
                retreated
                    ? new Color(
                        normalVisualColor.r *
                            RetreatVisualBrightness,
                        normalVisualColor.g *
                            RetreatVisualBrightness,
                        normalVisualColor.b *
                            RetreatVisualBrightness,
                        normalVisualColor.a
                    )
                    : normalVisualColor;
        }
    }

    private void HandleMarshalDefeated(
        EnemyActor defeatedEnemy)
    {
        CancelReadyAbilityCoroutine();
        EndRetreat();
        StopRally();
        Unsubscribe();
    }

    private void OnDestroy()
    {
        CancelReadyAbilityCoroutine();
        EndRetreat();
        StopRally();

        if (enemyActor != null)
        {
            enemyActor.EndSpecialAbilityAnimationAction();
        }

        Unsubscribe();
    }
}
