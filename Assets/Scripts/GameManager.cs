using System;
using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public List<PathNode> pathNodes;
    public Monster monsterPrefab;

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


    private void Update()
    {
       if(Input.GetKeyDown(KeyCode.Space))
       {
           SpawnMonster();
       }
    }
    
    private void SpawnMonster()
    {
        if (pathNodes == null || pathNodes.Count == 0)
        {
            Debug.LogWarning("No path nodes available to spawn the monster.");
            return;
        }

        if (monsterPrefab == null)
        {
            Debug.LogError("Monster prefab is not assigned.");
            return;
        }

        MonsterSO monsterSO = GameDataManager.Instance?.GetMonsterSO();
        if (monsterSO == null)
        {
            Debug.LogError("MonsterSO is null or GameDataManager.Instance is not initialized.");
            return;
        }

        PathNode spawnNode = pathNodes[0];
        Monster monster = Instantiate(monsterPrefab, spawnNode.transform.position, Quaternion.identity);
        monster.Initialize(monsterSO, pathNodes, monsterSO.speed);
    }
}

