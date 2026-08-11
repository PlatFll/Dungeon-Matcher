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

            animationPlayback.AbilityImpactReached +=
                HandleAbilityImpactReached;
        }
    }

    private void OnDisable()
    {
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

            animationPlayback.AbilityImpactReached -=
                HandleAbilityImpactReached;
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
        PlayTrigger(AutoAttackTrigger);
    }

    private void HandleSpecialAbilityUsed(
        EnemyActor enemy)
    {
        PlayTrigger(AbilityTrigger);
    }

    private void HandleAutoAttackImpactReached()
    {
        if (enemyAutoAttack != null)
        {
            enemyAutoAttack.ResolveAnimationImpact();
        }
    }

    private void HandleAbilityImpactReached()
    {
        if (enemyActor != null)
        {
            enemyActor.NotifySpecialAbilityImpactReached();
        }
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
