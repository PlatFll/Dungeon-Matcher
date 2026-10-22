using UnityEngine;
using UnityEngine.UI;

public sealed partial class DungeonPixelLabBattleArt
{
    // Separate approved ornaments remain native sprites, outside the panel's
    // interior mask and behind its live label/icon. They never receive input.
    void DecorateHybridPanel(RectTransform root, string family, float unit)
    {
        if (family != "Wave" && family != "Resonance") return;
        var decor = root.Find("Hybrid ornaments") as RectTransform;
        if (!decor)
        {
            decor = Rect("Hybrid ornaments", root, 0, 0, 0, 0);
            decor.anchorMin = Vector2.zero; decor.anchorMax = Vector2.one;
            decor.offsetMin = decor.offsetMax = Vector2.zero;
        }
        float w = root.rect.width / unit, h = root.rect.height / unit;
        Ornament(decor, family + "_Wing_Left", -3, Mathf.Floor((h-15)/2), unit);
        Ornament(decor, family + "_Wing_Right", w-4, Mathf.Floor((h-15)/2), unit);
        if (family == "Wave") Ornament(decor,"Wave_Skull",Mathf.Floor((w-15)/2),h-4,unit);
        else
        {
            Ornament(decor,"Resonance_Clamp",Mathf.Floor((w-7)/2),1,unit,"Upper amethyst");
            Ornament(decor,"Resonance_Clamp",Mathf.Floor((w-7)/2),h-8,unit,"Lower amethyst");
        }
        decor.SetAsLastSibling();
    }
    void Ornament(RectTransform root,string sprite,float x,float y,float unit,string name=null)
    {
        name ??= sprite;
        var r=root.Find(name) as RectTransform;
        if(!r) {r=Image(sprite,root,x,y,unit).rectTransform;r.name=name;}
        var source=Sprite(sprite);
        TopRect(r,x*unit,y*unit,source.rect.width*unit,source.rect.height*unit);
        GameplayPixelGrid.Snap(r);
    }
}
