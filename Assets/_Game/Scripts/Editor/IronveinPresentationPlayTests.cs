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
    [UnityTest] public IEnumerator IronveinCaveScreenFitsFourPortraitsAndSafeInset()
    { yield return MineCaveScreen(); }
    private IEnumerator MineCaveScreen()
    {
        yield return LaunchMineKit("grand_delver","ore_hauler","rivet_gunner");QuietKitFixture();
        var board=Run.Board;var owner=Run.Waves.ActiveEnemies[0];
        board.TryQueuePlaceMineStones(owner,3,6,MineStoneStage.Obsidian);yield return Stable();
        var first=board.MineStoneTargets().First();
        board.TryQueueMineStoneOperation(owner,first.State.id,MineStoneOperation.ArmCharge,null);yield return Stable();
        board.TryQueueMineDrillPower(1,3);board.TryQueueMineDrillPower(2,2);yield return Stable();
        // Let the normal materialization reveal finish before pausing a review capture.
        yield return Until(()=>board.GetComponentsInChildren<SpriteRenderer>()
            .Where(r=>r.sprite==Run.Zone.Definition.theme.mineStoneStages[2]).All(r=>
            {var p=new MaterialPropertyBlock();r.GetPropertyBlock(p);return p.GetFloat("_FlashAmount")==0;}),"native stone reveal finishes");
        foreach(var stone in board.GetComponentsInChildren<SpriteRenderer>().Where(r=>r.sprite==Run.Zone.Definition.theme.mineStoneStages[2]))
        {
            var block=new MaterialPropertyBlock();stone.GetPropertyBlock(block);
            Assert.That(block.GetFloat("_FlashAmount"),Is.Zero,"materialization must reveal native stone colors");
        }
        var theme=Run.Zone.Definition.theme;Time.timeScale=0;
        foreach(var actor in Run.Waves.ActiveEnemies)
            Set(actor.GetComponent<EnemyAutoAttack>(),"remainingAttackTime",actor.AttackInterval);
        string folder=Path.GetFullPath(".utmp/Ironvein/Captures");Directory.CreateDirectory(folder);
        try
        {
            int index=0;
            foreach(var size in new[]{new Vector2Int(720,1280),new Vector2Int(1080,1920),new Vector2Int(1080,2400),new Vector2Int(1080,1920)})
            {
                bool inset=index==3;
                Object.FindFirstObjectByType<BattleBackgroundTilemapController>().ApplyGameplayTheme(theme,index%3);
                typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{size});
                GameplayPixelLayoutController.ValidationSafeArea=inset?new Rect(32,60,1016,1772):(Rect?)null;
                yield return Until(()=>Screen.width==size.x && Screen.height==size.y,"mine portrait viewport");
                for(int frame=0;frame<12;frame++)yield return null;
                var errors=GameplayPixelLayoutValidator.Validate(Object.FindFirstObjectByType<GameplayPixelLayoutController>(),out string report);
                string name="phase09-"+size.x+"x"+size.y+(inset?"-safe":"");File.WriteAllText(Path.Combine(folder,name+".txt"),report);
                Assert.That(errors,Is.Empty,report);
                foreach(var machine in board.GetComponentsInChildren<SpriteRenderer>().Where(r=>r.name=="NativeHousing"))
                    foreach(var point in new[]{machine.bounds.min,machine.bounds.max})
                    {var p=Camera.main.WorldToScreenPoint(point);Assert.That(p.x,Is.InRange(0f,(float)Screen.width));Assert.That(p.y,Is.InRange(0f,(float)Screen.height));}
                var frameProfile=theme.playerFrame;
                Assert.That(frameProfile.TopPiece.rect.width,Is.EqualTo(146));
                Assert.That(GameObject.Find("PlayerDungeonBackdrop").GetComponent<Image>().sprite,Is.SameAs(theme.panelBackground));
                Assert.That(GameObject.Find("BottomDungeonBackdrop").GetComponent<Image>().sprite,Is.SameAs(theme.panelBackground));
                ScreenCapture.CaptureScreenshot(Path.Combine(folder,name+".png"));yield return null;yield return null;index++;
            }
        }
        finally{GameplayPixelLayoutController.ValidationSafeArea=null;Time.timeScale=1;}
    }

    [UnityTest] public IEnumerator IronveinSmallDrillPresentationUsesActualStopAndPauses()
    { yield return MineSmallDrillPresentation(); }
    private IEnumerator MineSmallDrillPresentation()
    {
        yield return LaunchMineKit("bore_engineer","stonewright");QuietKitFixture();var board=Run.Board;
        var owner=Enemy("bore_engineer");board.TryQueuePlaceMineStones(owner,4,6,MineStoneStage.Obsidian);yield return Stable();
        var target=board.MineStoneTargets().OrderBy(s=>s.Cell.x).First();int endpoint=-1;bool stopped=false;
        board.SmallMineDrillPresented+=(row,lane,end,hit)=>{endpoint=end;stopped=hit;};
        Assert.That(board.TryQueueSmallMineDrill(owner,true,target.Cell.y,_=>{}),Is.True);
        yield return Until(()=>board.transform.Find("BoreDrillProjectile")!=null,"native small drill starts");
        var projectile=board.transform.Find("BoreDrillProjectile");Time.timeScale=0;Vector3 at=projectile.localPosition;
        yield return new WaitForSecondsRealtime(.12f);Assert.That(projectile.localPosition,Is.EqualTo(at));Time.timeScale=1;
        yield return Stable();Assert.That(stopped,Is.True);Assert.That(endpoint,Is.EqualTo(target.Cell.x));
        Assert.That(board.FindMineStone(target.State.id).Value.Durability,Is.EqualTo(2));
        yield return new WaitForSeconds(.5f);
        Assert.That(board.GetComponentsInChildren<SpriteRenderer>().Any(r=>r.sprite==Run.Zone.Definition.theme.mineObsidianDamaged[0]),Is.True,
            "damaged Obsidian keeps its material and selects its damage art");
        Assert.That(board.transform.Find("BoreDrillProjectile"),Is.Null);
        var view=board.GetComponent<MineEnvironmentView>();view.enabled=false;
        int before=board.FindMineStone(target.State.id).Value.Durability;
        board.TryQueueSmallMineDrill(owner,true,target.Cell.y,_=>{});yield return Stable();
        Assert.That(board.FindMineStone(target.State.id).Value.Durability,Is.EqualTo(before-1),"missing VFX cannot block actual damage");
    }
}

public sealed class IronveinThemeTests
{
    [Test] public void IronveinThemeUsesOwnNativeMaterialsAndSettingsAwareAudio()
    {
        var theme=Resources.Load<GameplayThemeDefinition>("Zones/IronveinTheme");
        Assert.That(theme.battleEnvironmentVariants.Length,Is.EqualTo(3));
        Assert.That(theme.generalBackground,Is.Not.SameAs(theme.panelBackground));
        Assert.That(theme.abilityIcons,Is.Empty,"original player skills are preserved");
        foreach(var sprite in new[]{theme.generalBackground,theme.panelBackground,theme.boardCells[0],theme.mineCore,theme.horizontalMineDrill,
            theme.mineHardenedDamaged,theme.mineObsidianDamaged[1],theme.playerFrame.TopPiece,theme.settingsNormal})
        {
            string path=AssetDatabase.GetAssetPath(sprite);Assert.That(path.StartsWith(IronveinArtImporter.Art),Is.True,path);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            Assert.That(importer.filterMode,Is.EqualTo(FilterMode.Point));Assert.That(importer.mipmapEnabled,Is.False);
            Assert.That(importer.textureCompression,Is.EqualTo(TextureImporterCompression.Uncompressed));
        }
        foreach(string name in new[]{"MineRail","MineSmallDrill","MineLargeDrill","MineStoneHarden","MineStoneBreak","MineFuse","MineBlast","MineOre","MineRivet","MineHammer","MinePiston","MineMechBreak"})
            Assert.That(Resources.Load<AudioClip>("Audio/Combat/"+name),Is.Not.Null,name);
        Assert.That(Resources.Load<ZoneDefinition>("Zones/ironvein-excavation").music.length,Is.EqualTo(120).Within(.1f));
    }
}
