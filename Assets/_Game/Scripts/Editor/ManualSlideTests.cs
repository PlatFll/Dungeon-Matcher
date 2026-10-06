using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public sealed class ManualSlideTests
{
    private const BindingFlags Flags=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
    private GameObject root;
    private BoardController board;
    private Gem[,] grid;
    [SetUp] public void Setup()
    {
        root=new GameObject("Manual slide fixture");root.SetActive(false);board=root.AddComponent<BoardController>();
        Set("width",5);Set("height",5);grid=new Gem[5,5];Set("gems",grid);Set("gemSprites",new Sprite[6]);
        for(int y=0;y<5;y++)for(int x=0;x<5;x++)
        {
            var go=new GameObject("Piece");go.SetActive(false);go.transform.SetParent(root.transform,false);
            var gem=go.AddComponent<Gem>();gem.Initialize(board,x,y,(GemType)((x+2*y)%6),null,1);grid[x,y]=gem;
        }
        board.ExtraManualSwapStep=()=>true;
    }
    [TearDown] public void Cleanup() { UnityEngine.Object.DestroyImmediate(root);Time.timeScale=1; }
    private void Set(string field,object value)=>typeof(BoardController).GetField(field,Flags).SetValue(board,value);
    private object Call(string name,params object[] args)=>typeof(BoardController).GetMethods(Flags)
        .First(m=>m.Name==name && m.GetParameters().Length==args.Length).Invoke(board,args);
    private Dictionary<Gem,int> Pins=>(Dictionary<Gem,int>)typeof(BoardController).GetField("pinnedGemOwners",Flags).GetValue(board);
    [TestCase(1,0)] [TestCase(-1,0)] [TestCase(0,1)] [TestCase(0,-1)]
    public void DeterministicDirectionAndBackwardDisplacement(int dx,int dy)
    {
        var a=grid[2,2];var b=grid[2+dx,2+dy];var c=grid[2+2*dx,2+2*dy];
        var plan=board.PreviewManualSwap(a,b);
        CollectionAssert.AreEqual(new[]{a,b,c},plan.Gems);
        Assert.That(plan.Destination(0),Is.EqualTo(new Vector2Int(c.Column,c.Row)));
        Assert.That(plan.Destination(1),Is.EqualTo(new Vector2Int(a.Column,a.Row)));
        Assert.That(plan.Destination(2),Is.EqualTo(new Vector2Int(b.Column,b.Row)));
    }
    [Test] public void EdgesBlockedCellsPinnedAndMissingExtraPiecesFallBack()
    {
        Assert.That(board.PreviewManualSwap(grid[3,0],grid[4,0]).IsExtended,Is.False);
        var c=grid[2,0];Pins[c]=1;
        Assert.That(board.PreviewManualSwap(grid[0,0],grid[1,0]).IsExtended,Is.False);
        Pins.Clear();grid[2,0]=null;
        Assert.That(board.PreviewManualSwap(grid[0,0],grid[1,0]).IsExtended,Is.False);
        grid[2,0]=c;Pins[grid[1,0]]=1;
        Assert.That(board.PreviewManualSwap(grid[0,0],grid[1,0]),Is.Null);
    }
    [Test] public void CrystalUsesFinalContactAndMiddleCrystalOnlyMoves()
    {
        var a=grid[0,0];var b=grid[1,0];var c=grid[2,0];
        b.SetSpecialType(GemSpecialType.ColorCrystal);
        Assert.That(board.PreviewManualSwap(a,b).IsLegal,Is.EqualTo(PhysicalLegality(a,b)));
        c.SetSpecialType(GemSpecialType.ColorCrystal);
        Assert.That(board.PreviewManualSwap(a,b).IsLegal,Is.True);
        Pins[c]=1;
        Assert.That(board.PreviewManualSwap(a,b).IsExtended,Is.False);
        Assert.That(board.PreviewManualSwap(a,b).IsLegal,Is.True,"Fallback contacts the adjacent crystal");
    }
    [Test] public void SnapshotHintsAndCounterplayAgreeWithPhysicalFinalArrangement()
    {
        var random=new System.Random(7281);
        for(int sample=0;sample<20;sample++)
        {
            Pins.Clear();
            foreach(var gem in grid)
            {
                gem.SetType((GemType)random.Next(6),null);
                gem.SetSpecialType(random.Next(10)==0?GemSpecialType.ColorCrystal:GemSpecialType.None);
                if(random.Next(12)==0) Pins[gem]=1;
            }
            var responses=board.GetImmediateResponses();bool any=false;
            foreach(var first in grid) foreach(var direction in new[]{Vector2Int.up,Vector2Int.right,Vector2Int.down,Vector2Int.left})
            {
                var second=board.GetGem(first.Column+direction.x,first.Row+direction.y);
                bool actual=PhysicalLegality(first,second);any|=actual;
                Assert.That(board.PreviewManualSwap(first,second)?.IsLegal==true,Is.EqualTo(actual));
                Assert.That(board.IsHintMoveStillValid(first,second),Is.EqualTo(actual));
                Assert.That(responses.Any(r=>r.Source==first && r.Target==second),Is.EqualTo(actual));
            }
            Assert.That((bool)Call("HasAvailableMove"),Is.EqualTo(any));
            if(board.TryGetRandomHintMove(out var from,out var to)) Assert.That(PhysicalLegality(from,to),Is.True);
        }
        Assert.That(board.CompletedValidPlayerMoves,Is.Zero);Assert.That(board.IsBusy,Is.False);
    }
    [Test] public void DragPreviewsWithoutCommittingAndCancellationClears()
    {
        var first=grid[0,0];board.BeginPointerGesture(first,Vector2.zero,42);
        board.UpdatePointerGesture(first,Vector2.right*80,42);
        Assert.That(board.SwapPreview,Is.Not.Null);Assert.That(board.IsBusy,Is.False);
        Assert.That(board.GetGem(0,0),Is.SameAs(first));Assert.That(board.CompletedValidPlayerMoves,Is.Zero);
        using(board.AcquireExternalInputBlock()) Assert.That(board.SwapPreview,Is.Null);
        board.SelectGem(first);board.SelectGem(grid[1,0]);Assert.That(board.SwapPreview,Is.Not.Null);
        board.SelectGem(first);Assert.That(board.SwapPreview,Is.Null);
    }
    [Test] public void FixedCellCounterplayUsesFinalRotatedPositions()
    {
        grid[0,0].SetType(GemType.Ruby,null);
        grid[2,1].SetType(GemType.Ruby,null);grid[2,2].SetType(GemType.Ruby,null);
        var option=board.GetImmediateResponses().Single(r=>r.Source==grid[0,0] && r.Target==grid[1,0]);
        Assert.That(option.Clears,Does.Contain(grid[0,0]));
        Assert.That(option.FinalCell(grid[0,0]),Is.EqualTo(new Vector2Int(2,0)));
        Assert.That(option.FinalCell(grid[1,0]),Is.EqualTo(new Vector2Int(0,0)));
        Assert.That(option.FinalCell(grid[2,0]),Is.EqualTo(new Vector2Int(1,0)));
        var cells=(HashSet<Vector2Int>)Call("ImmediatelyClearableOrdinaryCells");
        Assert.That(cells,Does.Contain(new Vector2Int(2,0)));
    }
    private bool PhysicalLegality(Gem first,Gem second)
    {
        if(first==null || second==null || board.IsGemPinned(first) || board.IsGemPinned(second)) return false;
        var a=new Vector2Int(first.Column,first.Row);var b=new Vector2Int(second.Column,second.Row);
        var third=board.GetGem(b.x+(b.x-a.x),b.y+(b.y-a.y));
        bool extended=third!=null && !board.IsGemPinned(third);
        var pieces=extended?new[]{first,second,third}:new[]{first,second};
        var cells=pieces.Select(g=>new Vector2Int(g.Column,g.Row)).ToArray();
        if(first.SpecialType==GemSpecialType.ColorCrystal || pieces.Last().SpecialType==GemSpecialType.ColorCrystal) return true;
        for(int i=0;i<pieces.Length;i++)
        {
            var c=cells[(i+pieces.Length-1)%pieces.Length];grid[c.x,c.y]=pieces[i];pieces[i].SetGridPosition(c.x,c.y);
        }
        try {return pieces.Any(g=>((HashSet<Gem>)Call("FindMatchesFrom",g,null)).Count>0);}
        finally {for(int i=0;i<pieces.Length;i++){var c=cells[i];grid[c.x,c.y]=pieces[i];pieces[i].SetGridPosition(c.x,c.y);}}
    }
}
