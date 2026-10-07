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
    }
    private BoardController board;
    private GameplayThemeDefinition theme;
    private readonly List<Particle> particles=new List<Particle>();
    private void Start()
    {
        board=GetComponent<BoardController>();theme=GameplayThemeSkin.Current;
        if(board!=null)board.CofferShellBroken+=ShellBreak;
    }
    private void ShellBreak(Vector3 at)
    {
        int count=PresentationPreferences.ReducedMotion?2:4;
        for(int i=0;i<count;i++)
        {
            float side=i%2==0?-1:1;
            Emit(theme?.shellFragment,at+Vector3.right*side*board.CellSize*.22f,
                new Vector3(side*(i<2?.8f:.5f),i<2?.6f:-.2f)*board.CellSize,.28f);
        }
    }
    private void Emit(Sprite sprite,Vector3 at,Vector3 velocity,float duration)
    {
        if(sprite==null || particles.Count>=24)return;
        var go=new GameObject("CourtParticle");go.transform.SetParent(board.transform,false);
        var renderer=go.AddComponent<SpriteRenderer>();renderer.sprite=sprite;
        renderer.sortingLayerName="Gems";renderer.sortingOrder=28;
        renderer.maskInteraction=SpriteMaskInteraction.VisibleInsideMask;
        float pixel=board.CellSize*.88f/32;
        go.transform.localScale=Vector3.one*pixel*sprite.pixelsPerUnit;go.transform.position=at;
        particles.Add(new Particle{renderer=renderer,origin=at,velocity=velocity,duration=duration,pixel=pixel});
    }
    private void LateUpdate()
    {
        for(int i=particles.Count-1;i>=0;i--)
        {
            var p=particles[i];p.elapsed+=Time.deltaTime;
            if(p.renderer==null || p.elapsed>=p.duration || !board.IsFlooded)
            {if(p.renderer!=null)Destroy(p.renderer.gameObject);particles.RemoveAt(i);continue;}
            Vector3 point=p.origin+p.velocity*p.elapsed;
            point.x=Mathf.Round(point.x/p.pixel)*p.pixel;point.y=Mathf.Round(point.y/p.pixel)*p.pixel;
            p.renderer.transform.position=point;
        }
    }
    private void OnDestroy(){if(board!=null)board.CofferShellBroken-=ShellBreak;}
}
