#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;

/// <summary>Editor-only motion for the isolated forest review scene; not a production presentation system.</summary>
[ExecuteAlways]
public sealed class ForestPolishPreviewMotion : MonoBehaviour
{
    public Image[] lights;
    public Vector2[] origins;
    public bool automatic = true;
    float clock;
    void Update()
    {
        if (!Application.isPlaying || !automatic) return;
        if (!PresentationPreferences.ReducedMotion) clock += Time.deltaTime;
        Evaluate(clock, PresentationPreferences.ReducedMotion);
    }
    public void Evaluate(float time, bool reduced)
    {
        if (lights == null || origins == null) return;
        for (int i = 0; i < lights.Length; i++)
        {
            float phase = i * .17f;
            float q = Mathf.Repeat(time / 6f + phase, 1f);
            float alpha = q < .12f ? q / .12f : q < .55f ? 1f : q < .8f ? (.8f - q) / .25f : 0f;
            int bob = reduced ? 0 : Mathf.RoundToInt(Mathf.Sin((time / 6f + phase) * Mathf.PI * 2));
            lights[i].rectTransform.anchoredPosition = origins[i] + new Vector2(0, bob);
            lights[i].color = new Color(1, 1, 1, reduced ? .7f : alpha * .88f);
        }
    }
}
#endif
