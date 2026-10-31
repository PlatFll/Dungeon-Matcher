using System.Collections.Generic;
using UnityEngine;

/// <summary>Approved snow and ice glints: cosmetic only, native steps, pause and Reduced Motion aware.</summary>
public sealed class SnowyMountainAmbientMotion : MonoBehaviour
{
    public sealed class Track{public SpriteRenderer renderer;public Vector2 low,high;public float phase,opacity;public Sprite[] frames;}
    public readonly List<Track> tracks=new();
    public bool automatic=true;
    float clock;
    void Update(){if(!automatic)return;bool reduced=PresentationPreferences.ReducedMotion;if(!reduced)clock+=Time.deltaTime;Evaluate(clock,reduced);}
    public void Evaluate(float time,bool reduced)
    {
        foreach(var d in tracks)
        {
            if(reduced){d.renderer.enabled=false;continue;}
            float t=Mathf.Repeat(time+d.phase,12);
            if(d.frames!=null){int f=Mathf.FloorToInt(t/.125f);d.renderer.enabled=f<d.frames.Length;d.renderer.sprite=d.frames[Mathf.Min(f,d.frames.Length-1)];d.renderer.color=new Color(1,1,1,.65f);}
            else{float u=t/12,fade=Mathf.Min(Mathf.Clamp01(u/.12f),Mathf.Clamp01((1-u)/.12f));
                d.renderer.transform.localPosition=new Vector3(Mathf.Round(d.high.x+Mathf.Sin(u*Mathf.PI*2)*3),Mathf.Round(Mathf.Lerp(d.high.y,d.low.y,u)),0);
                d.renderer.color=new Color(1,1,1,d.opacity*fade);d.renderer.enabled=fade>0;}
        }
    }
}
