using UnityEngine;

[DefaultExecutionOrder(-100)]
[RequireComponent(typeof(BoardController))]
public sealed class BoardVisuals : MonoBehaviour
{
    [Header("Frame")]
    [SerializeField, Min(0f)]
    private float frameThickness = 0.18f;

    [SerializeField, Min(0f)]
    private float backgroundPadding = 0.04f;

    [SerializeField]
    private Color frameColor =
        new Color32(52, 42, 59, 255);

    [SerializeField]
    private Color backgroundColor =
        new Color32(19, 23, 31, 255);

    private BoardController board;

    private Texture2D runtimeTexture;
    private Sprite runtimeSquareSprite;

    public float OuterLocalWidth =>
        board.LocalBoardWidth +
        frameThickness * 2f;

    public float OuterLocalHeight =>
        board.LocalBoardHeight +
        frameThickness * 2f;

    private void Awake()
    {
        board = GetComponent<BoardController>();

        CreateSquareSprite();
        CreateBoardFrame();
        CreateBoardMask();
    }

    private void CreateSquareSprite()
    {
        runtimeTexture = new Texture2D(
            1,
            1,
            TextureFormat.RGBA32,
            false
        );

        runtimeTexture.name =
            "Runtime Board Square Texture";

        runtimeTexture.filterMode =
            FilterMode.Point;

        runtimeTexture.wrapMode =
            TextureWrapMode.Clamp;

        runtimeTexture.SetPixel(
            0,
            0,
            Color.white
        );

        runtimeTexture.Apply();

        runtimeSquareSprite = Sprite.Create(
            runtimeTexture,
            new Rect(0f, 0f, 1f, 1f),
            new Vector2(0.5f, 0.5f),
            1f
        );

        runtimeSquareSprite.name =
            "Runtime Board Square Sprite";
    }

    private void CreateBoardFrame()
    {
        CreateSpriteObject(
            "BoardFrame",
            new Vector2(
                OuterLocalWidth,
                OuterLocalHeight
            ),
            frameColor,
            0
        );

        CreateSpriteObject(
            "BoardBackground",
            new Vector2(
                board.LocalBoardWidth +
                backgroundPadding * 2f,
                board.LocalBoardHeight +
                backgroundPadding * 2f
            ),
            backgroundColor,
            1
        );
    }

    private void CreateSpriteObject(
        string objectName,
        Vector2 size,
        Color color,
        int sortingOrder)
    {
        GameObject visualObject =
            new GameObject(objectName);

        visualObject.transform.SetParent(
            transform,
            false
        );

        visualObject.transform.localPosition =
            Vector3.zero;

        visualObject.transform.localScale =
            new Vector3(
                size.x,
                size.y,
                1f
            );

        SpriteRenderer renderer =
            visualObject.AddComponent<SpriteRenderer>();

        renderer.sprite = runtimeSquareSprite;
        renderer.color = color;

        renderer.sortingLayerName =
            "BoardBackground";

        renderer.sortingOrder = sortingOrder;
    }

    private void CreateBoardMask()
    {
        GameObject maskObject =
            new GameObject("BoardClipMask");

        maskObject.transform.SetParent(
            transform,
            false
        );

        maskObject.transform.localPosition =
            Vector3.zero;

        maskObject.transform.localScale =
            new Vector3(
                board.LocalBoardWidth,
                board.LocalBoardHeight,
                1f
            );

        SpriteMask spriteMask =
            maskObject.AddComponent<SpriteMask>();

        spriteMask.sprite =
            runtimeSquareSprite;

        spriteMask.alphaCutoff = 0.01f;
    }

    private void OnDestroy()
    {
        if (runtimeSquareSprite != null)
        {
            Destroy(runtimeSquareSprite);
        }

        if (runtimeTexture != null)
        {
            Destroy(runtimeTexture);
        }
    }
}