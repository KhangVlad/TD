using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Defines a single monster in a wave
/// </summary>
[Serializable]
public class WaveMonster
{
    [FormerlySerializedAs("monsterType")] public MonsterID monsterID;
    public int quantity;
    [Tooltip("Delay between spawning each monster of this type (in seconds)")]
    public float spawnDelay = 1f;
}

/// <summary>
/// Defines a single wave of monsters
/// </summary>
[Serializable]
public class Wave
{
    public string waveName;
    [Tooltip("Time before this wave starts (in seconds)")]
    public float waveStartDelay;
    public List<WaveMonster> monsters = new List<WaveMonster>();
}

/// <summary>
/// Scriptable Object that defines a complete level with multiple waves
/// </summary>
[CreateAssetMenu(fileName = "New Level", menuName = "TD/LevelDataSO", order = 2)]
public class LevelDataSO : ScriptableObject
{
    [Header("Level Information")]
    public string levelName;
    public int levelNumber;
    public Sprite levelThumbnail;
    [TextArea(3, 5)]
    public string levelDescription;
    
    [Header("Economy")]
    public int startingResources = 100;
    public int resourcesPerWaveCompleted = 50;
    
    [Header("Wave Configuration")]
    [Tooltip("Time between consecutive waves (in seconds)")]
    public float timeBetweenWaves = 20f;
    public List<Wave> waves = new List<Wave>();
    
    [Header("Victory Conditions")]
    public int maxAllowedLeaks = 10; // How many monsters can reach the end before losing
    
    // Helper method to get total number of monsters in the level
    public int GetTotalMonsterCount()
    {
        int total = 0;
        foreach (Wave wave in waves)
        {
            foreach (WaveMonster monster in wave.monsters)
            {
                total += monster.quantity;
            }
        }
        return total;
    }
    
    // Helper method to calculate approximate level duration
    public float GetApproximateLevelDuration()
    {
        float totalTime = 0;
        
        for (int i = 0; i < waves.Count; i++)
        {
            // Add wave start delay
            totalTime += waves[i].waveStartDelay;
            
            // Add time for spawning monsters in the wave
            float waveSpawnTime = 0;
            foreach (WaveMonster monster in waves[i].monsters)
            {
                float monsterSpawnTime = monster.spawnDelay * (monster.quantity - 1);
                waveSpawnTime = Mathf.Max(waveSpawnTime, monsterSpawnTime);
            }
            
            totalTime += waveSpawnTime;
            
            // Add time between waves (except after the last wave)
            if (i < waves.Count - 1)
            {
                totalTime += timeBetweenWaves;
            }
        }
        
        return totalTime;
    }
}