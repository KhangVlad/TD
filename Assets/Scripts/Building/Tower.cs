using System;
using UnityEngine;
using System.Collections;
using DG.Tweening;
using System.Collections.Generic;
public interface ITower
{

}
public abstract class Tower : MonoBehaviour, ITower
{
    public TowerSO dataSO;
    public Flag flagInstance;
    
    [Header("Flag Settings")]
    [SerializeField] protected float flagDetectRange = 1f;
    
    public List<Monster> _monstersInArea = new List<Monster>();
    protected Vector2 flagPosition;
    protected Coroutine flagCoroutine;
    protected FlagAreaTrigger _flagAreaCollider;
    
    // Event for notifying flag position changes
    public event Action<Vector2> OnFlagPositionChanged;

    protected virtual void Update()
    {
        // Base update logic
    }
    
    protected virtual void Start()
    {
        LoadFlag();
        UpdateFlagColliderPosition();
    }

    private void LoadFlag()
    {
        if (dataSO != null)
        {
            // Load flag prefab
            GameObject flagPrefab = Resources.Load<GameObject>("flag");
            if (flagPrefab != null)
            {
                // Create flag instance
                GameObject flagObject = Instantiate(flagPrefab, transform.position, Quaternion.identity);
                flagInstance = flagObject.GetComponent<Flag>();
                flagObject.SetActive(false); 
                
                // Create flag area collider
                InitializeFlagCollider();
            }
            else
            {
                Debug.LogError("Failed to load flag prefab from Resources/flag");
            }
        }
    }

    protected void SetDataSO(TowerSO so)
    {
        this.dataSO = so;
    }
    
    protected void InitializeFlagCollider()
    {
        if (_flagAreaCollider != null)
        {
            return;
        }

        var a = new GameObject("FlagAreaCollider");
        a.transform.parent = transform;
        a.transform.position = flagPosition;

        CircleCollider2D collider = a.AddComponent<CircleCollider2D>();
        collider.radius = flagDetectRange;
        collider.isTrigger = true;

        Rigidbody2D rb = a.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.simulated = true;
        
        FlagAreaTrigger trigger = a.AddComponent<FlagAreaTrigger>();
        _flagAreaCollider = trigger;
        _flagAreaCollider.Initialize(this);
    }
    
    public virtual void UpdateFlagColliderPosition()
    {
        if (_flagAreaCollider != null)
        {
            _flagAreaCollider.transform.position = flagPosition;
        }
    }
    
    private void ChangeFlagPosition(Vector2 newPosition)
    {
        flagPosition = newPosition;
        UpdateFlagColliderPosition();
        OnFlagPositionChanged?.Invoke(newPosition);
    }
    
    // Method that can be called when flag is placed
    public virtual void OnFlagPlaced(Vector2 position)
    {
        ChangeFlagPosition(position);
        
        if (flagInstance == null)
        {
            LoadFlag();
        }
        
        if (flagInstance != null)
        {
            flagInstance.SetPosition(position);
            
            if (flagCoroutine != null)
            {
                StopCoroutine(flagCoroutine);
            }
            flagCoroutine = StartCoroutine(flagInstance.AnimateAppearance());
        }
    }
    
    // protected virtual void OnDrawGizmos()
    // {
    //     if (flagPosition != Vector2.zero)
    //     {
    //         Gizmos.color = Color.green;
    //         Gizmos.DrawWireSphere(flagPosition, flagDetectRange);
    //     }
    //     
    //     // Draw activity range
    //     Gizmos.color = Color.yellow;
    //     Gizmos.DrawWireSphere(transform.position, _flagAreaCollider.colliderRange);
    // }
    
    public void AddMonsterToArea(Monster monster)
    {
        if (monster != null && !_monstersInArea.Contains(monster))
        {
            Debug.Log("New monster entered the area");
            _monstersInArea.Add(monster);
            OnMonsterEnterArea(monster);
        }
    }
    
    public void RemoveMonsterFromArea(Monster monster)
    {
        if (monster != null)
        {
            _monstersInArea.Remove(monster);
            OnMonsterExitArea(monster);
        }
    }
    protected virtual void OnMonsterEnterArea(Monster monster)
    {
       
    }

    protected virtual void OnMonsterExitArea(Monster monster)
    {
        _monstersInArea.Remove(monster);
    }

    protected void SetFlagColliderRandAndPos(Vector2 pos, float range)
    {
        _flagAreaCollider.SetPosition(pos);
        _flagAreaCollider.SetRange((range));
    }
}


