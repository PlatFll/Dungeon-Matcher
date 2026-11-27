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
        string requested=Environment.GetEnvironmentVariable("DM_CR_BUILD_OUTPUT");
        string folder=Path.GetFullPath(string.IsNullOrWhiteSpace(requested)
            ? Path.Combine(".utmp/CrystalResonance/Build",Guid.NewGuid().ToString("N")) : requested);
        string output=Path.Combine(folder,filename);
        string evidence=Path.Combine(folder,target+".json");
        if(File.Exists(output)||File.Exists(evidence)||(target==BuildTarget.StandaloneWindows64 && Directory.Exists(Path.GetDirectoryName(output))))
            throw new IOException("Refusing to overwrite an existing build: "+output);
        Directory.CreateDirectory(Path.GetDirectoryName(output));
        bool bundle=EditorUserBuildSettings.buildAppBundle,signing=PlayerSettings.Android.useCustomKeystore;
        try
        {
            if(target==BuildTarget.Android){EditorUserBuildSettings.buildAppBundle=false;PlayerSettings.Android.useCustomKeystore=false;}
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{
                scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),
                locationPathName=output,target=target,options=BuildOptions.Development});
            File.WriteAllText(evidence,JsonUtility.ToJson(new Evidence{
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
