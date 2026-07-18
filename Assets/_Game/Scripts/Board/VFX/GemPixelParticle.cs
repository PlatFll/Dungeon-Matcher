using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public sealed class GemPixelParticle :
    MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    private Vector2 velocity;

    private float angularVelocity;
    private float gravity;

    private float drag;

    private Color startingColor;
    private Vector3 startingScale;

    private float lifetime;
    private float elapsedTime;

    private Action<GemPixelParticle>
        releaseAction;

    private bool isPlaying;

    public bool IsPlaying =>
        isPlaying;

    private void Awake()
    {
        CacheReferences();
    }

    private void CacheReferences()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer =
                GetComponent<SpriteRenderer>();
        }
    }

    public void ConfigureRendering(
        string sortingLayerName,
        int sortingOrder)
    {
        CacheReferences();

        spriteRenderer.sortingLayerName =
            sortingLayerName;

        spriteRenderer.sortingOrder =
            sortingOrder;

        spriteRenderer.maskInteraction =
            SpriteMaskInteraction
                .VisibleInsideMask;
    }

    public void Play(
        Vector3 localPosition,
        Vector2 initialVelocity,
        float duration,
        float size,
        float aspectRatio,
        float startingRotation,
        float spinSpeed,
        float downwardGravity,
        float movementDrag,
        Color color,
        Action<GemPixelParticle> onFinished)
    {
        CacheReferences();

        releaseAction =
            onFinished;

        velocity =
            initialVelocity;

        angularVelocity =
            spinSpeed;

        gravity =
            Mathf.Max(
                0f,
                downwardGravity
            );

        drag =
            Mathf.Max(
                0f,
                movementDrag
            );

        lifetime =
            Mathf.Max(
                0.05f,
                duration
            );

        elapsedTime = 0f;

        float safeSize =
            Mathf.Max(
                0.005f,
                size
            );

        float safeAspectRatio =
            Mathf.Max(
                1f,
                aspectRatio
            );

        /*
         * Produces a narrow rectangular shard rather
         * than a soft square particle.
         */
        startingScale =
            new Vector3(
                safeSize,
                safeSize *
                safeAspectRatio,
                1f
            );

        startingColor =
            color;

        startingColor.a = 1f;

        transform.localPosition =
            localPosition;

        transform.localRotation =
            Quaternion.Euler(
                0f,
                0f,
                startingRotation
            );

        transform.localScale =
            startingScale;

        spriteRenderer.color =
            startingColor;

        spriteRenderer.enabled =
            true;

        isPlaying = true;

        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (!isPlaying)
        {
            return;
        }

        float deltaTime =
            Time.deltaTime;

        elapsedTime +=
            deltaTime;

        /*
         * Slight gravity gives the fragments a physical
         * ballistic arc without making them visibly fall.
         */
        velocity.y -=
            gravity *
            deltaTime;

        transform.localPosition +=
            (Vector3)(
                velocity *
                deltaTime
            );

        transform.Rotate(
            0f,
            0f,
            angularVelocity *
            deltaTime
        );

        /*
         * Lower drag lets shards travel outward rather
         * than stopping immediately beside the gem.
         */
        velocity *=
            Mathf.Exp(
                -drag *
                deltaTime
            );

        angularVelocity *=
            Mathf.Exp(
                -0.8f *
                deltaTime
            );

        float normalizedTime =
            Mathf.Clamp01(
                elapsedTime /
                lifetime
            );

        UpdateColor(
            normalizedTime
        );

        UpdateScale(
            normalizedTime
        );

        if (elapsedTime >= lifetime)
        {
            Finish();
        }
    }

    private void UpdateColor(
        float normalizedTime)
    {
        /*
         * Keep shards solid for most of their life,
         * then make them disappear sharply.
         */
        float fadeProgress =
            Mathf.InverseLerp(
                0.68f,
                1f,
                normalizedTime
            );

        fadeProgress =
            fadeProgress *
            fadeProgress;

        Color currentColor =
            startingColor;

        currentColor.a =
            1f -
            fadeProgress;

        spriteRenderer.color =
            currentColor;
    }

    private void UpdateScale(
        float normalizedTime)
    {
        /*
         * Crystal fragments retain their shape instead
         * of continuously shrinking like soft particles.
         */
        float shrinkProgress =
            Mathf.InverseLerp(
                0.78f,
                1f,
                normalizedTime
            );

        float scaleMultiplier =
            Mathf.Lerp(
                1f,
                0.65f,
                shrinkProgress
            );

        transform.localScale =
            startingScale *
            scaleMultiplier;
    }

    private void Finish()
    {
        if (!isPlaying)
        {
            return;
        }

        isPlaying = false;

        Action<GemPixelParticle>
            callback =
                releaseAction;

        releaseAction = null;

        callback?.Invoke(this);
    }

    public void StopImmediately()
    {
        isPlaying = false;
        releaseAction = null;

        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        isPlaying = false;
        elapsedTime = 0f;
    }
}