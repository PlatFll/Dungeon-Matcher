using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.TestTools;

public sealed partial class ForestFoundationPlayTests
{
    private EnemyActor judgmentKing;
    private KingEnemyAbility judgmentAbility;
    private BoardController.GemSetThreat judgmentWarning;

    private IEnumerator PrepareJudgment()
    {
        yield return LaunchDungeonHazards(true);
        Assert.That(EditorUtility.audioMasterMute, Is.True);
        Assert.That(CombatMoveClock.Active, Is.False, "explicit legacy dungeon seconds profile");
        var data = AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Game/Data/Enemies/Enemy_King.asset");
        Assert.That(Run.Waves.TrySummonEnemy(data, out judgmentKing), Is.True);
        yield return Stable(); PreserveRoster(); PrepareSafeMove();
        Set(Run.Player, "maximumHealth", 10000); Set(Run.Player, "currentHealth", 10000);
        judgmentAbility = judgmentKing.GetComponent<KingEnemyAbility>();
        int announced = 0;
        judgmentKing.AbilityCastCommitted += (actor, name) => { if (name == "Judgment") announced++; };
        for (int i = 0; i < judgmentKing.SpecialTurnRequirement; i++) judgmentKing.RegisterValidPlayerTurn();
        yield return Until(() => Get(judgmentAbility, "judgment") != null && Run.Continuation.CanCapture, "Judgment warning committed");
        judgmentWarning = (BoardController.GemSetThreat)Get(judgmentAbility, "judgment");
        Assert.That(judgmentWarning.Targets.Count, Is.EqualTo(3));
        Assert.That(announced, Is.EqualTo(1));
        Assert.That(judgmentWarning.DueMove - Run.Board.CompletedValidPlayerMoves, Is.EqualTo(3));
    }

    private void MakeJudgmentDue() => typeof(BoardController.GemSetThreat).GetProperty("DueMove")
        .SetValue(judgmentWarning, Run.Board.CompletedValidPlayerMoves);

    [UnityTest] public IEnumerator JudgmentRevisionZeroSurvivors() { yield return JudgmentSurvivors(0); }
    [UnityTest] public IEnumerator JudgmentRevisionOneSurvivor() { yield return JudgmentSurvivors(1); }
    [UnityTest] public IEnumerator JudgmentRevisionTwoSurvivors() { yield return JudgmentSurvivors(2); }
    [UnityTest] public IEnumerator JudgmentRevisionThreeSurvivors() { yield return JudgmentSurvivors(3); }

    private IEnumerator JudgmentSurvivors(int count)
    {
        yield return PrepareJudgment();
        var converted = judgmentWarning.Targets.Skip(count).ToArray();
        foreach (var gem in converted) gem.SetSpecialType(GemSpecialType.RowBomb);
        var survivors = judgmentWarning.Targets.ToArray();
        Assert.That(survivors.Length, Is.EqualTo(count), "special conversion removes the physical mark");
        var original = new Dictionary<Vector2Int, Gem>();
        for (int x=0;x<Run.Board.Width;x++) for(int y=0;y<Run.Board.Height;y++) original[new Vector2Int(x,y)] = Run.Board.GetGem(x,y);
        var removed = new HashSet<Vector2Int>();
        var states = new List<string>(); var damage = new List<int>();
        float started = 0;
        Run.Player.DamageTaken += (player, amount) => damage.Add(amount);
        judgmentKing.SpecialMotionRequested += actor =>
        {
            int index = states.Count; states.Add(actor.SpecialMotionState); started = Time.time;
            Assert.That(index, Is.LessThan(count));
            var gem = survivors[index];
            // Unity's destroyed-object wrapper retains no usable coordinates;
            // recover its original cell from the transaction snapshot.
            var cell = original.Single(pair => ReferenceEquals(pair.Value, gem)).Key;
            removed.Add(cell);
            Assert.That(Run.Board.IsBusy, Is.True);
            Assert.That(Run.Continuation.CanCapture, Is.False, "no checkpoint halfway through a strike transaction");
            foreach (var pair in original)
                if (removed.Contains(pair.Key)) Assert.That(Run.Board.GetGem(pair.Key.x,pair.Key.y), Is.Null, "prior gaps stay empty");
                else Assert.That(Run.Board.GetGem(pair.Key.x,pair.Key.y), Is.SameAs(pair.Value), "no gravity, refill or cascade between strikes");
            Assert.That(damage.Count, Is.EqualTo(index), "gem consumption precedes its animation and damage");
        };
        judgmentAbility.HeavyStrike += actor =>
        {
            string state = states.Last();
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/_Game/Animations/CombatActions/King_" + state + ".anim");
            float impact = clip.events.Single(e=>e.functionName=="AbilityBeat").time;
            Assert.That(Time.time-started, Is.InRange(impact-.03f, impact+.15f), "damage follows the authored contact, not the 3-second fallback");
            Assert.That(Run.Board.IsBusy, Is.True);
        };
        MakeJudgmentDue();
        yield return Until(()=>judgmentWarning.Ended && Run.Continuation.CanCapture, "complete Judgment transaction");
        Assert.That(states, Is.EqualTo(new[]{"JudgmentStrike1","JudgmentStrike2","JudgmentFinisher"}.Take(count)));
        var expected = Enumerable.Range(0,count).Select(i=>CombatAmounts.Round(Mathf.RoundToInt(
            (i==2 ? judgmentKing.Definition.JudgmentFinisherBaseDamage : judgmentKing.Definition.JudgmentBaseDamage)*judgmentKing.RuntimeStats.DamageMultiplier)));
        Assert.That(damage, Is.EqualTo(expected));
        Assert.That(Run.Board.GetImmediateResponses().Count, Is.GreaterThan(0));
        for(int x=0;x<Run.Board.Width;x++)for(int y=0;y<Run.Board.Height;y++)
            if(Run.Board.IsCellPlayable(x,y)) Assert.That(Run.Board.GetGem(x,y), Is.Not.Null);
        yield return new WaitForSeconds(.3f);Assert.That(damage.Count, Is.EqualTo(count), "no duplicate delayed contact");
    }

    [UnityTest] public IEnumerator JudgmentRevisionSavePreservesPhysicalOrderAndPause()
    {
        yield return PrepareJudgment();
        var ids = judgmentWarning.Targets.Select(g=>g.BoardIdentity).ToArray();
        yield return ResumeRoster();
        judgmentKing=Enemy("king");judgmentAbility=judgmentKing.GetComponent<KingEnemyAbility>();
        judgmentWarning=(BoardController.GemSetThreat)Get(judgmentAbility,"judgment");
        Assert.That(judgmentWarning.Targets.Select(g=>g.BoardIdentity), Is.EqualTo(ids));
        int motions=0,hits=0;
        judgmentKing.SpecialMotionRequested+=_=>motions++;
        judgmentAbility.HeavyStrike+=_=>hits++;
        MakeJudgmentDue();yield return Until(()=>motions==1,"first strike starts");
        Run.GetComponent<RunControlsUI>().OpenSettings();int hp=Run.Player.CurrentHealth;
        yield return new WaitForSecondsRealtime(.7f);
        Assert.That(hits,Is.Zero);Assert.That(Run.Player.CurrentHealth,Is.EqualTo(hp));
        Assert.That(Run.Continuation.CanCapture,Is.False);
        Run.GetComponent<RunControlsUI>().Close();
        yield return Until(()=>Run.Continuation.CanCapture,"unpaused sequence settles");
        Assert.That(hits,Is.EqualTo(3));
    }

    [UnityTest] public IEnumerator JudgmentRevisionKingDeathCancelsLaterStrikes()
    {
        yield return PrepareJudgment();int hits=0,motions=0;
        judgmentKing.SpecialMotionRequested+=_=>motions++;
        judgmentAbility.HeavyStrike+=_=>hits++;
        MakeJudgmentDue();yield return Until(()=>motions==1,"windup before death");
        judgmentKing.ResolveDamageWithoutFeedback(20000);
        yield return Until(()=>!Run.Board.IsBusy&&!Run.Board.HasPendingBoardMutation,"cancelled holes settle");
        Assert.That(hits,Is.Zero);Assert.That(motions,Is.EqualTo(1));
        Assert.That(Run.Board.GetImmediateResponses().Count,Is.GreaterThan(0));
    }

    [UnityTest] public IEnumerator JudgmentRevisionPlayerDeathStopsSequence()
    { yield return JudgmentPlayerDeath(); }

    private IEnumerator JudgmentPlayerDeath()
    {
        yield return PrepareJudgment();Set(Run.Player,"currentHealth",5);int hits=0,motions=0;
        judgmentKing.SpecialMotionRequested+=_=>motions++;
        judgmentAbility.HeavyStrike+=_=>hits++;
        MakeJudgmentDue();yield return Until(()=>Run.Player.IsDefeated,"first impact defeats player");
        // The normal death screen pauses the scene. Let cancellation cleanup run.
        Time.timeScale=1;
        yield return Until(()=>!Run.Board.IsBusy&&!Run.Board.HasPendingBoardMutation,"death settles consumed gap");
        Assert.That(hits,Is.EqualTo(1));Assert.That(motions,Is.EqualTo(1));
    }

    [UnityTest] public IEnumerator JudgmentRevisionPhysicalSwapAndMinisterContinue()
    { yield return JudgmentMovementAndMinister(); }

    private IEnumerator JudgmentMovementAndMinister()
    {
        yield return PrepareJudgment();
        // Put a mark on the non-matching partner in the fixture's legal swap.
        // Board ownership still performs the real swap, gravity and settlement.
        var moving=Run.Board.GetGem(safeMoveTo.x,safeMoveTo.y);
        var origin=new Vector2Int(moving.Column,moving.Row);
        judgmentWarning.Targets.Clear();judgmentWarning.Targets.Add(moving);
        Run.Board.StartCoroutine((IEnumerator)Call(Run.Board,"TrySwap",Run.Board.GetGem(safeMoveFrom.x,safeMoveFrom.y),moving));
        yield return Until(()=>Run.Board.CompletedValidPlayerMoves==1&&Run.Continuation.CanCapture,"marked identity moves legally");
        Assert.That(moving,Is.Not.Null);Assert.That(new Vector2Int(moving.Column,moving.Row),Is.Not.EqualTo(origin));
        Assert.That(judgmentWarning.Targets,Does.Contain(moving));
        int identity=moving.BoardIdentity;
        var ministerData=AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Game/Data/Enemies/Enemy_RoyalArchbishop.asset");
        Assert.That(Run.Waves.TrySummonEnemy(ministerData,out var minister),Is.True);
        yield return Stable();QuietKitFixture();
        Assert.That(CombatGuide.Enemy(minister),Does.Contain("The Minister"));
        yield return ResumeRoster();
        Assert.That(Enemy("royal_arcanist").Definition,Is.SameAs(ministerData));
        Assert.That(CombatGuide.Enemy(Enemy("royal_arcanist")),Does.Contain("The Minister"));
        judgmentKing=Enemy("king");judgmentAbility=judgmentKing.GetComponent<KingEnemyAbility>();
        judgmentWarning=(BoardController.GemSetThreat)Get(judgmentAbility,"judgment");
        Assert.That(judgmentWarning.Targets.Select(g=>g.BoardIdentity),Is.EqualTo(new[]{identity}));
        int hits=0;judgmentAbility.HeavyStrike+=_=>hits++;
        MakeJudgmentDue();yield return Until(()=>judgmentWarning.Ended&&Run.Continuation.CanCapture,"moved saved identity resolves once");
        Assert.That(hits,Is.EqualTo(1));
    }

    [UnityTest] public IEnumerator JudgmentRevisionFinisherPortraits()
    { yield return JudgmentPortraits(); }

    private IEnumerator JudgmentPortraits()
    {
        yield return PrepareJudgment();
        bool impact=false;Sprite frozen=null;
        var image=judgmentKing.transform.Find("VisualRoot").GetComponent<UnityEngine.UI.Image>();
        var animator=image.GetComponent<Animator>();
        judgmentAbility.HeavyStrike+=_=>
        {
            if(judgmentKing.SpecialMotionState!="JudgmentFinisher")return;
            frozen=image.sprite;Assert.That(frozen.name,Does.Contain("JudgmentFinisher"));
            Time.timeScale=0;impact=true;
        };
        MakeJudgmentDue();yield return Until(()=>impact,"freeze the rendered finisher contact");
        var layout=UnityEngine.Object.FindFirstObjectByType<GameplayPixelLayoutController>();
        try
        {
            string folder=Path.GetFullPath(".utmp/RosterEndless/Judgment");Directory.CreateDirectory(folder);
            animator.enabled=false;
            var sizes=new[]{new Vector2Int(720,1280),new Vector2Int(1080,1920),new Vector2Int(1080,2400),new Vector2Int(1080,2400)};
            for(int view=0;view<sizes.Length;view++)
            {
                var size=sizes[view];bool inset=view==3;
                typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic).Invoke(null,new object[]{size});
                GameplayPixelLayoutController.ValidationSafeArea=inset?new Rect(32,72,1016,2240):(Rect?)null;
                layout.Refresh(true);
                yield return Until(()=>Screen.width==size.x&&Screen.height==size.y,"Judgment portrait size");
                layout.Refresh(true);
                // Resizing rebuilds presentation from the ready sprite. This
                // pose fixture reapplies the actual captured contact pixels.
                image.sprite=frozen;
                for(int i=0;i<8;i++)yield return null;
                Assert.That(image.sprite,Is.SameAs(frozen));
                Assert.That(GameplayPixelLayoutValidator.Validate(layout,out string report),Is.Empty,report);
                ScreenCapture.CaptureScreenshot(Path.Combine(folder,"judgment-finisher-"+(inset?"safe-inset":size.x+"x"+size.y)+".png"));
                yield return null;yield return null;
            }
        }
        finally {GameplayPixelLayoutController.ValidationSafeArea=null;animator.enabled=true;Time.timeScale=1;}
        yield return Until(()=>Run.Continuation.CanCapture,"finisher capture releases");
    }

    [UnityTest] public IEnumerator JudgmentRevisionMissingMotionStillSettles()
    {
        yield return PrepareJudgment();
        var animator=judgmentKing.transform.Find("VisualRoot").GetComponent<Animator>();animator.enabled=false;
        foreach(var gem in judgmentWarning.Targets.Skip(1).ToArray())gem.SetSpecialType(GemSpecialType.RowBomb);
        int hits=0;judgmentAbility.HeavyStrike+=_=>hits++;
        MakeJudgmentDue();yield return Until(()=>judgmentWarning.Ended&&Run.Continuation.CanCapture,"fallback completes");
        Assert.That(hits,Is.EqualTo(1));
    }
}

public sealed class JudgmentRevisionAssetTests
{
    [Test] public void MinisterKeepsStableIdentityAndKit()
    {
        var data=AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Game/Data/Enemies/Enemy_RoyalArchbishop.asset");
        Assert.That(data.DisplayName,Is.EqualTo("The Minister"));Assert.That(data.EnemyId,Is.EqualTo("royal_arcanist"));
        Assert.That(data.SpecialAbilityKind,Is.EqualTo(EnemySpecialAbilityKind.RoyalArchbishop));
    }
    [Test] public void JudgmentMotionHasNativePixelsAndOneContactPerState()
    {
        var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/_Game/Animations/CombatIdles/King_Idle.controller");
        foreach(string name in new[]{"JudgmentStrike1","JudgmentStrike2","JudgmentFinisher"})
        {
            var clip=(AnimationClip)controller.layers[0].stateMachine.states.Single(s=>s.state.name==name).state.motion;
            Assert.That(clip.events.Count(e=>e.functionName=="AbilityBeat"),Is.EqualTo(1));
            Assert.That(clip.events.Count(e=>e.functionName=="AbilityComplete"),Is.EqualTo(1));
            var path="Assets/_Game/Art/CombatActions/King_"+name+".png";
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            Assert.That(importer.filterMode,Is.EqualTo(FilterMode.Point));Assert.That(importer.mipmapEnabled,Is.False);
            Assert.That(importer.textureCompression,Is.EqualTo(TextureImporterCompression.Uncompressed));
            Assert.That(File.ReadAllBytes(path),Is.EqualTo(File.ReadAllBytes("ArtSource/EnemyAttacks/King_"+name+".png")));
        }
    }
}
