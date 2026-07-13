using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(EnemyActor))]
public sealed class EnemyStagger : MonoBehaviour
{
    [Header("Runtime Debug Information")]
    [SerializeField]
    private float remainingStaggerTime;

    [SerializeField]
    private bool isStaggered;

    private EnemyActor enemyActor;

    public event Action<
        EnemyStagger,
        float,
        float
    > StaggerApplied;

    public event Action<EnemyStagger>
        StaggerEnded;

    public EnemyActor EnemyActor =>
        enemyActor;

    public float RemainingStaggerTime =>
        remainingStaggerTime;

    public bool IsStaggered =>
        isStaggered &&
        remainingStaggerTime > 0f;

    private void Awake()
    {
        enemyActor =
            GetComponent<EnemyActor>();
    }

    private void OnEnable()
    {
        if (enemyActor == null)
        {
            enemyActor =
                GetComponent<EnemyActor>();
        }

        if (enemyActor != null)
        {
            enemyActor.Defeated -=
                HandleEnemyDefeated;

            enemyActor.Defeated +=
                HandleEnemyDefeated;
        }
    }

    private void Update()
    {
        if (!IsStaggered)
        {
            return;
        }

        if (enemyActor == null ||
            enemyActor.IsDefeated)
        {
            ClearStagger();
            return;
        }

        remainingStaggerTime -=
            Time.deltaTime;

        if (remainingStaggerTime <= 0f)
        {
            FinishStagger();
        }
    }

    public float ApplyStagger(
        float durationToAdd,
        float maximumStoredDuration)
    {
        if (enemyActor == null ||
            !enemyActor.IsInitialized ||
            enemyActor.IsDefeated ||
            enemyActor.Definition == null ||
            durationToAdd <= 0f ||
            maximumStoredDuration <= 0f)
        {
            return 0f;
        }

        float resistanceMultiplier =
            Mathf.Max(
                0f,
                enemyActor.Definition
                    .StaggerDurationMultiplier
            );

        if (resistanceMultiplier <= 0f)
        {
            return 0f;
        }

        float scaledDurationToAdd =
            durationToAdd *
            resistanceMultiplier;

        float scaledMaximumDuration =
            maximumStoredDuration *
            resistanceMultiplier;

        float previousRemainingTime =
            remainingStaggerTime;

        remainingStaggerTime =
            Mathf.Min(
                scaledMaximumDuration,
                remainingStaggerTime +
                scaledDurationToAdd
            );

        float actualAddedTime =
            remainingStaggerTime -
            previousRemainingTime;

        if (actualAddedTime <= 0f)
        {
            return 0f;
        }

        isStaggered = true;

        StaggerApplied?.Invoke(
            this,
            actualAddedTime,
            remainingStaggerTime
        );

        return actualAddedTime;
    }

    public void ClearStagger()
    {
        bool wasStaggered =
            IsStaggered;

        remainingStaggerTime = 0f;
        isStaggered = false;

        if (wasStaggered)
        {
            StaggerEnded?.Invoke(this);
        }
    }

    private void FinishStagger()
    {
        remainingStaggerTime = 0f;
        isStaggered = false;

        StaggerEnded?.Invoke(this);
    }

    private void HandleEnemyDefeated(
        EnemyActor enemy)
    {
        ClearStagger();
    }

    private void OnDisable()
    {
        if (enemyActor != null)
        {
            enemyActor.Defeated -=
                HandleEnemyDefeated;
        }

        remainingStaggerTime = 0f;
        isStaggered = false;
    }
}