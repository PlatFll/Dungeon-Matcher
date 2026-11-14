using UnityEngine;
using UnityEngine.UI;

public sealed partial class DungeonPresentationArt
{
    RectTransform sideArchitecture;
    string sideArchitectureGeometry;

    // Decoration remains in the existing world canvas below all gems and UI.
    // Use physical edges for the masonry and live board bounds for its ornaments.
    void RefreshSideArchitecture(DungeonPixelLabBattleArt art, GameplayPixelLayoutController layout, int floor)
    {
        var viewport=layout.Current.Viewport;
        float scale=DungeonPixelLabBattleArt.PhysicalPixelScale;
        var board=GameplayPixelLayoutController.ScreenRect(pixelLabBoardFrame);
        string key=pixelLabContent.GetInstanceID()+":"+board+":"+viewport+":"+scale;
        if(sideArchitecture && sideArchitectureGeometry==key)return;
        sideArchitectureGeometry=key;
        if(sideArchitecture){sideArchitecture.gameObject.SetActive(false);Destroy(sideArchitecture.gameObject);}
        float left=-viewport.xMin/scale, width=UnityEngine.Device.Screen.width/scale;
        float end=viewport.yMax/scale, start=floor+34;
        float boardTop=Mathf.Round((viewport.yMax-board.yMax)/scale);
        float boardBottom=Mathf.Round((viewport.yMax-board.yMin)/scale);
        sideArchitecture=DungeonPixelLabBattleArt.Rect("Side architecture",pixelLabContent,left,start,width,end-start);
        for(int side=0;side<2;side++)
        {
            float x=side==0?0:width-32, height=end-start;
            // Each 112x128 foundation has a 22px column socket and a broad foot.
            // Its last occupied row meets the physical bottom; only the outer
            // bedrock edge overscans. The UI remains on its existing higher canvas.
            float foundationTop=height-126;
            var column=DungeonPixelLabBattleArt.Rect(side==0?"Left pillar":"Right pillar",sideArchitecture,x,0,32,height);
            var shaft=art.Image("Side_Shaft",column,0,20);
            shaft.type=Image.Type.Tiled;shaft.pixelsPerUnitMultiplier=1;
            shaft.rectTransform.sizeDelta=new Vector2(32,foundationTop-20);
            // The native foundation continues the column without a stacked base.
            art.Image("Side_Capital",column,0,0);

            float torchY=Mathf.Max(53,boardTop-start+35);
            float skullY=Mathf.Round(boardTop-start+(boardBottom-boardTop)*.47f);
            float shelfY=boardBottom-start-7;
            // These copies share the shaft's exact 15px seam grid; only local stone colors differ.
            float warmRow=20+15*Mathf.Floor((torchY-20)/15);
            art.Image("Side_Shaft_Warm_Faint",column,0,warmRow-15);
            art.Image("Side_Shaft_Warm",column,0,warmRow);
            art.Image("Side_Shaft_Warm_Faint",column,0,warmRow+15);
            art.Image("Side_SkullRelief",column,0,skullY);
            if(boardBottom-start+14+art.Sprite("Side_Recess").rect.height<=foundationTop)
                art.Image("Side_Recess",column,0,boardBottom-start+14);
            // Existing approved iron sources keep the same native scale and lighting.
            HangSideChain(art,column,9,19,Mathf.Max(0,Mathf.FloorToInt((torchY-19-37)/14)));
            HangSideChain(art,column,9,skullY+37,side==0?2:3,side==0?"Side_Chain_End_Left":"Side_Chain_End_Right");
            art.Image("Side_Torch_ReferenceStyle",column,4,torchY);
            SideCandleShelf(art,column,shelfY,side==0?"Side_Candles_Tall":"Side_Candles_Mixed",21);
            // One native sprite per side, no jars/barrels/bone piles and no
            // extra foot layered over the connected formation.
            art.Image(side==0?"Dungeon_Foundation_Left":"Dungeon_Foundation_Right",
                sideArchitecture,side==0?-11:width-101,foundationTop);

        }
    }

    static void HangSideChain(DungeonPixelLabBattleArt art,RectTransform root,float x,float y,int repeats,string end="Chain_End")
    {
        var chain=DungeonPixelLabBattleArt.Rect("Mounted chain",root,x,y,14,25+repeats*14+12);
        art.Image("Chain_Anchor",chain,0,0);
        for(int i=0;i<repeats;i++)art.Image("Chain_Repeat",chain,0,25+i*14);
        art.Image(end,chain,0,25+repeats*14);
    }
    static void SideCandleShelf(DungeonPixelLabBattleArt art,RectTransform root,float y,string candle,int bottom)
    {
        art.Image("Side_Ledge",root,0,y);
        art.Image(candle,root,4,y-bottom);
    }
}
