using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed partial class ForestFoundationPlayTests
{
    [UnityTest,Explicit]public IEnumerator IronveinProductionDrillMotionPreview()
    {
        yield return LaunchMineKit("pickaxe_delver","rivet_gunner","stonewright");QuietKitFixture();
        typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{new Vector2Int(720,1280)});
        yield return Until(()=>Screen.width==720&&Screen.height==1280,"production motion viewport");
        for(int i=0;i<12;i++)yield return null;
        Assert.That(SceneManager.sceneCount,Is.EqualTo(1));Assert.That(SceneManager.GetActiveScene().name,Is.EqualTo("Game"));
        Assert.That(GameObject.Find("ApprovedIronveinEnvironment"),Is.Not.Null);
        string root="ArtSource/IronveinBelowTopHUD/Integration/Motion";Directory.CreateDirectory(root);
        var b=Run.Board;float old=Time.captureDeltaTime;var times=new List<float>();
        try
        {
            Time.captureDeltaTime=1f/30f;int frame=0;float start=Time.time;
            b.TryQueueMineDrillPower(1,4);b.TryQueueMineDrillPower(2,4);
            do
            {
                string png=Path.GetFullPath(root+"/"+(frame++).ToString("D4")+".png");if(File.Exists(png))File.Delete(png);
                times.Add(Time.time-start);ScreenCapture.CaptureScreenshot(png);yield return Until(()=>File.Exists(png),"production screenshot");yield return null;
                Assert.That(frame,Is.LessThan(600));
            }while(b.IsBusy||frame<6);
            File.WriteAllText(root+"/Evidence.json",JsonUtility.ToJson(new ProductionDrillEvidence{frames=frame,seconds=times.ToArray()},true));
        }finally{Time.captureDeltaTime=old;}
    }
    [Serializable]class ProductionDrillEvidence{public int frames;public float[] seconds;public string scene="Game",note="Actual production Game, live actors/controls/gems and canonical two-shot drill batch; no additive art scene or substituted board.";}
}
