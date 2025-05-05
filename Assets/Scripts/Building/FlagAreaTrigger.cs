using UnityEngine;

/// <summary>
/// Handles entity detection in a circular area for any game object that needs it.
/// </summary>
public class FlagAreaTrigger : MonoBehaviour
{
    // Interface to allow any object to receive trigger notifications
    public interface IFlagAreaListener
    {
        void AddMonsterToArea(MonsterBase monster);
        void RemoveMonsterFromArea(MonsterBase monster);
    }

    private IFlagAreaListener _listener;
    private CircleCollider2D _collider;

    public float colliderRange => _collider?.radius ?? 0f;

    public void Initialize(IFlagAreaListener listener)
    {
        _listener = listener;
        _collider = GetComponent<CircleCollider2D>();
    }

    private void Start()
    {
        // Try to find listener if not initialized yet
        if (_listener == null)
        {
            // Try to get the listener from parent
            _listener = GetComponentInParent<IFlagAreaListener>();
            
            if (_listener == null)
            {
                Debug.LogError("FlagAreaTrigger: No listener found!");
            }
        }
        
        if (_collider == null)
        {
            _collider = GetComponent<CircleCollider2D>();
        }
    }

    public void SetPosition(Vector2 position)
    {
        transform.position = position;
    }

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
        if (_listener == null)
        {
            // Try one more time to get listener
            _listener = GetComponentInParent<IFlagAreaListener>();
            if (_listener == null)
            {
                Debug.LogError("FlagAreaTrigger: No listener found!");
                return;
            }
        }

        MonsterBase monster = other.GetComponent<MonsterBase>();
        if (monster != null)
        {
            _listener.AddMonsterToArea(monster);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (_listener == null)
        {
            return;
        }

        MonsterBase monster = other.GetComponent<MonsterBase>();
        if (monster != null)
        {
            _listener.RemoveMonsterFromArea(monster);
        }
    }
    
    protected virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, colliderRange);
    }
}