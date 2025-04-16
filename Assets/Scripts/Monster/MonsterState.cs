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
        // If the monster already has a target and the target is valid, pursue it
        if (monster.targetSoldier != null && monster.targetSoldier.isActiveAndEnabled)
        {
            monster.transform.position = Vector2.MoveTowards(
                monster.transform.position,
                monster.targetSoldier.transform.position + new Vector3(0.1f, 0.1f, 0),
                monster.Speed * Time.deltaTime);
                
            if (Vector2.Distance(monster.transform.position, monster.targetSoldier.transform.position) <= monster.attackRange)
            {
                monster.ChangeState(MonsterState.Attack);
            }
        }
        else
        {
            // No target, follow the path
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

            // Use squared distance for better performance
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
       
    }
}

// Attacking state for when a soldier is detected
public class MonsterAttackingState : IMonsterState
{
    private float _attackTimer = 0;
    private float _attackCooldown = 1;

    public void EnterState(Monster monster)
    {
    }

    public void UpdateState(Monster monster)
    {
        if (monster.targetSoldier == null || !monster.targetSoldier.isActiveAndEnabled)
        {
            monster.targetSoldier = null;
            monster.ChangeState(MonsterState.Moving);
            return;
        }
        
        if (Vector2.Distance(monster.transform.position, monster.targetSoldier.transform.position) <= monster.attackRange)
        {
            _attackTimer += Time.deltaTime;
            if (_attackTimer >= _attackCooldown)
            {
                // Attack the soldier
                monster.Attack(monster.targetSoldier);
                _attackTimer = 0;
            }
        }
        else if (Vector2.Distance(monster.transform.position, monster.targetSoldier.transform.position) > monster.attackRange * 1.5f)
        {
            monster.ChangeState(MonsterState.Moving);
        }
    }

    public void ExitState(Monster monster)
    {
    }
}

public class PathCompleteState : IMonsterState
{
    public void EnterState(Monster monster)
    {
        Debug.Log("Monster entered Path Complete state");
        MonsterManager.Instance.OnMonsterReachedEnd(monster);
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