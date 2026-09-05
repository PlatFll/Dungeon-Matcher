using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Procedural pixel art and transient sweep only; removing this component must
// never change warning duration, selected gems, damage or board ownership.
[DisallowMultipleComponent]
public sealed class BoardHammerThreatVFX : MonoBehaviour
{
    private sealed class WarningView
    {
        public BoardController.GemPairThreat Threat;
        public Transform[] Roots;
        public SpriteRenderer[][] Pips;
    }

    private BoardController board;
    private Texture2D hammerTexture;
    private Sprite hammerSprite;
    private Sprite solidSprite;
    private readonly List<WarningView> warnings = new List<WarningView>();
    private readonly Dictionary<Transform, Quaternion> strikePoses =
        new Dictionary<Transform, Quaternion>();
    private readonly List<GameObject> sweeps = new List<GameObject>();

    private void OnEnable()
    {
        board = GetComponent<BoardController>();
        if (board == null) return;
        board.GemPairMarked += ShowWarning;
        board.GemPairImpact += ShowImpact;
    }

    private void EnsureSprites()
    {
        if (hammerSprite != null) return;
        hammerTexture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
        hammerTexture.filterMode = FilterMode.Point;
        hammerTexture.wrapMode = TextureWrapMode.Clamp;
        bool[,] shape = new bool[16,16];
        // Broad blunt head, short handle: readable without covering gem color.
        for (int y = 9; y <= 13; y++)
            for (int x = 3; x <= 12; x++) shape[x,y] = true;
        for (int y = 2; y <= 9; y++)
            for (int x = 7; x <= 8; x++) shape[x,y] = true;
        Color[] pixels = new Color[256];
        for (int y = 0; y < 16; y++)
            for (int x = 0; x < 16; x++)
            {
                bool outline = false;
                for (int dy = -1; dy <= 1; dy++)
                    for (int dx = -1; dx <= 1; dx++)
                        if (x+dx >= 0 && x+dx < 16 && y+dy >= 0 && y+dy < 16)
                            outline |= shape[x+dx,y+dy];
                pixels[y*16+x] = shape[x,y] ? Color.white :
                    outline ? new Color(0.12f,0.10f,0.16f,1f) : Color.clear;
            }
        hammerTexture.SetPixels(pixels);
        hammerTexture.Apply();
        hammerSprite = Sprite.Create(hammerTexture, new Rect(0,0,16,16),
            new Vector2(0.5f,0.5f),16f);
        hammerSprite.name = "WhiteHammerWarning";
        solidSprite = Sprite.Create(Texture2D.whiteTexture,
            new Rect(0,0,Texture2D.whiteTexture.width,Texture2D.whiteTexture.height),
            new Vector2(0.5f,0.5f), Texture2D.whiteTexture.width);
    }

    private SpriteRenderer MakeSprite(string objectName, Transform parent,
        Sprite sprite, int order)
    {
        GameObject go = new GameObject(objectName);
        go.transform.SetParent(parent, false);
        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingLayerName = "Gems";
        renderer.sortingOrder = order;
        renderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        return renderer;
    }

    private void ShowWarning(BoardController.GemPairThreat threat)
    {
        EnsureSprites();
        WarningView view = new WarningView
        {
            Threat = threat,
            Roots = new Transform[2],
            Pips = new SpriteRenderer[2][]
        };
        Gem[] gems = { threat.First, threat.Second };
        for (int i = 0; i < 2; i++)
        {
            SpriteRenderer icon = MakeSprite("HammerWarning", transform, hammerSprite, 40);
            view.Roots[i] = icon.transform;
            icon.transform.position = gems[i].transform.position;
            icon.transform.localScale = Vector3.one * board.CellSize * 0.55f;
            int count = Mathf.Clamp(threat.DueMove - board.CompletedValidPlayerMoves, 1, 6);
            view.Pips[i] = new SpriteRenderer[count];
            for (int p = 0; p < count; p++)
            {
                SpriteRenderer pip = MakeSprite("MoveRemaining", icon.transform, solidSprite, 41);
                pip.transform.localPosition = new Vector3((p-(count-1)*0.5f)*0.22f,-0.53f,0);
                pip.transform.localScale = new Vector3(0.14f,0.10f,1);
                view.Pips[i][p] = pip;
            }
        }
        warnings.Add(view);
    }

    private void LateUpdate()
    {
        for (int i = warnings.Count-1; i >= 0; i--)
        {
            WarningView view = warnings[i];
            if (board == null || !board.IsGemPairThreatValid(view.Threat))
            {
                foreach (Transform root in view.Roots)
                    if (root != null) Destroy(root.gameObject);
                warnings.RemoveAt(i);
                continue;
            }
            Gem[] gems = { view.Threat.First, view.Threat.Second };
            int remaining = Mathf.Max(0,view.Threat.DueMove-board.CompletedValidPlayerMoves);
            for (int g = 0; g < 2; g++)
            {
                // Stay aligned to gem identity without inheriting its hit/burst scale.
                view.Roots[g].position = gems[g].transform.position;
                float pulse = 1f + Mathf.Sin(Time.time * (remaining <= 1 ? 12f : 6f))*0.045f;
                view.Roots[g].localScale = Vector3.one * board.CellSize * 0.55f * pulse;
                for (int p = 0; p < view.Pips[g].Length; p++)
                    view.Pips[g][p].color = p < remaining ? Color.white : new Color(0.22f,0.18f,0.25f,1);
            }
        }
    }

    private void ShowImpact(EnemyActor owner, Vector3 first, Vector3 second, float clearDuration)
    {
        EnsureSprites();
        StartCoroutine(Sweep(first,second,Mathf.Max(0.08f,clearDuration)));
        Transform visual = owner != null ? owner.transform.Find("VisualRoot") : null;
        if (visual != null && !strikePoses.ContainsKey(visual))
            StartCoroutine(StrikePose(visual));
    }

    private IEnumerator StrikePose(Transform visual)
    {
        Quaternion rest = visual.localRotation;
        strikePoses.Add(visual,rest);
        for (float elapsed = 0; elapsed < 0.18f && visual != null; elapsed += Time.deltaTime)
        {
            float t = elapsed/0.18f;
            visual.localRotation = rest * Quaternion.Euler(0,0,
                Mathf.Sin(t*Mathf.PI)*-12f);
            yield return null;
        }
        if (visual != null) visual.localRotation = rest;
        strikePoses.Remove(visual);
    }

    private IEnumerator Sweep(Vector3 first, Vector3 second, float clearDuration)
    {
        Vector3 a = transform.InverseTransformPoint(first);
        Vector3 b = transform.InverseTransformPoint(second);
        Vector3 direction = (b-a).normalized;
        if (direction.sqrMagnitude < 0.01f) direction = Vector3.right;
        a -= direction * board.CellSize * 0.38f;
        b += direction * board.CellSize * 0.38f;
        float length = Vector3.Distance(a,b);
        GameObject root = new GameObject("HammerBluntSweep");
        root.transform.SetParent(transform,false);
        root.transform.localPosition = a;
        root.transform.localRotation = Quaternion.Euler(0,0,
            Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg);
        sweeps.Add(root);
        SpriteRenderer[] strips = new SpriteRenderer[3];
        for (int i = 0; i < strips.Length; i++)
            strips[i] = MakeSprite("BluntSwipe",root.transform,solidSprite,45+i);
        float duration = clearDuration+0.12f;
        for (float elapsed = 0; elapsed < duration; elapsed += Time.deltaTime)
        {
            float travel = Mathf.Clamp01(elapsed/clearDuration);
            float alpha = 1f-Mathf.Clamp01((elapsed-clearDuration)/0.12f);
            for (int i = 0; i < strips.Length; i++)
            {
                // Squared-off impact head with two shorter offset trails.
                float visibleLength = Mathf.Max(board.CellSize*0.08f,
                    length*travel - i*board.CellSize*0.14f);
                strips[i].transform.localPosition = new Vector3(
                    visibleLength*0.5f,(i-1)*board.CellSize*0.10f,0);
                strips[i].transform.localScale = new Vector3(visibleLength,
                    board.CellSize*(i==1 ? 0.18f : 0.08f),1);
                strips[i].color = new Color(1,1,1,alpha);
            }
            yield return null;
        }
        sweeps.Remove(root);
        Destroy(root);
    }

    private void OnDisable()
    {
        if (board != null)
        {
            board.GemPairMarked -= ShowWarning;
            board.GemPairImpact -= ShowImpact;
        }
        StopAllCoroutines();
        foreach (KeyValuePair<Transform,Quaternion> pose in strikePoses)
            if (pose.Key != null) pose.Key.localRotation = pose.Value;
        strikePoses.Clear();
        foreach (WarningView view in warnings)
            foreach (Transform root in view.Roots)
                if (root != null) Destroy(root.gameObject);
        warnings.Clear();
        foreach (GameObject sweep in sweeps) if (sweep != null) Destroy(sweep);
        sweeps.Clear();
    }

    private void OnDestroy()
    {
        if (hammerSprite != null) Destroy(hammerSprite);
        if (solidSprite != null) Destroy(solidSprite);
        if (hammerTexture != null) Destroy(hammerTexture);
    }
}
