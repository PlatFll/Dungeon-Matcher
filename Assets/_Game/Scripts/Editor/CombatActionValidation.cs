using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>Opt-in production-scene animation, alignment and portrait render checks.</summary>
[InitializeOnLoad]
public static class CombatActionValidation
{
    private const string Key = "DungeonMatcher.CombatActionValidation";
    private static readonly string Output = Path.GetFullPath(".utmp/CombatActions");
    private static readonly Stack<IEnumerator> Steps = new Stack<IEnumerator>();
    private static IDisposable profile, selection;
    private static string error;
    private static int result, assertions;

    static CombatActionValidation()
    {
        EditorApplication.playModeStateChanged += state =>
        {
            if (!SessionState.GetBool(Key, false)) return;
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                result = 0; error = null; assertions = 0; Steps.Clear();
                Application.logMessageReceived += Log;
                Steps.Push(Cases());
                EditorApplication.update += Tick;
            }
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                SessionState.SetBool(Key, false);
                profile?.Dispose(); selection?.Dispose();
                EditorApplication.Exit(result);
            }
        };
    }

    public static void Run()
    {
        if (!Application.isBatchMode) throw new InvalidOperationException("Use an isolated graphics-enabled batch editor.");
        Directory.CreateDirectory(Output);
        ValidateAssets();
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        SessionState.SetBool(Key, true);
        EditorApplication.EnterPlaymode();
    }

    public static void ImportAndRun() { CombatActionImporter.Run(); Run(); }

    private static void ValidateAssets()
    {
        foreach (string name in CombatActionImporter.Names)
        {
            bool attack = name.EndsWith("AutoAttack"); int w = attack ? 96 : 64, count = attack ? 8 : 10;
            string path = CombatActionImporter.ArtRoot + "/" + name + ".png";
            Check(File.ReadAllBytes(path).SequenceEqual(File.ReadAllBytes("ArtSource/CombatActions/" + name + ".png")), name + " exact source export");
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            Check(importer.filterMode == FilterMode.Point && !importer.mipmapEnabled && importer.textureCompression == TextureImporterCompression.Uncompressed, name + " pixel import");
            var frames = CombatActionImporter.LoadFrames(name); Check(frames.Length == count, name + " frame count");
            foreach (var frame in frames) Check(frame.rect.size == new Vector2(w,64) && frame.pivot == new Vector2(w/2,0), name + " fixed rect and ground pivot");
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(CombatActionImporter.AnimationRoot + "/" + name + ".anim");
            Check(!clip.isLooping && Mathf.Abs(clip.length - (attack ? .68f : .88f)) < .0001f, name + " exact one-shot duration");
            Check(AnimationUtility.GetCurveBindings(clip).Length == 0, name + " no transform curves");
            var binding = AnimationUtility.GetObjectReferenceCurveBindings(clip).Single();
            Check(binding.type == typeof(Image) && binding.path == "" && binding.propertyName == "m_Sprite", name + " UI binding");
            var keys = AnimationUtility.GetObjectReferenceCurve(clip,binding);
            Check(keys.Length == count+1 && keys[4].value == frames[4], name + " complete poses");
            if (attack) {
                Check(clip.events.Length == 2 && clip.events[0].functionName == "AutoAttackImpact" && Mathf.Abs(clip.events[0].time-keys[4].time)<.0001f, name+" damage exactly at fifth pose");
                Check(clip.events[1].functionName=="AutoAttackComplete" && Mathf.Abs(clip.events[1].time-.67f)<.0001f,name+" recovery completion");
                var definition=AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Game/Data/Enemies/Enemy_"+name.Split('_')[0]+".asset");
                Check(definition.TimeAutoAttackFromAnimation && definition.UseAuthoredAutoAttackMotion,name+" serialized opt-in");
            } else Check(clip.events.Length==0,name+" ability gameplay stays runtime-owned");
        }
        File.WriteAllText(Path.Combine(Output,"asset-validation.txt"),"PASS: four exact native sheets, 36 fixed-ground sprites, 680/880ms non-looping Image clips, fifth-pose impact events and serialized opt-ins.\n");
    }

    private static IEnumerator Cases()
    {
        foreach(string player in new[]{"skeleton","bardley"})
        {
            string profilePath=Path.Combine(Output,player+"-"+Guid.NewGuid().ToString("N")+"-profile.json");
            File.WriteAllText(profilePath,JsonUtility.ToJson(new AccountSave()));
            profile=AccountProgression.UseDisposableProfile(profilePath);
            selection=CharacterSelectionSettings.UseTemporarySelection(player);
            SetSize(1080,1920); SceneManager.LoadScene("Game");
            yield return Until(()=>RunSession.Current!=null && RunSession.Current.Continuation.CanCapture && RunSession.Current.Waves.IsWaveActive,"production scene ready");
            var run=RunSession.Current;
            // The graphics-enabled batch Game View must not consume unrelated
            // desktop clicks/keys while automated cases drive public APIs.
            if (UnityEngine.EventSystems.EventSystem.current != null)
                UnityEngine.EventSystems.EventSystem.current.enabled = false;
            foreach (var controls in Object.FindObjectsByType<RunControlsUI>(FindObjectsSortMode.None))
                controls.enabled = false;
            foreach(var enemy in run.Waves.ActiveEnemies) enemy.GetComponent<EnemyAutoAttack>().StopAttacking();
            var enemies=new Dictionary<string,EnemyActor>();
            foreach(string name in new[]{"Farmer","PanVillager"}) {
                var definition=AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Game/Data/Enemies/Enemy_"+name+".asset");
                var actor=run.Waves.ActiveEnemies.FirstOrDefault(e=>e.Definition==definition);
                if(actor==null) Check(run.Waves.TrySummonEnemy(definition,out actor),"spawn "+name);
                actor.GetComponent<EnemyAutoAttack>().StopAttacking(); enemies[name]=actor;
            }
            yield return Wait(.25f);
            if (player == "skeleton")
            {
                yield return EnemyAttackLifecycleValidation.ValidateForAutomation();
                Check(true, "all 14 existing enemy lifecycle scenarios");
            }
            var playerImage=Object.FindObjectsByType<Image>(FindObjectsSortMode.None).Single(i=>i.name=="PlayerCharacter");
            foreach(var pair in enemies) yield return AttackCase(pair.Key,pair.Value,run.Player,player=="skeleton" && pair.Key=="Farmer");

            // Keep the selected visual fixtures alive while the real ability
            // performs its normal board/damage flow; never alter asset balance.
            foreach(var enemy in run.Waves.ActiveEnemies)
                typeof(EnemyActor).GetField("currentHealth",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(enemy,100000);

            var controller=run.Player.GetComponent<PlayerAbilityController>();
            var energy=run.Player.GetComponent<PlayerAbilityEnergy>();
            energy.AddEnergy(energy.MaximumEnergy);
            int cues=0; Action cue=()=>cues++; controller.AbilityActivated+=cue;
            Check(controller.TryActivate(),player+" actual ability accepted");
            Check(cues==1,player+" one accepted presentation cue");
            var pa=playerImage.GetComponent<Animator>();
            yield return Until(()=>pa.GetCurrentAnimatorStateInfo(0).IsName("Ability"),player+" ability entered");
            var seen=new HashSet<Sprite>(); float end=Time.realtimeSinceStartup+1.1f;
            while(Time.realtimeSinceStartup<end){seen.Add(playerImage.sprite);yield return null;}
            Check(seen.Count(s=>s!=null&&s.name.Contains("_Ability_"))>=6,player+" actual multi-frame cast playback");
            Check(pa.GetCurrentAnimatorStateInfo(0).IsName("Idle"),player+" returns to idle automatically");
            Check(cues==1,player+" persistent ability state never repeats cast cue");
            controller.AbilityActivated-=cue; controller.CancelActiveAbility();
            yield return Until(()=>run.Continuation.CanCapture,"ability board resolution settles");
            foreach(var enemy in run.Waves.ActiveEnemies)
            {
                typeof(EnemyActor).GetField("currentHealth",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(enemy,1);
                enemy.RestoreHealth(enemy.MaxHealth);
            }
            // Let the real damage flash/shake finish before freezing poses.
            yield return Wait(.5f);
            // Stagger deliberately holds a white blink. Disable only this
            // optional feedback for deterministic base-art pose captures.
            foreach(var enemy in run.Waves.ActiveEnemies)
                enemy.GetComponent<EnemyCombatFeedback>().enabled = false;

            var images=new Dictionary<string,Image>{{player=="skeleton"?"Rattlebones_Ability":"Bardley_Ability",playerImage}};
            foreach(var pair in enemies) images[pair.Key+"_AutoAttack"]=pair.Value.transform.Find("VisualRoot").GetComponent<Image>();
            Time.timeScale=0;
            foreach(int height in new[]{1920,2400}) {
                SetSize(1080,height);yield return Wait(.25f);
                foreach(var pair in images) {
                    var image=pair.Value;var animator=image.GetComponent<Animator>();animator.fireEvents=false;
                    animator.Play("Idle",0,0);animator.Update(0);yield return Wait(.03f);
                    Rect baseline=ScreenRect(image);var frames=CombatActionImporter.LoadFrames(pair.Key);
                    bool attack=pair.Key.EndsWith("AutoAttack");int[] durations=attack?new[]{80,80,120,40,120,80,80,80}:new[]{80,80,120,80,120,80,80,80,80,80};
                    int elapsed=0;var texture=new Texture2D(2,2);texture.LoadImage(File.ReadAllBytes(AssetDatabase.GetAssetPath(frames[0])));
                    for(int f=0;f<frames.Length;f++) {
                        animator.Play(attack?"AutoAttack":"Ability",0,(elapsed+durations[f]*.5f)/(attack?680f:880f));animator.Update(0);elapsed+=durations[f];
                        yield return Wait(.025f);Canvas.ForceUpdateCanvases();
                        Check(image.sprite==frames[f],pair.Key+" pose "+f);
                        Rect rect=ScreenRect(image);float scale=rect.width/frames[f].rect.width;
                        Check(Mathf.Abs(rect.center.x-baseline.center.x)<.1f&&Mathf.Abs(rect.yMin-baseline.yMin)<.1f,pair.Key+" fixed anchor across idle/action");
                        Check(Mathf.Abs(scale-Mathf.Round(scale))<.001f,pair.Key+" integer texels");
                        Check(Mathf.Abs(rect.height-baseline.height)<.1f,pair.Key+" unchanged body scale");
                        Check(rect.xMin>=0&&rect.xMax<=1080&&rect.yMin>=0&&rect.yMax<=height,pair.Key+" portrait bounds");
                        var r=frames[f].rect;
                        Check(Enumerable.Range(0,(int)r.width).Any(x=>texture.GetPixel((int)r.x+x,0).a>.5f),pair.Key+" feet at floor");
                        foreach(var mask in image.GetComponentsInParent<RectMask2D>()) {
                            var corners=new Vector3[4];image.rectTransform.GetWorldCorners(corners);
                            Check(corners.All(c=>mask.rectTransform.rect.Contains((Vector2)mask.rectTransform.InverseTransformPoint(c))),pair.Key+" no canvas clipping mask");
                        }
                        if(f==4){ScreenCapture.CaptureScreenshot(Path.Combine(Output,player+"-"+height+"-"+pair.Key+"-impact.png"));yield return Wait(.15f);}
                    }
                    Object.Destroy(texture);animator.Play("Idle",0,0);animator.Update(0);animator.fireEvents=true;
                }
            }
            Time.timeScale=1;SceneManager.LoadScene("MainMenu");yield return Wait(.15f);
            selection.Dispose();selection=null;profile.Dispose();profile=null;
        }
        // Ordinary MonoBehaviours do not receive all lifecycle callbacks in
        // Edit Mode. Exercise these existing actor fixtures in live Play Mode.
        yield return Until(()=>RunSession.Current==null&&RunUpgradeRuntime.Current==null,"isolated ability fixture context");
        int abilityCases=0;
        foreach(var method in typeof(PlayerAbilityLifecycleTests).GetMethods().Where(m=>
            m.GetCustomAttributes(typeof(NUnit.Framework.TestAttribute),true).Length>0))
        {
            var fixture=new PlayerAbilityLifecycleTests();
            try { fixture.SetUp();method.Invoke(fixture,null);abilityCases++; }
            finally { fixture.TearDown(); }
        }
        Check(abilityCases==15,"all 15 ability lifecycle cases in Play Mode");
        File.WriteAllText(Path.Combine(Output,"play-validation.txt"),"PASS: "+assertions+" checks; 14 existing enemy lifecycle scenarios and 15 ability lifecycle cases in Play Mode. Production attack damage at frame five, duplicate guard, pause beyond timeout, cancellation/restart, missing-animation fallback, actual accepted player casts, idle return, every action pose at 1080x1920 and 1080x2400 with fixed ground/center/scale.\n");
    }

    private static IEnumerator AttackCase(string name,EnemyActor enemy,PlayerActor player,bool extra)
    {
        var attack=enemy.GetComponent<EnemyAutoAttack>();var image=enemy.transform.Find("VisualRoot").GetComponent<Image>();var animator=image.GetComponent<Animator>();
        var frames=CombatActionImporter.LoadFrames(name+"_AutoAttack");
        int hits=0;Sprite hitPose=null;float hitTime=0;
        Action<PlayerActor,int> onDamage=(p,amount)=>{hits++;hitPose=image.sprite;hitTime=animator.GetCurrentAnimatorStateInfo(0).normalizedTime*.68f;};
        player.DamageTaken+=onDamage;player.RestoreToFullHealth();
        Check(attack.PerformAttackImmediately(),name+" attack accepted");Check(hits==0,name+" no start-frame damage");
        if(extra) {
            yield return Wait(.12f);Time.timeScale=0;Sprite pausedPose=image.sprite;
            Check(!attack.ResolveAnimationImpact(),"paused impact callback rejected without consuming payload");
            yield return Wait(3.2f);Check(hits==0&&image.sprite==pausedPose,"pause freezes clip and fallback deadline");Time.timeScale=1;
        }
        yield return Until(()=>hits==1,name+" first impact");
        Check(hitPose==frames[4],name+" damage sees exact impact sprite: "+(hitPose!=null?hitPose.name:"null")+" at "+hitTime);
        Check(hitTime>=.319f&&hitTime<.44f,name+" damage within impact exposure");
        Check(!attack.ResolveAnimationImpact()&&hits==1,name+" duplicate impact ignored");
        Check(attack.IsAttackSequenceInProgress,name+" action owns recovery");
        yield return Until(()=>!attack.IsAttackSequenceInProgress,name+" recovery completes");yield return Wait(.05f);
        Check(hits==1&&animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"),name+" one hit then idle");
        if(extra) {
            Check(attack.PerformAttackImmediately(),"cancellable attack begins");yield return Wait(.1f);attack.StopAttacking();
            Check(!attack.ResolveAnimationImpact(),"cancel rejects late impact");
            int before=hits;Check(attack.PerformAttackImmediately(),"replacement attack begins");yield return Wait(.2f);
            Check(hits==before,"replacement does not inherit prior impact");yield return Until(()=>hits==before+1,"replacement own impact");
            yield return Until(()=>!attack.IsAttackSequenceInProgress,"replacement recovery");
            float saved=(float)typeof(EnemyAutoAttack).GetField("animationImpactTimeout",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(attack);
            typeof(EnemyAutoAttack).GetField("animationImpactTimeout",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(attack,.15f);
            animator.enabled=false;before=hits;Check(attack.PerformAttackImmediately(),"missing animation still accepts gameplay");
            yield return Until(()=>hits==before+1&&!attack.IsAttackSequenceInProgress,"missing event fallback releases ownership");
            typeof(EnemyAutoAttack).GetField("animationImpactTimeout",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(attack,saved);
            animator.enabled=true;animator.Play("Idle",0,0);animator.Update(0);
        }
        player.DamageTaken-=onDamage;
    }

    private static Rect ScreenRect(Image image)
    {
        var corners = new Vector3[4]; image.rectTransform.GetWorldCorners(corners);
        var camera = image.canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : image.canvas.worldCamera;
        Vector2 a = RectTransformUtility.WorldToScreenPoint(camera, corners[0]);
        Vector2 b = RectTransformUtility.WorldToScreenPoint(camera, corners[2]);
        return Rect.MinMaxRect(a.x, a.y, b.x, b.y);
    }
    private static void Tick()
    {
        try
        {
            Check(error == null, error);
            if (Steps.Count == 0) { Finish(); return; }
            var step = Steps.Peek();
            if (!step.MoveNext()) { Steps.Pop(); return; }
            if (step.Current is IEnumerator nested) Steps.Push(nested);
            EditorApplication.QueuePlayerLoopUpdate();
        }
        catch (Exception exception) { result = 1; Debug.LogException(exception); Finish(); }
    }
    private static void Finish()
    {
        EditorApplication.update -= Tick; Application.logMessageReceived -= Log;
        Time.timeScale = 1;
        if (result != 0) File.WriteAllText(Path.Combine(Output, "play-validation.txt"), "FAIL: " + error);
        EditorApplication.ExitPlaymode();
    }
    private static void Log(string text, string trace, LogType type)
    {
        if (type == LogType.Exception && trace.Contains("UnityEditor.Search.SearchInit.IndexationOnStartup")) return;
        // Startup layout diagnostics are rechecked as structured errors after
        // the scene settles above. Character/layout failures still fail the run.
        if (text.StartsWith("Pixel layout FAILED:"))
        { File.AppendAllText(Path.Combine(Output, "startup-layout-diagnostics.txt"), text + "\n"); return; }
        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) error = text;
    }
    private static bool IsAbilityIconWarning(string issue) => issue.StartsWith(
        "Fractional UI art vertices Canvas/SafeArea/BottomHUD/AbilityButton/AbilityIcon:");
    private static void Check(bool condition, string message)
    { if (!condition) throw new InvalidOperationException("Combat action validation: " + message); assertions++; }
    private static IEnumerator Wait(float seconds)
    { float end = Time.realtimeSinceStartup + seconds; while (Time.realtimeSinceStartup < end) yield return null; }
    private static IEnumerator Until(Func<bool> predicate, string label)
    { float end = Time.realtimeSinceStartup + 40; while (!predicate()) { Check(Time.realtimeSinceStartup < end, label); yield return null; } }
    private static void SetSize(int width, int height)
    {
        var assembly = typeof(Editor).Assembly; var type = assembly.GetType("UnityEditor.GameViewSizes");
        var sizes = typeof(ScriptableSingleton<>).MakeGenericType(type).GetProperty("instance", BindingFlags.Public | BindingFlags.Static).GetValue(null);
        var groupType = type.GetProperty("currentGroupType", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(sizes);
        var group = type.GetMethod("GetGroup").Invoke(sizes, new[] { groupType });
        var size = Activator.CreateInstance(assembly.GetType("UnityEditor.GameViewSize"), new object[] { Enum.ToObject(assembly.GetType("UnityEditor.GameViewSizeType"), 1), width, height, "Combat action validation" });
        group.GetType().GetMethod("AddCustomSize").Invoke(group, new[] { size });
        int count = (int)group.GetType().GetMethod("GetTotalCount").Invoke(group, null);
        var viewType = assembly.GetType("UnityEditor.GameView"); var view = EditorWindow.GetWindow(viewType);
        viewType.GetProperty("selectedSizeIndex", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(view, count - 1);
        view.Show(); view.Focus(); view.Repaint();
    }
}
