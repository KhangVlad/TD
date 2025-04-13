using System;
using UnityEngine;
using System.Collections.Generic;


public interface ITower
{
    void Attack();
    void Upgrade();
    void Sell();
}
[Serializable]
public abstract class BaseTower : MonoBehaviour, ITower
{
    public TowerSO dataSO;
    protected float lastAttackTime;
    protected Transform target;
    protected List<Transform> enemiesInRange = new List<Transform>();
    protected BuildSpot parentSpot;

    protected virtual void Start()
    {
    }

    protected virtual void Update()
    {
       
    }

    protected virtual void FindTarget()
    {
       
    }

    public virtual void Attack()
    {
        
       
    }

    public virtual void Upgrade()
    {
    }

    public virtual void Sell()
    {
        Destroy(gameObject);
    }

    public void SetParentSpot(BuildSpot spot)
    {
        parentSpot = spot;
    }

}




public enum TowerType
{
    None = 0, 
    Barracks = 1,
    Barracks2 = 2,
    Barracks3 = 3,
    Barracks4 = 4,
    
    
    
    Archer = 5,
    Archer2 = 6,
    Archer3 = 7,
    Archer4 = 8,
    Mage = 9,
}