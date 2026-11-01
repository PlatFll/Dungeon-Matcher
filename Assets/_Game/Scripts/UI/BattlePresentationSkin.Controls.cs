using TMPro;
using UnityEngine;
using UnityEngine.UI;

// One native control family for every battle zone. Section/board geometry and
// gameplay callbacks remain owned by their existing controllers.
public static partial class BattlePresentationSkin
{
    public static bool HasUnifiedControls => Shared("Ability") && Shared("Supply") && Shared("Settings") &&
        Resources.Load<Sprite>("UI/BattlePresentation/Unified/WaveBody");

    public static void PrepareSharedControl(RectTransform rect)
    {
        if(rect.TryGetComponent<NativePixelUiSurface>(out var native)) native.enabled=false;
        if(rect.TryGetComponent<ReferenceKitSurface>(out var registered)) registered.enabled=false;
        if(rect.TryGetComponent<Outline>(out var outline)) outline.enabled=false;
        rect.localScale=Vector3.one;
    }

    public static void PlaceTopControl(RectTransform rect, string kind, RectTransform top)
    {
        if(!rect || !top)return;
        PrepareSharedControl(rect);
        if(rect.parent!=top)rect.SetParent(top,false);
        rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(0,1);
        // CanvasScaler already chooses one whole physical-pixel scale. Never map
        // these controls through the independently responsive environment height.
        Vector2 settingsSize=Shared("Settings").rect.size;
        float right=top.rect.width-8, waveRight=right-settingsSize.x-8;
        Vector2 size;float left,y;
        if(kind=="Settings"){size=settingsSize;left=right-settingsSize.x;y=12;}
        else
        {
            float minLeft=Mathf.Ceil(top.rect.width*.29f)+6;
            float width=Mathf.Min(176,Mathf.Floor(waveRight-minLeft));
            size=new Vector2(Mathf.Max(64,width),48);
            left=Mathf.Clamp(Mathf.Round((top.rect.width-size.x)*.5f),minLeft,waveRight-size.x);y=10;
        }
        rect.sizeDelta=size;rect.anchoredPosition=new Vector2(left,-y);GameplayPixelGrid.Snap(rect);
        if(kind=="Wave")
        {
            var image=rect.GetComponent<Image>();
            image.sprite=Resources.Load<Sprite>("UI/BattlePresentation/Stone/WaveBody") ??
                Resources.Load<Sprite>("UI/BattlePresentation/Unified/WaveBody");
            image.type=Image.Type.Tiled;image.pixelsPerUnitMultiplier=1;image.color=Color.white;image.raycastTarget=false;
            var pendant=rect.Find("SharedWavePendant") as RectTransform;
            if(!pendant)
            {
                pendant=GameUi.Rect("SharedWavePendant",rect,new Vector2(16,19),Vector2.zero);
                var ornament=pendant.gameObject.AddComponent<Image>();ornament.sprite=Resources.Load<Sprite>("UI/BattlePresentation/Unified/WavePendant");ornament.raycastTarget=false;
            }
            pendant.anchorMin=pendant.anchorMax=pendant.pivot=new Vector2(.5f,1);
            pendant.anchoredPosition=new Vector2(0,-41);GameplayPixelGrid.Snap(pendant);
            var label=rect.GetComponentInChildren<TMP_Text>();
            if(label){label.rectTransform.anchorMin=Vector2.zero;label.rectTransform.anchorMax=Vector2.one;label.rectTransform.offsetMin=new Vector2(16,0);label.rectTransform.offsetMax=new Vector2(-16,0);label.textWrappingMode=TextWrappingModes.NoWrap;label.overflowMode=TextOverflowModes.Overflow;PixelTextFitter.ApplyNativeMenu(label,48);}
        }

    }

    public static float BottomControlY(RectTransform hud) => hud.rect.height<=110 ? -hud.rect.height*.5f+39 : 0;

    public static void PlaceSupply(Button button, Image icon, int index)
    {
        var r=(RectTransform)button.transform;PrepareSharedControl(r);
        r.anchorMin=r.anchorMax=r.pivot=Vector2.one*.5f;r.sizeDelta=Shared("Supply").rect.size;
        r.anchoredPosition=new Vector2(index==0?-132:132,BottomControlY((RectTransform)r.parent));GameplayPixelGrid.Snap(r);
        PrepareSharedControl(icon.rectTransform);GameplayPixelGrid.FitImage(icon,new Vector2(48,48));
    }
}
