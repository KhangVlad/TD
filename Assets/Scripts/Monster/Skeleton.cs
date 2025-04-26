using UnityEngine;

public class SkeletonMonster : MonsterBase
{
    [Header("Skeleton Specific")]
    [SerializeField] private GameObject bonePrefab;
    [SerializeField] private Transform boneThrowPoint;
    [SerializeField] private float boneThrowCooldown = 3f;
    [SerializeField] private float boneThrowSpeed = 5f;
    [SerializeField] private float boneDamage = 5f;
    [SerializeField] private ParticleSystem dustParticles;
    
    // Special animation parameters
    private static readonly int Resurrect = Animator.StringToHash("Resurrect");
    private static readonly int ThrowBone = Animator.StringToHash("ThrowBone");
    
    // Skeleton state tracking
    private bool hasResurrected = false;
    private float boneThrowTimer = 0f;
    
    protected override void Awake()
    {
        base.Awake();
        
        // Initialize skeleton-specific components
        if (boneThrowPoint == null)
        {
            boneThrowPoint = new GameObject("BoneThrowPoint").transform;
            boneThrowPoint.SetParent(transform);
            boneThrowPoint.localPosition = new Vector3(0.2f, 0.5f, 0); // Position for throwing bones
        }
    }
    
    protected override void Update()
    {
        base.Update();
        
        // Update bone throw cooldown if not in attacking state
        if (CurrentState != _attackState && boneThrowTimer < boneThrowCooldown)
        {
            boneThrowTimer += Time.deltaTime;
            
            // Throw bone as a special attack when cooldown is ready
            if (boneThrowTimer >= boneThrowCooldown && targetSoldier != null)
            {
                ThrowBoneAttack();
            }
        }
    }
    
    private void ThrowBoneAttack()
    {
        if (bonePrefab == null || targetSoldier == null) return;
        
        // Reset timer
        boneThrowTimer = 0;
        
        // Face target
        LookAtTarget(targetSoldier.transform.position);
        
        // Play animation
        if (animator != null)
        {
            animator.SetTrigger(ThrowBone);
        }
        
        // The actual bone will be instantiated via animation event
    }
    
    // Called by animation event during bone throw animation
    public void OnThrowBone()
    {
        if (bonePrefab == null || targetSoldier == null) return;
        
        // Create bone projectile
        GameObject bone = Instantiate(bonePrefab, boneThrowPoint.position, Quaternion.identity);
        
        // Get direction to target
        Vector2 direction = (targetSoldier.transform.position - boneThrowPoint.position).normalized;
        
        // Set up projectile (assuming it has a Projectile component)
        Projectile projectile = bone.GetComponent<Projectile>();
        if (projectile != null)
        {
           
        }
        else
        {
            // Simple movement if no projectile component
            Rigidbody2D rb = bone.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = direction * boneThrowSpeed;
            }
            
            // Destroy after some time
            Destroy(bone, 3f);
        }
    }
    
    // Override TakeDamage to handle resurrection
    public override void TakeDamage(float damage)
    {
        // Apply damage
        base.TakeDamage(damage);
        
        // Check for resurrection ability (first time only)
        if (CurrentHealth <= 0 && !hasResurrected)
        {
            // Cancel normal death
            CurrentHealth = MaxHealth * 0.3f; // Resurrect with 30% health
            hasResurrected = true;
            
            // Play resurrection animation instead of dying
            if (animator != null)
            {
                animator.SetTrigger(Resurrect);
                
                // Update health bar
                if (healthBar != null)
                {
                    healthBar.UpdateFillAmount(CurrentHealth / MaxHealth);
                }
            }
            
            // Play resurrection VFX
            if (dustParticles != null)
            {
                dustParticles.Play();
            }
            
            // Cancel any death sequence that might have been started
            CancelInvoke(nameof(Die));
            
            return;
        }
    }
    
    public override void LookAtTarget(Vector2 targetPosition)
    {
        base.LookAtTarget(targetPosition);
        
        // Update bone throw point based on direction
        if (boneThrowPoint != null)
        {
            float xOffset = transform.localScale.x > 0 ? 0.2f : -0.2f;
            boneThrowPoint.localPosition = new Vector3(xOffset, 0.5f, 0);
        }
    }
    
    // Called when the skeleton is done resurrecting
    public void OnResurrectionComplete()
    {
        // Resume normal behavior
        if (CurrentState != _movingState)
        {
            ChangeState(MonsterState.Moving);
        }
    }
    
    // Override initialization to set skeleton-specific parameters
    public override void Initialize(MonsterSO so)
    {
        base.Initialize(so);
        hasResurrected = false;
        boneThrowTimer = boneThrowCooldown * 0.5f; // Start with half cooldown
    }
    
    // Create footstep dust
    public void OnFootstep()
    {
        if (dustParticles != null)
        {
            dustParticles.Play();
        }
    }
}