using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Transform target;
    private float damage;
    private float speed;
    private float lifetime;
    
    // Bezier curve height
    private float arcHeight = 1.5f;
    
    // Reference to the coroutine
    private Coroutine flightCoroutine;

    public void Initialize(Transform target, float damage, float speed, float lifetime)
    {
        this.target = target;
        this.damage = damage;
        this.speed = speed;
        this.lifetime = lifetime;
        
        // Start flight coroutine
        if (target != null)
        {
            flightCoroutine = StartCoroutine(BezierFlight());
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // private IEnumerator BezierFlight()
    // {
    //     Vector3 startPos = transform.position;
    //     float startTime = Time.time;
    //     
    //     // Continue until we hit the target or exceed lifetime
    //     while (Time.time - startTime < lifetime && target != null)
    //     {
    //         // Calculate how far along the path we are (0 to 1)
    //         float distanceToTarget = Vector3.Distance(startPos, target.position);
    //         float journeyLength = distanceToTarget;
    //         float speedFactor = speed / journeyLength;
    //         
    //         // Calculate progress based on time and speed
    //         float timeProgress = (Time.time - startTime) * speedFactor;
    //         
    //         if (timeProgress >= 1.0f)
    //         {
    //             // We've reached the target
    //             HitTarget();
    //             yield break;
    //         }
    //         
    //         // Generate a midpoint for the bezier curve
    //         Vector3 midPoint = Vector3.Lerp(startPos, target.position, 0.5f);
    //         midPoint.y += arcHeight; // Add height for the arc
    //         
    //         // Calculate position along the Bezier curve
    //         Vector3 newPosition = QuadraticBezier(startPos, midPoint, target.position, timeProgress);
    //         transform.position = newPosition;
    //         
    //         // Calculate direction for rotation
    //         if (timeProgress < 0.95f) // Don't change rotation right at the end
    //         {
    //             Vector3 nextPosition = QuadraticBezier(startPos, midPoint, target.position, timeProgress + 0.05f);
    //             Vector3 direction = (nextPosition - newPosition).normalized;
    //             
    //             float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    //             transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    //         }
    //         
    //         yield return null;
    //     }
    //     
    //     // If we've reached here, either target was destroyed or lifetime expired
    //     Destroy(gameObject);
    // }
    private IEnumerator BezierFlight()
    {
        Vector3 startPos = transform.position;
        float startTime = Time.time;
        float fixedDuration = 0.3f; // Fixed duration of 1 second
    
        // Calculate the initial distance to determine arc height
        float distanceToTarget = Vector3.Distance(startPos, target.position);
        float dynamicArcHeight = arcHeight * (distanceToTarget / 10f); // Scale arc height based on distance
    
        // Continue until we hit the target or exceed lifetime
        while (Time.time - startTime < Mathf.Min(fixedDuration, lifetime) && target != null)
        {
            // Calculate progress based on fixed duration
            float timeProgress = (Time.time - startTime) / fixedDuration;
        
            if (timeProgress >= 1.0f)
            {
                // We've reached the target
                HitTarget();
                yield break;
            }
        
            // Generate a midpoint for the bezier curve
            Vector3 midPoint = Vector3.Lerp(startPos, target.position, 0.5f);
            midPoint.y += dynamicArcHeight; // Add height for the arc, scaled by distance
        
            // Calculate position along the Bezier curve
            Vector3 newPosition = QuadraticBezier(startPos, midPoint, target.position, timeProgress);
            transform.position = newPosition;
        
            // Calculate direction for rotation
            if (timeProgress < 0.95f) // Don't change rotation right at the end
            {
                Vector3 nextPosition = QuadraticBezier(startPos, midPoint, target.position, timeProgress + 0.05f);
                Vector3 direction = (nextPosition - newPosition).normalized;
            
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }
        
            yield return null;
        }
    
        // If we've reached here, either target was destroyed or lifetime expired
        Destroy(gameObject);
    }
    
    // Quadratic Bezier curve formula
    private Vector3 QuadraticBezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        
        Vector3 point = uu * p0; // (1-t)² * P0
        point += 2 * u * t * p1; // 2(1-t)t * P1
        point += tt * p2; // t² * P2
        
        return point;
    }
    
    private void HitTarget()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }
        
        // Apply damage to monster
        Monster monster = target.GetComponent<Monster>();
        if (monster != null)
        {
            monster.TakeDamage(damage);
        }
        
        // Destroy arrow after hit
        Destroy(gameObject);
    }
    
    private void OnDestroy()
    {
        // Make sure to stop the coroutine if the object is destroyed
        if (flightCoroutine != null)
        {
            StopCoroutine(flightCoroutine);
        }
    }
    
    // Optional: Adjust arc height
    public void SetArcHeight(float height)
    {
        arcHeight = height;
    }
}