using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;

public class UnitDataImporter : EditorWindow
{
    private TextAsset jsonFile;
    private string savePath = "Assets/Resources/DataSO/Units";
    
    [MenuItem("Tools/TD/Import Unit Data")]
    public static void ShowWindow()
    {
        GetWindow<UnitDataImporter>("Unit Data Importer");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Unit Data Importer", EditorStyles.boldLabel);
        
        EditorGUILayout.Space();
        
        jsonFile = (TextAsset)EditorGUILayout.ObjectField("JSON File", jsonFile, typeof(TextAsset), false);
        
        savePath = EditorGUILayout.TextField("Save Path", savePath);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Import"))
        {
            if (jsonFile != null)
            {
                ImportUnitData();
            }
            else
            {
                Debug.LogError("Please select a JSON file first!");
            }
        }
    }
    
    private void ImportUnitData()
    {
        string jsonText = jsonFile.text;
        UnitDataContainer data = JsonConvert.DeserializeObject<UnitDataContainer>(jsonText);
        
        if (data == null || data.units == null)
        {
            Debug.LogError("Failed to parse JSON data or no units found!");
            return;
        }
        
        // Ensure directory exists
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }
        
        int createdCount = 0;
        
        foreach (UnitData unitData in data.units)
        {
            switch (unitData.type)
            {
                case "Soldier":
                    SoldierSO soldier = CreateInstance<SoldierSO>();
                    SetBaseUnitProperties(soldier, unitData);
                    soldier.damage = unitData.damage;
                    soldier.meleeAttackBonus = unitData.meleeAttackBonus;
                    SaveAsset(soldier, unitData.unitName);
                    createdCount++;
                    break;
                    
                case "Archer":
                    ArcherSO archer = CreateInstance<ArcherSO>();
                    SetBaseUnitProperties(archer, unitData);
                    archer.arrowSpeed = unitData.arrowSpeed;
                    archer.arrowLifetime = unitData.arrowLifetime;
                    archer.criticalHitChance = unitData.criticalHitChance;
                    archer.criticalHitMultiplier = unitData.criticalHitMultiplier;
                    SaveAsset(archer, unitData.unitName);
                    createdCount++;
                    break;
                    
                case "Hero":
                    HeroSO hero = CreateInstance<HeroSO>();
                    SetBaseUnitProperties(hero, unitData);
                    hero.detectionRadius = unitData.detectionRadius;
                    SaveAsset(hero, unitData.unitName);
                    createdCount++;
                    break;
                    
                default:
                    Debug.LogWarning($"Unknown unit type: {unitData.type}");
                    break;
            }
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"Successfully created {createdCount} unit ScriptableObjects!");
    }
    
    private void SetBaseUnitProperties(UnitSO unit, UnitData data)
    {
        unit.unitID = (UnitID)data.unitID;
        unit.unitName = data.unitName;
        unit.health = data.health;
        unit.attackDamage = data.attackDamage;
        unit.attackRange = data.attackRange;
        unit.attackSpeed = data.attackSpeed;
        unit.moveSpeed = data.moveSpeed;
    }
    
    private void SaveAsset(ScriptableObject asset, string name)
    {
        string assetPath = $"{savePath}/{name}.asset";
        AssetDatabase.CreateAsset(asset, assetPath);
        Debug.Log($"Created ScriptableObject at {assetPath}");
    }
}

// Classes for JSON deserialization
[System.Serializable]
public class UnitDataContainer
{
    public List<UnitData> units;
}

[System.Serializable]
public class UnitData
{
    public string type;
    public int unitID;
    public string unitName;
    public float health;
    public float attackDamage;
    public float attackRange;
    public float attackSpeed;
    public float moveSpeed;

    // Soldier specific properties
    public float damage;
    public float meleeAttackBonus;

    // Archer specific properties
    public float arrowSpeed;
    public float arrowLifetime;
    public float criticalHitChance;
    public float criticalHitMultiplier;
    
    //hero
    public float detectionRadius;
}


// Hero specific properties can be added here if needed
