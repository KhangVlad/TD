using System.Collections.Generic;
using UnityEngine;

public class MonsterManager : MonoBehaviour
{
    private static MonsterManager _instance;
    public static MonsterManager Instance => _instance;
    public List<PathNode> mainPath = new List<PathNode>();
    private List<Monster> activeMonsters = new List<Monster>();
    
    public Monster monsterPrefab;
    
    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Destroy(gameObject);
    }

    private void SpawnMonster()
    {
        MonsterSO mon = GameDataManager.Instance?.GetMonsterSO();
        if (mon == null)
        {
            Debug.LogError("MonsterSO is null or GameDataManager.Instance is not initialized.");
            return;
        }
        PathNode spawnNode = mainPath[0];
        Monster monster = Instantiate(monsterPrefab, spawnNode.transform.position, Quaternion.identity);
        monster.Initialize(mon);
    }
    
    private void Update()
    {
        // Update all monsters' states in one place
        for (int i = activeMonsters.Count - 1; i >= 0; i--)
        {
            activeMonsters[i].CurrentState.UpdateState(activeMonsters[i]);
        }
        if( Input.GetKeyDown(KeyCode.Space))
        {
            SpawnMonster();
        }
    }
    
    public Vector3 GetNodePosition(int nodeIndex)
    {
        if (nodeIndex < 0 || nodeIndex >= mainPath.Count)
            return Vector3.zero;
            
        return mainPath[nodeIndex].transform.position;
    }
    
    public bool IsPathComplete(int nodeIndex)
    {
        return nodeIndex >= mainPath.Count;
    }
    
    public void RegisterMonster(Monster monster)
    {
        activeMonsters.Add(monster);
        if (mainPath.Count > 0)
        {
            monster.transform.position = mainPath[0].transform.position;
            monster.currentNodeIndex = 1; 
            monster.ChangeState(MonsterState.Moving);
        }
    }
    
    public void RemoveMonster(Monster monster)
    {
        activeMonsters.Remove(monster);
    }
}