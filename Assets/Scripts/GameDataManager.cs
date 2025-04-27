using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class GameDataManager : MonoBehaviour
{
    #region Singleton
    public static GameDataManager Instance { get; private set; }

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
    #endregion

    [Header("Data Collections")]
    [SerializeField] private List<TowerSO> towerSOList = new List<TowerSO>();
    [SerializeField] private List<MonsterSO> monsterSOList = new List<MonsterSO>();
    [SerializeField] private List<LevelDataSO> levelDataList = new List<LevelDataSO>();

    [Header("Economy")]
    [SerializeField] private int currentResources = 100;
    public UnityEvent<int> onResourcesChanged;

    [Header("Game Progress")]
    [SerializeField] private int currentLevelIndex = 0;
    [SerializeField] private int highestUnlockedLevel = 0;
    
    [Header("Settings")]
    [SerializeField] private bool loadDataOnStart = true;

    private void Start()
    {
        if (loadDataOnStart)
        {
            LoadDataSO();
        }
    }

    #region Data Loading
    
    /// <summary>
    /// Loads all ScriptableObject data from Resources folders
    /// </summary>
    public void LoadDataSO()
    {
        // Load Tower Data
        towerSOList.Clear();
        towerSOList.AddRange(Resources.LoadAll<TowerSO>("DataSO/Towers"));
        
        // Load Monster Data
        monsterSOList.Clear();
        monsterSOList.AddRange(Resources.LoadAll<MonsterSO>("DataSO/Monsters"));
        
        // Load Level Data
        levelDataList.Clear();
        levelDataList.AddRange(Resources.LoadAll<LevelDataSO>("DataSO/Levels"));
        
        Debug.Log($"Loaded: {towerSOList.Count} towers, {monsterSOList.Count} monsters, {levelDataList.Count} levels");
    }
    
    #endregion
    
    #region Monster Data
    
    /// <summary>
    /// Gets the first available monster ScriptableObject
    /// </summary>
    public MonsterSO GetMonsterSO()
    {
        if (monsterSOList.Count > 0)
            return monsterSOList[0];
            
        Debug.LogError("No monster data found in GameDataManager!");
        return null;
    }
    
    /// <summary>
    /// Gets a monster ScriptableObject by type
    /// </summary>
    public MonsterSO GetMonsterSOByType(MonsterID id)
    {
        foreach (MonsterSO monster in monsterSOList)
        {
            if (monster.id == id)
                return monster;
        }
        
        Debug.LogWarning($"Monster type {id} not found in GameDataManager!");
        return null;
    }
    
    /// <summary>
    /// Gets a monster ScriptableObject by name
    /// </summary>
    public MonsterSO GetMonsterSOByName(string monsterName)
    {
        foreach (MonsterSO monster in monsterSOList)
        {
            if (monster.monsterName == monsterName)
                return monster;
        }
        
        Debug.LogWarning($"Monster named {monsterName} not found in GameDataManager!");
        return null;
    }
    
    #endregion
    
    #region Tower Data
    
    /// <summary>
    /// Gets all possible tower upgrades for a given tower
    /// </summary>
    public List<TowerSO> GetPossibleUpgrade(TowerSO tow)
    {
        List<TowerSO> possibleUpgrades = new List<TowerSO>();
        
        if (tow == null || tow.possibleUpgrades == null)
            return possibleUpgrades;
            
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

    /// <summary>
    /// Gets all base (level 1) towers
    /// </summary>
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
    
    /// <summary>
    /// Gets a tower by its type
    /// </summary>
    public TowerSO GetTowerByType(TowerType type)
    {
        foreach (TowerSO tower in towerSOList)
        {
            if (tower.type == type)
                return tower;
        }
        
        Debug.LogWarning($"Tower type {type} not found in GameDataManager!");
        return null;
    }
    
    #endregion
    
    #region Level Data
    
    /// <summary>
    /// Gets the level data for a specific level index
    /// </summary>
    public LevelDataSO GetLevelData(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= levelDataList.Count)
        {
            Debug.LogError($"Level index {levelIndex} is out of range!");
            return null;
        }
        
        return levelDataList[levelIndex];
    }
    
    /// <summary>
    /// Gets the current level data
    /// </summary>
    public LevelDataSO GetCurrentLevelData()
    {
        return GetLevelData(currentLevelIndex);
    }
    
    /// <summary>
    /// Sets the current level index
    /// </summary>
    public void SetCurrentLevel(int levelIndex)
    {
        currentLevelIndex = Mathf.Clamp(levelIndex, 0, levelDataList.Count - 1);
    }
    
    /// <summary>
    /// Unlocks the next level
    /// </summary>
    public void UnlockNextLevel()
    {
        highestUnlockedLevel = Mathf.Max(highestUnlockedLevel, currentLevelIndex + 1);
        highestUnlockedLevel = Mathf.Min(highestUnlockedLevel, levelDataList.Count - 1);
    }
    
    /// <summary>
    /// Checks if a level is unlocked
    /// </summary>
    public bool IsLevelUnlocked(int levelIndex)
    {
        return levelIndex <= highestUnlockedLevel;
    }
    
    /// <summary>
    /// Gets the total number of levels
    /// </summary>
    public int GetTotalLevelCount()
    {
        return levelDataList.Count;
    }
    
    #endregion
    
    #region Economy
    
    /// <summary>
    /// Checks if player can afford a cost
    /// </summary>
    public bool CanAfford(float cost)
    {
        return currentResources >= cost;
    }
    
    /// <summary>
    /// Attempts to spend resources
    /// </summary>
    public bool SpendResources(int amount)
    {
        if (currentResources >= amount)
        {
            currentResources -= amount;
            onResourcesChanged?.Invoke(currentResources);
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Adds resources to the player's total
    /// </summary>
    public void AddResources(int amount)
    {
        currentResources += amount;
        onResourcesChanged?.Invoke(currentResources);
    }
    
    /// <summary>
    /// Sets the current resource amount
    /// </summary>
    public void SetCurrentResources(int amount)
    {
        currentResources = Mathf.Max(0, amount);
        onResourcesChanged?.Invoke(currentResources);
    }
    
    /// <summary>
    /// Gets the current resource amount
    /// </summary>
    public int GetCurrentResources()
    {
        return currentResources;
    }
    
    #endregion
    
    #region Save/Load Game State
    
    // TODO: Implement save/load functionality when needed
    // This would include saving current resources, unlocked levels, etc.
    
    #endregion
}