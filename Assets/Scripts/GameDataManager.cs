using System;
using UnityEngine;
using System.Collections.Generic;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }
    public List<TowerSO> towerSOList;
    public List<MonsterSO> monsterSOList;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    private void Start()
    {
        LoadDataSO();
    }


    private void LoadDataSO()
    {
        towerSOList = new List<TowerSO>(Resources.LoadAll<TowerSO>("DataSO/Towers"));
        monsterSOList = new List<MonsterSO>(Resources.LoadAll<MonsterSO>("DataSO/Monsters"));
    }

    public MonsterSO GetMonsterSO()
    {
        return monsterSOList[0];
    }

    public List<TowerSO> GetPossibleUpgrade(TowerSO tow)
    {
        List<TowerSO> possibleUpgrades = new List<TowerSO>();
        for (int i = 0; i < tow.possibleUpgrades.Count; i++)
        {
            foreach (TowerSO towerSO in towerSOList)
            {
                if (towerSO.type == tow.possibleUpgrades[i])
                {
                    possibleUpgrades.Add(towerSO);
                }
            }
        }

        return possibleUpgrades;
    }


    public List<TowerSO> GetAllBaseTowers()
    {
        List<TowerSO> baseTowers = new List<TowerSO>();
        foreach (TowerSO towerSO in towerSOList)
        {
            if (towerSO.level == 1)
            {
                baseTowers.Add(towerSO);
            }
        }

        return baseTowers;
    }

    public bool CanAfford(float cost)
    {
        return true;
    }
}