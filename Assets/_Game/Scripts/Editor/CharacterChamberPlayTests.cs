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
using Object = UnityEngine.Object;

public sealed class CharacterChamberPlayTests
{
    private const string Output = ".utmp/CharacterChamber";
    private static void Size(int width,int height)
    {
        typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.Static|BindingFlags.NonPublic)
            .Invoke(null,new object[]{new Vector2Int(width,height)});
    }

    [UnityTest]
    public IEnumerator BrowseUpgradeAndNavigateFromMainMenu()
    {

        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        yield return new EnterPlayMode();
        bool rendered = SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Null;
        if (rendered) Size(720,1280);
        Directory.CreateDirectory(Output);
        string savePath = Path.GetFullPath(Path.Combine(Output,Guid.NewGuid()+".json"));
        var save = new AccountSave { gold = 2450 };
        save.discoveredCards = new List<string>(new[] { "prototype_gem_grinder", "prototype_thicker_hide", "glass_cannon",
            "aegis_reservoir", "cascade_catalyst", "boss_hunter", "opening_volley", "chromatic_conductor",
            "chain_reaction", "emergency_plating", "longer_reign", "final_word" });
        string[] ids = {"skeleton","bardley","kobold_gunner","dungeon_attender"};
        string[] buttons = {"RattlebonesButton","BardleyButton","KoboldButton","MinotaurButton"};
        for(int i=0;i<ids.Length;i++) save.characters.Add(new CharacterProgress {id=ids[i],level=8+i});
        File.WriteAllText(savePath,JsonUtility.ToJson(save));
        using(AccountProgression.UseDisposableProfile(savePath))
        using(CharacterSelectionSettings.UseTemporarySelection("skeleton"))
        {
            SceneManager.LoadScene("MainMenu"); yield return null; yield return null;
            Click("CharactersButton"); yield return null;
            var panel = Object.FindFirstObjectByType<CharacterSelectMenuController>();
            Assert.NotNull(panel);
            var gems = Resources.Load<EnemyWeaknessIndicatorConfig>("UI/EnemyWeaknessIndicatorConfig");
            for(int i=0;i<ids.Length;i++)
            {
                Click(buttons[i]); yield return null;
                Assert.AreEqual(ids[i],CharacterSelectionSettings.SelectedPlayerId);
                Assert.True(PlayerDefinitionRegistry.TryGetDefinition(ids[i],out var definition));
                Assert.AreSame(definition.BattleCharacterSprite,Find<Image>(Find<Button>(panel,buttons[i]),"Portrait").sprite);
                yield return VerifyIdle(panel,definition,rendered);
                Assert.AreSame(gems.GetSprite(definition.AffinityGemType),Find<Image>(panel,"Icon2").sprite);
                Assert.AreEqual(definition.ActiveAbility.EnergyCost.ToString(),Find<TMP_Text>(panel,"Next4").text);
                if(rendered) yield return Shot(panel,ids[i]+"-720x1280");
            }
            Click(buttons[0]);
            int balance=AccountProgression.Current.Gold;
            int cost=BalanceV1.Current.UpgradeCost(8);
            Click("LevelUp");yield return null;
            Assert.AreEqual(9,AccountProgression.Current.Level("skeleton"));
            Assert.AreEqual(balance-cost,AccountProgression.Current.Gold);
            Assert.AreEqual(AccountProgression.Current.Gold.ToString("N0"),Find<TMP_Text>(panel,"Gold").text);
            Click("CardsTab");yield return null;
            Assert.AreEqual("CARDS",Find<TMP_Text>(panel,"ScreenTitle").text);
            Assert.IsNull(GameObject.Find("CharacterPreview"),"Stats preview should be hidden in the library");
            Assert.NotNull(Find<Image>(panel,"Artwork").sprite);
            var seen = Resources.Load<RunUpgradeCatalog>("RunUpgrades/PrototypeRunUpgradeCatalog").Upgrades
                .Where(card => AccountProgression.Current.HasDiscoveredCard(card.UpgradeId)).ToArray();
            string[] RowNames() => panel.GetComponentsInChildren<TMP_Text>().Where(label=>label.name=="CardTitle")
                .Select(label=>label.text).ToArray();
            CollectionAssert.AreEqual(seen.OrderBy(card=>card.Rarity).Select(card=>card.DisplayTitle.ToUpperInvariant()), RowNames());
            var scroll = Find<ScrollRect>(panel,"JournalViewport");
            Assert.Greater(scroll.content.rect.height,scroll.viewport.rect.height);
            scroll.verticalNormalizedPosition = 0; yield return null;
            Assert.Less(scroll.verticalNormalizedPosition,.01f);
            Click("SortCards"); yield return null;
            CollectionAssert.AreEqual(seen.OrderBy(card=>card.DisplayTitle,StringComparer.Ordinal)
                .Select(card=>card.DisplayTitle.ToUpperInvariant()), RowNames());
            Click("SortCards"); yield return null;
            if(rendered) yield return Shot(panel,"cards-720x1280");
            Click("SpecialGemsTab"); yield return null;
            Assert.AreEqual("SPECIAL GEMS",Find<TMP_Text>(panel,"ScreenTitle").text);
            foreach (string shape in new[] {"StraightFive","TShape","LShape","CrossShape"})
            {
                // The summary diagram is decorative and shares the shape name.
                Find<Button>(panel,shape).onClick.Invoke();
                Assert.AreEqual(5,Find<Button>(panel,shape).GetComponentsInChildren<Image>().Count(i=>i.name=="Gem"));
            }
            Find<Button>(panel,"TShape").onClick.Invoke();
            Click("AssignPoisonBomb");
            Assert.AreEqual(GemMasteryReward.PoisonBomb,GemMasterySettings.GetReward(GemMasteryShape.TShape));
            Assert.AreEqual(GemMasteryReward.ColorCrystal,GemMasterySettings.GetReward(GemMasteryShape.CrossShape));
            if(rendered) yield return Shot(panel,"gems-720x1280");
            Click("CharactersTab"); Click("BardleyButton"); Click("SpecialGemsTab");
            Assert.AreEqual(GemMasteryReward.ColorCrystal,GemMasterySettings.GetReward(GemMasteryShape.TShape));
            Click("AssignShieldBomb");
            Click("BackButton"); Click("RattlebonesButton"); Click("SpecialGemsTab");
            Assert.AreEqual(GemMasteryReward.PoisonBomb,GemMasterySettings.GetReward(GemMasteryShape.TShape));
            Click("CardsTab");
            if (rendered)
            {
                Size(1080,2340); yield return null; yield return Shot(panel,"cards-tall");
                Click("SpecialGemsTab"); yield return Shot(panel,"gems-tall");
                Size(360,640); yield return null; yield return Shot(panel,"gems-compact");
            }
            Click("CharactersTab");
            if(rendered)
            {
                Size(1080,2340); yield return null; yield return null;
                yield return Shot(panel,"skeleton-1080x2340");
                Size(360,640); yield return null; yield return null;
                yield return Shot(panel,"skeleton-360x640");
                // Simulate a top cutout and bottom gesture inset in the safe-area container.
                var safe=panel.GetComponentsInChildren<RectTransform>().Single(rt=>rt.name=="CharacterSafeArea");
                safe.anchorMin=new Vector2(.035f,.045f);safe.anchorMax=new Vector2(.965f,.95f);
                yield return null; yield return null;
                yield return Shot(panel,"skeleton-safe-insets");
                Click("CardsTab"); yield return Shot(panel,"cards-safe-insets");
                Click("SpecialGemsTab"); yield return Shot(panel,"gems-safe-insets");
                Click("CharactersTab");
            }
            var account = AccountProgression.Current;
            int savedLevel = account.Level("skeleton");
            int savedGold = account.Gold;
            account.BeginRun("skeleton");
            Assert.False(Find<Button>(panel,"LevelUp").IsInteractable());
            Assert.False(Find<Button>(panel,"ResetLevel").IsInteractable());
            Assert.AreEqual(savedGold,account.Gold);
            Assert.AreEqual(savedLevel,account.Level("skeleton"));
            Click("BackButton");yield return null;
            Assert.True(GameObject.Find("CharactersButton").activeInHierarchy);
        }
        yield return new ExitPlayMode();
    }

    private static GemMasteryLoadout BuildFor(string id)
    {
        using (CharacterSelectionSettings.UseTemporarySelection(id)) return GemMasterySettings.Current;
    }

    [Test]
    public void IndependentBuildsAndVisibleCardsSurviveReload()
    {
        Directory.CreateDirectory(Output);
        string path = Path.GetFullPath(Path.Combine(Output, Guid.NewGuid() + ".json"));
        var inherited = new List<GemMasteryReward> { GemMasteryReward.PoisonBomb, GemMasteryReward.ColorCrystal,
            GemMasteryReward.ColorCrystal, GemMasteryReward.ColorCrystal };
        var save = new AccountSave { inheritedMastery = inherited };
        save.characters.Add(new CharacterProgress {id="skeleton", level=7});
        File.WriteAllText(path, JsonUtility.ToJson(save));
        var cards = Resources.Load<RunUpgradeCatalog>("RunUpgrades/PrototypeRunUpgradeCatalog").Upgrades.Take(3).ToArray();
        using (CharacterSelectionSettings.UseTemporarySelection("skeleton"))
        {
            using (AccountProgression.UseDisposableProfile(path))
            {
                var profile = AccountProgression.Current;
                foreach (string id in new[] {"skeleton","bardley","kobold_gunner","dungeon_attender"})
                    Assert.AreEqual(GemMasteryReward.PoisonBomb,BuildFor(id).StraightFive);
                Assert.True(GemMasterySettings.SetReward(GemMasteryShape.StraightFive,GemMasteryReward.ColorCrystal));
                CharacterSelectionSettings.SetSelectedPlayerId("bardley");
                Assert.True(GemMasterySettings.SetReward(GemMasteryShape.CrossShape,GemMasteryReward.ShieldBomb));
                // Journal the specified owner even while another character is being browsed.
                Assert.NotNull(profile.BeginRun("skeleton"));
                Assert.AreEqual(GemMasteryReward.ColorCrystal,profile.ActiveRun.mastery[0]);
                Assert.True(profile.RememberCardOffer(cards,2));
                Assert.True(profile.RememberCardOffer(cards,2));
                using (AccountProgression.UsePracticeProfile())
                {
                    var practice = AccountProgression.Current;
                    practice.BeginRun("bardley");
                    practice.RememberCardOffer(cards,3);
                }
            }
            using (AccountProgression.UseDisposableProfile(path))
            {
                var profile = AccountProgression.Current;
                Assert.True(profile.HasDiscoveredCard(cards[0].UpgradeId));
                Assert.True(profile.HasDiscoveredCard(cards[1].UpgradeId));
                Assert.False(profile.HasDiscoveredCard(cards[2].UpgradeId));
                Assert.AreEqual(GemMasteryReward.PoisonBomb,BuildFor("bardley").StraightFive);
                Assert.AreEqual(GemMasteryReward.ShieldBomb,BuildFor("bardley").CrossShape);
                Assert.AreEqual(GemMasteryReward.ColorCrystal,BuildFor("skeleton").StraightFive);
                Assert.AreEqual(GemMasteryReward.ColorCrystal,profile.ActiveRun.mastery[3]);
            }
        }
    }

    [UnityTest]
    public IEnumerator InsufficientGoldAndMaximumLevelHaveClearStates()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        yield return new EnterPlayMode();
        Directory.CreateDirectory(Output);
        string path = Path.GetFullPath(Path.Combine(Output,Guid.NewGuid()+".json"));
        var save = new AccountSave { gold = 0 };
        save.characters.Add(new CharacterProgress { id="bardley", level=BalanceV1.Current.levelCap });
        File.WriteAllText(path,JsonUtility.ToJson(save));
        using(AccountProgression.UseDisposableProfile(path))
        using(CharacterSelectionSettings.UseTemporarySelection("skeleton"))
        {
            SceneManager.LoadScene("MainMenu");yield return null;yield return null;
            Click("CharactersButton");yield return null;
            var menu = Object.FindFirstObjectByType<CharacterSelectMenuController>();
            Assert.False(Find<Button>(menu,"LevelUp").IsInteractable());
            StringAssert.Contains("more gold",Find<TMP_Text>(menu,"UnlockFeedback").text);
            Click("BardleyButton");yield return null;
            Assert.False(Find<Button>(menu,"LevelUp").IsInteractable());
            Assert.AreEqual("MAX LEVEL",Find<TMP_Text>(menu,"GoldCost").text);
            Assert.AreEqual("",Find<TMP_Text>(menu,"Next0").text);
            Assert.AreEqual(0,AccountProgression.Current.Gold);
            Click("CardsTab");
            Assert.True(Find<RectTransform>(menu,"EmptyJournal").gameObject.activeInHierarchy);
            Assert.False(menu.GetComponentsInChildren<TMP_Text>().Any(label=>label.name=="CardTitle"));
        }
        yield return new ExitPlayMode();
    }

    private static T Find<T>(Component root,string name) where T:Component =>
        root.GetComponentsInChildren<T>().First(c=>c.name==name);

    private static void Click(string name)
    {
        var button=GameObject.Find(name).GetComponent<Button>();
        Assert.True(button.IsInteractable(),name);
        button.onClick.Invoke();
    }

    private static void CheckFooting(Component panel, Image hero)
    {
        var chamber = Find<RectTransform>(panel,"TorchlitChamber");
        var foot = chamber.InverseTransformPoint(hero.rectTransform.position);
        float row = (chamber.rect.yMax-foot.y) / chamber.rect.height * 192;
        Assert.That(row,Is.InRange(165f,177f),"Feet must be inside the top face of the painted dais");
        Assert.AreEqual(hero.sprite.pivot / hero.sprite.rect.size,hero.rectTransform.pivot);
        Assert.AreEqual(hero.sprite.rect.size*2,hero.rectTransform.sizeDelta);
    }

    private static IEnumerator VerifyIdle(Component panel, PlayerDefinition player, bool rendered)
    {
        var hero = Find<Image>(panel,"CharacterPreview");
        var animator = hero.GetComponent<Animator>();
        Assert.AreSame(player.BattleAnimatorController,animator.runtimeAnimatorController);
        yield return null;
        var clip = animator.GetCurrentAnimatorClipInfo(0).Single().clip;
        Assert.True(clip.isLooping);
        var binding = AnimationUtility.GetObjectReferenceCurveBindings(clip)
            .Single(b => b.type == typeof(Image) && b.propertyName == "m_Sprite");
        var keys = AnimationUtility.GetObjectReferenceCurve(clip,binding);
        Assert.AreSame(player.BattleCharacterSprite,keys[0].value);
        var authored = new HashSet<Sprite>(keys.Select(key => (Sprite)key.value));
        var seen = new HashSet<Sprite>();
        float start = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        float deadline = Time.realtimeSinceStartup + 10;
        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime-start < 1)
        {
            Assert.Less(Time.realtimeSinceStartup,deadline,"Idle did not loop while browsing " + player.DisplayName);
            Assert.True(authored.Contains(hero.sprite),"Unexpected frame for " + player.DisplayName);
            seen.Add(hero.sprite);
            CheckFooting(panel,hero);
            yield return null;
        }
        Assert.Greater(seen.Count, 2, "The displayed sprite stayed on a single pose");
        if (!rendered) yield break;

        // Freeze the already verified live Animator at each authored key for visual review.
        int state = animator.GetCurrentAnimatorStateInfo(0).fullPathHash;
        string folder = Path.Combine("Idles",player.PlayerId);
        Directory.CreateDirectory(Path.Combine(Output,folder));
        animator.speed = 0;
        try
        {
            for (int i=0;i<keys.Length;i++)
            {
                if (i>0 && keys[i].value==keys[i-1].value) continue;
                float end = i+1<keys.Length ? keys[i+1].time : clip.length;
                animator.Play(state,0,(keys[i].time+end)*.5f/clip.length);
                animator.Update(0);
                Assert.AreSame(keys[i].value,hero.sprite);
                yield return Shot(panel,Path.Combine(folder,i.ToString("00")));
            }
        }
        finally { animator.speed = 1; }
    }

    private static IEnumerator Shot(Component panel,string name)
    {
        yield return null; yield return null;
        Canvas.ForceUpdateCanvases();
        var hero = panel.GetComponentsInChildren<Image>().FirstOrDefault(i => i.name=="CharacterPreview");
        if (hero != null) CheckFooting(panel,hero);
        foreach(var label in panel.GetComponentsInChildren<TMP_Text>())
        {
            label.ForceMeshUpdate();
            Assert.False(label.isTextTruncated,label.name + " truncated: " + label.text);
        }
        string path=Path.GetFullPath(Path.Combine(Output,name+".png"));
        var previous = File.Exists(path) ? File.GetLastWriteTimeUtc(path) : DateTime.MinValue;
        ScreenCapture.CaptureScreenshot(path);
        float deadline = Time.realtimeSinceStartup + 25;
        while (!File.Exists(path) || File.GetLastWriteTimeUtc(path) <= previous || new FileInfo(path).Length == 0)
        {
            Assert.Less(Time.realtimeSinceStartup,deadline,"Screenshot was not written: " + path);
            yield return null;
        }
    }
}
