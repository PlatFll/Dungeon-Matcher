using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(100)]
[DisallowMultipleComponent]
public sealed class BottomHudModularFrameController : MonoBehaviour
{
    private const string BottomHudName = "BottomHUD";
    private const string GeneratedFrameName = "GeneratedBottomHudFrame";

    // Preserve the existing 104px enclosure and 50px corner footprint. The
    // shared fitter derives edge thickness and tile pitch from the same scale.
    private const float HudHeight = 104f;
    private const float FrameCornerSize = 50f;
    private float FrameThickness => normalPiece != null && cornerPiece != null
        ? FrameCornerSize * normalPiece.rect.height / cornerPiece.rect.width
        : 0f;

    private RectTransform bottomHud;
    private Sprite cornerPiece;
    private Sprite normalPiece;
    private bool frameBuilt;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.AfterSceneLoad
    )]
    private static void InstallOnGameScene()
    {
        GameObject bottomHudObject =
            GameObject.Find(BottomHudName);

        if (bottomHudObject == null)
        {
            return;
        }

        if (!bottomHudObject.TryGetComponent(
                out BottomHudModularFrameController _
            ))
        {
            bottomHudObject.AddComponent<
                BottomHudModularFrameController
            >();
        }
    }

    private void Awake()
    {
        bottomHud = transform as RectTransform;
        if (bottomHud != null)
            bottomHud.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, HudHeight);
    }

    private void Start()
    {
        TryBuildFrame();
    }

    private void LateUpdate()
    {
        if (!frameBuilt)
        {
            TryBuildFrame();
        }
    }

    private void TryBuildFrame()
    {
        if (bottomHud == null)
        {
            bottomHud = transform as RectTransform;
        }

        if (bottomHud == null)
        {
            return;
        }

        ResolveFrameSprites();

        if (cornerPiece == null ||
            normalPiece == null)
        {
            return;
        }

        RemoveExistingFrame();
        BuildFrame();
        KeepAbilityContentInsideFrame();

        frameBuilt = true;
    }

    private void ResolveFrameSprites()
    {
        if (cornerPiece != null &&
            normalPiece != null)
        {
            return;
        }

        BoardVisuals boardVisuals =
            FindFirstObjectByType<BoardVisuals>();

        if (boardVisuals != null)
        {
            Transform boardFrame =
                boardVisuals.transform.Find("BoardFrame");

            if (boardFrame != null)
            {
                SpriteRenderer[] renderers =
                    boardFrame.GetComponentsInChildren<SpriteRenderer>(true);

                foreach (SpriteRenderer renderer in renderers)
                {
                    if (renderer == null ||
                        renderer.sprite == null)
                    {
                        continue;
                    }

                    if (cornerPiece == null &&
                        renderer.name == "TopLeftCorner")
                    {
                        cornerPiece = renderer.sprite;
                    }

                    if (normalPiece == null &&
                        renderer.name.StartsWith("TopEdge_"))
                    {
                        normalPiece = renderer.sprite;
                    }

                    if (cornerPiece != null &&
                        normalPiece != null)
                    {
                        return;
                    }
                }
            }
        }

        /*
         * Fallback to the already-generated battle frame. This keeps the HUD
         * functional if the board hierarchy changes later while still ensuring
         * that the BottomHUD uses the same visual frame family as the battle UI.
         */
        GameObject topHud = GameObject.Find("TopHUD");

        if (topHud == null)
        {
            return;
        }

        Image[] images =
            topHud.GetComponentsInChildren<Image>(true);

        foreach (Image image in images)
        {
            if (image == null ||
                image.sprite == null)
            {
                continue;
            }

            if (cornerPiece == null &&
                image.name == "TopLeftCorner" &&
                image.transform.parent != null &&
                image.transform.parent.name == "BattleArenaFrame")
            {
                cornerPiece = image.sprite;
            }

            if (normalPiece == null &&
                image.name == "TopEdge" &&
                image.transform.parent != null &&
                image.transform.parent.name == "BattleArenaFrame")
            {
                normalPiece = image.sprite;
            }
        }
    }

    private void BuildFrame()
    {
        RectTransform frameRoot =
            CreateRectTransform(
                GeneratedFrameName,
                bottomHud
            );

        StretchToParent(frameRoot);
        frameRoot.SetAsFirstSibling();

        CreateCorner(
            frameRoot,
            "TopLeftCorner",
            new Vector2(0f, 1f),
            new Vector2(
                FrameCornerSize * 0.5f,
                -FrameCornerSize * 0.5f
            ),
            new Vector3(1f, 1f, 1f)
        );

        CreateCorner(
            frameRoot,
            "TopRightCorner",
            new Vector2(1f, 1f),
            new Vector2(
                -FrameCornerSize * 0.5f,
                -FrameCornerSize * 0.5f
            ),
            new Vector3(-1f, 1f, 1f)
        );

        CreateCorner(
            frameRoot,
            "BottomLeftCorner",
            new Vector2(0f, 0f),
            new Vector2(
                FrameCornerSize * 0.5f,
                FrameCornerSize * 0.5f
            ),
            new Vector3(1f, -1f, 1f)
        );

        CreateCorner(
            frameRoot,
            "BottomRightCorner",
            new Vector2(1f, 0f),
            new Vector2(
                -FrameCornerSize * 0.5f,
                FrameCornerSize * 0.5f
            ),
            new Vector3(-1f, -1f, 1f)
        );

        CreateHorizontalEdge(
            frameRoot,
            "TopEdge",
            true
        );

        CreateHorizontalEdge(
            frameRoot,
            "BottomEdge",
            false
        );

        CreateVerticalEdge(
            frameRoot,
            "LeftEdge",
            true
        );

        CreateVerticalEdge(
            frameRoot,
            "RightEdge",
            false
        );

        frameRoot.gameObject.AddComponent<ResponsiveModularFrameFitter>()
            .SetPreferredCornerSize(FrameCornerSize);
    }

    private void CreateCorner(
        RectTransform parent,
        string objectName,
        Vector2 anchor,
        Vector2 anchoredPosition,
        Vector3 localScale)
    {
        Image image =
            CreateImage(
                objectName,
                parent,
                cornerPiece
            );

        RectTransform rect = image.rectTransform;

        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta =
            new Vector2(
                FrameCornerSize,
                FrameCornerSize
            );
        rect.localScale = localScale;

        image.type = Image.Type.Simple;
    }

    private void CreateHorizontalEdge(
        RectTransform parent,
        string objectName,
        bool top)
    {
        Image image =
            CreateImage(
                objectName,
                parent,
                normalPiece
            );

        RectTransform rect = image.rectTransform;
        float yAnchor = top ? 1f : 0f;

        rect.anchorMin = new Vector2(0f, yAnchor);
        rect.anchorMax = new Vector2(1f, yAnchor);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin =
            new Vector2(
                FrameCornerSize,
                top ? -FrameThickness : 0f
            );
        rect.offsetMax =
            new Vector2(
                -FrameCornerSize,
                top ? 0f : FrameThickness
            );

        if (!top)
        {
            rect.localScale =
                new Vector3(1f, -1f, 1f);
        }

        image.type = Image.Type.Tiled;
    }

    private void CreateVerticalEdge(
        RectTransform parent,
        string objectName,
        bool left)
    {
        Image image =
            CreateImage(
                objectName,
                parent,
                normalPiece
            );

        RectTransform rect = image.rectTransform;

        rect.anchorMin =
            new Vector2(
                left ? 0f : 1f,
                0.5f
            );
        rect.anchorMax = rect.anchorMin;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition =
            new Vector2(
                left
                    ? FrameThickness * 0.5f
                    : -FrameThickness * 0.5f,
                0f
            );

        float verticalLength =
            Mathf.Max(
                1f,
                bottomHud.rect.height -
                FrameCornerSize * 2f
            );

        rect.sizeDelta =
            new Vector2(
                verticalLength,
                FrameThickness
            );
        rect.localRotation =
            Quaternion.Euler(
                0f,
                0f,
                left ? 90f : -90f
            );

        image.type = Image.Type.Tiled;
    }

    private void KeepAbilityContentInsideFrame()
    {
        AbilityButtonUI abilityUi =
            bottomHud.GetComponentInChildren<AbilityButtonUI>(true);

        if (abilityUi == null)
        {
            return;
        }

        RectTransform abilityRect =
            abilityUi.transform as RectTransform;

        if (abilityRect == null)
        {
            return;
        }

        // Keep the authored 176x64 button centered in the existing enclosure.
        Vector2 position = abilityRect.anchoredPosition;
        position.y = 0f;
        abilityRect.anchoredPosition = position;
    }

    private void RemoveExistingFrame()
    {
        Transform existingFrame =
            bottomHud.Find(GeneratedFrameName);

        if (existingFrame == null)
        {
            return;
        }

        existingFrame.gameObject.SetActive(false);

        if (Application.isPlaying)
        {
            Destroy(existingFrame.gameObject);
        }
        else
        {
            DestroyImmediate(existingFrame.gameObject);
        }
    }

    private static RectTransform CreateRectTransform(
        string objectName,
        RectTransform parent)
    {
        GameObject rectObject =
            new GameObject(
                objectName,
                typeof(RectTransform)
            );

        rectObject.layer = parent.gameObject.layer;

        RectTransform rect =
            rectObject.GetComponent<RectTransform>();

        rect.SetParent(parent, false);
        rect.localScale = Vector3.one;

        return rect;
    }

    private static Image CreateImage(
        string objectName,
        RectTransform parent,
        Sprite sprite)
    {
        GameObject imageObject =
            new GameObject(
                objectName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image)
            );

        imageObject.layer = parent.gameObject.layer;

        RectTransform rect =
            imageObject.GetComponent<RectTransform>();

        rect.SetParent(parent, false);

        Image image = imageObject.GetComponent<Image>();
        image.sprite = sprite;
        image.color = Color.white;
        image.raycastTarget = false;
        image.preserveAspect = false;
        image.pixelsPerUnitMultiplier = 1f;

        return image;
    }

    private static void StretchToParent(
        RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
    }
}
