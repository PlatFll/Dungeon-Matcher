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
        // The connected top surface replaces the old shallow strip. Its rear
        // edge is behind the shared actor soles; the front meets the fascia.
        // Original art remains the fallback if the optional surface is absent.
        var standing = art.Sprite("Floor_Standing");
        int rear = floor - (standing ? 20 : 0);
        int fascia = floor + 12;
        var structure=DungeonPixelLabBattleArt.Rect("Structure",pixelLabContent,0,0,width,height);
        foreach(int x in new[]{0,96,width-36})
        {
            // Continue the native shaft beyond the physical display, including
            // camera cutouts. The upper termination belongs above the screen.
            int shaftTop = 16 + 28 * Mathf.FloorToInt((top - 16) / 28f);
            art.Image("Support_Capital",structure,x,shaftTop-16);
            // Every column lands at the same wall/floor junction, including
            // the posts behind the portrait. Trim only the final shaft repeat.
            int footTop = rear - (int)art.Sprite("Support_Foot").rect.height;
            for(int y=shaftTop;y<footTop;y+=28)
            {
                var shaft=art.Image("Support_Core",structure,x,y);
                if(y+28>footTop)
                {
                    shaft.type=Image.Type.Filled;shaft.fillMethod=Image.FillMethod.Vertical;
                    shaft.fillOrigin=(int)Image.OriginVertical.Top;shaft.fillAmount=(footTop-y)/28f;
                }
            }
            art.Image("Support_Foot",structure,x,footTop);
        }
        if (!art.UsesSideArchitecture)
        {
            // One bottom-anchored course grid joins the side stones to the corner
            // additions. The physical screen edges are independent of safe-area UI.
            float screenLeft=-v.xMin/scale;
            float screenWidth=UnityEngine.Device.Screen.width/scale;
            float screenBottom=v.yMax/scale, edgeTop=floor+34;
            var edges=DungeonPixelLabBattleArt.Rect("Edge masonry",structure,screenLeft,edgeTop,screenWidth,screenBottom-edgeTop);
            edges.gameObject.AddComponent<RectMask2D>();
            float first=screenBottom-Mathf.Ceil((screenBottom-edgeTop)/28)*28;
            bool corners=art.Sprite("Masonry_Corner_Left")&&art.Sprite("Masonry_Corner_Right");
            float sideBottom=screenBottom-(corners?84:0);
            for(float y=first;y<sideBottom-.01f;y+=28)
            {
                art.Image("Support_Corner_Left",edges,0,y-edgeTop);
                art.Image("Support_Corner_Right",edges,screenWidth-28,y-edgeTop);
            }
            if(corners)
            {
                // The approved corner replaces the last three side repeats. Its
                // 2px transparent padding lies outside the physical display.
                var cornerRoot=DungeonPixelLabBattleArt.Rect("Approved corner masonry",pixelLabContent,0,0,screenLeft+screenWidth,screenBottom);
                art.Image("Masonry_Corner_Left",cornerRoot,screenLeft,screenBottom-86);
                art.Image("Masonry_Corner_Right",cornerRoot,screenLeft+screenWidth-58,screenBottom-86);
            }
        }
        art.Image("Support_Corbel",structure,96,fascia+20);
        art.Image("Support_Corbel",structure,210+Mathf.Round((width-360)/2f),fascia+20);
        var gate=DungeonPixelLabBattleArt.Rect("Approved hybrid gate",pixelLabContent,0,0,width,height);
        foreach(var p in art.Manifest.placements.Where(p=>p.layer==20))
            art.Image(p.name,gate,p.x+Mathf.Round((width-360)/2f),p.y+rear-178);
        var platform=DungeonPixelLabBattleArt.Rect("Standing platform",pixelLabContent,0,0,width,height);
        if (standing)
        {
            // One opaque, full-depth surface continues behind the masked portrait
            // and across physical screen margins. Native repeats overhang only the
            // display edges; no RectMask2D fringe can expose the wall at a join.
            float screenLeft = -v.xMin/scale;
            float screenWidth = UnityEngine.Device.Screen.width/scale;
            var surface = DungeonPixelLabBattleArt.Rect("Connected standing floor", platform,
                screenLeft, rear, screenWidth, standing.rect.height);
            for (int x = Mathf.FloorToInt(screenLeft/standing.rect.width)*(int)standing.rect.width;
                 x < screenLeft+screenWidth; x += (int)standing.rect.width)
                art.Image("Floor_Standing", surface, x-screenLeft, 0);
        }
        else
        {
            // Continue the existing tile phase beneath the portrait to the viewport edge.
            // Keep the enemy-side floor, masked recess and actor baseline unchanged.
            for(int x=74,n=0;x>-32;x-=32,n++)
                art.Image("Floor_Mid_"+(n%3==0?"A":n%3==1?"C":"B"),platform,x,floor);
            for(int x=106,n=0;x<width;x+=32,n++)
                art.Image(n==0?"Floor_Left":"Floor_Mid_"+(n%3==0?"A":n%3==1?"B":"C"),platform,x,floor);
        }
        for(int x=0,n=0;x<(UnityEngine.Device.Screen.width-v.xMin)/scale;n++)
        {
            string name=n==0?"Fascia_Left":"Fascia_Mid_"+(n%2==0?"A":"B");
            art.Image(name,platform,x,fascia);x+=(int)art.Sprite(name).rect.width;
        }
        // Continue the existing separator phase through the left safe-area
        // margin as well. Full opaque native modules meet without masks/scaling.
        for(int x=-64,n=0;x+64>-v.xMin/scale;x-=64,n++)
            art.Image("Fascia_Mid_"+(n%2==0?"A":"B"),platform,x,fascia);
        var props=DungeonPixelLabBattleArt.Rect("Approved hybrid props",pixelLabContent,0,0,width,height);
        // Tiny, native contact clusters use the existing floor shadow colors.
        // They sit beneath the complete prop silhouettes, never recolor the props.
        if(standing)foreach(var p in art.Manifest.placements.Where(p=>p.layer==25))
        {
            float dx=p.x>=280?width-360:p.x>=200?Mathf.Round((width-360)/2f):0;
            string contact=p.name=="Barrel"?"Floor_ContactLarge":p.name=="Skull"?"Floor_ContactSkull":"Floor_ContactSmall";
            if(art.Sprite(contact))
                art.Image(contact,props,p.x+dx,p.y+art.Sprite(p.name).rect.height-6+rear-178);
        }
        foreach(var p in art.Manifest.placements.Where(p=>p.layer==24||p.layer==25))
        {
            // Left/right wall dressing stays attached to its column; the second
            // complete chain and center bones follow the centered gate.
            float dx=p.x>=280?width-360:p.x>=200?Mathf.Round((width-360)/2f):0;
            art.Image(p.name,props,p.x+dx,p.y+(p.layer==25?rear-178:0));
        }
    }
}
