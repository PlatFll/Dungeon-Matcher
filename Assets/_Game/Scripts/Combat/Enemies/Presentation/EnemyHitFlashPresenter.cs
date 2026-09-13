using UnityEngine;

/// <summary>
/// Presentation-only bridge for the new stagger buildup model. Every normal
/// feedback-producing hit produces one short solid-white flash; stagger itself
/// remains the only state that blinks repeatedly.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(EnemyActor))]
public sealed class EnemyHitFlashPresenter : MonoBehaviour
{
    [SerializeField, Min(0.01f)]
    [Tooltip("Duration of the single white flash produced by a normal hit.")]
    private float hitFlashDuration = 0.08f;

    private EnemyActor enemyActor;
    private EnemyCombatFeedback combatFeedback;

    public static bool EnsureInstalled(GameObject enemyObject)
    {
        if (enemyObject == null)
        {
            return false;
        }

        EnemyCombatFeedback feedback = enemyObject.GetComponent<EnemyCombatFeedback>();
        if (feedback == null)
        {
            return false;
        }

        EnemyHitFlashPresenter presenter = enemyObject.GetComponent<EnemyHitFlashPresenter>();
        if (presenter == null)
        {
            presenter = enemyObject.AddComponent<EnemyHitFlashPresenter>();
        }

        presenter.ResolveReferences();
        presenter.Subscribe();
        return true;
    }

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnEnable()
    {
        ResolveReferences();
        Subscribe();
    }

    private void ResolveReferences()
    {
        if (enemyActor == null)
        {
            enemyActor = GetComponent<EnemyActor>();
        }

        if (combatFeedback == null)
        {
            combatFeedback = GetComponent<EnemyCombatFeedback>();
        }
    }

    private void Subscribe()
    {
        Unsubscribe();

        if (enemyActor == null)
        {
            return;
        }

        enemyActor.DamageReceived += HandleDamage;
        enemyActor.ShieldDamaged += HandleDamage;
    }

    private void Unsubscribe()
    {
        if (enemyActor == null)
        {
            return;
        }

        enemyActor.DamageReceived -= HandleDamage;
        enemyActor.ShieldDamaged -= HandleDamage;
    }

    private void HandleDamage(EnemyActor enemy, int damageAmount)
    {
        if (enemy != enemyActor ||
            damageAmount <= 0 ||
            combatFeedback == null)
        {
            return;
        }

        combatFeedback.RefreshHitFlash(hitFlashDuration);
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void OnValidate()
    {
        hitFlashDuration = Mathf.Max(0.01f, hitFlashDuration);
    }
}