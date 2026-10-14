using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Cosmetic only: no gameplay events, RNG calls, coroutines, colliders or input surfaces.</summary>
[DisallowMultipleComponent]
public sealed class BattleAmbientPresentation : MonoBehaviour
{
    private sealed class Detail
    {
        public SpriteRenderer renderer;
        public Sprite[] frames;
        public Vector3 origin;
        public string kind;
        public float phase;
    }
    private readonly List<Detail> details=new();
    private readonly Dictionary<Texture,Material> materials=new();
    private Transform worldRoot;
    private float clock;
    private bool ready;
    private GameplayPixelLayoutController layout;
    private RectTransform[] sideArt;
    private RectTransform[] sideLeaves;
    private RectTransform borderDetails;
    public float AnimationTime => clock;
    public int DetailCount => details.Count;
    private void Start()
    {
        layout=FindFirstObjectByType<GameplayPixelLayoutController>();
        FindFirstObjectByType<BoardVisuals>()?.RefreshPresentationFrame();
        var player=GameObject.Find("PlayerSection");var banner=BattlePresentationSkin.ZoneSprite("Banner");
        if(player!=null && banner!=null)
        {
            var r=GameUi.Rect("ZoneBanner",player.transform,new Vector2(44,64),new Vector2(0,-50));
            r.anchorMin=r.anchorMax=new Vector2(.5f,1);
            var image=r.gameObject.AddComponent<Image>();image.sprite=banner;image.raycastTarget=false;
            r.SetSiblingIndex(1);r.gameObject.AddComponent<NativePixelUiSurface>();
        }
        var side=BattlePresentationSkin.ZoneSprite("SideDressing");
        if(layout!=null && side!=null)
        {
            borderDetails=GameUi.Rect("ZoneBorderDetails",layout.transform,Vector2.zero,Vector2.zero);
            borderDetails.anchorMin=Vector2.zero;borderDetails.anchorMax=Vector2.one;
            borderDetails.offsetMin=borderDetails.offsetMax=Vector2.zero;borderDetails.SetAsFirstSibling();
            sideArt=new RectTransform[2];sideLeaves=new RectTransform[2];
            for(int i=0;i<2;i++)
            {
                var ornament=i==1 ? BattlePresentationSkin.ZoneSprite("SideRight") ?? side : side;
                sideArt[i]=UiDetail("ZoneSideDressing"+i,ornament,ornament.rect.size);
            }
        }
    }
    private void LateUpdate()
    {
        if(!ready)
        {
            var environment=FindFirstObjectByType<BattleBackgroundTilemapController>()?.ActiveEnvironment;
            if(environment==null || !environment.EnvironmentId.StartsWith("presentation-"))return;
            worldRoot=new GameObject("AmbientDetails").transform;worldRoot.SetParent(environment.transform,false);
            switch(BattlePresentationSkin.Folder)
            {
                case "Dungeon":
                    Add("Torch",157,124,"glow",0);Add("Torch",397,125,"glow",.4f);break;
                case "Forest":
                    Add("Lantern",178,104,"glow",0);Add("Lantern",466,117,"glow",2);
                    Add("Rune",255,107,"glow",0);Add("Waterfall",408,177,"flow",0);break;
                case "Court":
                    for(int i=0;i<6;i++)Add("Bubble",183+i*54,264,"bubble",i*1.63f);
                    break;
            }
            ready=true;
        }
        // Time.deltaTime obeys pause; reduced motion presents a stable, authored first frame.
        bool reduced=PresentationPreferences.ReducedMotion;
        if(!reduced)clock+=Time.deltaTime;
        if(sideArt!=null && layout!=null)
        {
            var g=layout.Current;
            var board=new Rect(g.Viewport.position+g.Board.position*g.Scale,g.Board.size*g.Scale);
            var viewport=g.Viewport;
            for(int i=0;i<2;i++)
            {
                float gutter=i==0?board.xMin-viewport.xMin:viewport.xMax-board.xMax;
                bool fits=gutter>=sideArt[i].rect.width*g.Scale;
                sideArt[i].gameObject.SetActive(fits);
                float x=i==0?board.xMin-sideArt[i].rect.width*.5f*g.Scale:board.xMax+sideArt[i].rect.width*.5f*g.Scale;
                sideArt[i].position=new Vector3(Mathf.Round(x),Mathf.Round(board.center.y),0);
                if(sideLeaves[i]!=null)
                {
                    sideLeaves[i].gameObject.SetActive(fits);
                    sideLeaves[i].position=new Vector3(Mathf.Round(x+(reduced?0:Mathf.Sin(clock*.7f+i)*g.Scale)),Mathf.Round(board.yMax-28*g.Scale),0);
                }
            }
        }
        foreach(var detail in details)
        {
            if(detail.renderer==null)continue;
            float t=reduced?0:clock+detail.phase;
            var position=detail.origin;var color=Color.white;int frame=0;
            if(detail.kind=="flame")frame=Mathf.FloorToInt(t*7)%detail.frames.Length;
            if(detail.kind=="flow")frame=Mathf.FloorToInt(t*6)%detail.frames.Length;
            if(detail.kind=="glow")color.a=reduced?.85f:.78f+Mathf.Sin(t*1.2f)*.18f;
            if(detail.kind=="sway" || detail.kind=="leaves")
                position.x+=Mathf.Round(Mathf.Sin(t*.7f)*Mathf.Sin(t*.19f)*(detail.kind=="sway"?2:1))/64;
            if(detail.kind=="bubble")
            {
                float cycle=t%12;
                if(!reduced){position.y+=Mathf.Floor(cycle*15)/64;position.x+=Mathf.Round(Mathf.Sin(t)*2)/64;}
                frame=reduced?0:cycle<11?Mathf.FloorToInt(t*2)%4:Mathf.Min(5,4+Mathf.FloorToInt((cycle-11)*2));
                color.a=reduced?.65f:cycle>11.8f?0:.85f;
            }
            detail.renderer.transform.localPosition=position;
            detail.renderer.sprite=detail.frames[frame];detail.renderer.color=color;
        }
    }
    private RectTransform UiDetail(string name,Sprite sprite,Vector2 size)
    {
        var rect=GameUi.Rect(name,borderDetails,size,Vector2.zero);
        var image=rect.gameObject.AddComponent<Image>();image.sprite=sprite;image.raycastTarget=false;
        return rect;
    }
    private void Add(string name,int x,int y,string kind,float phase)
    {
        var frames=Resources.LoadAll<Sprite>("UI/BattlePresentation/Motion/"+name).OrderBy(s=>s.name).ToArray();
        AddFrames(name,frames,x,y,kind,phase);
    }
    private void AddZone(string name,int x,int y,string kind,float phase)
    {
        var sprite=BattlePresentationSkin.ZoneSprite(name);
        if(sprite!=null)AddFrames(name,new[]{sprite},x,y,kind,phase);
    }
    private void AddFrames(string name,Sprite[] frames,int x,int y,string kind,float phase)
    {
        if(frames.Length==0)return;
        var child=new GameObject(name);child.transform.SetParent(worldRoot,false);
        var renderer=child.AddComponent<SpriteRenderer>();renderer.sprite=frames[0];
        var template=Resources.Load<Material>("UI/BattlePresentation/Motion/AmbientUnlit");
        if(template!=null)
        {
            // Keep each native sheet's binding independent under masked URP rendering.
            // The project's default lit material was displaying the last bound sheet
            // on all newly-created ambient renderers in the actual Unity captures.
            var texture=frames[0].texture;
            if(!materials.TryGetValue(texture,out var material))
            {material=new Material(template){mainTexture=texture};materials.Add(texture,material);}
            renderer.sharedMaterial=material;
        }
        renderer.sortingOrder=-72;renderer.maskInteraction=SpriteMaskInteraction.VisibleInsideMask;
        // UI ornaments share their original pixels in scenery without fractional texel scaling.
        child.transform.localScale=Vector3.one*(frames[0].pixelsPerUnit/64f);
        var origin=new Vector3((x-256)/64f,(272-y)/64f,0);
        child.transform.localPosition=origin;
        details.Add(new Detail{renderer=renderer,frames=frames,origin=origin,kind=kind,phase=phase});
    }
    private void OnDestroy()
    {
        if(worldRoot!=null)Destroy(worldRoot.gameObject);
        foreach(var material in materials.Values)if(material!=null)Destroy(material);
    }
}
