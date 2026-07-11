using UnityEngine;

public class BoardController : MonoBehaviour
{
    [Header("Board Size")]
    [SerializeField, Min(1)] private int width = 7;
    [SerializeField, Min(1)] private int height = 8;

    [Header("Gem Layout")]
    [SerializeField, Min(0.1f)] private float cellSize = 1f;
    [SerializeField, Range(0.1f, 1f)] private float gemScale = 0.86f;

    private Gem[,] gems;
    private Gem selectedGem;

    private Texture2D temporaryTexture;
    private Sprite temporarySprite;

    private readonly Color[] gemColors =
    {
        new Color32(220, 45, 55, 255),    // Ruby
        new Color32(245, 125, 35, 255),   // Amber
        new Color32(245, 210, 45, 255),   // Topaz
        new Color32(45, 190, 90, 255),    // Emerald
        new Color32(55, 120, 230, 255),   // Sapphire
        new Color32(155, 75, 210, 255)    // Amethyst
    };

    private void Start()
    {
        CreateTemporarySprite();
        GenerateBoard();
    }

    private void CreateTemporarySprite()
    {
        temporaryTexture = new Texture2D(
            1,
            1,
            TextureFormat.RGBA32,
            false
        );

        temporaryTexture.name = "Temporary Gem Texture";
        temporaryTexture.filterMode = FilterMode.Point;
        temporaryTexture.SetPixel(0, 0, Color.white);
        temporaryTexture.Apply();

        temporarySprite = Sprite.Create(
            temporaryTexture,
            new Rect(0, 0, 1, 1),
            new Vector2(0.5f, 0.5f),
            1f
        );

        temporarySprite.name = "Temporary Gem Sprite";
    }

    private void GenerateBoard()
    {
        gems = new Gem[width, height];

        float startX = -(width - 1) * cellSize * 0.5f;
        float startY = -(height - 1) * cellSize * 0.5f;

        for (int row = 0; row < height; row++)
        {
            for (int column = 0; column < width; column++)
            {
                CreateGem(column, row, startX, startY);
            }
        }
    }

    private void CreateGem(
        int column,
        int row,
        float startX,
        float startY)
    {
        GameObject gemObject = new GameObject(
            $"Gem_{column}_{row}"
        );

        gemObject.transform.SetParent(transform, false);

        gemObject.transform.localPosition = new Vector3(
            startX + column * cellSize,
            startY + row * cellSize,
            0f
        );

        SpriteRenderer spriteRenderer =
            gemObject.AddComponent<SpriteRenderer>();

        spriteRenderer.sprite = temporarySprite;
        spriteRenderer.sortingLayerName = "Gems";

        BoxCollider2D boxCollider =
            gemObject.AddComponent<BoxCollider2D>();

        boxCollider.size = Vector2.one;

        Gem gem = gemObject.AddComponent<Gem>();

        int typeIndex = Random.Range(0, gemColors.Length);
        GemType gemType = (GemType)typeIndex;

        gem.Initialize(
            this,
            column,
            row,
            gemType,
            gemColors[typeIndex],
            gemScale
        );

        gems[column, row] = gem;
    }

    public void SelectGem(Gem gem)
    {
        if (selectedGem == gem)
        {
            selectedGem.SetSelected(false);
            selectedGem = null;
            return;
        }

        if (selectedGem != null)
        {
            selectedGem.SetSelected(false);
        }

        selectedGem = gem;
        selectedGem.SetSelected(true);

        Debug.Log(
            $"Selected {gem.Type} at " +
            $"column {gem.Column}, row {gem.Row}"
        );
    }

    private void OnDestroy()
    {
        if (temporarySprite != null)
        {
            Destroy(temporarySprite);
        }

        if (temporaryTexture != null)
        {
            Destroy(temporaryTexture);
        }
    }
}