using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>Local Android smoke build; retains production scenes/backend/architectures.</summary>
public static class DungeonPixelPipelineStage16Build
{
    public static void Android()
    {
        const string output=".utmp/Stage16Build/DungeonMatcher-Stage16.apk";
        const string reportPath="ArtSource/DungeonPixelLab/Diagnostics/Stage16/AndroidBuild.json";
        Directory.CreateDirectory(Path.GetDirectoryName(output));
        bool bundle=EditorUserBuildSettings.buildAppBundle, signed=PlayerSettings.Android.useCustomKeystore;
        try
        {
            EditorUserBuildSettings.buildAppBundle=false;
            PlayerSettings.Android.useCustomKeystore=false; // Local debug signing only; no credentials read.
            var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),
                locationPathName=output,target=BuildTarget.Android,options=BuildOptions.Development });
            File.WriteAllText(reportPath,JsonUtility.ToJson(new Result {
                result=result.summary.result.ToString(),errors=(int)result.summary.totalErrors,warnings=(int)result.summary.totalWarnings,
                bytes=(long)result.summary.totalSize,seconds=result.summary.totalTime.TotalSeconds,
                apk=Path.GetFullPath(output),backend=PlayerSettings.GetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android).ToString(),
                deviceTested=false },true));
            if(result.summary.result!=BuildResult.Succeeded)throw new Exception("Stage16 Android build failed; see AndroidBuild.json and AndroidBuild.log.");
        }
        finally
        {
            EditorUserBuildSettings.buildAppBundle=bundle;
            PlayerSettings.Android.useCustomKeystore=signed;
        }
    }
    [Serializable] class Result {public string result,apk,backend;public int errors,warnings;public long bytes;public double seconds;public bool deviceTested;}
}
