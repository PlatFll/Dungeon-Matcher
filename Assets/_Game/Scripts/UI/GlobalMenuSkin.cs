using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>Native non-zone menu presentation. Navigation and run state stay with their existing owners.</summary>
public static class GlobalMenuSkin
{
    public static Sprite Load(string name) => Resources.Load<Sprite>("UI/GlobalMenu/" + name);
    public static RectTransform Root(Transform parent, string name, Vector2 size)
    {
        var root=GameUi.Rect(name,parent,size,Vector2.zero);
        root.gameObject.AddComponent<GlobalMenuPixelRoot>().Initialize(size);
        return root;
    }
    public static void Place(RectTransform rect, Transform parent, Vector2 size, Vector2 position)
    {
        rect.SetParent(parent,false); rect.anchorMin=rect.anchorMax=rect.pivot=Vector2.one*.5f;
        rect.localScale=Vector3.one;rect.sizeDelta=size;rect.anchoredPosition=position;
    }
    public static Image Art(string name,Transform parent,string asset,Vector2 size,Vector2 position)
    {
        var image=GameUi.Rect(name,parent,size,position).gameObject.AddComponent<Image>();
        image.sprite=Load(asset);image.color=Color.white;image.raycastTarget=false;
        return image;
    }
    public static TMP_Text Label(string name,Transform parent,string text,Vector2 size,Vector2 position,int em=16, bool dark=false)
    {
        var label=GameUi.Label(name,parent,text,size,position,em);
        StyleText(label,em,dark);return label;
    }
    public static void StyleText(TMP_Text text,int em=16,bool dark=false)
    {
        text.font=GameUi.TmpFont;text.fontSharedMaterial=GameUi.TmpFont.material;text.fontSize=em;
        text.enableAutoSizing=false;text.fontStyle=FontStyles.Normal;
        text.textWrappingMode=TextWrappingModes.NoWrap;text.overflowMode=TextOverflowModes.Truncate;
        text.color=dark?new Color32(20,21,34,255):new Color32(248,244,239,255);
        PixelTextFitter.ApplyNativeMenu(text,em);
    }
    public static void Skin(Button button,string family)
    {
        if(button.TryGetComponent<NativePixelUiSurface>(out var old))old.enabled=false;
        button.image.sprite=Load(family);button.image.type=Image.Type.Simple;button.image.color=Color.white;
        button.image.preserveAspect=false;button.image.CrossFadeColor(Color.white,0,true,true);
        button.transition=Selectable.Transition.SpriteSwap;
        var normal=Load(family);
        button.spriteState=new SpriteState {highlightedSprite=Load(family+"Hover")??normal,selectedSprite=Load(family+"Hover")??normal,
            pressedSprite=Load(family+"Pressed")??normal,disabledSprite=Load(family+"Disabled")??normal};
    }
    public static void Gear(Button button)
    {
        if(Load("Gear")==null)return;
        button.image.sprite=null;button.image.color=Color.clear;
        var face=Art("GlobalGear",button.transform,"Gear",((RectTransform)button.transform).rect.size,Vector2.zero);
        button.targetGraphic=face;Skin(button,"Gear");
        face.gameObject.AddComponent<NativePixelUiSurface>();
    }
    public static Button Button(string name,Transform parent,string text,string family,string icon,Vector2 position,UnityAction action)
    {
        var sprite=Load(family);var size=sprite!=null?sprite.rect.size:new Vector2(184,38);
        var button=GameUi.Button(name,parent,text,size,position,action);DressButton(button,family,icon);return button;
    }
    public static void DressButton(Button button,string family,string icon)
    {
        Skin(button,family);Vector2 size=Load(family).rect.size;
        ((RectTransform)button.transform).sizeDelta=size;
        var label=button.GetComponentInChildren<TMP_Text>();
        if(family=="Gear") {label.text="";Place(label.rectTransform,button.transform,size,Vector2.zero);return;}
        bool light=family=="Play"||family=="Secondary"||family=="Challenge";
        float inset=family=="Play"?62:light?56:48;
        Place(label.rectTransform,button.transform,new Vector2(size.x-inset,size.y-12),new Vector2(family=="Play"?19:light?14:16,0));
        // Short primary pause actions have the stronger lettering of the
        // reference; longer neutral actions retain their verified native fit.
        int em=family=="Play"?64:family=="Affirmative"||family=="Destructive"?24:16;
        StyleText(label,em,light);
        if(icon!=null)
        {
            float iconX=-size.x/2+(family=="Play"?32:26);
            if(light)Art("IconPlate",button.transform,"IconPlate",new Vector2(32,32),new Vector2(iconX,0));
            Art("ActionIcon",button.transform,icon,Load(icon).rect.size,new Vector2(iconX,0));
        }
    }
    public static void Panel(RectTransform panel,Vector2 size,string title)
    {
        panel.sizeDelta=size;
        if(panel.TryGetComponent<NativePixelUiSurface>(out var native))native.enabled=false;
        if(panel.TryGetComponent<Outline>(out var outline))outline.enabled=false;
        var fill=panel.GetComponent<Image>();fill.sprite=Load("Panel");fill.color=Color.white;fill.type=Image.Type.Tiled;
        // 100 native PPU cancels the canvas reference PPU; root owns physical pixel scale.
        fill.pixelsPerUnitMultiplier=fill.canvas.referencePixelsPerUnit/100f;
        var heading=panel.Find("Title")?.GetComponent<TMP_Text>();
        if(heading==null)heading=Label("Title",panel,title,new Vector2(size.x-28,36),new Vector2(0,size.y/2-26),48);
        else {heading.text=title;Place(heading.rectTransform,panel,new Vector2(size.x-28,36),new Vector2(0,size.y/2-26));StyleText(heading,48);}
        for(int x=-1;x<=1;x+=2)for(int y=-1;y<=1;y+=2)
            Art("CornerJewel",panel,"CornerJewel",new Vector2(22,22),new Vector2(x*(size.x/2-7),y*(size.y/2-7)));
        Art("SkullCrest",panel,"SettingsCrest",new Vector2(64,32),new Vector2(0,size.y/2+5));
        var rule=Art("HeaderRule",panel,"HeaderRule",new Vector2(size.x-18,8),new Vector2(0,size.y/2-46));
        rule.type=Image.Type.Tiled;rule.pixelsPerUnitMultiplier=fill.pixelsPerUnitMultiplier;
    }
    public static void SettingsRows(Transform panel,float top)
    {
        Row("Music","MUSIC","Music",panel,top,()=>!AudioPreferences.MusicMuted,()=>AudioPreferences.SetMusicMuted(!AudioPreferences.MusicMuted));
        Row("SFX","SFX","Sfx",panel,top-34,()=>!AudioPreferences.SfxMuted,()=>AudioPreferences.SetSfxMuted(!AudioPreferences.SfxMuted));
        Row("ReducedMotion","REDUCED MOTION","Motion",panel,top-68,()=>PresentationPreferences.ReducedMotion,()=>PresentationPreferences.SetReducedMotion(!PresentationPreferences.ReducedMotion));
        Row("Vibration","VIBRATION","Vibration",panel,top-102,()=>!AudioPreferences.VibrationMuted,()=>AudioPreferences.SetVibrationMuted(!AudioPreferences.VibrationMuted));
    }
    private static void Row(string name,string title,string icon,Transform parent,float y,Func<bool> value,UnityAction toggle)
    {
        var button=GameUi.Button(name,parent,"",new Vector2(208,32),new Vector2(0,y),()=>{});Skin(button,"SettingsRow");
        var label=button.GetComponentInChildren<TMP_Text>();label.text=title;
        Place(label.rectTransform,button.transform,new Vector2(116,24),new Vector2(-15,0));
        StyleText(label,16);label.alignment=TextAlignmentOptions.Left;
        Art("SettingIcon",button.transform,icon,new Vector2(24,24),new Vector2(-87,0));
        var state=Art("Toggle",button.transform,value()?"ToggleOn":"ToggleOff",new Vector2(52,24),new Vector2(73,0));
        var stateLabel=Label("State",state.transform,value()?"ON":"OFF",new Vector2(28,20),new Vector2(-9,0),16);
        button.onClick.AddListener(()=>{toggle();state.sprite=Load(value()?"ToggleOn":"ToggleOff");stateLabel.text=value()?"ON":"OFF";});
    }
}

/// <summary>One layout writer for a native menu stage, independent of the battle canvas's density.</summary>
public sealed class GlobalMenuPixelRoot : MonoBehaviour
{
    private Vector2 nativeSize;
    private RectTransform rect;
    public static Rect? ValidationSafeArea;
    public bool AlignTop { get; set; }
    public float AvailableHeight { get; private set; } = 640;
    public void Initialize(Vector2 size){nativeSize=size;rect=(RectTransform)transform;Refresh();}
    private void LateUpdate()=>Refresh();
    private void Refresh()
    {
        if(rect==null)return;
        var canvas=GetComponentInParent<Canvas>().rootCanvas;
        Rect safe=ValidationSafeArea??UnityEngine.Device.Screen.safeArea;
        // Decorative hall pixels can extend beneath phone cutouts; interactive
        // home content fits the 600px minimum, with edge labels clamped below.
        float minimumHeight=nativeSize.y==640?600:nativeSize.y;
        int zoom=Mathf.Max(1,Mathf.FloorToInt(Mathf.Min(safe.width/nativeSize.x,safe.height/minimumHeight)));
        AvailableHeight=safe.height/zoom;
        float parentScale=Mathf.Max(.001f,canvas.scaleFactor);
        rect.localScale=Vector3.one*(zoom/parentScale);
        Vector2 center=new Vector2(Mathf.Round(safe.center.x),Mathf.Round(safe.center.y));
        if(AlignTop)center.y+=Mathf.Round(Mathf.Max(0,AvailableHeight-nativeSize.y)*zoom*.5f);
        RectTransformUtility.ScreenPointToWorldPointInRectangle((RectTransform)rect.parent,center,canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.worldCamera,out Vector3 position);
        rect.position=position;
    }
}
