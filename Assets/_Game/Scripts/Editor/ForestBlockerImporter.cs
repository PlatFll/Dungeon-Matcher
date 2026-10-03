using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

/// <summary>Imports the user's seven explicit selections without rebuilding travel or other art.</summary>
public static class ForestBlockerImporter
{
    private const string Source="ArtSource/Forest/RosterProduction/";
    private const string Art="Assets/_Game/Art/Board/ApprovedBlockers/";
    [MenuItem("Dungeon Matcher/Forest/Import selected blockers")]
    public static void Run()
    {
        Sprite wood=Import("Wood_A"),stone=Import("Stone_B"),chain=Import("Chain_A"),thorn=Import("Thorn_A");
        var theme=Resources.Load<GameplayThemeDefinition>("Zones/ForestTheme");
        theme.rootLevelOne=Import("Root1_B");theme.rootLevelTwo=Import("Root2_B");theme.vineOverlay=Import("Vines_B");
        theme.vineSpreadFrames=ImportFrames("Spread");theme.vineHitFrames=ImportFrames("Hit");
        EditorUtility.SetDirty(theme);
        // Opening and saving Game runs ExecuteAlways layout/background callbacks.
        // Change only the four inspected YAML references, preserving scene state.
        string scenePath="Assets/_Game/Scenes/Game.unity",scene=File.ReadAllText(scenePath);
        scene=Bind(scene,"woodenBarricadeSprite",wood);
        scene=Bind(scene,"stoneBarricadeSprite",stone);
        scene=Bind(scene,"thornBarricadeSprite",thorn,"stoneBarricadeSprite");
        scene=Bind(scene,"pinnedGemOverlaySprite",chain);
        File.WriteAllText(scenePath,scene,new System.Text.UTF8Encoding(false));
        AssetDatabase.ImportAsset(scenePath,ImportAssetOptions.ForceSynchronousImport);AssetDatabase.SaveAssets();
        Debug.Log("Selected blockers imported: Wood A, Stone B, Chain A, Thorn A, Roots B/B, Vines B.");
    }
    private static string Bind(string text,string field,Sprite sprite,string insertAfter=null)
    {
        if(!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(sprite,out string guid,out long id))
            throw new InvalidDataException("Missing sprite identity: "+field);
        var pattern=new Regex("^  "+field+": \\{[^\\r\\n]+\\}",RegexOptions.Multiline);
        string line=$"  {field}: {{fileID: {id}, guid: {guid}, type: 3}}";
        if(pattern.Matches(text).Count==1) return pattern.Replace(text,line,1);
        if(insertAfter!=null && pattern.Matches(text).Count==0)
        {
            var anchor=new Regex("^  "+insertAfter+": \\{[^\\r\\n]+\\}",RegexOptions.Multiline);
            if(anchor.Matches(text).Count==1) return anchor.Replace(text,m=>m.Value+"\n"+line,1);
        }
        throw new InvalidDataException("Expected one scene reference: "+field);
    }
    private static Sprite[] ImportFrames(string state)
    {
        string source=Source+"Selected/Vines_B/"+state;
        return Directory.GetFiles(source,"*.png").OrderBy(p=>p)
            .Select(p=>Import("Vines_B/"+state+"/"+Path.GetFileNameWithoutExtension(p),p)).ToArray();
    }
    private static Sprite Import(string name,string source=null)
    {
        string path=Art+name+".png";Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.Copy(source??Source+"Raw/"+name+"/Concept/00.png",path,true);
        AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
        importer.spritePixelsPerUnit=64;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;
        importer.textureCompression=TextureImporterCompression.Uncompressed;importer.alphaIsTransparency=true;
        importer.npotScale=TextureImporterNPOTScale.None;importer.wrapMode=TextureWrapMode.Clamp;
        var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);
        settings.spriteMeshType=SpriteMeshType.FullRect;settings.spriteAlignment=(int)SpriteAlignment.Center;
        importer.SetTextureSettings(settings);importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
}
