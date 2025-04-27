using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingManager : MonoBehaviour
{
    #region Singleton
    public static BuildingManager Instance { get; private set; }

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
    #endregion

    #region SerializedFields
    [Header("UI References")]
    [SerializeField] private RectTransform uiCircleSelection;
    [SerializeField] private RectTransform hoverPointer;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Animator circleSelectionAnimator;
    [SerializeField] private UIBuidingSlot _uiBuidingSlotPrefab;
    [SerializeField] private Transform _uiBuildingSlotParent;
    [SerializeField] private Button flagBtn;
  
    [SerializeField] private SpriteRenderer map;
    
    // [Header("Settings")]
    // [SerializeField] private LayerMask buildingLayer;
    [SerializeField] private float selectionRadius = 100f;
    [SerializeField] private float flagAnimationDuration = 0.5f;
    #endregion
 
    #region Private Variables
    private Vector2 _currentMousePos;
    private BuildSpot _currentSelectedSpot;
    private Tower _currentSelectedTower;
    private Coroutine _flagCoroutine;
    #endregion

    #region Public Properties
    [SerializeField] public bool IsPlacingFlag;
    #endregion

    private void Start()
    {
        InitializeUI();
        SubscribeToEvents();
    }

    private void OnDestroy()
    {
        CleanupEventListeners();
        UnsubscribeFromEvents();
    }

    #region Event Subscriptions
    private void SubscribeToEvents()
    {
        if (ScreenInteract.Instance == null)
        {
            Debug.LogError("ScreenInteract instance is null. Make sure it's initialized before BuildingManager.");
            return;
        }

        // Subscribe to general events
        ScreenInteract.Instance.OnScreenClick += OnScreenClick;
        
        // Subscribe to specialized events
        ScreenInteract.Instance.OnTowerClick += OnTowerClick;
        ScreenInteract.Instance.OnBuildSpotClick += OnBuildSpotClick;
    }

    private void UnsubscribeFromEvents()
    {
        if (ScreenInteract.Instance != null)
        {
            ScreenInteract.Instance.OnScreenClick -= OnScreenClick;
            ScreenInteract.Instance.OnTowerClick -= OnTowerClick;
            ScreenInteract.Instance.OnBuildSpotClick -= OnBuildSpotClick;
        }
    }

    private void OnScreenClick(Vector2 mousePos)
    {
        _currentMousePos = mousePos;
        
        if (IsPlacingFlag)
        {
            PlaceFlag();
        }
    }

    private void OnTowerClick(Tower tower)
    {
        if (!IsPlacingFlag)
        {
            SelectTower(tower);
        }
    }

    private void OnBuildSpotClick(BuildSpot buildSpot)
    {
        if (!IsPlacingFlag)
        {
            SelectBuildingSpot(buildSpot);
        }
    }

    private void OnEmptyClick()
    {
        if (!IsPlacingFlag)
        {
            ClearSelection();
        }
    }
    #endregion

    #region Initialization Methods
    private void InitializeUI()
    {
        if (flagBtn != null)
        {
            flagBtn.onClick.AddListener(OnFlagButtonClicked);
        }
        else
        {
            Debug.LogError("Flag button reference is missing in BuildingManager.");
        }

        // Ensure UI elements are in the correct initial state
        if (uiCircleSelection != null)
        {
            uiCircleSelection.gameObject.SetActive(false);
        }
        
        if (hoverPointer != null)
        {
            hoverPointer.gameObject.SetActive(false);
        }
    }

    private void CleanupEventListeners()
    {
        if (flagBtn != null)
        {
            flagBtn.onClick.RemoveAllListeners();
        }
    }
    #endregion

    #region Flag Placement
    private void OnFlagButtonClicked()
    {
        if (_currentSelectedTower != null && _currentSelectedTower is Barracks)
        {
            IsPlacingFlag = true;
            HideSelectionUI();
        }
    }

    private void PlaceFlag()
    {
        if (_currentSelectedTower is Barracks barracks)
        {
            barracks.PutFlag(_currentMousePos);
            IsPlacingFlag = false;
        }
    }
    #endregion

    #region Selection Handling
    private void ClearSelection()
    {
        HideSelectionUI();
        _currentSelectedSpot = null;
        _currentSelectedTower = null;
    }

    private void SelectBuildingSpot(BuildSpot buildingSpot)
    {
        _currentSelectedSpot = buildingSpot;
        _currentSelectedTower = null;
        ShowSelectionCircleAt(buildingSpot.transform.position, false);
        ShowBaseTowerOptions();
    }

    private void SelectTower(Tower tower)
    {
        _currentSelectedTower = tower;
        _currentSelectedSpot = null;
        ShowSelectionCircleAt(tower.transform.position, tower is Barracks);
        ShowUpgradeOptions(tower.dataSO);
    }
    #endregion

    #region UI Visualization
    private void ShowSelectionCircleAt(Vector2 worldPosition, bool isBarrack)
    {
        if (isBarrack)
        {
            flagBtn.gameObject.SetActive(true);
        }
        else
        {
            flagBtn.gameObject.SetActive(false);
        }
        Vector2 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            screenPosition,
            canvas.worldCamera,
            out Vector2 localPosition))
        {
            uiCircleSelection.gameObject.SetActive(true);
            uiCircleSelection.localPosition = localPosition;
            
            if (circleSelectionAnimator != null)
            {
                circleSelectionAnimator.SetTrigger("Appear");
            }
        }
    }

    private void HideSelectionUI()
    {
        if (uiCircleSelection != null)
        {
            uiCircleSelection.gameObject.SetActive(false);
        }
        
        if (hoverPointer != null)
        {
            hoverPointer.gameObject.SetActive(false);
        }
    }
    #endregion

    #region Tower Building and Upgrading
    private void ShowBaseTowerOptions()
    {
        ClearBuildingSlots();
        
        if (GameDataManager.Instance == null)
        {
            Debug.LogError("GameDataManager instance is null. Cannot get base towers.");
            return;
        }

        List<TowerSO> baseTowers = GameDataManager.Instance.GetAllBaseTowers();
        if (baseTowers == null || baseTowers.Count == 0)
        {
            Debug.LogWarning("No base towers available for building.");
            return;
        }

        CreateBuildingSlots(baseTowers, (data) => {
            if (_currentSelectedSpot != null)
            {
                BuildTower(data, _currentSelectedSpot.transform.position, _currentSelectedSpot.gameObject);
            }
        });
    }

    private void ShowUpgradeOptions(TowerSO currentTowerData)
    {
        ClearBuildingSlots();
        
        if (currentTowerData == null || GameDataManager.Instance == null)
        {
            Debug.LogError("Cannot show upgrade options: Tower data or GameDataManager is null.");
            return;
        }

        List<TowerSO> upgrades = GameDataManager.Instance.GetPossibleUpgrade(currentTowerData);
        if (upgrades == null || upgrades.Count == 0)
        {
            Debug.Log($"No upgrades available for {currentTowerData.towerName}.");
            return;
        }

        CreateBuildingSlots(upgrades, (data) => {
            if (_currentSelectedTower != null)
            {
                BuildTower(data, _currentSelectedTower.transform.position, _currentSelectedTower.gameObject);
            }
        });
    }

    private void CreateBuildingSlots(List<TowerSO> towerOptions, System.Action<TowerSO> onUpgradeAction)
    {
        if (_uiBuidingSlotPrefab == null || _uiBuildingSlotParent == null)
        {
            Debug.LogError("UI Building Slot prefab or parent is missing.");
            return;
        }

        float angleStep = 360f / towerOptions.Count;

        for (int i = 0; i < towerOptions.Count; i++)
        {
            TowerSO towerSO = towerOptions[i];
            UIBuidingSlot slot = Instantiate(_uiBuidingSlotPrefab, _uiBuildingSlotParent);
            slot.transform.SetParent(_uiBuildingSlotParent, false);
            slot.Initialize(towerSO);

            slot.OnUpgrade += (data) =>
            {
                onUpgradeAction?.Invoke(data);
                HideSelectionUI();
            };

            float angle = i * angleStep * Mathf.Deg2Rad;
            float x = Mathf.Cos(angle) * selectionRadius;
            float y = Mathf.Sin(angle) * selectionRadius;
            slot.transform.localPosition = new Vector3(x, y, 0);
        }
    }

    private void BuildTower(TowerSO towerData, Vector2 position, GameObject objectToReplace)
    {
        if (towerData == null || towerData.prefab == null)
        {
            Debug.LogError("Cannot build tower: tower data or prefab is null.");
            return;
        }

        if (GameDataManager.Instance == null || !GameDataManager.Instance.CanAfford(towerData.buildCost))
        {
            Debug.Log($"Not enough resources to build {towerData.towerName}.");
            return;
        }

        GameObject towerObj = Instantiate(towerData.prefab, position, Quaternion.identity);
        Tower tower = towerObj.GetComponent<Tower>();
        
        if (tower != null)
        {
            tower.dataSO = towerData;

            // Initialize specific tower types
            if (tower is Barracks barracks)
            {
                barracks.InitializeTower(towerData);
            }

            // Handle economy
            
            // Clean up the replaced object
            if (objectToReplace != null)
            {
                Destroy(objectToReplace);
            }
        }
        else
        {
            Debug.LogError($"Failed to create tower: BaseTower component is missing on {towerData.towerName} prefab.");
            Destroy(towerObj);
        }
    }

    private void ClearBuildingSlots()
    {
        if (_uiBuildingSlotParent == null)
        {
            return;
        }

        foreach (Transform child in _uiBuildingSlotParent)
        {
            Destroy(child.gameObject);
        }
    }
    #endregion

    #region Public Methods
    public void CancelFlagPlacement()
    {
        IsPlacingFlag = false;
    }
    #endregion
}