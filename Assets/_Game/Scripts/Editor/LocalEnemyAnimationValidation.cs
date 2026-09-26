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
public static class LocalEnemyAnimationValidation
{
    private const string Key = "DungeonMatcher.LocalEnemyAnimationValidation";
    private static readonly string Output = Path.GetFullPath(".utmp/LocalEnemies");
    private static readonly Stack<IEnumerator> Steps = new Stack<IEnumerator>();
    private static IDisposable profile, selection;
    private static string error;
    private static int result, assertions;

    static LocalEnemyAnimationValidation()
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

    public static void Run() => Start(false);
    public static void RunAlignmentOnly() => Start(true);

    private static void Start(bool alignmentOnly)
    {
        if (!Application.isBatchMode) throw new InvalidOperationException("Use an isolated graphics-enabled batch editor.");
        Directory.CreateDirectory(Output);
        File.WriteAllText(Path.Combine(Output, "ground-coordinates.txt"), "");
        SessionState.SetBool(Key + ".alignmentOnly", alignmentOnly);
        ValidateAssets();
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        SessionState.SetBool(Key, true);
        EditorApplication.EnterPlaymode();
    }

    public static void ImportAndRun() { CombatActionImporter.ImportLocalEnemies(); Run(); }

    private static readonly string[] Characters = { "Miner", "BasketVillager", "BarricadeVillager" };

    private static void ValidateAssets()
    {
        foreach (string character in Characters)
        foreach (string action in new[] { "Idle", "AutoAttack", "Ability" })
        {
            if (character == "BasketVillager" && action == "Ability") continue;
            string name = character + "_" + action;
            bool idle = action == "Idle", attack = action == "AutoAttack";
            int w = character == "Miner" || !idle ? 96 : 64, h = character == "Miner" ? 80 : 64;
            int count = idle ? 9 : attack ? 8 : 10;
            string art = idle ? CombatIdleImporter.ArtRoot : CombatActionImporter.ArtRoot;
            string animations = idle ? CombatIdleImporter.AnimationRoot : CombatActionImporter.AnimationRoot;
            string path = art + "/" + name + ".png";
            Check(File.ReadAllBytes(path).SequenceEqual(File.ReadAllBytes("ArtSource/LocalEnemies/" + name + ".png")), name + " exact native PNG");
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            Check(importer.filterMode == FilterMode.Point && !importer.mipmapEnabled && importer.textureCompression == TextureImporterCompression.Uncompressed, name + " pixel import");
            var frames = idle ? CombatIdleImporter.LoadFrames(character) : CombatActionImporter.LoadFrames(name);
            Check(frames.Length == count, name + " complete frames");
            foreach (var sprite in frames) Check(sprite.rect.size == new Vector2(w,h) && sprite.pivot == new Vector2(w/2,0), name + " fixed bottom-center pivot");
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(animations + "/" + name + ".anim");
            Check(clip.isLooping == idle && Mathf.Abs(clip.length - (idle ? 1.17f : attack ? .68f : .88f)) < .0001f, name + " exact duration");
            Check(AnimationUtility.GetCurveBindings(clip).Length == 0, name + " no transform animation");
            if (!idle)
            {
                var keys = AnimationUtility.GetObjectReferenceCurve(clip, AnimationUtility.GetObjectReferenceCurveBindings(clip).Single());
                Check(keys[4].value == frames[4] && clip.events.Length == 2, name + " contact pose and events");
                Check(clip.events[0].functionName == (attack ? "AutoAttackImpact" : "AbilityImpact") && Mathf.Abs(clip.events[0].time-keys[4].time)<.0001f, name + " impact on frame five");
                Check(clip.events[1].functionName == (attack ? "AutoAttackComplete" : "AbilityComplete"), name + " completion event");
            }
            var definition = AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Game/Data/Enemies/Enemy_"+character+".asset");
            Check(definition.UseAuthoredAutoAttackMotion, character+" authored attack opt-in");
            if (action == "Ability") Check(definition.UseAuthoredSpecialAbilityMotion, character+" authored ability opt-in");
        }
        File.WriteAllText(Path.Combine(Output,"asset-validation.txt"),"PASS: all eight native exports, 71 fixed-canvas sprites, palette-safe import settings, clip lengths, frame-five impact events and serialized opt-ins.\n");
    }

    private static IEnumerator Cases()
    {
        Application.runInBackground = true;
        bool alignmentOnly = SessionState.GetBool(Key + ".alignmentOnly", false);
        string profilePath = Path.Combine(Output, "test-"+Guid.NewGuid().ToString("N")+"-profile.json");
        File.WriteAllText(profilePath,JsonUtility.ToJson(new AccountSave()));
        profile=AccountProgression.UseDisposableProfile(profilePath);
        selection=CharacterSelectionSettings.UseTemporarySelection("skeleton");
        SetSize(1080,1920); SceneManager.LoadScene("Game");
        yield return Until(()=>RunSession.Current!=null && RunSession.Current.Continuation.CanCapture && RunSession.Current.Waves.IsWaveActive,"production scene ready");
        var run=RunSession.Current;
        if (UnityEngine.EventSystems.EventSystem.current != null) UnityEngine.EventSystems.EventSystem.current.enabled=false;
        foreach(var controls in Object.FindObjectsByType<RunControlsUI>(FindObjectsSortMode.None))
        { controls.Close(); controls.enabled=false; }
        var initial=run.Waves.ActiveEnemies.ToArray();
        foreach(var enemy in initial) enemy.GetComponent<EnemyAutoAttack>().StopAttacking();
        var actors=new Dictionary<string,EnemyActor>();
        foreach(string character in Characters)
        {
            yield return Until(()=>run.Waves.HasFreeEnemySlot,"free production slot for "+character);
            var definition=AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Game/Data/Enemies/Enemy_"+character+".asset");
            Check(run.Waves.TrySummonEnemy(definition,out EnemyActor actor),"actual spawn "+character);
            actor.GetComponent<EnemyAutoAttack>().StopAttacking(); actors[character]=actor;
            if(character=="Miner") foreach(var old in initial) old.TryTakeDamageWithoutFeedback(100000);
        }
        yield return Wait(.5f);
        if (!alignmentOnly)
        {
            yield return EnemyAttackLifecycleValidation.ValidateForAutomation();
            foreach(var pair in actors)
                yield return CombatActionValidation.AttackCase(pair.Key,pair.Value,run.Player,pair.Key=="Miner");
            foreach(string character in new[]{"Miner","BarricadeVillager"})
                yield return AbilityCase(character,actors[character],run.Board);
        }

        foreach(var actor in actors.Values) actor.GetComponent<EnemyCombatFeedback>().enabled=false;
        foreach(var controls in Object.FindObjectsByType<RunControlsUI>(FindObjectsSortMode.None)) controls.Close();
        yield return Wait(.1f);
        Time.timeScale=0;
        foreach(int height in new[]{1920,2400})
        {
            SetSize(1080,height); yield return Wait(.25f);
            foreach(var pair in actors)
                yield return AllPoses(pair.Key,pair.Value,height);
            var floors = actors.Values.Select(a => ScreenRect(a.transform.Find("VisualRoot").GetComponent<Image>()).yMin).ToArray();
            File.AppendAllText(Path.Combine(Output,"ground-coordinates.txt"),height+": "+string.Join("; ",actors.Select(p=>p.Key+" "+ScreenRect(p.Value.transform.Find("VisualRoot").GetComponent<Image>())+" root "+((RectTransform)p.Value.transform).anchoredPosition+" visual "+p.Value.transform.Find("VisualRoot").localPosition))+"\n");
            Check(floors.Max()-floors.Min()<.1f,"all three characters share one floor at "+height+": "+string.Join(", ",floors));
            ScreenCapture.CaptureScreenshot(Path.Combine(Output,"family-"+height+".png"));
            yield return Wait(.2f);
        }
        Time.timeScale=1;
        // Cancel a queued contact by disabling its owner; re-enable must not
        // inherit an action lock or create a delayed structure.
        foreach(string character in alignmentOnly ? Array.Empty<string>() : new[]{"Miner","BarricadeVillager"})
        {
            var actor=actors[character]; int before=Owned(character,actor,run.Board);
            Charge(actor); yield return Wait(.1f); actor.gameObject.SetActive(false);
            yield return Until(()=>!run.Board.IsBusy,"cancelled board request settles");
            actor.gameObject.SetActive(true); yield return Wait(.5f);
            Check(Owned(character,actor,run.Board)==before && !actor.IsSpecialAbilityAnimationActionActive,character+" disable cancels contact and releases action");
            actor.transform.Find("VisualRoot").GetComponent<Animator>().Play("Idle",0,0);
        }
        if (!alignmentOnly)
            yield return ConcurrentAbilities(actors["Miner"], actors["BarricadeVillager"], run.Board);
        SceneManager.LoadScene("MainMenu"); yield return Wait(.2f);
        selection.Dispose(); selection=null; profile.Dispose(); profile=null;
        File.WriteAllText(Path.Combine(Output,alignmentOnly ? "alignment-validation.txt" : "play-validation.txt"), alignmentOnly
            ? "PASS: all 71 poses at both portrait sizes; fixed center, shared floor, integer texels and no canvas clipping.\n"
            : "PASS: actual damage and board effects at frame five, simultaneous-ready abilities, pause and missing-event fallback, recovery ownership, cancellation, 14 existing attack lifecycle scenarios, eight clips/all 71 poses at both portrait sizes with fixed center/shared ground, health-bar clearance and integer texels.\n");
    }

    private static IEnumerator ConcurrentAbilities(EnemyActor miner, EnemyActor builder, BoardController board)
    {
        int mines = Owned("Miner", miner, board), barricades = Owned("BarricadeVillager", builder, board);
        Charge(miner); Charge(builder);
        Check(miner.IsSpecialAbilityAnimationActionActive && !builder.IsSpecialAbilityAnimationActionActive,
            "second ready ability retains its charge until the board settles");
        yield return Until(() => Owned("Miner", miner, board) == mines + 1, "first concurrent mine contact");
        Check(miner.transform.Find("VisualRoot").GetComponent<Image>().sprite == CombatActionImporter.LoadFrames("Miner_Ability")[4],
            "first concurrent effect matches contact pose");
        yield return Until(() => Owned("BarricadeVillager", builder, board) == barricades + 1, "deferred build contact");
        Check(builder.transform.Find("VisualRoot").GetComponent<Image>().sprite == CombatActionImporter.LoadFrames("BarricadeVillager_Ability")[4],
            "deferred effect matches its own contact pose");
        yield return Until(() => !board.IsBusy && !miner.IsSpecialAbilityAnimationActionActive && !builder.IsSpecialAbilityAnimationActionActive,
            "concurrent abilities finish without leaked ownership");
        Check(Owned("Miner", miner, board) == mines + 1 && Owned("BarricadeVillager", builder, board) == barricades + 1,
            "concurrent abilities resolve exactly once each");
    }

    private static int Owned(string character,EnemyActor actor,BoardController board) => character=="Miner"
        ? board.GetMinedCellCountForOwner(actor.GetInstanceID()) : board.GetBarricadeCountForOwner(actor.GetInstanceID());

    private static void Charge(EnemyActor actor)
    {
        actor.ResetSpecialCounter();
        for(int i=0;i<actor.SpecialTurnRequirement;i++) actor.RegisterValidPlayerTurn();
    }

    private static IEnumerator AbilityCase(string character,EnemyActor actor,BoardController board)
    {
        var image=actor.transform.Find("VisualRoot").GetComponent<Image>();
        var animator=image.GetComponent<Animator>(); var playback=image.GetComponent<CharacterAnimationPlayback>();
        var frames=CombatActionImporter.LoadFrames(character+"_Ability");
        int before=Owned(character,actor,board), miningEvents=0; Sprite miningPose=null;
        Action<int,int,float> mining=(x,y,t)=>{miningEvents++;miningPose=image.sprite;};
        board.CellMiningStarted+=mining;
        Charge(actor);
        yield return Until(()=>actor.IsSpecialAbilityAnimationActionActive,character+" accepted ability");
        Check(Owned(character,actor,board)==before,character+" no start-frame structure");
        yield return Wait(.1f);Time.timeScale=0;Sprite paused=image.sprite;
        playback.AbilityImpact();
        yield return Wait(3.2f);
        Check(image.sprite==paused && Owned(character,actor,board)==before,character+" pause freezes impact and fallback");
        Time.timeScale=1;
        yield return Until(()=>Owned(character,actor,board)==before+1,character+" contact commits structure");
        Check(image.sprite==frames[4],character+" board mutation sees impact pose: "+image.sprite.name);
        if(character=="Miner") Check(miningEvents==1 && miningPose==frames[4],"mine flash begins at contact");
        Check(actor.IsSpecialAbilityAnimationActionActive,character+" recovery remains owned");
        Check(!actor.GetComponent<EnemyAutoAttack>().PerformAttackImmediately(),character+" auto attack cannot interrupt recovery");
        playback.AbilityImpact(); yield return Wait(.04f);
        Check(Owned(character,actor,board)==before+1,character+" duplicate event cannot place twice");
        yield return Until(()=>!actor.IsSpecialAbilityAnimationActionActive && !board.IsBusy,character+" ability completes and board settles");
        yield return Wait(.1f);Check(animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"),character+" returns to idle");
        board.CellMiningStarted-=mining;
        animator.enabled=false;before=Owned(character,actor,board);Charge(actor);
        yield return Until(()=>Owned(character,actor,board)==before+1 && !actor.IsSpecialAbilityAnimationActionActive && !board.IsBusy,character+" missing clip fallback");
        animator.enabled=true;animator.Play("Idle",0,0);animator.Update(0);
    }

    private static IEnumerator AllPoses(string character,EnemyActor actor,int height)
    {
        var image=actor.transform.Find("VisualRoot").GetComponent<Image>();var animator=image.GetComponent<Animator>();
        animator.fireEvents=false;animator.Play("Idle",0,0);animator.Update(0);yield return Wait(.04f);
        Rect baseline=ScreenRect(image);float baseTexels=baseline.width/image.sprite.rect.width;
        foreach(string action in new[]{"Idle","AutoAttack","Ability"})
        {
            if(character=="BasketVillager" && action=="Ability") continue;
            string name=character+"_"+action;bool idle=action=="Idle",attack=action=="AutoAttack";
            var frames=idle?CombatIdleImporter.LoadFrames(character):CombatActionImporter.LoadFrames(name);
            int[] ms=idle?Enumerable.Repeat(130,9).ToArray():attack?new[]{80,80,120,40,120,80,80,80}:new[]{80,80,120,80,120,80,80,80,80,80};
            int elapsed=0;float length=ms.Sum();
            var texture=new Texture2D(2,2);texture.LoadImage(File.ReadAllBytes(AssetDatabase.GetAssetPath(frames[0])));
            for(int f=0;f<frames.Length;f++)
            {
                animator.Play(action,0,(elapsed+ms[f]*.5f)/length);animator.Update(0);elapsed+=ms[f];
                yield return Wait(.025f);Canvas.ForceUpdateCanvases();
                Check(image.sprite==frames[f],name+" frame "+f);
                Rect rect=ScreenRect(image);float scale=rect.width/frames[f].rect.width;
                Check(Mathf.Abs(rect.center.x-baseline.center.x)<.1f && Mathf.Abs(rect.yMin-baseline.yMin)<.1f,name+" fixed center and floor");
                Check(Mathf.Abs(scale-Mathf.Round(scale))<.001f && Mathf.Abs(scale-baseTexels)<.001f,name+" integer texels and unchanged body scale");
                Check(rect.xMin>=0 && rect.xMax<=1080 && rect.yMin>=0 && rect.yMax<=height,name+" portrait containment");
                var health = actor.GetComponentInParent<EnemySlotUI>().transform.Find("EnemyHPBarBackground").GetComponent<Image>();
                Check(rect.yMin > ScreenRect(health).yMax, name+" feet clear the health bar");
                var r=frames[f].rect;
                bool floor=Enumerable.Range(0,(int)r.width).Any(x=>texture.GetPixel((int)r.x+x,0).a>.5f);
                Check(floor || (name=="Miner_Ability" && f==3),name+" ground contact or deliberate mining leap");
                foreach(var mask in image.GetComponentsInParent<RectMask2D>())
                {
                    var corners=new Vector3[4];image.rectTransform.GetWorldCorners(corners);
                    Check(corners.All(c=>mask.rectTransform.rect.Contains((Vector2)mask.rectTransform.InverseTransformPoint(c))),name+" no clipping mask");
                }
                if(f==4 && !idle){ScreenCapture.CaptureScreenshot(Path.Combine(Output,height+"-"+name+"-impact.png"));yield return Wait(.1f);}
            }
            Object.Destroy(texture);
        }
        animator.Play("Idle",0,0);animator.Update(0);animator.fireEvents=true;
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
        if (result != 0) File.WriteAllText(Path.Combine(Output, SessionState.GetBool(Key + ".alignmentOnly", false) ? "alignment-validation.txt" : "play-validation.txt"), "FAIL: " + error);
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
    { if (!condition) throw new InvalidOperationException("Local enemy animation validation: " + message); assertions++; }
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
        var size = Activator.CreateInstance(assembly.GetType("UnityEditor.GameViewSize"), new object[] { Enum.ToObject(assembly.GetType("UnityEditor.GameViewSizeType"), 1), width, height, "Local enemy animation validation" });
        group.GetType().GetMethod("AddCustomSize").Invoke(group, new[] { size });
        int count = (int)group.GetType().GetMethod("GetTotalCount").Invoke(group, null);
        var viewType = assembly.GetType("UnityEditor.GameView"); var view = EditorWindow.GetWindow(viewType);
        viewType.GetProperty("selectedSizeIndex", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(view, count - 1);
        view.Show(); view.Focus(); view.Repaint();
    }
}
