using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(EnemyActor))]
[RequireComponent(typeof(EnemyAutoAttack))]
[RequireComponent(typeof(EnemyStagger))]
public sealed class EnemyCombatFeedback :
    MonoBehaviour
{
    private static readonly int FlashAmountId =
        Shader.PropertyToID(
            "_FlashAmount"
        );

    [Header("Runtime Sources")]
    [SerializeField]
    private EnemyActor enemyActor;

    [SerializeField]
    private EnemyAutoAttack enemyAutoAttack;

    [SerializeField]
    private EnemyStagger enemyStagger;

    [Header("Visual References")]
    [SerializeField]
    [Tooltip(
        "Only this RectTransform moves during damage shake."
    )]
    private RectTransform visualRoot;

    [SerializeField]
    [Tooltip(
        "The Image displaying the enemy sprite."
    )]
    private Image enemyImage;

    [SerializeField]
    private CharacterAnimationPlayback characterAnimation;

    [SerializeField]
    [Tooltip(
        "The Image that fills toward the next attack."
    )]
    private Image attackTimerFill;

    [Header("Attack Timer")]
    [SerializeField]
    private bool showTimerOnlyWhileActive = true;

    [SerializeField]
    private bool fillClockwise = true;

    [SerializeField]
    private Image.Origin360 fillOrigin =
        Image.Origin360.Top;

    [Header("Attack Lunge")]
    [SerializeField, Min(0f)]
    [Tooltip(
    "How many UI pixels the enemy moves toward the player."
)]
    private float attackLungeDistance = 3f;

    [SerializeField, Min(0.01f)]
    private float attackLungeOutDuration = 0.06f;

    [SerializeField, Min(0f)]
    private float attackLungeHoldDuration = 0.025f;

    [SerializeField, Min(0.01f)]
    private float attackLungeReturnDuration = 0.08f;

    [Header("Damage Shake")]
    [SerializeField, Min(0f)]
    private float shakeDuration = 0.5f;

    [SerializeField]
    [Tooltip(
        "Shake distance in whole UI pixels."
    )]
    private Vector2 shakeStrength =
        new Vector2(3f, 2f);

    [SerializeField, Min(0.01f)]
    private float shakeStepInterval = 0.04f;

    [Header("Stagger Flash")]
    [SerializeField, Min(0.02f)]
    private float blinkInterval = 0.14f;

    [SerializeField, Range(0f, 1f)]
    private float whiteFlashAmount = 1f;

    private Material runtimeFlashMaterial;

    private Vector2 restingPosition;

    private Coroutine attackLungeCoroutine;
    private Coroutine shakeCoroutine;
    private Coroutine blinkCoroutine;

    private int activeAttackLungePresentationId;
    private bool damageShakePending;

    private readonly Queue<int>
        queuedAttackLungePresentationIds =
            new Queue<int>();

    private void Awake()
    {
        ResolveReferences();

        if (visualRoot != null)
        {
            restingPosition =
                visualRoot.anchoredPosition;
        }

        ConfigureAttackTimer();
        CreateRuntimeFlashMaterial();
    }

    private void OnEnable()
    {
        ResolveReferences();
        Subscribe();
    }

    private void Update()
    {
        UpdateAttackTimer();
        TryStartDeferredFeedback();
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

        if (enemyStagger == null)
        {
            enemyStagger =
                GetComponent<EnemyStagger>();
        }

        if (characterAnimation == null &&
            enemyImage != null)
        {
            characterAnimation =
                enemyImage.GetComponent<CharacterAnimationPlayback>();
        }
    }

    private void Subscribe()
    {
        Unsubscribe();

        if (enemyActor != null)
        {
            enemyActor.DamageReceived +=
                HandleDamageReceived;

            enemyActor.Defeated +=
                HandleEnemyDefeated;
        }

        if (enemyAutoAttack != null)
        {
            enemyAutoAttack.AttackStarted +=
                HandleAttackStarted;
        }

        if (enemyStagger != null)
        {
            enemyStagger.StaggerApplied +=
                HandleStaggerApplied;

            enemyStagger.StaggerEnded +=
                HandleStaggerEnded;
        }
    }

    private void Unsubscribe()
    {
        if (enemyActor != null)
        {
            enemyActor.DamageReceived -=
                HandleDamageReceived;

            enemyActor.Defeated -=
                HandleEnemyDefeated;
        }

        if (enemyAutoAttack != null)
        {
            enemyAutoAttack.AttackStarted -=
                HandleAttackStarted;
        }

        if (enemyStagger != null)
        {
            enemyStagger.StaggerApplied -=
                HandleStaggerApplied;

            enemyStagger.StaggerEnded -=
                HandleStaggerEnded;
        }
    }

    private void ConfigureAttackTimer()
    {
        if (attackTimerFill == null)
        {
            return;
        }

        attackTimerFill.type =
            Image.Type.Filled;

        attackTimerFill.fillMethod =
            Image.FillMethod.Radial360;

        attackTimerFill.fillOrigin =
            (int)fillOrigin;

        attackTimerFill.fillClockwise =
            fillClockwise;

        attackTimerFill.fillAmount = 0f;
    }

    private void UpdateAttackTimer()
    {
        if (attackTimerFill == null)
        {
            return;
        }

        bool timerIsAvailable =
            enemyActor != null &&
            enemyActor.IsInitialized &&
            !enemyActor.IsDefeated &&
            enemyAutoAttack != null &&
            enemyAutoAttack.IsRunning;

        if (showTimerOnlyWhileActive)
        {
            attackTimerFill.enabled =
                timerIsAvailable;
        }

        if (!timerIsAvailable)
        {
            attackTimerFill.fillAmount = 0f;
            return;
        }

        float attackInterval =
            Mathf.Max(
                0.01f,
                enemyActor.AttackInterval
            );

        float remainingNormalized =
            Mathf.Clamp01(
                enemyAutoAttack
                    .RemainingAttackTime /
                attackInterval
            );

        /*
         * Remaining time starts at 100% and approaches 0.
         * Inverting it makes the circle fill toward attack.
         */
        attackTimerFill.fillAmount =
            1f - remainingNormalized;
    }

    private void CreateRuntimeFlashMaterial()
    {
        if (enemyImage == null)
        {
            Debug.LogError(
                "EnemyCombatFeedback requires an Enemy Image.",
                this
            );

            return;
        }

        Material sourceMaterial =
            enemyImage.material;

        if (sourceMaterial == null ||
            !sourceMaterial.HasProperty(
                FlashAmountId
            ))
        {
            Debug.LogError(
                $"{name}'s Enemy Image must use the " +
                "Dungeon Matcher/UI/White Flash shader.",
                this
            );

            return;
        }

        runtimeFlashMaterial =
            new Material(sourceMaterial)
            {
                name =
                    $"{sourceMaterial.name} " +
                    $"({name} Runtime)"
            };

        enemyImage.material =
            runtimeFlashMaterial;

        SetWhiteFlash(0f);
    }

    private void HandleAttackStarted(
        EnemyAutoAttack autoAttack)
    {
        if (visualRoot == null ||
            enemyActor == null ||
            enemyActor.IsDefeated)
        {
            return;
        }

        int presentationId =
            autoAttack != null
                ? autoAttack
                    .ActiveAttackPresentationId
                : 0;

        if (attackLungeCoroutine != null ||
            shakeCoroutine != null)
        {
            /*
             * visualRoot has one movement owner. Preserve this request until
             * the current movement completes instead of dropping its impact
             * and return acknowledgements.
             */
            QueueAttackLunge(
                presentationId
            );
            return;
        }

        TryStartAttackLunge(
            presentationId
        );
    }

    private void QueueAttackLunge(
        int presentationId)
    {
        if (presentationId > 0 &&
            (presentationId ==
                activeAttackLungePresentationId ||
             queuedAttackLungePresentationIds.Contains(
                 presentationId
             )))
        {
            return;
        }

        queuedAttackLungePresentationIds.Enqueue(
            presentationId
        );
    }

    private bool TryStartAttackLunge(
        int presentationId)
    {
        if (attackLungeCoroutine != null ||
            shakeCoroutine != null ||
            !IsAttackPresentationCurrent(
                presentationId
            ))
        {
            return false;
        }

        activeAttackLungePresentationId =
            presentationId;

        attackLungeCoroutine =
            StartCoroutine(
                AttackLungeRoutine(
                    presentationId
                )
            );

        return true;
    }

    private IEnumerator AttackLungeRoutine(
        int presentationId)
    {
        float distance =
            Mathf.Round(
                Mathf.Abs(
                    attackLungeDistance
                )
            );

        Vector2 targetOffset =
            Vector2.left *
            distance;

        float elapsedTime = 0f;

        while (elapsedTime <
               attackLungeOutDuration)
        {
            float progress =
                Mathf.Clamp01(
                    elapsedTime /
                    attackLungeOutDuration
                );

            Vector2 currentOffset =
                Vector2.Lerp(
                    Vector2.zero,
                    targetOffset,
                    progress
                );

            ApplyRoundedVisualOffset(
                currentOffset
            );

            elapsedTime +=
                Time.deltaTime;

            yield return null;
        }

        ApplyRoundedVisualOffset(
            targetOffset
        );

        if (presentationId > 0)
        {
            enemyAutoAttack
                ?.ResolvePresentationImpact(
                    presentationId
                );
        }

        if (attackLungeHoldDuration > 0f)
        {
            yield return
                new WaitForSeconds(
                    attackLungeHoldDuration
                );
        }

        elapsedTime = 0f;

        while (elapsedTime <
               attackLungeReturnDuration)
        {
            float progress =
                Mathf.Clamp01(
                    elapsedTime /
                    attackLungeReturnDuration
                );

            Vector2 currentOffset =
                Vector2.Lerp(
                    targetOffset,
                    Vector2.zero,
                    progress
                );

            ApplyRoundedVisualOffset(
                currentOffset
            );

            elapsedTime +=
                Time.deltaTime;

            yield return null;
        }

        RestoreVisualPosition();

        activeAttackLungePresentationId = 0;
        attackLungeCoroutine = null;

        if (presentationId > 0)
        {
            enemyAutoAttack
                ?.CompleteAttackPresentation(
                    presentationId
                );
        }

        TryStartDeferredFeedback();
    }

    private void TryStartDeferredFeedback()
    {
        if (attackLungeCoroutine != null ||
            shakeCoroutine != null)
        {
            return;
        }

        if (enemyActor == null ||
            !enemyActor.IsInitialized ||
            enemyActor.IsDefeated)
        {
            queuedAttackLungePresentationIds.Clear();
            damageShakePending = false;
            return;
        }

        while (queuedAttackLungePresentationIds.Count > 0)
        {
            int presentationId =
                queuedAttackLungePresentationIds
                    .Dequeue();

            if (TryStartAttackLunge(
                    presentationId))
            {
                return;
            }

            /*
             * A synchronized request may have timed out or been cancelled
             * while it waited behind a shake. Its presentation ID no longer
             * matches, so discarding it cannot affect gameplay.
             */
        }

        if (!damageShakePending ||
            (enemyAutoAttack != null &&
             enemyAutoAttack
                 .IsAttackSequenceInProgress))
        {
            return;
        }

        StartDamageShake();
    }

    private bool IsAttackPresentationCurrent(
        int presentationId)
    {
        if (enemyActor == null ||
            !enemyActor.IsInitialized ||
            enemyActor.IsDefeated ||
            enemyAutoAttack == null ||
            enemyAutoAttack.PlayerTarget == null ||
            enemyAutoAttack.PlayerTarget.IsDefeated)
        {
            return false;
        }

        if (presentationId <= 0)
        {
            return true;
        }

        return
            enemyAutoAttack
                .IsAttackSequenceInProgress &&
            enemyAutoAttack
                .ActiveAttackPresentationId ==
                    presentationId;
    }

    private void ApplyRoundedVisualOffset(
        Vector2 offset)
    {
        if (visualRoot == null)
        {
            return;
        }

        visualRoot.anchoredPosition =
            restingPosition +
            new Vector2(
                Mathf.Round(offset.x),
                Mathf.Round(offset.y)
            );
    }

    private void StopAttackLunge()
    {
        queuedAttackLungePresentationIds.Clear();
        activeAttackLungePresentationId = 0;

        if (attackLungeCoroutine != null)
        {
            StopCoroutine(
                attackLungeCoroutine
            );

            attackLungeCoroutine = null;
        }

        RestoreVisualPosition();
    }

    private void HandleDamageReceived(
        EnemyActor enemy,
        int damageAmount)
    {
        if (damageAmount <= 0 ||
            visualRoot == null)
        {
            return;
        }

        if (attackLungeCoroutine != null ||
            queuedAttackLungePresentationIds.Count > 0 ||
            (enemyAutoAttack != null &&
             enemyAutoAttack
                 .IsAttackSequenceInProgress))
        {
            /*
             * Never interrupt a lunge or insert shake movement between hits
             * of one accepted sequence. Coalesce normal damage feedback and
             * play it after the sequence releases visual ownership.
             */
            damageShakePending = true;
            return;
        }

        StartDamageShake();
    }

    private void StartDamageShake()
    {
        if (enemyActor == null ||
            enemyActor.IsDefeated ||
            visualRoot == null)
        {
            damageShakePending = false;
            return;
        }

        if (attackLungeCoroutine != null)
        {
            damageShakePending = true;
            return;
        }

        if (shakeCoroutine != null)
        {
            StopCoroutine(
                shakeCoroutine
            );
        }

        damageShakePending = false;
        RestoreVisualPosition();
        shakeCoroutine =
            StartCoroutine(
                ShakeRoutine()
            );
    }

    private IEnumerator ShakeRoutine()
    {
        float elapsedTime = 0f;

        SetWhiteFlash(0f);

        while (elapsedTime <
               shakeDuration)
        {
            if (enemyActor == null ||
                enemyActor.IsDefeated ||
                visualRoot == null)
            {
                break;
            }

            float horizontalOffset =
                Mathf.Round(
                    Random.Range(
                        -Mathf.Abs(
                            shakeStrength.x
                        ),
                        Mathf.Abs(
                            shakeStrength.x
                        )
                    )
                );

            float verticalOffset =
                Mathf.Round(
                    Random.Range(
                        -Mathf.Abs(
                            shakeStrength.y
                        ),
                        Mathf.Abs(
                            shakeStrength.y
                        )
                    )
                );

            visualRoot.anchoredPosition =
                restingPosition +
                new Vector2(
                    horizontalOffset,
                    verticalOffset
                );

            float stepDuration =
                Mathf.Min(
                    shakeStepInterval,
                    shakeDuration -
                    elapsedTime
                );

            elapsedTime +=
                stepDuration;

            yield return
                new WaitForSeconds(
                    stepDuration
                );
        }

        RestoreVisualPosition();

        shakeCoroutine = null;
        TryStartDeferredFeedback();
    }

    private void HandleStaggerApplied(
        EnemyStagger stagger,
        float addedDuration,
        float remainingDuration)
    {
        characterAnimation?.Pause();

        if (runtimeFlashMaterial == null ||
            blinkCoroutine != null)
        {
            return;
        }

        blinkCoroutine =
            StartCoroutine(
                BlinkWhileStaggered()
            );
    }

    private IEnumerator BlinkWhileStaggered()
    {
        bool showWhite = false;

        while (enemyActor != null &&
               !enemyActor.IsDefeated &&
               enemyStagger != null &&
               enemyStagger.IsStaggered)
        {
            /*
             * The user should first see the shake.
             * Blinking resumes once the sprite stands still.
             */
            if (shakeCoroutine != null)
            {
                SetWhiteFlash(0f);
                yield return null;
                continue;
            }

            showWhite = !showWhite;

            SetWhiteFlash(
                showWhite
                    ? whiteFlashAmount
                    : 0f
            );

            float elapsedTime = 0f;

            while (elapsedTime <
                   blinkInterval)
            {
                if (enemyStagger == null ||
                    !enemyStagger.IsStaggered ||
                    shakeCoroutine != null ||
                    enemyActor == null ||
                    enemyActor.IsDefeated)
                {
                    break;
                }

                elapsedTime +=
                    Time.deltaTime;

                yield return null;
            }
        }

        SetWhiteFlash(0f);

        blinkCoroutine = null;
    }

    private void HandleStaggerEnded(
        EnemyStagger stagger)
    {
        StopBlinking();
        characterAnimation?.Resume();
    }

    private void HandleEnemyDefeated(
        EnemyActor enemy)
    {
        StopAllFeedback();

        if (attackTimerFill != null)
        {
            attackTimerFill.fillAmount = 0f;
            attackTimerFill.enabled = false;
        }
    }

    private void StopBlinking()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(
                blinkCoroutine
            );

            blinkCoroutine = null;
        }

        SetWhiteFlash(0f);
    }

    private void StopAllFeedback()
    {
        damageShakePending = false;
        StopAttackLunge();

        if (shakeCoroutine != null)
        {
            StopCoroutine(
                shakeCoroutine
            );

            shakeCoroutine = null;
        }

        StopBlinking();
        RestoreVisualPosition();
        characterAnimation?.Resume();
    }

    private void RestoreVisualPosition()
    {
        if (visualRoot != null)
        {
            visualRoot.anchoredPosition =
                restingPosition;
        }
    }

    private void SetWhiteFlash(
        float amount)
    {
        if (runtimeFlashMaterial == null)
        {
            return;
        }

        runtimeFlashMaterial.SetFloat(
            FlashAmountId,
            Mathf.Clamp01(amount)
        );
    }

    private void OnDisable()
    {
        Unsubscribe();
        StopAllFeedback();
    }

    private void OnDestroy()
    {
        Unsubscribe();
        StopAllFeedback();

        if (runtimeFlashMaterial != null)
        {
            Destroy(
                runtimeFlashMaterial
            );

            runtimeFlashMaterial = null;
        }
    }

    private void OnValidate()
    {
        attackLungeDistance =
            Mathf.Max(
                0f,
                attackLungeDistance
            );

        attackLungeOutDuration =
            Mathf.Max(
                0.01f,
                attackLungeOutDuration
            );

        attackLungeHoldDuration =
            Mathf.Max(
                0f,
                attackLungeHoldDuration
            );

        attackLungeReturnDuration =
            Mathf.Max(
                0.01f,
                attackLungeReturnDuration
            );

        shakeDuration =
            Mathf.Max(
                0f,
                shakeDuration
            );

        shakeStrength =
            new Vector2(
                Mathf.Abs(
                    shakeStrength.x
                ),
                Mathf.Abs(
                    shakeStrength.y
                )
            );

        shakeStepInterval =
            Mathf.Max(
                0.01f,
                shakeStepInterval
            );

        blinkInterval =
            Mathf.Max(
                0.02f,
                blinkInterval
            );

        whiteFlashAmount =
            Mathf.Clamp01(
                whiteFlashAmount
            );

        ConfigureAttackTimer();
    }
}
