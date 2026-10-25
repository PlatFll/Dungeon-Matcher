using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class ForestProductionBuild
{
    public static void Android()
    {
        const string output=".utmp/ForestProduction/DungeonMatcher-ApprovedZones.apk";
        const string report="ArtSource/ForestProductionIntegration/AndroidBuild.json";
        Directory.CreateDirectory(Path.GetDirectoryName(output));
        bool bundle=EditorUserBuildSettings.buildAppBundle,signing=PlayerSettings.Android.useCustomKeystore;
        try
        {
            EditorUserBuildSettings.buildAppBundle=false;PlayerSettings.Android.useCustomKeystore=false;
            var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{
                scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),
                locationPathName=output,target=BuildTarget.Android,options=BuildOptions.Development});
            File.WriteAllText(report,JsonUtility.ToJson(new Report{
                result=result.summary.result.ToString(),errors=(int)result.summary.totalErrors,warnings=(int)result.summary.totalWarnings,
                bytes=(long)result.summary.totalSize,seconds=result.summary.totalTime.TotalSeconds,apk=Path.GetFullPath(output),
                backend=PlayerSettings.GetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android).ToString(),deviceTested=false},true));
            if(result.summary.result!=BuildResult.Succeeded)throw new Exception("Approved-zone Android build failed; inspect AndroidBuild.json and build log.");
        }
        finally{EditorUserBuildSettings.buildAppBundle=bundle;PlayerSettings.Android.useCustomKeystore=signing;}
    }
    [Serializable]class Report{public string result,apk,backend;public int errors,warnings;public long bytes;public double seconds;public bool deviceTested;}
}
