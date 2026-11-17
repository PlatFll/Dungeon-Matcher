#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>Explicit local development build; never publishes or modifies approved art.</summary>
public static class CrystalResonanceBuild
{
    public static void Android()=>Build(BuildTarget.Android,"DungeonMatcher-CrystalResonance.apk");
    public static void Windows()=>Build(BuildTarget.StandaloneWindows64,"Windows/DungeonMatcher.exe");
    private static void Build(BuildTarget target,string filename)
    {
        string folder=Path.GetFullPath(".utmp/CrystalResonance/Build");
        string output=Path.Combine(folder,filename);Directory.CreateDirectory(Path.GetDirectoryName(output));
        bool bundle=EditorUserBuildSettings.buildAppBundle,signing=PlayerSettings.Android.useCustomKeystore;
        try
        {
            if(target==BuildTarget.Android){EditorUserBuildSettings.buildAppBundle=false;PlayerSettings.Android.useCustomKeystore=false;}
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{
                scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),
                locationPathName=output,target=target,options=BuildOptions.Development});
            File.WriteAllText(Path.Combine(folder,target+".json"),JsonUtility.ToJson(new Evidence{
                result=report.summary.result.ToString(),errors=(int)report.summary.totalErrors,warnings=(int)report.summary.totalWarnings,
                bytes=(long)report.summary.totalSize,seconds=report.summary.totalTime.TotalSeconds,artifact=output,
                unity=Application.unityVersion,deviceTested=false},true));
            if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Crystal Resonance build failed: "+target);
        }
        finally {EditorUserBuildSettings.buildAppBundle=bundle;PlayerSettings.Android.useCustomKeystore=signing;}
    }
    [Serializable]private sealed class Evidence
    {public string result,artifact,unity;public int errors,warnings;public long bytes;public double seconds;public bool deviceTested;}
}
#endif
