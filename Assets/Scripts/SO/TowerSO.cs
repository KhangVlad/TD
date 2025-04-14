using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "TD", menuName = "TD/TowerSO", order = 1)]
public class TowerSO : ScriptableObject
{
    [Header("Basic Info")]
    public string towerName;
    public TowerType type;
    public int level = 1;
    public Sprite icon;
    public GameObject prefab;
    
    [Header("Stats")]
    public int buildCost = 50;
    public int upgradeCost = 100;
    public int sellValue = 25;
    public float range = 5f;
    public float attackRate = 1f;
    public int damage = 10;
    
    [Header("Barracks Specific")]
    public int maxUnits = 3;
    public float spawnTime = 5f;
    public GameObject soldierPrefab;
    public SoldierSO soldierData;

    [Header("Upgrade Options")] 
    public List<TowerType> possibleUpgrades;
    
}