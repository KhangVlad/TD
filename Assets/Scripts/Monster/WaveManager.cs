using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Manages the spawning of monster waves based on LevelDataSO
/// </summary>
public class WaveManager : MonoBehaviour
{
    #region Singleton

    public static WaveManager Instance { get; private set; }

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

    [Header("Level Configuration")] [SerializeField]
    private LevelDataSO currentLevel;

    [Header("Events")] public UnityEvent onWaveStart;
    public UnityEvent onWaveComplete;
    public UnityEvent onLevelComplete;
    public UnityEvent onLevelFailed;
    public UnityEvent<int, int> onWaveProgress; // Current monster, total monsters

    [Header("Debug")] [SerializeField] private bool autoStartWaves = true;
    [SerializeField] private bool debugMode = false;

    // Private variables
    private int _currentWaveIndex = -1;
    private int _monstersRemainingInWave = 0;
    private int _monstersLeaked = 0;
    private Coroutine _waveCoroutine;
    private bool _isSpawningWave = false;
    private int _totalMonstersSpawnedInCurrentWave = 0;
    private int _totalMonstersInCurrentWave = 0;

    #region Properties

    public bool IsWaveInProgress => _monstersRemainingInWave > 0 || _isSpawningWave;
    public int CurrentWaveIndex => _currentWaveIndex;
    public int TotalWaves => currentLevel != null ? currentLevel.waves.Count : 0;
    public int MonstersLeaked => _monstersLeaked;
    public int MaxAllowedLeaks => currentLevel != null ? currentLevel.maxAllowedLeaks : 0;
    public bool IsLastWave => _currentWaveIndex >= TotalWaves - 1;

    #endregion

    private void Start()
    {
        // if (currentLevel == null)
        // {
        //     Debug.LogError("WaveManager: No level data assigned!");
        //     return;
        // }
        //
        // // Set initial resources
        // if (GameDataManager.Instance != null)
        // {
        //     GameDataManager.Instance.SetCurrentResources(currentLevel.startingResources);
        // }
        //
        // if (autoStartWaves)
        // {
        // }
    }

    private void Update()
    {
        // if (Input.GetKeyDown(KeyCode.A))
        // {
        //     StartNextWave();
        // }
        //
        // if (Input.GetKeyDown(KeyCode.Space))
        // {
        //     SpawnMonster(MonsterID.Goblin);
        // }
        // if (Input.GetKeyDown(KeyCode.A))
        // {
        //     SpawnMonster(MonsterID.Skeleton);
        // }
        if (Input.GetKeyDown(KeyCode.S))
        {
            SpawnMonster(MonsterID.ForestMonster);
        }
    }

    /// <summary>
    /// Starts the next wave in the sequence
    /// </summary>
    public void StartNextWave()
    {
        if (_isSpawningWave)
        {
            Debug.LogWarning("Cannot start new wave while current wave is still spawning");
            return;
        }

        if (_currentWaveIndex >= currentLevel.waves.Count - 1)
        {
            Debug.Log("All waves completed!");
            return;
        }

        _currentWaveIndex++;
        Wave currentWave = currentLevel.waves[_currentWaveIndex];

        // Calculate total monsters in this wave
        _totalMonstersInCurrentWave = 0;
        foreach (WaveMonster monster in currentWave.monsters)
        {
            _totalMonstersInCurrentWave += monster.quantity;
        }

        _monstersRemainingInWave = _totalMonstersInCurrentWave;
        _totalMonstersSpawnedInCurrentWave = 0;

        // Start wave coroutine
        if (_waveCoroutine != null)
        {
            StopCoroutine(_waveCoroutine);
        }

        _waveCoroutine = StartCoroutine(SpawnWaveCoroutine(currentWave));

        if (debugMode)
        {
            Debug.Log($"Starting Wave {_currentWaveIndex + 1}: {currentWave.waveName}");
        }

        onWaveStart?.Invoke();
    }

    /// <summary>
    /// Spawns all monsters in a wave with appropriate delays
    /// </summary>
    private IEnumerator SpawnWaveCoroutine(Wave wave)
    {
        _isSpawningWave = true;

        // Wait for initial wave delay
        if (wave.waveStartDelay > 0)
        {
            yield return new WaitForSeconds(wave.waveStartDelay);
        }

        // For each monster type in the wave
        foreach (WaveMonster monsterData in wave.monsters)
        {
            // Spawn the specified quantity of this monster
            for (int i = 0; i < monsterData.quantity; i++)
            {
                SpawnMonster(monsterData.monsterID);
                _totalMonstersSpawnedInCurrentWave++;

                // Update progress
                onWaveProgress?.Invoke(_totalMonstersSpawnedInCurrentWave, _totalMonstersInCurrentWave);

                // Wait for the delay between spawns
                if (i < monsterData.quantity - 1) // Don't wait after the last one
                {
                    yield return new WaitForSeconds(monsterData.spawnDelay);
                }
            }
        }

        _isSpawningWave = false;

        // Check if this was the last wave and all monsters are defeated
        CheckWaveCompletion();
    }

    /// <summary>
    /// Spawns a single monster of the specified type
    /// </summary>
    private void SpawnMonster(MonsterID monsterID)
    {
        // Get the monster scriptable object for this type
        MonsterSO monsterSO = GameDataManager.Instance.GetMonsterSOByType(monsterID);

        if (monsterSO == null)
        {
            Debug.LogError($"Monster type {monsterID} not found in GameDataManager!");
            return;
        }

        // Use the MonsterManager to spawn the monster
        if (MonsterManager.Instance != null)
        {
            MonsterManager.Instance.SpawnMonster(monsterSO);
        }
        else
        {
            Debug.LogError("MonsterManager instance not found!");
        }
    }

    /// <summary>
    /// Called when a monster is destroyed
    /// </summary>
    public void OnMonsterDefeated()
    {
        _monstersRemainingInWave--;
        CheckWaveCompletion();
    }

    /// <summary>
    /// Called when a monster reaches the end of the path
    /// </summary>
    public void OnMonsterLeaked()
    {
        _monstersRemainingInWave--;
        _monstersLeaked++;

        // Check if player has lost
        if (_monstersLeaked >= currentLevel.maxAllowedLeaks)
        {
            onLevelFailed?.Invoke();

            if (debugMode)
            {
                Debug.Log("Level failed: Too many monsters leaked!");
            }
        }

        CheckWaveCompletion();
    }

    /// <summary>
    /// Checks if the current wave is complete and acts accordingly
    /// </summary>
    private void CheckWaveCompletion()
    {
        if (_monstersRemainingInWave <= 0 && !_isSpawningWave)
        {
            onWaveComplete?.Invoke();

            // Award resources for completing the wave
            if (GameDataManager.Instance != null)
            {
                GameDataManager.Instance.AddResources(currentLevel.resourcesPerWaveCompleted);
            }

            if (debugMode)
            {
                Debug.Log($"Wave {_currentWaveIndex + 1} completed!");
            }

            // Check if this was the last wave
            if (IsLastWave)
            {
                onLevelComplete?.Invoke();

                if (debugMode)
                {
                    Debug.Log("All waves completed! Level finished!");
                }
            }
            else if (autoStartWaves)
            {
                // Start the next wave after the specified delay
                StartCoroutine(StartNextWaveDelayedCoroutine());
            }
        }
    }

    /// <summary>
    /// Starts the next wave after the appropriate delay
    /// </summary>
    private IEnumerator StartNextWaveDelayedCoroutine()
    {
        yield return new WaitForSeconds(currentLevel.timeBetweenWaves);
        StartNextWave();
    }

    /// <summary>
    /// Sets the current level data
    /// </summary>
    public void SetLevelData(LevelDataSO levelData)
    {
        currentLevel = levelData;
        _currentWaveIndex = -1;
        _monstersRemainingInWave = 0;
        _monstersLeaked = 0;
        _isSpawningWave = false;

        if (_waveCoroutine != null)
        {
            StopCoroutine(_waveCoroutine);
            _waveCoroutine = null;
        }
    }

    /// <summary>
    /// Gets the current wave data
    /// </summary>
    public Wave GetCurrentWaveData()
    {
        if (currentLevel == null || _currentWaveIndex < 0 || _currentWaveIndex >= currentLevel.waves.Count)
        {
            return null;
        }

        return currentLevel.waves[_currentWaveIndex];
    }

    /// <summary>
    /// Gets the time between waves from current level data
    /// </summary>
    public float GetTimeBetweenWaves()
    {
        return currentLevel != null ? currentLevel.timeBetweenWaves : 20f;
    }
}