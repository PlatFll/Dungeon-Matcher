using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public sealed class EnemyAbilityMotionTests
{
    [Serializable] public sealed class Entry
    {
        public string name, character, state;
        public int[] frames, durations, impactFrames;
        public bool loop, legacyImpact;
    }
    [Serializable] private sealed class Entries { public Entry[] entries; }
    public static Entry[] ReadEntries() => JsonUtility.FromJson<Entries>("{\"entries\":" +
        File.ReadAllText("ArtSource/EnemyAttacks/abilities.json") + "}").entries;

    [Test] public void AllApprovedStatesHaveNativeFramesAndExactlyTheAuthoredCues()
    {
        foreach (var entry in ReadEntries())
        {
            var data=AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Game/Data/Enemies/Enemy_"+entry.character+".asset");
            Assert.That(data.TimeSpecialAbilityFromAnimation && data.UseAuthoredSpecialAbilityMotion,Is.True,entry.name);
            var frames=CombatActionImporter.LoadFrames(entry.name);
            Assert.That(frames.Length,Is.EqualTo(entry.frames.Length),entry.name);
            var importer=(TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(frames[0]));
            Assert.That(importer.filterMode,Is.EqualTo(FilterMode.Point));
            Assert.That(importer.textureCompression,Is.EqualTo(TextureImporterCompression.Uncompressed));
            Assert.That(importer.mipmapEnabled,Is.False);
            Assert.That(frames[0].texture.width,Is.EqualTo(frames[0].rect.width*frames.Length),"No sheet downscale");
            var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>(CombatActionImporter.AnimationRoot+"/"+entry.name+".anim");
            Assert.That(clip.length,Is.EqualTo(entry.durations.Sum()/1000f).Within(.001f),entry.name);
            var beats=clip.events.Where(e=>e.functionName=="AbilityBeat" || e.functionName=="AbilityImpact").ToArray();
            Assert.That(beats.Length,Is.EqualTo(entry.impactFrames.Length),entry.name);
            for(int i=0;i<beats.Length;i++)
            {
                Assert.That(beats[i].time,Is.EqualTo(entry.durations.Take(entry.impactFrames[i]).Sum()/1000f).Within(.001f));
                if(!entry.legacyImpact)Assert.That(beats[i].intParameter,Is.EqualTo(i+1));
            }
            Assert.That(clip.events.Count(e=>e.functionName=="AbilityComplete"),Is.EqualTo(entry.loop?0:1));
        }
    }

    [Test] public void CancelledActionCannotDonateBeatsToNextCastAndPauseRejectsCues()
    {
        var go=new GameObject("Motion identity fixture");var data=ScriptableObject.CreateInstance<EnemyDefinition>();
        const BindingFlags flags=BindingFlags.Instance|BindingFlags.NonPublic;
        try
        {
            typeof(EnemyDefinition).GetField("useAuthoredSpecialAbilityMotion",flags).SetValue(data,true);
            typeof(EnemyDefinition).GetField("timeSpecialAbilityFromAnimation",flags).SetValue(data,true);
            var actor=go.AddComponent<EnemyActor>();
            typeof(EnemyActor).GetField("definition",flags).SetValue(actor,data);
            typeof(EnemyActor).GetField("isInitialized",flags).SetValue(actor,true);
            Assert.That(actor.TryBeginSpecialAbilityAnimationAction(),Is.True);
            int first=actor.StartSpecialMotion();actor.EndSpecialAbilityAnimationAction();
            Assert.That(actor.TryBeginSpecialAbilityAnimationAction(),Is.True);
            int second=actor.StartSpecialMotion();Assert.That(second,Is.Not.EqualTo(first));
            actor.NotifySpecialMotionBeat(first,2);actor.NotifySpecialMotionComplete(first);
            Assert.That(actor.WaitForSpecialMotionBeat(second).MoveNext(),Is.True);
            Time.timeScale=0;actor.NotifySpecialMotionBeat(second,1);
            Assert.That(actor.WaitForSpecialMotionBeat(second).MoveNext(),Is.True);
            Time.timeScale=1;actor.NotifySpecialMotionBeat(second,1);actor.NotifySpecialMotionBeat(second,1);
            Assert.That(actor.WaitForSpecialMotionBeat(second).MoveNext(),Is.False);
            Assert.That(actor.WaitForSpecialMotionBeat(second,2).MoveNext(),Is.True);
            actor.EndSpecialAbilityAnimationAction();
            Assert.That(actor.WaitForSpecialMotionBeat(second,2).MoveNext(),Is.False,"cancelled waits terminate");
        }
        finally { Time.timeScale=1;UnityEngine.Object.DestroyImmediate(go);UnityEngine.Object.DestroyImmediate(data); }
    }
}
