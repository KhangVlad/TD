using System;
using System.Collections.Generic;
using UnityEngine;

public class HeroesController : MonoBehaviour
{
    public static HeroesController Instance { get; private set; }
    public HeroBehavior currentSelected;
    [SerializeField] private LayerMask heroLayer;
    [SerializeField] private Transform[] spawnPositions; // Assign in inspector
    [SerializeField] private List<UnitID> initialHeroes = new List<UnitID>(); // Heroes to spawn initially
    
    private Dictionary<UnitID, HeroBehavior> activeHeroes = new Dictionary<UnitID, HeroBehavior>();
    private string heroPrefabsPath = "DataSO/Units";
    private string heroGameObjectsPath = "GameObjects/Heroes";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        InitializeHeroes();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, 100f, heroLayer);
            
            // If we hit a hero
            if (hit.collider != null && hit.collider.TryGetComponent(out HeroBehavior hero))
            {
                // Select the hero
                SelectHero(hero);
            }
            else if (currentSelected != null)
            {
                // If we have a hero selected and clicked elsewhere, move the hero directly
                MoveSelectedHero(mousePos);
            }
        }
    }

    private void InitializeHeroes()
    {
        // Spawn all initial heroes
        for (int i = 0; i < initialHeroes.Count; i++)
        {
            // Get spawn position (cycle through available positions if needed)
            Vector3 spawnPos = (spawnPositions.Length > 0) ? 
                spawnPositions[i % spawnPositions.Length].position : 
                new Vector3(i * 2.0f, 0, 0); // Default spacing if no positions set
            
            // Spawn the hero
            SpawnHero(initialHeroes[i], spawnPos);
        }
    }

    public HeroBehavior SpawnHero(UnitID heroID, Vector3 position)
    {
        // Check if hero already exists
        if (activeHeroes.TryGetValue(heroID, out HeroBehavior existingHero))
        {
            Debug.LogWarning($"Hero {heroID} already exists in the scene. Repositioning.");
            existingHero.transform.position = position;
            return existingHero;
        }

        // Load hero data from ScriptableObject
        HeroSO heroData = GameDataManager.Instance.GetHeroData(heroID);
        if (heroData == null)
        {
            Debug.LogError($"Failed to load hero data for {heroID}. Make sure the ScriptableObject exists at {heroPrefabsPath}/{heroID.ToString()}.asset");
            return null;
        }


        HeroBehavior[] allHero = Resources.LoadAll<HeroBehavior>(heroGameObjectsPath);
        HeroBehavior a = null;
        for (int i = 0; i < allHero.Length; i++)
        {
            if (allHero[i].heroID == heroID)
            {
                a = allHero[i];
            }
        }
        HeroBehavior heroInstance = Instantiate(a, position, Quaternion.identity);
        heroInstance.name = heroData.unitName;
        a.Initialize(heroData);
        activeHeroes.Add(heroID, a);
        
        Debug.Log($"Spawned hero {heroData.unitName} at {position}");
        return a;
    }

    public HeroBehavior GetHeroByID(UnitID heroID)
    {
        if (activeHeroes.TryGetValue(heroID, out HeroBehavior hero))
        {
            return hero;
        }
        return null;
    }

    private void SelectHero(HeroBehavior hero)
    {
        // Deselect the current hero if there is one
        if (currentSelected != null)
        {
           currentSelected.ListenClickEvent();
        }
        currentSelected = hero;
        hero.ListenClickEvent();
        Debug.Log($"Selected hero: {hero.name}");
    }

    private void MoveSelectedHero(Vector2 targetPosition)
    {
        if (currentSelected != null)
        {
            // Switch to moving state
            currentSelected.SetState(currentSelected.moveState);
            currentSelected.SetTargetPosition(targetPosition);
            currentSelected = null;
            Debug.Log($"Moving hero to: {targetPosition}");
        }
    }
}