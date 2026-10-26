using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public sealed partial class DungeonPresentationArt
{
    void UpdateCourt()
    {
        var layout=GetComponent<GameplayPixelLayoutController>();var art=DrownedCourtBattleAssembly.Current;
        if(!layout||!layout.Current.Fits||!art||!Camera.main)return;
        var player=GameObject.Find("PlayerSection");var slot=layout.TopHud.GetComponentsInChildren<EnemySlotUI>().FirstOrDefault(s=>s.CurrentEnemy&&s.EnemySpawnAnchor);
        if(!player||!slot)return;
        var v=layout.Current.Viewport;int scale=BattleFloorAnchor.PhysicalPixelScale;
        var board=RunSession.Current.Board;var camera=Camera.main;
        var lo=camera.WorldToScreenPoint(board.transform.TransformPoint(new Vector3(-board.LocalBoardWidth/2,-board.LocalBoardHeight/2)));
        var hi=camera.WorldToScreenPoint(board.transform.TransformPoint(new Vector3(board.LocalBoardWidth/2,board.LocalBoardHeight/2)));
        var opening=new Rect((lo.x-v.xMin)/scale,(v.yMax-hi.y)/scale,(hi.x-lo.x)/scale,(hi.y-lo.y)/scale);
        int width=Mathf.CeilToInt(v.width/scale),height=Mathf.CeilToInt(v.height/scale);
        // Revision04's soles stand midway across the front floor course (200..216).
        float offset=Mathf.Round((v.yMax-BattleFloorAnchor.EnemyFeetScreenY(slot))/scale)-208;
        string key=UnityEngine.Device.Screen.width+"x"+UnityEngine.Device.Screen.height+":"+v+":"+opening+":"+offset+":"+scale;
        if(pixelLabGeometry!=key)
        {
            pixelLabGeometry=key;
            if(pixelLabWorld){pixelLabWorld.gameObject.SetActive(false);Destroy(pixelLabWorld.gameObject);}
            var go=new GameObject("ApprovedDrownedCourtEnvironment",typeof(RectTransform),typeof(Canvas));
            pixelLabWorld=go.GetComponent<Canvas>();pixelLabWorld.renderMode=RenderMode.WorldSpace;pixelLabWorld.worldCamera=camera;
            pixelLabWorld.sortingLayerName="Default";pixelLabWorld.sortingOrder=-190;pixelLabWorld.referencePixelsPerUnit=64;pixelLabWorld.pixelPerfect=true;
            pixelLabContent=(RectTransform)go.transform;pixelLabContent.pivot=new Vector2(0,1);pixelLabContent.sizeDelta=new Vector2(width,height);
            BuildCourt(art,width,height,opening,offset);
            RefreshCourtPortrait(art,(RectTransform)player.transform);
        }
        PositionPixelLabWorld();
        float unit=BattleFloorAnchor.Unit(layout.GetComponentInParent<Canvas>());
        if(!pixelLabBoardFrame)
        {
            pixelLabBoardFrame=DungeonPixelLabBattleArt.Rect("CourtBoardFrame",layout.BoardArea,0,0,0,0);
            art.frames.Frame(pixelLabBoardFrame,"Board",120,unit);
        }
        float inset=(art.frames.Sprite("Board_Corner_TL").rect.width+art.frames.Sprite("Board_Rail_Top").rect.height)*.5f;
        float outside=Mathf.Ceil(opening.width)*unit+2*inset*unit;
        DungeonPixelLabBattleArt.TopRect(pixelLabBoardFrame,(layout.BoardArea.rect.width-outside)/2,(layout.BoardArea.rect.height-outside)/2,outside,outside);
        GameplayPixelGrid.Snap(pixelLabBoardFrame);panelsReady=true;
    }

    void BuildCourt(DrownedCourtBattleAssembly art,int width,int height,Rect opening,float offset)
    {
        var motion=pixelLabContent.gameObject.AddComponent<DrownedCourtAmbientMotion>();
        // Cover the physical display beyond the safe-area layout without stretching any sprite.
        CourtTile("Continuous submerged masonry",art.masonry,pixelLabContent,new Rect(-width,-height,width*3,height*3));
        var water=art.pieces.First(p=>p.name=="DeepWater");
        CourtTile("Ocean display extent",water.sprite,pixelLabContent,new Rect(-width,-height,width*3,height+184+offset));
        foreach(var p in art.pieces)
        {
            if(p.region=="Portrait"||p.region=="Side")continue;
            var r=p.rect;
            if(p.region=="BackgroundTop"||p.region=="Top"||p.region=="Floor"||p.region=="TopAmbient")
            {
                r.x+=(width-360)*.5f;r.y+=offset;
                // Repeat only floor modules past the display edges. The approved center remains intact.
                if(p.region=="Floor"&&p.rect.x==16)
                {
                    for(float x=r.x-r.width;x>-width;x-=r.width)CourtImage(p,pixelLabContent,new Rect(x,r.y,r.width,r.height),motion);
                    for(float x=width-(360-p.rect.x)+r.width;x<width*2;x+=r.width)
                        if(x>=width-1)CourtImage(p,pixelLabContent,new Rect(x,r.y,r.width,r.height),motion);
                }
            }
            else if(p.region.StartsWith("Lower"))
            {
                r.x+=p.rect.center.x<180?0:width-360;r.y+=height-640;
            }
            else
            {
                r.x+=p.rect.center.x<180?0:width-360;
                r.y+=opening.yMin-art.boardOpening.yMin;
            }
            r.x=Mathf.Round(r.x);r.y=Mathf.Round(r.y);CourtImage(p,pixelLabContent,r,motion);
        }
        // Keep the two coral-bearing shafts intact. Adjust only quiet repeating courses when the board grows.
        float top=Mathf.Round(opening.yMin+(243-art.boardOpening.yMin));
        float bottom=Mathf.Round(opening.yMax+(551-art.boardOpening.yMax));
        foreach(bool right in new[]{false,true})
        {
            var side=art.pieces.Where(p=>p.region=="Side"&&(p.rect.x>180)==right).OrderBy(p=>p.rect.y).ToArray();
            float y=top;
            foreach(var p in side)
            {
                if(p.name.StartsWith("Base"))
                {
                    var filler=side.First(s=>s.name.StartsWith("Quiet"));float end=bottom-p.rect.height;
                    if(y<end)CourtTile("Side shaft extension",filler.sprite,pixelLabContent,new Rect(right?width-31:0,y,31,end-y));
                    y=Mathf.Max(y,end);
                }
                CourtImage(p,pixelLabContent,new Rect(right?width-31:0,y,p.rect.width,p.rect.height),motion);y+=p.rect.height;
            }
        }
        // A one-native-pixel overlap under the live grid closes fractional board raster edges.
        CourtTile("Board underlap top",art.innerShadow,pixelLabContent,new Rect(opening.x-1,opening.y-1,opening.width+2,2));
        CourtTile("Board underlap bottom",art.innerShadow,pixelLabContent,new Rect(opening.x-1,opening.yMax-1,opening.width+2,2));
        CourtTile("Board underlap left",art.innerShadow,pixelLabContent,new Rect(opening.x-1,opening.y,2,opening.height));
        CourtTile("Board underlap right",art.innerShadow,pixelLabContent,new Rect(opening.xMax-1,opening.y,2,opening.height));
        motion.Evaluate(0,PresentationPreferences.ReducedMotion);
    }
    static Image CourtImage(DrownedCourtBattleAssembly.Piece p,Transform parent,Rect r,DrownedCourtAmbientMotion motion,float unit=1)
    {
        var t=DungeonPixelLabBattleArt.Rect(p.name,parent,r.x*unit,r.y*unit,r.width*unit,r.height*unit);
        var im=t.gameObject.AddComponent<Image>();im.sprite=p.sprite;im.color=p.color;im.type=p.type;im.pixelsPerUnitMultiplier=1/unit;im.raycastTarget=false;
        if(p.poses?.Length>0)motion.tracks.Add(new DrownedCourtAmbientMotion.Track{image=im,origin=t.anchoredPosition,poses=p.poses});return im;
    }
    static Image CourtTile(string name,Sprite sprite,Transform parent,Rect rect,float unit=1)
    {
        var im=CourtImage(new DrownedCourtBattleAssembly.Piece{name=name,sprite=sprite,type=Image.Type.Tiled},parent,rect,null,unit);return im;
    }
    void RefreshCourtPortrait(DrownedCourtBattleAssembly art,RectTransform player)
    {
        var prior=player.Find("CourtPortraitBackdrop");if(prior){prior.gameObject.SetActive(false);Destroy(prior.gameObject);}
        float u=BattleFloorAnchor.Unit(player.GetComponentInParent<Canvas>());
        var root=DungeonPixelLabBattleArt.Rect("CourtPortraitBackdrop",player,0,0,0,0);root.anchorMin=Vector2.zero;root.anchorMax=Vector2.one;root.offsetMin=root.offsetMax=Vector2.zero;root.SetAsFirstSibling();
        var mask=DungeonPixelLabBattleArt.Rect("PortraitMask",root,0,0,0,0);mask.anchorMin=Vector2.zero;mask.anchorMax=Vector2.one;mask.offsetMin=Vector2.one*10*u;mask.offsetMax=-mask.offsetMin;mask.gameObject.AddComponent<RectMask2D>();
        var backing=CourtTile("Overfilled Court recess",art.recess,mask,new Rect(0,0,1,1),u);backing.rectTransform.anchorMin=Vector2.zero;backing.rectTransform.anchorMax=Vector2.one;backing.rectTransform.offsetMin=-art.recess.rect.size*u;backing.rectTransform.offsetMax=-backing.rectTransform.offsetMin;
        var priorDecor=player.Find("CourtPortraitAccents");if(priorDecor){priorDecor.gameObject.SetActive(false);Destroy(priorDecor.gameObject);}
        var decor=DungeonPixelLabBattleArt.Rect("CourtPortraitAccents",player,0,0,player.rect.width,player.rect.height);
        foreach(var p in art.pieces.Where(p=>p.region=="Portrait"))
        {
            var r=p.rect;r.position-=art.portrait.position;var size=player.rect.size/u;
            if(p.rect.center.x>art.portrait.center.x)r.x+=size.x-art.portrait.width;
            r.y+=Mathf.InverseLerp(0,art.portrait.height,r.center.y)*(size.y-art.portrait.height);
            r.x=Mathf.Round(r.x);r.y=Mathf.Round(r.y);var im=CourtImage(p,decor,r,null,u);GameplayPixelGrid.Snap(im.rectTransform);
        }
    }
}
