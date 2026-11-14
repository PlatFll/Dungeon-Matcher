using System.Linq;
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
            var column=DungeonPixelLabBattleArt.Rect(side==0?"Left pillar":"Right pillar",sideArchitecture,x,0,32,height);
            var shaft=art.Image("Side_Shaft",column,0,20);
            shaft.type=Image.Type.Tiled;shaft.pixelsPerUnitMultiplier=1;
            shaft.rectTransform.sizeDelta=new Vector2(32,height-20-21);
            // Capitals and feet conceal the partial final course of the repeat.
            art.Image("Side_Capital",column,0,0);
            art.Image("Side_Base",column,0,height-23);

            float torchY=Mathf.Max(53,boardTop-start+35);
            float skullY=Mathf.Round(boardTop-start+(boardBottom-boardTop)*.47f);
            float shelfY=boardBottom-start-7;
            // These copies share the shaft's exact 15px seam grid; only local stone colors differ.
            float warmRow=20+15*Mathf.Floor((torchY-20)/15);
            art.Image("Side_Shaft_Warm_Faint",column,0,warmRow-15);
            art.Image("Side_Shaft_Warm",column,0,warmRow);
            art.Image("Side_Shaft_Warm_Faint",column,0,warmRow+15);
            art.Image("Side_SkullRelief",column,0,skullY);
            art.Image("Side_Recess",column,0,boardBottom-start+14);
            // Existing approved iron sources keep the same native scale and lighting.
            HangSideChain(art,column,9,19,Mathf.Max(0,Mathf.FloorToInt((torchY-19-37)/14)));
            HangSideChain(art,column,9,skullY+37,side==0?2:3,side==0?"Side_Chain_End_Left":"Side_Chain_End_Right");
            art.Image("Side_Torch_ReferenceStyle",column,4,torchY);
            SideCandleShelf(art,column,shelfY,side==0?"Side_Candles_Tall":"Side_Candles_Mixed",21);
            // The lower cluster rests directly on the heavy base, without another floating shelf.
            art.Image(side==0?"Side_Candles_Short":"Side_Candles_Tall",column,4,height-23-(side==0?22:21));
            // One grounded corner grouping on each side. Existing approved
            // native props rest on rubble at the physical bottom, outside HUD.
            var corner = DungeonPixelLabBattleArt.Rect(side == 0 ? "Left corner dressing" : "Right corner dressing",
                sideArchitecture, side == 0 ? 0 : width-60, height-62, 60, 62);
            art.Image("Rubble",corner,side==0?6:0,33);
            art.Image(side==0?"Jar":"Barrel",corner,side==0?0:20,side==0?14:6);
            art.Image("Bones",corner,side==0?27:0,34);
            // Use actual vertical overlap too: on tall phones the formation
            // sits below the buttons and can remain completely on-screen.
            // Only narrow layouts need the outer stones to peek off-screen.
            var controls = layout.BottomHud.GetComponentsInChildren<Button>()
                .Select(b => GameplayPixelLayoutController.ScreenRect((RectTransform)b.transform)).ToArray();
            float outward = 0;
            foreach (var prop in corner.GetComponentsInChildren<Image>())
            {
                var rect = GameplayPixelLayoutController.ScreenRect(prop.rectTransform);
                foreach (var button in controls)
                    if (rect.yMin < button.yMax && rect.yMax > button.yMin)
                        outward = Mathf.Max(outward, side == 0 ? rect.xMax-button.xMin : button.xMax-rect.xMin);
            }
            corner.anchoredPosition += Vector2.right * (Mathf.Ceil(outward/scale) * (side == 0 ? -1 : 1));

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
