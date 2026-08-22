using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class PlayerCombatFeedback :
    MonoBehaviour
{
    private static readonly int FlashAmountId =
        Shader.PropertyToID(
            "_FlashAmount"
        );

    [Header("Runtime Source")]
    [SerializeField]
    private PlayerActor playerActor;

    [Header("Visual References")]
    [SerializeField]
    [Tooltip(
        "The RectTransform that shakes when the player is damaged."
    )]
    private RectTransform visualRoot;

    [SerializeField]
    [Tooltip(
        "The Image displaying the player's character sprite."
    )]
    private Image playerImage;

    [SerializeField]
    private CharacterAnimationPlayback characterAnimation;

    [Header("Damage Shake")]
    [SerializeField, Min(0f)]
    private float shakeDuration = 0.3f;

    [SerializeField]
    [Tooltip(
        "Shake distance in whole UI pixels."
    )]
    private Vector2 shakeStrength =
        new Vector2(3f, 2f);

    [SerializeField, Min(0.01f)]
    private float shakeStepInterval = 0.04f;

    [Header("Damage Flash")]
    [SerializeField, Min(0.01f)]
    private float whiteFlashDuration = 0.12f;

    [SerializeField, Range(0f, 1f)]
    private float whiteFlashAmount = 1f;

    private Material runtimeFlashMaterial;
    private Vector2 restingPosition;
    private Coroutine damageFeedbackCoroutine;

    public RectTransform VisualRoot =>
        visualRoot;

    public Image PlayerImage =>
        playerImage;

    private void Awake()
    {

        if (characterAnimation == null &&
            playerImage != null)
        {
            characterAnimation =
                playerImage.GetComponent<CharacterAnimationPlayback>();
        }

        if (visualRoot != null)
        {
            restingPosition =
                visualRoot.anchoredPosition;
        }

        CreateRuntimeFlashMaterial();
    }

    private void OnEnable()
    {
        Subscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
        StopDamageFeedback();
    }

    private void Subscribe()
    {
        if (playerActor == null)
        {
            return;
        }

        playerActor.DamageTaken -=
            HandleDamageTaken;

        playerActor.DamageTaken +=
            HandleDamageTaken;
    }

    private void Unsubscribe()
    {
        if (playerActor == null)
        {
            return;
        }

        playerActor.DamageTaken -=
            HandleDamageTaken;
    }

    private void CreateRuntimeFlashMaterial()
    {
        if (playerImage == null)
        {
            Debug.LogError(
                "PlayerCombatFeedback requires a Player Image.",
                this
            );

            return;
        }

        Material sourceMaterial =
            playerImage.material;

        if (sourceMaterial == null ||
            !sourceMaterial.HasProperty(
                FlashAmountId
            ))
        {
            Debug.LogError(
                "The Player Image must use a material with " +
                "the Dungeon Matcher/UI/White Flash shader.",
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

        playerImage.material =
            runtimeFlashMaterial;

        SetWhiteFlash(0f);
    }

    private void HandleDamageTaken(
        PlayerActor player,
        int actualDamage)
    {
        if (actualDamage <= 0)
        {
            return;
        }

        /*
         * The top battle layout can reposition the player after this component's
         * Awake has already run. Capture the current settled position immediately
         * before a new shake begins so damage feedback always returns the player
         * to the layout's latest position instead of the old scene position.
         *
         * Do not recapture while a shake is already running because the visual
         * root may currently be sitting on a temporary shake offset.
         */
        if (damageFeedbackCoroutine == null)
        {
            CaptureCurrentRestingPosition();
        }

        StopDamageFeedback();

        damageFeedbackCoroutine =
            StartCoroutine(
                DamageFeedbackRoutine()
            );
    }

    private IEnumerator DamageFeedbackRoutine()
    {
        characterAnimation?.Pause();

        float elapsedTime = 0f;

        SetWhiteFlash(
            whiteFlashAmount
        );

        while (elapsedTime <
               shakeDuration)
        {
            if (visualRoot == null)
            {
                break;
            }

            if (elapsedTime >=
                whiteFlashDuration)
            {
                SetWhiteFlash(0f);
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
        SetWhiteFlash(0f);

        characterAnimation?.Resume();

        damageFeedbackCoroutine = null;
    }

    public void EnterDefeatedVisualState()
    {
        /*
         * Lethal damage raises DamageTaken immediately before PlayerActor raises
         * Defeated. Cancel that normal hit reaction first so it cannot fight the
         * death presentation for position, animation playback, or flash amount.
         */
        StopDamageFeedback();

        characterAnimation?.Pause();
        SetWhiteFlash(1f);
    }

    private void CaptureCurrentRestingPosition()
    {
        if (visualRoot == null)
        {
            return;
        }

        restingPosition =
            visualRoot.anchoredPosition;
    }

    private void StopDamageFeedback()
    {
        bool wasRunning =
            damageFeedbackCoroutine != null;

        if (wasRunning)
        {
            StopCoroutine(
                damageFeedbackCoroutine
            );

            damageFeedbackCoroutine = null;

            /*
             * Only restore when an actual shake was interrupted. If no feedback
             * is active, the layout may have intentionally moved the visual root
             * since Awake and restoring the old cached value would undo it.
             */
            RestoreVisualPosition();
        }

        SetWhiteFlash(0f);
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

    private void OnDestroy()
    {
        Unsubscribe();
        StopDamageFeedback();

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

        whiteFlashDuration =
            Mathf.Max(
                0.01f,
                whiteFlashDuration
            );

        whiteFlashAmount =
            Mathf.Clamp01(
                whiteFlashAmount
            );
    }
}