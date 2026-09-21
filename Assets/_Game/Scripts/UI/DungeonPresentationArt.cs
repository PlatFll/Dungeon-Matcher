using UnityEngine;
using UnityEngine.UI;

/// <summary>Decorates existing layout owners; never moves their content or owns gameplay.</summary>
public sealed class DungeonPresentationArt : MonoBehaviour
{
    private SpriteRenderer backdrop;
    private Camera worldCamera;
    private bool panelsReady;

    public static Sprite Load(string name) => Resources.Load<Sprite>("UI/DungeonPresentation/" + name);

    public static void InstallBattle(GameObject root)
    {
        if (root != null && !root.TryGetComponent<DungeonPresentationArt>(out _))
            root.AddComponent<DungeonPresentationArt>();
    }

    private void Start()
    {
        worldCamera = Camera.main;
        var tile = Load("DungeonBackdropTile");
        if (tile == null || worldCamera == null) return;
        var go = new GameObject("DungeonSurroundingMasonry");
        backdrop = go.AddComponent<SpriteRenderer>();
        backdrop.sprite = tile;
        backdrop.drawMode = SpriteDrawMode.Tiled;
        backdrop.sortingLayerName = "Default";
        backdrop.sortingOrder = -200;
    }

    private void LateUpdate()
    {
        if (backdrop != null && worldCamera != null && worldCamera.orthographic)
        {
            float height = worldCamera.orthographicSize * 2f;
            var position = worldCamera.transform.position;
            backdrop.transform.position = new Vector3(Mathf.Round(position.x * 64) / 64,
                Mathf.Round(position.y * 64) / 64, 10);
            backdrop.size = new Vector2(Mathf.Ceil(height * worldCamera.aspect * 64) / 64 + 2,
                Mathf.Ceil(height * 64) / 64 + 2);
        }
        if (panelsReady) return;
        var player = GameObject.Find("PlayerSection");
        var bottom = GameObject.Find("BottomHUD");
        if (player == null || bottom == null) return;
        AddTiledPanel(player.transform, "PlayerDungeonBackdrop", 8, new Color(.55f,.55f,.65f,1));
        AddTiledPanel(bottom.transform, "BottomDungeonBackdrop", 16, Color.white);
        panelsReady = true;
    }

    private static void AddTiledPanel(Transform parent, string name, float inset, Color tint)
    {
        if (parent.Find(name) != null) return;
        var tile = Load("DungeonBackdropTile");
        if (tile == null) return;
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        var image = go.GetComponent<Image>();
        image.transform.SetParent(parent, false);
        image.transform.SetAsFirstSibling();
        image.sprite = tile; image.type = Image.Type.Tiled; image.color = tint;
        image.raycastTarget = false;
        var rect = image.rectTransform;
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.one * inset; rect.offsetMax = Vector2.one * -inset;
    }

    public static void InstallMenu(GameObject home, RectTransform title)
    {
        var scene = Load("MenuDungeon");
        var logo = Load("DungeonMatcherLogo");
        if (scene == null || logo == null || home == null) return;
        var canvas = home.GetComponentInParent<Canvas>();
        if (canvas == null) return;
        var existingBackdrop = canvas.transform.Find("Backdrop");
        if (existingBackdrop != null && existingBackdrop.TryGetComponent<Image>(out var old))
            old.enabled = false;
        if (home.TryGetComponent<Image>(out var fill)) fill.color = Color.clear;
        var background = new GameObject("MenuDungeonArtwork", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        background.transform.SetParent(canvas.transform, false); background.transform.SetAsFirstSibling();
        var image = background.GetComponent<Image>();image.sprite = scene;image.raycastTarget = false;
        background.AddComponent<NativePixelArtwork>().Initialize(scene, true, 0);
        if (title != null) title.gameObject.SetActive(false);
        var emblem = new GameObject("DungeonMatcherLogo", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        emblem.transform.SetParent(home.transform, false);
        var emblemImage = emblem.GetComponent<Image>();emblemImage.sprite = logo;emblemImage.raycastTarget = false;
        emblem.AddComponent<NativePixelArtwork>().Initialize(logo, false, 270);
    }

    private void OnDestroy() { if (backdrop != null) Destroy(backdrop.gameObject); }
}
