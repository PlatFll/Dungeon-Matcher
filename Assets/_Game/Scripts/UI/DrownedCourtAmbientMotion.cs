using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Plays approved 12 Hz native poses. Scaled time respects pause; Reduced Motion holds pose zero.</summary>
public sealed class DrownedCourtAmbientMotion : MonoBehaviour
{
    public sealed class Track
    {
        public Image image;
        public Vector2 origin;
        public DrownedCourtBattleAssembly.Pose[] poses;
    }
    public readonly List<Track> tracks=new();
    public bool automatic=true;
    float clock;
    void Update()
    {
        if(!automatic)return;
        bool reduced=PresentationPreferences.ReducedMotion;
        if(!reduced)clock+=Time.deltaTime;
        Evaluate(clock,reduced);
    }
    public void Evaluate(float time,bool reduced)
    {
        foreach(var t in tracks)
        {
            if(!t.image || t.poses==null || t.poses.Length==0)continue;
            var p=t.poses[reduced?0:Mathf.FloorToInt(Mathf.Repeat(time,t.poses.Length/12f)*12)%t.poses.Length];
            t.image.sprite=p.sprite;
            t.image.rectTransform.anchoredPosition=t.origin+new Vector2(p.offset.x,-p.offset.y);
            var c=t.image.color;c.a=p.alpha;t.image.color=c;
        }
    }
}
