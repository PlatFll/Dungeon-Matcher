using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Approved native modules. Existing battle owners retain layout and behavior.</summary>
public sealed class ApprovedDungeonBattleArt : ScriptableObject
{
    public TextAsset placementManifest;
    public Sprite[] modules;

    [Serializable] public class Asset { public string name, group; }
    [Serializable] public class Placement { public string name; public int x, y, layer; }
    [Serializable] public class Manifest { public Asset[] assets; public Placement[] placements; }

    private Manifest manifest;
    private Dictionary<string, Sprite> sprites;
    private Dictionary<string, Asset> assets;

    public static ApprovedDungeonBattleArt Current => BattlePresentationSkin.Folder == "Dungeon"
        ? Resources.Load<ApprovedDungeonBattleArt>("UI/BattlePresentation/ApprovedDungeon") : null;

    public Manifest Data
    {
        get { Initialize(); return manifest; }
    }

    private void Initialize()
    {
        if (manifest != null) return;
        manifest = JsonUtility.FromJson<Manifest>(placementManifest.text);
        sprites = modules.ToDictionary(s => s.name);
        assets = manifest.assets.ToDictionary(a => a.name);
    }

    public Sprite Sprite(string name) { Initialize(); return sprites[name]; }
    public string Group(string name) { Initialize(); return assets[name].group; }
    public Placement Find(string name) => Data.placements.First(p => p.name == name);

    public RectTransform Ui(RectTransform parent, string name, Rect source, Vector2 extent)
    {
        var rect = GameUi.Rect("ApprovedDungeon" + name, parent, Vector2.zero, Vector2.zero);
        var image = rect.gameObject.AddComponent<Image>();
        image.sprite = Sprite(name);
        image.raycastTarget = false;
        rect.gameObject.AddComponent<ReferenceKitSurface>().Initialize(source, extent);
        return rect;
    }

    public RectTransform Ui(RectTransform parent, Placement placement, Rect region)
    {
        var sprite = Sprite(placement.name);
        return Ui(parent, placement.name,
            new Rect(placement.x - region.x, placement.y - region.y, sprite.rect.width, sprite.rect.height), region.size);
    }

    public void PlayerFrame(RectTransform parent)
    {
        foreach (var placement in Data.placements.Where(p => Group(p.name) == "Frames_Player"))
            Ui(parent, placement, new Rect(9, 20, 99, 190));
    }

    public void WaveFrame(Image image)
    {
        image.enabled = false;
        var root = GameUi.Rect("ApprovedDungeonWave", image.rectTransform, Vector2.zero, Vector2.zero);
        root.anchorMin = Vector2.zero;
        root.anchorMax = Vector2.one;
        root.offsetMin = root.offsetMax = Vector2.zero;
        root.SetAsFirstSibling(); // The live wave label stays above the decorative pieces.
        Ui(root, "SignFill", new Rect(6, 6, 97, 17), new Vector2(109, 41));
        foreach (var placement in Data.placements.Where(p =>
                     (Group(p.name) == "Frames_Top" && p.name.StartsWith("Wave")) || p.name == "WaveSkull"))
            Ui(root, placement, new Rect(130, 15, 109, 41));
    }

    // Map the new rim's opening to the existing board and reserved border extents.
    // Source-art replacement must not change the gameplay fit ratio.
    private static float Map(float value, float outerStart, float innerStart, float innerEnd,
        float outerEnd, float leadingBorder, float length, float trailingBorder)
    {
        if (value < innerStart)
            return Mathf.LerpUnclamped(-leadingBorder, 0, (value - outerStart) / (innerStart - outerStart));
        if (value > innerEnd)
            return Mathf.LerpUnclamped(length, length + trailingBorder, (value - innerEnd) / (outerEnd - innerEnd));
        return (value - innerStart) / (innerEnd - innerStart) * length;
    }

    public Rect BoardRect(string name, float width, float height)
    {
        var placement = Find(name);
        var sprite = Sprite(name);
        float x0 = Map(placement.x, 25, 40, 320, 334, width * 16 / 364, width, width * 20 / 364) - width / 2;
        float x1 = Map(placement.x + sprite.rect.width, 25, 40, 320, 334, width * 16 / 364, width, width * 20 / 364) - width / 2;
        float y0 = Map(placement.y, 224, 238, 518, 529, height * 18 / 369, height, height * 19 / 369);
        float y1 = Map(placement.y + sprite.rect.height, 224, 238, 518, 529, height * 18 / 369, height, height * 19 / 369);
        return Rect.MinMaxRect(x0, height / 2 - y1, x1, height / 2 - y0);
    }

    public void BoardFrame(Transform root, float width, float height, Action<SpriteRenderer, Sprite> configure)
    {
        foreach (var placement in Data.placements.Where(p => Group(p.name) == "Frames_Board"))
        {
            var sprite = Sprite(placement.name);
            var rect = BoardRect(placement.name, width, height);
            var piece = new GameObject("ApprovedDungeon" + placement.name);
            piece.transform.SetParent(root, false);
            piece.transform.localPosition = rect.center;
            piece.transform.localScale = new Vector3(rect.width * 64 / sprite.rect.width, rect.height * 64 / sprite.rect.height, 1);
            configure(piece.AddComponent<SpriteRenderer>(), sprite);
        }
    }
}
