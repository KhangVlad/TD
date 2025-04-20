using System;
using UnityEngine;

public class Monster : MonoBehaviour
{
    public MonsterSO monsterSO { get; private set; }
    public int currentNodeIndex = 0;
    public float Speed { get; private set; }
    public float CurrentHealth;
    public float MaxHealth { get; private set; }
    public float attackRange { get; private set; }

    [Header("State")]
    public MonsterMovingState _movingState;
    public PathCompleteState _pathCompleteState;
    public MonsterAttackingState _attackState;
    public IMonsterState CurrentState { get; private set; }
    
    [Header("References")]
    [SerializeField] private UIHealthBar healthBar;
    
    // Targeting properties - used by Soldier class
    public Unit targetSoldier;

    private void Awake()
    {
        _movingState = new MonsterMovingState();
        _pathCompleteState = new PathCompleteState();
        _attackState = new MonsterAttackingState();
    }

    private void Start()
    {
        ChangeState(MonsterState.Moving);
        
        // Get health bar if it's available
        if (UIHealthBarManager.Instance != null)
        {
            healthBar = UIHealthBarManager.Instance.CreateHealthBarForTarget(transform);
        }
    }
    public void SetSoldierTarget(Unit s)
    {
        if(s is Archer)  return;
        this.targetSoldier = s;
    }

    private void Update()
    {
        CurrentState?.UpdateState(this);
    }

    public void Initialize(MonsterSO so)
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
                throw new System.ArgumentOutOfRangeException(nameof(newState), newState, null);
        }

        CurrentState.EnterState(this);
    }

    public void OnPathComplete()
    {
        Debug.Log("Monster reached the end of the path.");
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        // Clear soldier targeting
        if (targetSoldier != null)
        {
            // Only clear if we are the target
            if (targetSoldier.monsterTarget == this)
            {
               
            }
            targetSoldier = null;
        }
        
        // Clean up references in MonsterManager
        if (MonsterManager.Instance != null)
        {
            MonsterManager.Instance.RemoveMonster(this);
        }
        
        // Release health bar if it exists
        if (healthBar != null && UIHealthBarManager.Instance != null)
        {
            UIHealthBarManager.Instance.ReleaseHealthBar(healthBar);
        }
    }
    
    public void TakeDamage(float damage)
    {
        CurrentHealth -= damage;
        
        // Update health bar if it exists
        if (healthBar != null)
        {
            healthBar.UpdateFillAmount(CurrentHealth / MaxHealth);
        }
        
        if (CurrentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }
}