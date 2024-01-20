using System.Runtime.Serialization;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(menuName = "Troop Prefab")]
public class TroopSO : ScriptableObject
{
    public int energyCost;
    public float speed = 1;
    public float damage = 5;
    public Sprite sprite;
    public GameObject prefab;

    private void OnValidate()
    {
        if (prefab == null) return;
        sprite = prefab.GetComponent<SpriteRenderer>().sprite;
        var troop = prefab.GetComponent<Troop>();
        troop.speed = speed;
        troop.damage = damage;
    }
}