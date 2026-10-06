using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object=UnityEngine.Object;

public sealed class EnemyAbilityPlayTests
{
    private const BindingFlags Flags=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
    private RunSession run;
    private BoardController board;
    private EnemyActor owner,ally;
    private string output;
    private readonly List<string> errors=new List<string>();
    private void Log(string message,string trace,LogType type)
    {
        if(type!=LogType.Error&&type!=LogType.Exception&&type!=LogType.Assert)return;
        if(message.StartsWith("Pixel layout FAILED:")) File.AppendAllText(Path.Combine(output,"resize-diagnostics.txt"),message+"\n");
        else errors.Add(message+"\n"+trace);
    }
    [UnityTearDown] public IEnumerator Cleanup()
    {
        Application.logMessageReceived-=Log;LogAssert.ignoreFailingMessages=false;Time.timeScale=1;
        if(Application.isPlaying)yield return new ExitPlayMode();
    }
    [UnityTest] public IEnumerator ApprovedAbilitiesApplyAtContactAndBombardmentRefillsAfterBothStrikes()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        yield return new EnterPlayMode();
        output=Path.GetFullPath(".utmp/EnemyAbilitiesReview");Directory.CreateDirectory(output);
        errors.Clear();Application.logMessageReceived+=Log;LogAssert.ignoreFailingMessages=true;
        using(AccountProgression.UseDisposableProfile(Path.Combine(output,Guid.NewGuid()+".json")))
        using(CharacterSelectionSettings.UseTemporarySelection("skeleton"))
        {
            Resize(1920);yield return new WaitForSecondsRealtime(.3f);SceneManager.LoadScene("Game");
            yield return Until(()=>RunSession.Current!=null&&RunSession.Current.Continuation.CanCapture,"Game ready");
            run=RunSession.Current;board=Object.FindFirstObjectByType<BoardController>();
            Set(run.Waves,"advanceWavesAutomatically",false);Set(run.Player,"maximumHealth",100000);Set(run.Player,"currentHealth",100000);
            if(UnityEngine.EventSystems.EventSystem.current!=null)UnityEngine.EventSystems.EventSystem.current.enabled=false;
            foreach(var ui in Object.FindObjectsByType<RunControlsUI>(FindObjectsSortMode.None)){ui.Close();ui.enabled=false;}
            foreach(string name in new[]{"CourtMage","CrossbowGuard","RoyalStandardBearer","ShieldKnight","TownMarshal","BarricadeGuard","SiegeSergeant","RoyalArchbishop","KnightCaptain"})
            {
                yield return Spawn(name);
                var castNames=new List<string>();owner.AbilityCastCommitted+=(a,n)=>castNames.Add(n);
                if(name=="ShieldKnight")yield return ReviewShieldClearance();
                int ownerId=owner.GetInstanceID();bool signal=false;Sprite contact=null;
                var image=owner.transform.Find("VisualRoot").GetComponent<Image>();
                Action<EnemyActor,int> shield=(a,n)=>{signal=true;contact=image.sprite;};
                if(name=="ShieldKnight")ally.ShieldGranted+=shield;
                if(name=="RoyalArchbishop")board.GemSetMarked+=Mark;
                Func<bool> effect=()=> name=="CourtMage"?((HashSet<Gem>)Get(board,"frozenPinnedGems")).Count>0:
                    name=="CrossbowGuard"||name=="KnightCaptain"?board.GetPinnedGemCountForOwner(ownerId)>0:
                    name=="RoyalStandardBearer"?board.GetRoyalBannerCountForOwner(ownerId)>0:
                    name=="ShieldKnight"||name=="RoyalArchbishop"?signal:
                    name=="TownMarshal"?run.Waves.ActiveEnemies.Count>2:board.GetBarricadeCountForOwner(ownerId)>0;
                float started=Time.time;Prime(owner);Assert.That(effect(),Is.False,name+" must wind up before applying");
                Assert.That(castNames,Is.Empty,name+" readiness and windup must remain silent");
                if(name=="ShieldKnight")
                {
                    yield return new WaitForSeconds(.1f);Time.timeScale=0;var frozen=image.sprite;
                    yield return new WaitForSecondsRealtime(.4f);Assert.That(effect(),Is.False);Assert.That(image.sprite,Is.SameAs(frozen));Time.timeScale=1;
                }
                yield return Until(effect,name+" effect");Assert.That(Time.time-started,Is.GreaterThan(.08f),name+" contact delay");
                if(name=="ShieldKnight")
                {
                    var selection=EnemyAbilityMotionTests.ReadEntries().Single(e=>e.name=="ShieldKnight_Ability");
                    Assert.That(contact,Is.SameAs(CombatActionImporter.LoadFrames(selection.name)[selection.impactFrames[0]]),"shield granted on blue pulse");
                    ally.ShieldGranted-=shield;
                }
                if(name=="RoyalArchbishop")board.GemSetMarked-=Mark;
                yield return Until(()=>!owner.HasAnimationActionInProgress&&!board.IsBusy,name+" recovery");
                Assert.That(castNames.Count,Is.EqualTo(1),name+" announces a successful cast exactly once");
                Assert.That(CombatGuide.Enemy(owner),Does.Contain(castNames[0]),name+" cast name is also in inspection");
                Assert.That(owner.IsSpecialReady,Is.False,name+" cadence consumed once");
                foreach(var enemy in run.Waves.ActiveEnemies)enemy.GetComponent<EnemyAutoAttack>().StopAttacking();
                if(name=="KnightCaptain")
                {
                    int hits=0;owner.GetComponent<EnemyAutoAttack>().AttackResolved+=(a,d,applied)=>hits++;
                    Set(owner.GetComponent<KnightCaptainEnemyAbility>(),"preferChains",false);Prime(owner);
                    yield return Until(()=>hits>0&&!owner.HasAnimationActionInProgress,"Captain commanded attack");
                }
                if(name=="RoyalArchbishop")
                {
                    var bishop=owner.GetComponent<RoyalArchbishopEnemyAbility>();
                    var runes=(BoardController.GemSetThreat)Get(bishop,"runes");
                    // Rune clears can legitimately cascade into player damage.
                    // Keep the healing/blessing recipient alive through that refill.
                    var stats=ally.RuntimeStats;
                    SetProperty(ally,"RuntimeStats",new EnemyRuntimeStats(stats.Wave,stats.Level,30000,stats.Damage,stats.FollowUpDamage,stats.AttackInterval,stats.SpecialTurnRequirement,stats.DamageMultiplier));
                    Set(ally,"currentHealth",30000);ally.TryTakeDamageWithoutFeedback(10000);
                    int survivingRunes=runes.Targets.Count(g=>board.IsEnvironmentalOrdinaryGem(g));
                    var heals=new List<int>();Action<EnemyActor,int> onHeal=(a,n)=>heals.Add(n);ally.Healed+=onHeal;
                    int before=ally.CurrentHealth;SetProperty(runes,"DueMove",board.CompletedValidPlayerMoves);
                    yield return Until(()=>runes.Ended&&!board.IsBusy&&!owner.HasAnimationActionInProgress,"Restoration heal");
                    ally.Healed-=onHeal;
                    int pulse=CombatAmounts.Round(Mathf.RoundToInt(ally.MaxHealth*owner.Definition.RestorationHealFraction));
                    Assert.That(heals,Is.EqualTo(Enumerable.Repeat(pulse,survivingRunes).ToArray()),"one exact healing pulse per surviving rune");
                    Assert.That(ally.CurrentHealth,Is.GreaterThan(before));Set(bishop,"preferRunes",false);Prime(owner);
                    yield return Until(()=>ally.GetComponent<EnemyAutoAttack>().HasNextSequenceModifier(bishop)&&!owner.HasAnimationActionInProgress,"Benediction blessing");
                }
                if(name=="SiegeSergeant")
                {
                    var siege=owner.GetComponent<SiegeSergeantEnemyAbility>();Prime(owner);
                    yield return Until(()=>Get(siege,"warning")!=null&&!board.IsBusy&&!owner.HasAnimationActionInProgress,"Hammer warning");
                    var warning=(BoardController.GemPairThreat)Get(siege,"warning");int before=run.Player.CurrentHealth;
                    SetProperty(warning,"DueMove",board.CompletedValidPlayerMoves);
                    yield return Until(()=>warning.Ended&&!board.IsBusy&&!owner.HasAnimationActionInProgress,"Hammer impact");Assert.That(run.Player.CurrentHealth,Is.LessThan(before));
                }
                yield return ReviewPoses(name);
                void Mark(BoardController.GemSetThreat threat){if(threat.Owner==owner)signal=true;}
            }
            // Actual raised-shield cast with missing optional controller must
            // resolve through the scaled-time fallback and release its lock.
            yield return Spawn("ShieldKnight");
            owner.transform.Find("VisualRoot").GetComponent<Animator>().runtimeAnimatorController=null;
            Prime(owner);yield return Until(()=>ally.HasShield&&!owner.HasAnimationActionInProgress,"missing-art fallback");
            int cancelledAnnouncements=0;
            yield return Spawn("ShieldKnight");owner.AbilityCastCommitted+=(a,n)=>cancelledAnnouncements++;
            Prime(owner);yield return new WaitForSeconds(.1f);
            owner.GetComponent<ShieldingAlliesEnemyAbility>().enabled=false;
            yield return new WaitForSeconds(1.2f);Assert.That(ally.HasShield,Is.False,"disabled cast cannot grant a late shield");
            Assert.That(cancelledAnnouncements,Is.Zero,"cancelled shield pre-cast stays silent");
            yield return Spawn("CourtMage");owner.AbilityCastCommitted+=(a,n)=>cancelledAnnouncements++;
            Prime(owner);yield return new WaitForSeconds(.1f);
            owner.GetComponent<CourtMageEnemyAbility>().enabled=false;
            yield return Until(()=>!board.IsBusy&&!board.HasPendingBoardMutation,"disabled freeze cleanup");
            Assert.That(board.GetFrozenGemCountForOwner(owner.GetInstanceID()),Is.Zero,"cancelled freeze releases target reservation");
            Assert.That(owner.HasAnimationActionInProgress,Is.False);
            Assert.That(cancelledAnnouncements,Is.Zero,"cancelled freeze pre-cast stays silent");

            yield return Spawn("King");
            var king=owner.GetComponent<KingEnemyAbility>();Prime(owner);
            yield return Until(()=>Get(king,"judgment")!=null&&!board.IsBusy,"Royal Judgment warning");
            var judgment=(BoardController.GemSetThreat)Get(king,"judgment");int playerBefore=run.Player.CurrentHealth;
            SetProperty(judgment,"DueMove",board.CompletedValidPlayerMoves);
            yield return Until(()=>judgment.Ended&&!board.IsBusy&&!owner.HasAnimationActionInProgress,"Royal Judgment impact");
            Assert.That(run.Player.CurrentHealth,Is.LessThan(playerBefore));
            int kingHits=0;owner.GetComponent<EnemyAutoAttack>().AttackResolved+=(a,d,applied)=>kingHits++;
            Set(king,"cycle",1);Prime(owner);
            yield return Until(()=>kingHits>0&&!(bool)Get(king,"pending"),"Royal Command full normal strike");
            owner.TryTakeDamageWithoutFeedback(CombatAmounts.Round(owner.MaxHealth*.6));
            yield return Until(()=>run.Waves.ActiveEnemies.Count==3&&!owner.HasAnimationActionInProgress,"King reinforcement gesture");
            yield return Spawn("King");king=owner.GetComponent<KingEnemyAbility>();Set(king,"cycle",2);
            var heldAttack=owner.GetComponent<EnemyAutoAttack>();
            heldAttack.RestoreContinuation(new EnemyCombatSnapshot {attackRemaining=1.5f,attackSpeed=1,attackRunning=true});
            Prime(owner);yield return Until(()=>heldAttack.IsPausedByAction,"pause starts with accepted sword raise");
            float heldTime=heldAttack.RemainingAttackTime;
            int heldHits=0;heldAttack.AttackResolved+=(a,d,applied)=>heldHits++;
            yield return Until(()=>Get(king,"bombardment")!=null&&!board.IsBusy,"lane warning");
            var threat=(BoardController.LaneThreat)Get(king,"bombardment");
            Assert.That(owner.SpecialIdleState,Is.EqualTo("BombardmentReady"));
            yield return new WaitForSeconds(.1f);var kingImage=owner.transform.Find("VisualRoot").GetComponent<Image>();
            Assert.That(kingImage.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("BombardmentReady"),Is.True);
            yield return new WaitForSeconds(1.7f);
            Assert.That(heldAttack.RemainingAttackTime,Is.EqualTo(heldTime).Within(.001f),"warning preserves the cooldown, even past its remaining duration");
            Assert.That(heldHits,Is.Zero);
            Assert.That(heldAttack.PerformAttackImmediately(),Is.False,"direct auto attack cannot interrupt raised sword");
            Assert.That(heldAttack.TryReserveCommand(this,true),Is.False,"command cannot interrupt raised sword");
            // A threshold reached during the warning must wait for the complete lane action.
            owner.TryTakeDamageWithoutFeedback(CombatAmounts.Round(owner.MaxHealth*.6));
            yield return new WaitForSeconds(.2f);
            Assert.That(run.Waves.ActiveEnemies.Count,Is.EqualTo(2),"reinforcements wait behind Bombardment");
            Assert.That(owner.SpecialIdleState,Is.EqualTo("BombardmentReady"));
            var cells=GetGrid();
            // Select a protected special on the cross. Observe it before refill,
            // where genuine cascades may legitimately activate it later.
            Gem protectedGem=cells[threat.Column,threat.Row];protectedGem.SetSpecialType(GemSpecialType.RowBomb);
            var originalColumn=new List<Gem>();for(int y=0;y<board.Height;y++)if(y!=threat.Row)originalColumn.Add(cells[threat.Column,y]);
            var lanes=new List<bool>();var impactSprites=new List<Sprite>();
            Action<bool,int,float> slash=(row,index,duration)=>
            {
                Assert.That(board.IsBusy,Is.True);lanes.Add(row);impactSprites.Add(kingImage.sprite);
                Assert.That(heldAttack.IsPausedByAction,Is.True,"both lane strikes retain the attack pause");
                Assert.That(heldAttack.RemainingAttackTime,Is.EqualTo(heldTime).Within(.001f));
                if(row)
                {
                    Assert.That(originalColumn.All(g=>g==null),Is.True,"column gems removed together before row strike");
                    for(int y=0;y<board.Height;y++)if(y!=threat.Row)Assert.That(GetGrid()[threat.Column,y],Is.Null,"no refill between strikes");
                    Assert.That(GetGrid()[threat.Column,threat.Row],Is.SameAs(protectedGem),"protected special survives direct lane clear");
                }
                ScreenCapture.CaptureScreenshot(Path.Combine(output,row?"King-row-impact.png":"King-column-impact.png"));
            };
            board.LaneSlash+=slash;
            SetProperty(threat,"DueMove",board.CompletedValidPlayerMoves);
            yield return Until(()=>lanes.Count==1,"first two-handed thrust");
            yield return new WaitForSeconds(.16f);Time.timeScale=0;
            yield return new WaitForSecondsRealtime(.3f);Assert.That(lanes.Count,Is.EqualTo(1));Assert.That(board.IsBusy,Is.True);Time.timeScale=1;
            yield return Until(()=>threat.Ended&&!board.IsBusy&&!owner.HasAnimationActionInProgress,"second thrust and single settlement");
            board.LaneSlash-=slash;
            Assert.That(lanes,Is.EqualTo(new[]{false,true}),"column then row, once each");
            Assert.That(heldHits,Is.Zero,"no attack during raise, warning, strikes or recovery");
            Assert.That(heldAttack.IsPausedByAction,Is.False,"complete Bombardment releases its pause");
            yield return Until(()=>heldAttack.RemainingAttackTime<heldTime-.1f,"cooldown resumes instead of resetting");
            heldAttack.StopAttacking();
            yield return Until(()=>run.Waves.ActiveEnemies.Count==3&&!owner.HasAnimationActionInProgress,"deferred reinforcement gesture");
            var bomb=EnemyAbilityMotionTests.ReadEntries().Single(e=>e.name=="King_Bombardment");var sprites=CombatActionImporter.LoadFrames(bomb.name);
            Assert.That(impactSprites,Is.EqualTo(bomb.impactFrames.Select(i=>sprites[i]).ToArray()),"each lane lands on its thrust drawing");
            for(int y=0;y<board.Height;y++)for(int x=0;x<board.Width;x++)if(board.IsCellPlayable(x,y))Assert.That(GetGrid()[x,y],Is.Not.Null,"final refill complete");
            Assert.That(owner.SpecialIdleState,Is.Null);yield return ReviewPoses("King");
            // Death after the first strike suppresses the second but still
            // settles the already-cleared column through the same board queue.
            Set(king,"cycle",2);Prime(owner);yield return Until(()=>Get(king,"bombardment")!=threat&&!board.IsBusy,"second warning");
            threat=(BoardController.LaneThreat)Get(king,"bombardment");int cancelledSlashes=0;
            Action<bool,int,float> kill=(row,index,duration)=>{cancelledSlashes++;owner.TryTakeDamageWithoutFeedback(1000000);};
            board.LaneSlash+=kill;SetProperty(threat,"DueMove",board.CompletedValidPlayerMoves);
            yield return Until(()=>owner.IsDefeated&&!board.IsBusy,"death settles partial footprint");board.LaneSlash-=kill;
            Assert.That(cancelledSlashes,Is.EqualTo(1));
            Assert.That(heldAttack.IsPausedByAction,Is.False,"death releases Bombardment pause");
            Assert.That(errors,Is.Empty,string.Join("\n",errors));
            File.WriteAllText(Path.Combine(output,"validation.txt"),"PASS: approved ability states at both portrait sizes; actual queued casts, shield contact, pause, missing-art fallback, disable cancellation, King two-handed column/row contact, no intermediate refill, protected special and death cleanup.\n");
            SceneManager.LoadScene("MainMenu");yield return null;
        }
        Application.logMessageReceived-=Log;LogAssert.ignoreFailingMessages=false;yield return new ExitPlayMode();
    }
    private IEnumerator Spawn(string name)
    {
        yield return Until(()=>!board.IsBusy,"fixture board settled");run.Waves.ClearCurrentWave();yield return null;
        yield return Until(()=>!board.IsBusy&&!board.HasPendingBoardMutation,"old owner cleanup");
        SetProperty(run.Waves,"IsWaveActive",true); // isolated encounter, with automatic wave advancement disabled
        Assert.That(run.Waves.TrySummonEnemy(Data(name),out owner),Is.True,name);
        Assert.That(run.Waves.TrySummonEnemy(Data("Farmer"),out ally),Is.True);
        foreach(var actor in run.Waves.ActiveEnemies)
        {
            var attack=actor.GetComponent<EnemyAutoAttack>();Set(attack,"attackAutomatically",false);attack.StopAttacking();
            actor.SetSpecialTurnRequirement(10000);actor.ResetSpecialCounter();
        }
        // These cases measure animation contact, not random opening strength.
        // Keep a real useful response available so interference placement is
        // accepted by the production counterplay guard on every opening.
        board.GetGem(0,0).SetSpecialType(GemSpecialType.ColorCrystal);
        Assert.That(CounterplayGuard.HasUsefulResponse(board.GetImmediateResponses(),run.Player,run.Waves),Is.True);
        yield return new WaitForSeconds(.4f);
    }
    private IEnumerator ReviewShieldClearance()
    {
        var slots=(EnemySlotUI[])Get(run.Waves,"enemySlots");
        var slot=slots.First(s=>s.CurrentEnemy==owner);
        var gem=slot.GetComponentInChildren<EnemyWeaknessIndicatorUI>(true);
        var bar=slot.GetComponentInChildren<ModularHealthBarUI>();
        var image=gem.GetComponent<Image>();
        foreach(int height in new[]{1920,2400})
        {
            Resize(height);yield return Until(()=>Screen.width==1080&&Screen.height==height,"shield review viewport");
            yield return null;yield return null;yield return null;
            Vector3 before=bar.transform.InverseTransformPoint(gem.transform.position);
            owner.GrantShield(10);yield return new WaitForSeconds(.6f);yield return null;
            Assert.That(Vector3.Distance(bar.transform.InverseTransformPoint(gem.transform.position),before),Is.LessThan(.01f),"shield grant must not move the weakness gem relative to its HP bar");
            var track=bar.transform.Find("ShieldTrack").GetComponent<Image>();
            var config=(EnemyWeaknessIndicatorConfig)Get(gem,"config");
            foreach(float scale in new[]{1f,config.MaterializeOvershootScale,config.DeathPopScale})
            {
                gem.transform.localScale=Vector3.one*scale;Canvas.ForceUpdateCanvases();
                Assert.That(RectOf(image).yMax,Is.LessThan(RectOf(track).yMin),"shield must clear weakness gem at "+height+", scale "+scale);
            }
            gem.transform.localScale=Vector3.one;
            ScreenCapture.CaptureScreenshot(Path.Combine(output,"ShieldKnight-clearance-"+height+".png"));
            yield return new WaitForSecondsRealtime(.12f);
            owner.TryTakeDamageWithoutFeedback(1000);yield return null;
            Assert.That(bar.GetComponent<ShieldBarUI>().IsVisible,Is.False);
            Assert.That(Vector3.Distance(bar.transform.InverseTransformPoint(gem.transform.position),before),Is.LessThan(.01f),"shield break must not move the weakness gem relative to its HP bar");
        }
    }
    private IEnumerator ReviewPoses(string character)
    {
        var image=owner.transform.Find("VisualRoot").GetComponent<Image>();var animator=image.GetComponent<Animator>();
        owner.GetComponent<EnemyCombatFeedback>().enabled=false;
        string idle=owner.SpecialIdleState;owner.SpecialIdleState=null;
        foreach(int height in new[]{1920,2400})
        {
            Resize(height);yield return new WaitForSeconds(.25f);Time.timeScale=0;animator.fireEvents=false;
            animator.Play("Idle",0,0);animator.Update(0);yield return null;Rect baseline=RectOf(image);float scale=baseline.height/image.sprite.rect.height;
            foreach(var entry in EnemyAbilityMotionTests.ReadEntries().Where(e=>e.character==character))
            {
                int elapsed=0;var frames=CombatActionImporter.LoadFrames(entry.name);
                for(int i=0;i<frames.Length;i++)
                {
                    animator.Play(entry.state,0,(elapsed+entry.durations[i]*.5f)/entry.durations.Sum());animator.Update(0);elapsed+=entry.durations[i];
                    yield return null;Canvas.ForceUpdateCanvases();Rect rect=RectOf(image);
                    Assert.That(image.sprite,Is.SameAs(frames[i]),entry.name+" pose "+i);
                    Assert.That(rect.height/frames[i].rect.height,Is.EqualTo(scale).Within(.01f),entry.name+" native scale");
                    Assert.That(scale,Is.EqualTo(Mathf.Round(scale)).Within(.01f));
                    Assert.That(rect.yMin,Is.EqualTo(baseline.yMin).Within(.1f),entry.name+" fixed floor");
                    Assert.That(rect.center.x,Is.EqualTo(baseline.center.x).Within(.1f),entry.name+" fixed center");
                    if(entry.impactFrames.Contains(i)||entry.loop&&i==0){ScreenCapture.CaptureScreenshot(Path.Combine(output,entry.name+"-"+i+"-"+height+".png"));yield return new WaitForSecondsRealtime(.08f);}
                }
            }
            animator.Play("Idle",0,0);animator.Update(0);animator.fireEvents=true;Time.timeScale=1;
            yield return new WaitForSeconds(.1f);
            var issues=GameplayPixelLayoutValidator.Validate(Object.FindFirstObjectByType<GameplayPixelLayoutController>(),out string layout);
            File.WriteAllText(Path.Combine(output,character+"-"+height+"-layout.txt"),layout+"\n"+string.Join("\n",issues));Assert.That(issues,Is.Empty,string.Join("; ",issues));
        }
        owner.SpecialIdleState=idle;
    }
    private Gem[,] GetGrid()=>(Gem[,])Get(board,"gems");
    private static void Prime(EnemyActor actor){actor.SetSpecialTurnRequirement(1);actor.ResetSpecialCounter();actor.RegisterValidPlayerTurn();}
    private static EnemyDefinition Data(string name)=>AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Game/Data/Enemies/Enemy_"+name+".asset");
    private static void Resize(int height)=>typeof(CombatActionValidation).GetMethod("SetSize",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{1080,height});
    private static object Get(object target,string name)=>target.GetType().GetField(name,Flags).GetValue(target);
    private static void Set(object target,string name,object value)=>target.GetType().GetField(name,Flags).SetValue(target,value);
    private static void SetProperty(object target,string name,object value)=>target.GetType().GetProperty(name,Flags).SetValue(target,value);
    private static IEnumerator Until(Func<bool> ready,string label){float deadline=Time.realtimeSinceStartup+25;while(!ready()){Assert.That(Time.realtimeSinceStartup,Is.LessThan(deadline),label);yield return null;}}
    private static Rect RectOf(Image image)
    {
        var c=new Vector3[4];image.rectTransform.GetWorldCorners(c);var camera=image.canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:image.canvas.worldCamera;
        var a=RectTransformUtility.WorldToScreenPoint(camera,c[0]);var b=RectTransformUtility.WorldToScreenPoint(camera,c[2]);return Rect.MinMaxRect(a.x,a.y,b.x,b.y);
    }
}
