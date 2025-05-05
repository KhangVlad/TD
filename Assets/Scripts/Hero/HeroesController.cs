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
                SelectHero(hero);
            }
            else if (currentSelected != null)
            {
                MoveSelectedHero(mousePos);
            }
        }
    }

    private void InitializeHeroes()
    {
        for (int i = 0; i < initialHeroes.Count; i++)
        {
            Vector3 spawnPos = (spawnPositions.Length > 0)
                ? spawnPositions[i % spawnPositions.Length].position
                : new Vector3(i * 2.0f, 0, 0); // Default spacing if no positions set
            SpawnHero(initialHeroes[i], spawnPos);
        }
    }

    private HeroBehavior SpawnHero(UnitID heroID, Vector2 position)
    {
        if (activeHeroes.TryGetValue(heroID, out HeroBehavior existingHero))
        {
            existingHero.transform.position = position;
            return existingHero;
        }

        HeroSO heroData = GameDataManager.Instance.GetHeroData(heroID);
        if (heroData == null)
        {
            return null;
        }

        HeroBehavior heroInstance = GameObjectFactory.CreateHeroGameObject(heroID, position) as HeroBehavior;
        if (heroInstance is not null)
        {
            heroInstance.Initialize(heroData);
        }
        activeHeroes.Add(heroID, heroInstance);
        return heroInstance;
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
        currentSelected = hero;
        hero.OnSelected();
    }

    private void MoveSelectedHero(Vector2 targetPosition)
    {
        if (currentSelected != null)
        {
            currentSelected.ChangeState(HeroState.MovingToFlag);
            currentSelected.SetTargetPosition(targetPosition);
            currentSelected = null;
        }
    }
}