using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public sealed partial class ForestFoundationPlayTests
{
    [UnityTest] public IEnumerator FrostwakeSceneryIceAndMinotaurFitPortraits()
    {
        using var player=CharacterSelectionSettings.UseTemporarySelection("dungeon_attender");
        yield return LaunchFrostwakeKit("jarl_haldra","rimecarver","frostfang_wolf");
        var owner=Enemy("jarl_haldra");
        foreach(var stage in new[]{FrostStage.Frost,FrostStage.HeavyIce,FrostStage.Bifrost})
        {Assert.That(Run.Board.TryQueueEncasedGem(owner,stage,_=>{}),Is.True);yield return Stable();}
        int variant=0;
        foreach(var size in new[]{new Vector2Int(1080,1920),new Vector2Int(1080,2400),new Vector2Int(720,1280)})
        {
            Object.FindFirstObjectByType<BattleBackgroundTilemapController>().ApplyGameplayTheme(Run.Zone.Definition.theme,variant++);
            typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{size});
            yield return Until(()=>Screen.width==size.x&&Screen.height==size.y,"Frostwake portrait");
            for(int n=0;n<12;n++)yield return null;
            var errors=GameplayPixelLayoutValidator.Validate(Object.FindFirstObjectByType<GameplayPixelLayoutController>(),out string report);
            Assert.That(errors,Is.Empty,report);
            foreach(var actor in Run.Waves.ActiveEnemies)
            {
                var row=actor.GetComponent<EnemyUnifiedIntentView>().ActionRoot;var slot=(RectTransform)row.parent;
                Assert.That(row.localPosition.x-row.rect.width/2,Is.GreaterThanOrEqualTo(slot.rect.xMin));
                Assert.That(row.localPosition.x+row.rect.width/2,Is.LessThanOrEqualTo(slot.rect.xMax));
            }
            Directory.CreateDirectory(".utmp/Frostwake/Screens");
            ScreenCapture.CaptureScreenshot(Path.GetFullPath(".utmp/Frostwake/Screens/scene-"+variant+"-"+size.x+"x"+size.y+".png"));
            yield return null;yield return null;
        }
    }
    [UnityTest] public IEnumerator FrostwakeAuthoredRosterBasicsCompleteAndPortraitHasNativeSprites()
    {
        string[][] casts={new[]{"iceborn_raider","frostfang_wolf","harpoon_hunter"},new[]{"rimecarver","shieldmaiden","frostblade_reaver"},
            new[]{"rime_skald","hearthkeeper","iceborn_raider"},new[]{"glacier_troll","iceborn_champion","jarl_haldra"}};
        foreach(var cast in casts)
        {
            yield return LaunchFrostwakeKit(cast);
            Assert.That(ZoneTravelController.DestinationReady(Run.Zone.Definition),Is.True);
            Run.Player.GrantShield(1000);Set(Run.Player,"currentHealth",9995);
            foreach(var actor in Run.Waves.ActiveEnemies)
            {
                var animator=actor.transform.Find("VisualRoot").GetComponent<Animator>();
                foreach(string state in new[]{"Idle","AutoAttack","Hit","Death"})Assert.That(animator.HasState(0,Animator.StringToHash("Base Layer."+state)),Is.True,actor.Definition.EnemyId+" "+state);
                Set(actor.GetComponent<EnemyAutoAttack>(),"unifiedRemaining",1);
            }
            yield return EnvironmentMove();
            foreach(var actor in Run.Waves.ActiveEnemies)
            {
                Assert.That(actor.HasAnimationActionInProgress,Is.False);
                var sprite=actor.transform.Find("VisualRoot").GetComponent<Image>().sprite;
                Assert.That(sprite.rect.size,Is.EqualTo(new Vector2(128,104)));
                Assert.That(sprite.texture.filterMode,Is.EqualTo(FilterMode.Point));Assert.That(sprite.pivot.y,Is.EqualTo(4).Within(.01));
            }
            Directory.CreateDirectory(".utmp/Frostwake/Screens");
            // File capture uses the rendered Game view in the batch editor;
            // CaptureScreenshotAsTexture can return an unpresented black buffer.
            yield return null;ScreenCapture.CaptureScreenshot(Path.GetFullPath(".utmp/Frostwake/Screens/"+cast[0]+".png"));
            yield return null;yield return null;
        }
    }
    [UnityTest] public IEnumerator FrostwakeImportedSupportsApplyShieldRallyHealAndFrostbite()
    {
        yield return LaunchFrostwakeKit("shieldmaiden","iceborn_raider");
        yield return ReadyFrostwake(Enemy("shieldmaiden"));
        Assert.That(Enemy("shieldmaiden").CurrentShield,Is.EqualTo(15));Assert.That(Enemy("iceborn_raider").CurrentShield,Is.EqualTo(10));
        yield return LaunchFrostwakeKit("rime_skald","iceborn_raider");
        yield return ReadyFrostwake(Enemy("rime_skald"));Assert.That(RosterSpeed(Enemy("iceborn_raider")),Is.GreaterThan(1));
        yield return LaunchFrostwakeKit("hearthkeeper","iceborn_raider");var ally=Enemy("iceborn_raider");Set(ally,"currentHealth",20);
        yield return ReadyFrostwake(Enemy("hearthkeeper"));var heal=Enemy("hearthkeeper").GetComponent<EnemyChannelRuntime>();
        Assert.That(heal.IsChanneling,Is.True);int before=ally.CurrentHealth;
        yield return EnvironmentMove();yield return EnvironmentMove();Assert.That(heal.Outcome,Is.EqualTo("Healed"));
        Assert.That(ally.CurrentHealth,Is.EqualTo(before+20));
        yield return LaunchFrostwakeKit("frostblade_reaver");yield return ReadyFrostwake(Enemy("frostblade_reaver"));
        Assert.That(Run.Player.Statuses.Has(PlayerStatusKind.Frostbite),Is.True);
    }
}
