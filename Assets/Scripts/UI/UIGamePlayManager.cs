using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGamePlayManager : MonoBehaviour
{
    public static UIGamePlayManager Instance { get; private set; }

    public RectTransform uiCircleSelection;
    public Canvas canvas;
    public Animator circleSelectionAnimator;
    [SerializeField] private SpriteRenderer map;
    [SerializeField] private UIBuidingSlot _uiBuidingSlotPrefab;
    [SerializeField] private Transform _uiBuildingSlotParent;
    [SerializeField] private Button flagBtn;

    public bool IsActiveFlag = false;
    public List<UIBuidingSlot> _buildingSlotPool = new List<UIBuidingSlot>();
    private Vector2 _mousePos;

    private BuildSpot _currentSelected;
    private BaseTower _currentTower;

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

    private void Start()
    {
        flagBtn.onClick.AddListener(() =>
        {
            if (_currentTower != null && _currentTower is Barracks)
            {
                IsActiveFlag = true;
                uiCircleSelection.gameObject.SetActive(false);
            }
        });
    }

    private void OnDestroy()
    {
        flagBtn.onClick.RemoveAllListeners();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !Utilities.IsPointerOverUI())
        {
            _mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (IsActiveFlag)
            {
                PlaceFlag();
            }
            else
            {
                HandleMouseClick();
            }
        }
    }

    private void PlaceFlag()
    {
        if (_currentTower != null && _currentTower is Barracks barracks)
        {
            barracks.OnFlagPlaced(_mousePos);
            IsActiveFlag = false;
        }
    }

    private void HandleMouseClick()
    {
        RaycastHit2D hit = Physics2D.Raycast(_mousePos, Vector2.zero);
        if (hit.collider == null)
        {
            uiCircleSelection?.gameObject.SetActive(false);
            _currentSelected = null;
            _currentTower = null;
            return;
        }

        if (hit.collider.TryGetComponent<BuildSpot>(out BuildSpot buildingSpot))
        {
            _currentSelected = buildingSpot;
            _currentTower = null;
            OnBuildingSpotClicked(buildingSpot);
        }

        if (hit.collider.TryGetComponent<BaseTower>(out BaseTower tower))
        {
            _currentTower = tower;
            _currentSelected = null;
            InitializeUpgradableSlots(tower.dataSO);
        }
    }

    private void OnBuildingSpotClicked(BuildSpot buildingSpot)
    {
        AppearCircle(buildingSpot.transform.position);
        InitializeAllBaseTowers();
    }

    private void AppearCircle(Vector2 p)
    {
        Vector2 screenPosition = Camera.main.WorldToScreenPoint(p);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            screenPosition,
            canvas.worldCamera,
            out Vector2 localPosition
        );
        uiCircleSelection.gameObject.SetActive(true);
        uiCircleSelection.localPosition = localPosition;
        circleSelectionAnimator.SetTrigger("Appear");
    }

    private void InitializeUpgradableSlots(TowerSO so)
    {
        HideAllSlots();
        List<TowerSO> nextLevelTowers = GameDataManager.Instance.GetPossibleUpgrade(so);
        if (nextLevelTowers.Count == 0) return;

        uiCircleSelection.gameObject.SetActive(true);
        float radius = 100f;
        float angleStep = 360f / nextLevelTowers.Count;

        for (int i = 0; i < nextLevelTowers.Count; i++)
        {
            UIBuidingSlot slot = GetBuildingSlot();
            slot.transform.SetParent(_uiBuildingSlotParent, false);
            slot.Initialize(nextLevelTowers[i]);

            slot.OnUpgrade += (data) =>
            {
                UpgradeTower(_currentTower, data);
                uiCircleSelection.gameObject.SetActive(false);
            };

            float angle = i * angleStep * Mathf.Deg2Rad;
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;
            slot.transform.localPosition = new Vector3(x, y, 0);
        }

        AppearCircle(_currentTower.transform.position);
    }

    private void CreateBuilding(TowerSO towerData)
    {
        if (_currentSelected == null || towerData == null || towerData.prefab == null)
        {
            Debug.LogError("Cannot create tower: missing build spot or tower data");
            return;
        }

        // Check if player has enough resources
        if (!GameDataManager.Instance.CanAfford(towerData.buildCost))
        {
            Debug.Log("Not enough resources to build " + towerData.towerName);
            return;
        }

        GameObject towerObj = Instantiate(towerData.prefab, _currentSelected.transform.position, Quaternion.identity);

        // Initialize the tower with the data
        BaseTower tower = towerObj.GetComponent<BaseTower>();
        if (tower != null)
        {
            tower.dataSO = towerData;

            // Special handling for Barracks tower
            if (tower is Barracks barracks)
            {
                barracks.InitializeTower(towerData);
            }

            _currentSelected.gameObject.SetActive(false);
        }
        else
        {
            Destroy(towerObj);
        }
    }

    private void UpgradeTower(BaseTower tower, TowerSO upgradeData)
    {
        if (tower == null || upgradeData == null)
        {
            Debug.LogError("Cannot upgrade tower: missing tower or upgrade data");
            return;
        }

        // Check if player has enough resources
        if (!GameDataManager.Instance.CanAfford(upgradeData.upgradeCost))
        {
            Debug.Log("Not enough resources to upgrade to " + upgradeData.towerName);
            return;
        }


        Vector3 position = tower.transform.position;

        // Destroy the old tower
        Destroy(tower.gameObject);

        // Instantiate the upgraded tower
        GameObject upgradedTowerObj = Instantiate(upgradeData.prefab, position, Quaternion.identity);

        // Initialize with the upgraded data
        BaseTower upgradedTower = upgradedTowerObj.GetComponent<BaseTower>();
        if (upgradedTower != null)
        {
            upgradedTower.dataSO = upgradeData;

            // Special handling for Barracks tower
            if (upgradedTower is Barracks barracks)
            {
                barracks.InitializeTower(upgradeData);
            }

            Debug.Log($"Upgraded to {upgradeData.towerName} tower at {position}");
        }
        else
        {
            Debug.LogError($"Upgraded tower prefab {upgradeData.prefab.name} does not have a BaseTower component");
            Destroy(upgradedTowerObj);
        }
    }

    private void HideAllSlots()
    {
        foreach (Transform child in _uiBuildingSlotParent)
        {
            child.gameObject.SetActive(false);
        }
    }

    private UIBuidingSlot GetBuildingSlot()
    {
        foreach (var slot in _buildingSlotPool)
        {
            if (!slot.gameObject.activeSelf)
            {
                slot.gameObject.SetActive(true);
                return slot;
            }
        }

        UIBuidingSlot newSlot = Instantiate(_uiBuidingSlotPrefab, _uiBuildingSlotParent);
        _buildingSlotPool.Add(newSlot);
        return newSlot;
    }

    private void InitializeAllBaseTowers()
    {
        HideAllSlots();
        List<TowerSO> baseTowers = GameDataManager.Instance.GetAllBaseTowers();
        float radius = 100f;
        float angleStep = 360f / baseTowers.Count;

        for (int i = 0; i < baseTowers.Count; i++)
        {
            TowerSO towerSO = baseTowers[i];
            UIBuidingSlot slot = GetBuildingSlot();
            slot.transform.SetParent(_uiBuildingSlotParent, false);
            slot.Initialize(towerSO);

            slot.OnUpgrade += (data) =>
            {
                CreateBuilding(data);
                uiCircleSelection.gameObject.SetActive(false);
            };

            float angle = i * angleStep * Mathf.Deg2Rad;
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;
            slot.transform.localPosition = new Vector3(x, y, 0);
        }
    }
}