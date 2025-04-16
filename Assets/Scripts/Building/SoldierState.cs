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
        // If we have a target assigned by the barracks, start moving to engage
        if (soldier.monsterTarget != null)
        {
            soldier.ChangeState(SoldierState.Moving);
        }
    }

    public void ExitState(Soldier soldier)
    {
        // Nothing special to do when leaving idle state
    }
}

// Moving state - soldier moves toward the targeted monster
public class MovingState : ISoldierState
{
    public void EnterState(Soldier soldier)
    {
        soldier.PlayRunAnimation(true);
        
        // Update direction immediately
        if (soldier.monsterTarget != null)
        {
            soldier.LookAtDirection(soldier.monsterTarget.transform.position);
        }
    }

    public void UpdateState(Soldier soldier)
    {
        // If target is lost, return to flag position
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

// MovingFlag state - soldier returns to flag position
public class MovingFlagState : ISoldierState
{
    public void EnterState(Soldier soldier)
    {
        soldier.PlayRunAnimation(true);
        soldier.LookAtDirection(soldier.flagPos);
    }

    public void UpdateState(Soldier soldier)
    {
        
        soldier.transform.position = Vector2.MoveTowards(
            soldier.transform.position,
            soldier.flagPos,
            soldier.MovementSpeed * Time.deltaTime
        );

        if (Vector2.Distance(soldier.transform.position, soldier.flagPos) <= 0.1f)//close enough to flag, 
        {
            if (soldier.monsterTarget != null)
            {
                soldier.ChangeState(SoldierState.Moving);
            }
            else
            {
                soldier.ChangeState(SoldierState.Idle);
            }
        }
    }

    public void ExitState(Soldier soldier)
    {
        soldier.PlayRunAnimation(false);
    }
}

// Attack state - soldier attacks the monster
public class AttackState : ISoldierState
{
    private float _attackTimer = 0;
    private float _attackCooldown = 1.5f;

    public void EnterState(Soldier soldier)
    {
        _attackTimer = 0;
        if (soldier.monsterTarget != null)
        {
            soldier.LookAtDirection(soldier.monsterTarget.transform.position);
        }
    }

    public void UpdateState(Soldier soldier)
    {
        // If target is lost, return to flag position
        if (soldier.monsterTarget == null)
        {
            soldier.ChangeState(SoldierState.MovingFlag);
            return;
        }

        float distanceToTarget = Vector2.Distance(
            soldier.transform.position, 
            soldier.monsterTarget.transform.position
        );
        
        // If target moved out of range, chase it
        if (distanceToTarget > soldier.attackRange * 1.1f)  // Small buffer to prevent oscillation
        {
            soldier.ChangeState(SoldierState.Moving);
            return;
        }
        
        _attackTimer += Time.deltaTime;
        if (_attackTimer >= _attackCooldown)
        {
            // Deal damage
            if (soldier.soldierSO != null && soldier.monsterTarget != null)
            {
                soldier.monsterTarget.TakeDamage(soldier.soldierSO.damage);
            }
            
            // Play attack animation
            soldier.PlayAttackAnimation();
            
            // Reset timer
            _attackTimer = 0;
        }
    }

    public void ExitState(Soldier soldier)
    {
    }
}