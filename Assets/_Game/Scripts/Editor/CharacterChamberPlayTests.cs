using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using TMPro;
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
                Assert.AreSame(definition.BattleCharacterSprite,Find<Image>(panel,"CharacterPreview").sprite);
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
            Assert.NotNull(Find<Image>(panel,"Artwork").sprite);
            string first=Find<TMP_Text>(panel,"CardTitle").text;
            Click("NextCards");yield return null;
            Assert.AreNotEqual(first,Find<TMP_Text>(panel,"CardTitle").text);
            if(rendered) yield return Shot(panel,"cards-720x1280");
            Click("CharactersTab");Click("SpecialGemsTab");yield return null;
            Assert.IsNotNull(Object.FindFirstObjectByType<GemMasteryMenuController>());
            var main=Object.FindFirstObjectByType<MainMenuController>();
            var back=(Button)typeof(MainMenuController).GetField("gemMasteryBackButton",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(main);
            back.onClick.Invoke();yield return null;
            Assert.True(panel.gameObject.activeInHierarchy);
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

    private static IEnumerator Shot(Component panel,string name)
    {
        yield return null; yield return null;
        Canvas.ForceUpdateCanvases();
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
