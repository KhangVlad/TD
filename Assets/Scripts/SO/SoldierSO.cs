
    using UnityEngine;

    
[CreateAssetMenu(fileName = "Soldier", menuName = "TD/SoldierSO", order = 1)]
    public class SoldierSO :ScriptableObject
    {
            public GameObject prefab;
            public Sprite sprite;
            public int health;
            public int damage;
            public float speed;
            public float attackRange;
            public float detectionRange;
    }
