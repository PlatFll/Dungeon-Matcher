using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Native mine presentation. The board alone owns charge, damage and settlement.</summary>
public sealed partial class MineEnvironmentView : MonoBehaviour
{
    private BoardController board;
    private Sprite square;
    private readonly Dictionary<int, Transform> machines = new Dictionary<int, Transform>();
    private readonly Dictionary<int, SpriteRenderer[]> pips = new Dictionary<int, SpriteRenderer[]>();
    private readonly List<GameObject> sweeps = new List<GameObject>();
    private readonly List<SpriteRenderer> warnings = new List<SpriteRenderer>();
    private void Awake()
    {
        board = GetComponent<BoardController>();
        square = Sprite.Create(Texture2D.whiteTexture, new Rect(0,0,Texture2D.whiteTexture.width,Texture2D.whiteTexture.height), new Vector2(.5f,.5f), Texture2D.whiteTexture.width);
        square.name = "Ironvein_Proof_Presentation";
        board.MineDrillFired += Fire;
        board.MineDrillTravelling += TravelRow;
        board.MineDrillExploded += ExplodeRow;
        board.MineDrillReloading += ReloadRow;
        board.SmallMineDrillPresented += SmallDrill;
        board.MineChargeDetonated += ChargeBurst;
    }
    private SpriteRenderer Piece(Transform parent, string name, Vector2 point, Vector2 size, Color color, int order = 110)
    {
        var go = new GameObject(name); go.transform.SetParent(parent, false);
        go.transform.localPosition = point; go.transform.localScale = new Vector3(size.x, size.y, 1);
        var image = go.AddComponent<SpriteRenderer>(); image.sprite = square; image.color = color;
        image.sortingLayerName = "Effects"; image.sortingOrder = order; return image;
    }
    private void LateUpdate()
    {
        if (board.Mine?.drills == null) {
            ClearEffects(); ClearRowShots();
            foreach (var m in machines.Values) if (m) m.gameObject.SetActive(false);
            return;
        }
        foreach (var drill in board.Mine.drills)
        {
            if (!machines.TryGetValue(drill.id, out var machine))
            {
                machine = new GameObject("MineDrill_" + drill.id).transform; machine.SetParent(transform, false);
                machines[drill.id] = machine;
                BuildRowMachine(machine, drill.id);
                pips[drill.id] = new SpriteRenderer[board.MineDrillCapacity];
                for (int n = 0; n < board.MineDrillCapacity; n++)
                    pips[drill.id][n] = Piece(machine, "Charge" + n, new Vector2(-.18f+n*.12f,.42f), new Vector2(.085f,.06f), Color.gray, 116);
                // Amber intake brackets identify the first cell that manual matches can fuel.
                Piece(machine,"IntakeTop",new Vector2(.62f,.23f),new Vector2(.16f,.035f),new Color(.9f,.53f,.18f,.65f),65);
                Piece(machine,"IntakeBottom",new Vector2(.62f,-.23f),new Vector2(.16f,.035f),new Color(.9f,.53f,.18f,.65f),65);
            }
            machine.gameObject.SetActive(true);
            machine.localPosition = board.GetCellLocalPosition(drill.fromRight ? board.Width-1 : 0,drill.lane)
                + (drill.fromRight ? Vector3.right : Vector3.left) * board.CellSize * ( .5f + 20f/34f );
            machine.localRotation = Quaternion.identity;
            machine.localScale = new Vector3(drill.fromRight ? -1 : 1, 1, 1) * board.CellSize;
            if (loadedBits.TryGetValue(drill.id, out var bit))
            {
                bit.enabled = !board.IsMineDrillInFlight(drill.id);
                float reveal = reloadStarted.TryGetValue(drill.id, out float t) ? Mathf.Clamp01((Time.time-t)/BoardController.MineDrillReloadSeconds) : 1;
                bit.transform.localPosition = new Vector3(Mathf.Round(Mathf.Lerp(-1,5,reveal))/34f,0,0);
            }
            for (int n = 0; n < pips[drill.id].Length; n++)
                pips[drill.id][n].color = n < drill.charge ? new Color(1,.57f,.12f) : new Color(.28f,.27f,.26f);
        }
        int used = 0;
        var roster = RunSession.Current?.Waves?.ActiveEnemies;
        if (roster != null) foreach (var actor in roster)
        {
            var ability = actor != null && !actor.IsDefeated ? actor.GetComponent<MineEnemyAbility>() : null;
            if (ability?.IsPreparing != true || (!ability.HasLaneTarget && !ability.TargetCell.HasValue)) continue;
            if (used == warnings.Count) warnings.Add(Piece(transform,"MineTarget",Vector2.zero,Vector2.one,Color.white,70));
            var view = warnings[used++]; view.enabled = true;
            if (ability.TargetCell is Vector2Int cell)
            { view.transform.localPosition = board.GetCellLocalPosition(cell.x,cell.y); view.transform.localScale = Vector3.one * board.CellSize * .85f; }
            else
            {
                bool row=ability.Horizontal; int lane=ability.Lane;
                view.transform.localPosition = (board.GetCellLocalPosition(row?0:lane,row?lane:0) +
                    board.GetCellLocalPosition(row?board.Width-1:lane,row?lane:board.Height-1)) * .5f;
                view.transform.localScale = new Vector3(row?board.Width:1,row?1:board.Height,1) * board.CellSize;
            }
            float pulse = PresentationPreferences.ReducedMotion ? .13f : .13f+.045f*Mathf.Sin(Time.time*7);
            view.color = new Color(1,.5f,.1f,pulse);
        }
        for (int i=used;i<warnings.Count;i++) warnings[i].enabled=false;
        UpdateStoneEffects();
    }
    private void Fire(int id, bool horizontal, int lane)
    {
        if (!isActiveAndEnabled) return;
        CombatAudioController.PlayMechanism(CombatSoundCue.MineLargeDrill);
        LaunchRow(id, lane);
    }
    private void OnDisable()
    {
        StopAllCoroutines(); foreach(var sweep in sweeps) if(sweep!=null) Destroy(sweep); sweeps.Clear();
        ClearEffects(); ClearRowShots();
        foreach(var machine in machines.Values) if(machine!=null) machine.gameObject.SetActive(false);
        foreach(var warning in warnings) if(warning!=null) warning.enabled=false;
    }
    private void OnEnable() { foreach(var machine in machines.Values) if(machine!=null) machine.gameObject.SetActive(true); }
    private void OnDestroy()
    {
        if (board != null) {
            board.MineDrillFired -= Fire;
            board.MineDrillTravelling -= TravelRow;
            board.MineDrillExploded -= ExplodeRow;
            board.MineDrillReloading -= ReloadRow;
        }
        if (board != null) board.SmallMineDrillPresented -= SmallDrill;
        if (board != null) board.MineChargeDetonated -= ChargeBurst;
        ClearEffects();
        foreach (var machine in machines.Values) if (machine != null) Destroy(machine.gameObject);
        if (square != null) Destroy(square);
    }
}
