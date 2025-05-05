using UnityEngine;
public static class GameObjectFactory
{
    private const string UnitsFolderPath = "GameObjects/Units";
    private const string HeroesFolderPath = "GameObjects/Heroes";
    private const string TowersFolderPath = "GameObjects/Towers";
    
    public static Unit CreateUnitGameObject(UnitID id, Vector2 pos)
    {
        // Load all prefabs from the Resources folder
        Unit[] allPrefabs = Resources.LoadAll<Unit>(UnitsFolderPath);
        
        // Find the prefab with matching ID
        foreach (Unit unit in allPrefabs)
        {
            if (unit != null)
            {
                if (unit.id == id)
                {   
                    // Instantiate the prefab at the specified position
                    Unit instantiatedUnit = GameObject.Instantiate(unit, pos, Quaternion.identity);
                    return instantiatedUnit;
                }
            }
        }
        
        Debug.LogError($"No unit prefab found with ID: {id}");
        return null;
    }
    
    public static Unit CreateHeroGameObject(UnitID id, Vector2 pos)
    {
        // Load all hero prefabs from the Resources folder
        Unit[] allPrefabs = Resources.LoadAll<Unit>(HeroesFolderPath);
        
        // Find the prefab with matching ID
        foreach (Unit hero in allPrefabs)
        {
            if (hero != null)
            {
                if (hero.id == id)
                {   
                    // Instantiate the prefab at the specified position
                    Unit instantiatedHero = GameObject.Instantiate(hero, pos, Quaternion.identity);
                    return instantiatedHero;
                }
            }
        }
        
        Debug.LogError($"No hero prefab found with ID: {id}");
        return null;
    }
    
    public static Tower CreateTowerGameObject(TowerID id, Vector2 pos)
    {
        // Load all tower prefabs from the Resources folder
        Tower[] allPrefabs = Resources.LoadAll<Tower>(TowersFolderPath);
        
        // Find the prefab with matching ID
        foreach (Tower tower in allPrefabs)
        {
            if (tower != null)
            {
                if (tower.TowerID == id)
                {   
                    // Instantiate the prefab at the specified position
                    Tower instantiatedTower = GameObject.Instantiate(tower, pos, Quaternion.identity);
                    return instantiatedTower;
                }
            }
        }
        
        Debug.LogError($"No tower prefab found with ID: {id}");
        return null;
    }
}