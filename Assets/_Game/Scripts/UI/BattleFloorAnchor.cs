using UnityEngine;

/// <summary>Selectable floor marker; its RectTransform is the authored baseline.</summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public sealed class BattleFloorAnchor : MonoBehaviour
{
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (transform.parent is not RectTransform battleArea) return;
        Vector3 floor = battleArea.InverseTransformPoint(transform.position);
        Vector3 left = battleArea.TransformPoint(new Vector3(battleArea.rect.xMin, floor.y, floor.z));
        Vector3 right = battleArea.TransformPoint(new Vector3(battleArea.rect.xMax, floor.y, floor.z));
        Color previous = UnityEditor.Handles.color;
        UnityEditor.Handles.color = new Color(0.2f, 1f, 0.4f, 1f);
        UnityEditor.Handles.DrawAAPolyLine(3f, left, right);
        UnityEditor.Handles.Label(left, "Battle floor — edit BattleFloorAnchor Y");
        UnityEditor.Handles.color = previous;
    }
#endif
}
