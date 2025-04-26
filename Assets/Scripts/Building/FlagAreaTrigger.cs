using UnityEngine;

/// <summary>
/// Handles monster detection in the flag area for Barracks towers.
/// </summary>
public class FlagAreaTrigger : MonoBehaviour
{
    private Tower _parentBarracks;
    private CircleCollider2D _collider;

    public float colliderRange => _collider.radius;

    public void Initialize(Tower parent)
    {
        _parentBarracks = parent;
        _collider = GetComponent<CircleCollider2D>();
    }

    private void Start()
    {
        // Try to find parent if not initialized yet
        if (_parentBarracks == null)
        {
            _parentBarracks = GetComponentInParent<Barracks>();
        }
        
        if (_collider == null)
        {
            _collider = GetComponent<CircleCollider2D>();
        }
    }

    /// <summary>
    /// Sets the position of the flag area.
    /// </summary>
    /// <param name="position">The new position for the flag area.</param>
    public void SetPosition(Vector2 position)
    {
        transform.position = position;
    }

    /// <summary>
    /// Sets the detection range of the flag area.
    /// </summary>
    /// <param name="range">The new radius for the detection area.</param>
    public void SetRange(float range)
    {
        if (_collider != null)
        {
            _collider.radius = range;
        }
        else
        {
            _collider = GetComponent<CircleCollider2D>();
            if (_collider != null)
            {
                _collider.radius = range;
            }
            else
            {
                Debug.LogError("FlagAreaTrigger: No collider found!");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_parentBarracks == null)
        {
            // Try one more time to get parent
            _parentBarracks = GetComponentInParent<Barracks>();
            if (_parentBarracks == null)
            {
                Debug.LogError("FlagAreaTrigger: No parent barracks found!");
                return;
            }
        }

        MonsterBase monster = other.GetComponent<MonsterBase>();
        if (monster != null)
        {
            _parentBarracks.AddMonsterToArea(monster);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (_parentBarracks == null)
        {
            return;
        }

        MonsterBase monster = other.GetComponent<MonsterBase>();
        if (monster != null)
        {
            _parentBarracks.RemoveMonsterFromArea(monster);
        }
    }
    
    protected virtual void OnDrawGizmos()
    {
       
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position,colliderRange);
    }
}