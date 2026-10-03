using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BoardController
{
    [Header("Royal Telegraph Art (optional)")]
    public Sprite archbishopRestorationRuneOverlay;
    public Sprite kingRoyalJudgmentExclamationOverlay;
    public Sprite royalBombardmentRowWarning;
    public Sprite royalBombardmentColumnWarning;
    public Sprite royalBombardmentRowSlash;
    public Sprite royalBombardmentColumnSlash;
    [Range(0.02f, 0.3f)] public float royalBombardmentWarningAlpha = 0.12f;
}

// Views observe threats; no damage, countdown or grid mutation lives here.
[DisallowMultipleComponent]
public sealed class BoardTelegraphVFX : MonoBehaviour
{
    private sealed class MarkView
    {
        public BoardController.GemSetThreat Threat;
        public Gem Gem;
        public SpriteRenderer Icon;
    }
    private sealed class LaneView
    {
        public BoardController.LaneThreat Threat;
        public SpriteRenderer Row, Column;
    }
    private BoardController board;
    private Sprite solid, rune, warning, rowCut, columnCut;
    private Texture2D runeTexture, warningTexture, rowCutTexture, columnCutTexture;
    private readonly List<MarkView> marks = new List<MarkView>();
    private readonly List<LaneView> lanes = new List<LaneView>();
    private readonly List<GameObject> slashes = new List<GameObject>();
    private void OnEnable()
    {
        board = GetComponent<BoardController>();
        board.GemSetMarked += ShowMarks; board.LanesMarked += ShowLanes; board.LaneSlash += ShowSlash;
    }
    private Sprite CreateIcon(bool circle, out Texture2D texture)
    {
        texture = new Texture2D(32,32,TextureFormat.RGBA32,false) { filterMode = FilterMode.Point };
        var colors = new Color[1024];
        for (int y=0;y<32;y++) for (int x=0;x<32;x++)
        {
            float radius = Vector2.Distance(new Vector2(x,y),new Vector2(15.5f,15.5f));
            bool pixel = circle ? (radius > 11 && radius < 13) ||
                (radius > 8 && radius < 15 && (x==15 || x==16 || y==15 || y==16)) :
                (x>=14 && x<=17 && ((y>=12 && y<=27) || (y>=5 && y<=8)));
            colors[y*32+x] = pixel ? Color.white : Color.clear;
        }
        texture.SetPixels(colors); texture.Apply();
        return Sprite.Create(texture,new Rect(0,0,32,32),new Vector2(0.5f,0.5f),32);
    }
    private void EnsureSprites()
    {
        if (solid != null) return;
        solid = Sprite.Create(Texture2D.whiteTexture,new Rect(0,0,Texture2D.whiteTexture.width,Texture2D.whiteTexture.height),
            new Vector2(0.5f,0.5f),Texture2D.whiteTexture.width);
        rune = CreateIcon(true, out runeTexture); warning = CreateIcon(false,out warningTexture);
        rowCut = CreateCut(false, out rowCutTexture); columnCut = CreateCut(true, out columnCutTexture);
    }
    private static Sprite CreateCut(bool vertical, out Texture2D texture)
    {
        int width=vertical?16:64, height=vertical?64:16;
        texture=new Texture2D(width,height,TextureFormat.RGBA32,false) { filterMode=FilterMode.Point };
        var pixels=new Color[width*height];
        for(int y=0;y<height;y++)for(int x=0;x<width;x++)
        {
            int along=vertical?y:x, across=vertical?x:y;
            int radius=Mathf.Max(1,7-Mathf.FloorToInt(Mathf.Abs(along-31.5f)*.2f));
            bool streak=Mathf.Abs(across-7.5f)<radius;
            bool spark=(along==12||along==47)&&(across==0||across==15);
            pixels[y*width+x]=streak||spark?Color.white:Color.clear;
        }
        texture.SetPixels(pixels);texture.Apply();
        return Sprite.Create(texture,new Rect(0,0,width,height),new Vector2(.5f,.5f),64);
    }
    private SpriteRenderer Make(string label, Sprite sprite, int order)
    {
        var go = new GameObject(label); go.transform.SetParent(transform,false);
        var renderer = go.AddComponent<SpriteRenderer>(); renderer.sprite = sprite;
        renderer.sortingLayerName = "Gems"; renderer.sortingOrder = order;
        renderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask; return renderer;
    }
    private void ShowMarks(BoardController.GemSetThreat threat)
    {
        EnsureSprites();
        Sprite sprite = threat.Vine ? GameplayThemeSkin.Current?.vineWarning ?? rune : threat.RestorationPresentation ? board.archbishopRestorationRuneOverlay : board.kingRoyalJudgmentExclamationOverlay;
        if (sprite == null) sprite = threat.RestorationPresentation ? rune : warning;
        foreach (Gem gem in threat.Targets)
        {
            var icon = Make(threat.RestorationPresentation ? "Restoration Rune" : "Royal Judgment !",sprite,42);
            icon.color = threat.Vine ? Color.white : threat.RestorationPresentation ? new Color(1f,0.85f,0.28f) : Color.white;
            marks.Add(new MarkView { Threat=threat, Gem=gem, Icon=icon });
        }
    }
    private void SetLane(SpriteRenderer renderer, bool row, int index, float thickness = 1f)
    {
        Vector3 start = board.GetCellLocalPosition(row ? 0 : index, row ? index : 0);
        Vector3 end = board.GetCellLocalPosition(row ? board.Width-1 : index, row ? index : board.Height-1);
        renderer.transform.localPosition = (start+end)*0.5f;
        renderer.transform.localScale = new Vector3(
            (row ? board.Width : thickness)*board.CellSize/renderer.sprite.bounds.size.x,
            (row ? thickness : board.Height)*board.CellSize/renderer.sprite.bounds.size.y,1);
    }
    private void ShowLanes(BoardController.LaneThreat threat)
    {
        EnsureSprites();
        var row = Make("Bombardment Row Warning",board.royalBombardmentRowWarning != null ? board.royalBombardmentRowWarning : solid,35);
        var column = Make("Bombardment Column Warning",board.royalBombardmentColumnWarning != null ? board.royalBombardmentColumnWarning : solid,35);
        SetLane(row,true,threat.Row); SetLane(column,false,threat.Column);
        lanes.Add(new LaneView { Threat=threat, Row=row, Column=column });
    }
    private void LateUpdate()
    {
        for (int i=marks.Count-1;i>=0;i--)
        {
            var view=marks[i];
            if (view.Threat.Ended || (!view.Threat.Environmental && (view.Threat.Owner == null || view.Threat.Owner.IsDefeated)) || !board.IsEnvironmentalOrdinaryGem(view.Gem))
            { if(view.Icon != null) Destroy(view.Icon.gameObject); marks.RemoveAt(i); continue; }
            view.Icon.transform.position=view.Gem.transform.position;
            float scale=board.CellSize*(0.8f+Mathf.Sin(Time.time*7f)*0.06f);
            if(view.Threat.Vine)
            {
                scale=board.CellSize;
                view.Icon.color=new Color(1,1,1,Mathf.Sin(Time.time*7f)>0?.95f:.5f);
            }
            view.Icon.transform.localScale=new Vector3(scale/view.Icon.sprite.bounds.size.x,scale/view.Icon.sprite.bounds.size.y,1);
        }
        for (int i=lanes.Count-1;i>=0;i--)
        {
            var view=lanes[i];
            if(view.Threat.Ended || view.Threat.Owner==null || view.Threat.Owner.IsDefeated)
            { Destroy(view.Row.gameObject); Destroy(view.Column.gameObject); lanes.RemoveAt(i); continue; }
            Color tint=new Color(1f,.73f,.20f,board.royalBombardmentWarningAlpha*(1.2f+.65f*Mathf.Sin(Time.time*9f)));
            view.Row.color=view.Column.color=tint;
            view.Row.enabled = !view.Threat.RowStruck;
            view.Column.enabled = !view.Threat.ColumnStruck;
        }
    }
    private void ShowSlash(bool row,int index,float duration) => StartCoroutine(Slash(row,index,Mathf.Max(0.1f,duration)));
    private IEnumerator Slash(bool row,int index,float duration)
    {
        EnsureSprites();
        Sprite art=row ? board.royalBombardmentRowSlash : board.royalBombardmentColumnSlash;
        Sprite cut=row?rowCut:columnCut;
        var edge=Make("Royal Bombardment Gold Slash",art != null ? art : cut,48);
        var core=Make("Royal Bombardment Flash",cut,49);
        slashes.Add(edge.gameObject); slashes.Add(core.gameObject);
        for(float t=0;t<duration;t+=Time.deltaTime)
        {
            float progress=t/duration;
            SetLane(edge,row,index,Mathf.Lerp(.60f,.08f,progress));
            SetLane(core,row,index,Mathf.Lerp(.18f,.02f,progress));
            edge.color=new Color(1f,.68f,.12f,1f-progress);
            core.color=new Color(1f,.98f,.80f,1f-progress);
            yield return null;
        }
        slashes.Remove(edge.gameObject); slashes.Remove(core.gameObject);
        Destroy(edge.gameObject); Destroy(core.gameObject);
    }
    private void OnDisable()
    {
        if(board != null) { board.GemSetMarked-=ShowMarks; board.LanesMarked-=ShowLanes; board.LaneSlash-=ShowSlash; }
        StopAllCoroutines();
        foreach(var view in marks) if(view.Icon!=null) Destroy(view.Icon.gameObject);
        foreach(var view in lanes) { if(view.Row!=null) Destroy(view.Row.gameObject); if(view.Column!=null) Destroy(view.Column.gameObject); }
        foreach(var go in slashes) if(go!=null) Destroy(go);
        marks.Clear(); lanes.Clear(); slashes.Clear();
    }
    private void OnDestroy()
    {
        if(solid!=null) Destroy(solid); if(rune!=null) Destroy(rune); if(warning!=null) Destroy(warning);
        if(runeTexture!=null) Destroy(runeTexture); if(warningTexture!=null) Destroy(warningTexture);
        if(rowCut!=null) Destroy(rowCut); if(columnCut!=null) Destroy(columnCut);
        if(rowCutTexture!=null) Destroy(rowCutTexture); if(columnCutTexture!=null) Destroy(columnCutTexture);
    }
}
