using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public sealed class BombLineVFX : MonoBehaviour
{
    SpriteRenderer fallback;
    readonly SpriteRenderer[] heads = new SpriteRenderer[2];
    readonly SpriteRenderer[] trails = new SpriteRenderer[2];
    static Sprite[] headFrames, trailFrames;
    float targetLength, cellSize, lifetime, elapsedTime;
    bool isPlaying;
    Action<BombLineVFX> releaseAction;
    public bool IsPlaying => isPlaying;

    public void ConfigureRendering(Sprite squareSprite, string sortingLayerName, int sortingOrder)
    {
        fallback = GetComponent<SpriteRenderer>();
        fallback.sprite = squareSprite;
        Configure(fallback, sortingLayerName, sortingOrder);
        if (headFrames == null) headFrames = Load("FireHead");
        if (trailFrames == null) trailFrames = Load("FireTrail");
        for (int i = 0; i < 2; i++)
        {
            if (heads[i] == null) heads[i] = Child("FireFront" + i);
            if (trails[i] == null) trails[i] = Child("FireTrail" + i);
            Configure(heads[i], sortingLayerName, sortingOrder + 1);
            Configure(trails[i], sortingLayerName, sortingOrder);
            trails[i].drawMode = SpriteDrawMode.Tiled;
            trails[i].tileMode = SpriteTileMode.Continuous;
            heads[i].flipX = i == 0;
            trails[i].flipX = i == 0;
        }
    }
    static Sprite[] Load(string name)
    {
        var frames = Resources.LoadAll<Sprite>("VFX/BombRefinement/" + name);
        Array.Sort(frames, (a,b) => string.CompareOrdinal(a.name,b.name));
        return frames;
    }
    SpriteRenderer Child(string name)
    {
        var child = new GameObject(name);
        child.transform.SetParent(transform, false);
        return child.AddComponent<SpriteRenderer>();
    }
    static void Configure(SpriteRenderer renderer, string layer, int order)
    {
        renderer.sortingLayerName = layer;
        renderer.sortingOrder = order;
        renderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        renderer.enabled = false;
    }
    public void Play(GemSpecialType type, Vector3 localPosition, float fullLength,
        float boardCellSize, float duration, Action<BombLineVFX> onFinished)
    {
        if (fallback == null || (type != GemSpecialType.RowBomb && type != GemSpecialType.ColumnBomb))
        { onFinished?.Invoke(this); return; }
        targetLength = Mathf.Max(.01f, fullLength);
        cellSize = Mathf.Max(.01f, boardCellSize);
        lifetime = Mathf.Max(.05f, duration);
        elapsedTime = 0;
        releaseAction = onFinished;
        transform.localPosition = localPosition;
        // Only a quarter turn; no arbitrary rotation or stretched sprite pixels.
        transform.localRotation = Quaternion.Euler(0,0,type == GemSpecialType.ColumnBomb ? 90 : 0);
        transform.localScale = Vector3.one * cellSize;
        isPlaying = true;
        gameObject.SetActive(true);
        ApplyVisualState(0);
    }
    void Update()
    {
        if (!isPlaying) return;
        elapsedTime += Time.deltaTime;
        ApplyVisualState(Mathf.Clamp01(elapsedTime / lifetime));
        if (elapsedTime >= lifetime)
        {
            isPlaying = false;
            var callback = releaseAction;
            releaseAction = null;
            Hide();
            callback?.Invoke(this);
        }
    }
    void ApplyVisualState(float t)
    {
        bool reduced = PresentationPreferences.ReducedMotion;
        float progress = reduced ? 1 : Mathf.Clamp01(t / .72f);
        float reach = Mathf.Round(progress * targetLength / (2 * cellSize) * 64) / 64;
        float alpha = reduced ? .38f * (1-t) : 1 - Mathf.InverseLerp(.72f,1,t);
        bool native = headFrames != null && headFrames.Length > 0 && trailFrames != null && trailFrames.Length > 0;
        fallback.enabled = !native;
        if (!native)
        {
            // Missing optional art never blocks the authoritative board clear.
            fallback.transform.localScale = new Vector3(reach * 2 * cellSize,.25f * cellSize,cellSize);
            fallback.color = new Color(1,.55f,.12f,alpha);
            return;
        }
        transform.localScale = Vector3.one * cellSize;
        int frame = reduced ? 0 : Mathf.FloorToInt(elapsedTime / .045f);
        for (int i = 0; i < 2; i++)
        {
            float sign = i == 0 ? -1 : 1;
            heads[i].enabled = !reduced && alpha > 0;
            heads[i].sprite = headFrames[frame % headFrames.Length];
            heads[i].transform.localPosition = new Vector3(sign * reach,0,0);
            heads[i].color = new Color(1,1,1,alpha);
            trails[i].enabled = reach > 0 && alpha > 0;
            trails[i].sprite = trailFrames[frame % trailFrames.Length];
            trails[i].size = new Vector2(reach,.5f);
            trails[i].transform.localPosition = new Vector3(sign * reach / 2,0,0);
            trails[i].color = new Color(1,1,1,alpha);
        }
    }
    void Hide()
    {
        if (fallback != null) fallback.enabled = false;
        for (int i=0;i<2;i++)
        { if(heads[i]!=null) heads[i].enabled=false; if(trails[i]!=null) trails[i].enabled=false; }
    }
    public void StopImmediately() { isPlaying=false;releaseAction=null;Hide();gameObject.SetActive(false); }
    void OnDisable() { isPlaying=false;elapsedTime=0;releaseAction=null;Hide();transform.localScale=Vector3.one; }
}
