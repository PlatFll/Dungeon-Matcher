using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

public sealed partial class DungeonPresentationArt
{
    Canvas pixelLabWorld;
    RectTransform pixelLabContent, pixelLabBoardFrame, pixelLabPortraitBanner;
    string pixelLabGeometry;
    bool pixelLabCellsReady;

    void UpdatePixelLabDungeon()
    {
        var layout = GetComponent<GameplayPixelLayoutController>();
        var art = DungeonPixelLabBattleArt.Current;
        if (!layout || !layout.Current.Fits || !art || !Camera.main) return;
        var player = GameObject.Find("PlayerSection");
        if (!player) return;
        var actor = layout.TopHud.GetComponentsInChildren<EnemySlotUI>()
            .FirstOrDefault(s => s.CurrentEnemy && s.EnemySpawnAnchor);
        if (!actor) return;
        var visual = actor.EnemySpawnAnchor.GetComponentsInChildren<Image>().FirstOrDefault(i => i.name == "VisualRoot" && i.sprite);
        if (!visual) return;
        if(!pixelLabCellsReady) { FindFirstObjectByType<BoardVisuals>().RefreshPresentationFrame(); pixelLabCellsReady=true; }
        int scale = DungeonPixelLabBattleArt.PhysicalPixelScale;
        var viewport = layout.Current.Viewport;
        // HP and feet move into the floor strip; retain the scenery's original rear edge.
        float floorScreen = GameplayPixelLayoutController.ScreenRect(actor.CombatBarRect).yMax +
            (2 + DungeonPixelLabBattleArt.EnemyFloorInset) * scale;
        int width = Mathf.CeilToInt(viewport.width / scale), height = Mathf.CeilToInt(viewport.height / scale);
        int floor = Mathf.RoundToInt((viewport.yMax - floorScreen) / scale);
        string key = viewport + ":" + width + ":" + height + ":" + floor + ":" + scale;
        if (pixelLabGeometry != key)
        {
            pixelLabGeometry = key;
            if (pixelLabWorld) { pixelLabWorld.gameObject.SetActive(false); Destroy(pixelLabWorld.gameObject); }
            var go = new GameObject("DungeonPixelLabEnvironment", typeof(RectTransform), typeof(Canvas));
            pixelLabWorld = go.GetComponent<Canvas>(); pixelLabWorld.renderMode = RenderMode.WorldSpace;
            pixelLabWorld.worldCamera = Camera.main; pixelLabWorld.sortingLayerName = "Default"; pixelLabWorld.sortingOrder = -190;
            pixelLabWorld.referencePixelsPerUnit = 64; pixelLabWorld.pixelPerfect = true;
            pixelLabContent = (RectTransform)go.transform; pixelLabContent.pivot = new Vector2(0,1);
            pixelLabContent.sizeDelta = new Vector2(width,height); go.AddComponent<RectMask2D>();
            BuildPixelLabEnvironment(art, width, height, floor);
            RefreshPixelLabPortrait(art, (RectTransform)player.transform);
        }
        PositionPixelLabWorld();
        if (!pixelLabBoardFrame)
        {
            pixelLabBoardFrame = DungeonPixelLabBattleArt.Rect("DungeonPixelLabBoardFrame",layout.BoardArea,0,0,0,0);
            art.Frame(pixelLabBoardFrame,"Board",80,DungeonPixelLabBattleArt.Unit(layout.GetComponentInParent<Canvas>()));
        }
        // Board sizing still belongs to BoardLayoutController. Only its rim changes.
        float unit = DungeonPixelLabBattleArt.Unit(layout.GetComponentInParent<Canvas>());
        float opening = RunSession.Current.Board.LocalBoardWidth * 64 * layout.Current.BoardTexelRatio / layout.Current.Scale;
        float outside = Mathf.Ceil(opening / unit) * unit + 26 * unit;
        DungeonPixelLabBattleArt.TopRect(pixelLabBoardFrame,(layout.BoardArea.rect.width-outside)/2,
            (layout.BoardArea.rect.height-outside)/2,outside,outside);
        GameplayPixelGrid.Snap(pixelLabBoardFrame);
        if (art.UsesHybrid) RefreshHybridBoardBacking(art,layout);
        if(pixelLabPortraitBanner)GameplayPixelGrid.Snap(pixelLabPortraitBanner);
        panelsReady = true;
    }

    void PositionPixelLabWorld()
    {
        if (!pixelLabContent || !Camera.main) return;
        var layout=GetComponent<GameplayPixelLayoutController>(); var v=layout.Current.Viewport;
        var camera=Camera.main;
        var a=camera.ScreenToWorldPoint(new Vector3(v.xMin,v.yMax,10));
        var b=camera.ScreenToWorldPoint(new Vector3(v.xMin+DungeonPixelLabBattleArt.PhysicalPixelScale,v.yMax,10));
        pixelLabContent.position=a;pixelLabContent.localScale=Vector3.one*Mathf.Abs(b.x-a.x);
    }
    void PixelLabBeforeCamera(ScriptableRenderContext context, Camera camera)
    { if(camera==Camera.main) PositionPixelLabWorld(); }

    void BuildPixelLabEnvironment(DungeonPixelLabBattleArt art,int width,int height,int floor)
    {
        if (art.UsesHybrid) { BuildHybridEnvironment(art,width,height,floor); return; }
        var wall = DungeonPixelLabBattleArt.Rect("Continuous wall",pixelLabContent,0,0,width,height);
        string[] walls={"Wall_A_Quiet","Wall_B_VioletStone","Wall_A_Quiet","Wall_C_SoftWear","Wall_A_Quiet","Wall_D_DeepStone"};
        for(int y=0;y<height;y+=48)for(int x=0;x<width;x+=48)
            art.Image(walls[(x/48+y/48*3)%walls.Length],wall,x,y);
        var structures=DungeonPixelLabBattleArt.Rect("Structure",pixelLabContent,0,0,width,height);
        foreach(var p in art.Manifest.placements.Where(p=>p.layer==12||p.layer==13||p.layer==14||p.name.StartsWith("Gate_")))
        {
            // Side columns extend through the entire viewport. The removed far-right corbel stays absent.
            if(p.name.StartsWith("Support_Corner_")||p.name=="Support_Edge_Foot")continue;
            float x=p.x>=300?p.x+width-360:p.x>=170?p.x+(width-360)/2:p.x;
            float y=p.y;
            if(p.name.StartsWith("Gate_")||p.name=="Support_Foot"||p.name=="Support_Corbel")y+=floor-178;
            if(p.name.StartsWith("Torch_Light"))y+=floor-178;
            art.Image(p.name,structures,Mathf.Round(x),Mathf.Round(y));
        }
        // Repeat only the column shaft, never stretch the capital/foot.
        for(int y=156;y<floor-22;y+=28)foreach(int x in new[]{0,96,width-36})art.Image("Support_Core",structures,x,y);
        for(int y=floor+34;y<height;y+=28)
        {
            art.Image("Support_Corner_Left",structures,0,y);
            art.Image("Support_Corner_Right",structures,width-28,y);
        }
        art.Image("Support_Edge_Foot",structures,0,height-8);
        art.Image("Support_Edge_Foot",structures,width-28,height-8);
        var platform=DungeonPixelLabBattleArt.Rect("Standing platform",pixelLabContent,0,0,width,height);
        for(int x=106,n=0;x<width;x+=32,n++)art.Image(n==0?"Floor_Left":"Floor_Mid_"+(n%3==0?"A":n%3==1?"B":"C"),platform,x,floor);
        for(int x=0,n=0;x<width;n++) {string name=n==0?"Fascia_Left":"Fascia_Mid_"+(n%2==0?"A":"B");art.Image(name,platform,x,floor+12);x+=(int)art.Sprite(name).rect.width;}
        var props=DungeonPixelLabBattleArt.Rect("Props",pixelLabContent,0,0,width,height);
        foreach(var p in art.Manifest.placements.Where(p=>p.layer>=24&&p.layer<=30))
        {
            float x=p.x>=240?p.x+width-360:p.x;
            float y=p.y+(p.name.StartsWith("Hanging_")?0:floor-178);
            art.Image(p.name,props,Mathf.Round(x),Mathf.Round(y));
        }
    }

    void RefreshPixelLabPortrait(DungeonPixelLabBattleArt art,RectTransform player)
    {
        var prior=player.Find("PixelLabPortraitBackdrop");
        if(prior) {prior.gameObject.SetActive(false);Destroy(prior.gameObject);}
        float u=DungeonPixelLabBattleArt.Unit(player.GetComponentInParent<Canvas>());
        var root=DungeonPixelLabBattleArt.Rect("PixelLabPortraitBackdrop",player,0,0,0,0);
        root.anchorMin=Vector2.zero;root.anchorMax=Vector2.one;root.offsetMin=root.offsetMax=Vector2.zero;root.SetAsFirstSibling();
        var mask=DungeonPixelLabBattleArt.Rect("PortraitMask",root,0,0,0,0);
        mask.anchorMin=Vector2.zero;mask.anchorMax=Vector2.one;mask.offsetMin=Vector2.one*9*u;mask.offsetMax=-mask.offsetMin;
        mask.gameObject.AddComponent<RectMask2D>();
        var backing=art.Image("Recess_Center",mask,0,0,u);backing.type=Image.Type.Tiled;backing.pixelsPerUnitMultiplier=1/u;
        backing.rectTransform.anchorMin=Vector2.zero;backing.rectTransform.anchorMax=Vector2.one;
        backing.rectTransform.offsetMin=-backing.sprite.rect.size*u;backing.rectTransform.offsetMax=-backing.rectTransform.offsetMin;
        var banner=art.Image(art.UsesHybrid?"Banner":"Hanging_Banner",mask,0,3*u,u);
        banner.rectTransform.anchorMin=banner.rectTransform.anchorMax=new Vector2(.5f,1);
        banner.rectTransform.anchoredPosition=new Vector2(-Mathf.Floor(banner.sprite.rect.width/2)*u,-3*u);
        pixelLabPortraitBanner=banner.rectTransform;
        GameplayPixelGrid.Snap(pixelLabPortraitBanner);
    }
}
