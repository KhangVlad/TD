using UnityEngine;
using System.Collections.Generic;
using System;

// Define the event arguments for flag position change
public class FlagPositionChangedEventArgs : EventArgs
{
    public Vector2 NewPosition { get; private set; }
    public Vector2[] FormationOffsets { get; private set; }
    
    public FlagPositionChangedEventArgs(Vector2 newPosition, Vector2[] formationOffsets)
    {
        NewPosition = newPosition;
        FormationOffsets = formationOffsets;
    }
}

public class Barracks : BaseTower
{
    // Event that soldiers can subscribe to
    public event Action<Vector2> OnFlagPositionChanged;
    
    [Header("Barracks Settings")]
    [SerializeField] private int maxSoldiers = 3;
    [SerializeField] private float spawnTime = 5f;
    [SerializeField] private GameObject flagPrefab;
    [SerializeField] private TowerSO towerData;
    
    public Soldier[] Soldiers = new Soldier[3];

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
        if (soldierCount < maxSoldiers && isSpawning)
        {
            spawnTimer += Time.deltaTime;
            
            if (spawnTimer >= spawnTime)
            {
                spawnTimer = 0f;
                SpawnSoldier();
            }
        }
        UpdateSoldierCount();
    }

    private void UpdateSoldierCount()
    {
        
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

        int formationIndex = soldierCount % triangleOffsets.Length;
        Vector2 spawnPosition = flagPosition + triangleOffsets[formationIndex];
        GameObject soldierObj = Instantiate(towerData.soldierPrefab, spawnPosition, Quaternion.identity);
        Soldier soldier = soldierObj.GetComponent<Soldier>();
        Soldiers[soldierCount] = soldier;
        soldier.SetFormationIndex(formationIndex);
        soldier.InitializeWithFlagPosition(flagPosition, triangleOffsets[formationIndex],this);
        soldierCount++;
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
        base.Sell();
    }

    public void ChangeFlagPosition(Vector2 newPosition)
    {
        flagPosition = newPosition;
        if (flagVisual != null)
        {
            flagVisual.transform.position = newPosition;
        }
        OnFlagPositionChanged?.Invoke(newPosition);
    }
    
    public void OnFlagPlaced(Vector2 position)
    {
        Debug.Log("Flag placed at: " + position);
        ChangeFlagPosition(position);
    }
}