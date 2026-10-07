using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

public sealed partial class ForestFoundationPlayTests
{
    private void ReadyForest(EnemyActor enemy)
    { Set(enemy,"isSpecialReady",true);Set(enemy,"currentSpecialTurnCount",enemy.SpecialTurnRequirement); }

    [UnityTest] public IEnumerator ForestRevisionInstantRootbinder() {yield return CheckInstantRoots(6);}
    [UnityTest] public IEnumerator ForestRevisionInstantWarden() {yield return CheckInstantRoots(9);}
    private IEnumerator CheckInstantRoots(int fixture)
    {
            yield return Launch(fixture,true);PreserveRoster();PrepareSafeMove();
            var actor=Enemy(fixture==6?"orc_rootbinder":"barkhide_warden");
            Assert.That(actor.Definition.UseAuthoredSpecialAbilityMotion,Is.True);
            var board=Run.Board;int marked=0;board.GemSetMarked+=_=>marked++;
            int tick=board.CompletedValidPlayerMoves;float began=-1,effect=-1;bool held=false;
            actor.SpecialMotionRequested+=_=>began=Time.time;
            board.RootsChanged+=()=>{if(effect<0 && board.OwnedRootCount(actor)>0){effect=Time.time;held=!Run.Continuation.CanCapture;}};
            ReadyForest(actor);
            Assert.That(board.OwnedRootCount(actor),Is.Zero,"no effect before contact");
            yield return Move();
            Assert.That(began,Is.GreaterThanOrEqualTo(0));
            Assert.That(effect-began,Is.InRange(.27f,.9f),"no three-second missing-contact fallback");
            Assert.That(held,Is.True,"release animation owns the transaction");
            Assert.That(marked,Is.Zero);Assert.That(board.CompletedValidPlayerMoves,Is.EqualTo(tick+1));
            var root=RootCell(actor);Assert.That(root.hasGem,Is.False);
            Assert.That(root.durability,Is.EqualTo(fixture==6?1:2));
            Assert.That(root.barricadeStyle,Is.EqualTo(fixture==6?EnemyBarricadeStyle.Root:EnemyBarricadeStyle.ShieldRoot));
            Assert.That(board.OwnedVineCount(actor),Is.EqualTo(4));
            Assert.That(actor.HasAnimationActionInProgress,Is.False);
            Assert.That(UnityEditor.EditorUtility.audioMasterMute,Is.True);
    }

    [UnityTest] public IEnumerator ForestRevisionWardedMultipleSourcesSummonContinueAndDeath()
    {
        yield return Launch(9,true);PreserveRoster();PrepareSafeMove();
        var first=Enemy("barkhide_warden");var ally=Enemy("elven_scout");
        Assert.That(Run.Waves.TrySummonEnemy(first.Definition,out var second),Is.True);
        QuietKitFixture();
        foreach(var owner in new[]{first,second})
        {Assert.That(Run.Board.TryQueuePlantRoots(owner,1,2,false,true,null,true),Is.True);yield return Stable();}
        foreach(var member in Run.Waves.ActiveEnemies)
        {
            Assert.That(member.IsWarded,Is.True);int hp=member.CurrentHealth;member.ResolveDirectDamage(20);
            Assert.That(hp-member.CurrentHealth,Is.EqualTo(15),"two sources still give 25%, once");
            Assert.That(CombatGuide.Enemy(member),Does.Contain("WARDED: 25%"));
        }
        yield return ResumeRoster();first=Enemy("barkhide_warden");ally=Enemy("elven_scout");
        Assert.That(ally.IsWarded,Is.True,"sources are rebuilt after the board restores");
        Assert.That(RootCell(first).barricadeStyle,Is.EqualTo(EnemyBarricadeStyle.ShieldRoot));
        first.ResolveDirectDamage(200000);yield return Stable();
        Assert.That(ally.IsWarded,Is.True,"other living root still owns Warded");
        second=Run.Waves.ActiveEnemies.First(e=>!e.IsDefeated && e.Definition.EnemyId=="barkhide_warden");
        second.ResolveDirectDamage(200000);yield return Stable();
        Assert.That(ally.IsWarded,Is.False);int before=ally.CurrentHealth;ally.ResolveDirectDamage(20);
        Assert.That(before-ally.CurrentHealth,Is.EqualTo(20));
    }

    [UnityTest] public IEnumerator ForestRevisionOldRootbinderWarning() {yield return CheckLegacyRootWarning(6);}
    [UnityTest] public IEnumerator ForestRevisionOldWardenWarning() {yield return CheckLegacyRootWarning(9);}
    private IEnumerator CheckLegacyRootWarning(int fixture)
    {
            yield return Launch(fixture,true);PreserveRoster();PrepareSafeMove();
            var owner=Enemy(fixture==6?"orc_rootbinder":"barkhide_warden");
            BoardController.GemSetThreat old=null;
            Run.Board.TryQueueRootWarning(owner,1,fixture==6?1:2,false,fixture==9,t=>old=t);yield return Stable();
            Assert.That(old,Is.Not.Null);
            if(fixture==6)owner.GetComponent<RootbinderEnemyAbility>().RestoreContinuation(new EnemyCombatSnapshot(),_=>null);
            else owner.GetComponent<ForestMilestoneEnemyAbility>().RestoreContinuation(new EnemyCombatSnapshot{
                forestMilestone=new ForestMilestoneSnapshot{version=2,state=1,sequence=1,deadline=old.DueMove}},_=>null);
            Assert.That(old.Ended,Is.True);Assert.That(Run.Board.OwnedRootCount(owner),Is.Zero);
            Assert.That(owner.GetComponent<EnemyStagger>().IsStaggered,Is.False);
            Assert.That(owner.GetComponent<EnemyAutoAttack>().IsPausedByAction,Is.False);
    }

    private IEnumerator PrepareRevisedBough()
    {
        yield return Launch(19,true);PreserveRoster();PrepareSafeMove();
        var actor=Enemy("ancient_treant");var kit=actor.GetComponent<ForestPressureAbility>();
        kit.RestoreContinuation(new EnemyCombatSnapshot{forestRoster=new ForestRosterSnapshot{cycle=1}},_=>null);
        ReadyForest(actor);yield return Move();
        var threat=Run.Board.RestoredSet(actor);Assert.That(threat,Is.Not.Null);
        Assert.That(threat.Targets.Count,Is.EqualTo(3));Assert.That(threat.CancelOnAnyTargetLost,Is.True);
        var a=threat.Targets[0];Assert.That(threat.Targets.All(g=>Mathf.Abs(g.Column-a.Column)+Mathf.Abs(g.Row-a.Row)<=1),Is.True);
    }

    [UnityTest] public IEnumerator ForestRevisionBoughAnyRealClearCancelsWholeCastWithoutConsumingOthers()
    {yield return BoughAnyClear();}
    private IEnumerator BoughAnyClear()
    {
        yield return PrepareRevisedBough();var actor=Enemy("ancient_treant");var board=Run.Board;
        var threat=board.RestoredSet(actor);var survivors=threat.Targets.Skip(1).ToArray();
        int hp=Run.Player.CurrentHealth;var first=threat.Targets[0];
        yield return (IEnumerator)Call(board,"ClearMatches",new HashSet<Gem>{first},null,false);
        Assert.That(threat.Ended,Is.True);
        yield return (IEnumerator)Call(board,"CollapseAndRefillBoard");yield return Stable();yield return null;
        Assert.That(survivors.All(g=>board.GetGem(g.Column,g.Row)==g),Is.True);
        Assert.That(actor.GetComponent<ForestPressureAbility>().IsPreparing,Is.False);
        Assert.That(Run.Player.CurrentHealth,Is.EqualTo(hp));
        Assert.That(actor.GetComponent<EnemyStagger>().IsStaggered,Is.False);
        Assert.That(actor.GetComponent<EnemyAutoAttack>().IsPausedByAction,Is.False);
    }

    [UnityTest] public IEnumerator ForestRevisionBoughKeepsMarksAcrossContinueThenHitsOnce()
    {yield return BoughSavedHit();}
    private IEnumerator BoughSavedHit()
    {
        yield return PrepareRevisedBough();var actor=Enemy("ancient_treant");var board=Run.Board;
        var threat=board.RestoredSet(actor);KeepBoughAwayFromFixtureMove(threat);
        int[] ids=threat.Targets.Select(g=>g.BoardIdentity).ToArray();
        yield return ResumeRoster();actor=Enemy("ancient_treant");board=Run.Board;threat=board.RestoredSet(actor);
        Assert.That(threat.Targets.Select(g=>g.BoardIdentity).ToArray(),Is.EqualTo(ids));
        Assert.That(CombatGuide.Enemy(actor),Does.Contain("Falling Bough: 3 marks"));
        var cells=threat.Targets.Select(g=>new Vector2Int(g.Column,g.Row)).ToArray();
        typeof(BoardController.GemSetThreat).GetProperty("DueMove").SetValue(threat,board.CompletedValidPlayerMoves+1);
        int hp=Run.Player.CurrentHealth,hits=0;
        Run.Player.DamageTaken+=(_,n)=>hits++;
        yield return MoveAvoidingBough(threat);
        Assert.That(hp-Run.Player.CurrentHealth,Is.EqualTo(CombatAmounts.Round(actor.Definition.FallingBoughDamage*actor.RuntimeStats.DamageMultiplier)));
        Assert.That(hits,Is.EqualTo(1));
        Assert.That(cells.All(c=>board.GetGem(c.x,c.y)!=null),Is.True);
        Assert.That(board.CaptureContinuation(Run.Waves.ContinuationOwnerSlot).cells.All(c=>!ids.Contains(c.identity)),Is.True);
        Assert.That(actor.GetComponent<ForestPressureAbility>().IsPreparing,Is.False);
        Assert.That(actor.GetComponent<EnemyStagger>().IsStaggered,Is.False);
    }

    [UnityTest] public IEnumerator ForestRevisionAnsweredReadyBoughCannotRecastOnSameMove()
    {yield return BoughReadyAnswer();}
    private IEnumerator BoughReadyAnswer()
    {
        yield return PrepareRevisedBough();PrepareSafeMove();
        var actor=Enemy("ancient_treant");var board=Run.Board;var threat=board.RestoredSet(actor);
        threat.Targets.Clear();threat.Targets.Add(board.GetGem(safeMoveFrom.x,safeMoveFrom.y));
        typeof(BoardController.GemSetThreat).GetProperty("DueMove").SetValue(threat,board.CompletedValidPlayerMoves+4);
        ReadyForest(actor);int casts=0;actor.AbilityCastCommitted+=(_,name)=>casts++;
        yield return Move();yield return null;
        Assert.That(threat.Ended,Is.True);Assert.That(board.RestoredSet(actor),Is.Null);
        Assert.That(casts,Is.Zero);Assert.That(actor.CurrentSpecialTurnCount,Is.Zero);
        Assert.That(actor.GetComponent<EnemyStagger>().IsStaggered,Is.False);
    }

    [UnityTest] public IEnumerator ForestRevisionBoughMarkFollowsLegalSwapAndSavedIdentity()
    {yield return BoughLegalMove();}
    private IEnumerator BoughLegalMove()
    {
        yield return PrepareRevisedBough();var actor=Enemy("ancient_treant");var board=Run.Board;
        var threat=board.RestoredSet(actor);PrepareSafeMove();var moving=board.GetGem(safeMoveTo.x,safeMoveTo.y);
        var origin=new Vector2Int(moving.Column,moving.Row);int id=moving.BoardIdentity;
        threat.Targets.Clear();threat.Targets.Add(moving);
        board.StartCoroutine((IEnumerator)Call(board,"TrySwap",board.GetGem(safeMoveFrom.x,safeMoveFrom.y),moving));
        int targetMove=board.CompletedValidPlayerMoves;
        yield return Until(()=>board.CompletedValidPlayerMoves>targetMove && Run.Continuation.CanCapture,"Bough physical mark moves");
        Assert.That(threat.Ended,Is.False);Assert.That(threat.Targets,Does.Contain(moving));
        Assert.That(new Vector2Int(moving.Column,moving.Row),Is.Not.EqualTo(origin));
        yield return ResumeRoster();
        Assert.That(Run.Board.RestoredSet(Enemy("ancient_treant")).Targets.Single().BoardIdentity,Is.EqualTo(id));
    }

    private void KeepBoughAwayFromFixtureMove(BoardController.GemSetThreat threat)
    {
        // Keep the physical targets outside this controlled opening and its
        // refill columns. Target selection compactness is checked separately.
        threat.Targets.Clear();
        foreach(var at in new[]{new Vector2Int(0,0),new Vector2Int(1,0),new Vector2Int(0,1)})
            threat.Targets.Add(Run.Board.GetGem(at.x,at.y));
    }
    private IEnumerator MoveAvoidingBough(BoardController.GemSetThreat threat)
    {
        PrepareSafeMove();var board=Run.Board;
        int tick=board.CompletedValidPlayerMoves;
        board.StartCoroutine((IEnumerator)Call(board,"TrySwap",board.GetGem(safeMoveFrom.x,safeMoveFrom.y),board.GetGem(safeMoveTo.x,safeMoveTo.y)));
        yield return Until(()=>board.CompletedValidPlayerMoves==tick+1 && Run.Continuation.CanCapture,"unanswered Bough release");
    }

    [UnityTest] public IEnumerator ForestRevisionBuffAndRootPortraits() {yield return ForestRootPortraits();}
    private IEnumerator ForestRootPortraits()
    {
        yield return Launch(9,true);PreserveRoster();PrepareSafeMove();
        var warden=Enemy("barkhide_warden");
        var matriarch=AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Game/Data/Enemies/Forest/Briar_Matriarch.asset");
        Assert.That(Run.Waves.TrySummonEnemy(matriarch,out var boss),Is.True);QuietKitFixture();
        Run.Board.TryQueuePlantRoots(warden,1,2,false,true,null,true);yield return Stable();
        Run.Board.TryQueuePlantRoots(boss,2,2,true,true,null);yield return Stable();
        var shield=RootCell(warden);yield return ClearRootSide(shield);yield return ClearRootSide(shield);
        Assert.That(RootCell(warden).durability,Is.EqualTo(1));
        var sprites=Run.Board.GetComponentsInChildren<SpriteRenderer>().Where(r=>r.sprite!=null).Select(r=>r.sprite.name).ToArray();
        Assert.That(sprites,Does.Contain("ShieldRoot_1"));Assert.That(sprites,Does.Contain("HeartRoot_2"));
        string folder=Path.GetFullPath(".utmp/RosterEndless/Forest");Directory.CreateDirectory(folder);
        var layout=UnityEngine.Object.FindFirstObjectByType<GameplayPixelLayoutController>();
        // Observe the live renderer before freezing presentation for review.
        yield return Until(()=>Run.Board.GetComponentsInChildren<SpriteRenderer>()
            .Where(r=>r.GetComponent<RootLifePulse>()!=null).All(r=>
            {var p=new MaterialPropertyBlock();r.GetPropertyBlock(p);return p.GetFloat("_FlashAmount")==0;}),"root hit flash finishes");
        foreach(var renderer in Run.Board.GetComponentsInChildren<SpriteRenderer>().Where(r=>r.GetComponent<RootLifePulse>()!=null))
        {
            var properties=new MaterialPropertyBlock();renderer.GetPropertyBlock(properties);
            Assert.That(properties.GetFloat("_FlashAmount"),Is.Zero,"root must finish its hit flash");
            Assert.That(renderer.color,Is.EqualTo(UnityEngine.Color.white));
        }
        Time.timeScale=0;
        try
        {
            var sizes=new[]{new Vector2Int(720,1280),new Vector2Int(1080,1920),new Vector2Int(1080,2400),new Vector2Int(1080,2400)};
            for(int view=0;view<sizes.Length;view++)
            {
                var size=sizes[view];bool inset=view==3;
                typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic).Invoke(null,new object[]{size});
                GameplayPixelLayoutController.ValidationSafeArea=inset?new Rect(32,72,1016,2240):(Rect?)null;
                yield return Until(()=>Screen.width==size.x&&Screen.height==size.y,"Forest portrait size");
                layout.Refresh(true);for(int i=0;i<8;i++)yield return null;
                Assert.That(GameplayPixelLayoutValidator.Validate(layout,out string report),Is.Empty,report);
                var buffs=UnityEngine.Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None).Where(t=>t.name=="EnemyBuffs"&&t.gameObject.activeInHierarchy).ToArray();
                Assert.That(buffs.Length,Is.EqualTo(3));
                foreach(var label in buffs)
                {
                    label.ForceMeshUpdate();Assert.That(label.text,Is.EqualTo("WARDED"));Assert.That(label.isTextOverflowing,Is.False);
                    var weakness=label.transform.parent.GetComponentInChildren<EnemyWeaknessIndicatorUI>(true).transform as RectTransform;
                    Assert.That(Mathf.Abs(label.rectTransform.position.y-weakness.position.y),Is.LessThan(2));
                    Assert.That(label.canvasRenderer.cull,Is.False,"Buff must be rendered inside the battle mask");
                    Assert.That(label.textInfo.characterInfo.Count(c=>c.isVisible),Is.EqualTo(6));
                }
                ScreenCapture.CaptureScreenshot(Path.Combine(folder,"roots-warded-"+(inset?"safe-inset":size.x+"x"+size.y)+".png"));
                yield return null;yield return null;
            }
        }
        finally {GameplayPixelLayoutController.ValidationSafeArea=null;Time.timeScale=1;}
    }
}

public sealed class ForestRevisionAssetTests
{
    [Test] public void ForestRevisionNativeRootsAndPlacementContacts()
    {
        var theme=Resources.Load<GameplayThemeDefinition>("Zones/ForestTheme");
        foreach(var sprite in new[]{theme.shieldRootLevelOne,theme.shieldRootLevelTwo,theme.heartrootLevelOne,theme.heartrootLevelTwo})
        {
            Assert.That(sprite,Is.Not.Null);Assert.That(sprite.rect.size,Is.EqualTo(new Vector2(64,64)));
            var path=AssetDatabase.GetAssetPath(sprite);var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            Assert.That(importer.filterMode,Is.EqualTo(FilterMode.Point));Assert.That(importer.mipmapEnabled,Is.False);
            Assert.That(importer.textureCompression,Is.EqualTo(TextureImporterCompression.Uncompressed));
            Assert.That(File.ReadAllBytes(path),Is.EqualTo(File.ReadAllBytes("ArtSource/Forest/RosterRevision/"+sprite.name+".png")));
        }
        foreach(string name in new[]{"Orc_Rootbinder","Barkhide_Warden"})
        {
            var definition=AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Game/Data/Enemies/Forest/"+name+".asset");
            Assert.That(definition.UseAuthoredSpecialAbilityMotion,Is.True);
            var clip=definition.AnimationControllerOverride.animationClips.Single(c=>c.name.EndsWith("_Release"));
            Assert.That(AnimationUtility.GetAnimationEvents(clip).Count(e=>e.functionName=="AbilityBeat"&&e.intParameter==1),Is.EqualTo(1));
            Assert.That(AnimationUtility.GetAnimationEvents(clip).Count(e=>e.functionName=="AbilityComplete"),Is.EqualTo(1));
        }
    }
}
