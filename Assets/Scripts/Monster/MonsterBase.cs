using UnityEngine;

public enum FacingDirection
{
    Up,
    Right,
    Down,
    Left,
    None
}

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
        CurrentState?.UpdateState(this);
    }
    
    public virtual void LookAtTarget(Vector2 targetPosition)
    {
        Vector2 direction = new Vector2(
            targetPosition.x - transform.position.x,
            targetPosition.y - transform.position.y
        ).normalized;
        facingDirection = direction;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        angle -= 90f;
        if (angle > 180f) angle -= 360f;
        if (angle < -180f) angle += 360f;
        if (direction.x < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
            
            if (currentDirection == FacingDirection.Right)
            {
                currentDirection = FacingDirection.Left;
            }
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);
            if (currentDirection == FacingDirection.Left)
            {
                currentDirection = FacingDirection.Right;
            }
        }
        UpdateAnimationDirectionFromAngle(angle);
    }
    
    protected virtual void UpdateAnimationDirectionFromAngle(float angle)
    {
        if (animator == null || !animator.runtimeAnimatorController) 
        {
            Debug.LogWarning("Animator missing or has no controller assigned");
            return;
        }
        
        try
        {
            animator.ResetTrigger(MovingUp);
            animator.ResetTrigger(MovingDown);
            animator.ResetTrigger(MovingHorizon);
            if (angle >= -60f && angle <= 60f)
            {
                // Up direction
                animator.SetTrigger(MovingUp);
                currentDirection = FacingDirection.Up;
            }
            else if ((angle > 60f && angle < 150f) || (angle < -60f && angle > -150f))
            {
                animator.SetTrigger(MovingHorizon);
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
                animator.SetTrigger(MovingDown);
                currentDirection = FacingDirection.Down;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error setting animation trigger: {e.Message}");
        }
    }

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