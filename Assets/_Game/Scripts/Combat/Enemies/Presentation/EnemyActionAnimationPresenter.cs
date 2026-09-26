using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(EnemyActor))]
[RequireComponent(typeof(EnemyAutoAttack))]
public sealed class EnemyActionAnimationPresenter : MonoBehaviour
{
    private const string VisualRootName = "VisualRoot";

    private static readonly int AutoAttackTrigger =
        Animator.StringToHash("AutoAttack");

    private static readonly int AbilityTrigger =
        Animator.StringToHash("Ability");

    [SerializeField]
    private Animator animator;

    private EnemyActor enemyActor;
    private EnemyAutoAttack enemyAutoAttack;
    private CharacterAnimationPlayback animationPlayback;
    private int queuedImpactPresentationId;
    private int abilityPresentationId, queuedAbilityImpactId;
    private bool abilityImpactDelivered;
    private float abilityStartedAt;

    private bool UsesAuthoredAbility => enemyActor != null && enemyActor.Definition != null &&
        enemyActor.Definition.UseAuthoredSpecialAbilityMotion;

    public static EnemyActionAnimationPresenter EnsureInstalled(
        GameObject enemyObject)
    {
        if (enemyObject == null)
        {
            return null;
        }

        EnemyActionAnimationPresenter existingPresenter =
            enemyObject.GetComponent<
                EnemyActionAnimationPresenter
            >();

        if (existingPresenter != null)
        {
            return existingPresenter;
        }

        return enemyObject.AddComponent<
            EnemyActionAnimationPresenter
        >();
    }

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnEnable()
    {
        ResolveReferences();

        if (enemyAutoAttack != null)
        {
            enemyAutoAttack.AttackStarted +=
                HandleAttackStarted;
        }

        if (enemyActor != null)
        {
            enemyActor.SpecialAbilityUsed +=
                HandleSpecialAbilityUsed;
        }

        if (animationPlayback != null)
        {
            animationPlayback.AutoAttackImpactReached +=
                HandleAutoAttackImpactReached;
            animationPlayback.AutoAttackCompleted += HandleAutoAttackCompleted;

            animationPlayback.AbilityImpactReached +=
                HandleAbilityImpactReached;
            animationPlayback.AbilityCompleted += HandleAbilityCompleted;
        }
    }

    private void OnDisable()
    {
        queuedImpactPresentationId = 0;
        queuedAbilityImpactId = 0;
        abilityPresentationId = 0;
        if (enemyAutoAttack != null)
        {
            enemyAutoAttack.AttackStarted -=
                HandleAttackStarted;
        }

        if (enemyActor != null)
        {
            enemyActor.SpecialAbilityUsed -=
                HandleSpecialAbilityUsed;
        }

        if (animationPlayback != null)
        {
            animationPlayback.AutoAttackImpactReached -=
                HandleAutoAttackImpactReached;
            animationPlayback.AutoAttackCompleted -= HandleAutoAttackCompleted;

            animationPlayback.AbilityImpactReached -=
                HandleAbilityImpactReached;
            animationPlayback.AbilityCompleted -= HandleAbilityCompleted;
        }
    }

    private void ResolveReferences()
    {
        if (enemyActor == null)
        {
            enemyActor =
                GetComponent<EnemyActor>();
        }

        if (enemyAutoAttack == null)
        {
            enemyAutoAttack =
                GetComponent<EnemyAutoAttack>();
        }

        Transform visualRoot =
            transform.Find(VisualRootName);

        if (visualRoot == null)
        {
            return;
        }

        if (animator == null)
        {
            animator =
                visualRoot.GetComponent<Animator>();
        }

        if (animationPlayback == null)
        {
            animationPlayback =
                visualRoot.GetComponent<
                    CharacterAnimationPlayback
                >();
        }
    }

    private void HandleAttackStarted(
        EnemyAutoAttack attack)
    {
        queuedImpactPresentationId = 0;
        // Restart the authored clip for this accepted hit. A cancelled prior
        // action must not donate its later impact event to a new sequence.
        int state = Animator.StringToHash("Base Layer.AutoAttack");
        if (enemyActor != null && enemyActor.Definition != null &&
            enemyActor.Definition.UseAuthoredAutoAttackMotion && animator != null &&
            animator.isActiveAndEnabled && animator.runtimeAnimatorController != null &&
            animator.HasState(0, state))
        {
            animator.ResetTrigger(AutoAttackTrigger);
            animator.Play(state, 0, 0f);
            return;
        }
        PlayTrigger(AutoAttackTrigger);
    }

    private void HandleSpecialAbilityUsed(
        EnemyActor enemy)
    {
        queuedAbilityImpactId = 0;
        abilityPresentationId = enemy.ActiveSpecialAbilityAnimationActionId;
        abilityImpactDelivered = false;
        abilityStartedAt = Time.time;
        int state = Animator.StringToHash("Base Layer.Ability");
        if (UsesAuthoredAbility && animator != null && animator.isActiveAndEnabled &&
            animator.runtimeAnimatorController != null && animator.HasState(0, state))
        {
            animator.ResetTrigger(AbilityTrigger);
            animator.Play(state, 0, 0f);
            return;
        }
        PlayTrigger(AbilityTrigger);
    }

    private void HandleAutoAttackImpactReached()
    {
        if (enemyAutoAttack != null)
        {
            if (enemyActor != null && enemyActor.Definition != null &&
                enemyActor.Definition.UseAuthoredAutoAttackMotion)
                queuedImpactPresentationId = enemyAutoAttack.ActiveAttackPresentationId;
            else enemyAutoAttack.ResolveAnimationImpact();
        }
    }

    private void LateUpdate()
    {
        // Unity emits Animation Events before applying this update's Image
        // sprite curve. Resolve later in the SAME rendered frame, once the
        // impact drawing is applied, retaining the accepted action's identity.
        int id = queuedImpactPresentationId;
        queuedImpactPresentationId = 0;
        if (id > 0 && enemyAutoAttack != null && enemyAutoAttack.ActiveAttackPresentationId == id)
            enemyAutoAttack.ResolveAnimationImpact();

        int abilityId = queuedAbilityImpactId;
        queuedAbilityImpactId = 0;
        if (Time.timeScale > 0f && abilityId > 0 && enemyActor != null &&
            enemyActor.isActiveAndEnabled && !enemyActor.IsDefeated &&
            enemyActor.ActiveSpecialAbilityAnimationActionId == abilityId && !abilityImpactDelivered)
        {
            abilityImpactDelivered = true;
            enemyActor.NotifySpecialAbilityImpactReached();
        }
        // The board supplies the missing-impact fallback. This only covers a
        // missing completion event after contact, without interrupting recovery.
        if (UsesAuthoredAbility && abilityImpactDelivered && Time.time - abilityStartedAt >= 3f)
            HandleAbilityCompleted();
    }

    private void HandleAbilityImpactReached()
    {
        if (enemyActor != null)
        {
            if (UsesAuthoredAbility)
            {
                if (Time.timeScale > 0f && abilityPresentationId > 0 &&
                    enemyActor.ActiveSpecialAbilityAnimationActionId == abilityPresentationId)
                    queuedAbilityImpactId = abilityPresentationId;
            }
            else enemyActor.NotifySpecialAbilityImpactReached();
        }
    }

    private void HandleAbilityCompleted()
    {
        if (!UsesAuthoredAbility || Time.timeScale <= 0f || !abilityImpactDelivered ||
            abilityPresentationId <= 0 || enemyActor.ActiveSpecialAbilityAnimationActionId != abilityPresentationId)
            return;
        abilityPresentationId = 0;
        queuedAbilityImpactId = 0;
        enemyActor.EndSpecialAbilityAnimationAction();
    }

    private void HandleAutoAttackCompleted()
    {
        if (enemyAutoAttack != null && enemyActor != null &&
            enemyActor.Definition != null && enemyActor.Definition.UseAuthoredAutoAttackMotion)
            enemyAutoAttack.CompleteAttackPresentation(enemyAutoAttack.ActiveAttackPresentationId);
    }

    private void PlayTrigger(int triggerHash)
    {
        if (animator == null ||
            !animator.isActiveAndEnabled ||
            animator.runtimeAnimatorController == null)
        {
            return;
        }

        if (!HasTriggerParameter(triggerHash))
        {
            return;
        }

        animator.SetTrigger(triggerHash);
    }

    private bool HasTriggerParameter(
        int triggerHash)
    {
        AnimatorControllerParameter[] parameters =
            animator.parameters;

        for (int i = 0; i < parameters.Length; i++)
        {
            if (parameters[i].nameHash == triggerHash &&
                parameters[i].type ==
                AnimatorControllerParameterType.Trigger)
            {
                return true;
            }
        }

        return false;
    }
}
