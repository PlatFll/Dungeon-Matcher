using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class FrostwakeArtImporter
{
    [MenuItem("Dungeon Matcher/Frostwake/Import approved motion")]
    public static void Run()
    {
        EditorUtility.audioMasterMute=true;
        const string art=FrostwakeZoneImporter.Art+"Motion/",animation="Assets/_Game/Animations/Frostwake/";
        ForestProductionImporter.ImportMotionSet(FrostwakeZoneImporter.Ids,"ArtSource/Frostwake/production/",art,animation,FrostwakeZoneImporter.Data,4);
        foreach(string path in Directory.GetFiles(art,"*.png"))
        {
            foreach(var sprite in AssetDatabase.LoadAllAssetsAtPath(path.Replace('\\','/')).OfType<Sprite>())
                if(Mathf.Abs(sprite.pivot.y-4)>.01f)throw new InvalidDataException(path+" did not preserve its four-pixel floor pivot.");
        }
        foreach(string id in FrostwakeZoneImporter.Ids)
        {
            var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(animation+id+".controller");
            var machine=controller.layers[0].stateMachine;var idle=machine.states.Single(x=>x.state.name=="Idle").state;
            var interrupt=machine.states.Select(x=>x.state).FirstOrDefault(x=>x.name=="Interrupt");
            if(interrupt!=null)
            {var t=interrupt.AddTransition(idle);t.hasExitTime=true;t.exitTime=1;t.duration=0;}
            EditorUtility.SetDirty(controller);
        }
        AssetDatabase.SaveAssets();Debug.Log("Frostwake: native 128x104 motion imported, 4-pixel floor padding retained, authored contacts bound.");
    }
}
