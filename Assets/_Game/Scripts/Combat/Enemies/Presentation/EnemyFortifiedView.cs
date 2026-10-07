using UnityEngine;
using UnityEngine.UI;

/// <summary>Two native pearls orbit the visual; the actor alone owns the stacks.</summary>
[DefaultExecutionOrder(11210)]
public sealed class EnemyFortifiedView : MonoBehaviour
{
    private EnemyActor actor;
    private RectTransform visual;
    private Image portrait;
    private readonly Image[] pearls=new Image[2];
    private Image pop;
    private float phase,popUntil;
    private void Awake()
    {
        actor=GetComponent<EnemyActor>();visual=transform.Find("VisualRoot") as RectTransform;
        portrait=visual!=null?visual.GetComponent<Image>():null;
        if(actor!=null)actor.FortifiedConsumed+=Consumed;
    }
    private Image Create(string name,Sprite sprite)
    {
        if(sprite==null || visual==null)return null;
        var rect=GameUi.Rect(name,visual.parent,sprite.rect.size,Vector2.zero);
        var image=rect.gameObject.AddComponent<Image>();image.sprite=sprite;image.raycastTarget=false;
        return image;
    }
    private void LateUpdate()
    {
        if(actor==null || visual==null || portrait?.sprite==null)return;
        var theme=GameplayThemeSkin.Current ?? Resources.Load<GameplayThemeDefinition>("Zones/DrownedCourtTheme");
        int count=actor.FortifiedStacks;
        if(!PresentationPreferences.ReducedMotion)phase+=Time.deltaTime*1.8f;
        float pixel=visual.rect.width/portrait.sprite.rect.width;
        for(int i=0;i<pearls.Length;i++)
        {
            if(i>=count){if(pearls[i]!=null)pearls[i].gameObject.SetActive(false);continue;}
            if(pearls[i]==null)pearls[i]=Create("FortifiedPearl"+i,theme?.tributePearl);
            var pearl=pearls[i];if(pearl==null)continue;pearl.gameObject.SetActive(true);
            float angle=(PresentationPreferences.ReducedMotion?0:phase)+i*Mathf.PI*2/count;
            float depth=Mathf.Sin(angle);
            Vector3 center=new Vector3(visual.rect.center.x,visual.rect.center.y-visual.rect.height*.06f,0);
            pearl.rectTransform.position=visual.TransformPoint(center+new Vector3(Mathf.Cos(angle)*visual.rect.width*.32f,depth*visual.rect.height*.12f,0));
            int sibling=visual.GetSiblingIndex();
            if(pearl.transform.GetSiblingIndex()<sibling)sibling--;
            pearl.transform.SetSiblingIndex(sibling+(depth>0?0:1));
            GameplayPixelGrid.FitImage(pearl,pearl.sprite.rect.size*pixel);
        }
        if(pop!=null)
        {
            pop.gameObject.SetActive(!actor.IsDefeated && Time.time<popUntil);
            if(pop.gameObject.activeSelf)GameplayPixelGrid.FitImage(pop,pop.sprite.rect.size*pixel);
        }
    }
    private void Consumed(EnemyActor source)
    {
        var theme=GameplayThemeSkin.Current ?? Resources.Load<GameplayThemeDefinition>("Zones/DrownedCourtTheme");
        if(pop==null)pop=Create("FortifiedPearlPop",theme?.pearlPop);
        if(pop==null)return;
        int index=Mathf.Clamp(source.FortifiedStacks,0,1);
        pop.rectTransform.position=pearls[index]!=null?pearls[index].transform.position:visual.position;
        pop.transform.SetAsLastSibling();popUntil=Time.time+.16f;
    }
    private void OnDisable()
    {
        foreach(var pearl in pearls)if(pearl!=null)pearl.gameObject.SetActive(false);
        if(pop!=null)pop.gameObject.SetActive(false);
    }
    private void OnDestroy()
    {
        if(actor!=null)actor.FortifiedConsumed-=Consumed;
        foreach(var pearl in pearls)if(pearl!=null)Destroy(pearl.gameObject);
        if(pop!=null)Destroy(pop.gameObject);
    }
}
