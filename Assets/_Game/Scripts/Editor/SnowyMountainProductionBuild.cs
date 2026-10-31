using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class SnowyMountainProductionBuild
{
    public static void Android()
    {
        const string output=".utmp/SnowProduction/DungeonMatcher-SnowyMountain.apk";
        const string evidence="ArtSource/SnowyMountainStaged/Integration/AndroidBuild.json";
        Directory.CreateDirectory(Path.GetDirectoryName(output));
        bool bundle=EditorUserBuildSettings.buildAppBundle,signing=PlayerSettings.Android.useCustomKeystore;
        try
        {
            if(!Resources.Load<SnowyMountainBattleAssembly>("UI/BattlePresentation/SnowyMountainAssembly"))throw new Exception("Missing production Snowy Mountain binding");
            if(!Resources.Load<DrownedCourtBattleAssembly>("UI/BattlePresentation/CourtAssembly"))throw new Exception("Missing production Court binding");
            EditorUserBuildSettings.buildAppBundle=false;PlayerSettings.Android.useCustomKeystore=false;
            var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{
                scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),
                locationPathName=output,target=BuildTarget.Android,options=BuildOptions.Development});
            File.WriteAllText(evidence,JsonUtility.ToJson(new Report{result=result.summary.result.ToString(),errors=(int)result.summary.totalErrors,warnings=(int)result.summary.totalWarnings,
                bytes=(long)result.summary.totalSize,seconds=result.summary.totalTime.TotalSeconds,apk=Path.GetFullPath(output),
                backend=PlayerSettings.GetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android).ToString(),deviceTested=false},true));
            if(result.summary.result!=BuildResult.Succeeded)throw new Exception("Snowy Mountain Android build failed; see AndroidBuild.json and build log.");
        }
        finally{EditorUserBuildSettings.buildAppBundle=bundle;PlayerSettings.Android.useCustomKeystore=signing;}
    }
    [Serializable]class Report{public string result,apk,backend;public int errors,warnings;public long bytes;public double seconds;public bool deviceTested;}
}
