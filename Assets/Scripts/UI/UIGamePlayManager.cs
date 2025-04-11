using System;
using UnityEngine;
using System.Collections.Generic;

public class UIGamePlayManager : MonoBehaviour
{
    public static UIGamePlayManager Instance { get; private set; }
    public RectTransform uiCircleSelection;
    public Canvas canvas; // Reference to the canvas
    public Animator circleSelectionAnimator;

    [SerializeField] private UIBuidingSlot _uiBuidingSlotPrefab;
    [SerializeField] private Transform _uiBuildingSlotParent;
    private BuildingSpot currentSelectedSlot;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !Utilities.IsPointerOverUI())
        {
            HandleMouseClick();
        }
    }

    private void HandleMouseClick()
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        if (hit.collider == null)
        {
            uiCircleSelection?.gameObject.SetActive(false);
            return;
        }

        if (hit.collider.TryGetComponent<BuildingSpot>(out BuildingSpot buildingSpot))
        {
            OnBuildingSpotClicked(buildingSpot);
        }
    }

    private void OnBuildingSpotClicked(BuildingSpot buildingSpot)
    {
        if( buildingSpot.tower == null)
        {
            currentSelectedSlot = buildingSpot;
            uiCircleSelection.gameObject.SetActive(true);
            Vector2 screenPosition = Camera.main.WorldToScreenPoint(buildingSpot.transform.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                screenPosition,
                canvas.worldCamera,
                out Vector2 localPosition
            );
            uiCircleSelection.localPosition = localPosition;
            circleSelectionAnimator.SetTrigger("Appear");
            InitializeAllBaseTowers();
        }
        // if (buildingSpot.tower == null || uiCircleSelection == null || canvas == null ||
        //     circleSelectionAnimator == null)
        // {
        //     return;
        // }
        //
        // currentSelectedSlot = buildingSpot;
        // uiCircleSelection.gameObject.SetActive(true);
        // Vector2 screenPosition = Camera.main.WorldToScreenPoint(buildingSpot.transform.position);
        // RectTransformUtility.ScreenPointToLocalPointInRectangle(
        //     canvas.transform as RectTransform,
        //     screenPosition,
        //     canvas.worldCamera,
        //     out Vector2 localPosition
        // );
        // uiCircleSelection.localPosition = localPosition;
        // circleSelectionAnimator.SetTrigger("Appear");
        // InitializeUpgradableSlots(buildingSpot.tower.dataSO);
    }

    private void InitializeUpgradableSlots(TowerSO so)
    {
        foreach (Transform child in _uiBuildingSlotParent)
        {
            Destroy(child.gameObject);
        }

        List<TowerSO> nextLevelTowers = GameDataManager.Instance.GetNextLevelTowers(so.type, so.level);
        int count = nextLevelTowers.Count;
        if (count == 0) return;
        float radius = 100f;
        float angleStep = 360f / count;
        for (int i = 0; i < count; i++)
        {
            UIBuidingSlot uiBuidingSlot = Instantiate(_uiBuidingSlotPrefab, _uiBuildingSlotParent);
            uiBuidingSlot.Initialize(nextLevelTowers[i]);
            uiBuidingSlot.OnUpgrade += (data) =>
            {
                currentSelectedSlot.SetNewTower(data);
                uiCircleSelection.gameObject.SetActive(false);
            };
            float angle = i * angleStep * Mathf.Deg2Rad;
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;
            uiBuidingSlot.transform.localPosition = new Vector3(x, y, 0);
        }
    }
    
    
    private void InitializeAllBaseTowers()
    {
        foreach (Transform child in _uiBuildingSlotParent)
        {
            Destroy(child.gameObject);
        }
        List<TowerSO> baseTowers = GameDataManager.Instance.GetAllBaseTowers();
        float radius = 100f;
        float angleStep = 360f / baseTowers.Count;
        foreach (TowerSO towerSO in baseTowers)
        {
            UIBuidingSlot uiBuidingSlot = Instantiate(_uiBuidingSlotPrefab, _uiBuildingSlotParent);
            uiBuidingSlot.Initialize(towerSO);
            uiBuidingSlot.OnUpgrade += (data) =>
            {
                currentSelectedSlot.SetNewTower(data);
                uiCircleSelection.gameObject.SetActive(false);
            };
            float angle = baseTowers.IndexOf(towerSO) * angleStep * Mathf.Deg2Rad;
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;
            uiBuidingSlot.transform.localPosition = new Vector3(x, y, 0);
        }
    }
}