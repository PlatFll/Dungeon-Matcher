using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public sealed partial class ForestFoundationPlayTests
{
    const string ForegroundRoot="ArtSource/IronveinBelowTopHUD/DrillForeground";
    [UnityTest,Explicit] public IEnumerator IronveinDrillForegroundBaseline()=>DrillForegroundProof(false);
    [UnityTest,Explicit] public IEnumerator IronveinDrillForegroundVisible()=>DrillForegroundProof(true);
    IEnumerator DrillForegroundProof(bool verify)
    {
        yield return LaunchMineKit("pickaxe_delver","rivet_gunner","stonewright");QuietKitFixture();
        var board=Run.Board;var view=board.GetComponent<MineEnvironmentView>();
        string folder=ForegroundRoot+(verify?"/After":"/Before");Directory.CreateDirectory(folder);
        var records=new List<DrillForegroundRecord>();
        foreach(var size in new[]{new Vector3Int(720,1280,0),new Vector3Int(1080,1920,0),new Vector3Int(1080,2400,0),new Vector3Int(1080,2400,1),new Vector3Int(360,640,0)})
        {
            GameplayPixelLayoutController.ValidationSafeArea=null;
            typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{new Vector2Int(size.x,size.y)});
            yield return Until(()=>Screen.width==size.x&&Screen.height==size.y,"foreground viewport");
            GameplayPixelLayoutController.ValidationSafeArea=size.z==1?new Rect(24,96,size.x-48,size.y-216):(Rect?)null;
            for(int n=0;n<12;n++)yield return null;
            Time.timeScale=0;Canvas.ForceUpdateCanvases();
            var frame=(RectTransform)GameObject.Find("IronveinBoardFrame").transform;
            var layout=UnityEngine.Object.FindFirstObjectByType<GameplayPixelLayoutController>();
            Assert.That(GameplayPixelLayoutValidator.Validate(layout,out _),Is.Empty);
            string label=size.x+"x"+size.y+(size.z==1?"-safe":"");
            var rec=new DrillForegroundRecord{size=label,frame=GameplayPixelLayoutController.ScreenRect(frame)};
            if(verify)
            {
                var before=JsonUtility.FromJson<DrillForegroundRecords>(File.ReadAllText(ForegroundRoot+"/Before/Measurements.json")).items.Single(r=>r.size==label);
                Assert.That(Vector2.Distance(rec.frame.position,before.frame.position),Is.LessThan(.1f),"Frame placement preserved: "+rec.frame+" vs "+before.frame);
                Assert.That(Vector2.Distance(rec.frame.size,before.frame.size),Is.LessThan(.1f),"Frame dimensions preserved");
                var canvas=frame.GetComponent<Canvas>();Assert.That(canvas,Is.Not.Null);
                Assert.That(canvas.renderMode,Is.EqualTo(RenderMode.WorldSpace));
                Assert.That(canvas.sortingLayerName,Is.EqualTo("Effects"));Assert.That(canvas.sortingOrder,Is.EqualTo(100));
                foreach(var im in frame.GetComponentsInChildren<Image>())
                {
                    Assert.That(im.raycastTarget,Is.False);Assert.That(im.sprite.texture.filterMode,Is.EqualTo(FilterMode.Point));
                    if(im.type==Image.Type.Tiled)continue;
                    var r=GameplayPixelLayoutController.ScreenRect(im.rectTransform);
                    Assert.That(r.width/im.sprite.rect.width,Is.EqualTo(BattleFloorAnchor.PhysicalPixelScale).Within(.01f),im.name);
                    Assert.That(r.height/im.sprite.rect.height,Is.EqualTo(BattleFloorAnchor.PhysicalPixelScale).Within(.01f),im.name);
                }
            }
            foreach(bool right in new[]{false,true})
            {
                var drill=board.Mine.drills.First(d=>d.fromRight==right);
                // Exercise the actual native projectile renderer at the rim crossing,
                // freezing only this visual fixture so visibility is measured exactly.
                Call(view,"LaunchRow",drill.id,drill.lane);yield return null;
                var shot=board.transform.Find("RowDrillProjectile_"+drill.id).GetComponent<SpriteRenderer>();
                var position=board.GetCellLocalPosition(right?board.Width-1:0,drill.lane);
                position.x+=(right?1:-1)*board.CellSize*.5f;shot.transform.localPosition=position;
                yield return null;
                string path=folder+"/"+label+(right?"-right":"-left");
                yield return ForegroundScreenshot(path+".png");
                shot.enabled=false;yield return ForegroundScreenshot(path+"-hidden.png");
                var visible=new Texture2D(2,2);visible.LoadImage(File.ReadAllBytes(path+".png"));
                var hidden=new Texture2D(2,2);hidden.LoadImage(File.ReadAllBytes(path+"-hidden.png"));
                var rail=(RectTransform)frame.Find("PixelLabBoardFrame/"+(right?"RightEdge":"LeftEdge"));
                var r=GameplayPixelLayoutController.ScreenRect(rail);int changed=0;
                for(int y=Mathf.CeilToInt(r.yMin);y<r.yMax;y++)for(int x=Mathf.CeilToInt(r.xMin);x<r.xMax;x++)
                    if(visible.GetPixel(x,y)!=hidden.GetPixel(x,y))changed++;
                if(right)rec.rightVisiblePixels=changed;else rec.leftVisiblePixels=changed;
                UnityEngine.Object.Destroy(visible);UnityEngine.Object.Destroy(hidden);
                if(verify)Assert.That(changed,Is.GreaterThan(10),label+" projectile must be visibly drawn over "+rail.name);
                Call(view,"ClearRowShots");yield return null;
            }
            records.Add(rec);File.WriteAllText(folder+"/Measurements.json",JsonUtility.ToJson(new DrillForegroundRecords{items=records.ToArray()},true));
            Time.timeScale=1;
        }
    }
    static IEnumerator ForegroundScreenshot(string path)
    {
        path=Path.GetFullPath(path);if(File.Exists(path))File.Delete(path);
        ScreenCapture.CaptureScreenshot(path);yield return Until(()=>File.Exists(path),"foreground screenshot");yield return null;
    }
    [Serializable]class DrillForegroundRecord{public string size;public Rect frame;public int leftVisiblePixels,rightVisiblePixels;}
    [Serializable]class DrillForegroundRecords{public DrillForegroundRecord[] items;}
}
