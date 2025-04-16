using System;
using UnityEngine;

public interface ISoldierState
{
    void EnterState(Soldier soldier);
    void UpdateState(Soldier soldier);
    void ExitState(Soldier soldier);
}

// Idle state - Soldier standing at flag position
public class IdleState : ISoldierState
{
    public void EnterState(Soldier soldier)
    {
        soldier.PlayIdleAnimation();
    }

    public void UpdateState(Soldier soldier)
    {
        // Check for monsters nearby - this is now handled in OnTriggerEnter2D
        // If the soldier gets a target while idle, it will change states through that mechanism
    }

    public void ExitState(Soldier soldier)
    {
    }
}

// Moving state - Soldier moving toward a monster
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
        if (soldier.monsterTarget == null || !soldier.monsterTarget.isActiveAndEnabled)
        {
            soldier.monsterTarget = null;
            soldier.isEngaged = false;
            soldier.ChangeState(SoldierState.MovingFlag);
            return;
        }

        // Move toward the target monster
        soldier.transform.position = Vector2.MoveTowards(
            soldier.transform.position,
            soldier.monsterTarget.transform.position,
            soldier.MovementSpeed * Time.deltaTime
        );
        
        // Look at the monster
        soldier.LookAtDirection(soldier.monsterTarget.transform.position);
        
        // Check if in attack range
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

// Moving to flag state - Soldier returning to flag position
public class MovingFlagState : ISoldierState
{
    public void EnterState(Soldier soldier)
    {
        soldier.PlayRunAnimation(true);
        soldier.LookAtDirection(soldier.flagPos);
    }

    public void UpdateState(Soldier soldier)
    {
        // If soldier has been assigned a target while moving, pursue it instead
        if (soldier.monsterTarget != null && soldier.isEngaged)
        {
            soldier.ChangeState(SoldierState.Moving);
            return;
        }
        
        // Move towards the flag position
        soldier.transform.position = Vector2.MoveTowards(
            soldier.transform.position,
            soldier.flagPos,
            soldier.MovementSpeed * Time.deltaTime
        );
        
        // Check if the soldier has reached the target position
        if (Vector2.Distance(soldier.transform.position, soldier.flagPos) <= 0.1f)
        {
            soldier.ChangeState(SoldierState.Idle);
        }
    }

    public void ExitState(Soldier soldier)
    {
        soldier.PlayRunAnimation(false);
    }
}

// Attack state - Soldier attacking a monster
public class AttackState : ISoldierState
{
    private float _attackTimer = 0;
    private float _attackCooldown = 1.5f;

    public void EnterState(Soldier soldier)
    {
        _attackTimer = _attackCooldown; 
        soldier.LookAtDirection(soldier.monsterTarget.transform.position);
    }

    public void UpdateState(Soldier soldier)
    {
        if (soldier.monsterTarget == null || !soldier.monsterTarget.isActiveAndEnabled)
        {
            soldier.monsterTarget = null;
            soldier.isEngaged = false;
            soldier.ChangeState(SoldierState.MovingFlag);
            return;
        }
        
        
        // Attack timer
        _attackTimer += Time.deltaTime;
        if (_attackTimer >= _attackCooldown)
        {
            // Perform attack
            if (soldier.monsterTarget != null)
            {
                soldier.monsterTarget.TakeDamage(soldier.soldierSO.damage);
                soldier.PlayAttackAnimation();
                _attackTimer = 0;
            }
        }
        
        // Check if target moved out of range
        // Check if target still exists before calculating distance
        if (soldier.monsterTarget != null && soldier.monsterTarget.isActiveAndEnabled)
        {
            // Check if target moved out of range
            if (Vector2.Distance(soldier.transform.position, soldier.monsterTarget.transform.position) > soldier.attackRange * 1.2f)
            {
                soldier.ChangeState(SoldierState.Moving);
            }
        }
        else
        {
            // Target is gone, go back to flag
            soldier.monsterTarget = null;
            soldier.isEngaged = false;
            soldier.ChangeState(SoldierState.MovingFlag);
        }
    }

    public void ExitState(Soldier soldier)
    {
    }
}