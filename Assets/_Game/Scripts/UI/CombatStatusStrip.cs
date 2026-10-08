using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Shared native 24px status badge layout; counters are live Unity text.</summary>
public sealed class CombatStatusStrip : MonoBehaviour
{
    private sealed class Cell { public RectTransform rect; public Image image; public PixelCounterText count; public string description; }
    private readonly List<Cell> cells=new List<Cell>();
    private static readonly Dictionary<string,Sprite> icons=new Dictionary<string,Sprite>();
    public RectTransform Rect => (RectTransform)transform;
    public int VisibleCount { get; private set; }
    public static Sprite Icon(string id)
    {
        if(!icons.TryGetValue(id,out var sprite) || sprite==null)
            icons[id]=sprite=Resources.Load<Sprite>("UI/CombatStatuses/"+id);
        return sprite;
    }
    public void Show(IReadOnlyList<CombatStatusDisplay> values, int columns, Action<string> inspect)
    {
        VisibleCount=values.Count; columns=Mathf.Max(1,columns);
        while(cells.Count<values.Count)
        {
            var cell=new Cell {rect=GameUi.Rect("Status",transform,new Vector2(24,24),Vector2.zero)};
            cell.image=cell.rect.gameObject.AddComponent<Image>();cell.image.raycastTarget=true;
            var button=cell.rect.gameObject.AddComponent<Button>();button.targetGraphic=cell.image;
            button.navigation=new Navigation {mode=Navigation.Mode.None};
            button.onClick.AddListener(()=>inspect?.Invoke(cell.description));
            cell.count=PixelCounterText.Create("Counter",cell.rect,new Vector2(20,12),new Vector2(1,-6),12);
            cell.count.Face.alignment=TextAlignmentOptions.BottomRight;
            cells.Add(cell);
        }
        int rows=Mathf.CeilToInt(values.Count/(float)columns);
        Rect.sizeDelta=new Vector2(Mathf.Min(columns,values.Count)*28,rows*28);
        for(int i=0;i<cells.Count;i++)
        {
            var cell=cells[i];bool visible=i<values.Count;cell.rect.gameObject.SetActive(visible);
            if(!visible)continue;
            var value=values[i];cell.image.sprite=Icon(value.Icon);cell.count.Set(value.Counter,new Color32(253,245,229,255));
            cell.description=value.Name+"\n\n"+value.Description+"\n"+(value.Counter=="·"?"Active while its condition holds.":"Remaining: "+value.Counter);
            int row=i/columns,count=Mathf.Min(columns,values.Count-row*columns);
            cell.rect.anchoredPosition=new Vector2((i%columns-(count-1)*.5f)*28,((rows-1)*.5f-row)*28);
            GameplayPixelGrid.Snap(cell.rect);
        }
        gameObject.SetActive(values.Count>0);
    }
}
