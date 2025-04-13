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
        if (soldier.monsterTarget is not null)
        {
            soldier.ChangeState(soldier.movingState);
        }
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
        if(soldier.monsterTarget is not null)
        {
    
            soldier.transform.position = Vector2.MoveTowards(
                soldier.transform.position,
                soldier.monsterTarget.transform.position,
                soldier.MovementSpeed * Time.deltaTime
            );

            if (Vector2.Distance(soldier.transform.position, soldier.monsterTarget.transform.position) <= soldier.attackRange)
            {
                soldier.ChangeState(soldier.attackState);
            }
            
            
        }
        else
        {
            soldier.ChangeState(soldier.idleState);
        }
    }

    public void ExitState(Soldier soldier)
    {
    }
}

public class MovingFlagState : ISoldierState
{
    public void EnterState(Soldier soldier)
    {
    }

    public void UpdateState(Soldier soldier)
    {
        // Move towards the flag position
        soldier.transform.position = Vector2.MoveTowards(
            soldier.transform.position,
            soldier.TargetPosition,
            soldier.MovementSpeed * Time.deltaTime
        );

        if (Vector2.Distance(soldier.transform.position, soldier.TargetPosition) <= soldier.attackRange)
        {
            soldier.ChangeState(soldier.idleState);
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
    }

    public void ExitState(Soldier soldier)
    {
    }
}

public class Soldier : MonoBehaviour 
{
    private ISoldierState currentState;
    public Monster monsterTarget;
    public Vector2 TargetPosition { get; private set; }
    
    public float attackRange { get; set; }
    
    public float MovementSpeed { get; set; } = 2f;
    public SoldierState State;
    private Vector2 offsetWithFlag;
    public IdleState idleState;
    public MovingState movingState;
    public AttackState attackState;
    public MovingFlagState movingFlagState;

    private void Awake()
    {
        idleState = new IdleState();
        movingState = new MovingState();
        attackState = new AttackState();
        movingFlagState = new MovingFlagState();
    }
    
   
    
    public void InitializeWithFlagPosition(Vector2 flagPosition, Vector2 offset, Barracks b)
    {
        offsetWithFlag = offset;
        attackRange = 0.2f;
        Vector2 targetPos = flagPosition + offset;
        TargetPosition = targetPos;
        ChangeState(movingFlagState);
        b.OnFlagPositionChanged += OnFlagPositionChanged;
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
        State = newState is IdleState ? SoldierState.Idle : newState is MovingState ? SoldierState.Moving : SoldierState.Attacking;
    }

    private void SetTargetPosition(Vector2 targetPos)
    {
        TargetPosition = targetPos;
        ChangeState(movingFlagState);
    }


    private void OnFlagPositionChanged(Vector2 newFlagPos)
    {
        Vector2 targetPos = newFlagPos + offsetWithFlag;
        SetTargetPosition(targetPos);
    }

  
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Monster mon = other.GetComponent<Monster>();
            if (mon != null && monsterTarget == null)
            {
                monsterTarget = mon;
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
                monsterTarget = null;
                ChangeState(movingState);
            }
        }
    }
    
   
}


public enum SoldierState
{
    Idle,
    Moving,
    MovingFlag,
    Attacking
    
}