using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class IronveinArtTests
{
    [Test] public void IronveinStillsHaveNativeCanvasesAndDedicatedPointFilteredAssets()
    {
        foreach(string id in IronveinArtImporter.Ids)
        {
            var def=AssetDatabase.LoadAssetAtPath<EnemyDefinition>(IronveinImporter.EnemyPath+id+".asset");
            var sprite=def.StaticVisualSprite;
            Assert.That(sprite,Is.Not.Null,id);
            int native=id=="grand_delver"||id=="obsidian_sentinel"?112:80;
            Assert.That(sprite.rect.size,Is.EqualTo(Vector2.one*native),id);
            string path=AssetDatabase.GetAssetPath(sprite);
            Assert.That(path.StartsWith(IronveinArtImporter.Art),Is.True,id+" must not show a placeholder from another zone");
            var import=(TextureImporter)AssetImporter.GetAtPath(path);
            Assert.That(import.filterMode,Is.EqualTo(FilterMode.Point));
            Assert.That(import.mipmapEnabled,Is.False);
            Assert.That(import.textureCompression,Is.EqualTo(TextureImporterCompression.Uncompressed));
            Assert.That(import.npotScale,Is.EqualTo(TextureImporterNPOTScale.None));
            Assert.That(import.spritePixelsPerUnit,Is.EqualTo(64));
            Assert.That(sprite.pivot,Is.EqualTo(new Vector2(native*.5f,0)));
        }
    }

    [Test] public void IronveinImportedAttackEventsUseTheReviewedContactFrames()
    {
        var def=AssetDatabase.LoadAssetAtPath<EnemyDefinition>(IronveinImporter.EnemyPath+"pickaxe_delver.asset");
        foreach(var pair in new[]{("AutoAttack",.27f,3),("OreChargedAutoAttack",.32f,4)})
        {
            var clip=def.AnimationControllerOverride.animationClips.Single(c=>c.name=="pickaxe_delver_"+pair.Item1);
            var events=AnimationUtility.GetAnimationEvents(clip);
            Assert.That(events.Count(e=>e.functionName=="AutoAttackImpact"),Is.EqualTo(1));
            Assert.That(events.Count(e=>e.functionName=="AutoAttackComplete"),Is.EqualTo(1));
            Assert.That(events.Single(e=>e.functionName=="AutoAttackImpact").time,Is.EqualTo(pair.Item2).Within(.001f));
            var binding=AnimationUtility.GetObjectReferenceCurveBindings(clip).Single();
            var keys=AnimationUtility.GetObjectReferenceCurve(clip,binding);
            var contact=keys.Last(k=>k.time<=pair.Item2+.001f);
            Assert.That(contact.value.name,Is.EqualTo("pickaxe_delver_"+pair.Item1+"_"+pair.Item3.ToString("00")));
            Assert.That(keys[0].value.name,Does.EndWith("_00"));
            Assert.That(events.Single(e=>e.functionName=="AutoAttackComplete").time,Is.GreaterThan(pair.Item2));
        }
    }

    [Test] public void IronveinEveryIdentityHasNativeIdleAttackHitAndDeath()
    {
        foreach(string id in IronveinArtImporter.Ids)
        {
            var def=AssetDatabase.LoadAssetAtPath<EnemyDefinition>(IronveinImporter.EnemyPath+id+".asset");
            Assert.That(def.AnimationControllerOverride,Is.Not.Null,id);
            foreach(string state in new[]{"Idle","AutoAttack","Hit","Death"})
            {
                var clip=def.AnimationControllerOverride.animationClips.Single(c=>c.name==id+"_"+state);
                var keys=AnimationUtility.GetObjectReferenceCurve(clip,AnimationUtility.GetObjectReferenceCurveBindings(clip).Single());
                Assert.That(keys.Length,Is.GreaterThan(2),id+" "+state);
                foreach(var key in keys)
                {
                    var sprite=(Sprite)key.value;
                    Assert.That(AssetDatabase.GetAssetPath(sprite).StartsWith(IronveinArtImporter.Art),Is.True);
                    Assert.That(sprite.rect.height,Is.EqualTo(def.StaticVisualSprite.rect.height),id+" fixed floor/canvas height");
                    Assert.That(sprite.rect.width,Is.GreaterThanOrEqualTo(def.StaticVisualSprite.rect.width),id+" wider action margins preserve native pixels");
                    Assert.That(sprite.pivot.y,Is.Zero);
                }
                var events=AnimationUtility.GetAnimationEvents(clip);
                Assert.That(events.Count(e=>e.functionName=="AutoAttackImpact"),Is.EqualTo(state=="AutoAttack"?1:0));
                Assert.That(events.Count(e=>e.functionName=="AutoAttackComplete"),Is.EqualTo(state=="AutoAttack"?1:0));
                if(state=="AutoAttack")
                    Assert.That(events.Single(e=>e.functionName=="AutoAttackImpact").time,Is.InRange(.1f,.65f),id+" readable windup/contact");
            }
        }
    }
}
