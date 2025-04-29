using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;

public static class UnitDataImporterCLI
{
    [MenuItem("Tools/TD/Import Unit Data From JSON File")]
    public static void ImportUnitDataFromFile()
    {
        string jsonPath = EditorUtility.OpenFilePanel("Select Unit JSON File", "", "json");
        if (string.IsNullOrEmpty(jsonPath))
            return;

        string jsonText = File.ReadAllText(jsonPath);
        ImportUnitDataFromJson(jsonText);
    }

    [MenuItem("Tools/TD/Import Unit Data From Text")]
    public static void ImportUnitDataFromText()
    {
        // Create a temporary window to get text input
        TextInputWindow window = EditorWindow.GetWindow<TextInputWindow>(true, "JSON Input");
        window.onTextSubmitted = ImportUnitDataFromJson;
        window.Show();
    }
    
    // Helper class for text input
    private class TextInputWindow : EditorWindow
    {
        public System.Action<string> onTextSubmitted;
        private string jsonText = "";
        private Vector2 scrollPosition;
        
        private void OnGUI()
        {
            EditorGUILayout.LabelField("Paste JSON Data:", EditorStyles.boldLabel);
            
            EditorGUILayout.Space();
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(position.height - 100));
            jsonText = EditorGUILayout.TextArea(jsonText, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.Space();
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Cancel"))
            {
                Close();
            }
            
            if (GUILayout.Button("Import"))
            {
                if (!string.IsNullOrEmpty(jsonText))
                {
                    onTextSubmitted?.Invoke(jsonText);
                    Close();
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "Please enter JSON data.", "OK");
                }
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    private static void ImportUnitDataFromJson(string jsonText)
    {
        UnitDataContainer data;
        try
        {
            data = JsonConvert.DeserializeObject<UnitDataContainer>(jsonText);
            
            if (data == null || data.units == null)
            {
                Debug.LogError("Failed to parse JSON data or no units found!");
                EditorUtility.DisplayDialog("Error", "Failed to parse JSON data or no units found!", "OK");
                return;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"JSON parsing error: {ex.Message}");
            EditorUtility.DisplayDialog("Error", $"JSON parsing error: {ex.Message}", "OK");
            return;
        }
        
        string savePath = "Assets/Resources/DataSO/Units";
        
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
                    SoldierSO soldier = ScriptableObject.CreateInstance<SoldierSO>();
                    SetBaseUnitProperties(soldier, unitData);
                    soldier.damage = unitData.damage;
                    soldier.meleeAttackBonus = unitData.meleeAttackBonus;
                    SaveAsset(soldier, unitData.unitName, savePath);
                    createdCount++;
                    break;
                    
                case "Archer":
                    ArcherSO archer = ScriptableObject.CreateInstance<ArcherSO>();
                    SetBaseUnitProperties(archer, unitData);
                    archer.arrowSpeed = unitData.arrowSpeed;
                    archer.arrowLifetime = unitData.arrowLifetime;
                    archer.criticalHitChance = unitData.criticalHitChance;
                    archer.criticalHitMultiplier = unitData.criticalHitMultiplier;
                    SaveAsset(archer, unitData.unitName, savePath);
                    createdCount++;
                    break;
                    
                case "Hero":
                    HeroSO hero = ScriptableObject.CreateInstance<HeroSO>();
                    SetBaseUnitProperties(hero, unitData);
                    SaveAsset(hero, unitData.unitName, savePath);
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
    
    private static void SetBaseUnitProperties(UnitSO unit, UnitData data)
    {
        unit.unitID = (UnitID)data.unitID;
        unit.unitName = data.unitName;
        unit.health = data.health;
        unit.attackDamage = data.attackDamage;
        unit.attackRange = data.attackRange;
        unit.attackSpeed = data.attackSpeed;
        unit.moveSpeed = data.moveSpeed;
    }
    
    private static void SaveAsset(ScriptableObject asset, string name, string savePath)
    {
        string assetPath = $"{savePath}/{name}.asset";
        AssetDatabase.CreateAsset(asset, assetPath);
        Debug.Log($"Created ScriptableObject at {assetPath}");
    }
}

