using UnityEngine;
using System;
using System.Collections.Generic;

public class Barracks : BaseTower
{
    public event Action<Vector2> OnFlagPositionChanged;
    
    [Header("Barracks Settings")]
    [SerializeField] private int maxSoldiers = 3;
    [SerializeField] private float spawnTime = 5f;
    [SerializeField] private GameObject flagPrefab;
    [SerializeField] private TowerSO towerData;
    private float _activityRange = 1.5f;
    
    private Soldier[] _soldiers = new Soldier[3];
    private int _soldierCount = 0;
    private float _spawnTimer = 0f;
    private Vector2 _flagPosition;
    private Vector2[] triangleOffsets = new Vector2[]
    {
        new Vector2(0f, 0.49f),
        new Vector2(-0.42f, -0.2555f),
        new Vector2(0.42f, -0.2555f),
    };

    
  

    protected override void Start()
    {
        base.Start();
    }

    public void InitializeTower(TowerSO data)
    {
        towerData = data;
        _flagPosition = transform.position + new Vector3(0, 0.7f, 0);
        _soldierCount = 0;
        _spawnTimer = 0;
    }

    protected override void Update()
    {
        base.Update();
        if (_soldierCount < maxSoldiers )
        {
            _spawnTimer += Time.deltaTime;
            
            if (_spawnTimer >= spawnTime)
            {
                _spawnTimer = 0f;
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
        if (_soldierCount >= maxSoldiers)
            return;
        int formationIndex = _soldierCount % triangleOffsets.Length;
        // Vector2 spawnPosition = _flagPosition + triangleOffsets[formationIndex];
        GameObject soldierObj = Instantiate(towerData.soldierPrefab, transform.position, Quaternion.identity);
        Soldier soldier = soldierObj.GetComponent<Soldier>();
        _soldiers[_soldierCount] = soldier;
        soldier.InitializeWithFlagPosition(_flagPosition, triangleOffsets[formationIndex],this);
        _soldierCount++;
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

    private void ChangeFlagPosition(Vector2 newPosition)
    {
        if (Vector2.Distance(transform.position, newPosition) > _activityRange)
            return;

        _flagPosition = newPosition;
        OnFlagPositionChanged?.Invoke(newPosition);
    }



    
    
    public void OnFlagPlaced(Vector2 position)
    {
        ChangeFlagPosition(position);
    }


}