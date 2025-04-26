using UnityEngine;

// Public enum for direction that will be visible in the inspector
public enum FacingDirection
{
    Up,
    Right,
    Down,
    Left,
    None
}

// Extending the MonsterBase class to include the direction enum
public abstract class MonsterBase : MonoBehaviour
{
    public MonsterSO monsterSO { get; protected set; }
    public int currentNodeIndex = 0;
    public float Speed { get; protected set; }
    public float CurrentHealth;
    public float MaxHealth { get; protected set; }
    public float attackRange { get; protected set; }

    [Header("State")]
    protected MonsterMovingState _movingState;
    protected PathCompleteState _pathCompleteState;
    protected MonsterAttackingState _attackState;
    public IMonsterState CurrentState { get; protected set; }
    
    [Header("References")]
    [SerializeField] protected UIHealthBar healthBar;
    [SerializeField] protected Animator animator;
    
    // Animation parameters
    protected static readonly int MovingDown = Animator.StringToHash("Down");
    protected static readonly int MovingUp = Animator.StringToHash("Up");
    protected static readonly int MovingHorizon = Animator.StringToHash("Horizon");
    protected static readonly int AttackTrigger = Animator.StringToHash("Attack");
    protected static readonly int DieState = Animator.StringToHash("Die");
    protected static readonly int SkillTrigger = Animator.StringToHash("Skill");
    
    // Direction tracking with inspector visibility
    [Header("Direction")]
    [SerializeField] public FacingDirection currentDirection = FacingDirection.None;
    public Unit targetSoldier;
    
   
    public Vector2 facingDirection;

    protected virtual void Awake()
    {
        _movingState = new MonsterMovingState();
        _pathCompleteState = new PathCompleteState();
        _attackState = new MonsterAttackingState();
    }

    protected virtual void Start()
    {
        ChangeState(MonsterState.Moving);
        
        // Get health bar if it's available
        if (UIHealthBarManager.Instance != null)
        {
            healthBar = UIHealthBarManager.Instance.CreateHealthBarForTarget(transform);
        }
    }
    
    public virtual void SetSoldierTarget(Unit s)
    {
        if(s is Archer) return;
        this.targetSoldier = s;
    }

    protected virtual void Update()
    {
        // For top-down games, we don't need to calculate movement direction every frame
        // Direction is determined when targeting or path changes
        
        // Update state
        CurrentState?.UpdateState(this);
    }
    
    protected virtual void UpdateMovementDirection()
    {
        // For a top-down game, we don't need to track incremental changes
        // We just need the direction toward the target
        if (CurrentState == _movingState)
        {
            // We're only concerned with significant movement
            // The movement direction is already determined by LookAtTarget
            // No need to calculate it again here
        }
    }
    
    public virtual void LookAtTarget(Vector2 targetPosition)
    {
        // Calculate the direction vector from current position to target
        Vector2 direction = new Vector2(
            targetPosition.x - transform.position.x,
            targetPosition.y - transform.position.y
        ).normalized;
        
        // Store facing direction for animation
        facingDirection = direction;
        
        // Calculate the angle in degrees
        // Atan2 returns angle in radians between -π and π
        // Convert to degrees (-180 to 180)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        // Adjust the angle to make 0 degrees point up (instead of right)
        // This rotates the coordinate system by 90 degrees
        angle -= 90f;
        
        // Normalize the angle to be between -180 and 180
        if (angle > 180f) angle -= 360f;
        if (angle < -180f) angle += 360f;
        
        // Flip sprite based on horizontal direction
        if (direction.x < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
            
            // Update enum for left-facing direction when horizontal
            if (currentDirection == FacingDirection.Right)
            {
                currentDirection = FacingDirection.Left;
            }
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);
            
            // Update enum for right-facing direction when horizontal
            if (currentDirection == FacingDirection.Left)
            {
                currentDirection = FacingDirection.Right;
            }
        }
            
        // Determine facing direction based on angle segments
        UpdateAnimationDirectionFromAngle(angle);
    }
    
    protected virtual void UpdateAnimationDirectionFromAngle(float angle)
    {
        // Check if we have a valid animator with a controller
        if (animator == null || !animator.runtimeAnimatorController) 
        {
            Debug.LogWarning("Animator missing or has no controller assigned");
            return;
        }
        
        try
        {
            // Reset all triggers to avoid conflicts
            animator.ResetTrigger(MovingUp);
            animator.ResetTrigger(MovingDown);
            animator.ResetTrigger(MovingHorizon);
            
            // Apply the angle segment logic:
            // -60 to 60 = Up
            // 60 to 150 or -60 to -150 = Horizontal
            // Greater than 150 or less than -150 = Down
            
            float absAngle = Mathf.Abs(angle);
            
            if (angle >= -60f && angle <= 60f)
            {
                // Up direction
                animator.SetTrigger(MovingUp);
                currentDirection = FacingDirection.Up;
            }
            else if ((angle > 60f && angle < 150f) || (angle < -60f && angle > -150f))
            {
                // Horizontal direction
                animator.SetTrigger(MovingHorizon);
                
                // Update the direction enum based on horizontal scale
                if (transform.localScale.x < 0)
                {
                    currentDirection = FacingDirection.Left;
                }
                else
                {
                    currentDirection = FacingDirection.Right;
                }
            }
            else
            {
                // Down direction (angle > 150 or angle < -150)
                animator.SetTrigger(MovingDown);
                currentDirection = FacingDirection.Down;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error setting animation trigger: {e.Message}");
        }
    }

    // Rest of the MonsterBase class methods remain unchanged
    public virtual void Initialize(MonsterSO so)
    {
        monsterSO = so;
        Speed = so.speed;
        CurrentHealth = so.health;
        MaxHealth = so.health;
        attackRange = 0.2f;
        MonsterManager.Instance.RegisterMonster(this);
        
        // Initialize health bar if it exists
        if (healthBar != null)
        {
            healthBar.SetFillAmountImmediate(1f);
        }
    }

    public virtual void ChangeState(MonsterState newState)
    {
        CurrentState?.ExitState(this);
        switch (newState)
        {
            case MonsterState.Moving:
                CurrentState = _movingState;
                break;
            case MonsterState.Attack:
                CurrentState = _attackState;
                break;
            case MonsterState.PathComplete:
                CurrentState = _pathCompleteState;
                break;
            default:
                throw new System.ArgumentOutOfRangeException(nameof(newState), newState, null);
        }

        CurrentState.EnterState(this);
    }

    public virtual void OnPathComplete()
    {
        Destroy(gameObject);
    }
    
    // Call this when getting a new path node
    public virtual void UpdatePathNode(Vector3 newNodePosition)
    {
        LookAtTarget(newNodePosition);
    }

    protected virtual void OnDestroy()
    {
        if (targetSoldier != null)
        {
            if (targetSoldier.monsterTarget == this)
            {
                targetSoldier.monsterTarget = null;
            }
            targetSoldier = null;
        }
        
        if (MonsterManager.Instance != null)
        {
            MonsterManager.Instance.RemoveMonster(this);
        }
        
        if (healthBar != null && UIHealthBarManager.Instance != null)
        {
            UIHealthBarManager.Instance.ReleaseHealthBar(healthBar);
        }
    }
    
    public virtual void TakeDamage(float damage)
    {
        CurrentHealth -= damage;
        
        if (healthBar != null)
        {
            healthBar.UpdateFillAmount(CurrentHealth / MaxHealth);
        }
        
        if (CurrentHealth <= 0)
        {
            Die();
        }
    }
    
    protected virtual void Die()
    {
        if (animator != null)
        {
            animator.SetTrigger(DieState);
            Destroy(gameObject, animator.GetCurrentAnimatorStateInfo(0).length);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}