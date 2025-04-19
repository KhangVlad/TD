using UnityEngine;

public class FlagAreaTrigger : MonoBehaviour
{
    private Barracks _parentBarracks;

    public void Initialize(Barracks parent)
    {
        _parentBarracks = parent;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_parentBarracks == null)
        {
            // Try to get parent if not initialized properly
            _parentBarracks = GetComponentInParent<Barracks>();
            if (_parentBarracks == null)
            {
                Debug.LogError("FlagAreaTrigger: No parent barracks found!");
                return;
            }
        }

        Monster monster = other.GetComponent<Monster>();
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

        Monster monster = other.GetComponent<Monster>();
        if (monster != null)
        {
            _parentBarracks.RemoveMonsterFromArea(monster);
        }
    }
}