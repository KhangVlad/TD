using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

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
    [SerializeField] private GameObject flagPrefab;
    
    [SerializeField] private LayerMask buildingLayer;
    private GameObject _flagInstance;
    public bool IsActiveFlag = false;
    public List<UIBuidingSlot> _buildingSlotPool = new List<UIBuidingSlot>();
    private Vector2 _mousePos;
    private BuildSpot _currentSelected;
    private BaseTower _currentTower;
    private Coroutine _flagCoroutine;

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
        if (_currentTower is not null && _currentTower is Barracks barracks)
        {
            barracks.OnFlagPlaced(_mousePos);
            StartCoroutine(FlagAppear());
            IsActiveFlag = false;
        }
    }

    private IEnumerator FlagAppear()
    {
        if (_flagCoroutine != null)
        {
            StopCoroutine(_flagCoroutine);
            //
        }

        float durationAppear = 0.5f;

        if (_flagInstance == null)
        {
            _flagInstance = Instantiate(flagPrefab, _mousePos, Quaternion.identity);
        }
        else
        {
            _flagInstance.SetActive(true);
        }

        _flagInstance.transform.position = _mousePos;

        // Ensure the flag has a SpriteRenderer
        SpriteRenderer spriteRenderer = _flagInstance.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            // Set initial alpha to 0
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);
            yield return spriteRenderer.DOFade(1f, durationAppear).WaitForCompletion();
            yield return new WaitForSeconds(durationAppear);
            yield return spriteRenderer.DOFade(0f, durationAppear).WaitForCompletion();
        }

        _flagInstance.SetActive(false);
    }

    private void HandleMouseClick()
    {
        RaycastHit2D hit = Physics2D.Raycast(_mousePos, Vector2.zero, 100f, buildingLayer);
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
            UIBuidingSlot slot = Instantiate(_uiBuidingSlotPrefab, _uiBuildingSlotParent);
            slot.transform.SetParent(_uiBuildingSlotParent, false);
            slot.Initialize(nextLevelTowers[i]);

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

        AppearCircle(_currentTower.transform.position);
    }

    private void CreateBuilding(TowerSO towerData)
    {
        if (_currentSelected == null || towerData == null || towerData.prefab == null)
        {
            Debug.LogError("Cannot create tower: missing build spot or tower data");
            return;
        }

        if (!GameDataManager.Instance.CanAfford(towerData.buildCost))
        {
            Debug.Log("Not enough resources to build " + towerData.towerName);
            return;
        }

        GameObject towerObj = Instantiate(towerData.prefab, _currentSelected.transform.position, Quaternion.identity);

        BaseTower tower = towerObj.GetComponent<BaseTower>();
        if (tower != null)
        {
            tower.dataSO = towerData;
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


    private void HideAllSlots()
    {
        foreach (Transform child in _uiBuildingSlotParent)
        {
            Destroy(child.gameObject);
        }
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
            UIBuidingSlot slot = Instantiate(_uiBuidingSlotPrefab, _uiBuildingSlotParent);
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