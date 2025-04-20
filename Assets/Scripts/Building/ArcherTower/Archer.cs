using System;
using UnityEngine;

public class Archer : Unit
{
    [Header("Archer State")]
    public ArcherIdleState idleState;
    public ArcherAttackState attackState;
    private IArcherState currentState;
    public ArcherState State;

    [Header("Archer-specific Components")]
    [SerializeField] private Transform arrowSpawnPoint;
    [SerializeField] private Projectile arrowPrefab;

    
    // Archer-specific properties
    public float arrowSpeed = 10f;
    public float arrowLifetime = 3f;

    private void Awake()
    {
        idleState = new ArcherIdleState();
        attackState = new ArcherAttackState();
    }

    protected override void Start()
    {
        base.Start();
    }

    public void Initialize(ArcherSO data)
    {
        base.Initialize(data);
        if (data != null)
        {
            arrowSpeed = data.arrowSpeed;
            arrowLifetime = data.arrowLifetime;
        }
        ChangeState(ArcherState.Idle);
    }

    protected override void CleanupMonsterTarget()
    {
        if (monsterTarget != null)
        {
            // monsterTarget.SetSoldierTarget(null);
        }
    }

    protected override void HandleTargetChange(Monster target)
    {
        if (target != null)
        {
            ChangeState(ArcherState.Attacking);
        }
        else
        {
            ChangeState(ArcherState.Idle);
        }
    }

    protected override void InitializeStateMachine()
    {
        ChangeState(ArcherState.Idle);
    }

    protected override void UpdateStateMachine()
    {
        currentState?.UpdateState(this);
    }

    public void ChangeState(ArcherState newState)
    {
        // Exit the current state
        currentState?.ExitState(this);
        State = newState;

        switch (newState)
        {
            case ArcherState.Idle:
                currentState = idleState;
                break;
            case ArcherState.Attacking:
                currentState = attackState;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }

        // Enter the new state
        currentState.EnterState(this);
    }

    public void FireArrow()
    {
        if (monsterTarget == null || arrowPrefab == null)
        {
            return;
        }
    
        // Create arrow
        Projectile arrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, Quaternion.identity);
        if (arrow == null)
        {
            Debug.LogWarning("Failed to instantiate arrow prefab");
            return;
        }
        float damage = 10f; 
        if (unitSO != null)
        {
            damage = unitSO.attackDamage;
        }
        else
        {
            Debug.LogWarning("unitSO is null, using default damage value");
        }
    
        arrow.Initialize(monsterTarget.transform, damage, arrowSpeed, arrowLifetime);
    }
}


// Projectile class for archers
