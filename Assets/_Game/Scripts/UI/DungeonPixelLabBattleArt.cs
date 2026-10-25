using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Approved Dungeon artwork and native UI construction, used by the existing presentation owners.</summary>
public sealed partial class DungeonPixelLabBattleArt : ScriptableObject
{
    public ForestBattleAssembly forestAssembly;
    public Sprite[] modules;
    public TextAsset placementManifest;
    public Sprite boardCell64;
    public Sprite[] hybridModules;
    public Sprite[] sideArchitectureModules;
    public bool UsesSideArchitecture => sideArchitectureModules != null && sideArchitectureModules.Length > 0;
    public TextAsset hybridPlacementManifest;
    public bool UsesHybrid => hybridPlacementManifest && hybridModules != null && hybridModules.Length > 0;
    Dictionary<string, Sprite> lookup;
    public static DungeonPixelLabBattleArt Current => BattlePresentationSkin.Folder == "Dungeon"
        ? Resources.Load<DungeonPixelLabBattleArt>("UI/BattlePresentation/DungeonPixelLab")
        : BattlePresentationSkin.Folder == "Forest" ? Resources.Load<DungeonPixelLabBattleArt>("UI/BattlePresentation/ForestPixelLab") : null;
    public Sprite Sprite(string name)
    {
        if (lookup == null)
        {
            lookup = modules.ToDictionary(s => s.name, s => s);
            if (UsesHybrid) foreach (var sprite in hybridModules) lookup[sprite.name] = sprite;
            if (UsesSideArchitecture) foreach (var sprite in sideArchitectureModules) lookup[sprite.name] = sprite;
            if (forestAssembly) foreach (var module in forestAssembly.modules) lookup[module.key] = module.sprite;
        }
        return lookup.TryGetValue(name, out var s) ? s : null;
    }
    DungeonSideViewArtPreview.Manifest manifest;
    public DungeonSideViewArtPreview.Manifest Manifest => manifest ??= JsonUtility.FromJson<DungeonSideViewArtPreview.Manifest>((UsesHybrid ? hybridPlacementManifest : placementManifest).text);
    public static int PhysicalPixelScale => BattleFloorAnchor.PhysicalPixelScale;
    public static float Unit(Canvas canvas) => PhysicalPixelScale / canvas.rootCanvas.scaleFactor;
    public static RectTransform Rect(string name, Transform parent, float x, float y, float width, float height)
    {
        var r = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        r.SetParent(parent, false); TopRect(r, x, y, width, height); return r;
    }
    public static void TopRect(RectTransform r, float x, float y, float width, float height)
    {
        r.anchorMin = r.anchorMax = r.pivot = new Vector2(0, 1);
        r.localScale = Vector3.one; r.anchoredPosition = new Vector2(x, -y); r.sizeDelta = new Vector2(width, height);
    }
    public Image Image(string name, Transform parent, float x, float y, float unit = 1)
    {
        var s = Sprite(name); var r = Rect(name, parent, x, y, s.rect.width * unit, s.rect.height * unit);
        var image = r.gameObject.AddComponent<Image>(); image.sprite = s; image.raycastTarget = false; return image;
    }
    public RectTransform Frame(Transform parent, string prefix, float pitch, float unit)
    {
        var r = Rect("PixelLab" + prefix + "Frame", parent, 0, 0, 0, 0);
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero;
        string[] names = { "TopLeftCorner", "TopRightCorner", "BottomLeftCorner", "BottomRightCorner", "TopEdge", "BottomEdge", "LeftEdge", "RightEdge" };
        string[] suffixes = { "Corner_TL", "Corner_TR", "Corner_BL", "Corner_BR", "Rail_Top", "Rail_Bottom", "Rail_Left", "Rail_Right" };
        for (int i = 0; i < names.Length; i++)
        {
            var image = Rect(names[i], r, 0, 0, 0, 0).gameObject.AddComponent<Image>();
            image.sprite = Sprite(prefix + "_" + suffixes[i]); image.raycastTarget = false;
        }
        r.gameObject.AddComponent<ResponsiveModularFrameFitter>().ConfigureNativePieces(Sprite(prefix + "_Clamp"), pitch, unit);
        return r;
    }
    public void Panel(RectTransform root, string prefix, string backing, int inset, float pitch)
    {
        float unit = Unit(root.GetComponentInParent<Canvas>());
        if (UsesHybrid) DecorateHybridPanel(root, prefix, unit);
        if (root.Find("PixelLab" + prefix + "Frame"))
        {
            var existing=root.Find("PixelLabInteriorMask") as RectTransform;
            if(existing)
            {
                existing.offsetMin=Vector2.one*inset*unit;existing.offsetMax=-existing.offsetMin;
                var fill=existing.GetComponentInChildren<Image>();
                fill.pixelsPerUnitMultiplier=1/unit;
                fill.rectTransform.offsetMin=-fill.sprite.rect.size*unit;fill.rectTransform.offsetMax=-fill.rectTransform.offsetMin;
            }
            return;
        }
        if (!string.IsNullOrEmpty(backing))
        {
            var mask = Rect("PixelLabInteriorMask", root, 0, 0, 0, 0);
            mask.anchorMin = Vector2.zero; mask.anchorMax = Vector2.one;
            mask.offsetMin = Vector2.one * inset * unit; mask.offsetMax = -mask.offsetMin;
            mask.gameObject.AddComponent<RectMask2D>(); mask.SetAsFirstSibling();
            var fill = Image(backing, mask, 0, 0, unit);
            fill.type = UnityEngine.UI.Image.Type.Tiled; fill.pixelsPerUnitMultiplier = 1 / unit;
            fill.rectTransform.anchorMin = Vector2.zero; fill.rectTransform.anchorMax = Vector2.one;
            fill.rectTransform.offsetMin = -fill.sprite.rect.size * unit; fill.rectTransform.offsetMax = -fill.rectTransform.offsetMin;
        }
        Frame(root, prefix, pitch, unit);
        if (root.TryGetComponent<NativePixelUiSurface>(out var native)) native.enabled = false;
    }
    public void PlaceControl(RectTransform root, string name, RectTransform top)
    {
        float unit = Unit(top.GetComponentInParent<Canvas>());
        var p = Manifest.panels.First(x => x.name == name);
        float nativeWidth = top.rect.width / unit;
        float x = p.x + (name == "Wave" ? (nativeWidth - 360) / 2 : nativeWidth - 360);
        TopRect(root, Mathf.Round(x) * unit, p.y * unit, p.width * unit, p.height * unit);
        string family = UsesHybrid ? (name == "Wave" ? "Wave" : "Control") : "Plaque";
        Panel(root,family,family+"_Inset",UsesHybrid?6:7,UsesHybrid?1000:72);
        GameplayPixelGrid.Snap(root);
        var text=root.GetComponentInChildren<TMPro.TMP_Text>();
        if(text)
        {
            text.rectTransform.anchorMin=Vector2.zero;text.rectTransform.anchorMax=Vector2.one;
            text.rectTransform.offsetMin=new Vector2(7,3)*unit;text.rectTransform.offsetMax=-text.rectTransform.offsetMin;
            text.textWrappingMode=TMPro.TextWrappingModes.NoWrap;
            PixelTextFitter.ApplyNativeMenu(text,(UsesHybrid && name == "Wave" ? 24 : 16)*unit);
        }
        var gear=root.Find("Plaque_Gear") as RectTransform;
        if(gear) {var s=Sprite("Plaque_Gear");TopRect(gear,Mathf.Floor((root.rect.width/unit-s.rect.width)/2)*unit,Mathf.Floor((root.rect.height/unit-s.rect.height)/2)*unit,s.rect.width*unit,s.rect.height*unit);GameplayPixelGrid.Snap(gear);}

    }
    public void SkinControl(RectTransform root, string name, RectTransform top)
    {
        PlaceControl(root, name, top);
        if (root.TryGetComponent<Image>(out var image)) { image.sprite = null; image.color = Color.clear; }
        // PlaceControl already builds the correct approved modular family.
        if (name == "Settings" && !root.Find("Plaque_Gear"))
        {
            float unit = Unit(root.GetComponentInParent<Canvas>()); var s = Sprite("Plaque_Gear");
            Image("Plaque_Gear", root, Mathf.Floor((root.rect.width / unit - s.rect.width) / 2) * unit,
                Mathf.Floor((root.rect.height / unit - s.rect.height) / 2) * unit, unit);
        }
    }
}
