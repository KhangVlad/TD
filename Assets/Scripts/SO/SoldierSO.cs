using UnityEngine;
[CreateAssetMenu(fileName = "Soldier", menuName = "TD/Units/Soldier", order = 1)]
public class SoldierSO : UnitSO
{
    public float damage; // Used in AttackState
    public float meleeAttackBonus;
}