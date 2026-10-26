using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public sealed partial class DungeonPresentationArt
{
    void UpdateForest()
    {
        var layout=GetComponent<GameplayPixelLayoutController>();
        var art=DungeonPixelLabBattleArt.Current;
        if(!layout || !layout.Current.Fits || !art || !Camera.main)return;
        var player=GameObject.Find("PlayerSection");
        var actor=layout.TopHud.GetComponentsInChildren<EnemySlotUI>().FirstOrDefault(s=>s.CurrentEnemy&&s.EnemySpawnAnchor);
        if(!player||!actor)return;
        if(!pixelLabCellsReady){FindFirstObjectByType<BoardVisuals>().RefreshPresentationFrame();pixelLabCellsReady=true;}
        int scale=DungeonPixelLabBattleArt.PhysicalPixelScale;
        var viewport=layout.Current.Viewport;
        var board=RunSession.Current.Board;
        var camera=Camera.main;
        Vector3 lo=camera.WorldToScreenPoint(board.transform.TransformPoint(new Vector3(-board.LocalBoardWidth/2,-board.LocalBoardHeight/2)));
        Vector3 hi=camera.WorldToScreenPoint(board.transform.TransformPoint(new Vector3(board.LocalBoardWidth/2,board.LocalBoardHeight/2)));
        var opening=new Rect((lo.x-viewport.xMin)/scale,(viewport.yMax-hi.y)/scale,(hi.x-lo.x)/scale,(hi.y-lo.y)/scale);
        // The approved ground's standing line is 213; HP remains below the actor's feet.
        float feet=BattleFloorAnchor.EnemyFeetScreenY(actor);
        float groundOffset=Mathf.Round((viewport.yMax-feet)/scale)-213;
        int width=Mathf.CeilToInt(viewport.width/scale),height=Mathf.CeilToInt(viewport.height/scale);
        string key=UnityEngine.Device.Screen.width+"x"+UnityEngine.Device.Screen.height+":"+viewport+":"+opening+":"+groundOffset+":"+scale;
        if(pixelLabGeometry!=key)
        {
            pixelLabGeometry=key;
            if(pixelLabWorld){pixelLabWorld.gameObject.SetActive(false);Destroy(pixelLabWorld.gameObject);}
            var go=new GameObject("ApprovedForestEnvironment",typeof(RectTransform),typeof(Canvas));
            pixelLabWorld=go.GetComponent<Canvas>();pixelLabWorld.renderMode=RenderMode.WorldSpace;
            pixelLabWorld.worldCamera=camera;pixelLabWorld.sortingLayerName="Default";pixelLabWorld.sortingOrder=-190;
            pixelLabWorld.referencePixelsPerUnit=64;pixelLabWorld.pixelPerfect=true;
            pixelLabContent=(RectTransform)go.transform;pixelLabContent.pivot=new Vector2(0,1);pixelLabContent.sizeDelta=new Vector2(width,height);
            BuildForestEnvironment(art,width,height,opening,groundOffset);
            RefreshPixelLabPortrait(art,(RectTransform)player.transform);
            RefreshForestPortrait(art,(RectTransform)player.transform);
        }
        PositionPixelLabWorld();
        float unit=DungeonPixelLabBattleArt.Unit(layout.GetComponentInParent<Canvas>());
        if(!pixelLabBoardFrame)
        {
            pixelLabBoardFrame=DungeonPixelLabBattleArt.Rect("ForestBoardFrame",layout.BoardArea,0,0,0,0);
            art.Frame(pixelLabBoardFrame,"Board",150,unit);
        }
        float inset=(art.Sprite("Board_Corner_TL").rect.width+art.Sprite("Board_Rail_Top").rect.height)*.5f;
        float outside=Mathf.Ceil(opening.width)*unit+2*inset*unit;
        DungeonPixelLabBattleArt.TopRect(pixelLabBoardFrame,(layout.BoardArea.rect.width-outside)/2,(layout.BoardArea.rect.height-outside)/2,outside,outside);
        GameplayPixelGrid.Snap(pixelLabBoardFrame);
        panelsReady=true;
    }

    void BuildForestEnvironment(DungeonPixelLabBattleArt art,int width,int height,Rect opening,float groundOffset)
    {
        var assembly=art.forestAssembly;
        var tracks=new List<ForestAmbientMotion.Track>();
        // Native repeats extend past the safe area to the physical display edges.
        var backing=assembly.pieces.First(p=>p.region=="Backdrop");
        var fill=ForestImage(backing,pixelLabContent,new Rect(-width,-height,width*3,height*3));
        fill.type=Image.Type.Tiled;fill.pixelsPerUnitMultiplier=1;
        foreach(var p in assembly.pieces)
        {
            if(p.region=="Backdrop"||p.region.StartsWith("Portrait"))continue;
            var r=p.rect;
            if(p.region=="Top"||p.region=="Grove"||p.region=="Ground")
            {
                r.x+=(width-360)/2f;r.y+=groundOffset;
                if(p.region=="Grove")
                {
                    // Mirror the quiet canopy at its top boundary, not the whole scene's
                    // lower river/floor. The original vista remains upright and unchanged.
                    var canopy=ForestImage(p,pixelLabContent,new Rect(r.x,r.y,r.width,r.height));
                    canopy.name="Canopy display extension";
                    canopy.rectTransform.localScale=new Vector3(1,-1,1);
                    for(int side=-1;side<=1;side+=2)
                        ForestImage(p,pixelLabContent,new Rect(r.x+side*r.width,r.y,r.width,r.height));
                }
                if(p.name=="Combat_Ground")
                {
                    for(int side=-1;side<=1;side+=2)
                        ForestImage(p,pixelLabContent,new Rect(r.x+side*r.width,r.y,r.width,r.height));
                }
            }
            else
            {
                // Move complete sprites; never stretch the approved roots, rocks or leaves.
                if(p.region=="Lower")
                {
                    // Keep each lower corner formation intact and flush with the display.
                    r.x+=p.rect.center.x<180?0:width-360;
                    r.y+=height-680;
                }
                else
                {
                    r.x+=p.rect.center.x<180?opening.xMin-assembly.boardOpening.xMin:opening.xMax-assembly.boardOpening.xMax;
                    r.x=Mathf.Clamp(r.x,0,width-r.width);
                    float t=Mathf.InverseLerp(assembly.boardOpening.yMin,assembly.boardOpening.yMax,p.rect.center.y);
                    r.y+=Mathf.Lerp(opening.yMin-assembly.boardOpening.yMin,opening.yMax-assembly.boardOpening.yMax,t);
                }
            }
            r.x=Mathf.Round(r.x);r.y=Mathf.Round(r.y);
            var image=ForestImage(p,pixelLabContent,r);
            if(p.motion!=null&&p.motion.frames?.Length>0)tracks.Add(p.motion.Bind(image,image.rectTransform.anchoredPosition,1));
        }
        var motion=pixelLabContent.gameObject.AddComponent<ForestAmbientMotion>();motion.tracks=tracks.ToArray();
    }

    static Image ForestImage(ForestBattleAssembly.Piece p,Transform parent,Rect rect,float unit=1)
    {
        var r=DungeonPixelLabBattleArt.Rect(p.name,parent,rect.x*unit,rect.y*unit,rect.width*unit,rect.height*unit);
        var image=r.gameObject.AddComponent<Image>();image.sprite=p.sprite;image.color=p.color;
        image.type=p.type;image.pixelsPerUnitMultiplier=1/unit;image.raycastTarget=false;
        return image;
    }

    void RefreshForestPortrait(DungeonPixelLabBattleArt art,RectTransform player)
    {
        var previous=player.Find("ForestPortraitFoliage");if(previous){previous.gameObject.SetActive(false);Destroy(previous.gameObject);}
        var root=DungeonPixelLabBattleArt.Rect("ForestPortraitFoliage",player,0,0,player.rect.width,player.rect.height);
        root.SetAsLastSibling();
        float unit=DungeonPixelLabBattleArt.Unit(player.GetComponentInParent<Canvas>());
        var source=art.forestAssembly.portrait;
        var size=player.rect.size/unit;
        var tracks=new List<ForestAmbientMotion.Track>();
        foreach(var p in art.forestAssembly.pieces.Where(p=>p.region.StartsWith("Portrait")))
        {
            var r=p.rect;r.position-=source.position;
            if(p.region=="PortraitBanner")
            {
                r.x+=(size.x-source.width)/2;
                var mask=player.Find("PixelLabPortraitBackdrop/PortraitMask");
                ForestImage(p,mask,new Rect(Mathf.Round(r.x)-9,r.y-9,r.width,r.height),unit);
                continue;
            }
            r.x+=p.rect.center.x>source.center.x?size.x-source.width:0;
            float y=Mathf.InverseLerp(0,source.height,r.center.y);
            r.y+=y*(size.y-source.height);r.x=Mathf.Round(r.x);r.y=Mathf.Round(r.y);
            var image=ForestImage(p,root,r,unit);GameplayPixelGrid.Snap(image.rectTransform);
            if(p.motion!=null&&p.motion.frames?.Length>0)tracks.Add(p.motion.Bind(image,image.rectTransform.anchoredPosition,unit));
        }
        if(tracks.Count>0)root.gameObject.AddComponent<ForestAmbientMotion>().tracks=tracks.ToArray();
    }
}
