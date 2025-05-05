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



public enum UnitID
{
    None = 0,
    Warrior = 1,
    Archer = 2,
    
    
    
    
    
    //hero start from 200
    Bolverk = 200,
    
    
}