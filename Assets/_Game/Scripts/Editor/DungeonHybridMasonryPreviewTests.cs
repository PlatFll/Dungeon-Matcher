using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

// The approved pieces must be supplied by production, never an editor decorator.
public sealed class DungeonHybridMasonryPreviewTests
{
    const string Root="ArtSource/DungeonHybrid/SideMasonry/ApprovedIntegration";
    static void AssertProductionCorners(GameplayPixelLayoutController layout)
    {
        var world=GameObject.Find("DungeonPixelLabEnvironment").transform;
        Assert.That(world.Find("Candidate side masonry"),Is.Null);
        var root=world.Find("Approved corner masonry");Assert.That(root,Is.Not.Null);
        var art=DungeonPixelLabBattleArt.Current;
        var sides=world.Find("Structure/Edge masonry").GetComponentsInChildren<Image>();
        float scale=DungeonPixelLabBattleArt.PhysicalPixelScale;
        foreach(string side in new[]{"Left","Right"})
        {
            var name="Masonry_Corner_"+side;
            var image=root.Find(name).GetComponent<Image>();
            Assert.That(image.sprite,Is.SameAs(art.Sprite(name)));
            Assert.That(art.hybridModules.Contains(image.sprite),Is.True,"Sprite must be serialized in the production resource");
            var r=GameplayPixelLayoutController.ScreenRect(image.rectTransform);
            Assert.That(r.yMin+2*scale,Is.EqualTo(0).Within(.1f));
            Assert.That(side=="Left"?r.xMin:r.xMax,Is.EqualTo(side=="Left"?0:UnityEngine.Device.Screen.width).Within(.1f));
            var upper=sides.Where(i=>i.name.EndsWith(side)).Select(i=>GameplayPixelLayoutController.ScreenRect(i.rectTransform)).OrderBy(b=>b.yMin).First();
            Assert.That(upper.yMin,Is.EqualTo(84*scale).Within(.1f),"Production corners must replace three lower repeats, joining the upper row exactly");
        }
        Assert.That(root.childCount,Is.EqualTo(2));
    }
    [UnityTearDown]public IEnumerator Cleanup()=>new DungeonHybridIntegrationTests().Cleanup();
    [UnityTest,Explicit("Verify the user-approved corner masonry in the actual production Game scene.")]
    public IEnumerator ApprovedCornersWithThreeLiveEnemies()
        =>new DungeonHybridIntegrationTests().CaptureScreens(Root,"Production",AssertProductionCorners);
}
