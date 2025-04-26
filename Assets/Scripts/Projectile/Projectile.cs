using System.Collections;
using UnityEngine;

// Abstract base class for all projectiles
public abstract class Projectile : MonoBehaviour
{
    protected Transform target;
    protected float damage;
    protected float speed;
    protected float lifetime;
    protected Coroutine flightCoroutine;

    // Store the projectile ID for pool management
    public ProjectileID id = ProjectileID.None;

    // Common initialization method for all projectiles
    public virtual void Initialize(Transform target, float damage, float speed, float lifetime)
    {
        this.target = target;
        this.damage = damage;
        this.speed = speed;
        this.lifetime = lifetime;

        // Start flight behavior
        if (target != null)
        {
            BeginFlight();
        }
        else
        {
            ReturnToPool();
        }
    }

    // Abstract method to be implemented by derived classes
    protected abstract void BeginFlight();

    // Common method to handle hitting the target
    protected virtual void HitTarget()
    {
        if (target == null)
        {
            ReturnToPool();
            return;
        }

        // Apply damage to monster
        MonsterBase monster = target.GetComponent<MonsterBase>();
        if (monster != null)
        {
            monster.TakeDamage(damage);
        }

        // Return projectile to pool after hit
        ReturnToPool();
    }

    // Return this projectile to the pool instead of destroying it
    protected virtual void ReturnToPool()
    {
        // Stop any running coroutines
        if (flightCoroutine != null)
        {
            StopCoroutine(flightCoroutine);
            flightCoroutine = null;
        }

        ProjectileManager.Instance.ReturnProjectile(this);
    }

    protected virtual void OnDestroy()
    {
        // Make sure to stop any coroutines if the object is destroyed
        if (flightCoroutine != null)
        {
            StopCoroutine(flightCoroutine);
        }
    }
}

// Enum for projectile types
public enum ProjectileID
{
    None = -1, // Default value for unassigned projectiles
    Arrow = 0,
    MagicBullet = 1,
}