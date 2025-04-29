using System;
using UnityEngine;

public class HeroesController : MonoBehaviour
{
    public static HeroesController Instance { get; private set; }
    public HeroBehavior currentSelected;
    [SerializeField] private LayerMask heroLayer;

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


    private void InitializeHero()
    {
        UnitID id = UnitID.Bolverk;
        
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
            Debug.Log($"Moving hero to: {targetPosition}");
        }
    }
}
