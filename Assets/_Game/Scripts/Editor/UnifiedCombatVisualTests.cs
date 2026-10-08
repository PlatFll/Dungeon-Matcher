using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

public sealed class UnifiedCombatAssetTests
{
    [Test] public void AllStatusArtIsNativeAndUncompressed()
    {
        var icons=Resources.LoadAll<Sprite>("UI/CombatStatuses");
        Assert.That(icons.Length,Is.EqualTo(23));
        foreach(var icon in icons)
        {
            Assert.That(icon.rect.size,Is.EqualTo(new Vector2(24,24)),icon.name);
            var importer=(TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(icon));
            Assert.That(importer.filterMode,Is.EqualTo(FilterMode.Point));
            Assert.That(importer.mipmapEnabled,Is.False);
            Assert.That(importer.textureCompression,Is.EqualTo(TextureImporterCompression.Uncompressed));
        }
        var defs=AssetDatabase.FindAssets("t:EnemyDefinition",new[]{"Assets/_Game"})
            .Select(g=>AssetDatabase.LoadAssetAtPath<EnemyDefinition>(AssetDatabase.GUIDToAssetPath(g))).ToArray();
        Assert.That(defs.Length,Is.EqualTo(62));
        Assert.That(defs.Select(d=>d.UnifiedAttackMoves).Distinct().Count(),Is.GreaterThanOrEqualTo(5));
        foreach(var def in defs) Assert.That(def.UnifiedAttackMoves,Is.InRange(2,6),def.name);
    }
}

public sealed partial class ForestFoundationPlayTests
{
    [UnityTest] public IEnumerator UnifiedTwoChannelsResumeIndependentlyAndKeepTargetLinks()
    {
        yield return LaunchUnified();
        var data=Resources.Load<ZoneDefinition>("Zones/magical-forest").enemies.First(d=>d.EnemyId=="elven_mender");
        Assert.That(Run.Waves.TrySummonEnemy(data,out var first),Is.True);
        Assert.That(Run.Waves.TrySummonEnemy(data,out var second),Is.True);
        yield return Stable();DurableUnifiedFixture();
        var target=Run.Waves.ActiveEnemies[0];Set(target,"currentHealth",5000);
        first.SetSpecialTurnRequirement(1);second.SetSpecialTurnRequirement(1);
        yield return Move();
        foreach(var actor in new[]{first,second})
        {
            Assert.That(actor.GetComponent<EnemyChannelRuntime>().ResponseMoves,Is.EqualTo(2));
            Assert.That(actor.GetComponent<EnemyAutoAttack>().IsPausedByAction,Is.True);
        }
        yield return ResumeRoster();
        var channels=Run.Waves.ActiveEnemies.Where(e=>e.Definition==data).ToArray();
        Assert.That(channels.Length,Is.EqualTo(2));
        foreach(var actor in channels)
            Assert.That(actor.GetComponent<EnemyChannelRuntime>().ResponseMoves,Is.EqualTo(2));
        yield return null;
        foreach(var actor in channels)
        {
            var view=actor.GetComponent<EnemyUnifiedIntentView>();
            Assert.That(view.ActionRoot.Find("SpecialMoves").GetComponent<PixelCounterText>().Face.text,Is.EqualTo("2"));
            Assert.That(view.ActionRoot.Find("ImpactUnderline").GetComponent<UnityEngine.UI.Image>().enabled,Is.True);
        }
        Directory.CreateDirectory(".utmp/UnifiedCombat/Visual");
        ScreenCapture.CaptureScreenshot(".utmp/UnifiedCombat/Visual/two-healer-channels.png");yield return null;yield return null;
        Set(channels[0].GetComponent<EnemyStagger>(),"remainingImmunityTime",0f);
        yield return Move(()=>channels[0].GetComponent<EnemyStagger>().ApplyStagger(2,2));
        Assert.That(channels[0].GetComponent<EnemyChannelRuntime>().Outcome,Is.EqualTo("Interrupted"));
        Assert.That(channels[1].GetComponent<EnemyChannelRuntime>().ResponseMoves,Is.EqualTo(1));
        yield return Move();Assert.That(channels[1].GetComponent<EnemyChannelRuntime>().Outcome,Is.EqualTo("Healed"));
    }

    [UnityTest] public IEnumerator UnifiedLeaderHudAndAnnouncementStayInsidePortrait()
    {
        yield return LaunchMineKit("grand_delver","bore_engineer","powder_sapper");DurableUnifiedFixture();
        var boss=Enemy("grand_delver");
        var poison=boss.GetComponent<EnemyPoisonStatus>()??boss.gameObject.AddComponent<EnemyPoisonStatus>();poison.Apply(3,1,5);
        var announcement=boss.GetComponent<EnemyCastAnnouncement>()??boss.gameObject.AddComponent<EnemyCastAnnouncement>();
        foreach(var size in new[]{new Vector2Int(720,1280),new Vector2Int(1080,1920),new Vector2Int(1080,2400)})
        {
            typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{size});
            yield return Until(()=>Screen.width==size.x && Screen.height==size.y,"leader viewport");
            for(int n=0;n<8;n++)yield return null;
            announcement.Show("Catastrophic Excavation");yield return null;
            var view=boss.GetComponent<EnemyUnifiedIntentView>();var corners=new Vector3[4];view.ActionRoot.GetWorldCorners(corners);
            foreach(var p in corners)Assert.That(new Rect(0,0,Screen.width,Screen.height).Contains(p),Is.True,"leader action display fits viewport");
            var wave=Object.FindFirstObjectByType<GameplayPixelLayoutController>().TopHud.Find("WaveTracker") as RectTransform;
            var plaque=new Vector3[4];wave.GetWorldCorners(plaque);
            foreach(var label in boss.GetComponentInParent<EnemySlotUI>().GetComponentsInChildren<TMP_Text>())
                if(label.name=="CastAnnouncement")
                {
                    label.rectTransform.GetWorldCorners(corners);
                    Assert.That(corners[1].y,Is.LessThan(plaque[0].y),"announcement stays below wave plaque");
                }
            ScreenCapture.CaptureScreenshot(".utmp/UnifiedCombat/Visual/leader-"+size.x+"x"+size.y+".png");yield return null;yield return null;
        }
    }

    [UnityTest] public IEnumerator UnifiedStatusAndActionDisplayFitsFourZonesAndPortraits()
    {
        Assert.That(SystemInfo.graphicsDeviceType,Is.Not.EqualTo(UnityEngine.Rendering.GraphicsDeviceType.Null));
        string output=Path.GetFullPath(".utmp/UnifiedCombat/Visual");Directory.CreateDirectory(output);
        foreach(string zone in new[]{"dungeon","magical-forest","drowned-court","ironvein-excavation"})
        {
            yield return LaunchUnified(zone);DurableUnifiedFixture();
            string[] fill=zone=="dungeon"?new[]{"shield_knight","court_mage"}:zone=="magical-forest"?new[]{"elven_mender","elven_thornkeeper"}:
                zone=="drowned-court"?new[]{"conch_marshal","reef_netweaver"}:new[]{"bore_engineer","powder_sapper"};
            var roster=AssetDatabase.FindAssets("t:EnemyDefinition",new[]{"Assets/_Game"}).Select(g=>AssetDatabase.LoadAssetAtPath<EnemyDefinition>(AssetDatabase.GUIDToAssetPath(g)));
            foreach(string id in fill) if(Run.Waves.ActiveEnemies.Count<3)Assert.That(Run.Waves.TrySummonEnemy(roster.First(d=>d.EnemyId==id),out _),Is.True);
            yield return Stable();DurableUnifiedFixture();
            foreach(var data in Resources.LoadAll<PlayerStatusDefinition>("PlayerStatuses")) Run.Player.Statuses.Apply(data,Run.Waves.ActiveEnemies[0]);
            Run.Player.GrantShield(30);
            Run.Player.GetComponent<PlayerAbilityEnergy>().AddEnergy(100);
            Run.Player.GetComponent<PlayerAbilityController>().TryActivate();
            foreach(var actor in Run.Waves.ActiveEnemies)
            {
                // Keep Fear's source active; Stagger correctly cleanses its Fear.
                if(actor!=Run.Waves.ActiveEnemies[0])actor.GetComponent<EnemyStagger>().ApplyStagger(2,2);
                var poison=actor.GetComponent<EnemyPoisonStatus>()??actor.gameObject.AddComponent<EnemyPoisonStatus>();
                poison.Apply(3,1,5);
            }
            yield return new WaitForSeconds(.8f);
            int variant=0;
            foreach(var size in new[]{new Vector2Int(720,1280),new Vector2Int(1080,1920),new Vector2Int(1080,2400),new Vector2Int(1080,1920)})
            {
                bool inset=variant++==3;
                typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{size});
                GameplayPixelLayoutController.ValidationSafeArea=inset?new Rect(32,60,1016,1772):(Rect?)null;
                yield return Until(()=>Screen.width==size.x && Screen.height==size.y,"unified viewport");
                for(int n=0;n<8;n++)yield return null;
                string name=zone+"-"+size.x+"x"+size.y+(inset?"-safe":"");
                ScreenCapture.CaptureScreenshot(Path.Combine(output,name+".png"));yield return null;yield return null;
                var layout=Object.FindFirstObjectByType<GameplayPixelLayoutController>();
                var errors=GameplayPixelLayoutValidator.Validate(layout,out string report);
                File.WriteAllText(Path.Combine(output,name+".txt"),report+"\n"+string.Join("\n",errors));
                Assert.That(errors,Is.Empty,name);
                foreach(var actor in Run.Waves.ActiveEnemies)
                {
                    var view=actor.GetComponent<EnemyUnifiedIntentView>();Assert.That(view,Is.Not.Null);
                    Assert.That(view.ActionRoot.Find("AttackMoves").GetComponent<PixelCounterText>().Face.text,Does.Not.Contain("s"));
                    Assert.That(actor.GetComponentInParent<EnemySlotUI>().transform.Find("MoveIntent"),Is.Null);
                    Assert.That(view.Statuses.VisibleCount,Is.GreaterThanOrEqualTo(1));
                    var corners=new Vector3[4];view.ActionRoot.GetWorldCorners(corners);
                    // Large animation canvases include transparent headroom; the
                    // header clamp may use it. Require viewport/header clearance,
                    // then inspect the captured actual sprites for face clearance.
                    foreach(var p in corners)Assert.That(new Rect(0,0,Screen.width,Screen.height).Contains(p),Is.True);
                    var wave=(RectTransform)layout.TopHud.Find("WaveTracker");var header=new Vector3[4];wave.GetWorldCorners(header);
                    view.Statuses.Rect.GetWorldCorners(corners);
                    Assert.That(corners[1].y,Is.LessThan(header[0].y),"status rows stay below wave plaque");
                }
                var player=Object.FindFirstObjectByType<PlayerStatusPanel>();
                Assert.That(player.StatusRoot.GetComponent<CombatStatusStrip>().VisibleCount,Is.EqualTo(9));
                var bounds=new Vector3[4];player.StatusRoot.GetWorldCorners(bounds);
                foreach(var p in bounds) Assert.That(((RectTransform)player.transform).rect.Contains(player.transform.InverseTransformPoint(p)),Is.True,"Player icons fit panel: "+player.transform.InverseTransformPoint(p)+" in "+((RectTransform)player.transform).rect);
            }
            GameplayPixelLayoutController.ValidationSafeArea=null;
        }
    }
}
