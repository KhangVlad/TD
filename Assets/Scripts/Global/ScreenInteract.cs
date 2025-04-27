using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenInteract : MonoBehaviour
{
    #region Singleton
    public static ScreenInteract Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    #region SerializedFields
    [Header("Settings")]
    [SerializeField] private LayerMask interactableLayers;
    #endregion

    #region Public Events
    public event Action<Vector2> OnScreenClick;
    
    // Specialized click events
    public event Action<Tower> OnTowerClick;
    public event Action<BuildSpot> OnBuildSpotClick;
    public event Action<HeroBehavior> OnHeroClick;
    #endregion

    #region Private Variables
    private Vector2 _mousePos;
    private Camera _mainCamera;
    #endregion

    private void Start()
    {
        _mainCamera = Camera.main;
        if (_mainCamera == null)
        {
            Debug.LogError("Main camera not found in the scene!");
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !Utilities.IsPointerOverUI())
        {
            _mousePos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
            HandleMouseClick();
        }
    }

    private void HandleMouseClick()
    {
        // Trigger the general screen click event
        OnScreenClick?.Invoke(_mousePos);

        // Raycast to check if an object was clicked
        RaycastHit2D hit = Physics2D.Raycast(_mousePos, Vector2.zero, 100f, interactableLayers);
        
        if (hit.collider != null)
        {
            GameObject clickedObject = hit.collider.gameObject;
            if (clickedObject.TryGetComponent<Tower>(out Tower tower))
            {
                OnTowerClick?.Invoke(tower);
            }
            else if (clickedObject.TryGetComponent<BuildSpot>(out BuildSpot buildSpot))
            {
                OnBuildSpotClick?.Invoke(buildSpot);
            }
            else if (clickedObject.TryGetComponent<HeroBehavior>(out HeroBehavior hero))
            {
                OnHeroClick?.Invoke(hero);
            }
        }
    }

    #region Public Methods
    public bool IsOverObject(Vector2 screenPosition, LayerMask layerMask)
    {
        Vector2 worldPos = _mainCamera.ScreenToWorldPoint(screenPosition);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero, 100f, layerMask);
        return hit.collider != null;
    }
    #endregion
}