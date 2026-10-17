using UnityEngine;
using UnityEngine.UI;

/// <summary>Decorates existing layout owners; never moves their content or owns gameplay.</summary>
public sealed class DungeonPresentationArt : MonoBehaviour
{
    private SpriteRenderer backdrop;
    private Camera worldCamera;
    private bool panelsReady;
    private Sprite croppedReferenceFloor;

    public static Sprite Load(string name) => Resources.Load<Sprite>("UI/DungeonPresentation/" + name);

    public static void InstallBattle(GameObject root)
    {
        if (root != null && !root.TryGetComponent<DungeonPresentationArt>(out _))
            root.AddComponent<DungeonPresentationArt>();
    }

    private void Start()
    {
        worldCamera = Camera.main;
        var tile = BattlePresentationSkin.ZoneSprite("General") ?? RunSession.Current?.Zone?.Definition?.theme?.generalBackground ?? Load("DungeonBackdropTile");
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
        UpdateReferenceSides();
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
        if(BattlePresentationSkin.UsesReferenceKit)
        {
            BuildReferenceKit(player.transform,(RectTransform)bottom.transform);
            if(!TryGetComponent<BattleAmbientPresentation>(out _))gameObject.AddComponent<BattleAmbientPresentation>();
            panelsReady=true;return;
        }
        AddTiledPanel(player.transform, "PlayerDungeonBackdrop", 8, new Color(.55f,.55f,.65f,1));
        AddTiledPanel(bottom.transform, "BottomDungeonBackdrop", BattlePresentationSkin.Folder != null ? 0 : 16, new Color(.55f,.55f,.65f,1));
        if(BattlePresentationSkin.Folder!=null && !TryGetComponent<BattleAmbientPresentation>(out _))
            gameObject.AddComponent<BattleAmbientPresentation>();
        panelsReady = true;
    }

    private void BuildReferenceKit(Transform player,RectTransform bottom)
    {
        var layout=FindFirstObjectByType<GameplayPixelLayoutController>();
        var top=layout.TopHud;
        // Separate reference-registered layers; no complete-screen texture is
        // imported. Live actors, counters and controls retain their own owners.
        var layers=GameUi.Rect("ReferenceKitScenery",top,Vector2.zero,Vector2.zero);
        layers.anchorMin=Vector2.zero;layers.anchorMax=Vector2.one;layers.offsetMin=layers.offsetMax=Vector2.zero;
        layers.SetAsFirstSibling();
        bool dungeon=BattlePresentationSkin.Folder=="Dungeon";
        if(dungeon)
        {
            ReferenceLayer(layers,"FarWall",new Rect(0,0,470,180),new Vector2(470,298));
            ReferenceLayer(layers,"ArchDoor",new Rect(180,58,115,122),new Vector2(470,298));
            ReferenceLayer(layers,"BattleFloor",new Rect(137,177,333,121),new Vector2(470,298));
            ReferenceLayer(layers,"PlayerFootMasonry",new Rect(0,280,137,18),new Vector2(470,298));
            ReferenceLayer(layers,"PlayerSurround",new Rect(0,28,140,252),new Vector2(470,298));
        }
        else
        {
            var extent=new Vector2(470,BattlePresentationSkin.ReferenceTopHeight);
            ReferenceLayer(layers,"FarScenery",new Rect(0,0,470,160),extent);
            ReferenceLayer(layers,"MidScenery",new Rect(0,160,470,51),extent);
            var floor=ReferenceLayer(layers,"BattleFloor",new Rect(0,211,470,77),extent).GetComponent<Image>();
            // The source's last ten rows belong behind the board rim in the
            // assembled mockup. Exclude that overlap from the separate arena.
            var original=floor.sprite;var area=original.rect;area.y+=10;area.height-=10;
            croppedReferenceFloor=Sprite.Create(original.texture,area,Vector2.one*.5f,original.pixelsPerUnit,0,SpriteMeshType.FullRect);
            croppedReferenceFloor.name="BattleFloor_Arena";floor.sprite=croppedReferenceFloor;
        }
        var panelSize=BattlePresentationSkin.ReferencePlayerRect.size;
        var backing=ReferenceLayer((RectTransform)player,"PlayerMasonry",dungeon?new Rect(11,14,105,224):new Rect(14,18,105,224),panelSize);
        backing.SetAsFirstSibling();
        var banner=ReferenceLayer((RectTransform)player,"PlayerBanner",dungeon?new Rect(40,14,49,65):new Rect(43,18,49,66),panelSize);
        banner.SetSiblingIndex(1);
        ReferenceLayer(bottom,"BottomMasonry",new Rect(0,0,470,135),new Vector2(470,135)).SetAsFirstSibling();
        // Side strips bind to the existing GameArea, never repeat vertically.
        // Their horizontal coverage includes the gap to the viewport edge.
        foreach(bool right in new[]{false,true})
        {
            var sprite=BattlePresentationSkin.Reference(right?"RightBorderDressing":"LeftBorderDressing");
            var rect=GameUi.Rect("ReferenceKit"+sprite.name,layout.transform,Vector2.zero,Vector2.zero);
            var image=rect.gameObject.AddComponent<Image>();image.sprite=sprite;image.raycastTarget=false;
            rect.SetAsFirstSibling();referenceSides[right?1:0]=rect;
        }
        referenceLayout=layout;
        UpdateReferenceSides();
    }

    private GameplayPixelLayoutController referenceLayout;
    private readonly RectTransform[] referenceSides=new RectTransform[2];
    private void UpdateReferenceSides()
    {
        if(referenceLayout==null)return;
        var g=referenceLayout.Current;
        GameplayPixelLayoutController.SetRect(referenceSides[0],new Rect(0,g.Board.y,g.Board.x,g.Board.height));
        GameplayPixelLayoutController.SetRect(referenceSides[1],new Rect(g.Board.xMax,g.Board.y,g.Viewport.width/g.Scale-g.Board.xMax,g.Board.height));
    }

    internal static RectTransform ReferenceLayer(RectTransform parent,string name,Rect source,Vector2 extent)
    {
        var rect=GameUi.Rect("ReferenceKit"+name,parent,Vector2.zero,Vector2.zero);
        var image=rect.gameObject.AddComponent<Image>();image.sprite=BattlePresentationSkin.Reference(name);image.raycastTarget=false;
        rect.anchorMin=new Vector2(source.xMin/extent.x,1-source.yMax/extent.y);
        rect.anchorMax=new Vector2(source.xMax/extent.x,1-source.yMin/extent.y);
        rect.offsetMin=rect.offsetMax=Vector2.zero;
        rect.gameObject.AddComponent<ReferenceKitSurface>().Initialize(source,extent);
        return rect;
    }

    private static void AddTiledPanel(Transform parent, string name, float inset, Color tint)
    {
        if (parent.Find(name) != null) return;
        var theme = RunSession.Current?.Zone?.Definition?.theme;
        bool panorama = name == "BottomDungeonBackdrop" && BattlePresentationSkin.ZoneSprite("BottomPanel") != null;
        var tile = (panorama ? BattlePresentationSkin.ZoneSprite("BottomPanel") : BattlePresentationSkin.ZoneSprite("Panel"))
            ?? theme?.panelBackground ?? Load("DungeonBackdropTile");
        if (tile == null) return;
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        var image = go.GetComponent<Image>();
        image.transform.SetParent(parent, false);
        image.transform.SetAsFirstSibling();
        image.sprite = tile; image.type = Image.Type.Tiled; image.color = theme != null || BattlePresentationSkin.Folder!=null ? Color.white : tint;
        image.raycastTarget = false;
        var rect = image.rectTransform;
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.one * inset; rect.offsetMax = Vector2.one * -inset;
        if (BattlePresentationSkin.Folder != null) go.AddComponent<NativePixelUiSurface>();
        if (panorama)
        {
            // Keep the foreground at the two physical edges. A full panorama
            // tiled here repeated its columns on wider viewports.
            foreach (bool right in new[] { false, true })
            {
                var sprite = BattlePresentationSkin.ZoneSprite(right ? "BottomRight" : "BottomLeft");
                if (sprite == null) continue;
                var prop = new GameObject(right ? "BottomRightScenery" : "BottomLeftScenery", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                prop.transform.SetParent(go.transform, false);
                var art = prop.GetComponent<Image>(); art.sprite = sprite; art.raycastTarget = false;
                var bounds = art.rectTransform;
                bounds.anchorMin = bounds.anchorMax = bounds.pivot = new Vector2(right ? 1 : 0, 0);
                bounds.sizeDelta = sprite.rect.size;
                prop.AddComponent<NativePixelUiSurface>();
            }
        }
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

    private void OnDestroy()
    {
        if (backdrop != null) Destroy(backdrop.gameObject);
        if (croppedReferenceFloor != null) Destroy(croppedReferenceFloor);
    }
}
