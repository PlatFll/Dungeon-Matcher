using UnityEngine;

/// <summary>Optional, cell-local motion. The board removes gameplay vines immediately.</summary>
public sealed class VineOverlayMotion : MonoBehaviour
{
    private SpriteRenderer view;
    private Sprite[] frames;
    private Sprite resting;
    private float elapsed;
    private bool retiring;
    public const float FrameSeconds = .045f;

    public void Initialize(SpriteRenderer renderer, Sprite still, Sprite[] spread, bool animate)
    {
        view=renderer;resting=still;
        frames=animate && !PresentationPreferences.ReducedMotion ? spread : null;
        view.sprite=frames!=null && frames.Length>0 ? frames[0] : resting;
    }
    public void Retire(Sprite[] hit)
    {
        retiring=true;elapsed=0;
        frames=PresentationPreferences.ReducedMotion ? null : hit;
        if(frames==null || frames.Length==0) { gameObject.SetActive(false);Destroy(gameObject);return; }
        view.sprite=frames[0];
    }
    private void Update()
    {
        if(view==null || frames==null || frames.Length==0) return;
        elapsed+=Time.deltaTime;
        int frame=Mathf.FloorToInt(elapsed/(retiring ? .035f : FrameSeconds));
        if(frame<frames.Length) { view.sprite=frames[frame];return; }
        frames=null;
        if(retiring) { gameObject.SetActive(false);Destroy(gameObject); }
        else view.sprite=resting;
    }
}
