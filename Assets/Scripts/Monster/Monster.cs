using System;
using UnityEngine;

public class Monster : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;
    
    [Header("Stats")]
    public MonsterSO monsterSO { get; private set; }
    public int currentNodeIndex = 0;
    public float Speed { get; private set; }
    public float CurrentHealth { get; private set; }
    public float MaxHealth { get; private set; }
    public float attackRange { get; private set; }
    public int rewardAmount { get; private set; }
    
    [Header("State")]
    public MonsterMovingState _movingState;
    public PathCompleteState _pathCompleteState;
    public MonsterAttackingState _attackState;
    public IMonsterState CurrentState { get; private set; }
    public Soldier targetSoldier;
    
    private UIHealthBar _healthBar;
    
    #region Unity Lifecycle
    
    private void Awake()
    {
        _movingState = new MonsterMovingState();
        _pathCompleteState = new PathCompleteState();
        _attackState = new MonsterAttackingState();
        
        // Get references if not already assigned
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    private void Start()
    {
        ChangeState(MonsterState.Moving);
        
        // Create health bar
        if (UIHealthBarManager.Instance != null)
        {
            _healthBar = UIHealthBarManager.Instance.CreateHealthBarForTarget(transform);
        }
    }

    private void Update()
    {
        CurrentState?.UpdateState(this);
    }
    
    private void OnDestroy()
    {
        // Clear soldier's target reference if we're dying
        if (targetSoldier != null)
        {
            targetSoldier.monsterTarget = null;
            targetSoldier.isEngaged = false;
        }
        
        // Clean up health bar
        if (_healthBar != null && UIHealthBarManager.Instance != null)
        {
            UIHealthBarManager.Instance.ReleaseHealthBar(_healthBar);
        }
        
        // Clean up in monster manager
        MonsterManager.Instance?.RemoveMonster(this);
    }
    
    #endregion
    
    #region Initialization
    
    public void Initialize(MonsterSO so)
    {
        monsterSO = so;
        
        // Set stats from SO
        Speed = so.speed;
        MaxHealth = so.health;
        CurrentHealth = MaxHealth;
        attackRange = 0.2f;
        rewardAmount = so.rewardAmount;
        
        // Set visuals
        if (spriteRenderer != null && so.sprite != null)
        {
            spriteRenderer.sprite = so.sprite;
        }
        
        // Register with MonsterManager
        MonsterManager.Instance.RegisterMonster(this);
        
        // Set animator parameters if needed
        if (animator != null)
        {
            animator.SetInteger("MonsterType", (int)so.type);
        }
    }
    
    #endregion
    
    #region State Management
    
    public void ChangeState(MonsterState newState)
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
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }

        CurrentState.EnterState(this);
    }
    
    #endregion
    
    #region Monster Actions
    
    public void OnPathComplete()
    {
        // Monster reached the end of the path
        // Notify the MonsterManager
        MonsterManager.Instance.OnMonsterReachedEnd(this);
        
        // Destroy the monster
        Destroy(gameObject);
    }
    
    public void TakeDamage(float damage)
    {
        // Apply damage
        CurrentHealth -= damage;
        
        // Update health bar
        if (_healthBar != null)
        {
            _healthBar.UpdateFillAmount(CurrentHealth / MaxHealth);
        }
        
        // Play hit animation or effects
        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }
        
        // Check if monster is defeated
        if (CurrentHealth <= 0)
        {
            Die();
        }
    }
    
    private void Die()
    {
        // Notify the MonsterManager about the defeat
        MonsterManager.Instance.OnMonsterDefeated(this, rewardAmount);
        
        // Release the assigned soldier to find new targets
        if (targetSoldier != null)
        {
            targetSoldier.monsterTarget = null;
            targetSoldier.isEngaged = false;
            targetSoldier = null;
        }

        Destroy(gameObject, 1f);
    }
    
    public void Attack(Soldier target)
    {
        if (target == null)
            return;
            
        // Play attack animation
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
        
        // Deal damage to the soldier after a delay (could be called from animation event)
        // For simplicity, we'll deal damage directly here
        target.TakeDamage(monsterSO.damage);
    }
    
    #endregion
    
    #region Collision Detection
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Soldier"))
        {
            Soldier soldier = other.GetComponent<Soldier>();
            
            // Only detect a soldier if:
            // 1. We don't already have a target
            // 2. The soldier doesn't already have a target
            // 3. The soldier is active
            if (soldier != null && targetSoldier == null && !soldier.isEngaged && soldier.isActiveAndEnabled)
            {
                targetSoldier = soldier;
                soldier.monsterTarget = this;
                soldier.isEngaged = true;
            }
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Soldier"))
        {
            Soldier soldier = other.GetComponent<Soldier>();
            
            // Only clear if this is our current target
            if (soldier != null && targetSoldier == soldier)
            {
                // Let the soldier know it's no longer engaged with us
                targetSoldier.monsterTarget = null;
                targetSoldier.isEngaged = false;
                targetSoldier = null;
            }
        }
    }
    
    #endregion
}