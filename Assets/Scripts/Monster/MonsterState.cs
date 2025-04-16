using UnityEngine;
public interface IMonsterState
{
    void EnterState(Monster monster);
    void UpdateState(Monster monster);
    void ExitState(Monster monster);
}

// Moving state implementation
public class MonsterMovingState : IMonsterState
{
    private Vector3 targetNodePosition;
    private bool isTargetNodeCached = false;

    public void EnterState(Monster monster)
    {
        isTargetNodeCached = false;
    }

    public void UpdateState(Monster monster)
    {
        if (monster.targetSoldier != null)
        {
            if (Vector2.Distance(monster.transform.position, monster.targetSoldier.transform.position) <= monster.attackRange)
            {
                monster.ChangeState(MonsterState.Attack);
                return;
            }
            
            // Otherwise, move toward the soldier
            monster.transform.position = Vector2.MoveTowards(
                monster.transform.position,
                monster.targetSoldier.transform.position + new Vector3(0.1f, 0.1f, 0),
                monster.Speed * Time.deltaTime);
                
            // Check again if we've reached attack range
            if (Vector2.Distance(monster.transform.position, monster.targetSoldier.transform.position) <= monster.attackRange)
            {
                monster.ChangeState(MonsterState.Attack);
                return;
            }
        }
        else
        {
            if (MonsterManager.Instance.IsPathComplete(monster.currentNodeIndex))
            {
                monster.ChangeState(MonsterState.PathComplete);
                return;
            }

            if (!isTargetNodeCached)
            {
                targetNodePosition = MonsterManager.Instance.GetNodePosition(monster.currentNodeIndex);
                isTargetNodeCached = true;
            }

            // Move toward target node
            monster.transform.position = Vector3.MoveTowards(
                monster.transform.position,
                targetNodePosition,
                monster.Speed * Time.deltaTime);

            // Check if we've reached the target node
            Vector3 offset = monster.transform.position - targetNodePosition;
            float sqrDistance = offset.sqrMagnitude;

            if (sqrDistance <= 0.0016f) // This is 0.04 squared
            {
                monster.transform.position = targetNodePosition;
                monster.currentNodeIndex++;
                isTargetNodeCached = false; // Need to get new target

                if (MonsterManager.Instance.IsPathComplete(monster.currentNodeIndex))
                {
                    monster.ChangeState(MonsterState.PathComplete);
                }
            }
        }
    }

    public void ExitState(Monster monster)
    {
        // Nothing special needed when exiting moving state
    }
}

// Attacking state for when a soldier is detected
public class MonsterAttackingState : IMonsterState
{
    private float _attackTimer = 0;
    private float _attackCooldown = 1;

    public void EnterState(Monster monster)
    {
        _attackTimer = 0;
    }

    public void UpdateState(Monster monster)
    {
        if (monster.targetSoldier == null)
        {
            monster.ChangeState(MonsterState.Moving);
            return;
        }
        
        float distanceToSoldier = Vector2.Distance(monster.transform.position, monster.targetSoldier.transform.position);
        
        // If we're in attack range, attack on a timer
        if (distanceToSoldier <= monster.attackRange)
        {
            _attackTimer += Time.deltaTime;
            if (_attackTimer >= _attackCooldown)
            {
                // Attack the soldier (damage would be applied here)
                // TODO: Add soldier damage implementation
                
                // Play attack animation or effect here if available
                
                _attackTimer = 0;
            }
        }
        // If the soldier moved too far away, go back to chasing
        else if (distanceToSoldier > monster.attackRange * 1.5f)
        {
            monster.ChangeState(MonsterState.Moving);
        }
    }

    public void ExitState(Monster monster)
    {
        // Nothing special needed when exiting attack state
    }
}

public class PathCompleteState : IMonsterState
{
    public void EnterState(Monster monster)
    {
        // // Clean up any soldier references
        // if (monster.targetSoldier != null)
        // {
        //     monster.targetSoldier = null;
        // }
        //
        MonsterManager.Instance.RemoveMonster(monster);
        monster.OnPathComplete();
    }

    public void UpdateState(Monster monster)
    {
        // Nothing to update in this state as the monster will be destroyed
    }

    public void ExitState(Monster monster)
    {
        // This won't be called as the monster is destroyed in EnterState
    }
}

public enum MonsterState
{
    Moving,
    Attack,
    PathComplete
}