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

    private bool isPlaying;

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
        Color color)
    {
        CacheReferences();

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

        rectTransform.anchoredPosition +=
            velocity *
            deltaTime;

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

        currentColor.a =
            1f - normalizedTime;

        particleImage.color =
            currentColor;

        float scale =
            Mathf.Lerp(
                1f,
                0.35f,
                normalizedTime
            );

        rectTransform.localScale =
            Vector3.one *
            scale;

        if (elapsedTime >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}