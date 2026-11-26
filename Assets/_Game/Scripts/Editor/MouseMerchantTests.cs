using System;
using System.Collections;
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

public sealed class MouseMerchantTests
{
    private const string Output = ".utmp/MouseMerchant";
    private IDisposable profile;
    private static IEnumerator Until(Func<bool> ready)
    {
        float deadline = Time.realtimeSinceStartup + 90;
        while (!ready()) { Assert.Less(Time.realtimeSinceStartup,deadline); yield return null; }
    }

    private static IEnumerator Capture(string path)
    {
        var previous = File.Exists(path) ? File.GetLastWriteTimeUtc(path) : DateTime.MinValue;
        ScreenCapture.CaptureScreenshot(Path.GetFullPath(path));
        yield return Until(() => File.Exists(path) && File.GetLastWriteTimeUtc(path) > previous && new FileInfo(path).Length > 0);
    }

    private static string Seed(AccountSave data)
    {
        Directory.CreateDirectory(Output);
        var path=Path.GetFullPath(Path.Combine(Output,Guid.NewGuid()+".json"));
        File.WriteAllText(path,JsonUtility.ToJson(data));
        return path;
    }

    private static void Size(int w,int h) => typeof(GameplayPixelLayoutTests)
        .GetMethod("SetGameViewSize",BindingFlags.NonPublic|BindingFlags.Static)
        .Invoke(null,new object[]{new Vector2Int(w,h)});

    [UnityTearDown] public IEnumerator Cleanup()
    {
        Time.timeScale=1;
        profile?.Dispose(); profile=null;
        if(Application.isPlaying) yield return new ExitPlayMode();
    }

    [Test] public void ShopArtLoadsAsPointFilteredSprites()
    {
        var paths = Directory.GetFiles("Assets/_Game/Resources/UI/MerchantStall", "*.png");
        Assert.AreEqual(25, paths.Length);
        foreach (string file in paths)
        {
            string path=file.Replace((char)92,'/');
            string name=Path.GetFileNameWithoutExtension(path);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            string details=name + " resourceTexture=" + Resources.Load<Texture2D>("UI/MerchantStall/"+name) + " all=" + Resources.LoadAll("UI/MerchantStall").Length + " knownSprite=" + Resources.Load<Sprite>("UI/Consumables/Potion") + " " + importer.textureType + " " + importer.spriteImportMode + " " +
                string.Join(", ", AssetDatabase.LoadAllAssetsAtPath(path).Select(asset=>asset.GetType().Name));
            var sprite=Resources.Load<Sprite>("UI/MerchantStall/"+name);
            Assert.IsNotNull(sprite,details);
            Assert.AreEqual(FilterMode.Point,sprite.texture.filterMode,details);
            Assert.AreEqual(1,sprite.texture.mipmapCount,details);
        }
    }

    [Test] public void FiveInventoriesShareTwoSlotsAndKeepTheirIdentityOnReload()
    {
        string path=Seed(new AccountSave {gold=200,potions=2,bombs=2,equipPotions=true,equipBombs=true});
        var account=new AccountProgression(path);
        foreach(var kind in SupplyCatalog.DisplayOrder) Assert.IsTrue(account.TryPurchase(kind));
        Assert.AreEqual(200-SupplyCatalog.DisplayOrder.Sum(SupplyCatalog.Price),account.Gold);
        Assert.IsTrue(account.SetEquipped(ConsumableKind.ShieldPotion,true));
        Assert.IsTrue(account.SetEquipped(ConsumableKind.MegaPotion,true));
        Assert.IsFalse(account.Equipped(ConsumableKind.ShieldPotion));
        Assert.IsTrue(account.SetEquipped(ConsumableKind.ColorBomb,true));
        Assert.IsFalse(account.Equipped(ConsumableKind.Bomb));
        var id=account.BeginRun("skeleton");
        account=new AccountProgression(path);
        Assert.AreEqual(1,account.Charges(id,ConsumableKind.MegaPotion));
        Assert.AreEqual(0,account.Charges(id,ConsumableKind.HealthPotion));
        Assert.IsFalse(account.TryPurchase(ConsumableKind.HealthPotion));
        Assert.IsTrue(account.TrySpendCharge(id,ConsumableKind.ColorBomb));
        Assert.IsFalse(account.TrySpendCharge(id,ConsumableKind.ColorBomb));
        Assert.AreEqual(3,account.Owned(ConsumableKind.Bomb));
        Assert.AreEqual(SupplyCatalog.Price(ConsumableKind.ColorBomb),account.PreviewReward("test").supplyValue);
    }

    [UnityTest] public IEnumerator PortraitShopScrollPurchaseEquipAndBack()
    {
        Size(720,1280);
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        yield return new EnterPlayMode();
        profile=AccountProgression.UseDisposableProfile(Seed(new AccountSave {gold=2480,potions=12,bombs=3,shieldPotions=4,megaPotions=1}));
        SceneManager.LoadScene("MainMenu");yield return null;yield return null;
        var menu=Object.FindFirstObjectByType<MainMenuController>();
        var shopButton=menu.GetComponentsInChildren<Button>(true).First(b=>b.name=="ShopButton");
        shopButton.onClick.Invoke();yield return null;yield return null;
        var shop=Object.FindFirstObjectByType<ShopMenuController>();
        Assert.IsNotNull(shop);
        var scroll=shop.GetComponentInChildren<ScrollRect>();
        var merchant=(RectTransform)scroll.transform.parent.Find("Merchant");
        var coin=merchant.Find("CurrencyPlaque/Gold").GetComponent<TMP_Text>();
        foreach(var resolution in new[]{new Vector2Int(720,1280),new Vector2Int(1080,2340),new Vector2Int(540,960)})
        {
            Size(resolution.x,resolution.y);
            yield return Until(()=>Screen.width==resolution.x&&Screen.height==resolution.y);
            scroll.verticalNormalizedPosition=1;Canvas.ForceUpdateCanvases();yield return null;yield return null;
            Assert.AreEqual(5,scroll.content.childCount);
            Assert.IsTrue(shop.GetComponentsInChildren<Image>().Where(i=>i.name=="ItemIcon").All(i=>i.sprite!=null && i.sprite.texture.filterMode==FilterMode.Point));
            foreach(var label in shop.GetComponentsInChildren<TMP_Text>())
            {
                Assert.AreEqual("LiberationSans SDF",label.font.name);
                label.ForceMeshUpdate();
                Assert.IsFalse(label.isTextOverflowing,label.name+": "+label.text);
            }
            foreach (Transform row in scroll.content)
            {
                var body = row.Find("Contents");
                var well = (RectTransform)body.Find("ItemSocket");
                var icon = (RectTransform)well.Find("ItemIcon");
                Assert.That(icon.pivot, Is.EqualTo(new Vector2(.5f, .5f)));
                Assert.That(Vector3.Distance(well.TransformPoint(well.rect.center), icon.TransformPoint(icon.rect.center)), Is.LessThan(.1f));
                var socket = well.GetComponent<Image>();
                Vector4 rim = socket.sprite.border / (socket.pixelsPerUnit * socket.pixelsPerUnitMultiplier);
                // Measure empty space inside the stone on all four sides, in shop layout units.
                Bounds itemBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(well, icon);
                Assert.That(itemBounds.min.x - (well.rect.xMin + rim.x), Is.GreaterThanOrEqualTo(8f), row.name + " left");
                Assert.That(itemBounds.min.y - (well.rect.yMin + rim.y), Is.GreaterThanOrEqualTo(8f), row.name + " bottom");
                Assert.That(well.rect.xMax - rim.z - itemBounds.max.x, Is.GreaterThanOrEqualTo(8f), row.name + " right");
                Assert.That(well.rect.yMax - rim.w - itemBounds.max.y, Is.GreaterThanOrEqualTo(8f), row.name + " top");
                var group = (RectTransform)body.Find("PriceGroup");
                var price = (RectTransform)group.Find("Price");
                var priceCoin = (RectTransform)group.Find("Coin");
                float left = GameplayPixelLayoutController.ScreenRect(priceCoin).xMin;
                float right = GameplayPixelLayoutController.ScreenRect(price).xMax;
                Assert.That((left + right) / 2, Is.EqualTo(GameplayPixelLayoutController.ScreenRect((RectTransform)body.Find("Buy")).center.x).Within(1));
            }
            string stem=Output+"/Shop-"+resolution.x+"x"+resolution.y;
            yield return Capture(stem+".png");
            Vector3 position=merchant.position;
            var first=scroll.content.GetChild(0).position;
            scroll.verticalNormalizedPosition=0;Canvas.ForceUpdateCanvases();yield return null;
            Assert.AreEqual(position,merchant.position);Assert.Greater(Vector3.Distance(first,scroll.content.GetChild(0).position),10);
            Assert.IsTrue(GameplayPixelLayoutController.ScreenRect(scroll.viewport).Contains(GameplayPixelLayoutController.ScreenRect((RectTransform)scroll.content.GetChild(4)).center));
            if(resolution.x==720) {yield return Capture(stem+"-bottom.png");}
        }
        // Exercise an asymmetric safe rectangle on the ordinary production safe-area parent.
        var screen = (RectTransform)shop.transform;
        var holder = (RectTransform)screen.Find("ShopSafeArea");
        holder.anchorMin = new Vector2(.035f, .055f);
        holder.anchorMax = new Vector2(.99f, .94f);
        holder.offsetMin = holder.offsetMax = Vector2.zero;
        yield return null; yield return null;
        var insetBounds = GameplayPixelLayoutController.ScreenRect(holder);
        foreach (var control in new[] {merchant.Find("Back"), merchant.Find("CurrencyPlaque/Gold"), scroll.viewport})
        {
            Rect bounds = GameplayPixelLayoutController.ScreenRect((RectTransform)control);
            Assert.That(bounds.xMin, Is.GreaterThanOrEqualTo(insetBounds.xMin - 1));
            Assert.That(bounds.yMin, Is.GreaterThanOrEqualTo(insetBounds.yMin - 1));
            Assert.That(bounds.xMax, Is.LessThanOrEqualTo(insetBounds.xMax + 1));
            Assert.That(bounds.yMax, Is.LessThanOrEqualTo(insetBounds.yMax + 1));
        }
        var last=scroll.content.Find("ColorBomb/Contents");
        last.Find("Buy").GetComponent<Button>().onClick.Invoke();
        Assert.AreEqual("2,432",coin.text);
        last.Find("Equip").GetComponent<Button>().onClick.Invoke();
        Assert.IsTrue(AccountProgression.Current.Equipped(ConsumableKind.ColorBomb));
        Assert.AreEqual("MouseHelpful",merchant.Find("Shopkeeper").GetComponent<Image>().sprite.name);
        merchant.Find("Back").GetComponent<Button>().onClick.Invoke();yield return null;
        Assert.IsFalse(shop.gameObject.activeInHierarchy);
    }

    [UnityTest] public IEnumerator SelectedVariantsWorkInTheGameScene()
    {
        Size(720,1280);
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        yield return new EnterPlayMode();
        foreach(var kind in new[]{ConsumableKind.ShieldPotion,ConsumableKind.MegaPotion})
        {
            profile?.Dispose();
            string path=Seed(new AccountSave {potionKind=kind,bombKind=ConsumableKind.ColorBomb,shieldPotions=3,megaPotions=3,colorBombs=3,equipPotions=true,equipBombs=true});
            profile=AccountProgression.UseDisposableProfile(path);
            SceneManager.LoadScene("Game");yield return null;
            yield return Until(()=>RunSession.Current?.Continuation?.CanCapture==true&&RunSession.Current.Waves.IsWaveActive);
            var run=RunSession.Current;
            Assert.AreEqual(kind,run.PotionKind); Assert.AreEqual(ConsumableKind.ColorBomb,run.BombKind);
            var button=run.GetComponent<RunControlsUI>().GameplayCanvas.GetComponentsInChildren<Button>().First(b=>b.name==kind.ToString());
            Assert.AreEqual(kind.ToString(),button.transform.Find("Icon").GetComponent<Image>().sprite.name);
            var colorSlot = run.GetComponent<RunControlsUI>().GameplayCanvas.GetComponentsInChildren<Button>().First(b => b.name == "ColorBomb");
            var colorIcon = colorSlot.transform.Find("Icon").GetComponent<Image>();
            CollectionAssert.Contains(Resources.LoadAll<Sprite>("VFX/BombRefinement/Idles/Star"), colorIcon.sprite);

            if(kind==ConsumableKind.MegaPotion) {
                Assert.IsFalse(run.TryUsePotion());run.Player.TryTakeDamage(25);
                Assert.Less(run.Player.CurrentHealth,run.Player.MaximumHealth);
                button.onClick.Invoke();Assert.AreEqual(run.Player.MaximumHealth,run.Player.CurrentHealth);
            } else {
                run.Player.GrantShield(5);button.onClick.Invoke();
                var saved=run.Player.CaptureContinuation();
                Assert.Greater(saved.potionShield,0);Assert.AreEqual(3,saved.potionShieldMoves);
                run.Player.RestoreContinuation(saved);
                for(int i=0;i<3;i++)run.Player.AdvancePotionShield();
                Assert.AreEqual(5,run.Player.CurrentShield);
            }
            Assert.AreEqual(2,run.Charges(kind));
            Assert.IsTrue(run.ToggleBombTargeting());run.CancelTargeting();Assert.AreEqual(3,run.Charges(run.BombKind));
            var gem=run.Board.GetGem(0,0);
            var targets=Enumerable.Range(0,run.Board.Width).SelectMany(x=>Enumerable.Range(0,run.Board.Height).Select(y=>run.Board.GetGem(x,y)))
                .Where(g=>run.Board.IsSupplyTarget(gem,g,1,true)).ToArray();
            Assert.IsTrue(targets.All(g=>g.Type==gem.Type));
            Assert.IsTrue(run.Continuation.SaveNow());
            Assert.IsTrue(run.TryUseBomb(gem));Assert.IsTrue(run.Board.IsBusy);
            Assert.AreEqual(2,run.Charges(run.BombKind));
            Assert.IsTrue(run.Continuation.SaveNow());
            string inFlight = File.ReadAllText(path);
            yield return Until(()=>!run.Board.IsBusy);
            Assert.IsTrue(targets.All(g=>g==null || run.Board.GetGem(g.Column,g.Row)!=g));
            SceneManager.LoadScene("MainMenu");yield return null;
            if (kind == ConsumableKind.ShieldPotion)
            {
                File.WriteAllText(path, inFlight);
                profile.Dispose();
                profile = AccountProgression.UseDisposableProfile(path);
                SceneManager.LoadScene("Game");yield return null;
                yield return Until(()=>RunSession.Current?.Continuation!=null && !RunSession.Current.Continuation.IsRestoring);
                var restored = RunSession.Current;
                Assert.IsNull(restored.Continuation.Error);
                Assert.AreEqual(ConsumableKind.ColorBomb, restored.BombKind);
                Assert.AreEqual(2,restored.Charges(restored.BombKind));
                Assert.AreEqual(2,AccountProgression.Current.Owned(restored.BombKind));
                Assert.AreEqual(2,restored.Charges(restored.PotionKind));
                restored.GetComponent<RunControlsUI>().Close();
                SceneManager.LoadScene("MainMenu");yield return null;
            }
        }
    }
}
