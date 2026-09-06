using UnityEngine;
using UnityEngine.UI;

// One icon per blessing source; lifetime observes the attack's pending modifier.
public sealed class EnemyBlessingView : MonoBehaviour
{
    private EnemyAutoAttack attack;
    private object owner;
    public static void Show(EnemyAutoAttack target, object source, Sprite sprite)
    {
        var go = new GameObject("Benediction Halo", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(target.transform, false);
        var rect = (RectTransform)go.transform;
        rect.sizeDelta = new Vector2(36, 12); rect.anchoredPosition = new Vector2(0, 62);
        var icon = go.GetComponent<Image>(); icon.sprite = sprite;
        icon.color = new Color(1f, 0.83f, 0.25f, 0.9f); icon.raycastTarget = false;
        var view = go.AddComponent<EnemyBlessingView>(); view.attack = target; view.owner = source;
    }
    private void Update()
    {
        if (attack == null || attack.EnemyActor == null || attack.EnemyActor.IsDefeated ||
            !attack.HasNextSequenceModifier(owner)) { Destroy(gameObject); return; }
        transform.localScale = Vector3.one * (1f + Mathf.Sin(Time.time * 5f) * 0.08f);
    }
}
