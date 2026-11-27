using System;
using UnityEngine;

public sealed partial class ShopMenuController
{
    private enum MerchantState { Browsing, Speaking, Leaving }

    private static readonly string[] ImpatientRemarks = {
        "Well... are you gonna buy something or not?",
        "Just counting my coins for me, are you?",
        "I can put a price tag on the view."
    };

    private MerchantState merchantState;
    private Sprite[] humming, talking, goodbye;
    private Sprite expression;
    private AudioSource hum;
    private CanvasGroup shopInput;
    private Action returnHome;
    private float poseTime, untilRemark, untilHum;
    private int nextRemark;
    private bool applicationPaused;

    private void PrepareMerchant(Action back)
    {
        returnHome = back;
        shopInput = page.gameObject.AddComponent<CanvasGroup>();
        humming = LoadPoses("Humming");
        talking = LoadPoses("Talking");
        goodbye = LoadPoses("Goodbye");
        hum = mouse.gameObject.AddComponent<AudioSource>();
        hum.playOnAwake = false;
        hum.spatialBlend = 0;
        hum.volume = .23f;
        hum.clip = Resources.Load<AudioClip>("UI/MerchantMotion/MerchantHum");
        Greet();
    }

    private static Sprite[] LoadPoses(string name)
    {
        var frames = Resources.LoadAll<Sprite>("UI/MerchantMotion/" + name);
        Array.Sort(frames, (a, b) => string.CompareOrdinal(a.name, b.name));
        return frames;
    }

    private void Greet()
    {
        shopInput.interactable = true;
        merchantState = MerchantState.Browsing;
        expression = null;
        mouse.sprite = Art("MouseHappy");
        poseTime = 0;
        untilRemark = 20;
        untilHum = .9f;
        comment.text = "Have a look around!";
        ApplySoundSettings();
    }

    private void Speak(string line, Sprite face = null)
    {
        hum.Stop();
        merchantState = MerchantState.Speaking;
        expression = face;
        mouse.sprite = face != null ? face : Art("MouseHappy");
        comment.text = line;
        poseTime = 0;
        untilHum = 1.4f;
        untilRemark = 20;
    }

    private void LeaveShop()
    {
        if (merchantState == MerchantState.Leaving) return;
        merchantState = MerchantState.Leaving;
        shopInput.interactable = false;
        scroll.StopMovement();
        hum.Stop();
        comment.text = "See you 'round!";
        poseTime = 0;
        mouse.sprite = goodbye.Length > 0 ? goodbye[0] : Art("MouseHappy");
    }

    private void Update()
    {
        if (!initialized || applicationPaused) return;
        float dt = Time.unscaledDeltaTime;
        poseTime += dt;
        bool reduced = PresentationPreferences.ReducedMotion;
        if (merchantState == MerchantState.Leaving)
        {
            if (goodbye.Length > 0)
                mouse.sprite = goodbye[reduced ? Mathf.Min(4, goodbye.Length - 1) :
                    Mathf.Min((int)(poseTime / .05f), goodbye.Length - 1)];
            if (poseTime >= (reduced ? .28f : 1f)) returnHome?.Invoke();
            return;
        }

        bool canChat = account.ActiveRun == null && account.LastError == null;
        untilRemark -= dt;
        if (canChat && untilRemark <= 0)
        {
            Speak(ImpatientRemarks[nextRemark]);
            nextRemark = (nextRemark + 1) % ImpatientRemarks.Length;
            untilRemark = 30;
        }

        if (merchantState == MerchantState.Speaking)
        {
            if (expression == null && !reduced && poseTime < .85f && talking.Length > 0)
                mouse.sprite = talking[(int)(poseTime / .095f) % talking.Length];
            else mouse.sprite = expression != null ? expression : Art("MouseHappy");
            if (poseTime < 3.8f) return;
            merchantState = MerchantState.Browsing;
            poseTime = 0;
            if (canChat) comment.text = "Hmm, hm-hmm...";
        }

        mouse.sprite = !reduced && humming.Length > 0 ? humming[(int)(poseTime / .16f) % humming.Length] : Art("MouseHappy");
        untilHum -= dt;
        if (untilHum <= 0)
        {
            if (!AudioPreferences.SfxMuted && hum.clip != null) hum.Play();
            untilHum = 8.4f;
        }
    }

    private void ApplySoundSettings()
    {
        if (hum != null && AudioPreferences.SfxMuted) hum.Stop();
    }

    private void OnApplicationPause(bool paused)
    {
        applicationPaused = paused;
        if (paused && hum != null) hum.Stop();
    }
}
