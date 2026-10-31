using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public sealed partial class DungeonPresentationArt
{
    GameObject snowWorld;
    RectTransform snowBoard,snowPortraitDressing;
    readonly List<Sprite> snowGenerated=new();
    string snowGeometry;

    void ReleaseSnowWorld()
    {
        if(snowWorld){snowWorld.SetActive(false);Destroy(snowWorld);}
        foreach(var s in snowGenerated){if(!s)continue;Destroy(s.texture);Destroy(s);}snowGenerated.Clear();
    }
    void PositionSnowWorld()
    {
        if(!snowWorld||!Camera.main)return;
        var c=Camera.main;float p=BattleFloorAnchor.PhysicalPixelScale;
        var a=c.ScreenToWorldPoint(new Vector3(0,Screen.height,10));
        var b=c.ScreenToWorldPoint(new Vector3(p,Screen.height,10));
        snowWorld.transform.position=a;snowWorld.transform.localScale=Vector3.one*Mathf.Abs(b.x-a.x);
    }
    SpriteRenderer SnowPiece(Sprite sprite,float x,float top,int order,string name=null)
    {
        var g=new GameObject(name??sprite.name);g.transform.SetParent(snowWorld.transform,false);
        var sr=g.AddComponent<SpriteRenderer>();sr.sprite=sprite;sr.sortingLayerName="Default";sr.sortingOrder=order;
        g.transform.localPosition=new Vector3(x+sprite.rect.width/2,-top-sprite.rect.height/2,0);
        g.transform.localScale=Vector3.one*sprite.pixelsPerUnit;return sr;
    }
    void UpdateSnow()
    {
        var a=SnowyMountainBattleAssembly.Current;var layout=GetComponent<GameplayPixelLayoutController>();
        if(!a||!layout||!layout.Current.Fits||!Camera.main)return;
        var player=GameObject.Find("PlayerSection")?.transform as RectTransform;
        var slot=layout.TopHud.GetComponentsInChildren<EnemySlotUI>().FirstOrDefault(s=>s.CurrentEnemy&&s.EnemySpawnAnchor);
        var mask=FindFirstObjectByType<BattleBackgroundTilemapController>()?.GetComponentInChildren<SpriteMask>();
        if(!player||!slot||!mask)return;
        float p=BattleFloorAnchor.PhysicalPixelScale,u=BattleFloorAnchor.Unit(layout.GetComponentInParent<Canvas>());
        if(!snowBoard){snowBoard=DungeonPixelLabBattleArt.Rect("SnowyMountainBoardFrame",layout.BoardArea,0,0,0,0);a.frames.Frame(snowBoard,"Board",100,u);}
        float opening=RunSession.Current.Board.LocalBoardWidth*64*layout.Current.BoardTexelRatio/layout.Current.Scale;
        float outside=Mathf.Ceil(opening/u)*u+26*u;
        DungeonPixelLabBattleArt.TopRect(snowBoard,(layout.BoardArea.rect.width-outside)/2,(layout.BoardArea.rect.height-outside)/2,outside,outside);
        GameplayPixelGrid.Snap(snowBoard);snowBoard.GetComponentInChildren<ResponsiveModularFrameFitter>().RefreshFrame();
        a.frames.Panel(player,"Portrait","Backing",9,50);
        player.Find("PixelLabPortraitFrame").GetComponent<ResponsiveModularFrameFitter>().RefreshFrame();
        var fr=GameplayPixelLayoutController.ScreenRect(snowBoard);var mb=mask.bounds;
        float feet=BattleFloorAnchor.EnemyFeetScreenY(slot);
        var cam=Camera.main;var maskTop=cam.WorldToScreenPoint(mb.max);var maskBottom=cam.WorldToScreenPoint(mb.min);
        string key=Screen.width+"x"+Screen.height+":"+layout.Current.Viewport+":"+fr+":"+feet+":"+maskTop+":"+player.rect;
        if(snowGeometry!=key)
        {
            snowGeometry=key;ReleaseSnowWorld();snowWorld=new GameObject("ApprovedSnowyMountainEnvironment");
            if(snowPortraitDressing)Destroy(snowPortraitDressing.gameObject);
            snowPortraitDressing=SnowPortraitDecor(a.frames,player,u);
            foreach(var im in snowBoard.GetComponentsInChildren<Image>().Where(i=>i.name.StartsWith("Board_Snow")||i.name=="Board_Icicles"))Destroy(im.gameObject);
            SnowBoardDecor(a.frames,snowBoard,u);
            float width=Screen.width/p,height=Screen.height/p;
            // Keep the exact approved masked-vista registration, but expose its native overscan.
            var vista=a.Get("AlpinePass");float h=vista.rect.height;
            float top=maskTop.y+Mathf.Floor(Mathf.Max(0,h-(maskTop.y-maskBottom.y)/p)*.35f)*p;
            var center=new Vector2(Mathf.Round(cam.WorldToScreenPoint(mb.center).x),Mathf.Round(top-h*.5f*p));
            float vx=center.x/p-vista.rect.width/2,vy=height-center.y/p-h/2;
            SnowPiece(vista,vx,vy,-95);
            var sky=a.Get("SkyExtension");SnowPiece(sky,vx,vy-sky.rect.height,-95);
            // Cover cutout gutters using the existing muted wall; no empty camera borders.
            var wall=a.Get("CobbleWall_Muted");
            for(float y=-wall.rect.height;y<height;y+=wall.rect.height)for(float x=-wall.rect.width;x<width;x+=wall.rect.width)SnowPiece(wall,x,y,-200);
            float sourceTop=height-feet/p-34;
            for(float y=sourceTop+84;y<height;y+=wall.rect.height)for(float x=0;x<width;x+=wall.rect.width)SnowPiece(wall,x,y,-93);
            for(int i=0;i<4;i++)SnowPiece(a.Get("Ledge_"+i),i*64,sourceTop,-92);
            for(float x=256;x<width-64;x+=64)SnowPiece(a.Get("Ledge_3"),x,sourceTop,-92);
            SnowPiece(a.Get("Ledge_4"),width-64,sourceTop,-91);
            float nativeTop=Mathf.Floor(fr.yMax/p),sideTop=height-nativeTop;
            for(int side=0;side<2;side++)
            {
                bool left=side==0;string n=left?"Left":"Right";
                int shaft=Mathf.CeilToInt((left?fr.xMin:Screen.width-fr.xMax)/p)+8;
                var cliff=SnowyMountainCliff.Build(a.Get(n+"_Complete"),a.Get("Corner_"+n),left,Mathf.CeilToInt(nativeTop),shaft);snowGenerated.Add(cliff);
                SnowPiece(cliff,left?-4:width-cliff.rect.width+4,sideTop,-80);
                float oldX=left?-5:width-39;
                SnowPiece(a.Get("Lantern"),oldX+(left?2:19),sideTop+43,-76);
                float anchor=left?shaft-11:width+13-shaft;
                SnowPiece(a.Get("Banner_"+n+"_Mounted"),anchor+(left?-27:-15)-4,sideTop+87,-75);
            }
            AddSnowAtmosphere(a,fr,p,feet,GameplayPixelLayoutController.ScreenRect(player));
        }
        PositionSnowWorld();panelsReady=true;
    }
    static RectTransform SnowPortraitDecor(DungeonPixelLabBattleArt a,RectTransform player,float u)
    {
        float w=Mathf.Round(player.rect.width/u),h=Mathf.Round(player.rect.height/u);
        var r=DungeonPixelLabBattleArt.Rect("SnowPortraitDressing",player,0,0,player.rect.width,player.rect.height);
        a.Image("SnowCap_Left",r,0,-4*u,u);a.Image("SnowCap_Right",r,(w-32)*u,-4*u,u);
        var mid=a.Image("SnowCap_Middle",r,32*u,-4*u,u);mid.type=Image.Type.Tiled;mid.pixelsPerUnitMultiplier=1/u;mid.rectTransform.sizeDelta=new Vector2(Mathf.Max(0,w-64),16)*u;
        a.Image("SnowFoot_Left",r,9*u,(h-18)*u,u);a.Image("SnowFoot_Right",r,(w-33)*u,(h-18)*u,u);r.SetAsLastSibling();return r;
    }
    static void SnowBoardDecor(DungeonPixelLabBattleArt a,RectTransform target,float u)
    {
        float w=Mathf.Round(target.rect.width/u),h=Mathf.Round(target.rect.height/u);
        var s=a.Image("Board_SnowRail",target,18*u,-2*u,u);s.type=Image.Type.Tiled;s.pixelsPerUnitMultiplier=1/u;s.rectTransform.sizeDelta=new Vector2(w-36,8)*u;
        a.Image("Board_SnowCorner",target,0,-4*u,u);a.Image("Board_SnowCorner",target,(w-18)*u,-4*u,u);
        a.Image("Board_Icicles",target,u,(h-1)*u,u);a.Image("Board_Icicles",target,(w-17)*u,(h-1)*u,u);
    }
    void AddSnowAtmosphere(SnowyMountainBattleAssembly a,Rect frame,float p,float feet,Rect portrait)
    {
        var motion=snowWorld.AddComponent<SnowyMountainAmbientMotion>();float height=Screen.height/p;
        var names=new[]{"MoteSmall","MoteWide","Snowflake"};
        void Add(float x,float low,float high,int id,float opacity)
        {
            if(high-low<12*p)return;
            x=Mathf.Round(x/p);low=Mathf.Round(low/p);high=Mathf.Round(high/p);var s=a.Get(names[id%3]);
            var sr=SnowPiece(s,x-s.rect.width/2,height-high-s.rect.height/2,-64);
            motion.tracks.Add(new SnowyMountainAmbientMotion.Track{renderer=sr,low=new Vector2(x,low-height),high=new Vector2(x,high-height),phase=id*2.371f%12,opacity=opacity});
        }
        float x0=portrait.xMax+14*p,x1=Screen.width-14*p;
        for(int i=0;i<12;i++){float upper=Screen.height-(55+i%3*20)*p;Add(Mathf.Lerp(x0,x1,(i+.5f)/12),Mathf.Max(feet+66*p,upper-(48+i%4*9)*p),upper,i,.35f+i%3*.09f);}
        for(int side=0;side<2;side++)for(int i=0;i<4;i++){float x=side==0?Mathf.Max(9*p,frame.xMin*.4f):Screen.width-Mathf.Max(9*p,(Screen.width-frame.xMax)*.4f);float top=Mathf.Lerp(frame.yMax-50*p,frame.yMin+95*p,i/4f);Add(x,top-36*p,top,12+side*4+i,.26f);}
        var glints=Enumerable.Range(0,6).Select(i=>a.Get("Glint_"+i.ToString("D2"))).ToArray();
        // The connected sides retain the three approved ice-tip accents near the lower formations.
        var positions=new[]{new Vector2(13,height-Mathf.Round(frame.yMin/p)-22),new Vector2(Screen.width/p-21,height-Mathf.Round(frame.yMin/p)-22),new Vector2(42,height-14)};
        for(int i=0;i<positions.Length;i++){var pos=positions[i];var sr=SnowPiece(glints[0],pos.x-4,pos.y-4,-63);motion.tracks.Add(new SnowyMountainAmbientMotion.Track{renderer=sr,frames=glints,phase=i*4+1.25f});}
    }
}
