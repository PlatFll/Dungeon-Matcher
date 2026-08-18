using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Image))]
public sealed class EnemyWeaknessBurstParticle :
    MonoBehaviour
{
    private RectTransform particleRect;
    private Image particleImage;

    private Vector2 velocity;
    private float lifetime;
    private float elapsedTime;
    private float endScale;
    private Color baseColor;
    private bool isPlaying;

    private void Awake()
    {
        particleRect =
            GetComponent<RectTransform>();

        particleImage =
            GetComponent<Image>();

        particleImage.raycastTarget = false;
    }

    public void Play(
        Vector2 localPosition,
        Vector2 initialVelocity,
        float duration,
        float size,
        float targetEndScale,
        Color color)
    {
        lifetime =
            Mathf.Max(0.01f, duration);

        velocity = initialVelocity;
        endScale =
            Mathf.Clamp01(targetEndScale);

        baseColor = color;
        elapsedTime = 0f;
        isPlaying = true;

        particleRect.anchorMin =
            new Vector2(0.5f, 0.5f);

        particleRect.anchorMax =
            new Vector2(0.5f, 0.5f);

        particleRect.pivot =
            new Vector2(0.5f, 0.5f);

        particleRect.localPosition =
            new Vector3(
                localPosition.x,
                localPosition.y,
                0f
            );

        particleRect.sizeDelta =
            Vector2.one *
            Mathf.Max(1f, size);

        particleRect.localScale =
            Vector3.one;

        particleImage.sprite = null;
        particleImage.color = baseColor;
    }

    private void Update()
    {
        if (!isPlaying)
        {
            return;
        }

        float deltaTime =
            Time.deltaTime;

        elapsedTime += deltaTime;

        float progress =
            Mathf.Clamp01(
                elapsedTime /
                lifetime
            );

        Vector3 localPosition =
            particleRect.localPosition;

        localPosition.x +=
            velocity.x * deltaTime;

        localPosition.y +=
            velocity.y * deltaTime;

        particleRect.localPosition =
            localPosition;

        /*
         * A small amount of damping makes the square fragments feel like
         * a tight magical burst instead of endlessly drifting confetti.
         */
        velocity *=
            Mathf.Pow(
                0.08f,
                deltaTime
            );

        float smoothProgress =
            progress *
            progress *
            (3f - 2f * progress);

        float scale =
            Mathf.Lerp(
                1f,
                endScale,
                smoothProgress
            );

        particleRect.localScale =
            Vector3.one * scale;

        float fadeProgress =
            Mathf.InverseLerp(
                0.28f,
                1f,
                progress
            );

        Color color = baseColor;
        color.a *=
            1f - fadeProgress;

        particleImage.color = color;

        if (progress < 1f)
        {
            return;
        }

        isPlaying = false;
        Destroy(gameObject);
    }
}
