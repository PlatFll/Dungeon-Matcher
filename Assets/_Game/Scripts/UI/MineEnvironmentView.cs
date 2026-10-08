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
        if (board.Mine?.drills == null) { ClearEffects(); return; }
        foreach (var drill in board.Mine.drills)
        {
            if (!machines.TryGetValue(drill.id, out var machine))
            {
                machine = new GameObject("MineDrill_" + drill.id).transform; machine.SetParent(transform, false);
                machines[drill.id] = machine;
                var native=GameplayThemeSkin.Current?.horizontalMineDrill;
                if(native!=null) Native(machine,"NativeHousing",native,Vector3.zero,.82f);
                else
                {
                Piece(machine, "Housing", Vector2.zero, new Vector2(.52f,.42f), new Color(.12f,.14f,.18f));
                Piece(machine, "Iron", new Vector2(-.04f,.02f), new Vector2(.33f,.30f), new Color(.35f,.39f,.44f));
                Piece(machine, "Motor", new Vector2(-.08f,.02f), new Vector2(.12f,.16f), new Color(.96f,.39f,.08f), 111);
                for (int tooth = 0; tooth < 3; tooth++)
                    Piece(machine, "Bit" + tooth, new Vector2(.20f + tooth * .08f,0), new Vector2(.07f,.32f-tooth*.09f), new Color(.76f,.66f,.42f));
                }
                pips[drill.id] = new SpriteRenderer[board.MineDrillCapacity];
                for (int n = 0; n < board.MineDrillCapacity; n++)
                    pips[drill.id][n] = Piece(machine, "Charge" + n, new Vector2(-.18f+n*.12f,.45f), new Vector2(.085f,.08f), Color.gray);
                // Amber intake brackets identify the first cell that manual matches can fuel.
                Piece(machine,"IntakeTop",new Vector2(.62f,.23f),new Vector2(.16f,.035f),new Color(.9f,.53f,.18f,.65f),65);
                Piece(machine,"IntakeBottom",new Vector2(.62f,-.23f),new Vector2(.16f,.035f),new Color(.9f,.53f,.18f,.65f),65);
            }
            machine.localPosition = drill.horizontal
                ? board.GetCellLocalPosition(0,drill.lane) + Vector3.left * board.CellSize * 1.02f
                : board.GetCellLocalPosition(drill.lane,0) + Vector3.down * board.CellSize * 1.02f;
            machine.localRotation = Quaternion.Euler(0,0,drill.horizontal ? 0 : 90);
            machine.localScale = Vector3.one * board.CellSize;
            var body=machine.Find("NativeHousing")?.GetComponent<SpriteRenderer>();
            var frames=GameplayThemeSkin.Current?.mineDrillFrames;
            if(body!=null && frames?.Length>0)
                body.sprite=spinningUntil.TryGetValue(drill.id,out float end) && Time.time<end && !PresentationPreferences.ReducedMotion
                    ? frames[(int)(Time.time*24)%frames.Length] : GameplayThemeSkin.Current.horizontalMineDrill;
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
        if(!isActiveAndEnabled)return;
        spinningUntil[id]=Time.time+.5f;
        CombatAudioController.PlayMechanism(CombatSoundCue.MineLargeDrill);
        StartCoroutine(Sweep(horizontal,lane));
    }
    private IEnumerator Sweep(bool horizontal, int lane)
    {
        yield return DrillTravel(horizontal,lane,horizontal?board.Width:board.Height,false,true);
    }
    private void OnDisable()
    {
        StopAllCoroutines(); foreach(var sweep in sweeps) if(sweep!=null) Destroy(sweep); sweeps.Clear();
        ClearEffects();
        foreach(var machine in machines.Values) if(machine!=null) machine.gameObject.SetActive(false);
        foreach(var warning in warnings) if(warning!=null) warning.enabled=false;
    }
    private void OnEnable() { foreach(var machine in machines.Values) if(machine!=null) machine.gameObject.SetActive(true); }
    private void OnDestroy()
    {
        if (board != null) board.MineDrillFired -= Fire;
        if (board != null) board.SmallMineDrillPresented -= SmallDrill;
        if (board != null) board.MineChargeDetonated -= ChargeBurst;
        ClearEffects();
        foreach (var machine in machines.Values) if (machine != null) Destroy(machine.gameObject);
        if (square != null) Destroy(square);
    }
}
