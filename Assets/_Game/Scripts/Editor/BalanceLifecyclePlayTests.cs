using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;

public sealed class BalanceLifecyclePlayTests
{
    [UnityTest] public IEnumerator ExistingLifecycleAssertionsWithActualUnityCallbacks()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        yield return new EnterPlayMode();
        Assert.That(Application.isPlaying,Is.True,"Lifecycle assertions require real Unity callbacks.");
        string path=Path.GetFullPath(".utmp/LifecycleProfiles/"+Guid.NewGuid().ToString("N")+".json");
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        var save=new AccountSave();
        foreach(GemSpecialType type in Enum.GetValues(typeof(GemSpecialType)))
            if(BalanceV1.Current.UnlockLevel(type)!=int.MaxValue&&!save.unlocked.Contains(type))save.unlocked.Add(type);
        File.WriteAllText(path,JsonUtility.ToJson(save));
        var failures=new List<string>();int passed=0;
        using(AccountProgression.UseDisposableProfile(path))
        using(GemMasterySettings.UseTemporaryLoadout(new GemMasteryLoadout(GemMasteryReward.ColorCrystal,
            GemMasteryReward.PoisonBomb,GemMasteryReward.HealBomb,GemMasteryReward.ShieldBomb)))
        {
            foreach(Type fixtureType in new[]{typeof(CrackedCenterRewardTests),typeof(CrackedChainUpgradeScopeTests),
                typeof(RunUpgradeLifecycleTests),typeof(UpgradeIntermissionLifecycleTests),typeof(PlayerAbilityLifecycleTests)})
            foreach(var method in fixtureType.GetMethods().OrderBy(m=>m.Name))
            {
                var cases=method.GetCustomAttributes<TestCaseAttribute>().Select(c=>c.Arguments).ToList();
                if(method.GetCustomAttribute<TestAttribute>()!=null)cases.Add(Array.Empty<object>());
                foreach(var arguments in cases)
                {
                    object fixture=Activator.CreateInstance(fixtureType);
                    string name=fixtureType.Name+"."+method.Name+"("+string.Join(",",arguments)+")";
                    try
                    {
                        foreach(var setup in fixtureType.GetMethods().Where(m=>m.GetCustomAttribute<SetUpAttribute>()!=null))setup.Invoke(fixture,null);
                        method.Invoke(fixture,arguments);passed++;
                    }
                    catch(Exception error){failures.Add(name+"\n"+(error is TargetInvocationException?error.InnerException:error));}
                    finally
                    {
                        foreach(var cleanup in fixtureType.GetMethods().Where(m=>m.GetCustomAttribute<TearDownAttribute>()!=null))
                            try{cleanup.Invoke(fixture,null);}catch(Exception error){failures.Add(name+" teardown: "+error);}
                    }
                }
            }
        }
        File.WriteAllText(".utmp/balance-lifecycle-play.txt",$"Executed in Unity Play Mode: {passed} passed; {failures.Count} failed.\n"+string.Join("\n\n",failures));
        yield return new ExitPlayMode();
        Assert.That(failures,Is.Empty,string.Join("\n",failures));
    }
}
