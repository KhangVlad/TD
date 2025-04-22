using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ProjectileManager : MonoBehaviour
{
    [System.Serializable]
    public class ProjectileEntry
    {
        public ProjectileID id;
        public Projectile prefab;
        public int poolSize = 30;
    }

    [SerializeField] private List<ProjectileEntry> projectileTypes = new List<ProjectileEntry>();
    [SerializeField] private Transform poolContainer; // Container for inactive projectiles

    private Dictionary<ProjectileID, ObjectPool<Projectile>> projectilePools = new();
    private Dictionary<int, ProjectileID> projectileInstanceToIDMap = new(); // Maps instance ID to projectile type
    
    private static ProjectileManager _instance;
    public static ProjectileManager Instance => _instance;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;

        // Create container for pooled projectiles if not assigned
        if (poolContainer == null)
        {
            poolContainer = transform;
        }

        // Initialize all projectile pools
        InitializePools();
    }

    private void InitializePools()
    {
        foreach (var entry in projectileTypes)
        {
            // Create a pool for each projectile type
            ObjectPool<Projectile> pool = new ObjectPool<Projectile>(
                createFunc: () => CreateProjectile(entry),
                actionOnGet: (projectile) => OnGetProjectile(projectile),
                actionOnRelease: (projectile) => OnReleaseProjectile(projectile),
                actionOnDestroy: (projectile) => OnDestroyProjectile(projectile),
                collectionCheck: false,
                defaultCapacity: entry.poolSize,
                maxSize: entry.poolSize * 2
            );

            projectilePools.Add(entry.id, pool);
        }
    }

    private Projectile CreateProjectile(ProjectileEntry entry)
    {
        Projectile projectile = Instantiate(entry.prefab, poolContainer);
        
        // Store the projectile ID for the instance
        int instanceID = projectile.GetInstanceID();
        projectileInstanceToIDMap[instanceID] = entry.id;
        
        // Set the projectile's ID property
        projectile.id = entry.id;
        
        // Disable it immediately (it will be enabled when retrieved from pool)
        projectile.gameObject.SetActive(false);
        
        return projectile;
    }

    private void OnGetProjectile(Projectile projectile)
    {
        // When getting from pool, detach from the container
        projectile.transform.SetParent(null);
        projectile.gameObject.SetActive(true);
    }

    private void OnReleaseProjectile(Projectile projectile)
    {
        // When releasing to pool, make it a child of the container
        projectile.gameObject.SetActive(false);
        projectile.transform.SetParent(poolContainer);
        
        // Reset transform values
        projectile.transform.localPosition = Vector3.zero;
        projectile.transform.localRotation = Quaternion.identity;
    }

    private void OnDestroyProjectile(Projectile projectile)
    {
        // Remove from the instance ID map if it exists
        int instanceID = projectile.GetInstanceID();
        if (projectileInstanceToIDMap.ContainsKey(instanceID))
        {
            projectileInstanceToIDMap.Remove(instanceID);
        }
        
        Destroy(projectile.gameObject);
    }

    // Fire a projectile from the pool
    public Projectile FireProjectile(ProjectileID id, Vector3 position, Transform target, float damage, float speed,
        float lifetime)
    {
        if (!projectilePools.TryGetValue(id, out ObjectPool<Projectile> pool))
        {
            Debug.LogError($"No pool found for projectile ID: {id}");
            return null;
        }

        Projectile projectile = pool.Get();

        if (projectile != null)
        {
            projectile.transform.position = position;
            projectile.Initialize(target, damage, speed, lifetime);
        }

        return projectile;
    }

    // Return a projectile to the pool
    public void ReturnProjectile(Projectile projectile)
    {
        if (projectile == null)
        {
            return;
        }
        
        int instanceID = projectile.GetInstanceID();
        
        // Try to get the projectile ID from the instance map
        if (!projectileInstanceToIDMap.TryGetValue(instanceID, out ProjectileID id))
        {
            // If not found in map, try to use the ID from the projectile itself
            id = projectile.id;
            
            // If still no ID, destroy the projectile
            if (id == ProjectileID.None)
            {
                Debug.LogWarning("Projectile has no ID and is not in the instance map. Destroying it.");
                Destroy(projectile.gameObject);
                return;
            }
        }

        // Return to the correct pool
        if (!projectilePools.TryGetValue(id, out ObjectPool<Projectile> pool))
        {
            Debug.LogWarning($"No pool found for projectile ID: {id}. Destroying it.");
            Destroy(projectile.gameObject);
            return;
        }

        pool.Release(projectile);
    }
}