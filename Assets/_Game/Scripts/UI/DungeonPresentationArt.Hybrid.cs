using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public sealed partial class DungeonPresentationArt
{
    RectTransform hybridBoardBacking;
    // The live gems are SpriteRenderers. Keep the inset on the existing world
    // background canvas, never in the overlay UI canvas above those gems.
    void RefreshHybridBoardBacking(DungeonPixelLabBattleArt art,GameplayPixelLayoutController layout)
    {
        if (!hybridBoardBacking)
        {
            hybridBoardBacking=DungeonPixelLabBattleArt.Rect("Hybrid board backing",pixelLabContent,0,0,0,0);
            hybridBoardBacking.gameObject.AddComponent<RectMask2D>();
            var fill=art.Image("Control_Inset",hybridBoardBacking,-32,-32);
            fill.type=Image.Type.Tiled;fill.pixelsPerUnitMultiplier=1;
            fill.rectTransform.anchorMin=Vector2.zero;fill.rectTransform.anchorMax=Vector2.one;
            fill.rectTransform.offsetMin=-Vector2.one*32;fill.rectTransform.offsetMax=Vector2.one*32;
        }
        var r=GameplayPixelLayoutController.ScreenRect(pixelLabBoardFrame);
        var v=layout.Current.Viewport;float scale=DungeonPixelLabBattleArt.PhysicalPixelScale;
        DungeonPixelLabBattleArt.TopRect(hybridBoardBacking,(r.xMin-v.xMin)/scale+13,
            (v.yMax-r.yMax)/scale+13,r.width/scale-26,r.height/scale-26);
    }

    // The same world canvas, camera binding, native pixel scale and actor floor
    // from the production PixelLab integration own this approved library too.
    void BuildHybridEnvironment(DungeonPixelLabBattleArt art,int width,int height,int floor)
    {
        var wall=DungeonPixelLabBattleArt.Rect("Continuous wall",pixelLabContent,0,0,width,height);
        var walls=art.Manifest.placements.Where(p=>p.layer==0).ToArray();
        // Keep the established tile phase and native pitch, but cover physical
        // screen margins/cutouts as well as the safe gameplay viewport.
        var v=GetComponent<GameplayPixelLayoutController>().Current.Viewport;
        float scale=DungeonPixelLabBattleArt.PhysicalPixelScale;
        int left=Mathf.FloorToInt(-v.xMin/scale/48)*48-48;
        int top=Mathf.FloorToInt((v.yMax-UnityEngine.Device.Screen.height)/scale/48)*48-48;
        int right=Mathf.CeilToInt((UnityEngine.Device.Screen.width-v.xMin)/scale/48)*48+48;
        int bottom=Mathf.CeilToInt(v.yMax/scale/48)*48+48;
        for(int y=top;y<bottom;y+=48)for(int x=left;x<right;x+=48)
        {
            var p=walls.First(p=>p.x==((x%384+384)%384)&&p.y==((y%240+240)%240));
            art.Image(p.name,wall,x,y);
            // The 360px reference ends with half a tile at x336. Complete its
            // matching right half when repeating across the full screen.
            if(p.name=="Wall_Edge_Left")art.Image("Wall_Edge_Right",wall,x+24,y);
        }
        var structure=DungeonPixelLabBattleArt.Rect("Structure",pixelLabContent,0,0,width,height);
        foreach(int x in new[]{0,96,width-36})
        {
            art.Image("Support_Capital",structure,x,0);
            for(int y=16;y<floor-22;y+=28)art.Image("Support_Core",structure,x,y);
            art.Image("Support_Foot",structure,x,floor-22);
        }
        // Continue the quiet edge masonry to the bottom, without decorative bases.
        for(int y=floor+34;y<Mathf.CeilToInt(v.yMax/scale);y+=28)
        {
            art.Image("Support_Corner_Left",structure,0,y);
            art.Image("Support_Corner_Right",structure,width-28,y);
        }
        art.Image("Support_Corbel",structure,96,floor+32);
        art.Image("Support_Corbel",structure,210+Mathf.Round((width-360)/2f),floor+32);
        var gate=DungeonPixelLabBattleArt.Rect("Approved hybrid gate",pixelLabContent,0,0,width,height);
        foreach(var p in art.Manifest.placements.Where(p=>p.layer==20))
            art.Image(p.name,gate,p.x+Mathf.Round((width-360)/2f),p.y+floor-178);
        var platform=DungeonPixelLabBattleArt.Rect("Standing platform",pixelLabContent,0,0,width,height);
        // Continue the existing tile phase beneath the portrait to the viewport edge.
        // Keep the enemy-side floor, masked recess and actor baseline unchanged.
        for(int x=74,n=0;x>-32;x-=32,n++)
            art.Image("Floor_Mid_"+(n%3==0?"A":n%3==1?"C":"B"),platform,x,floor);
        for(int x=106,n=0;x<width;x+=32,n++)
            art.Image(n==0?"Floor_Left":"Floor_Mid_"+(n%3==0?"A":n%3==1?"B":"C"),platform,x,floor);
        for(int x=0,n=0;x<width;n++)
        {
            string name=n==0?"Fascia_Left":"Fascia_Mid_"+(n%2==0?"A":"B");
            art.Image(name,platform,x,floor+12);x+=(int)art.Sprite(name).rect.width;
        }
        var props=DungeonPixelLabBattleArt.Rect("Approved hybrid props",pixelLabContent,0,0,width,height);
        foreach(var p in art.Manifest.placements.Where(p=>p.layer==24||p.layer==25))
        {
            // Left/right wall dressing stays attached to its column; the second
            // complete chain and center bones follow the centered gate.
            float dx=p.x>=280?width-360:p.x>=200?Mathf.Round((width-360)/2f):0;
            art.Image(p.name,props,p.x+dx,p.y+(p.layer==25?floor-178:0));
        }
    }
}
