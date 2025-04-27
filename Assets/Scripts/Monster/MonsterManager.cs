using System.Collections.Generic;
using UnityEngine;

public class MonsterManager : MonoBehaviour
{
    #region Singleton
    public static MonsterManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion
    
    [Header("Path Configuration")]
    public List<PathNode> mainPath = new List<PathNode>();
    
    [Header("Monster Configuration")]
    [SerializeField] private MonsterBase monsterPrefab;
    [SerializeField] private MonsterBase skeletonPrefab;
    [SerializeField] private MonsterBase forestMonsterPrefab;
    [SerializeField] private Transform monsterContainer;
    
    private List<MonsterBase> activeMonsters = new List<MonsterBase>();
    
    private void Start()
    {
        // Verify path is set up correctly
        if (mainPath.Count == 0)
        {
            Debug.LogError("Monster path is not set up! Add PathNode components to the monster path.");
        }
        
        // Create monster container if needed
        if (monsterContainer == null)
        {
            monsterContainer = new GameObject("MonsterContainer").transform;
            monsterContainer.SetParent(transform);
        }
    }
    
    /// <summary>
    /// Spawns a monster with the specified monster data
    /// </summary>
    public void SpawnMonster(MonsterSO monsterData)
    {
        if (monsterData == null)
        {
            Debug.LogError("Cannot spawn monster: MonsterSO is null");
            return;
        }
        
        if (monsterPrefab == null)
        {
            Debug.LogError("Cannot spawn monster: monster prefab is not assigned");
            return;
        }
        
        if (mainPath.Count == 0)
        {
            Debug.LogError("Cannot spawn monster: path is not defined");
            return;
        }
        
        // Spawn at the first path node
        PathNode spawnNode = mainPath[0];
        // MonsterBase monster = Instantiate(monsterPrefab, spawnNode.transform.position, Quaternion.identity, monsterContainer);
        MonsterBase monster = null;
        switch (monsterData.id)
        {
            case MonsterID.Skeleton:
                monster = Instantiate(monsterPrefab, spawnNode.transform.position, Quaternion.identity, monsterContainer);
                break;
            case MonsterID.ForestMonster:
                monster = Instantiate(forestMonsterPrefab, spawnNode.transform.position, Quaternion.identity, monsterContainer);
                break;
        }
        
        // Initialize the monster with its data
        monster.Initialize(monsterData);
    }
    
    /// <summary>
    /// Returns the position of a path node by index
    /// </summary>
    public Vector3 GetNodePosition(int nodeIndex)
    {
        if (nodeIndex < 0 || nodeIndex >= mainPath.Count)
            return Vector3.zero;
            
        return mainPath[nodeIndex].transform.position;
    }
    
    /// <summary>
    /// Checks if the path is complete based on node index
    /// </summary>
    public bool IsPathComplete(int nodeIndex)
    {
        return nodeIndex >= mainPath.Count;
    }
    
    /// <summary>
    /// Registers a monster with the manager
    /// </summary>
    public void RegisterMonster(MonsterBase monster)
    {
        if (!activeMonsters.Contains(monster))
        {
            activeMonsters.Add(monster);
        }
        
        if (mainPath.Count > 0)
        {
            monster.transform.position = mainPath[0].transform.position;
            monster.currentNodeIndex = 1; 
            monster.ChangeState(MonsterState.Moving);
        }
    }
    
    /// <summary>
    /// Removes a monster from the manager
    /// </summary>
    public void RemoveMonster(MonsterBase monster)
    {
        activeMonsters.Remove(monster);
    }
    
    /// <summary>
    /// Called when a monster reaches the end of the path
    /// </summary>
    public void OnMonsterReachedEnd(MonsterBase monster)
    {
        // Notify the WaveManager that a monster leaked through
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnMonsterLeaked();
        }
        
        RemoveMonster(monster);
    }
    
    /// <summary>
    /// Called when a monster is defeated by the player
    /// </summary>
    public void OnMonsterDefeated(MonsterBase monster, int rewardAmount)
    {
        // Add resources for defeating the monster
        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.AddResources(rewardAmount);
        }
        
        // Notify the WaveManager that a monster was defeated
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnMonsterDefeated();
        }
        
        RemoveMonster(monster);
    }
    

    
    /// <summary>
    /// Gets the number of active monsters
    /// </summary>
    public int GetActiveMonsterCount()
    {
        return activeMonsters.Count;
    }
    
    /// <summary>
    /// For debugging only: Spawns a test monster
    /// </summary>
    public void SpawnTestMonster()
    {
        MonsterSO monsterSO = GameDataManager.Instance?.GetMonsterSO();
        if (monsterSO != null)
        {
            SpawnMonster(monsterSO);
        }
    }
}