using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Art-only assembly. No run, board, combat, input, or save owners.</summary>
public sealed class DungeonModularArtPreview : MonoBehaviour
{
    public Camera previewCamera;
    public Canvas previewCanvas;
    public Sprite[] modules;
    public TextAsset placementManifest;
    public int width = 1080, height = 1920;
    public Rect safeArea = new Rect(0, 0, 1080, 1920);
    public string waveLabel = "WAVE 1", guideLabel = "GUIDE";
    public GameplayPixelLayoutController.Geometry Geometry { get; private set; }
    public RectTransform PortraitInterior { get; private set; }
    public RectTransform BoardInterior { get; private set; }
    public readonly List<RectTransform> FixedPieces = new List<RectTransform>();
    readonly Dictionary<string, Sprite> lookup = new Dictionary<string, Sprite>();
    [Serializable] public class Placement { public string name, group; public int x, y, width, height; }
    [Serializable] public class Manifest { public Placement[] placements; }

    Sprite Get(string name)
    {
        if (!lookup.TryGetValue(name, out var value) || value == null)
            throw new InvalidOperationException("Missing modular preview asset: " + name);
        return value;
    }
    static RectTransform RectNode(string name, Transform parent, Rect bounds)
    {
        var r = GameUi.Rect(name, parent, bounds.size, Vector2.zero);
        GameplayPixelLayoutController.SetRect(r, bounds);
        return r;
    }
    Image ImageNode(string asset, Transform parent, Rect bounds, bool tiled = false)
    {
        var r = RectNode(asset, parent, bounds);
        var i = r.gameObject.AddComponent<Image>();
        i.sprite = Get(asset); i.raycastTarget = false;
        i.type = tiled ? Image.Type.Tiled : Image.Type.Simple;
        i.pixelsPerUnitMultiplier = 1;
        return i;
    }
    RectTransform Mask(string name, Transform parent, Rect bounds)
    {
        var r = RectNode(name, parent, bounds);
        r.gameObject.AddComponent<RectMask2D>();
        return r;
    }
    void Native(string name, Transform parent, float x, float y)
    {
        var s = Get(name);
        var i = ImageNode(name, parent, new Rect(Mathf.Round(x), Mathf.Round(y), s.rect.width, s.rect.height));
        FixedPieces.Add(i.rectTransform);
    }
    void Tiles(Transform parent, Rect bounds, string prefix)
    {
        var clip = Mask(prefix + "Field", parent, bounds);
        for (int y = 0; y < Mathf.CeilToInt(bounds.height / 64); y++)
            for (int x = 0; x < Mathf.CeilToInt(bounds.width / 64); x++)
                Native(prefix + (char)('A' + (x + y * 3) % 4), clip, x * 64, y * 64);
    }
    void FloorTilesFromTop(Transform parent, Rect bounds, string prefix)
    {
        var clip=Mask(prefix+"Field",parent,bounds);
        for(int y=0;y<Mathf.CeilToInt(bounds.height/64);y++)
            for(int x=0;x<Mathf.CeilToInt(bounds.width/64);x++)
                Native(prefix+(char)('A'+(x+y*3)%4),clip,x*64,bounds.height-(y+1)*64);
    }
    // Separate fixed corners; only straight sections tile. Different thicknesses
    // on each side are retained rather than forcing a square nine-slice border.
    RectTransform Frame(string name, Transform parent, Rect bounds, int corner,
        int left, int right, int top, int bottom, string fill = null, bool board = false)
    {
        var r = RectNode("Preview" + name + "Frame", parent, bounds);
        var inner = Mask(name + "Interior", r, new Rect(left, bottom, bounds.width-left-right, bounds.height-top-bottom));
        if (fill != null) ImageNode(fill, inner, new Rect(Vector2.zero, inner.sizeDelta), true);
        string suffix = board ? "Rail" : "";
        float cornerWidth=Get(name+"TL").rect.width;
        ImageNode(name+"Top"+suffix, r, new Rect(cornerWidth,bounds.height-top,bounds.width-2*cornerWidth,top),true);
        ImageNode(name+"Bottom"+suffix, r, new Rect(cornerWidth,0,bounds.width-2*cornerWidth,bottom),true);
        ImageNode(name+"Left"+suffix, r, new Rect(0,corner,left,bounds.height-2*corner),true);
        ImageNode(name+"Right"+suffix, r, new Rect(bounds.width-right,corner,right,bounds.height-2*corner),true);
        Native(name+"TL",r,0,bounds.height-corner); Native(name+"TR",r,bounds.width-cornerWidth,bounds.height-corner);
        Native(name+"BL",r,0,0); Native(name+"BR",r,bounds.width-cornerWidth,0);
        return inner;
    }
    static Rect SourceRect(Rect target, float x, float y, float w, float h, float sourceHeight = 298)
    {
        float x0 = Mathf.Round(x * target.width / 470), x1 = Mathf.Round((x+w)*target.width/470);
        float y0 = Mathf.Round(target.height-(y+h)*target.height/sourceHeight), y1 = Mathf.Round(target.height-y*target.height/sourceHeight);
        return new Rect(x0,y0,x1-x0,y1-y0);
    }
    void Label(string text, RectTransform inner, int fontSize)
    {
        var label=GameUi.Label("EditableLabel", inner, text, inner.sizeDelta, Vector2.zero, fontSize);
        // These fixed preview labels use the shared pixel font at a whole native
        // em. Full line metrics over-shrink its short capitals inside this plaque.
        label.GetComponent<PixelTextFitter>().enabled=false;
        label.enableAutoSizing=false;label.fontSize=32;
        label.textWrappingMode=TMPro.TextWrappingModes.NoWrap;
        float step=16f/Geometry.Scale;
        while(label.fontSize>step && label.GetPreferredValues(text,float.PositiveInfinity,float.PositiveInfinity).x>inner.sizeDelta.x-2)
            label.fontSize-=step;
    }

    [ContextMenu("Rebuild art preview from modules")]
    public void Rebuild()
    {
        if (previewCamera == null || previewCanvas == null || placementManifest == null)
            throw new InvalidOperationException("Preview camera, canvas and placement manifest are required.");
        Geometry = GameplayPixelLayoutController.Calculate(width,height,safeArea,new Vector2(568,568),320,true);
        if (!Geometry.Fits) throw new InvalidOperationException("Unsupported preview safe area.");
        lookup.Clear(); FixedPieces.Clear();
        foreach (var s in modules) if (s != null) lookup.Add(s.name,s);
        var old = previewCanvas.transform.Find("PreviewContent");
        if (old != null) { old.gameObject.SetActive(false); if (Application.isPlaying) Destroy(old.gameObject); else DestroyImmediate(old.gameObject); }
        previewCamera.orthographic = true; previewCamera.orthographicSize = height/128f;
        previewCamera.aspect = (float)width/height; previewCamera.transform.position = new Vector3(0,0,-10);
        previewCamera.clearFlags = CameraClearFlags.SolidColor; previewCamera.backgroundColor = new Color32(10,13,17,255);
        previewCamera.allowHDR = false; previewCamera.allowMSAA = false;
        previewCanvas.renderMode = RenderMode.WorldSpace; previewCanvas.worldCamera = previewCamera;
        previewCanvas.referencePixelsPerUnit = 64; previewCanvas.pixelPerfect = true;
        var canvasRect = (RectTransform)previewCanvas.transform;
        canvasRect.pivot = Vector2.zero; canvasRect.sizeDelta = new Vector2(width,height);
        canvasRect.position = new Vector3(-width/128f,-height/128f,0);
        canvasRect.localScale = Vector3.one/64;
        var root = RectNode("PreviewContent",canvasRect,new Rect(Geometry.Viewport.position,Geometry.Viewport.size/Geometry.Scale));
        root.localScale = Vector3.one*Geometry.Scale;
        root.gameObject.AddComponent<RectMask2D>();
        Tiles(root,new Rect(Vector2.zero,root.sizeDelta),"OuterWall");
        var top = RectNode("PreviewBattle",root,Geometry.Top);
        Tiles(top,new Rect(Vector2.zero,Geometry.Top.size),"BattleWall");
        var floor = SourceRect(Geometry.Top,137,177,333,121);

        // The mockup's floor recedes into compressed stone courses. Separate
        // native tile families carry that depth without scaling any texture.
        float surfaceHeight=Mathf.Max(1,floor.height-23);
        float rearHeight=Mathf.Round(surfaceHeight*.25f),nearHeight=Mathf.Round(surfaceHeight*.5f);
        float middleHeight=surfaceHeight-rearHeight-nearHeight;
        FloorTilesFromTop(top,new Rect(floor.x,floor.y+23,floor.width,nearHeight),"Floor");
        FloorTilesFromTop(top,new Rect(floor.x,floor.y+23+nearHeight,floor.width,middleHeight),"FloorMiddle");
        FloorTilesFromTop(top,new Rect(floor.x,floor.y+23+nearHeight+middleHeight,floor.width,rearHeight),"FloorBack");
        var bottom = RectNode("PreviewLower",root,Geometry.Bottom);
        Tiles(bottom,new Rect(Vector2.zero,Geometry.Bottom.size),"HudWall");

        // Static warm masonry remains behind the independently editable torches.
        var leftLight=SourceRect(Geometry.Top,135,95,25,65);
        Native("TorchLitMasonryLeft",top,leftLight.center.x-32,leftLight.y+8);
        var rightPillar=SourceRect(Geometry.Top,354,0,23,180);
        Native("TorchLitMasonryRight",top,rightPillar.center.x-25,rightPillar.y+28);

        // Foreground architectural pieces are separate native-sized sprites.
        var manifest = JsonUtility.FromJson<Manifest>(placementManifest.text);
        Array.Sort(manifest.placements,(a,b)=>PropDepth(a.name).CompareTo(PropDepth(b.name)));
        foreach (var p in manifest.placements)
        {
            if (p.name == "PortraitBanner" || p.name == "BattleFoundation") continue;
            if (p.y < 298)
            {
                var mapped = SourceRect(Geometry.Top,p.x,p.y,p.width,p.height);
                if(p.name=="RightTorch")
                {
                    var pillar=SourceRect(Geometry.Top,354,0,23,180);
                    Native(p.name,top,Mathf.Round(pillar.center.x-23/2f)+5,pillar.y+20);
                }
                else Native(p.name,top,mapped.center.x-p.width/2f,mapped.y);
            }
            else if (p.y >= 701)
            {
                var mapped = SourceRect(Geometry.Bottom,p.x,p.y-701,p.width,p.height,135);
                Native(p.name,bottom,mapped.center.x-p.width/2f,mapped.y);
            }
            else
            {
                float x = p.x < 235 ? 0 : root.sizeDelta.x-p.width;
                float y = Geometry.Board.yMax-(p.y-298)*Geometry.Board.height/403-p.height;
                Native(p.name,root,x,y);
            }
        }
        Native("FoundationStart",top,floor.x,floor.y);
        ImageNode("FoundationTopRail",top,new Rect(floor.x+24,floor.y,floor.width-54,23),true);
        Native("FoundationEnd",top,floor.xMax-30,floor.y);

        var portraitBounds = SourceRect(Geometry.Top,11,28,129,252);
        PortraitInterior = Frame("Portrait",top,portraitBounds,18,11,13,13,13);
        Tiles(PortraitInterior,new Rect(Vector2.zero,PortraitInterior.sizeDelta),"PortraitWall");
        var banner = Get("PortraitBanner");
        Native("PortraitBanner",PortraitInterior,(PortraitInterior.sizeDelta.x-banner.rect.width)/2,PortraitInterior.sizeDelta.y-banner.rect.height-3);
        PortraitCollars((RectTransform)PortraitInterior.parent);
        var health=RectNode("PreviewEmptyHealthFrame",PortraitInterior,new Rect(4,7,PortraitInterior.sizeDelta.x-8,20));
        ImageNode("HudDark",health,new Rect(0,0,health.sizeDelta.x,20),true);
        var hs=Get("PlayerHealthStart");var he=Get("PlayerHealthEnd");
        ImageNode("PlayerHealthMiddle",health,new Rect(hs.rect.width,0,health.sizeDelta.x-hs.rect.width-he.rect.width,20),true);
        Native("PlayerHealthStart",health,0,0);Native("PlayerHealthEnd",health,health.sizeDelta.x-he.rect.width,0);

        BoardInterior = Frame("Board",root,Geometry.Board,24,16,20,18,19,"HudDark",true);
        // Eight empty cells per axis; native eight-pixel borders stay fixed while
        // their quiet interiors tile. There are no board rules or gem data.
        for(int row=0;row<8;row++)for(int col=0;col<8;col++)
        {
            float x0=Mathf.Floor(col*BoardInterior.sizeDelta.x/8),x1=Mathf.Floor((col+1)*BoardInterior.sizeDelta.x/8);
            float y0=Mathf.Floor(row*BoardInterior.sizeDelta.y/8),y1=Mathf.Floor((row+1)*BoardInterior.sizeDelta.y/8);
            ImageNode("EmptyCell"+(col+row)%3,BoardInterior,new Rect(x0,y0,x1-x0,y1-y0),true);
        }
        Native("BoardCenterFinial",BoardInterior.parent,(Geometry.Board.width-20)/2,Geometry.Board.height-18);

        var wave = Frame("Wave",top,SourceRect(Geometry.Top,171,21,135,38),17,10,10,9,9,"HudDark");
        Label(waveLabel,wave,24);
        Native("WaveSkull",wave.parent,(((RectTransform)wave.parent).sizeDelta.x-27)/2,-9);
        var guide = Frame("Guide",top,SourceRect(Geometry.Top,331,29,79,39),13,9,9,8,8,"HudDark");
        Label(guideLabel,guide,19);
        var settings = Frame("Settings",top,SourceRect(Geometry.Top,412,29,45,40),12,7,7,7,7,"HudDark");
        Native("SettingsGear",settings,(settings.sizeDelta.x-22)/2,(settings.sizeDelta.y-24)/2);
        var energyBounds=SourceRect(Geometry.Bottom,162,4,147,24,135);energyBounds.height=24;
        var energy=RectNode("PreviewEnergyFrame",bottom,energyBounds);
        ImageNode("HudDark",energy,new Rect(13,6,energyBounds.width-28,12),true);
        ImageNode("EnergyTopRail",energy,new Rect(13,18,energyBounds.width-28,6),true);
        ImageNode("EnergyBottomRail",energy,new Rect(13,0,energyBounds.width-28,6),true);
        ImageNode("EnergyPurpleFill",energy,new Rect(13,8,Mathf.Floor((energyBounds.width-28)*.52f),8),true);
        Native("EnergyStart",energy,0,0);Native("EnergyEnd",energy,energyBounds.width-15,0);
        var ability=Frame("Ability",bottom,SourceRect(Geometry.Bottom,148,30,176,66,135),16,14,14,10,12,"AbilityDark");
        var abilityFrame=(RectTransform)ability.parent;
        Native("AbilityLeftJewel",abilityFrame,0,(abilityFrame.sizeDelta.y-16)/2);
        Native("AbilityRightJewel",abilityFrame,abilityFrame.sizeDelta.x-14,(abilityFrame.sizeDelta.y-16)/2);
        Native("AbilityLightningLeft",ability,0,(ability.sizeDelta.y-44)/2);
        Native("AbilityLightningRight",ability,ability.sizeDelta.x-44,(ability.sizeDelta.y-44)/2);
        Native("AbilityEmblem",ability,(ability.sizeDelta.x-44)/2,(ability.sizeDelta.y-48)/2);
        var potion=Frame("Supply",bottom,SourceRect(Geometry.Bottom,67,32,70,63,135),20,10,10,9,16,"SilverInset");
        Native("PotionIcon",potion,(potion.sizeDelta.x-33)/2,(potion.sizeDelta.y-39)/2);
        var bomb=Frame("Supply",bottom,SourceRect(Geometry.Bottom,333,32,70,63,135),20,10,10,9,16,"SilverInset");
        Native("BombIcon",bomb,(bomb.sizeDelta.x-39)/2,(bomb.sizeDelta.y-43)/2);
        var resonance=SourceRect(Geometry.Bottom,121,95,295,22,135);
        Native("ResonanceDiamond",bottom,resonance.x,resonance.y);
        var caption=RectNode("ResonanceCaption",bottom,new Rect(resonance.x+25,resonance.y,resonance.width-25,22));
        Label("PHASE RESONANCE: +15% GEM DAMAGE",caption,12);
        Canvas.ForceUpdateCanvases();
    }
    static int PropDepth(string name)
    {
        if(name.Contains("Pillar")||name.Contains("Buttress"))return 0;
        if(name.Contains("Chain")||name.Contains("Banner"))return 1;
        if(name=="Gateway")return 2;
        if(name.Contains("RearBarrel"))return 3;
        if(name.Contains("Torch"))return 4;
        return 5;
    }
    void PortraitCollars(RectTransform frame)
    {
        Native("PortraitCollarTopA",frame,frame.sizeDelta.x*41/129-6,frame.sizeDelta.y-13);
        Native("PortraitCollarTopB",frame,frame.sizeDelta.x*90/129-6,frame.sizeDelta.y-13);
        Native("PortraitCollarBottomA",frame,frame.sizeDelta.x*45/129-7,0);
        Native("PortraitCollarBottomB",frame,frame.sizeDelta.x*91/129-7,0);
        Native("PortraitCollarLeftA",frame,0,frame.sizeDelta.y-frame.sizeDelta.y*49/252-7);
        Native("PortraitCollarLeftB",frame,0,frame.sizeDelta.y-frame.sizeDelta.y*123/252-7);
        Native("PortraitCollarRightA",frame,frame.sizeDelta.x-13,frame.sizeDelta.y-frame.sizeDelta.y*49/252-7);
        Native("PortraitCollarRightB",frame,frame.sizeDelta.x-13,frame.sizeDelta.y-frame.sizeDelta.y*123/252-7);
    }
}
