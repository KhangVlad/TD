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
        Debug.Log("Soldier entered Idle state.");
    }

    public void UpdateState(Soldier soldier)
    {
    }

    public void ExitState(Soldier soldier)
    {
        Debug.Log("Soldier exiting Idle state.");
    }
}

public class MovingState : ISoldierState
{
    public void EnterState(Soldier soldier)
    {
        Debug.Log("Soldier entered Moving state.");
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
        Debug.Log("Soldier exiting Moving state.");
    }
}

// Attack state
public class AttackState : ISoldierState
{
    public void EnterState(Soldier soldier)
    {
        Debug.Log("Soldier entered Attack state.");
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
    public float DetechRange  { get; private set; } = 1f; //us

    private void Start()
    {
        ChangeState(new IdleState());
    }

    private void Update()
    {
        currentState?.UpdateState(this);
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, DetechRange);
   
    }
}