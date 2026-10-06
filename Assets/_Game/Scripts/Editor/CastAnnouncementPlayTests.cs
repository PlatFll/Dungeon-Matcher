using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;

public sealed partial class ForestFoundationPlayTests
{
    [UnityTest] public IEnumerator CourtCastAnnouncementsCommitOncePauseFadeAndDoNotReplayOnResume()
    {
        yield return LaunchCourt("moray_siphoner","shellback_porter");
        var actor=Enemy("moray_siphoner");var casts=new List<string>();
        actor.AbilityCastCommitted+=(a,n)=>{casts.Add(n);Time.timeScale=0;};
        for(int i=0;i<3;i++) yield return Move();
        Assert.That(casts,Is.Empty);
        var moving=StartCoroutineForTest(Move());
        yield return Until(()=>casts.Count>0,"Siphon commits its channel");
        yield return null;
        Assert.That(casts,Is.EqualTo(new[]{"Siphon"}));
        var label=actor.GetComponentInParent<EnemySlotUI>().GetComponentsInChildren<TMP_Text>().Single(t=>t.name=="CastAnnouncement");
        Assert.That(label.font,Is.SameAs(GameUi.TmpFont));
        Assert.That(label.color.r,Is.GreaterThan(.85f));
        string output=System.IO.Path.GetFullPath(".utmp/StatusRevision/Visual");System.IO.Directory.CreateDirectory(output);
        ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(output,"cast-announcement.png"));yield return null;yield return null;
        var position=label.rectTransform.position;float alpha=label.color.a;
        yield return new WaitForSecondsRealtime(.15f);
        Assert.That(label.rectTransform.position,Is.EqualTo(position));Assert.That(label.color.a,Is.EqualTo(alpha));
        Time.timeScale=1;yield return moving;
        float expires=Time.time+1.4f;yield return Until(()=>Time.time>=expires,"announcement expires");
        Assert.That(actor.GetComponent<EnemyCastAnnouncement>().VisibleCount,Is.Zero);
        yield return ResumeCourtCheckpoint();
        actor=Enemy("moray_siphoner");Assert.That(actor.GetComponent<AquaticEnemyAbility>().IsPreparing,Is.True);
        Assert.That(actor.GetComponent<EnemyCastAnnouncement>(),Is.Null,"restore does not announce the same channel again");
        yield return Move();yield return Move();
        Assert.That(actor.GetComponent<EnemyCastAnnouncement>(),Is.Null,"release and recovery do not announce each channel tick");
    }
    private Coroutine StartCoroutineForTest(IEnumerator routine)=>Run.StartCoroutine(routine);
}
