using System;
using UnityEngine;

// State interface
public interface ISoldierState
{
    void EnterState(Soldier soldier);
    void UpdateState(Soldier soldier);
    void ExitState(Soldier soldier);
}

// Idle state
public class IdleState : ISoldierState
{
    public void EnterState(Soldier soldier)
    {
    }

    public void UpdateState(Soldier soldier)
    {
        // Check for enemies in detection range
    }

    public void ExitState(Soldier soldier)
    {
    }
}

public class MovingState : ISoldierState
{
    public void EnterState(Soldier soldier)
    {
    }

    public void UpdateState(Soldier soldier)
    {
        // Move towards the target position
        soldier.transform.position = Vector2.MoveTowards(
            soldier.transform.position,
            soldier.TargetPosition,
            soldier.MovementSpeed * Time.deltaTime
        );

        // Check if the soldier has reached the target
        if (Vector2.Distance(soldier.transform.position, soldier.TargetPosition) <= 0.1f)
        {
            soldier.ChangeState(new IdleState());
        }
    }

    public void ExitState(Soldier soldier)
    {
    }
}

// Attack state
public class AttackState : ISoldierState
{
    public void EnterState(Soldier soldier)
    {
    }

    public void UpdateState(Soldier soldier)
    {
        // Attack behavior (e.g., deal damage to a target)
    }

    public void ExitState(Soldier soldier)
    {
        Debug.Log("Soldier exiting Attack state.");
    }
}

// Soldier class with state management
public class Soldier : MonoBehaviour
{
    private ISoldierState currentState;
    public Vector2 TargetPosition { get; private set; }
    public float MovementSpeed { get; private set; } = 2f;
    public float DetectionRange { get; private set; } = 1f;
    
    private int formationIndex = 0;
    private bool isInitialized = false;
    
    private Vector2 offsetWithFlag;

    private void Start()
    {
        ChangeState(new IdleState());
     
    }
    
    public void InitializeWithFlagPosition(Vector2 flagPosition, Vector2 offset, Barracks b)
    {
        offsetWithFlag = offset;
        isInitialized = true;
        Vector2 targetPos = flagPosition + offset;
        
        TargetPosition = targetPos;
        
        // If the flag is not at the current position, move there
        if (Vector2.Distance(transform.position, targetPos) > 0.1f)
        {
            ChangeState(new MovingState());
        }
        b.OnFlagPositionChanged += OnFlagPositionChanged;
    }
    
    private void OnDestroy()
    {
     
    }

    private void Update()
    {
        currentState?.UpdateState(this);
    }
    
    public void SetFormationIndex(int index)
    {
        formationIndex = index;
    }

    public void ChangeState(ISoldierState newState)
    {
        currentState?.ExitState(this);
        currentState = newState;
        currentState.EnterState(this);
    }

    public void SetTargetPosition(Vector2 targetPos)
    {
        TargetPosition = targetPos;
        ChangeState(new MovingState());
    }

    public void Attack()
    {
        ChangeState(new AttackState());
    }
    
    // Event handler for when flag position changes
    private void OnFlagPositionChanged(Vector2 newFlagPos)
    {
        // Calculate this soldier's target position based on their formation index
        Vector2 targetPos = newFlagPos + offsetWithFlag;
        
        // Move to the new position
        SetTargetPosition(targetPos);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, DetectionRange);
    }
}