using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public sealed partial class ForestFoundationPlayTests
{
    const string SnowStage09="ArtSource/SnowyMountainStaged/Stage09";
    [UnityTest,Explicit] public IEnumerator SnowyMountainStage09AssemblyReview()
    { yield return SnowyMountainAssembledReview(SnowStage09,true); }

    void SnowStage09RecordAssembly(string output,string size,Camera cam,GameObject plate,GameObject floor,GameObject sides,GameObject lower,RectTransform portrait,RectTransform mask,RectTransform dressing,RectTransform board,GameObject atmosphere)
    { SnowStage09RecordAssembly(output,size,cam,new Transform[]{plate.transform,floor.transform,sides.transform,lower.transform,portrait,mask,dressing,board,atmosphere.transform}); }

    void SnowStage09RecordAssembly(string output,string size,Camera cam,Transform[] roots)
    {
        var items=new List<SnowAssemblyItem>();
        foreach(var root in roots.Where(r=>r))
        {
            foreach(var sr in root.GetComponentsInChildren<SpriteRenderer>().Where(r=>r.sprite))
            {
                var lo=cam.WorldToScreenPoint(sr.bounds.min);var hi=cam.WorldToScreenPoint(sr.bounds.max);
                items.Add(new SnowAssemblyItem{group=root.name,name=sr.name,sprite=sr.sprite.name,sourceSize=sr.sprite.rect.size,screen=Rect.MinMaxRect(lo.x,lo.y,hi.x,hi.y),order=sr.sortingOrder,kind="SpriteRenderer"});
                Assert.That(sr.sprite.texture.filterMode,Is.EqualTo(FilterMode.Point));
            }
            foreach(var im in root.GetComponentsInChildren<Image>().Where(i=>i.sprite))
            {
                items.Add(new SnowAssemblyItem{group=root.name,name=im.name,sprite=im.sprite.name,sourceSize=im.sprite.rect.size,screen=GameplayPixelLayoutController.ScreenRect(im.rectTransform),order=im.transform.GetSiblingIndex(),kind="UI Image"});
                Assert.That(im.raycastTarget,Is.False,"New scenery cannot block shared controls");
                Assert.That(im.sprite.texture.filterMode,Is.EqualTo(FilterMode.Point));
            }
        }
        File.WriteAllText(output+"/QA/Assembly-"+size+".json",JsonUtility.ToJson(new SnowAssemblyItems{items=items.ToArray()},true));
    }

    IEnumerator SnowStage09LiveReview(string output,Camera cam,SnowAtmosphereReview atmosphere,GameplayPixelLayoutController layout,Rect frame,float pixel)
    {
        float originalCapture=Time.captureDeltaTime;
        var frames=new List<SnowLiveFrame>();
        var initialGeometry=SnowGeometry(layout);
        var boardSprites=Run.Board.GetComponentsInChildren<SpriteRenderer>().Where(r=>r.gameObject.activeInHierarchy).ToArray();
        // Hint motion deliberately translates a gem (BoardHintController.PulseHint).
        // Retain exact logical occupancy, sprite, scale and color during live playback;
        // the preceding frozen 96-frame pass also checks every transform exactly.
        var fixedBoard=boardSprites.Where(r=>!r.GetComponentInParent<Gem>()).ToArray();
        var beforeFixed=SnowBoardSpriteState(fixedBoard);
        var beforeBoard=SnowStage09BoardContents();
        var actors=layout.TopHud.GetComponentsInChildren<EnemySlotUI>().Where(s=>s.CurrentEnemy).ToArray();
        var previousImages=actors.Select(s=>s.GetComponentsInChildren<Image>().Select(i=>i.sprite?i.sprite.GetInstanceID():0).ToArray()).ToArray();
        bool[] moved=new bool[actors.Length];
        float start=Time.time;
        try
        {
            // Live production idle playback; only the optional scenery clock is sampled here.
            Time.captureDeltaTime=1f/24;Time.timeScale=1;
            for(int n=0;n<96;n++)
            {
                float elapsed=Time.time-start;
                atmosphere.Sample(elapsed,false);atmosphere.Validate(frame,pixel);
                frames.Add(new SnowLiveFrame{file=n.ToString("D3")+".png",seconds=elapsed});
                yield return ForegroundScreenshot(output+"/Preview/Live/"+n.ToString("D3")+".png");
                var geometry=SnowGeometry(layout);
                Assert.That(geometry.feet,Is.EqualTo(initialGeometry.feet),"Live idle roots retain the shared floor");
                Assert.That(SnowBoardSpriteState(fixedBoard),Is.EqualTo(beforeFixed),"Cells and non-gem board visuals remain fixed");
                Assert.That(SnowStage09BoardContents(),Is.EqualTo(beforeBoard),"Live hints do not mutate board occupancy, gem identity, artwork, scale or color");
                for(int i=0;i<actors.Length;i++)
                    moved[i]|=!actors[i].GetComponentsInChildren<Image>().Select(im=>im.sprite?im.sprite.GetInstanceID():0).SequenceEqual(previousImages[i]);
            }
            Assert.That(moved.All(v=>v),Is.True,"All three live enemy idles advance in the preview");
            File.WriteAllText(output+"/QA/LiveTiming.json",JsonUtility.ToJson(new SnowLiveFrames{frames=frames.ToArray(),endSeconds=Time.time-start,enemyIdlesAdvanced=moved},true));
        }
        finally {Time.captureDeltaTime=originalCapture;Time.timeScale=0;}
    }
    string SnowStage09BoardContents()
    {
        var values=new List<string>();
        for(int y=0;y<Run.Board.Height;y++)for(int x=0;x<Run.Board.Width;x++)
        {
            var g=Run.Board.GetGem(x,y);Assert.That(g,Is.Not.Null);
            var sr=g.GetComponent<SpriteRenderer>();
            values.Add(x+","+y+":"+g.GetInstanceID()+":"+g.Column+","+g.Row+":"+g.Type+":"+g.SpecialType+":"+sr.sprite.GetInstanceID()+":"+sr.transform.lossyScale.ToString("R")+":"+sr.color);
        }
        return string.Join(";",values);
    }
    [Serializable] sealed class SnowAssemblyItem {public string group,name,sprite,kind;public Vector2 sourceSize;public Rect screen;public int order;}
    [Serializable] sealed class SnowAssemblyItems {public SnowAssemblyItem[] items;}
    [Serializable] sealed class SnowLiveFrame {public string file;public float seconds;}
    [Serializable] sealed class SnowLiveFrames {public SnowLiveFrame[] frames;public float endSeconds;public bool[] enemyIdlesAdvanced;}
}
