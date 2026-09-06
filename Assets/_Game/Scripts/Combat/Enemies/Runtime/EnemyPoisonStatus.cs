using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(EnemyActor))]
public sealed class EnemyPoisonStatus : MonoBehaviour
{
    [Header("Runtime Debug Information")]
    [SerializeField]
    private bool isPoisoned;

    [SerializeField]
    private float remainingDuration;

    [SerializeField]
    private float remainingTimeUntilTick;

    [SerializeField]
    private int tickDamage;

    private EnemyActor enemyActor;
    private float expirationTime;
    private float nextTickTime;
    private float tickInterval = 1f;

    public event Action<EnemyPoisonStatus, bool> PoisonApplied;
    public event Action<EnemyPoisonStatus, int> TickDamageApplied;
    public event Action<EnemyPoisonStatus> PoisonExpired;

    public EnemyActor EnemyActor => enemyActor;
    public bool IsPoisoned =>
        isPoisoned &&
        enemyActor != null &&
        enemyActor.IsInitialized &&
        !enemyActor.IsDefeated;
    public float RemainingDuration =>
        IsPoisoned
            ? Mathf.Max(0f, expirationTime - Time.time)
            : 0f;

    private void Awake()
    {
        enemyActor = GetComponent<EnemyActor>();
    }

    private void OnEnable()
    {
        ResolveEnemyActor();
        Subscribe();
    }

    private void Update()
    {
        if (!IsPoisoned)
        {
            SyncDebugState();
            return;
        }

        float currentTime = Time.time;

        while (nextTickTime <= expirationTime + 0.0001f &&
               currentTime >= nextTickTime &&
               IsPoisoned)
        {
            ApplyTick();
            nextTickTime += tickInterval;
        }

        if (IsPoisoned && currentTime >= expirationTime)
        {
            ClearPoison(true);
            return;
        }

        SyncDebugState();
    }

    public void Apply(
        float duration,
        float interval,
        int damagePerTick)
    {
        ResolveEnemyActor();

        if (enemyActor == null ||
            !enemyActor.IsInitialized ||
            enemyActor.IsDefeated ||
            duration <= 0f ||
            interval <= 0f ||
            damagePerTick <= 0)
        {
            return;
        }

        bool wasAlreadyPoisoned = IsPoisoned;
        tickInterval = Mathf.Max(0.05f, interval);
        tickDamage = Mathf.Max(1, damagePerTick);
        expirationTime = Time.time + Mathf.Max(0.05f, duration);

        if (!wasAlreadyPoisoned)
        {
            nextTickTime = Time.time + tickInterval;
        }

        isPoisoned = true;
        SyncDebugState();
        PoisonApplied?.Invoke(this, wasAlreadyPoisoned);
    }

    public void ClearPoison()
    {
        ClearPoison(true);
    }

    private void ApplyTick()
    {
        if (enemyActor == null || enemyActor.IsDefeated || tickDamage <= 0)
        {
            return;
        }

        int resolvedTickDamage =
            RunUpgradeResolver.ResolveEnemyDamage(
                tickDamage,
                enemyActor
            );

        int healthBeforeDamage = enemyActor.CurrentHealth;

        bool damageApplied =
            enemyActor.TryTakeDamageWithoutFeedback(resolvedTickDamage);

        if (!damageApplied)
        {
            return;
        }

        int actualDamage = Mathf.Max(
            0,
            healthBeforeDamage - enemyActor.CurrentHealth
        );

        if (actualDamage <= 0)
        {
            return;
        }

        TickDamageApplied?.Invoke(this, actualDamage);
    }

    private void ResolveEnemyActor()
    {
        if (enemyActor == null)
        {
            enemyActor = GetComponent<EnemyActor>();
        }
    }

    private void Subscribe()
    {
        Unsubscribe();

        if (enemyActor != null)
        {
            enemyActor.Initialized += HandleEnemyInitialized;
            enemyActor.Defeated += HandleEnemyDefeated;
        }
    }

    private void Unsubscribe()
    {
        if (enemyActor != null)
        {
            enemyActor.Initialized -= HandleEnemyInitialized;
            enemyActor.Defeated -= HandleEnemyDefeated;
        }
    }

    private void HandleEnemyInitialized(EnemyActor enemy)
    {
        ClearPoison(false);
    }

    private void HandleEnemyDefeated(EnemyActor enemy)
    {
        ClearPoison(true);
    }

    private void ClearPoison(bool notify)
    {
        bool wasPoisoned = isPoisoned;
        isPoisoned = false;
        expirationTime = 0f;
        nextTickTime = 0f;
        remainingDuration = 0f;
        remainingTimeUntilTick = 0f;
        tickDamage = 0;

        if (notify && wasPoisoned)
        {
            PoisonExpired?.Invoke(this);
        }
    }

    private void SyncDebugState()
    {
        if (!isPoisoned)
        {
            remainingDuration = 0f;
            remainingTimeUntilTick = 0f;
            return;
        }

        remainingDuration = Mathf.Max(0f, expirationTime - Time.time);
        remainingTimeUntilTick = Mathf.Max(0f, nextTickTime - Time.time);
    }

    private void OnDisable()
    {
        Unsubscribe();
        ClearPoison(false);
    }
}
