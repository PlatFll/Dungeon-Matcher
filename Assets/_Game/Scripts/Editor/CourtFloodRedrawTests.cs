#if UNITY_EDITOR
using NUnit.Framework;
using UnityEngine;

public sealed class CourtFloodRedrawTests
{
    private GameObject root;
    private CourtFloodGraphic water;
    private int redraws;

    [SetUp]
    public void SetUp()
    {
        root = new GameObject("Court flood test", typeof(RectTransform), typeof(Canvas));
        var surface = new GameObject("Production water surface", typeof(RectTransform), typeof(CanvasRenderer));
        surface.transform.SetParent(root.transform, false);
        water = surface.AddComponent<CourtFloodGraphic>();
        water.RegisterDirtyVerticesCallback(() => redraws++);
    }

    [TearDown]
    public void TearDown() => Object.DestroyImmediate(root);

    [Test]
    public void DryPausedAndReducedWaterOnlyDirtyChangedGeometry()
    {
        water.Present(false, 1f / 60f, false);
        water.Present(false, 1f / 60f, true);
        Assert.That(redraws, Is.Zero, "Dry water has no visible geometry in either motion mode.");

        water.Present(true, 0.5f, false);
        Assert.That(water.FillAmount, Is.GreaterThan(0f).And.LessThan(1f));
        Assert.That(redraws, Is.EqualTo(1));
        float rise = water.AnimationTime;
        water.Present(true, 0f, false);
        Assert.That(water.AnimationTime, Is.EqualTo(rise));
        Assert.That(redraws, Is.EqualTo(1), "A paused rising frame can be reused.");

        water.Present(true, 0f, true);
        Assert.That(water.FillAmount, Is.EqualTo(1f));
        Assert.That(redraws, Is.EqualTo(2));
        water.Present(true, 0.5f, true);
        Assert.That(redraws, Is.EqualTo(2), "Settled reduced motion uses a static tint.");
        water.Present(true, 0f, false);
        Assert.That(redraws, Is.EqualTo(3), "Restoring full motion makes submerged bubbles visible again.");
        water.Present(true, 0.1f, false);
        Assert.That(redraws, Is.EqualTo(4), "At full depth, bubbles must continue moving.");
        water.Present(false, 0f, true);
        Assert.That(water.FillAmount, Is.Zero);
        Assert.That(redraws, Is.EqualTo(5), "Drain removes the last visible mesh.");
    }
}
#endif
