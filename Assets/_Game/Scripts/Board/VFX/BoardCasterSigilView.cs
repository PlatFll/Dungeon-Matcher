using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>WHO markers. Existing countdowns/telegraphs still communicate WHEN.</summary>
[DefaultExecutionOrder(11300)]
public sealed class BoardCasterSigilView : MonoBehaviour
{
    private BoardController board;
    private readonly List<CasterBoardTarget> targets=new List<CasterBoardTarget>();
    private readonly List<Image> pool=new List<Image>();
    private RectTransform overlay;
    private Canvas canvas;
    private readonly Dictionary<Vector2Int,int> groups=new Dictionary<Vector2Int,int>();
    private readonly List<Vector2Int> order=new List<Vector2Int>();
    public IReadOnlyList<Image> Images=>pool;
    public int ActiveCount {get;private set;}
    private void Awake()=>board=GetComponent<BoardController>();
    private void LateUpdate()
    {
        var run=RunSession.Current;
        if(board==null || run?.Board!=board || run.Waves==null) {Hide();return;}
        if(!EnsureOverlay()) {Hide();return;}
        board.CollectCasterTargets(targets);
        foreach(var actor in run.Waves.ActiveEnemies)
        {
            if(actor==null || actor.IsDefeated) continue;
            var aquatic=actor.GetComponent<AquaticEnemyAbility>();
            if(aquatic?.IsPreparing!=true) continue;
            foreach(var cell in aquatic.ResponseCells) targets.Add(CasterBoardTarget.OnCell(actor,cell));
            foreach(int id in aquatic.MarkedBubbles)
            {
                var gem=board.FindAquaticGem(id);
                if(gem!=null && board.IsFlooded && board.Aquatic.bubbles.Contains(id)) targets.Add(CasterBoardTarget.OnGem(actor,gem));
            }
            if(aquatic.CofferTarget is Vector2Int site) targets.Add(CasterBoardTarget.OnCell(actor,site));
        }
        groups.Clear();order.Clear();
        foreach(var target in targets)
        {
            if(target.Owner==null || target.Owner.IsDefeated || (target.Kind==CasterTargetKind.Gem && target.Gem==null))continue;
            int slot=run.Waves.ContinuationSlot(target.Owner);if(slot<0 || slot>2)continue;
            var position=board.CasterTargetLocalPosition(target);
            if(target.Kind==CasterTargetKind.Gem || target.Kind==CasterTargetKind.Cell)
                position+=new Vector3(.32f,.32f)*board.CellSize;
            // Merge gem and cell marks when their visible anchors coincide.
            var key=new Vector2Int(Mathf.RoundToInt(position.x/board.CellSize*64),Mathf.RoundToInt(position.y/board.CellSize*64));
            if(!groups.ContainsKey(key)) {groups.Add(key,0);order.Add(key);}
            groups[key]|=1<<slot;
        }
        ActiveCount=0;
        foreach(var key in order)
        {
            int mask=groups[key],count=0;for(int s=0;s<3;s++)if((mask&(1<<s))!=0)count++;
            int index=0;
            for(int slot=0;slot<3;slot++) if((mask&(1<<slot))!=0)
            {
                var sprite=CasterSigilArt.ForSlot(slot);
                if(sprite==null) {index++;continue;}
                var icon=Take();icon.sprite=sprite;icon.name="CasterSigil_"+slot;
                var local=new Vector3(key.x/64f-(count-1-index)*13f/64,key.y/64f)*board.CellSize;
                var world=board.transform.TransformPoint(local);
                var screen=Camera.main.WorldToScreenPoint(world);
                var right=Camera.main.WorldToScreenPoint(board.transform.TransformPoint(local+Vector3.right*board.CellSize*12f/64));
                RectTransformUtility.ScreenPointToLocalPointInRectangle(overlay,screen,null,out var point);
                icon.rectTransform.anchoredPosition=point;
                icon.rectTransform.sizeDelta=Vector2.one*Mathf.Max(1,Mathf.Round(Mathf.Abs(right.x-screen.x)))/canvas.scaleFactor;
                icon.enabled=true;index++;
            }
        }
        for(int i=ActiveCount;i<pool.Count;i++)pool[i].enabled=false;
    }
    private bool EnsureOverlay()
    {
        if(overlay!=null)return Camera.main!=null;
        var layout=FindFirstObjectByType<GameplayPixelLayoutController>();
        if(layout==null || Camera.main==null)return false;
        var source=layout.GetComponentInParent<Canvas>().rootCanvas;
        overlay=GameUi.Rect("CasterBoardSigils",source.transform,Vector2.zero,Vector2.zero);
        overlay.anchorMin=Vector2.zero;overlay.anchorMax=Vector2.one;overlay.offsetMin=overlay.offsetMax=Vector2.zero;
        canvas=overlay.gameObject.AddComponent<Canvas>();canvas.overrideSorting=true;canvas.sortingOrder=source.sortingOrder+1;
        // The frame is overlay UI. The sigils must render above it to remain
        // visible outside a row/column, but below menus and travel smoke.
        return true;
    }
    private Image Take()
    {
        if(ActiveCount==pool.Count)
        {
            var rect=GameUi.Rect("CasterSigil",overlay,Vector2.zero,Vector2.zero);
            var icon=rect.gameObject.AddComponent<Image>();icon.raycastTarget=false;pool.Add(icon);
        }
        return pool[ActiveCount++];
    }
    private void Hide(){ActiveCount=0;foreach(var r in pool)if(r!=null)r.enabled=false;}
    private void OnDisable()=>Hide();
    private void OnDestroy(){if(overlay!=null)Destroy(overlay.gameObject);}
}
