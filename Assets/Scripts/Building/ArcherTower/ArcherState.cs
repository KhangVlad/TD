using UnityEngine;
public enum ArcherState
{
    Idle,
    Attacking
}

// Interface for Archer States
public interface IArcherState
{
    void EnterState(Archer archer);
    void UpdateState(Archer archer);
    void ExitState(Archer archer);
}


public class ArcherIdleState : IArcherState
{
    public void EnterState(Archer archer)
    {
        archer.PlayIdleAnimation();
    }

    public void UpdateState(Archer archer)
    {
     
    }

    public void ExitState(Archer archer)
    {
    }
}



public class ArcherAttackState : IArcherState
{
    private float _attackTimer = 0;

    public void EnterState(Archer archer)
    {
        _attackTimer = 0;
        if (archer.monsterTarget != null)
        {
            archer.monsterTarget.SetSoldierTarget(archer);
            archer.LookAtDirection(archer.monsterTarget.transform.position);
        }
    }

    public void UpdateState(Archer archer)
    {
        if (archer.monsterTarget == null)
        {
            archer.ChangeState(ArcherState.Idle);
            return;
        }
        else
        {
            _attackTimer += Time.deltaTime;
            if (_attackTimer >= archer.unitSO.attackSpeed)
            {
                // Play animation and fire arrow
                archer.PlayAttackAnimation();
                // archer.FireArrow();
                _attackTimer = 0;
                
            }   
        }
       
    }

    public void ExitState(Archer archer)
    {
        if (archer.monsterTarget != null)
        {
          
        }
    }
}