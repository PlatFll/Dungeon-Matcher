using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Image))]
public sealed class UIPixelParticle :
    MonoBehaviour
{
    private RectTransform rectTransform;
    private Image particleImage;

    private Vector2 velocity;
    private Color startingColor;

    private float lifetime;
    private float elapsedTime;

    private bool isPlaying, pooled, toward;
    private float pixelStep;
    private Vector2 precisePosition, startPosition, destination;
    public bool IsPlaying => isPlaying;
    public void Stop() { isPlaying = false; gameObject.SetActive(false); }
    public void PlayToward(Vector2 start, Vector2 end, float duration, float size, Color color, float nativePixel)
    { Play(start, Vector2.zero, duration, size, color, true, nativePixel); toward = true; destination = end; }

    private void Awake()
    {
        CacheReferences();
    }

    private void CacheReferences()
    {
        if (rectTransform == null)
        {
            rectTransform =
                GetComponent<RectTransform>();
        }

        if (particleImage == null)
        {
            particleImage =
                GetComponent<Image>();
        }
    }

    public void Play(
        Vector2 localPosition,
        Vector2 initialVelocity,
        float duration,
        float size,
        Color color, bool reuse = false, float nativePixel = 0)
    {
        CacheReferences();
        pooled = reuse; pixelStep = nativePixel; toward = false;
        startPosition = precisePosition = localPosition;
        velocity =
            initialVelocity;

        lifetime =
            Mathf.Max(
                0.05f,
                duration
            );

        elapsedTime = 0f;

        startingColor =
            color;

        startingColor.a = 1f;

        rectTransform.anchorMin =
            new Vector2(0.5f, 0.5f);

        rectTransform.anchorMax =
            new Vector2(0.5f, 0.5f);

        rectTransform.pivot =
            new Vector2(0.5f, 0.5f);

        rectTransform.anchoredPosition =
            localPosition;

        rectTransform.sizeDelta =
            Vector2.one *
            Mathf.Max(1f, size);

        rectTransform.localScale =
            Vector3.one;

        rectTransform.localRotation =
            Quaternion.identity;

        particleImage.color =
            startingColor;

        particleImage.raycastTarget =
            false;

        particleImage.enabled = true;

        isPlaying = true;

        gameObject.SetActive(true);
        transform.SetAsLastSibling();
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

        precisePosition = toward ? Vector2.Lerp(startPosition, destination, Mathf.Clamp01(elapsedTime / lifetime)) :
            precisePosition + velocity * deltaTime;
        rectTransform.anchoredPosition = pixelStep > 0 ? new Vector2(
            Mathf.Round(precisePosition.x / pixelStep), Mathf.Round(precisePosition.y / pixelStep)) * pixelStep : precisePosition;

        /*
         * Gradually slows horizontal movement while
         * allowing the particle to continue floating up.
         */
        velocity.x =
            Mathf.Lerp(
                velocity.x,
                0f,
                4f * deltaTime
            );

        velocity.y =
            Mathf.Lerp(
                velocity.y,
                velocity.y * 0.65f,
                1.5f * deltaTime
            );

        float normalizedTime =
            Mathf.Clamp01(
                elapsedTime /
                lifetime
            );

        Color currentColor =
            startingColor;

        currentColor.a = pixelStep > 0 ? 1 : 1f - normalizedTime;

        particleImage.color =
            currentColor;

        float scale = pixelStep > 0 ? 1 : Mathf.Lerp(
                1f,
                0.35f,
                normalizedTime
            );

        rectTransform.localScale =
            Vector3.one *
            scale;

        if (elapsedTime >= lifetime)
        {
            if (pooled) Stop(); else Destroy(gameObject);
        }
    }
}