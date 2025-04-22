// using System.Collections;
// using UnityEngine;
//
// // Arrow projectile that uses a bezier flight path
// public class Arrow : Projectile
// {
//     private float arcHeight = 3f;
//     [SerializeField] private GameObject shadowSprite;
//     
//     protected override void BeginFlight()
//     {
//         flightCoroutine = StartCoroutine(BezierFlight());
//     }
//     
//     private IEnumerator BezierFlight()
//     {
//         Vector3 startPos = transform.position;
//         float startTime = Time.time;
//         float fixedDuration = 0.3f;
//     
//         float distanceToTarget = Vector3.Distance(startPos, target.position);
//         float dynamicArcHeight = arcHeight * (distanceToTarget / 10f); // Scale arc height based on distance
//     
//         while (Time.time - startTime < Mathf.Min(fixedDuration, lifetime) && target != null)
//         {
//             // Calculate progress based on fixed duration
//             float timeProgress = (Time.time - startTime) / fixedDuration;
//         
//             if (timeProgress >= 1.0f)
//             {
//                 // We've reached the target
//                 HitTarget();
//                 yield break;
//             }
//         
//             // Generate a midpoint for the bezier curve
//             Vector3 midPoint = Vector3.Lerp(startPos, target.position, 0.5f);
//             midPoint.y += dynamicArcHeight; // Add height for the arc, scaled by distance
//         
//             // Calculate position along the Bezier curve
//             Vector3 newPosition = QuadraticBezier(startPos, midPoint, target.position, timeProgress);
//             transform.position = newPosition;
//         
//             // Calculate direction for rotation
//             if (timeProgress < 0.95f) // Don't change rotation right at the end
//             {
//                 Vector3 nextPosition = QuadraticBezier(startPos, midPoint, target.position, timeProgress + 0.05f);
//                 Vector3 direction = (nextPosition - newPosition).normalized;
//             
//                 float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
//                 transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
//             }
//             
//             yield return null;
//         }
//     
//         // If we've reached here, either target was destroyed or lifetime expired
//         ReturnToPool();
//     }
//     
//     // Quadratic Bezier curve formula
//     private Vector3 QuadraticBezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
//     {
//         float u = 1 - t;
//         float tt = t * t;
//         float uu = u * u;
//         
//         Vector3 point = uu * p0; // (1-t)² * P0
//         point += 2 * u * t * p1; // 2(1-t)t * P1
//         point += tt * p2; // t² * P2
//         
//         return point;
//     }
//     
//     // Optional: Adjust arc height
//     public void SetArcHeight(float height)
//     {
//         arcHeight = height;
//     }
//     
//  
//     public override void Initialize(Transform target, float damage, float speed, float lifetime)
//     {
//         // Call the base initialization
//         base.Initialize(target, damage, speed, lifetime);
//         
//         // Any additional Arrow-specific initialization can go here
//     }
// }


using System.Collections;
using UnityEngine;

// Arrow projectile that uses a bezier flight path
public class Arrow : Projectile
{
    private float arcHeight = 3f;
    [SerializeField] private GameObject shadowSprite;
    
    // Add these variables to track height
    private float currentHeight = 0f;
    private float maxHeight;
    private float shadowScale = 0.5f; // Minimum shadow scale when at maximum height
    private Vector3 originalScale = new Vector3(1f, 1f, 1f); // Default scale in case transform.localScale is zero
    
    protected override void BeginFlight()
    {
        flightCoroutine = StartCoroutine(BezierFlight());
    }
    
    private IEnumerator BezierFlight()
    {
        Vector3 startPos = transform.position;
        float startTime = Time.time;
        float fixedDuration = 0.3f;
    
        float distanceToTarget = Vector3.Distance(startPos, target.position);
        float dynamicArcHeight = arcHeight * (distanceToTarget / 10f); // Scale arc height based on distance
        
        // Set the maximum height for scaling calculations
        maxHeight = dynamicArcHeight;
    
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
            
            // Calculate current height relative to the ground plane
            // In a top-down 2D game, this is the height above the line connecting start and target
            Vector3 groundPos = Vector3.Lerp(startPos, target.position, timeProgress);
            currentHeight = Vector3.Distance(newPosition, groundPos);
        
            // Calculate direction for rotation
            if (timeProgress < 0.95f) // Don't change rotation right at the end
            {
                Vector3 nextPosition = QuadraticBezier(startPos, midPoint, target.position, timeProgress + 0.05f);
                Vector3 direction = (nextPosition - newPosition).normalized;
            
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }
            
            // Scale the arrow based on height
            float heightScale = Mathf.Lerp(1.0f, 1.2f, currentHeight / maxHeight);
            transform.localScale = new Vector3(
                originalScale.x * heightScale,
                originalScale.y * heightScale,
                originalScale.z
            );
            
            // Update shadow based on current height
            UpdateShadow();
            
            yield return null;
        }
    
        ReturnToPool();
    }
    
    // Separate method to update the shadow based on current height
    private void UpdateShadow()
    {
        if (shadowSprite != null)
        {
            // Calculate how high the arrow is relative to maximum height (0 to 1)
            float heightRatio = Mathf.Clamp01(currentHeight / maxHeight);
            
            // The shadow gets smaller as the arrow gets higher
            float shadowSizeFactor = Mathf.Lerp(1.0f, shadowScale, heightRatio);
            shadowSprite.transform.localScale = new Vector3(
                originalScale.x * shadowSizeFactor,
                originalScale.y * shadowSizeFactor,
                originalScale.z
            );
            
            // Shadow opacity decreases with height
            Color shadowColor = shadowSprite.GetComponent<SpriteRenderer>().color;
            shadowColor.a = Mathf.Lerp(0.5f, 0.2f, heightRatio);
            shadowSprite.GetComponent<SpriteRenderer>().color = shadowColor;
            
            // Optionally, you could also offset the shadow position based on height
            // to create a more realistic effect where the shadow moves away from the arrow
            float shadowOffset = heightRatio * 0.5f; // Adjust this value as needed
            shadowSprite.transform.localPosition = new Vector3(shadowOffset, -shadowOffset, 0);
        }
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
    
    // Optional: Adjust arc height
    public void SetArcHeight(float height)
    {
        arcHeight = height;
    }
    
    // Optional: Set shadow scale factor
    public void SetShadowScale(float scale)
    {
        shadowScale = scale;
    }
    
    public override void Initialize(Transform target, float damage, float speed, float lifetime)
    {
        // Call the base initialization
        base.Initialize(target, damage, speed, lifetime);
        
        // Reset height values
        currentHeight = 0f;
        
        // Store original scale for reference, but ensure it's never zero
        if (transform.localScale.x != 0 && transform.localScale.y != 0) {
            originalScale = transform.localScale;
        }
        // If we're still at zero scale, keep the default
        
        // Any additional Arrow-specific initialization can go here
    }
    
    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        // Store original scale right at the start to ensure it's captured
        if (transform.localScale.x != 0 && transform.localScale.y != 0) {
            originalScale = transform.localScale;
        }
    }
}