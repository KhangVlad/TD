using System;
using UnityEngine;
using System.Collections.Generic;

public class ArcherTower : Tower
{
    [SerializeField] private Transform archerLeftPos;
    [SerializeField] private Transform archerRightPos;

    public ArcherWithTarget[] archers = new ArcherWithTarget[2];

    protected override void Start()
    {
        base.Start();
        InitializeArcher();
    }

    private void InitializeArcher()
    {
        base.SetFlagColliderRandAndPos(new Vector2(transform.position.x, transform.position.y),
            2.2f); //set range detect enemy, flag to tower pos

        SpawnSoldier();
    }

    private void SpawnSoldier()
    {
        // Make sure array is initialized with the correct size
        if (archers == null || archers.Length != 2)
        {
            archers = new ArcherWithTarget[2];
        }
    
        // Check for null before assignment
        GameObject archer1 = Instantiate(dataSO.soldierPrefab, archerLeftPos.position, Quaternion.identity);
        if (archer1 != null)
        {
            Archer soldier1 = archer1.GetComponent<Archer>();
            if (soldier1 != null)
            {
                soldier1.Initialize(( dataSO.soldierData));
                archers[0] = new ArcherWithTarget(soldier1);
            }
        }
    
        GameObject archer2 = Instantiate(dataSO.soldierPrefab, archerRightPos.position, Quaternion.identity);
        if (archer2 != null)
        {
            Archer soldier2 = archer2.GetComponent<Archer>();
            soldier2.Initialize(( dataSO.soldierData));
            if (soldier2 != null)
            {
                archers[1] = new ArcherWithTarget(soldier2);
            }
        }
    }

    private void UpdateSoldierTargets(Monster entranceMonster)
    {
        _monstersInArea.RemoveAll(monster => monster == null);
    
        // Check array is valid
        if (archers == null) return;
    
        for (int i = 0; i < archers.Length; i++)
        {
            // Check for null before accessing
            if (archers[i] != null && archers[i].monster == null)
            {
                archers[i].SetTarget(entranceMonster);
            }
        }
    }

    protected override void OnMonsterEnterArea(Monster monster)
    {
        UpdateSoldierTargets(monster);
    }

    protected override void OnMonsterExitArea(Monster monster)
    {
        if (monster != null)
        {
            for (int i = 0; i < archers.Length; i++)
            {
                if (archers[i].monster == monster)
                {
                    archers[i].SetTarget(null);
                }
            }
        }
    }
}

[Serializable]
public class ArcherWithTarget
{
    public Archer soldier;
    public Monster monster;

    public ArcherWithTarget(Archer s)
    {
        soldier = s;
        monster = null;
    }

    public void SetTarget(Monster m)
    {
        monster = m;
        soldier.ChangeTarget(m);
    }
}