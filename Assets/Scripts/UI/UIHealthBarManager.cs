using System;
using System.Collections.Generic;
using UnityEngine;

public class UIHealthBarManager : MonoBehaviour
{
    [SerializeField] private UIHealthBar healthBarPrefab;
    [SerializeField] private Transform healthBarContainer;
    [SerializeField] private Camera uiCamera; // Only needed for Screen Space - Camera
    [SerializeField] private Vector2 healthBarOffset = new Vector2(0, 30f); // Default offset in screen space
    
    private static UIHealthBarManager _instance;
    private Canvas _canvas;
    private RectTransform _canvasRectTransform;
    private ObjectPool<UIHealthBar> _healthBarPool;
    private List<HealthBarData> _activeHealthBars = new List<HealthBarData>();
    private Camera _mainCamera;
    
    public static UIHealthBarManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("UIHealthBarManager instance not found!");
            }
            return _instance;
        }
        private set { _instance = value; }
    }
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        _canvas = GetComponent<Canvas>();
        _canvasRectTransform = _canvas.GetComponent<RectTransform>();
        _mainCamera = Camera.main;
        
        // If no UI camera set and using Screen Space - Camera, get the canvas's worldCamera
        if (uiCamera == null && _canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            uiCamera = _canvas.worldCamera;
        }
        
        // Create container if not assigned
        if (healthBarContainer == null)
        {
            healthBarContainer = new GameObject("HealthBarContainer").transform;
            healthBarContainer.SetParent(_canvasRectTransform);
            healthBarContainer.localPosition = Vector3.zero;
        }
        
        // Initialize the health bar pool
        _healthBarPool = new ObjectPool<UIHealthBar>(
            createFunc: () => CreateHealthBarInstance(),
            actionOnGet: (bar) => bar.gameObject.SetActive(true),
            actionOnRelease: (bar) => bar.gameObject.SetActive(false),
            actionOnDestroy: (bar) => Destroy(bar.gameObject),
            defaultCapacity: 20
        );
    }
    
    private void LateUpdate()
    {
        UpdateAllHealthBarPositions();
    }
    
    /// <summary>
    /// Updates the position of all active health bars
    /// </summary>
    private void UpdateAllHealthBarPositions()
    {
        // Clean up destroyed targets
        for (int i = _activeHealthBars.Count - 1; i >= 0; i--)
        {
            if (_activeHealthBars[i].Target == null)
            {
                ReleaseHealthBar(_activeHealthBars[i].HealthBar);
                _activeHealthBars.RemoveAt(i);
                continue;
            }
            
            // Update position
            UpdateHealthBarPosition(_activeHealthBars[i]);
        }
    }
    
    /// <summary>
    /// Updates the position of a single health bar
    /// </summary>
    private void UpdateHealthBarPosition(HealthBarData data)
    {
        if (data.Target == null || data.HealthBar == null)
            return;
            
        // Get screen point from world position
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(_mainCamera, data.Target.position);
        
        // Apply offset in screen space
        screenPoint += data.Offset;
        
        // Convert to canvas local point
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRectTransform,
            screenPoint,
            _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : uiCamera,
            out Vector2 localPoint))
        {
            // Set the health bar position
            data.HealthBar.transform.localPosition = localPoint;
        }
        else
        {
            Debug.LogWarning("Failed to convert screen point to local point for health bar.");
        }
    }
    
    /// <summary>
    /// Creates a health bar and links it to a target
    /// </summary>
    public UIHealthBar CreateHealthBarForTarget(Transform target)
    {
        if (target == null)
            return null;
        UIHealthBar healthBar = _healthBarPool.Get();
        healthBar.ResetFill();
        Vector2 offset = healthBarOffset;
        var healthBarData = new HealthBarData(target, healthBar, offset);
        _activeHealthBars.Add(healthBarData);
        UpdateHealthBarPosition(healthBarData);
        
        return healthBar;
    }
    
    /// <summary>
    /// Releases a health bar back to the pool
    /// </summary>
    public void ReleaseHealthBar(UIHealthBar healthBar)
    {
        if (healthBar == null)
            return;
            
        // Remove from active list
        for (int i = _activeHealthBars.Count - 1; i >= 0; i--)
        {
            if (_activeHealthBars[i].HealthBar == healthBar)
            {
                _activeHealthBars.RemoveAt(i);
                break;
            }
        }
        
        // Return to pool
        _healthBarPool.Release(healthBar);
    }
    

    
    /// <summary>
    /// Updates health fill amount for a target
    /// </summary>
    public void UpdateTargetHealthFill(Transform target, float fillAmount)
    {
        if (target == null)
            return;
            
        foreach (var data in _activeHealthBars)
        {
            if (data.Target == target)
            {
                data.HealthBar.UpdateFillAmount(fillAmount);
                return;
            }
        }
    }
    
    private UIHealthBar CreateHealthBarInstance()
    {
        UIHealthBar instance = Instantiate(healthBarPrefab, healthBarContainer);
        instance.transform.localScale = Vector3.one;
        return instance;
    }
    
    // Internal data class for tracking health bars
    [Serializable]
    private class HealthBarData
    {
        public Transform Target { get; private set; }
        public UIHealthBar HealthBar { get; private set; }
        public Vector2 Offset { get; set; }
        
        public HealthBarData(Transform target, UIHealthBar healthBar, Vector2 offset)
        {
            Target = target;
            HealthBar = healthBar;
            Offset = offset;
        }
    }
    
    public class ObjectPool<T> where T : Component
    {
      
        private readonly System.Func<T> _createFunc;
        private readonly System.Action<T> _actionOnGet;
        private readonly System.Action<T> _actionOnRelease;
        private readonly System.Action<T> _actionOnDestroy;
        private readonly Stack<T> _pool;
        
        public ObjectPool(System.Func<T> createFunc, 
                        System.Action<T> actionOnGet, 
                        System.Action<T> actionOnRelease, 
                        System.Action<T> actionOnDestroy,
                        int defaultCapacity = 10)
        {
            _createFunc = createFunc;
            _actionOnGet = actionOnGet;
            _actionOnRelease = actionOnRelease;
            _actionOnDestroy = actionOnDestroy;
            _pool = new Stack<T>(defaultCapacity);
        }
        
        public T Get()
        {
            T element = _pool.Count > 0 ? _pool.Pop() : _createFunc();
            _actionOnGet?.Invoke(element);
            return element;
        }
        
        public void Release(T element)
        {
            if (element == null) return;
            
            _actionOnRelease?.Invoke(element);
            _pool.Push(element);
        }
        
        public void Clear()
        {
            if (_actionOnDestroy != null)
            {
                while (_pool.Count > 0)
                {
                    T element = _pool.Pop();
                    _actionOnDestroy(element);
                }
            }
            else
            {
                _pool.Clear();
            }
        }
    }
}