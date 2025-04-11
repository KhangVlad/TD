using UnityEngine;
using UnityEngine.Serialization;


[CreateAssetMenu(fileName = "TD", menuName = "TD/TowerSO", order = 1)]
public class TowerSO : ScriptableObject
{
    public TowerType type;
    public Sprite sprite;
    public int level;
    public int cost;
    public int damage;
    public float attackSpeed;
    public float range;
}