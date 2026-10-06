using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>Transient presentation only. Never starts or completes a combat action.</summary>
[DefaultExecutionOrder(11010)]
public sealed class EnemyCastAnnouncement : MonoBehaviour
{
    private sealed class Entry { public TMP_Text text; public float age; }
    private readonly List<Entry> entries=new List<Entry>();
    private EnemyActor actor;
    private RectTransform visual;
    public int VisibleCount => entries.Count;
    public void Show(string displayName)
    {
        actor=GetComponent<EnemyActor>();
        var slot=GetComponentInParent<EnemySlotUI>();
        if(slot==null || actor==null || actor.IsDefeated) return;
        visual=transform.Find("VisualRoot") as RectTransform;
        var label=GameUi.Label("CastAnnouncement",slot.transform,displayName,new Vector2(136,28),Vector2.zero,12);
        label.color=new Color32(235,234,221,255);label.raycastTarget=false;
        label.textWrappingMode=TextWrappingModes.Normal;label.enableAutoSizing=true;
        label.fontSizeMin=10;label.fontSizeMax=12;
        entries.Add(new Entry{text=label});Position();
    }
    private void LateUpdate()
    {
        if(actor==null || actor.IsDefeated) {Clear();return;}
        for(int i=entries.Count-1;i>=0;i--)
        {
            var e=entries[i];e.age+=Time.deltaTime;
            if(e.age>=1.25f) {Destroy(e.text.gameObject);entries.RemoveAt(i);continue;}
            var c=e.text.color;c.a=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.4f,1.25f,e.age));e.text.color=c;
        }
        Position();
    }
    private void Position()
    {
        for(int i=0;i<entries.Count;i++)
        {
            var e=entries[i];var anchor=visual!=null?visual.TransformPoint(new Vector3(visual.rect.center.x,visual.rect.yMax,0)):transform.position;
            e.text.rectTransform.position=anchor;e.text.rectTransform.anchoredPosition+=Vector2.up*(36+10*Mathf.Clamp01(e.age/1.25f)+i*14);
            GameplayPixelGrid.Snap(e.text.rectTransform);
        }
    }
    private void Clear() {foreach(var e in entries) if(e.text!=null) Destroy(e.text.gameObject);entries.Clear();}
    private void OnDisable()=>Clear();
    private void OnDestroy()=>Clear();
}
