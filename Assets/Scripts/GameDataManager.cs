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

    public List<TowerSO> GetNextLevelTowers(TowerType t, int currentLv)
    {
        List<TowerSO> nextTowers = new List<TowerSO>();
        if (t == TowerType.None)
        {
            foreach (TowerSO towerSO in towerSOList)
            {
                if (towerSO.level == 1)
                {
                    nextTowers.Add(towerSO);
                }
            }
        }
        else
        {
            foreach (TowerSO towerSO in towerSOList)
            {
                if (towerSO.type == t && towerSO.level == currentLv + 1)
                {
                    nextTowers.Add(towerSO);
                }
            }
        }

        return nextTowers;
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
}