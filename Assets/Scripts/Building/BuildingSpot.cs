using System;
using UnityEngine;

public class BuildingSpot : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    public Tower tower;

    public void SetNewTower(TowerSO so)
    {
        if (so == null)
        {
            Debug.LogError("TowerSO is null");
            return;
        }

        tower = so.type switch
        {
            TowerType.Warrior => new Warrior(so),
            TowerType.Mage => new Mage(so),
            TowerType.Archer => new Archer(so),
            TowerType.Barracks => new Barracks(so),
            _ => throw new ArgumentException($"Invalid tower type: {so.type}")
        };

        spriteRenderer.sprite = so.sprite;
    }
}

[Serializable]
public abstract class Tower
{
    public TowerSO dataSO { get; }

    protected Tower(TowerSO so)
    {
        dataSO = so ?? throw new ArgumentNullException(nameof(so));
    }

    public virtual void Upgrade(TowerSO so)
    {
        Debug.Log($"Upgrading tower to {so.type}");
    }
}

public class Barracks : Tower
{
    public Barracks(TowerSO so) : base(so)
    {
        // Barracks-specific initialization
    }

    public override void Upgrade(TowerSO so)
    {
        base.Upgrade(so);
        // Additional upgrade logic for Barracks
    }
}

public class Warrior : Tower
{
    public Warrior(TowerSO so) : base(so)
    {
        // Warrior-specific initialization
    }

    public override void Upgrade(TowerSO so)
    {
        base.Upgrade(so);
        // Additional upgrade logic for Warrior
    }
}

public class Mage : Tower
{
    public Mage(TowerSO so) : base(so)
    {
        // Mage-specific initialization
    }

    public override void Upgrade(TowerSO so)
    {
        base.Upgrade(so);
        // Additional upgrade logic for Mage
    }
}

public class Archer : Tower
{
    public Archer(TowerSO so) : base(so)
    {
        // Archer-specific initialization
    }

    public override void Upgrade(TowerSO so)
    {
        base.Upgrade(so);
        // Additional upgrade logic for Archer
    }
}

public enum TowerType
{
    None = 0, // Build spot
    Warrior = 1,
    Mage = 2,
    Archer = 3,
    Barracks = 4
}