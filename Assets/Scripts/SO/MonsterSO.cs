using UnityEngine;


[CreateAssetMenu(fileName = "Monster", menuName = "TD/MonsterSO", order = 1)]
public class MonsterSO : ScriptableObject
{
    public Sprite sprite;
    public MonsterType type;
    public int health;
    public int damage;
    public float speed;
}


public enum MonsterType
{
    Goblin =0,
}