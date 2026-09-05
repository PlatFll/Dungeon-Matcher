using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemySpecialAbilityRuntime
{
    void InitializeSpecialAbility(
        EnemyActor enemyActor,
        BoardController boardController,
        IReadOnlyList<EnemyActor> activeEnemies
    );
}

internal sealed class EnemySpecialActionAvailability
{
    private readonly MonoBehaviour coroutineHost;
    private readonly EnemyActor enemyActor;
    private readonly BoardController boardController;
    private readonly EnemyStagger enemyStagger;
    private readonly Func<bool> tryExecuteReadyAbility;

    private Coroutine boardIdleRetryCoroutine;
    private bool wasDeferredByStagger;
    private bool isDisposed;

    public EnemySpecialActionAvailability(
        MonoBehaviour initializedCoroutineHost,
        EnemyActor initializedEnemyActor,
        BoardController initializedBoardController,
        Func<bool> initializedTryExecuteReadyAbility)
    {
        coroutineHost = initializedCoroutineHost;
        enemyActor = initializedEnemyActor;
        boardController = initializedBoardController;
        tryExecuteReadyAbility =
            initializedTryExecuteReadyAbility;

        enemyStagger =
            enemyActor != null
                ? enemyActor.GetComponent<EnemyStagger>()
                : null;

        if (enemyStagger != null)
        {
            enemyStagger.StaggerEnded +=
                HandleStaggerEnded;
        }
    }

    public void RequestExecution()
    {
        if (!CanRetryReadyAbility())
        {
            wasDeferredByStagger = false;
            CancelBoardIdleRetry();
            return;
        }

        if (enemyStagger != null &&
            enemyStagger.IsStaggered)
        {
            /*
             * Readiness is retained by EnemyActor. Do not claim an animation
             * action or enqueue board work while stagger prevents the enemy
             * from beginning a new action.
             */
            wasDeferredByStagger = true;
            CancelBoardIdleRetry();
            return;
        }

        if (wasDeferredByStagger &&
            boardController.IsBusy)
        {
            EnsureBoardIdleRetry();
            return;
        }

        CancelBoardIdleRetry();

        bool executed =
            tryExecuteReadyAbility != null &&
            tryExecuteReadyAbility();

        if (executed ||
            !CanRetryReadyAbility())
        {
            wasDeferredByStagger = false;
        }
    }

    public void Dispose()
    {
        if (isDisposed)
        {
            return;
        }

        isDisposed = true;

        if (enemyStagger != null)
        {
            enemyStagger.StaggerEnded -=
                HandleStaggerEnded;
        }

        CancelBoardIdleRetry();
    }

    private void HandleStaggerEnded(
        EnemyStagger endedStagger)
    {
        if (!CanRetryReadyAbility())
        {
            return;
        }

        wasDeferredByStagger = true;

        /*
         * Retry on the following frame so every StaggerEnded observer can
         * finish (including presentation resume) before a new action begins.
         * The coroutine also waits out any player resolution already in
         * progress without creating a pending board mutation.
         */
        EnsureBoardIdleRetry();
    }

    private void EnsureBoardIdleRetry()
    {
        if (boardIdleRetryCoroutine != null ||
            coroutineHost == null ||
            !coroutineHost.isActiveAndEnabled)
        {
            return;
        }

        boardIdleRetryCoroutine =
            coroutineHost.StartCoroutine(
                RetryAfterBoardBecomesIdle()
            );
    }

    private IEnumerator RetryAfterBoardBecomesIdle()
    {
        yield return null;

        while (CanRetryReadyAbility() &&
               wasDeferredByStagger)
        {
            if (enemyStagger != null &&
                enemyStagger.IsStaggered)
            {
                boardIdleRetryCoroutine = null;
                yield break;
            }

            if (!boardController.IsBusy)
            {
                boardIdleRetryCoroutine = null;
                RequestExecution();
                yield break;
            }

            yield return null;
        }

        boardIdleRetryCoroutine = null;
    }

    private bool CanRetryReadyAbility()
    {
        return
            !isDisposed &&
            coroutineHost != null &&
            enemyActor != null &&
            boardController != null &&
            enemyActor.IsInitialized &&
            !enemyActor.IsDefeated &&
            enemyActor.IsSpecialReady;
    }

    private void CancelBoardIdleRetry()
    {
        if (boardIdleRetryCoroutine == null)
        {
            return;
        }

        if (coroutineHost != null)
        {
            coroutineHost.StopCoroutine(
                boardIdleRetryCoroutine
            );
        }

        boardIdleRetryCoroutine = null;
    }
}
