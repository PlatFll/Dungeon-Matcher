using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class MineEnvironmentView
{
    readonly Dictionary<int, SpriteRenderer> loadedBits = new Dictionary<int, SpriteRenderer>();
    readonly Dictionary<int, SpriteRenderer> rowShots = new Dictionary<int, SpriteRenderer>();
    readonly Dictionary<int, float> reloadStarted = new Dictionary<int, float>();
    readonly Dictionary<int, Coroutine> rowMotion = new Dictionary<int, Coroutine>();

    Sprite[] RowFrames => Theme?.mineRowSpinFrames?.Length > 0 ? Theme.mineRowSpinFrames : Theme?.mineSmallDrillFrames;
    void BuildRowMachine(Transform machine, int id)
    {
        // All pieces share one native 34-pixel-per-cell construction grid.
        // A negative X scale is a lossless reflection for the right-side hardware.
        void Part(string name, Sprite art, float x, float y, int order)
        { if (art) Native(machine, name, art, new Vector3(x/34f,y/34f),art.rect.width/34f,order); }
        if (Theme?.mineRowMount) {
            Part("Mount",Theme.mineRowMount,0,0,110);
            Part("Motor",Theme.mineRowMotor,-3,0,111);
            Part("Power",Theme.mineRowPower,-3,0,115);
        } else {
            Piece(machine,"Housing",Vector2.zero,new Vector2(.7f,.65f),new Color(.19f,.20f,.24f));
            Piece(machine,"Motor",new Vector2(-.12f,0),new Vector2(.35f,.4f),new Color(.46f,.48f,.53f),111);
        }
        if (Theme?.mineRowSocket) Part("EmptySocket",Theme.mineRowSocket,4,0,112);
        else Piece(machine,"EmptySocket",new Vector2(4/34f,0),new Vector2(.24f,.35f),new Color(.035f,.04f,.06f),112);
        var frames=RowFrames;
        var bit=frames?.Length>0 ? Native(machine,"LoadedBit",frames[0],new Vector3(5/34f,0),40/34f,114)
            : Piece(machine,"LoadedBit",new Vector2(5/34f,0),new Vector2(.3f,.16f),Color.gray,114);
        loadedBits[id]=bit;
    }
    Vector3 RowPoint(Vector2Int cell) => board.GetCellLocalPosition(cell.x,cell.y);
    void LaunchRow(int id,int lane)
    {
        if(rowShots.TryGetValue(id,out var old) && old) RemoveTransient(old.gameObject);
        var drill=board.Mine.drills.Find(d=>d.id==id);if(drill==null)return;
        var frames=RowFrames;
        Vector3 at=RowPoint(new Vector2Int(drill.fromRight?board.Width-1:0,lane))
            +(drill.fromRight?Vector3.right:Vector3.left)*board.CellSize*(.5f+15f/34f);
        var head=frames?.Length>0?Native(transform,"RowDrillProjectile_"+id,frames[0],at,board.CellSize*40/34f,140)
            : Piece(transform,"RowDrillProjectile_"+id,at,new Vector2(.4f,.2f)*board.CellSize,Color.gray,140);
        if(drill.fromRight)head.transform.localScale=Vector3.Scale(head.transform.localScale,new Vector3(-1,1,1));
        rowShots[id]=head;sweeps.Add(head.gameObject);
        if(loadedBits.TryGetValue(id,out var bit))bit.enabled=false;
    }
    void TravelRow(int id,Vector2Int cell,float seconds)
    {
        if(!isActiveAndEnabled)return;
        if(rowMotion.TryGetValue(id,out var prior) && prior!=null)StopCoroutine(prior);
        rowMotion[id]=StartCoroutine(TravelRowTo(id,RowPoint(cell),seconds));
    }
    IEnumerator TravelRowTo(int id,Vector3 end,float seconds)
    {
        if(!rowShots.TryGetValue(id,out var head)||!head)yield break;
        Vector3 start=head.transform.localPosition;float pixel=board.CellSize/64f;
        for(float age=0;age<seconds;age+=Time.deltaTime){
            if(!head)yield break;
            var p=Vector3.Lerp(start,end,Mathf.Clamp01(age/seconds));
            p.x=Mathf.Round(p.x/pixel)*pixel;p.y=Mathf.Round(p.y/pixel)*pixel;
            head.transform.localPosition=p;yield return null;
        }
        if(head)head.transform.localPosition=end;
    }
    void Update()
    {
        var frames=RowFrames;if(frames==null||frames.Length==0)return;
        int frame=PresentationPreferences.ReducedMotion?0:(int)(Time.time*24)%frames.Length;
        foreach(var shot in rowShots.Values)if(shot)shot.sprite=frames[frame];
    }
    void ExplodeRow(int id,Vector2Int cell)
    {
        if(rowShots.TryGetValue(id,out var head)&&head)RemoveTransient(head.gameObject);
        rowShots.Remove(id);
        if(isActiveAndEnabled)StartCoroutine(Burst(RowPoint(cell),Theme?.mineOreFrames,BoardController.MineDrillBurstSeconds,.55f));
    }
    void ReloadRow(int id){reloadStarted[id]=Time.time;}
    void ClearRowShots(){
        foreach(var shot in rowShots.Values)if(shot)RemoveTransient(shot.gameObject);
        rowShots.Clear();rowMotion.Clear();reloadStarted.Clear();
    }
}
