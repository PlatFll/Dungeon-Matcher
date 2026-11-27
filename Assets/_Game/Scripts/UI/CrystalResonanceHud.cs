using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Presentation beneath the existing WaveTracker owner. Never awards or gates gameplay.</summary>
[DisallowMultipleComponent]
public sealed class CrystalResonanceHud : MonoBehaviour
{
    private CrystalResonanceRuntime resonance;
    private RectTransform root, track;
    private TMP_Text score, caption, notice;
    private Image committed, pending, crystal, highlight, noticeBacking;
    private readonly Image[] notches = new Image[3];
    private readonly UIPixelParticle[] motes = new UIPixelParticle[28];
    private Vector2 priorSize;
    private float priorScale, phase, emission, flash, noticeTime, trackWidth, pixel = 1;
    private int previousCharge, previousMask, serial;
    private bool reduced;
    private string idleNotice = "";
    private static readonly Color Pink = new Color32(214,56,80,255);
    private static readonly Color Pale = new Color32(255,182,185,255);
    private static readonly Color Light = new Color32(255,224,255,255);
    private static readonly Color Gold = new Color32(197,136,38,255);

    public void Initialize(TMP_Text existingScore)
    {
        resonance = RunSession.Current.Resonance; root = (RectTransform)transform; score = existingScore;
        crystal = Picture("ResonanceStoryCrystal", root, Color.white);
        crystal.sprite = Resources.Load<Sprite>("UI/Finalized/SplitStoryGem"); crystal.preserveAspect = true;
        track = GameUi.Rect("ResonanceTrack", root, Vector2.zero, Vector2.zero);
        var well = track.gameObject.AddComponent<Image>(); well.color = new Color32(36,31,58,255); well.raycastTarget = false;
        committed = Picture("Committed", track, Pink); pending = Picture("Provisional", track, Pale);
        highlight = Picture("ChargeGlint", root, Light);
        for (int i = 0; i < notches.Length; i++) notches[i] = Picture("CardNotch" + resonance.Tuning.milestones[i], track, Gold);
        caption = GameUi.Label("ResonanceCaption", root, "", Vector2.zero, Vector2.zero, 16);
        noticeBacking = Picture("ResonanceFeedbackBacking", root, new Color32(10,13,17,255));
        notice = GameUi.Label("ResonanceFeedback", root, "", Vector2.zero, Vector2.zero, 16);
        caption.textWrappingMode = notice.textWrappingMode = TextWrappingModes.NoWrap;
        for (int i = 0; i < motes.Length; i++)
        {
            var r = GameUi.Rect("ResonancePixel" + i, root, Vector2.one, Vector2.zero);
            r.gameObject.AddComponent<Image>().raycastTarget = false;
            motes[i] = r.gameObject.AddComponent<UIPixelParticle>(); motes[i].Stop();
        }
        previousCharge = resonance.Committed; previousMask = resonance.State.triggeredMask;
        resonance.Changed += Changed; resonance.MasteryEarned += Mastery; resonance.CapstoneApproaching += Approaching;
        Changed(); Layout();
    }
    private static Image Picture(string name, Transform parent, Color color)
    {
        var rect = GameUi.Rect(name, parent, Vector2.zero, Vector2.zero);
        var image = rect.gameObject.AddComponent<Image>(); image.color = color; image.raycastTarget = false; return image;
    }
    private static void Position(RectTransform rect, float x, float y, float width, float height)
    {
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0,1);
        rect.anchoredPosition = new Vector2(x,-y); rect.sizeDelta = new Vector2(width,height);
    }
    private void Layout()
    {
        float scale = score != null && score.canvas != null ? score.canvas.rootCanvas.scaleFactor : 1;
        if (priorSize == root.rect.size && Mathf.Approximately(priorScale,scale)) return;
        priorSize = root.rect.size; priorScale = scale;
        float w = root.rect.width, h = root.rect.height, s = h / 66f;
        // Existing frame, settings, actor anchors and board hitboxes stay in their original layout.
        Position(crystal.rectTransform, 15*s, 24*s, 28*s, 26*s);
        GameplayPixelGrid.FitImage(crystal, new Vector2(28,26)*s);
        pixel = crystal.sprite != null ? crystal.rectTransform.rect.width / crystal.sprite.rect.width : 1/scale;
        // The crystal rounds up to a full source pixel scale on compact phones.
        float left = Mathf.Max(44*s, crystal.rectTransform.anchoredPosition.x + crystal.rectTransform.rect.width + 3*pixel);
        float width = Mathf.Max(pixel, w - 28*s - left);
        Position(score.rectTransform, left, 18*s, width, 20*s);
        score.alignment = TextAlignmentOptions.Center;
        RefreshScore();
        Position(caption.rectTransform,left,32*s,width,13*s);
        RefreshCaption();
        trackWidth = Mathf.Floor(width/pixel)*pixel;
        Position(track,left,44*s,trackWidth,4*pixel);
        Position(notice.rectTransform,0,h+1*s,w,16*s); PixelTextFitter.ApplyNativeMenu(notice,16*s);
        for (int i=0;i<notches.Length;i++)
        {
            float x=Mathf.Round(trackWidth*resonance.Tuning.milestones[i]/100f/pixel)*pixel;
            Position(notches[i].rectTransform,x, -pixel, pixel,6*pixel);
        }
        Position(highlight.rectTransform,27*s,29*s,2*pixel,pixel);
        DrawFill(); DrawNoticeBacking();
    }
    private void RefreshScore()
    {
        float scale=score.canvas!=null?Mathf.Max(.01f,score.canvas.rootCanvas.scaleFactor):1;
        string em=(16/scale).ToString("0.###",System.Globalization.CultureInfo.InvariantCulture);
        string value = resonance.RunScore.ToString("N0");
        score.richText = true;
        score.text = "<size=" + em + ">SCORE</size> " + value;
        PixelTextFitter.ApplyNativeMenu(score, 28 * root.rect.height / 66f);
        if (score.preferredWidth > score.rectTransform.rect.width)
        {
            score.text = value + " PTS";
            if (score.preferredWidth > score.rectTransform.rect.width) score.text = value;
        }
    }
    private void RefreshCaption()
    {
        string amount = resonance.Committed + "%";
        string state = resonance.Sealed ? "SEALED" : resonance.PortalReady ? "READY" : "";
        caption.text = state.Length > 0 ? amount + " " + state : "RESONANCE " + amount;
        PixelTextFitter.ApplyNativeMenu(caption, 16 * root.rect.height / 66f);
        if (state.Length == 0 && caption.preferredWidth > caption.rectTransform.rect.width)
            caption.text = "CHARGE " + amount;
    }
    private void Changed()
    {
        RefreshScore();
        RefreshCaption();
        if (resonance.State.triggeredMask != previousMask)
        { flash=0.5f; CombatAudioController.PlayMechanism(CombatSoundCue.ResonanceNotch); }
        if (previousCharge < 100 && resonance.Committed >= 100)
        { flash=.65f; Burst(8); CombatAudioController.PlayMechanism(CombatSoundCue.ResonanceFull); }
        previousCharge=resonance.Committed; previousMask=resonance.State.triggeredMask;
        idleNotice=resonance.Pending>0 ? "+"+resonance.Pending+" PENDING VICTORY" : NextCard();
        DrawFill();
    }
    private void DrawFill()
    {
        if (track == null || trackWidth <= 0) return;
        float fill=Mathf.Round(trackWidth*resonance.Committed/100f/pixel)*pixel;
        float preview=Mathf.Round(trackWidth*Mathf.Min(100,resonance.Committed+resonance.Pending)/100f/pixel)*pixel;
        Position(committed.rectTransform,0,0,fill,track.rect.height);
        Position(pending.rectTransform,fill,0,Mathf.Max(0,preview-fill),track.rect.height);
        for (int i=0;i<notches.Length;i++)
            notches[i].color=(resonance.State.claimedMask&(1<<i))!=0 ? Light :
                (resonance.State.triggeredMask&(1<<i))!=0 ? Pale : Gold;
    }
    private void DrawNoticeBacking()
    {
        if (noticeBacking == null || notice == null) return;
        noticeBacking.enabled = !string.IsNullOrEmpty(notice.text);
        if (!noticeBacking.enabled) return;
        PixelTextFitter.ApplyNativeMenu(notice,16*root.rect.height/66f);
        float width = Mathf.Min(root.rect.width, Mathf.Ceil((notice.preferredWidth + 6*pixel)/pixel)*pixel);
        float x = Mathf.Round((root.rect.width-width)*.5f/pixel)*pixel;
        float top = -notice.rectTransform.anchoredPosition.y;
        float y = Mathf.Floor((top-pixel)/pixel)*pixel;
        float bottom = Mathf.Ceil((top+notice.rectTransform.rect.height+pixel)/pixel)*pixel;
        Position(noticeBacking.rectTransform,x,y,width,bottom-y);
    }
    private void Mastery(ResonanceMastery earned)
    {
        notice.text="+1 RESONANCE  (PENDING)"; notice.color=Light; noticeTime=1.4f; flash=.3f;
        DrawNoticeBacking();
        CombatAudioController.PlayMechanism(CombatSoundCue.ResonanceMastery);
        if (PresentationPreferences.ReducedMotion) return;
        var mote=FreeMote(); if (mote==null) return;
        var canvas=score.canvas.rootCanvas;
        var screen=Camera.main!=null ? (Vector2)Camera.main.WorldToScreenPoint(resonance.LastMasteryOrigin) : (Vector2)resonance.LastMasteryOrigin;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(root,screen,canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.worldCamera,out var start);
        start-=root.rect.center;
        Vector2 end=(Vector2)track.localPosition+new Vector2(trackWidth*.5f,0)-root.rect.center;
        mote.PlayToward(start,end,.4f,2*pixel,Light,pixel);
    }
    private void Approaching()
    {
        notice.text="A POWERFUL PRESENCE APPROACHES";notice.color=Pale;noticeTime=2.2f;flash=.55f;
        DrawNoticeBacking();
        CombatAudioController.PlayMechanism(CombatSoundCue.CapstoneApproach);
    }
    public void Tick()
    {
        if (resonance == null || score == null) return;
        Layout();
        bool nowReduced=PresentationPreferences.ReducedMotion;
        if (nowReduced && !reduced) foreach (var mote in motes) mote.Stop();
        reduced=nowReduced;
        float dt=Time.deltaTime;
        if (dt<=0 || RunSession.Current?.Continuation?.IsRestoring==true) return;
        phase+=dt; flash=Mathf.Max(0,flash-dt);noticeTime=Mathf.Max(0,noticeTime-dt);
        if (noticeTime==0 && notice.text!=idleNotice) { notice.text=idleNotice; DrawNoticeBacking(); }
        pending.color=reduced || (int)(phase*4)%2==0 ? Pale : Light;
        highlight.enabled=resonance.Committed>=70 || flash>0;
        highlight.color=resonance.Committed>=100 || flash>0 ? Light : Pale;
        if (reduced) return;
        // A bounded local pool; no gameplay randomness, blurred particles or per-frame spawning allocations.
        if (resonance.Committed>=70)
        {
            emission+=dt;
            float interval=resonance.Committed>=100 ? .045f : resonance.Committed>=80 ? .065f : .28f;
            if (emission>=interval) { emission=0; Burst(resonance.Committed>=80?2:1); }
        }
        for(int i=0;i<notches.Length;i++)
            if ((resonance.State.triggeredMask&(1<<i))==0 && resonance.Committed>=resonance.Tuning.milestones[i]-4)
                notches[i].color=(int)(phase*.7f)%2==0 ? Gold : Pale;
    }
    private string NextCard()
    {
        for(int i=0;i<notches.Length;i++)
            if ((resonance.State.claimedMask&(1<<i))==0) return "NEXT CARD " + resonance.Tuning.milestones[i];
        return resonance.Sealed ? "DEFEAT THE CAPSTONE" : resonance.State.capstonePhase==ResonanceCapstonePhase.Defeated ? "CRYSTAL UNSEALED" : "";
    }
    private UIPixelParticle FreeMote()
    { foreach(var mote in motes) if (!mote.IsPlaying) return mote; return null; }
    private void Burst(int count)
    {
        if (PresentationPreferences.ReducedMotion || Time.timeScale<=0) return;
        for(int i=0;i<count;i++)
        {
            var mote=FreeMote();if(mote==null)return;
            serial++;int direction=serial%2==0?-1:1;
            Vector2 start=(Vector2)crystal.rectTransform.localPosition+new Vector2(14,-9)*pixel-root.rect.center;
            mote.Play(start,new Vector2(direction*(8+serial%7),14+serial%9)*pixel,.7f,
                (1+serial%3)*pixel,serial%3==0?Light:Pale,true,pixel);
        }
    }
    private void OnDestroy()
    {
        if(resonance==null)return;
        resonance.Changed-=Changed;resonance.MasteryEarned-=Mastery;resonance.CapstoneApproaching-=Approaching;
    }
}
