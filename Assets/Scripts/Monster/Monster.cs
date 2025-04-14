using System;
using UnityEngine;

public class Monster : MonoBehaviour
{
    public MonsterSO monsterSO { get; private set; }
    public int currentNodeIndex = 0;
    public float Speed { get; private set; }
    public float CurrentHealth { get; private set; }
    public float attackRange  { get; private set; }
    
    [Header("State")]
    public MonsterMovingState _movingState;
    public PathCompleteState _pathCompleteState;
    public MonsterAttackingState _attackState;
    public IMonsterState CurrentState { get; private set; }
    public Soldier targetSoldier;

    private void Awake()
    {
        _movingState = new MonsterMovingState();
        _pathCompleteState = new PathCompleteState();
        _attackState = new MonsterAttackingState();
    }

    private void Start()
    {
        ChangeState(MonsterState.Moving);
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
        attackRange = 0.2f;
        MonsterManager.Instance.RegisterMonster(this);
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
        MonsterManager.Instance.RemoveMonster(this);
    }

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
            if (soldier != null)
            {
                targetSoldier = null;
            }
        }
    }
    
    
    public void TakeDamage(float damage)
    {
        CurrentHealth -= damage;
        if (CurrentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }
    
    
}
