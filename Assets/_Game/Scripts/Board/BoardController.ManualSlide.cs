using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class BoardController
{
    // The run supplies environmental/status eligibility. Board rules stay generic.
    public Func<bool> ExtraManualSwapStep { get; set; }
    public bool UsesExtraManualSwapStep => ExtraManualSwapStep?.Invoke() == true;
    public ManualSwapPlan SwapPreview { get; private set; }
    public event Action<ManualSwapPlan> SwapPreviewChanged;
    private Gem previewTapTarget;
    private bool lastExtraStepRule;
    private bool manualRuleCheckPending = true;
    private bool ManualSwapRulesSettled => ExtraManualSwapStep==null || (!manualRuleCheckPending && UsesExtraManualSwapStep==lastExtraStepRule);
    private void LateUpdate()
    {
        bool extra=UsesExtraManualSwapStep;
        if(extra!=lastExtraStepRule)
        {
            lastExtraStepRule=extra;manualRuleCheckPending=true;ClearManualSwapPreview();
        }
        if(!manualRuleCheckPending || gems==null || IsBusy || HasPendingBoardMutation) return;
        // Read-only verification is safe under the Continue/settings overlay.
        // Only an actual reshuffle waits for input ownership and running time.
        if(HasAvailableMove()) {manualRuleCheckPending=false;return;}
        if(IsExternalInputBlocked || Time.timeScale<=0) return;
        manualRuleCheckPending=false;
        // Expiring a movement rule (including a draining tide) may remove the
        // last legal move. Reuse the ordinary board-owned reshuffle pipeline.
        StartCoroutine(ForceReshuffleRoutine());
    }

    public sealed class ManualSwapPlan
    {
        public Gem[] Gems { get; internal set; }
        public Vector2Int[] Cells { get; internal set; }
        public GemType[] Types { get; internal set; }
        public GemSpecialType[] Specials { get; internal set; }
        public bool IsLegal { get; internal set; }
        public bool IsExtended => Gems.Length == 3;
        public Vector2Int Destination(int index) => Cells[index == 0 ? Cells.Length - 1 : index - 1];
    }

    private Vector2Int[] ManualSwapCells(int ax, int ay, int bx, int by)
    {
        if (Mathf.Abs(ax-bx)+Mathf.Abs(ay-by)!=1 || !IsCellPlayable(ax,ay) || !IsCellPlayable(bx,by) ||
            IsCellPinned(ax,ay) || IsCellPinned(bx,by)) return null;
        var a = new Vector2Int(ax,ay); var b = new Vector2Int(bx,by); var c = b + (b-a);
        if (UsesExtraManualSwapStep && IsCellPlayable(c.x,c.y) && !IsCellPinned(c.x,c.y) && GetGem(c.x,c.y)!=null)
            return new[] { a,b,c };
        return new[] { a,b };
    }
    public ManualSwapPlan PreviewManualSwap(Gem first, Gem second)
    {
        if (first==null || second==null || GetGem(first.Column,first.Row)!=first || GetGem(second.Column,second.Row)!=second) return null;
        var cells = ManualSwapCells(first.Column,first.Row,second.Column,second.Row);
        if (cells==null) return null;
        var pieces = cells.Select(c=>GetGem(c.x,c.y)).ToArray();
        if (pieces.Any(g=>g==null)) return null;
        return new ManualSwapPlan { Cells=cells, Gems=pieces,Types=pieces.Select(g=>g.Type).ToArray(),Specials=pieces.Select(g=>g.SpecialType).ToArray(),
            IsLegal=ManualSwapCreatesMove(BuildCurrentTypeGrid(),BuildCurrentCrystalGrid(),cells) };
    }
    public bool ShowManualSwapPreview(Gem first, Gem second)
    {
        if (IsBusy || IsExternalInputBlocked || Time.timeScale<=0) return false;
        var plan=PreviewManualSwap(first,second);
        if(SameManualPlan(SwapPreview,plan)) return true;
        SwapPreview=plan;
        SwapPreviewChanged?.Invoke(SwapPreview);
        return SwapPreview!=null;
    }
    public void ClearManualSwapPreview()
    {
        previewTapTarget=null;
        if (SwapPreview==null) return;
        SwapPreview=null; SwapPreviewChanged?.Invoke(null);
    }
    private bool PreviewSelectedSwap(Gem first, Gem second)
    {
        if (!UsesExtraManualSwapStep) return false;
        if (previewTapTarget==second && SameManualPlan(SwapPreview,PreviewManualSwap(first,second))) return false;
        previewTapTarget=second;
        ShowManualSwapPreview(first,second);
        return true;
    }
    private bool StageUnpreviewedSwipe(Gem first, Vector2 delta)
    {
        if(!UsesExtraManualSwapStep) return false;
        var second=SwipeNeighbor(first,delta);
        if(SameManualPlan(SwapPreview,PreviewManualSwap(first,second))) return false;
        ClearSelection();
        if(!ShowManualSwapPreview(first,second)) return true;
        selectedGem=first;first.SetSelected(true);previewTapTarget=second;
        return true;
    }
    private static bool SameManualPlan(ManualSwapPlan a,ManualSwapPlan b) => a!=null && b!=null &&
        a.Gems.SequenceEqual(b.Gems) && a.Cells.SequenceEqual(b.Cells) && a.Types.SequenceEqual(b.Types) &&
        a.Specials.SequenceEqual(b.Specials) && a.IsLegal==b.IsLegal;
    private Gem SwipeNeighbor(Gem first, Vector2 delta)
    {
        var direction = Mathf.Abs(delta.x)>Mathf.Abs(delta.y)
            ? new Vector2Int(delta.x>0?1:-1,0) : new Vector2Int(0,delta.y>0?1:-1);
        return GetGem(first.Column+direction.x,first.Row+direction.y);
    }

    private static void RotateSnapshot<T>(T[,] grid, Vector2Int[] cells, bool reverse=false)
    {
        if (grid==null) return;
        var original=cells.Select(c=>grid[c.x,c.y]).ToArray();
        for(int i=0;i<cells.Length;i++)
        {
            int source=reverse?(i+cells.Length-1)%cells.Length:(i+1)%cells.Length;
            grid[cells[i].x,cells[i].y]=original[source];
        }
    }
    private bool ManualSwapCreatesMove(GemType[,] types,bool[,] crystals,Vector2Int[] cells)
    {
        if(cells==null) return false;
        // The chosen gem interacts with the piece at its final destination.
        // A crystal displaced from the middle is simply moved, not activated.
        var first=cells[0];var last=cells[cells.Length-1];
        bool crystalContact=crystals!=null && (crystals[first.x,first.y] || crystals[last.x,last.y]);
        RotateSnapshot(types,cells); RotateSnapshot(crystals,cells);
        try { return crystalContact || cells.Any(c=>HasMatchAtInSnapshot(types,crystals,c.x,c.y)); }
        finally { RotateSnapshot(types,cells,true); RotateSnapshot(crystals,cells,true); }
    }
    private bool HasExtendedManualMove(GemType[,] types,bool[,] crystals)
    {
        foreach(var cells in DirectedManualPaths()) if(ManualSwapCreatesMove(types,crystals,cells)) return true;
        return false;
    }
    private IEnumerable<Vector2Int[]> DirectedManualPaths()
    {
        for(int y=0;y<height;y++) for(int x=0;x<width;x++)
            foreach(var direction in new[]{Vector2Int.right,Vector2Int.up,Vector2Int.left,Vector2Int.down})
            {
                var cells=ManualSwapCells(x,y,x+direction.x,y+direction.y);
                if(cells!=null && cells.All(c=>GetGem(c.x,c.y)!=null)) yield return cells;
            }
    }
    private List<ResponseOption> ExtendedManualResponses()
    {
        var result=new List<ResponseOption>(); var types=BuildCurrentTypeGrid(); var crystals=BuildCurrentCrystalGrid();
        foreach(var cells in DirectedManualPaths())
        {
            if(!ManualSwapCreatesMove(types,crystals,cells)) continue;
            var pieces=cells.Select(c=>GetGem(c.x,c.y)).ToArray();
            var first=pieces[0];var last=pieces[pieces.Length-1];
            var option=new ResponseOption{Source=first,Target=pieces[1],
                UsesSpecial=first.SpecialType==GemSpecialType.ColorCrystal || last.SpecialType==GemSpecialType.ColorCrystal};
            for(int i=0;i<pieces.Length;i++) option.FinalCells[pieces[i]]=cells[(i+pieces.Length-1)%pieces.Length];
            if(option.UsesSpecial) {option.Clears.Add(first);option.Clears.Add(last);result.Add(option);continue;}
            RotateSnapshot(types,cells); RotateSnapshot(crystals,cells);
            try
            {
                foreach(var origin in cells)
                {
                    if(crystals[origin.x,origin.y]) continue;
                    foreach(var axis in new[]{Vector2Int.right,Vector2Int.up})
                    {
                        var line=new List<Vector2Int>{origin};
                        foreach(int sign in new[]{-1,1}) for(int step=1;;step++)
                        {
                            var c=origin+axis*(step*sign);
                            if(!IsCellPlayable(c.x,c.y) || GetGem(c.x,c.y)==null || crystals[c.x,c.y] || types[c.x,c.y]!=types[origin.x,origin.y]) break;
                            line.Add(c);
                        }
                        if(line.Count<3) continue;
                        option.CreatesSpecial|=line.Count>=4;
                        foreach(var c in line)
                        {
                            int moved=Array.IndexOf(cells,c);
                            var piece=moved<0?GetGem(c.x,c.y):pieces[(moved+1)%pieces.Length];
                            option.Clears.Add(piece); option.UsesSpecial|=piece.SpecialType!=GemSpecialType.None;
                            option.BreaksObstacle|=IsGemPinned(piece);
                            foreach(var d in new[]{Vector2Int.right,Vector2Int.up,Vector2Int.left,Vector2Int.down})
                                option.BreaksObstacle|=barricadeCells.ContainsKey(c+d) || (d==Vector2Int.up && royalBannerCells.ContainsKey(c+d));
                        }
                    }
                }
                result.Add(option);
            }
            finally {RotateSnapshot(types,cells,true);RotateSnapshot(crystals,cells,true);}
        }
        return result;
    }
    private IEnumerator AnimateManualSwap(Gem first,Gem second,Gem third,bool reverse=false)
    {
        if(third==null) {yield return AnimateSwap(first,second);yield break;}
        var pieces=new[]{first,second,third};
        var cells=pieces.Select(g=>new Vector2Int(g.Column,g.Row)).ToArray();
        var start=pieces.Select(g=>g.transform.localPosition).ToArray();
        var destination=new Vector3[3];
        for(int i=0;i<3;i++)
        {
            int next=reverse?(i+1)%3:(i+2)%3;
            var cell=cells[next];gems[cell.x,cell.y]=pieces[i];pieces[i].SetGridPosition(cell.x,cell.y);
            destination[i]=GetLocalPosition(cell.x,cell.y);
        }
        float duration=GetResponsiveSwapDuration();float elapsed=0;
        foreach(var piece in pieces)GemMotionPresented?.Invoke(piece,duration,0,false);
        while(elapsed<duration)
        {
            for(int i=0;i<3;i++) pieces[i].transform.localPosition=Vector3.Lerp(start[i],destination[i],EaseOutCubic(Mathf.Clamp01(elapsed/duration)));
            elapsed+=Time.deltaTime;yield return null;
        }
        for(int i=0;i<3;i++) pieces[i].transform.localPosition=destination[i];
    }
}
