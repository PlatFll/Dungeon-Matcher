using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public sealed partial class DungeonPresentationArt
{
    void UpdateIronvein()
    {
        var layout=GetComponent<GameplayPixelLayoutController>();var art=IronveinBattleAssembly.Current;
        if(!layout||!layout.Current.Fits||!art||!Camera.main)return;
        var player=GameObject.Find("PlayerSection");var slot=layout.TopHud.GetComponentsInChildren<EnemySlotUI>().FirstOrDefault(s=>s.CurrentEnemy&&s.EnemySpawnAnchor);
        if(!player||!slot)return;
        var viewport=layout.Current.Viewport;int scale=BattleFloorAnchor.PhysicalPixelScale;
        var board=RunSession.Current.Board;var camera=Camera.main;
        var lo=camera.WorldToScreenPoint(board.transform.TransformPoint(new Vector3(-board.LocalBoardWidth/2,-board.LocalBoardHeight/2)));
        var hi=camera.WorldToScreenPoint(board.transform.TransformPoint(new Vector3(board.LocalBoardWidth/2,board.LocalBoardHeight/2)));
        var opening=new Rect((lo.x-viewport.xMin)/scale,(viewport.yMax-hi.y)/scale,(hi.x-lo.x)/scale,(hi.y-lo.y)/scale);
        int width=Mathf.CeilToInt(viewport.width/scale),height=Mathf.CeilToInt(viewport.height/scale);
        // Approved Revision04: feet at 204, walking slabs 188..220, separator 220..239.
        float offset=Mathf.Round((viewport.yMax-BattleFloorAnchor.EnemyFeetScreenY(slot))/scale)-204;
        string key=UnityEngine.Device.Screen.width+"x"+UnityEngine.Device.Screen.height+":"+viewport+":"+opening+":"+offset+":"+scale;
        if(pixelLabGeometry!=key)
        {
            pixelLabGeometry=key;
            if(pixelLabWorld){pixelLabWorld.gameObject.SetActive(false);Destroy(pixelLabWorld.gameObject);}
            var go=new GameObject("ApprovedIronveinEnvironment",typeof(RectTransform),typeof(Canvas));
            pixelLabWorld=go.GetComponent<Canvas>();pixelLabWorld.renderMode=RenderMode.WorldSpace;pixelLabWorld.worldCamera=camera;
            pixelLabWorld.sortingLayerName="Default";pixelLabWorld.sortingOrder=-190;pixelLabWorld.referencePixelsPerUnit=64;pixelLabWorld.pixelPerfect=true;
            pixelLabContent=(RectTransform)go.transform;pixelLabContent.pivot=new Vector2(0,1);pixelLabContent.sizeDelta=new Vector2(width,height);
            BuildIronvein(art,width,height,opening,offset);
            RefreshIronveinPortrait(art,(RectTransform)player.transform);
        }
        PositionPixelLabWorld();
        float unit=BattleFloorAnchor.Unit(layout.GetComponentInParent<Canvas>());
        if(!pixelLabBoardFrame)
        {
            pixelLabBoardFrame=DungeonPixelLabBattleArt.Rect("IronveinBoardFrame",layout.BoardArea,0,0,0,0);
            art.frames.Frame(pixelLabBoardFrame,"Board",90,unit);
        }
        float inset=(art.frames.Sprite("Board_Corner_TL").rect.width+art.frames.Sprite("Board_Rail_Top").rect.height)*.5f;
        float outside=Mathf.Ceil(opening.width)*unit+2*inset*unit;
        DungeonPixelLabBattleArt.TopRect(pixelLabBoardFrame,(layout.BoardArea.rect.width-outside)/2,(layout.BoardArea.rect.height-outside)/2,outside,outside);
        GameplayPixelGrid.Snap(pixelLabBoardFrame);
        var frame=pixelLabBoardFrame.GetComponentInChildren<ResponsiveModularFrameFitter>();frame.RefreshFrame();
        if(art.amberInset)
        foreach(RectTransform c in frame.transform.Find("NativeConnectors"))
        {
            var p=c.anchoredPosition+new Vector2(c.rect.width*.5f,-c.rect.height*.5f);var half=frame.GetComponent<RectTransform>().rect.size*.5f;
            bool center=c.gameObject.activeSelf&&(Mathf.Abs(p.x-half.x)<unit||Mathf.Abs(-p.y-half.y)<unit);
            var accent=c.Find("AmberInset") as RectTransform;
            if(!accent){accent=DungeonPixelLabBattleArt.Rect("AmberInset",c,0,0,0,0);accent.anchorMin=Vector2.zero;accent.anchorMax=Vector2.one;accent.offsetMin=accent.offsetMax=Vector2.zero;var im=accent.gameObject.AddComponent<Image>();im.sprite=art.amberInset;im.raycastTarget=false;}
            // The separate top beam owns its central crystal; only bottom/side connectors receive this inset.
            accent.gameObject.SetActive(center&&-p.y>half.y*.5f);
        }
        panelsReady=true;
    }

    void BuildIronvein(IronveinBattleAssembly art,int width,int height,Rect opening,float offset)
    {
        var background=art.pieces.First(p=>p.region=="IronveinNativeBackground");
        MineImage(background,pixelLabContent,new Rect(-width,-height,width*3,height*3));
        float left=opening.xMin-art.boardOpening.xMin,right=opening.xMax-art.boardOpening.xMax;
        float top=opening.yMin-art.boardOpening.yMin,bottom=opening.yMax-art.boardOpening.yMax;
        foreach(var p in art.pieces)
        {
            if(p.region=="IronveinNativeBackground")continue;
            var r=p.rect;
            if(p.region=="IronveinTopModules")
            {r.x+=width-360;r.y+=offset;if(p.type==Image.Type.Tiled){r.x=-width;r.width=width*3;}}
            else if(p.region=="Revision02_CombatFloor"||p.region=="Revision03_WalkingSurface")
            {
                r.x+=(width-360)*.5f;r.y+=offset;
                if(p.rect.x==0)
                    for(float x=r.x-r.width;x>-width;x-=r.width)MineImage(p,pixelLabContent,new Rect(x,r.y,r.width,r.height));
                if(p.rect.xMax>=360)
                    for(float x=r.xMax;x<width*2;x+=p.sprite.rect.width)MineImage(p,pixelLabContent,new Rect(x,r.y,p.sprite.rect.width,r.height));
            }
            else if(p.region=="Stage06_HUDSeating_PreviewOnly"){r.x+=(width-360)*.5f;r.y+=height-640;}
            else if(p.region=="IronveinLowerModules"||p.region=="Revision01_LowerCrystals"){r.x+=r.center.x<180?0:width-360;r.y+=height-640;}
            else if(p.region=="Stage02_Separator_PreviewOnly")
            {
                r.y+=top;
                if(p.name.EndsWith("RepeatLeft")){r.x+=left;r.width+=(right-left)*.5f;}
                else if(p.name.EndsWith("RepeatRight")){r.x+=(left+right)*.5f;r.width+=(right-left)*.5f;}
                else r.x+=p.rect.center.x<80?left:p.rect.center.x>280?right:(left+right)*.5f;
            }
            else
            {
                r.x+=r.center.x<180?left:right;
                if(p.name.Contains("UnderBoardRock")){r.x=-width;r.width=width*3;r.y+=bottom;}
                else if(p.name.Contains("TimberShaft")){r.y+=top;r.height+=bottom-top;}
                else if(p.name.Contains("Foot"))r.y+=bottom;
                else r.y+=Mathf.Lerp(top,bottom,Mathf.InverseLerp(244,548,p.rect.y));
            }
            r.x=Mathf.Round(r.x);r.y=Mathf.Round(r.y);r.width=Mathf.Round(r.width);r.height=Mathf.Round(r.height);
            MineImage(p,pixelLabContent,r);
        }
    }
    static Image MineImage(IronveinBattleAssembly.Piece p,Transform parent,Rect rect,float unit=1)
    {
        var r=DungeonPixelLabBattleArt.Rect(p.name.Replace('/','_'),parent,rect.x*unit,rect.y*unit,rect.width*unit,rect.height*unit);
        var im=r.gameObject.AddComponent<Image>();im.sprite=p.sprite;im.color=p.color;im.type=p.type;im.raycastTarget=false;
        im.pixelsPerUnitMultiplier=im.canvas.referencePixelsPerUnit/p.sprite.pixelsPerUnit/unit;
        if(p.reflection.x<0){r.pivot=new Vector2(1,r.pivot.y);r.localScale=new Vector3(-1,1,1);}
        return im;
    }
    void RefreshIronveinPortrait(IronveinBattleAssembly art,RectTransform player)
    {
        var prior=player.Find("IronveinPortraitBackdrop");if(prior){prior.gameObject.SetActive(false);Destroy(prior.gameObject);}
        float u=BattleFloorAnchor.Unit(player.GetComponentInParent<Canvas>());
        var root=DungeonPixelLabBattleArt.Rect("IronveinPortraitBackdrop",player,0,0,0,0);root.anchorMin=Vector2.zero;root.anchorMax=Vector2.one;root.offsetMin=root.offsetMax=Vector2.zero;root.SetAsFirstSibling();
        var mask=DungeonPixelLabBattleArt.Rect("PortraitMask",root,0,0,0,0);mask.anchorMin=Vector2.zero;mask.anchorMax=Vector2.one;mask.offsetMin=Vector2.one*2*u;mask.offsetMax=-mask.offsetMin;mask.gameObject.AddComponent<RectMask2D>();
        var fill=MineImage(new IronveinBattleAssembly.Piece{name="Overfilled cave recess",sprite=art.recess,type=Image.Type.Tiled},mask,new Rect(0,0,1,1),u);
        fill.rectTransform.anchorMin=Vector2.zero;fill.rectTransform.anchorMax=Vector2.one;fill.rectTransform.offsetMin=-art.recess.rect.size*u;fill.rectTransform.offsetMax=-fill.rectTransform.offsetMin;
    }
}
