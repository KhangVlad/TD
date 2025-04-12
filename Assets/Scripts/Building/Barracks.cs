using UnityEngine;
using  System.Collections.Generic;

public class Barracks : BaseTower
{
    [Header("Barracks Settings")]
    [SerializeField] private int maxSoldiers = 3;
    [SerializeField] private float spawnTime = 5f;
    [SerializeField] private GameObject flagPrefab;
    [SerializeField] private TowerSO towerData;

    private int soldierCount = 0;
    private float spawnTimer = 0f;
    private bool isSpawning = true;
    private Vector2 flagPosition;
    private GameObject flagVisual;

    // Triangle formation offsets for soldier spawning positions
    private Vector2[] triangleOffsets = new Vector2[]
    {
        new Vector2(0, 0.7f),
        new Vector2(-0.6f, -0.365f),
        new Vector2(0.6f, -0.365f),
    };
    
    // References to spawned soldiers
    private List<Soldier> spawnedSoldiers = new List<Soldier>();

    protected override void Start()
    {
        base.Start();
        InitializeTower();
    }

    public void InitializeTower(TowerSO data = null)
    {
        if (data != null)
        {
            towerData = data;
        }
        flagPosition = transform.position + new Vector3(0.5f, 1.5f, 0);
        CreateFlagVisual();
        isSpawning = true;
        soldierCount = 0;
        spawnTimer = 0;
        
        // Apply tower data settings
        if (towerData != null)
        {
           ;
        }
    }

    private void CreateFlagVisual()
    {
        // Destroy previous flag if it exists
        if (flagVisual != null)
        {
            Destroy(flagVisual);
        }
        
        // Create new flag visual
        if (flagPrefab != null)
        {
            flagVisual = Instantiate(flagPrefab, flagPosition, Quaternion.identity);
            flagVisual.transform.SetParent(transform);
        }
        else
        {
            // Create a simple visual if no prefab is assigned
            flagVisual = new GameObject("FlagVisual");
            flagVisual.transform.position = flagPosition;
            flagVisual.transform.SetParent(transform);
            
            SpriteRenderer sr = flagVisual.AddComponent<SpriteRenderer>();
            sr.sprite = Resources.Load<Sprite>("Sprites/Flag");
            sr.color = Color.red;
        }
    }

    protected override void Update()
    {
        base.Update();
        
        // Handle soldier spawning logic
        if (soldierCount < maxSoldiers && isSpawning)
        {
            spawnTimer += Time.deltaTime;
            
            if (spawnTimer >= spawnTime)
            {
                spawnTimer = 0f;
                SpawnSoldier();
            }
        }
        
        // Check for dead soldiers and update count
        UpdateSoldierCount();
    }

    private void UpdateSoldierCount()
    {
        // Remove null references (destroyed soldiers)
        spawnedSoldiers.RemoveAll(s => s == null);
        soldierCount = spawnedSoldiers.Count;
    }

    private void SpawnSoldier()
    {
        if (towerData == null || towerData.soldierPrefab == null)
        {
            Debug.LogError("Soldier prefab is not assigned in TowerSO for " + gameObject.name);
            return;
        }

        if (soldierCount >= maxSoldiers)
            return;

        // Calculate spawn position using the appropriate offset
        Vector2 spawnPosition = transform.position + (Vector3)triangleOffsets[soldierCount % triangleOffsets.Length];
        
        // Spawn the soldier
        GameObject soldierObj = Instantiate(towerData.soldierPrefab, spawnPosition, Quaternion.identity);
        Soldier soldier = soldierObj.GetComponent<Soldier>();
        
        if (soldier != null)
        {
            spawnedSoldiers.Add(soldier);
            soldierCount++;
            
            Debug.Log($"Spawned soldier {soldierCount} at position {spawnPosition}");
        }
    }

    public override void Attack()
    {
    }

    public override void Upgrade()
    {
        base.Upgrade();
        if (towerData != null)
        {
            maxSoldiers = towerData.maxUnits;
            spawnTime = towerData.spawnTime;
        }
    }

    public override void Sell()
    {
        // Destroy all spawned soldiers
        foreach (Soldier soldier in spawnedSoldiers)
        {
            if (soldier != null)
            {
                Destroy(soldier.gameObject);
            }
        }
        
        // Destroy flag visual
        if (flagVisual != null)
        {
            Destroy(flagVisual);
        }
        
        base.Sell();
    }

    public void ChangeFlagPosition(Vector2 newPosition)
    {
        flagPosition = newPosition;
        if (flagVisual != null)
        {
            flagVisual.transform.position = flagPosition;
        }
        
        foreach (Soldier soldier in spawnedSoldiers)
        {
            if (soldier != null)
            {
            }
        }
    }
    
    public void OnFlagPlaced(Vector2 position)
    {
        ChangeFlagPosition(position);
    }
}