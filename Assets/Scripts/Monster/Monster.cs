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
        // Clean up health bar
        if (_healthBar != null && UIHealthBarManager.Instance != null)
        {
            UIHealthBarManager.Instance.ReleaseHealthBar(_healthBar);
        }
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
        MonsterManager.Instance.OnMonsterDefeated(this, rewardAmount);
        
        // // Play death animation or effects
        // if (animator != null && animator.HasTrigger("Death"))
        // {
        //     animator.SetTrigger("Death");
        //     // You might want to delay the destruction here
        //     Destroy(gameObject, 1f); // Destroy after animation
        // }
        // else
        // {
        //     Destroy(gameObject);
        // }
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
        
        // Deal damage to the soldier
        // This would typically be called from animation events
        // or after a delay to match the animation
    }
    
    // This can be called from animation events when the attack hits
    public void DealDamage()
    {
        if (targetSoldier != null)
        {
            // targetSoldier.TakeDamage(monsterSO.damage);
        }
    }
    
    #endregion
    
    #region Collision Detection
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Soldier"))
        {
            Soldier soldier = other.GetComponent<Soldier>();
            if (soldier != null && targetSoldier == null)
            {
                targetSoldier = soldier;
            }
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Soldier"))
        {
            Soldier soldier = other.GetComponent<Soldier>();
            if (soldier != null && targetSoldier == soldier)
            {
                targetSoldier = null;
            }
        }
    }
    
    #endregion
}