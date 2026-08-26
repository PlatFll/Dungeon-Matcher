using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(EnemyActor))]
public sealed class EnemyPoisonStatusPresenter : MonoBehaviour
{
    private static readonly int FlashAmountId =
        Shader.PropertyToID("_FlashAmount");

    [Header("Status Icon")]
    [SerializeField]
    [Tooltip(
        "Displayed size of the 32x32 Poisoned source sprite. " +
        "Sixteen UI pixels keeps status information readable " +
        "without covering the enemy."
    )]
    private Vector2 iconSize = new Vector2(16f, 16f);

    [SerializeField]
    [Tooltip("Vertical gap above the enemy sprite in UI pixels.")]
    private float iconOffsetY = 2f;

    [SerializeField, Min(0f)]
    private float materializeWhiteHoldDuration = 0.04f;

    [SerializeField, Min(0.01f)]
    private float materializeDuration = 0.12f;

    [SerializeField, Min(0f)]
    [Tooltip(
        "The status icon starts blinking this many seconds " +
        "before Poison expires."
    )]
    private float expirationBlinkLeadTime = 1.4f;

    [SerializeField, Min(0.02f)]
    private float expirationBlinkInterval = 0.12f;

    [Header("Poison Tick Feedback")]
    [SerializeField, Min(0.01f)]
    [Tooltip(
        "Brief white flash shown for each poison tick. " +
        "0.10 seconds is long enough to read clearly at 60 FPS " +
        "without becoming a full normal-hit reaction."
    )]
    private float poisonTickWhiteFlashDuration = 0.10f;

    private EnemyActor enemyActor;
    private EnemyPoisonStatus poisonStatus;
    private EnemyStagger enemyStagger;
    private Image enemyImage;

    private Image poisonIcon;
    private Material poisonIconFlashMaterial;

    private Coroutine materializeCoroutine;
    private Coroutine tickFlashCoroutine;

    private bool isMaterializing;
    private bool blinkVisible = true;
    private float blinkElapsedTime;
    private bool warnedMissingStatusSprite;

    public static EnemyPoisonStatusPresenter EnsureInstalled(
        GameObject enemyObject,
        EnemyPoisonStatus status)
    {
        if (enemyObject == null || status == null)
        {
            return null;
        }

        EnemyPoisonStatusPresenter presenter =
            enemyObject.GetComponent<EnemyPoisonStatusPresenter>();

        if (presenter == null)
        {
            presenter =
                enemyObject.AddComponent<EnemyPoisonStatusPresenter>();
        }

        presenter.Bind(status);
        return presenter;
    }

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnEnable()
    {
        ResolveReferences();
        Subscribe();

        if (poisonStatus != null && poisonStatus.IsPoisoned)
        {
            ShowAppliedState();
        }
    }

    private void Update()
    {
        UpdateExpirationBlink();
    }

    public void Bind(EnemyPoisonStatus status)
    {
        if (status == null)
        {
            return;
        }

        Unsubscribe();
        poisonStatus = status;

        ResolveReferences();
        EnsureStatusIcon();
        Subscribe();

        if (poisonStatus.IsPoisoned)
        {
            ShowAppliedState();
        }
        else
        {
            HideStatusIcon();
        }
    }

    private void ResolveReferences()
    {
        if (enemyActor == null)
        {
            enemyActor = GetComponent<EnemyActor>();
        }

        if (poisonStatus == null)
        {
            poisonStatus = GetComponent<EnemyPoisonStatus>();
        }

        if (enemyStagger == null)
        {
            enemyStagger = GetComponent<EnemyStagger>();
        }

        if (enemyImage == null)
        {
            enemyImage = FindEnemyImage();
        }
    }

    private Image FindEnemyImage()
    {
        CharacterAnimationPlayback playback =
            GetComponentInChildren<CharacterAnimationPlayback>(true);

        if (playback != null)
        {
            Image playbackImage = playback.GetComponent<Image>();

            if (playbackImage != null)
            {
                return playbackImage;
            }
        }

        Image[] images = GetComponentsInChildren<Image>(true);

        foreach (Image candidate in images)
        {
            if (candidate == null || candidate == poisonIcon)
            {
                continue;
            }

            Material candidateMaterial = candidate.material;

            if (candidateMaterial != null &&
                candidateMaterial.HasProperty(FlashAmountId))
            {
                return candidate;
            }
        }

        return null;
    }

    private void EnsureStatusIcon()
    {
        ResolveReferences();

        RectTransform actorRect = GetComponent<RectTransform>();
        RectTransform parentRect =
            actorRect != null
                ? actorRect
                : enemyImage != null
                    ? enemyImage.rectTransform
                    : null;

        if (parentRect == null)
        {
            return;
        }

        if (poisonIcon == null)
        {
            GameObject iconObject = new GameObject(
                "PoisonStatusIcon",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image)
            );

            // Runtime-created UI objects start on Default. Match the enemy
            // hierarchy explicitly so the icon behaves like the rest of the UI.
            iconObject.layer = parentRect.gameObject.layer;
            iconObject.transform.SetParent(parentRect, false);

            RectTransform iconRect =
                iconObject.GetComponent<RectTransform>();

            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0f);
            iconRect.sizeDelta = new Vector2(
                Mathf.Max(1f, iconSize.x),
                Mathf.Max(1f, iconSize.y)
            );

            poisonIcon = iconObject.GetComponent<Image>();
            poisonIcon.raycastTarget = false;
            poisonIcon.preserveAspect = true;
            poisonIcon.maskable = false;
            poisonIcon.enabled = false;
            poisonIcon.canvasRenderer.cullTransparentMesh = false;

            // Keep status information above the enemy sprite, health bar and
            // other presentation children rather than burying it inside the
            // animated sprite Image hierarchy.
            iconObject.transform.SetAsLastSibling();
        }

        PositionStatusIcon(parentRect);
        RefreshStatusSprite();
        EnsureIconFlashMaterial();
    }

    private void PositionStatusIcon(RectTransform parentRect)
    {
        if (poisonIcon == null || parentRect == null)
        {
            return;
        }

        RectTransform iconRect = poisonIcon.rectTransform;
        float x = 0f;
        float y = parentRect.rect.height * 0.5f + iconOffsetY;

        if (enemyImage != null)
        {
            RectTransform enemyRect = enemyImage.rectTransform;

            if (enemyRect == parentRect)
            {
                y = enemyRect.rect.height * 0.5f + iconOffsetY;
            }
            else
            {
                Bounds enemyBounds =
                    RectTransformUtility.CalculateRelativeRectTransformBounds(
                        parentRect,
                        enemyRect
                    );

                x = enemyBounds.center.x;
                y = enemyBounds.max.y + iconOffsetY;
            }
        }

        iconRect.anchoredPosition = new Vector2(
            Mathf.Round(x),
            Mathf.Round(y)
        );
    }

    private void RefreshStatusSprite()
    {
        if (poisonIcon == null)
        {
            return;
        }

        BoardController boardController =
            Object.FindFirstObjectByType<BoardController>();

        Sprite statusSprite =
            boardController != null
                ? boardController.PoisonedStatusEffectSprite
                : null;

        poisonIcon.sprite = statusSprite;

        if (statusSprite == null && !warnedMissingStatusSprite)
        {
            warnedMissingStatusSprite = true;

            Debug.LogWarning(
                "Poison is working, but BoardController's " +
                "Poisoned Status Effect Sprite is empty.",
                this
            );
        }
    }

    private void EnsureIconFlashMaterial()
    {
        if (poisonIcon == null ||
            poisonIconFlashMaterial != null ||
            enemyImage == null)
        {
            return;
        }

        Material sourceMaterial = enemyImage.material;

        if (sourceMaterial == null ||
            !sourceMaterial.HasProperty(FlashAmountId))
        {
            // A missing flash shader must never prevent the icon from rendering.
            poisonIcon.material = null;
            return;
        }

        poisonIconFlashMaterial = new Material(sourceMaterial)
        {
            name = $"Poison Status Flash ({name})"
        };

        poisonIcon.material = poisonIconFlashMaterial;
        SetIconWhiteFlash(0f);
    }

    private void Subscribe()
    {
        Unsubscribe();

        if (poisonStatus == null)
        {
            return;
        }

        poisonStatus.PoisonApplied += HandlePoisonApplied;
        poisonStatus.TickDamageApplied += HandleTickDamageApplied;
        poisonStatus.PoisonExpired += HandlePoisonExpired;
    }

    private void Unsubscribe()
    {
        if (poisonStatus == null)
        {
            return;
        }

        poisonStatus.PoisonApplied -= HandlePoisonApplied;
        poisonStatus.TickDamageApplied -= HandleTickDamageApplied;
        poisonStatus.PoisonExpired -= HandlePoisonExpired;
    }

    private void HandlePoisonApplied(
        EnemyPoisonStatus status,
        bool wasAlreadyPoisoned)
    {
        ShowAppliedState();
    }

    private void ShowAppliedState()
    {
        EnsureStatusIcon();

        if (poisonIcon == null || poisonIcon.sprite == null)
        {
            return;
        }

        ResetBlink();
        StopMaterialization();

        materializeCoroutine =
            StartCoroutine(MaterializeStatusIcon());
    }

    private IEnumerator MaterializeStatusIcon()
    {
        if (poisonIcon == null)
        {
            materializeCoroutine = null;
            yield break;
        }

        isMaterializing = true;
        poisonIcon.enabled = true;

        Color iconColor = poisonIcon.color;
        iconColor.a = 1f;
        poisonIcon.color = iconColor;

        bool canWhiteFlash =
            poisonIconFlashMaterial != null &&
            poisonIconFlashMaterial.HasProperty(FlashAmountId);

        if (canWhiteFlash)
        {
            SetIconWhiteFlash(1f);

            if (materializeWhiteHoldDuration > 0f)
            {
                yield return new WaitForSeconds(
                    materializeWhiteHoldDuration
                );
            }

            float elapsedTime = 0f;

            while (elapsedTime < materializeDuration)
            {
                float progress = Mathf.Clamp01(
                    elapsedTime / materializeDuration
                );

                SetIconWhiteFlash(1f - progress);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            SetIconWhiteFlash(0f);
        }
        else
        {
            // Reliable fallback when the white-flash material is unavailable.
            float elapsedTime = 0f;

            while (elapsedTime < materializeDuration)
            {
                float progress = Mathf.Clamp01(
                    elapsedTime / materializeDuration
                );

                Color fadedColor = poisonIcon.color;
                fadedColor.a = progress;
                poisonIcon.color = fadedColor;

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            Color finalColor = poisonIcon.color;
            finalColor.a = 1f;
            poisonIcon.color = finalColor;
        }

        isMaterializing = false;
        materializeCoroutine = null;
    }

    private void HandleTickDamageApplied(
        EnemyPoisonStatus status,
        int actualDamage)
    {
        if (actualDamage <= 0)
        {
            return;
        }

        PlayPoisonTickWhiteFlash();

        if (enemyActor != null)
        {
            CombatTextController.ShowText(
                actualDamage.ToString(),
                enemyActor.transform.position,
                CombatTextKind.PoisonDamage,
                true
            );
        }
    }

    private void PlayPoisonTickWhiteFlash()
    {
        ResolveReferences();

        if (enemyImage == null)
        {
            return;
        }

        Material flashMaterial = enemyImage.material;

        if (flashMaterial == null ||
            !flashMaterial.HasProperty(FlashAmountId))
        {
            return;
        }

        if (tickFlashCoroutine != null)
        {
            StopCoroutine(tickFlashCoroutine);
            tickFlashCoroutine = null;
        }

        tickFlashCoroutine =
            StartCoroutine(PoisonTickWhiteFlashRoutine(flashMaterial));
    }

    private IEnumerator PoisonTickWhiteFlashRoutine(Material flashMaterial)
    {
        if (flashMaterial == null ||
            !flashMaterial.HasProperty(FlashAmountId))
        {
            tickFlashCoroutine = null;
            yield break;
        }

        float previousFlashAmount = Mathf.Clamp01(
            flashMaterial.GetFloat(FlashAmountId)
        );

        flashMaterial.SetFloat(FlashAmountId, 1f);

        yield return new WaitForSeconds(
            Mathf.Max(0.01f, poisonTickWhiteFlashDuration)
        );

        // Stagger owns its ongoing white blink. Do not clear it if a poison
        // tick lands at the same time.
        if (enemyStagger == null || !enemyStagger.IsStaggered)
        {
            flashMaterial.SetFloat(
                FlashAmountId,
                previousFlashAmount
            );
        }

        tickFlashCoroutine = null;
    }

    private void UpdateExpirationBlink()
    {
        if (poisonStatus == null ||
            poisonIcon == null ||
            poisonIcon.sprite == null ||
            !poisonStatus.IsPoisoned ||
            isMaterializing)
        {
            return;
        }

        if (poisonStatus.RemainingDuration > expirationBlinkLeadTime)
        {
            ResetBlink();
            return;
        }

        blinkElapsedTime += Time.deltaTime;

        if (blinkElapsedTime < expirationBlinkInterval)
        {
            return;
        }

        blinkElapsedTime = 0f;
        blinkVisible = !blinkVisible;
        poisonIcon.enabled = blinkVisible;
    }

    private void ResetBlink()
    {
        blinkElapsedTime = 0f;
        blinkVisible = true;

        if (poisonIcon != null &&
            poisonIcon.sprite != null &&
            poisonStatus != null &&
            poisonStatus.IsPoisoned)
        {
            poisonIcon.enabled = true;
        }
    }

    private void HandlePoisonExpired(EnemyPoisonStatus status)
    {
        HideStatusIcon();
    }

    private void HideStatusIcon()
    {
        StopMaterialization();
        blinkElapsedTime = 0f;
        blinkVisible = true;

        if (poisonIcon != null)
        {
            SetIconWhiteFlash(0f);
            poisonIcon.enabled = false;
        }
    }

    private void StopMaterialization()
    {
        if (materializeCoroutine != null)
        {
            StopCoroutine(materializeCoroutine);
            materializeCoroutine = null;
        }

        isMaterializing = false;
        SetIconWhiteFlash(0f);
    }

    private void SetIconWhiteFlash(float amount)
    {
        if (poisonIconFlashMaterial == null ||
            !poisonIconFlashMaterial.HasProperty(FlashAmountId))
        {
            return;
        }

        poisonIconFlashMaterial.SetFloat(
            FlashAmountId,
            Mathf.Clamp01(amount)
        );
    }

    private void StopTickFlash()
    {
        if (tickFlashCoroutine != null)
        {
            StopCoroutine(tickFlashCoroutine);
            tickFlashCoroutine = null;
        }

        if (enemyImage != null &&
            enemyImage.material != null &&
            enemyImage.material.HasProperty(FlashAmountId) &&
            (enemyStagger == null || !enemyStagger.IsStaggered))
        {
            enemyImage.material.SetFloat(FlashAmountId, 0f);
        }
    }

    private void OnDisable()
    {
        Unsubscribe();
        StopTickFlash();
        HideStatusIcon();
    }

    private void OnDestroy()
    {
        Unsubscribe();
        StopTickFlash();
        HideStatusIcon();

        if (poisonIconFlashMaterial != null)
        {
            Destroy(poisonIconFlashMaterial);
            poisonIconFlashMaterial = null;
        }
    }

    private void OnValidate()
    {
        iconSize = new Vector2(
            Mathf.Max(1f, iconSize.x),
            Mathf.Max(1f, iconSize.y)
        );

        materializeWhiteHoldDuration = Mathf.Max(
            0f,
            materializeWhiteHoldDuration
        );

        materializeDuration = Mathf.Max(
            0.01f,
            materializeDuration
        );

        expirationBlinkLeadTime = Mathf.Max(
            0f,
            expirationBlinkLeadTime
        );

        expirationBlinkInterval = Mathf.Max(
            0.02f,
            expirationBlinkInterval
        );

        poisonTickWhiteFlashDuration = Mathf.Max(
            0.01f,
            poisonTickWhiteFlashDuration
        );
    }
}
