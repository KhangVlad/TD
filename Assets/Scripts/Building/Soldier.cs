using System;
using UnityEngine;
using UnityEngine.Serialization;

public class Soldier : MonoBehaviour
{
    [Header("Components")] 
    [SerializeField] private Animator anim;
    [SerializeField] private SpriteRenderer soldierSprite;

    private ISoldierState currentState;
    public Monster monsterTarget;
    public Vector2 flagPos;
    public float attackRange { get; set; }
    public float MovementSpeed { get; set; } = 2f;
    public SoldierState State;
    private Vector2 offsetWithFlag;
    private UIHealthBar _healthBar;
    
    [Header("Combat")]
    public float currentHealth;
    public float maxHealth;
    public bool isEngaged = false; // Tracks if soldier is engaged with a monster

    [Header("State")] 
    public IdleState idleState;
    public MovingState movingState;
    public AttackState attackState;
    public MovingFlagState movingFlagState;

    [Header("Data and Stats")] 
    public SoldierSO soldierSO;

    private void Awake()
    {
        idleState = new IdleState();
        movingState = new MovingState();
        attackState = new AttackState();
        movingFlagState = new MovingFlagState();
    }

    private void Start()
    {
        _healthBar = UIHealthBarManager.Instance.CreateHealthBarForTarget(transform);
        
        // Initialize health
        if (soldierSO != null)
        {
            maxHealth = soldierSO.health;
            currentHealth = maxHealth;
        }
        else
        {
            maxHealth = 100;
            currentHealth = maxHealth;
        }
        
        if (_healthBar != null)
        {
            _healthBar.SetFillAmountImmediate(1f);
        }
    }
    
    private void OnDestroy()
    {
        if (_healthBar != null)
        {
            UIHealthBarManager.Instance.ReleaseHealthBar(_healthBar);
        }
    }

    public void InitializeWithFlagPosition(Vector2 flagPosition, Vector2 offset, Barracks b, SoldierSO s)
    {
        offsetWithFlag = offset;
        attackRange = s != null ? s.attackRange : 0.2f;
        Vector2 targetPos = flagPosition + offset;
        flagPos = targetPos;
        soldierSO = s;
        
        maxHealth = s.health;
        currentHealth = maxHealth;
        
        ChangeState(SoldierState.MovingFlag);
        b.OnFlagPositionChanged += OnFlagPositionChanged;
    }

    private void Update()
    {
        currentState?.UpdateState(this);
    }

    public void LookAtDirection(Vector2 dir)
    {
        if (dir.x < transform.position.x)
            soldierSprite.flipX = true;
        else
            soldierSprite.flipX = false;
    }

    #region State Machine

    public void ChangeState(SoldierState newState)
    {
        currentState?.ExitState(this);
        State = newState;
        switch (newState)
        {
            case SoldierState.Idle:
                currentState = idleState;
                break;
            case SoldierState.Moving:
                currentState = movingState;
                break;
            case SoldierState.Attacking:
                currentState = attackState;
                break;
            case SoldierState.MovingFlag:
                currentState = movingFlagState;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }

        currentState.EnterState(this);
    }

    private void OnFlagPositionChanged(Vector2 newFlagPos)
    {
        Vector2 targetPos = newFlagPos + offsetWithFlag;
        flagPos = targetPos;
        
        // Only change to moving flag state if not currently engaged with a monster
        if (!isEngaged)
        {
            ChangeState(SoldierState.MovingFlag);
        }
    }

    #endregion

    #region Detect Enemy

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Monster mon = other.GetComponent<Monster>();
            
            // Only engage if the soldier is not already engaged and the monster has no target
            if (mon != null && monsterTarget == null && !isEngaged && mon.targetSoldier == null)
            {
                monsterTarget = mon;
                mon.targetSoldier = this;
                isEngaged = true;
                
                // Change state to attacking if in range
                if (Vector2.Distance(transform.position, mon.transform.position) <= attackRange)
                {
                    ChangeState(SoldierState.Attacking);
                }
                else
                {
                    ChangeState(SoldierState.Moving);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Monster mon = other.GetComponent<Monster>();
            if (mon != null && monsterTarget == mon)
            {
                // Only clear the target if this is actually our target
                monsterTarget = null;
                isEngaged = false;
                
                // If the monster had this soldier as target, clear it
                if (mon.targetSoldier == this)
                {
                    mon.targetSoldier = null;
                }
                
                // Go back to flag position
                ChangeState(SoldierState.MovingFlag);
            }
        }
    }

    #endregion

    #region Combat

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        
        // Update health bar
        if (_healthBar != null)
        {
            _healthBar.UpdateFillAmount(currentHealth / maxHealth);
        }
        
        // Play hit animation if available
        
        // Check if dead
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    private void Die()
    {
        // Tell monster this soldier is no longer a valid target
        if (monsterTarget != null && monsterTarget.targetSoldier == this)
        {
            monsterTarget.targetSoldier = null;
        }
        
        // Notify barracks that a soldier died (optional)
        
        // Play death animation
        
        // Destroy after animation or immediately
        Destroy(gameObject);
    }
    
    #endregion

    #region Animation Controller

    public void PlayAttackAnimation()
    {
        anim.SetTrigger("Attack");
        anim.SetFloat("Blend", UnityEngine.Random.Range(0f, 1f));
    }

    public void PlayRunAnimation(bool active)
    {
        anim.SetBool("Run", active);
    }

    public void PlayIdleAnimation()
    {
        anim.SetTrigger("Idle");
    }

    #endregion
}

public enum SoldierState
{
    Idle,
    Moving,
    MovingFlag,
    Attacking
}