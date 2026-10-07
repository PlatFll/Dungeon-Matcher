using UnityEngine;

/// <summary>Optional glow child; never changes durability, parent scale or hit-flash material.</summary>
[DisallowMultipleComponent]
public sealed class RootLifePulse : MonoBehaviour
{
    private SpriteRenderer source,glow;
    private Color tint;
    public void Bind(SpriteRenderer root,bool heart)
    {
        source=root;tint=heart?new Color(1f,.64f,.4f):new Color(.72f,1f,.52f);
        if(glow==null)
        {
            var child=new GameObject("Root life glow");child.transform.SetParent(transform,false);
            glow=child.AddComponent<SpriteRenderer>();glow.maskInteraction=root.maskInteraction;
        }
        glow.sprite=root.sprite;glow.sortingLayerID=root.sortingLayerID;glow.sortingOrder=root.sortingOrder+1;
    }
    private void LateUpdate()
    {
        if(source==null || glow==null) return;
        glow.enabled=source.enabled;glow.sprite=source.sprite;
        float pulse=PresentationPreferences.ReducedMotion?1:Mathf.Floor((Mathf.Sin(Time.time*2.2f)+1)*1.5f);
        tint.a=(.025f+.025f*pulse)*source.color.a;glow.color=tint;
    }
}
