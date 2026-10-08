using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed partial class MineEnvironmentView
{
    private readonly Dictionary<int,float> spinningUntil=new Dictionary<int,float>();
    private readonly Dictionary<long,MineStoneTarget> observedStones=new Dictionary<long,MineStoneTarget>();
    private readonly Dictionary<long,SpriteRenderer> fuses=new Dictionary<long,SpriteRenderer>();
    private readonly Dictionary<long,SpriteRenderer> hardeningHints=new Dictionary<long,SpriteRenderer>();
    private readonly Dictionary<int,int> observedLanes=new Dictionary<int,int>();
    private bool observed;
    private GameplayThemeDefinition Theme=>GameplayThemeSkin.Current;

    private SpriteRenderer Native(Transform parent,string name,Sprite sprite,Vector3 at,float width,int order=112)
    {
        var go=new GameObject(name);go.transform.SetParent(parent,false);go.transform.localPosition=at;
        var view=go.AddComponent<SpriteRenderer>();view.sprite=sprite;view.sortingLayerName="Effects";view.sortingOrder=order;
        go.transform.localScale=Vector3.one*(sprite==null?width:width*sprite.pixelsPerUnit/sprite.rect.width);
        return view;
    }
    private void SmallDrill(bool horizontal,int lane,int endpoint,bool stopped)
    {
        if(!isActiveAndEnabled)return;
        CombatAudioController.PlayMechanism(CombatSoundCue.MineSmallDrill);
        StartCoroutine(DrillTravel(horizontal,lane,endpoint,stopped,false));
    }
    private void ChargeBurst(Vector2Int cell)
    {
        if(!isActiveAndEnabled)return;
        CombatAudioController.PlayMechanism(CombatSoundCue.MineBlast);
        StartCoroutine(Burst(board.GetCellLocalPosition(cell.x,cell.y),Theme?.mineOreFrames,.28f,.85f));
    }
    private IEnumerator DrillTravel(bool horizontal,int lane,int endpoint,bool stopped,bool large)
    {
        if(sweeps.Count>=16)yield break;
        var frames=Theme?.mineSmallDrillFrames;
        var first=board.GetCellLocalPosition(horizontal?0:lane,horizontal?lane:0);
        var axis=horizontal?Vector3.right:Vector3.up;
        Vector3 start=first-axis*board.CellSize;
        Vector3 end=first+axis*board.CellSize*(stopped?endpoint:endpoint-.4f);
        SpriteRenderer head=frames?.Length>0?Native(transform,large?"PenetratingDrillHead":"BoreDrillProjectile",frames[0],start,
            board.CellSize*(large?.86f:.43f)):null;
        if(head!=null){head.transform.localRotation=Quaternion.Euler(0,0,horizontal?0:90);sweeps.Add(head.gameObject);}
        var shaft=large?Piece(transform,"ExtendedMineShaft",start,Vector2.one,new Color(.64f,.67f,.69f),108):null;
        if(shaft!=null)sweeps.Add(shaft.gameObject);
        float duration=PresentationPreferences.ReducedMotion?.12f:large?.28f:.22f;
        float pixel=board.CellSize/64f;
        for(float age=0;age<duration;age+=Time.deltaTime)
        {
            Vector3 point=Vector3.Lerp(start,end,age/duration);
            point.x=Mathf.Round(point.x/pixel)*pixel;point.y=Mathf.Round(point.y/pixel)*pixel;
            if(head!=null){head.transform.localPosition=point;head.sprite=frames[PresentationPreferences.ReducedMotion?0:(int)(age*30)%frames.Length];}
            if(shaft!=null)
            {
                shaft.transform.localPosition=(start+point)*.5f;float length=Vector3.Distance(start,point);
                shaft.transform.localScale=horizontal?new Vector3(length,board.CellSize*.055f,1):new Vector3(board.CellSize*.055f,length,1);
            }
            yield return null;
        }
        if(head!=null){head.transform.localPosition=end;RemoveTransient(head.gameObject);}
        if(shaft!=null)
        {
            // Fast retraction is visual only; clear/refill already belongs to the board.
            for(float age=0;age<.09f;age+=Time.deltaTime)
            {
                var point=Vector3.Lerp(end,start,age/.09f);shaft.transform.localPosition=(start+point)*.5f;
                float length=Vector3.Distance(start,point);
                shaft.transform.localScale=horizontal?new Vector3(length,board.CellSize*.055f,1):new Vector3(board.CellSize*.055f,length,1);
                yield return null;
            }
            RemoveTransient(shaft.gameObject);
        }
        if(stopped)StartCoroutine(Burst(end,Theme?.mineOreFrames,.23f,.5f));
    }
    private void UpdateStoneEffects()
    {
        var current=board.MineStoneTargets().ToDictionary(t=>t.State.id);
        bool feedback=observed && CombatAudioController.FeedbackAllowed;
        foreach(var pair in current)
        {
            var target=pair.Value;
            bool maturesNext=target.State.stage!=MineStoneStage.Obsidian &&
                target.State.ignoredMoves>=Mathf.Max(1,RunSession.Current.Zone.Definition.mineMovesPerStage)-1;
            if(maturesNext && Theme?.mineSpark!=null)
            {
                if(!hardeningHints.TryGetValue(pair.Key,out var hint))
                {hint=Native(transform,"MineHardening_"+pair.Key,Theme.mineSpark,Vector3.zero,board.CellSize*.24f,124);hardeningHints[pair.Key]=hint;}
                hint.transform.localPosition=board.GetCellLocalPosition(target.Cell.x,target.Cell.y)+new Vector3(.24f,.25f)*board.CellSize;
                hint.color=new Color(1,1,1,PresentationPreferences.ReducedMotion?.6f:.6f+.15f*Mathf.Sin(Time.time*5));
            }
            if(feedback && observedStones.TryGetValue(pair.Key,out var before) && before.State.stage!=target.State.stage)
                CombatAudioController.PlayMechanism(CombatSoundCue.MineStoneHarden);
            if(target.State.bombOwnerId>0)
            {
                if(!fuses.TryGetValue(pair.Key,out var fuse))
                {
                    if(Theme?.mineCharge==null)continue;
                    fuse=Native(transform,"MineCharge_"+pair.Key,Theme.mineCharge,Vector3.zero,board.CellSize*.43f,125);fuses[pair.Key]=fuse;
                    if(feedback)CombatAudioController.PlayMechanism(CombatSoundCue.MineFuse);
                }
                fuse.transform.localPosition=board.GetCellLocalPosition(target.Cell.x,target.Cell.y)+new Vector3(.25f,.22f,0)*board.CellSize;
                var frames=Theme?.mineFuseFrames;
                if(frames?.Length>0)fuse.sprite=frames[PresentationPreferences.ReducedMotion?0:(int)(Time.time*10)%frames.Length];
            }
        }
        foreach(var pair in observedStones)
            if(feedback && !current.ContainsKey(pair.Key))
            {
                var old=pair.Value;
                CombatAudioController.PlayMechanism(CombatSoundCue.MineStoneBreak);
                if(old.State.isCore)StartCoroutine(Burst(board.GetCellLocalPosition(old.Cell.x,old.Cell.y),Theme?.mineCoreBreakFrames,.36f,.88f));
            }
        foreach(long id in fuses.Keys.ToArray())
            if(!current.TryGetValue(id,out var stone) || stone.State.bombOwnerId==0)
            {if(fuses[id]!=null)Destroy(fuses[id].gameObject);fuses.Remove(id);}
        foreach(long id in hardeningHints.Keys.ToArray())
            if(!current.TryGetValue(id,out var stone) || stone.State.stage==MineStoneStage.Obsidian ||
                stone.State.ignoredMoves<Mathf.Max(1,RunSession.Current.Zone.Definition.mineMovesPerStage)-1)
            {if(hardeningHints[id]!=null)Destroy(hardeningHints[id].gameObject);hardeningHints.Remove(id);}
        foreach(var drill in board.Mine.drills)
        {
            if(feedback && observedLanes.TryGetValue(drill.id,out int lane) && lane!=drill.lane)
                CombatAudioController.PlayMechanism(CombatSoundCue.MineRail);
            observedLanes[drill.id]=drill.lane;
        }
        observedStones.Clear();foreach(var pair in current)observedStones.Add(pair.Key,pair.Value);observed=true;
    }
    private IEnumerator Burst(Vector3 at,Sprite[] frames,float duration,float width)
    {
        if(frames==null || frames.Length==0 || sweeps.Count>=16)yield break;
        var view=Native(transform,"MineImpact",frames[0],at,board.CellSize*width,120);sweeps.Add(view.gameObject);
        if(PresentationPreferences.ReducedMotion)duration=.1f;
        for(float age=0;age<duration;age+=Time.deltaTime)
        {if(view==null)yield break;view.sprite=frames[Mathf.Min(frames.Length-1,(int)(age/duration*frames.Length))];yield return null;}
        if(view!=null)RemoveTransient(view.gameObject);
    }
    private void RemoveTransient(GameObject go){sweeps.Remove(go);Destroy(go);}
    private void ClearEffects()
    {
        foreach(var fuse in fuses.Values)if(fuse!=null)Destroy(fuse.gameObject);
        foreach(var hint in hardeningHints.Values)if(hint!=null)Destroy(hint.gameObject);
        hardeningHints.Clear();
        fuses.Clear();observedStones.Clear();observedLanes.Clear();spinningUntil.Clear();observed=false;
    }
}
