using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public sealed class NewPlayablesAssetTests
{
    [Test] public void ProductionDefinitionsHaveNativeControllersSeparateProgressionAndCompleteCues()
    {
        var defs=Resources.LoadAll<PlayerDefinition>("Players");
        Assert.That(defs.Select(d=>d.PlayerId).Distinct().Count(),Is.EqualTo(defs.Length));
        foreach(string id in new[]{CharacterSelectionSettings.MinotaurPlayerId,CharacterSelectionSettings.KoboldPlayerId})
        {
            var player=defs.Single(d=>d.PlayerId==id);Assert.That(player.ActiveAbility,Is.Not.Null);
            Assert.That(player.BattleReferenceCanvas,Is.EqualTo(new Vector2(64,64)));
            var controller=(AnimatorController)player.BattleAnimatorController;
            var states=controller.layers[0].stateMachine.states.Select(s=>s.state.name).ToArray();
            foreach(string state in new[]{"Idle","Ability","Recovery","Hit","Death"})Assert.That(states,Does.Contain(state));
            var texture=AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(player.BattleCharacterSprite)) as TextureImporter;
            Assert.That(texture.filterMode,Is.EqualTo(FilterMode.Point));Assert.That(texture.mipmapEnabled,Is.False);
            Assert.That(texture.spritePixelsPerUnit,Is.EqualTo(64));Assert.That(texture.textureCompression,Is.EqualTo(TextureImporterCompression.Uncompressed));
            Assert.That(player.HealthAtLevel(2),Is.GreaterThan(player.HealthAtLevel(1)));
            Assert.That(player.ActiveAbility.EnergyCost,Is.EqualTo(90));
        }
        Assert.That(defs.Single(d=>d.PlayerId==CharacterSelectionSettings.KoboldPlayerId).PassiveAbility,Is.TypeOf<RepeatRicochetPassiveDefinition>());
        foreach(string cue in new[]{"AttenderJump","AttenderStep","AttenderCrash","KoboldSpin","KoboldShot","KoboldRicochet"})
        {var clip=Resources.Load<AudioClip>("Audio/Combat/"+cue);Assert.That(clip,Is.Not.Null,cue);Assert.That(clip.channels,Is.EqualTo(1));Assert.That(clip.length,Is.LessThan(.5f));}
    }
}
