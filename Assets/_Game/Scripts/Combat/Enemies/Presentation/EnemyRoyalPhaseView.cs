using UnityEngine;
using UnityEngine.UI;
using TMPro;

public sealed class EnemyRoyalPhaseView : MonoBehaviour
{
    private KingEnemyAbility king;
    private GameObject icon;
    private TextMeshProUGUI callout;
    private float hideCalloutAt;
    public void Initialize(KingEnemyAbility owner)
    {
        Unsubscribe();
        king = owner; king.Enraged += Show;
        king.CommandIssued += ShowCommand; king.HeavyStrike += ShowStrike;
    }
    private void Show(KingEnemyAbility owner)
    {
        if (icon != null) return;
        icon = new GameObject("Crown Last Stand", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        icon.transform.SetParent(transform, false);
        var rect = (RectTransform)icon.transform;
        rect.anchoredPosition = new Vector2(0, 78); rect.sizeDelta = new Vector2(40, 5);
        var image = icon.GetComponent<Image>(); image.color = new Color(1f, 0.35f, 0.1f); image.raycastTarget = false;
    }
    private void ShowCommand(KingEnemyAbility owner) => Callout("ASSAULT", 0.6f);
    private void ShowStrike(KingEnemyAbility owner) => Callout("!", 0.2f);
    private void Callout(string text, float duration)
    {
        if (callout == null)
        {
            var go = new GameObject("Royal Command", typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(transform,false); callout=go.AddComponent<TextMeshProUGUI>();
            callout.rectTransform.anchoredPosition=new Vector2(0,92); callout.rectTransform.sizeDelta=new Vector2(140,30);
            callout.alignment=TextAlignmentOptions.Center; callout.fontSize=20; callout.raycastTarget=false;
            callout.color=new Color(1f,0.88f,0.4f);
        }
        callout.text=text; callout.enabled=true; hideCalloutAt=Time.time+duration;
    }
    private void Update() { if(callout!=null && Time.time>=hideCalloutAt) callout.enabled=false; }
    private void Unsubscribe()
    {
        if(king==null) return;
        king.Enraged-=Show; king.CommandIssued-=ShowCommand; king.HeavyStrike-=ShowStrike;
    }
    private void OnDestroy() { Unsubscribe(); }
}
