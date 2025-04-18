using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

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
    
    [Header("Game Objects")]
    [SerializeField] private GameObject flagPrefab;
    [SerializeField] private SpriteRenderer map;
    
    [Header("Settings")]
    [SerializeField] private LayerMask buildingLayer;
    [SerializeField] private float selectionRadius = 100f;
    [SerializeField] private float flagAnimationDuration = 0.5f;
    #endregion

    #region Private Variables
    private GameObject _flagInstance;
    private Vector2 _mousePos;
    private BuildSpot _currentSelectedSpot;
    private BaseTower _currentSelectedTower;
    private Coroutine _flagCoroutine;
    #endregion

    #region Public Properties
    public bool IsPlacingFlag { get; private set; }
    #endregion

    private void Start()
    {
        InitializeUI();
    }

    private void OnDestroy()
    {
        CleanupEventListeners();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !Utilities.IsPointerOverUI())
        {
            _mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (IsPlacingFlag)
            {
                PlaceFlag();
            }
            else
            {
                HandleMouseClick();
            }
        }
    }

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
            barracks.OnFlagPlaced(_mousePos);
            // StartCoroutine(AnimateFlagAppearance());
            IsPlacingFlag = false;
        }
    }

    private IEnumerator AnimateFlagAppearance()
    {
        if (_flagCoroutine != null)
        {
            StopCoroutine(_flagCoroutine);
        }

        if (_flagInstance == null)
        {
            _flagInstance = Instantiate(flagPrefab, _mousePos, Quaternion.identity);
        }
        else
        {
            _flagInstance.transform.position = _mousePos;
            _flagInstance.SetActive(true);
        }

        SpriteRenderer spriteRenderer = _flagInstance.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            // Fade in
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);
            yield return spriteRenderer.DOFade(1f, flagAnimationDuration).WaitForCompletion();
            
            // Hold
            yield return new WaitForSeconds(flagAnimationDuration);
            
            // Fade out
            yield return spriteRenderer.DOFade(0f, flagAnimationDuration).WaitForCompletion();
        }

        _flagInstance.SetActive(false);
    }
    #endregion

    #region Mouse Click Handling
    private void HandleMouseClick()
    {
        RaycastHit2D hit = Physics2D.Raycast(_mousePos, Vector2.zero, 100f, buildingLayer);
        
        if (hit.collider == null)
        {
            ClearSelection();
            return;
        }

        if (hit.collider.TryGetComponent<BuildSpot>(out BuildSpot buildingSpot))
        {
            SelectBuildingSpot(buildingSpot);
        }
        else if (hit.collider.TryGetComponent<BaseTower>(out BaseTower tower))
        {
            SelectTower(tower);
        }
    }

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
        ShowSelectionCircleAt(buildingSpot.transform.position);
        ShowBaseTowerOptions();
    }

    private void SelectTower(BaseTower tower)
    {
        _currentSelectedTower = tower;
        _currentSelectedSpot = null;
        ShowSelectionCircleAt(tower.transform.position);
        ShowUpgradeOptions(tower.dataSO);
    }
    #endregion

    #region UI Visualization
    private void ShowSelectionCircleAt(Vector2 worldPosition)
    {
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
        BaseTower tower = towerObj.GetComponent<BaseTower>();
        
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