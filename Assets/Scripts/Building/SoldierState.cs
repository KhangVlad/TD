using System;
using UnityEngine;

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
        soldier.LookAtDirection( soldier.monsterTarget.transform.position);
    }

    public void UpdateState(Soldier soldier)
    {
        if (soldier.monsterTarget is not null)
        {
            soldier.transform.position = Vector2.MoveTowards(
                soldier.transform.position,
                soldier.monsterTarget.transform.position,
                soldier.MovementSpeed * Time.deltaTime
            );
            if (Vector2.Distance(soldier.transform.position, soldier.monsterTarget.transform.position) <=
                soldier.attackRange)
            {
                soldier.ChangeState(SoldierState.Attacking);
            }
        }
    }

    public void ExitState(Soldier soldier)
    {
        soldier.PlayRunAnimation(false);
    }
}

public class MovingFlagState : ISoldierState
{
    private bool reachTargetPosition = false;
    public void EnterState(Soldier soldier)
    {
        soldier.PlayRunAnimation(true);
        soldier.LookAtDirection(soldier.flagPos);
    }

    public void UpdateState(Soldier soldier)
    {
        // Move towards the flag position
        soldier.transform.position = Vector2.MoveTowards(
            soldier.transform.position,
            soldier.flagPos,
            soldier.MovementSpeed * Time.deltaTime
        );

        // Check if the soldier has reached the target position
        if (Vector2.Distance(soldier.transform.position, soldier.flagPos) <= 0.1f)
        {
            reachTargetPosition = true;
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
    }

    public void UpdateState(Soldier soldier)
    {
        if (soldier.monsterTarget is null)
        {
            soldier.ChangeState(SoldierState.MovingFlag);
        }
        else
        {
            _attackTimer += Time.deltaTime;
            if (_attackTimer >= _attackCooldown)
            {
                soldier.monsterTarget.TakeDamage(soldier.soldierSO.damage);
                soldier.PlayAttackAnimation();
                _attackTimer = 0;
            }
        }
    }

    public void ExitState(Soldier soldier)
    {
    }
}