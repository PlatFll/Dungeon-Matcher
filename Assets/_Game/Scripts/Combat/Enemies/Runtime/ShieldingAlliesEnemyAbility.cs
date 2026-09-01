using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(EnemyActor))]
public sealed class ShieldingAlliesEnemyAbility :
    MonoBehaviour,
    IEnemySpecialAbilityRuntime
{
    private EnemyActor enemyActor;
    private BoardController boardController;
    private IReadOnlyList<EnemyActor> activeEnemies;
    private EnemySpecialActionAvailability
        specialActionAvailability;

    private Coroutine readyAbilityCoroutine;
    private bool isAttemptingReadyAbility;

    public void InitializeSpecialAbility(
        EnemyActor initializedEnemy,
        BoardController initializedBoard,
        IReadOnlyList<EnemyActor> initializedActiveEnemies)
    {
        CancelReadyAbilityCoroutine();
        specialActionAvailability?.Dispose();
        Unsubscribe();

        enemyActor = initializedEnemy;
        boardController = initializedBoard;
        activeEnemies = initializedActiveEnemies;
        isAttemptingReadyAbility = false;

        if (enemyActor == null ||
            boardController == null ||
            activeEnemies == null ||
            enemyActor.Definition == null)
        {
            Debug.LogError(
                "ShieldingAlliesEnemyAbility requires an initialized " +
                "EnemyActor, BoardController, active-enemy roster, and " +
                "EnemyDefinition.",
                this
            );

            return;
        }

        specialActionAvailability =
            new EnemySpecialActionAvailability(
                this,
                enemyActor,
                boardController,
                TryUseReadyAbility
            );

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

        enemyActor.AnimationActionReleased -=
            HandleAnimationActionReleased;

        enemyActor.AnimationActionReleased +=
            HandleAnimationActionReleased;

        enemyActor.Defeated -=
            HandleEnemyDefeated;

        enemyActor.Defeated +=
            HandleEnemyDefeated;
    }

    private void Unsubscribe()
    {
        if (enemyActor == null)
        {
            return;
        }

        enemyActor.SpecialBecameReady -=
            HandleSpecialBecameReady;

        enemyActor.AnimationActionReleased -=
            HandleAnimationActionReleased;

        enemyActor.Defeated -=
            HandleEnemyDefeated;
    }

    private void HandleSpecialBecameReady(
        EnemyActor readyEnemy)
    {
        if (readyEnemy == null ||
            readyEnemy != enemyActor ||
            readyEnemy.IsDefeated)
        {
            return;
        }

        specialActionAvailability?.RequestExecution();
    }

    private void HandleAnimationActionReleased(
        EnemyActor releasedEnemy)
    {
        if (releasedEnemy == null ||
            releasedEnemy != enemyActor ||
            releasedEnemy.IsDefeated ||
            !releasedEnemy.IsSpecialReady)
        {
            return;
        }

        specialActionAvailability?.RequestExecution();
    }

    private bool TryUseReadyAbility()
    {
        if (isAttemptingReadyAbility ||
            enemyActor == null ||
            boardController == null ||
            activeEnemies == null ||
            enemyActor.Definition == null ||
            enemyActor.IsDefeated ||
            !enemyActor.IsSpecialReady)
        {
            return false;
        }

        if (boardController.IsBusy ||
            enemyActor.HasAnimationActionInProgress)
        {
            EnsureReadyAbilityCoroutine();
            return false;
        }

        CancelReadyAbilityCoroutine();

        if (!enemyActor
                .TryBeginSpecialAbilityAnimationAction())
        {
            EnsureReadyAbilityCoroutine();
            return false;
        }

        isAttemptingReadyAbility = true;

        try
        {
            EnemyDefinition definition =
                enemyActor.Definition;

            enemyActor.NotifySpecialAbilityUsed();

            GrantShieldToLivingEnemies(
                definition.AllyShieldAmount,
                definition.SelfShieldAmount
            );

            /*
             * A cast is successful even when every target is already capped.
             * Reset based on the accepted action, not on the actual amount
             * granted by any individual EnemyActor.
             */
            enemyActor.ResetSpecialCounter();
            return true;
        }
        finally
        {
            enemyActor.EndSpecialAbilityAnimationAction();
            isAttemptingReadyAbility = false;

            if (enemyActor.IsSpecialReady)
            {
                EnsureReadyAbilityCoroutine();
            }
        }
    }

    private void GrantShieldToLivingEnemies(
        int allyShieldAmount,
        int selfShieldAmount)
    {
        for (int index = 0;
             index < activeEnemies.Count;
             index++)
        {
            EnemyActor target =
                activeEnemies[index];

            if (target == null ||
                target == enemyActor ||
                !target.IsInitialized ||
                target.IsDefeated)
            {
                continue;
            }

            target.GrantShield(
                allyShieldAmount
            );
        }

        enemyActor.GrantShield(
            selfShieldAmount
        );
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
                !enemyActor.HasAnimationActionInProgress)
            {
                readyAbilityCoroutine = null;
                specialActionAvailability
                    ?.RequestExecution();
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

    private void HandleEnemyDefeated(
        EnemyActor defeatedEnemy)
    {
        CancelReadyAbilityCoroutine();
        specialActionAvailability?.Dispose();
        Unsubscribe();
    }

    private void OnDestroy()
    {
        CancelReadyAbilityCoroutine();
        specialActionAvailability?.Dispose();

        if (enemyActor != null)
        {
            enemyActor.EndSpecialAbilityAnimationAction();
        }

        Unsubscribe();
    }
}
