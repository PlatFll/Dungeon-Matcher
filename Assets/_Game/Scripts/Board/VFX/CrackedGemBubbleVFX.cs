using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class CrackedGemBubbleVFX :
    MonoBehaviour
{
    private const int TextureSize = 16;
    private const float PixelsPerUnit = 16f;

    private static Sprite cachedBubbleSprite;

    private BoardController boardController;
    private Transform originTransform;
    private Sprite bubbleSprite;

    public static CrackedGemBubbleVFX EnsureInstalled(
        BoardController board,
        Transform origin,
        Sprite customBubbleSprite = null)
    {
        if (board == null)
        {
            return null;
        }

        CrackedGemBubbleVFX vfx =
            board.GetComponent<
                CrackedGemBubbleVFX
            >();

        if (vfx == null)
        {
            vfx =
                board.gameObject.AddComponent<
                    CrackedGemBubbleVFX
                >();
        }

        vfx.Configure(
            board,
            origin,
            customBubbleSprite
        );

        return vfx;
    }

    private void OnEnable()
    {
        ResolveReferences();
        Subscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    public void Configure(
        BoardController board,
        Transform origin,
        Sprite customBubbleSprite = null)
    {
        if (boardController != board)
        {
            Unsubscribe();
            boardController = board;
            Subscribe();
        }

        originTransform = origin;
        bubbleSprite = customBubbleSprite;
    }

    private void ResolveReferences()
    {
        if (boardController == null)
        {
            boardController =
                GetComponent<BoardController>();
        }
    }

    private void Subscribe()
    {
        if (boardController == null)
        {
            return;
        }

        boardController.CrackedGemTargetsSelected -=
            HandleTargetsSelected;

        boardController.CrackedGemTargetsSelected +=
            HandleTargetsSelected;
    }

    private void Unsubscribe()
    {
        if (boardController == null)
        {
            return;
        }

        boardController.CrackedGemTargetsSelected -=
            HandleTargetsSelected;
    }

    private void HandleTargetsSelected(
        IReadOnlyList<Vector3> targetPositions,
        float travelDuration,
        float hoverDuration)
    {
        if (targetPositions == null ||
            targetPositions.Count == 0)
        {
            return;
        }

        Vector3 origin =
            originTransform != null
                ? originTransform.position
                : transform.position;

        for (int index = 0;
             index < targetPositions.Count;
             index++)
        {
            StartCoroutine(
                PlayBubble(
                    origin,
                    targetPositions[index],
                    Mathf.Max(
                        0f,
                        travelDuration
                    ),
                    Mathf.Max(
                        0f,
                        hoverDuration
                    ),
                    index
                )
            );
        }
    }

    private IEnumerator PlayBubble(
        Vector3 origin,
        Vector3 target,
        float travelDuration,
        float hoverDuration,
        int sequenceIndex)
    {
        GameObject bubbleObject =
            new GameObject(
                $"BardleyBubble_{sequenceIndex}"
            );

        bubbleObject.transform.SetParent(
            transform,
            true
        );

        bubbleObject.transform.position = origin;
        bubbleObject.transform.localScale =
            Vector3.one * 0.62f;

        SpriteRenderer renderer =
            bubbleObject.AddComponent<
                SpriteRenderer
            >();

        bool usesCustomSprite =
            bubbleSprite != null;

        renderer.sprite =
            usesCustomSprite
                ? bubbleSprite
                : GetOrCreateBubbleSprite();

        renderer.color =
            usesCustomSprite
                ? Color.white
                : new Color(
                    0.90f,
                    0.98f,
                    1f,
                    0.92f
                );

        renderer.sortingOrder = 50;

        if (travelDuration > 0f)
        {
            float elapsed = 0f;

            while (elapsed < travelDuration &&
                   bubbleObject != null)
            {
                elapsed += Time.deltaTime;

                float normalized =
                    Mathf.Clamp01(
                        elapsed /
                        travelDuration
                    );

                float eased =
                    Mathf.SmoothStep(
                        0f,
                        1f,
                        normalized
                    );

                float arc =
                    Mathf.Sin(
                        normalized *
                        Mathf.PI
                    ) *
                    boardController.CellSize *
                    0.22f;

                Vector3 position =
                    Vector3.Lerp(
                        origin,
                        target,
                        eased
                    );

                position.y += arc;

                bubbleObject.transform.position =
                    position;

                float pulse =
                    0.62f +
                    Mathf.Sin(
                        normalized *
                        Mathf.PI *
                        3f
                    ) *
                    0.05f;

                bubbleObject.transform.localScale =
                    Vector3.one * pulse;

                yield return null;
            }
        }

        if (bubbleObject == null)
        {
            yield break;
        }

        bubbleObject.transform.position = target;
        bubbleObject.transform.localScale =
            Vector3.one * 0.68f;

        if (hoverDuration > 0f)
        {
            float elapsed = 0f;

            while (elapsed < hoverDuration &&
                   bubbleObject != null)
            {
                elapsed += Time.deltaTime;

                float pulse =
                    0.68f +
                    Mathf.Sin(
                        elapsed *
                        Mathf.PI *
                        8f
                    ) *
                    0.04f;

                bubbleObject.transform.localScale =
                    Vector3.one * pulse;

                yield return null;
            }
        }

        if (bubbleObject == null)
        {
            yield break;
        }

        float popDuration = 0.08f;
        float popElapsed = 0f;

        while (popElapsed < popDuration &&
               bubbleObject != null)
        {
            popElapsed += Time.deltaTime;

            float normalized =
                Mathf.Clamp01(
                    popElapsed /
                    popDuration
                );

            bubbleObject.transform.localScale =
                Vector3.one *
                Mathf.Lerp(
                    0.68f,
                    0.95f,
                    normalized
                );

            Color color = renderer.color;
            color.a = 1f - normalized;
            renderer.color = color;

            yield return null;
        }

        if (bubbleObject != null)
        {
            Destroy(bubbleObject);
        }
    }

    private static Sprite GetOrCreateBubbleSprite()
    {
        if (cachedBubbleSprite != null)
        {
            return cachedBubbleSprite;
        }

        Texture2D texture =
            new Texture2D(
                TextureSize,
                TextureSize,
                TextureFormat.RGBA32,
                false
            );

        texture.name =
            "Runtime_BardleyBubble";

        texture.filterMode =
            FilterMode.Point;

        texture.wrapMode =
            TextureWrapMode.Clamp;

        texture.hideFlags =
            HideFlags.HideAndDontSave;

        Color transparent =
            new Color(0f, 0f, 0f, 0f);

        Color outline =
            new Color(
                0.82f,
                0.96f,
                1f,
                0.92f
            );

        Color highlight =
            new Color(
                1f,
                1f,
                1f,
                0.96f
            );

        for (int y = 0;
             y < TextureSize;
             y++)
        {
            for (int x = 0;
                 x < TextureSize;
                 x++)
            {
                texture.SetPixel(
                    x,
                    y,
                    transparent
                );
            }
        }

        Vector2 center =
            new Vector2(
                7.5f,
                7.5f
            );

        for (int y = 1;
             y < TextureSize - 1;
             y++)
        {
            for (int x = 1;
                 x < TextureSize - 1;
                 x++)
            {
                float distance =
                    Vector2.Distance(
                        new Vector2(x, y),
                        center
                    );

                if (distance >= 5.2f &&
                    distance <= 6.5f)
                {
                    texture.SetPixel(
                        x,
                        y,
                        outline
                    );
                }
            }
        }

        texture.SetPixel(5, 11, highlight);
        texture.SetPixel(6, 12, highlight);
        texture.SetPixel(5, 12, highlight);
        texture.SetPixel(4, 10, highlight);

        texture.Apply(false, true);

        cachedBubbleSprite =
            Sprite.Create(
                texture,
                new Rect(
                    0f,
                    0f,
                    TextureSize,
                    TextureSize
                ),
                new Vector2(0.5f, 0.5f),
                PixelsPerUnit,
                0,
                SpriteMeshType.FullRect
            );

        cachedBubbleSprite.name =
            "Runtime_BardleyBubble";

        cachedBubbleSprite.hideFlags =
            HideFlags.HideAndDontSave;

        return cachedBubbleSprite;
    }
}
