using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object=UnityEngine.Object;

// Read-only snapshot of current production portrait graphics, using a disposable save profile.
public sealed class DrownedCourtV2Stage04HudCapture
{
    const string Root="ArtSource/DrownedCourtEnvironmentV2";
    [Serializable] public class GraphicRecord { public string path,kind,asset,sprite,text; public Rect rect; public Color color; public int imageType,fillMethod,fillOrigin,alignment,fontStyle; public float fill,ppuMultiplier,fontSize; public bool preserveAspect; }
    [Serializable] public class Snapshot { public Rect portrait; public GraphicRecord[] pieces; public string scope="Read-only production HUD graphics; no scene or production asset saved."; }
    static IEnumerator Until(Func<bool> condition) { float end=Time.realtimeSinceStartup+60; while(!condition()){Assert.That(Time.realtimeSinceStartup,Is.LessThan(end));yield return null;} }
    [UnityTearDown] public IEnumerator Cleanup(){Time.timeScale=1;RunLaunchOptions.StartingZone=null;GameplayPixelLayoutController.ValidationSafeArea=null;if(Application.isPlaying)yield return new ExitPlayMode();}
    [UnityTest,Explicit] public IEnumerator CapturePortraitGraphics()
    {
        typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{new Vector2Int(720,1280)});
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        yield return new EnterPlayMode();
        string profile=Path.GetFullPath(".utmp/CourtV2/"+Guid.NewGuid()+".json");Directory.CreateDirectory(Path.GetDirectoryName(profile));
        File.WriteAllText(profile,JsonUtility.ToJson(new AccountSave{potions=3,bombs=3,equipPotions=true,equipBombs=true}));
        using(CharacterSelectionSettings.UseTemporarySelection("skeleton"))
        using(AccountProgression.UseDisposableProfile(profile))
        {
            RunLaunchOptions.StartingZone="drowned-court";UnityEngine.Random.InitState(3101);SceneManager.LoadScene("Game");yield return null;
            yield return Until(()=>RunSession.Current?.Continuation?.CanCapture==true&&RunSession.Current.Waves.IsWaveActive);
            yield return new WaitForSecondsRealtime(1.5f);Canvas.ForceUpdateCanvases();
            var layout=Object.FindFirstObjectByType<GameplayPixelLayoutController>();var portrait=(RectTransform)layout.TopHud.Find("GeneratedTopBattleLayout/PlayerSection");
            var entries=new List<GraphicRecord>();
            foreach(var g in portrait.GetComponentsInChildren<Graphic>())
            {
                string path=PathOf(g.transform);if(!g.enabled||g.color.a==0||g is TMP_SubMeshUI)continue;
                if(!(path.Contains("/PlayerCharacter")||path.Contains("/PlayerAffinityGem")||path.Contains("/PlayerHPBarBackground")||path.Contains("/PlayerShield")))continue;
                var e=new GraphicRecord{path=path,rect=GameplayPixelLayoutController.ScreenRect(g.rectTransform),color=g.color};
                if(g is Image im){e.kind="image";e.asset=im.sprite?AssetDatabase.GetAssetPath(im.sprite):"";e.sprite=im.sprite?im.sprite.name:"";e.imageType=(int)im.type;e.fill=im.fillAmount;e.fillMethod=(int)im.fillMethod;e.fillOrigin=im.fillOrigin;e.preserveAspect=im.preserveAspect;e.ppuMultiplier=im.pixelsPerUnitMultiplier/im.canvas.scaleFactor;if(im.sprite&&string.IsNullOrEmpty(e.asset))continue;}
                else if(g is TMP_Text text){e.kind="text";e.asset=AssetDatabase.GetAssetPath(text.font);e.text=text.text;e.fontSize=text.fontSize*e.rect.height/text.rectTransform.rect.height;e.alignment=(int)text.alignment;e.fontStyle=(int)text.fontStyle;}else continue;
                entries.Add(e);
            }
            Assert.That(entries.Any(e=>e.path.Contains("/PlayerCharacter")),Is.True);Assert.That(entries.Any(e=>e.path.Contains("/PlayerAffinityGem")),Is.True);Assert.That(entries.Any(e=>e.path.Contains("/PlayerHPBarBackground")),Is.True);
            File.WriteAllText(Root+"/QA/Stage04/PlayerHudSnapshot.json",JsonUtility.ToJson(new Snapshot{portrait=GameplayPixelLayoutController.ScreenRect(portrait),pieces=entries.ToArray()},true));
        }
        yield return new ExitPlayMode();
    }
    static string PathOf(Transform t)=>t.parent?PathOf(t.parent)+"/"+t.name:t.name;
}
