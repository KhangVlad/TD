using UnityEngine;

public interface ISoldierState
{
    void EnterState(Soldier soldier);
    void UpdateState(Soldier soldier);
    void ExitState(Soldier soldier);
}

public class IdleState : ISoldierState
{
    public void EnterState(Soldier soldier)
    {
        soldier.PlayIdleAnimation();
    }

    public void UpdateState(Soldier soldier)
    {
        if (soldier.monsterTarget != null)
        {
            soldier.ChangeState(SoldierState.Moving);
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
        soldier.PlayRunAnimation(true);
        if (soldier.monsterTarget != null)
        {
            soldier.LookAtDirection(soldier.monsterTarget.transform.position);
        }
    }

    public void UpdateState(Soldier soldier)
    {
        if (soldier.monsterTarget == null)
        {
            soldier.ChangeState(SoldierState.MovingFlag);
            return;
        }
        soldier.transform.position = Vector2.MoveTowards(
            soldier.transform.position,
            soldier.monsterTarget.transform.position,
            soldier.MovementSpeed * Time.deltaTime
        );
        
        // If within attack range, switch to attacking state
        if (Vector2.Distance(soldier.transform.position, soldier.monsterTarget.transform.position) <= soldier.attackRange)
        {
            soldier.ChangeState(SoldierState.Attacking);
        }
    }

    public void ExitState(Soldier soldier)
    {
        soldier.PlayRunAnimation(false);
    }
}

public class MovingFlagState : ISoldierState
{
    private const float ARRIVAL_THRESHOLD = 0.04f;
    
    public void EnterState(Soldier soldier)
    {
        Debug.Log("AABABABHAH");
        soldier.PlayRunAnimation(true);
        soldier.LookAtDirection(soldier.flagPos);
    }

    // public void UpdateState(Soldier soldier)
    // {
    //  Debug.Log("AAAA");
    //     soldier.transform.position = Vector2.MoveTowards(
    //         soldier.transform.position,
    //         soldier.flagPos,
    //         soldier.MovementSpeed * Time.deltaTime
    //     );
    //     if (Vector2.Distance(soldier.transform.position, soldier.flagPos) <= ARRIVAL_THRESHOLD)
    //     {
    //         soldier.ChangeState(SoldierState.Idle);
    //     }
    // }
    public void UpdateState(Soldier soldier)
    {
        // Log the key values to understand what's happening
        Debug.Log($"Current position: {soldier.transform.position}, Flag position: {soldier.flagPos}, " +
                  $"Distance: {Vector2.Distance(soldier.transform.position, soldier.flagPos)}, " +
                  $"Movement amount: {soldier.MovementSpeed * Time.deltaTime}");
    
        Vector2 oldPosition = soldier.transform.position;
    
        soldier.transform.position = Vector2.MoveTowards(
            soldier.transform.position,
            soldier.flagPos,
            soldier.MovementSpeed * Time.deltaTime
        );
    
        // Check if position actually changed
        if (oldPosition == (Vector2)soldier.transform.position)
        {
            Debug.LogWarning("Position did not change!");
        }
        else
        {
            Debug.Log($"Moved from {oldPosition} to {soldier.transform.position}");
        }
    
        if (Vector2.Distance(soldier.transform.position, soldier.flagPos) <= ARRIVAL_THRESHOLD)
        {
            Debug.Log("Reached flag, changing to Idle state");
            soldier.ChangeState(SoldierState.Idle);
        }
    }

    public void ExitState(Soldier soldier)
    {
        soldier.PlayRunAnimation(false);
    }
}

public class AttackState : ISoldierState
{
    private float _attackTimer = 0;
    private float _attackCooldown = 1.5f;

    public void EnterState(Soldier soldier)
    {
        _attackTimer = 0;
        if (soldier.monsterTarget != null)
        {
            soldier.monsterTarget.SetSoldierTarget(soldier);
            soldier.LookAtDirection(soldier.monsterTarget.transform.position);
        }
    }

    public void UpdateState(Soldier soldier)
    {
        if (soldier.monsterTarget == null)
        {
            soldier.ChangeState(SoldierState.MovingFlag);
            return;
        }

        float distanceToTarget = Vector2.Distance(
            soldier.transform.position, 
            soldier.monsterTarget.transform.position
        );
        
        if (distanceToTarget > soldier.attackRange * 1.1f) 
        {
            soldier.ChangeState(SoldierState.Moving);
            return;
        }
        
        _attackTimer += Time.deltaTime;
        if (_attackTimer >= _attackCooldown)
        {
            if (soldier.unitSO != null && soldier.monsterTarget != null)
            {
                soldier.monsterTarget.TakeDamage(soldier.unitSO.attackDamage);
            }
            
            soldier.PlayAttackAnimation();
            _attackTimer = 0;
        }
    }

    public void ExitState(Soldier soldier)
    {
        if (soldier.monsterTarget != null)
        {
            soldier.monsterTarget.SetSoldierTarget(null);
        }
    }
}