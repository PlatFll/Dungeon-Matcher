using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Proof presentation only. Native production art replaces these labelled fallback shapes.</summary>
public sealed class MineEnvironmentView : MonoBehaviour
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
        if (board.Mine?.drills == null) return;
        foreach (var drill in board.Mine.drills)
        {
            if (!machines.TryGetValue(drill.id, out var machine))
            {
                machine = new GameObject("MineDrill_" + drill.id).transform; machine.SetParent(transform, false);
                machines[drill.id] = machine;
                Piece(machine, "Housing", Vector2.zero, new Vector2(.52f,.42f), new Color(.12f,.14f,.18f));
                Piece(machine, "Iron", new Vector2(-.04f,.02f), new Vector2(.33f,.30f), new Color(.35f,.39f,.44f));
                Piece(machine, "Motor", new Vector2(-.08f,.02f), new Vector2(.12f,.16f), new Color(.96f,.39f,.08f), 111);
                for (int tooth = 0; tooth < 3; tooth++)
                    Piece(machine, "Bit" + tooth, new Vector2(.20f + tooth * .08f,0), new Vector2(.07f,.32f-tooth*.09f), new Color(.76f,.66f,.42f));
                pips[drill.id] = new SpriteRenderer[board.MineDrillCapacity];
                for (int n = 0; n < board.MineDrillCapacity; n++)
                    pips[drill.id][n] = Piece(machine, "Charge" + n, new Vector2(-.18f+n*.12f,.31f), new Vector2(.085f,.08f), Color.gray);
            }
            machine.localPosition = drill.horizontal
                ? board.GetCellLocalPosition(0,drill.lane) + Vector3.left * board.CellSize * 1.02f
                : board.GetCellLocalPosition(drill.lane,0) + Vector3.down * board.CellSize * 1.02f;
            machine.localRotation = Quaternion.Euler(0,0,drill.horizontal ? 0 : 90);
            machine.localScale = Vector3.one * board.CellSize;
            for (int n = 0; n < pips[drill.id].Length; n++)
                pips[drill.id][n].color = n < drill.charge ? new Color(1,.57f,.12f) : new Color(.28f,.27f,.26f);
        }
        int used = 0;
        var roster = RunSession.Current?.Waves?.ActiveEnemies;
        if (roster != null) foreach (var actor in roster)
        {
            var ability = actor != null && !actor.IsDefeated ? actor.GetComponent<MineEnemyAbility>() : null;
            if (ability?.IsPreparing != true) continue;
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
    }
    private void Fire(int id, bool horizontal, int lane) => StartCoroutine(Sweep(horizontal,lane));
    private IEnumerator Sweep(bool horizontal, int lane)
    {
        var center = horizontal ? (board.GetCellLocalPosition(0,lane)+board.GetCellLocalPosition(board.Width-1,lane))*.5f
            : (board.GetCellLocalPosition(lane,0)+board.GetCellLocalPosition(lane,board.Height-1))*.5f;
        var size = horizontal ? new Vector2(board.Width*board.CellSize,.18f*board.CellSize)
            : new Vector2(.18f*board.CellSize,board.Height*board.CellSize);
        var band = Piece(transform,"MineDrillSweep",center,size,new Color(1,.76f,.32f),80);
        sweeps.Add(band.gameObject);
        float duration = PresentationPreferences.ReducedMotion ? .10f : .22f;
        for (float age = 0; age < duration; age += Time.deltaTime)
        { if(band == null) yield break; band.color = new Color(1,.76f,.32f,1-age/duration); yield return null; }
        if (band != null) { sweeps.Remove(band.gameObject); Destroy(band.gameObject); }
    }
    private void OnDisable()
    {
        StopAllCoroutines(); foreach(var sweep in sweeps) if(sweep!=null) Destroy(sweep); sweeps.Clear();
        foreach(var machine in machines.Values) if(machine!=null) machine.gameObject.SetActive(false);
        foreach(var warning in warnings) if(warning!=null) warning.enabled=false;
    }
    private void OnEnable() { foreach(var machine in machines.Values) if(machine!=null) machine.gameObject.SetActive(true); }
    private void OnDestroy()
    {
        if (board != null) board.MineDrillFired -= Fire;
        foreach (var machine in machines.Values) if (machine != null) Destroy(machine.gameObject);
        if (square != null) Destroy(square);
    }
}
