using System;
using UnityEngine;
using UnityEngine.Serialization;

// [CreateAssetMenu(fileName = "TD", menuName = "TD/Unit", order = 1)]
public class UnitSO : ScriptableObject
{
    public UnitID unitID;
    public string unitName;
    public float health;
    public float attackDamage;
    public float attackRange;
    public float attackSpeed;
    public float moveSpeed;
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
}

[CreateAssetMenu(fileName = "Hero", menuName = "TD/Units/Hero")]
public class HeroSO : UnitSO
{
    
}



public enum UnitID
{
    None = 0,
    Warrior = 1,
    Archer = 2,
    
    
    
    
    
    //hero start from 200
    Bolverk = 200,
    
    
}