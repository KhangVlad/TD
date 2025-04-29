using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class Barracks : Tower
{
    [Header("Barracks Settings")] [SerializeField]
    private int maxSoldiers = 3;

    [SerializeField] private float spawnTime = 5f;
    [SerializeField] private float _detectRange = 1f;
    [SerializeField] private float flagAnimationDuration = 0.5f;

    [Header("Flag Area")] [SerializeField] private float _activityRange = 1.5f;

    private readonly Vector2[] _triangleOffsets = new Vector2[]
    {
        new Vector2(0f, 0.49f),
        new Vector2(-0.42f, -0.2555f),
        new Vector2(0.42f, -0.2555f),
    };

    private SoldierWithTarget[] _soldiers;
    private Coroutine _flagCoroutine;
    private float _spawnTimer = 0f;
    private int _soldierCount = 0;

    #region Unity Lifecycle

    protected override void Start()
    {
        base.Start();
        _soldiers = new SoldierWithTarget[maxSoldiers];
    }

    protected override void Update()
    {
        base.Update();
        TrySpawnSoldier();
    }

    #endregion

    #region Tower Initialization

    public void InitializeTower(TowerSO data)
    {
        base.SetDataSO(data);
        base.flagPosition = transform.position + new Vector3(0, 0.7f, 0);
        _soldierCount = 0;
        _spawnTimer = 0;
        UpdateFlagColliderPosition();
        _soldiers = new SoldierWithTarget[data.maxUnits];
    }

    #endregion

    #region Soldier Management

    private void TrySpawnSoldier()
    {
        if (_soldierCount >= maxSoldiers)
        {
            return;
        }

        _spawnTimer += Time.deltaTime;

        if (_spawnTimer >= spawnTime)
        {
            _spawnTimer = 0f;
            SpawnSoldier();
        }
    }

    private void SpawnSoldier()
    {
        if (_soldierCount >= maxSoldiers)
        {
            return;
        }

        int slotIndex = -1;
        for (int i = 0; i < _soldiers.Length; i++)
        {
            if (_soldiers[i] == null || _soldiers[i].soldier == null)
            {
                slotIndex = i;
                break;
            }
        }

        if (slotIndex == -1)
        {
            return;
        }

        // Determine formation position
        int formationIndex = slotIndex % _triangleOffsets.Length;

        // Instantiate soldier
        GameObject soldierObj = Instantiate(base.dataSO.soldierPrefab, transform.position, Quaternion.identity);
        Soldier soldier = soldierObj.GetComponent<Soldier>();

        _soldiers[slotIndex] = new SoldierWithTarget(soldier);
        soldier.InitializeWithFlagPosition(flagPosition, _triangleOffsets[formationIndex], this,
            base.dataSO.soldierData as SoldierSO);
        _soldierCount++;
        if (_monstersInArea.Count > 0)
        {
            UpdateSoldierTargets(null);
        }
    }

    private void UpdateSoldierCount()
    {
        int activeCount = 0;

        for (int i = 0; i < _soldiers.Length; i++)
        {
            if (_soldiers[i]?.soldier != null)
            {
                activeCount++;
            }
            else
            {
                _soldiers[i] = null;
            }
        }

        _soldierCount = activeCount;
    }

    #endregion

    #region Monster Targeting

    private void UpdateSoldierTargets(MonsterBase entranceMonster)
    {
        _monstersInArea.RemoveAll(monster => monster == null);
        UpdateSoldierCount();
        if (_monstersInArea.Count == 0 || _soldierCount == 0)
        {
            return;
        }

        List<SoldierWithTarget> activeSoldiers = new List<SoldierWithTarget>();
        foreach (var soldierWithTarget in _soldiers)
        {
            if (soldierWithTarget?.soldier != null)
            {
                activeSoldiers.Add(soldierWithTarget);
            }
        }

        foreach (var soldier in activeSoldiers)
        {
            soldier.monster = null;
        }

        if (_monstersInArea.Count >= activeSoldiers.Count)
        {
            AssignOneMonsterPerSoldier(activeSoldiers, entranceMonster);
        }
        else
        {
            DistributeSoldiersToMonsters(activeSoldiers, entranceMonster);
        }
    }

    protected override void OnMonsterEnterArea(MonsterBase monster)
    {
        UpdateSoldierTargets(monster);
    }

    protected override void OnMonsterExitArea(MonsterBase monster)
    {
        if (monster != null)
        {
            _monstersInArea.Remove(monster);

            // Clear this monster from any soldiers targeting it
            foreach (var soldierWithTarget in _soldiers)
            {
                if (soldierWithTarget?.monster == monster)
                {
                    soldierWithTarget.monster = null;
                    if (soldierWithTarget.soldier != null)
                    {
                        soldierWithTarget.SetTarget(null);
                    }
                }
            }

            UpdateSoldierTargets(null);
        }
    }

    private void AssignOneMonsterPerSoldier(List<SoldierWithTarget> soldiers, MonsterBase entranceMonster)
    {
        List<MonsterBase> availableMonsters = new List<MonsterBase>(_monstersInArea);

        if (entranceMonster != null && availableMonsters.Contains(entranceMonster))
        {
            soldiers[0].SetTarget(entranceMonster);
            availableMonsters.Remove(entranceMonster);
            for (int i = 1; i < soldiers.Count; i++)
            {
                if (availableMonsters.Count > 0)
                {
                    MonsterBase target = availableMonsters[0];
                    soldiers[i].SetTarget(target);
                    availableMonsters.RemoveAt(0);
                }
                else
                {
                    MonsterBase target = _monstersInArea[i % _monstersInArea.Count];
                    soldiers[i].SetTarget(target);
                }
            }
        }
        else
        {
            for (int i = 0; i < soldiers.Count; i++)
            {
                if (i < availableMonsters.Count)
                {
                    soldiers[i].SetTarget(availableMonsters[i]);
                }
                else
                {
                    soldiers[i].SetTarget(_monstersInArea[i % _monstersInArea.Count]);
                }
            }
        }
    }

    private void DistributeSoldiersToMonsters(List<SoldierWithTarget> soldiers, MonsterBase entranceMonster)
    {
        int soldiersPerMonster = Mathf.Max(1, soldiers.Count / _monstersInArea.Count);
        int currentMonsterIndex = 0;
        int currentSoldierIndex = 0;
        if (entranceMonster != null && _monstersInArea.Contains(entranceMonster))
        {
            for (int i = 0; i < soldiersPerMonster && currentSoldierIndex < soldiers.Count; i++)
            {
                soldiers[currentSoldierIndex].SetTarget(entranceMonster);
                currentSoldierIndex++;
            }

            List<MonsterBase> remainingMonsters = new List<MonsterBase>(_monstersInArea);
            remainingMonsters.Remove(entranceMonster);

            while (currentSoldierIndex < soldiers.Count && remainingMonsters.Count > 0)
            {
                MonsterBase currentMonster = remainingMonsters[currentMonsterIndex];

                for (int i = 0; i < soldiersPerMonster && currentSoldierIndex < soldiers.Count; i++)
                {
                    soldiers[currentSoldierIndex].SetTarget(currentMonster);
                    currentSoldierIndex++;
                }

                currentMonsterIndex = (currentMonsterIndex + 1) % remainingMonsters.Count;
            }

            if (currentSoldierIndex < soldiers.Count)
            {
                currentMonsterIndex = 0; // Start from the beginning of the monster list
                while (currentSoldierIndex < soldiers.Count)
                {
                    soldiers[currentSoldierIndex].SetTarget(_monstersInArea[currentMonsterIndex]);
                    currentSoldierIndex++;
                    currentMonsterIndex = (currentMonsterIndex + 1) % _monstersInArea.Count;
                }
            }
        }
        else
        {
            // No entrance monster, distribute evenly
            for (int i = 0; i < soldiers.Count; i++)
            {
                MonsterBase target = _monstersInArea[currentMonsterIndex];
                soldiers[i].SetTarget(target);

                // Move to next monster after assigning soldiersPerMonster
                if ((i + 1) % soldiersPerMonster == 0)
                {
                    currentMonsterIndex = (currentMonsterIndex + 1) % _monstersInArea.Count;
                }
            }
        }
    }

    #endregion

    #region Flag Management

    public void PutFlag(Vector2 position)
    {
        if (!IsPointInRange(position)) return;
        base.OnFlagPlaced(position);
    }

    public bool IsPointInRange(Vector2 p)
    {
        float distance = Vector2.Distance(p, transform.position);
        return distance <= _activityRange;
    }

    #endregion
}

[Serializable]
public class SoldierWithTarget
{
    public Soldier soldier;
    public MonsterBase monster;

    public SoldierWithTarget(Soldier s)
    {
        soldier = s;
        monster = null;
    }

    public void SetTarget(MonsterBase m)
    {
        monster = m;
        soldier.ChangeTarget(m);
    }
}