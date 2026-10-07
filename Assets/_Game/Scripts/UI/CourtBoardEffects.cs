using System.Collections.Generic;
using UnityEngine;

/// <summary>Bounded native-pixel effects. No board mutation, rewards, RNG or ownership.</summary>
public sealed class CourtBoardEffects : MonoBehaviour
{
    private sealed class Particle
    {
        public SpriteRenderer renderer;
        public Vector3 origin, velocity;
        public float elapsed, duration, pixel;
        public Sprite[] frames;
    }
    private sealed class Trail
    {
        public Gem gem;
        public Vector3 start,end;
        public float elapsed,duration,delay,next;
        public bool coffer,gravity;
        public int emitted;
    }
    private BoardController board;
    private readonly List<Particle> particles=new List<Particle>();
    private readonly List<Trail> trails=new List<Trail>();
    private int fallSequence;
    private float nextRipple;
    private GameplayThemeDefinition Theme=>GameplayThemeSkin.Current;
    private void Start()
    {
        board=GetComponent<BoardController>();
        if(board==null)return;
        board.CofferShellBroken+=ShellBreak;
        board.GemMotionPresented+=GemMoved;
        board.CofferMoving+=CofferMoved;
    }
    private void GemMoved(Gem gem,float duration,float delay,bool gravity)
    {
        if(!isActiveAndEnabled || !board.IsFlooded || gem==null || trails.Count>=10)return;
        // Local presentation counter deliberately does not touch saved game RNG.
        if(gravity && ++fallSequence%(PresentationPreferences.ReducedMotion?8:4)!=0)return;
        trails.Add(new Trail{gem=gem,duration=duration,delay=delay,gravity=gravity});
    }
    private void CofferMoved(Vector3 start,Vector3 end,float duration)
    {
        if(!isActiveAndEnabled || !board.IsFlooded)return;
        if(trails.Count>=10)trails.RemoveAt(0);
        trails.Add(new Trail{coffer=true,start=start,end=end,duration=duration});
        Ripple(start);
    }
    private void ShellBreak(Vector3 at)
    {
        if(!isActiveAndEnabled || !board.IsFlooded)return;
        int count=PresentationPreferences.ReducedMotion?2:4;
        for(int i=0;i<count;i++)
        {
            float side=i%2==0?-1:1;
            Emit(Theme?.shellFragment,at+Vector3.right*side*board.CellSize*.22f,
                new Vector3(side*(i<2?.8f:.5f),i<2?.6f:-.2f)*board.CellSize,.28f);
        }
    }
    private void Ripple(Vector3 at)
    {
        var frames=Theme?.waterRippleFrames;
        if(PresentationPreferences.ReducedMotion || Time.time<nextRipple || frames==null || frames.Length==0)return;
        nextRipple=Time.time+.1f;
        Emit(frames[0],at,Vector3.zero,.24f,frames);
    }
    private void Emit(Sprite sprite,Vector3 at,Vector3 velocity,float duration,Sprite[] frames=null)
    {
        if(sprite==null || particles.Count>=24)return;
        var go=new GameObject(frames==null?"CourtParticle":"CourtRipple");go.transform.SetParent(board.transform,false);
        var renderer=go.AddComponent<SpriteRenderer>();renderer.sprite=sprite;
        renderer.sortingLayerName="Gems";renderer.sortingOrder=28;
        renderer.maskInteraction=SpriteMaskInteraction.VisibleInsideMask;
        float pixel=board.CellSize*.88f/32;
        go.transform.localScale=Vector3.one*pixel*sprite.pixelsPerUnit;go.transform.position=at;
        if(frames!=null)renderer.color=new Color(1,1,1,.6f);
        particles.Add(new Particle{renderer=renderer,origin=at,velocity=velocity,duration=duration,pixel=pixel,frames=frames});
    }
    private void LateUpdate()
    {
        if(board==null)return;
        if(!board.IsFlooded){Clear();return;}
        for(int i=trails.Count-1;i>=0;i--)
        {
            var t=trails[i];t.elapsed+=Time.deltaTime;float age=t.elapsed-t.delay;
            if(age<0)continue;
            if(age>t.duration || (!t.coffer && t.gem==null)){trails.RemoveAt(i);continue;}
            int limit=PresentationPreferences.ReducedMotion?(t.coffer?2:1):(t.coffer?6:t.gravity?1:2);
            if(age<t.next || t.emitted>=limit)continue;
            var at=t.coffer?Vector3.Lerp(t.start,t.end,Mathf.SmoothStep(0,1,age/t.duration)):t.gem.transform.position;
            Emit(Theme?.waterMicroBubble,at+Vector3.right*(t.emitted%2==0?-.06f:.06f)*board.CellSize,
                Vector3.up*board.CellSize*.45f,.24f);
            if(!t.gravity && t.emitted==0)Ripple(at);
            t.emitted++;t.next=age+t.duration/limit;
        }
        for(int i=particles.Count-1;i>=0;i--)
        {
            var p=particles[i];p.elapsed+=Time.deltaTime;
            if(p.renderer==null || p.elapsed>=p.duration || (p.frames!=null && PresentationPreferences.ReducedMotion))
            {if(p.renderer!=null)Destroy(p.renderer.gameObject);particles.RemoveAt(i);continue;}
            Vector3 point=p.origin+p.velocity*p.elapsed;
            point.x=Mathf.Round(point.x/p.pixel)*p.pixel;point.y=Mathf.Round(point.y/p.pixel)*p.pixel;
            p.renderer.transform.position=point;
            if(p.frames!=null)p.renderer.sprite=p.frames[Mathf.Min(p.frames.Length-1,(int)(p.elapsed/p.duration*p.frames.Length))];
        }
    }
    private void Clear()
    {
        foreach(var p in particles)if(p.renderer!=null)Destroy(p.renderer.gameObject);
        particles.Clear();trails.Clear();
    }
    private void OnDisable()=>Clear();
    private void OnDestroy()
    {
        if(board==null)return;
        board.CofferShellBroken-=ShellBreak;board.GemMotionPresented-=GemMoved;board.CofferMoving-=CofferMoved;
    }
}
