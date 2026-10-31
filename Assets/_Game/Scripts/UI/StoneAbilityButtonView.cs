using UnityEngine;
using UnityEngine.UI;

// Optional shared presentation only. The existing Button/controller own input,
// availability and energy. Motion is a translated stone slab, never stretched art.
[DisallowMultipleComponent]
public sealed class StoneAbilityButtonView : MonoBehaviour
{
    private Image slab, engraving, glow;
    private Button button;
    private float lift, glowClock;
    private bool ready;
    public RectTransform Slab => slab ? slab.rectTransform : null;
    public Image Engraving => engraving;
    public Image Glow => glow;
    public bool IsRaised => ready && lift >= 1f;

    public void Present(Button owner, PlayerAbilityController controller, CharacterAbilityDefinition definition)
    {
        if(!owner || !controller || !definition || !definition.StoneEngraving)return;
        button=owner;
        if(!slab)
        {
            slab=Make("StoneSlab",transform,new Vector2(176,66));
            slab.sprite=Resources.Load<Sprite>("UI/BattlePresentation/Stone/AbilityPlate");
            engraving=Make("StoneEngraving",slab.transform,definition.StoneEngraving.rect.size);
            glow=Make("StoneEngravingGlow",slab.transform,definition.StoneEngraving.rect.size);
        }
        slab.gameObject.SetActive(true);
        // The fixed dark socket exposes a small side face when the slab lifts.
        button.image.sprite=Resources.Load<Sprite>("UI/BattlePresentation/Stone/AbilitySocket");
        button.image.color=Color.white;button.image.CrossFadeColor(Color.white,0,true,true);
        button.transition=Selectable.Transition.None;
        engraving.sprite=definition.StoneEngraving;engraving.rectTransform.sizeDelta=engraving.sprite.rect.size;
        glow.sprite=definition.ReadyStoneEngraving;glow.rectTransform.sizeDelta=engraving.sprite.rect.size;
        if(Time.timeScale>0)
        {
            ready=controller.CanActivate;
            lift=PresentationPreferences.ReducedMotion ? (ready?1:0) : Mathf.MoveTowards(lift,ready?1:0,Time.deltaTime/0.16f);
            glowClock+=Time.deltaTime;
        }
        // One native pixel horizontally and three vertically imply slab depth.
        // Reduced Motion retains the ready cue without any positional movement.
        slab.rectTransform.anchoredPosition=PresentationPreferences.ReducedMotion?Vector2.zero:
            new Vector2(-Mathf.Round(lift),Mathf.Round(lift*3));
        GameplayPixelGrid.Snap(slab.rectTransform);
        slab.color=Color.white;engraving.color=Color.white;
        float shine=PresentationPreferences.ReducedMotion ? .75f : Mathf.Round((.72f+.10f*Mathf.Sin(glowClock*3))*8)/8;
        glow.color=new Color(1,1,1,ready?shine:0);
        glow.enabled=glow.sprite!=null;
    }

    public void Hide()
    {
        if(slab)slab.gameObject.SetActive(false);
        lift=0;ready=false;
        if(button){button.transition=Selectable.Transition.ColorTint;button.targetGraphic=button.image;}
    }

    private static Image Make(string name,Transform parent,Vector2 size)
    {
        var rect=GameUi.Rect(name,parent,size,Vector2.zero);
        var image=rect.gameObject.AddComponent<Image>();image.type=Image.Type.Simple;image.raycastTarget=false;
        return image;
    }
}
