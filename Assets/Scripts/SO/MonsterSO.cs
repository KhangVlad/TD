using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Monster", menuName = "TD/MonsterSO", order = 1)]
public class MonsterSO : ScriptableObject
{
    [Header("Basic Info")]
    public string monsterName;
    public Sprite sprite;
    [FormerlySerializedAs("type")] public MonsterID id;
    
    [Header("Stats")]
    public int health = 100;
    public int damage = 10;
    public float speed = 1.5f;
    public int rewardAmount = 10;
    
    [Header("Visual")]
    public Color tintColor = Color.white;
    public float scale = 1.0f;
    
    [Header("Resistances")]
    [Range(0f, 1f)] public float physicalResistance = 0f;
    [Range(0f, 1f)] public float magicalResistance = 0f;
    
    [Header("Behavior")]
    public bool canFly = false;
    public bool isBoss = false;
    [Tooltip("If true, this monster will prioritize attacking buildings over reaching the end")]
    public bool aggressiveBehavior = false;
    
    [TextArea(3, 6)]
    public string description;
}

public enum MonsterID
{
    Goblin = 0,
    Orc = 1,
    Skeleton = 2,
    Zombie = 3,
    Ghost = 4,
    Dragon = 5,
    Boss = 6,
    ForestMonster = 7,
}