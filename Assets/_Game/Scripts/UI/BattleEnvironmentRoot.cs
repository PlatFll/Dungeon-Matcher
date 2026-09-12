using UnityEngine;

/// <summary>Marks the authored Tilemap hierarchy for one battle environment prefab.</summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Grid))]
public sealed class BattleEnvironmentRoot : MonoBehaviour
{
    [SerializeField] private string environmentId = "dungeon-default";

    public string EnvironmentId => environmentId;
}
