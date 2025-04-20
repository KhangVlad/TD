
using UnityEngine;
// [CreateAssetMenu(fileName = "TD", menuName = "TD/Unit", order = 1)]
public class UnitSO : ScriptableObject
{
    public string unitName;
    public float health;
    public float attackDamage;
    public float attackRange;
    public float attackSpeed;
    public float speed;
    // Add other common properties as needed
}



[CreateAssetMenu(fileName = "Soldier", menuName = "TD/Units/Soldier", order = 1)]
public class SoldierSO : UnitSO
{
    public float damage; // Used in AttackState
    public float meleeAttackBonus;
}


[CreateAssetMenu(fileName = "NewArcherData", menuName = "TD/Units/ArcherData")]
public class ArcherSO : UnitSO
{
    public float arrowSpeed = 10f;
    public float arrowLifetime = 3f;
    public float criticalHitChance = 0.1f;
    public float criticalHitMultiplier = 2f;
    // Add any other archer-specific stats or properties
}