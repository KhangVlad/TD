using UnityEngine;
public interface IMonsterState
{
    void EnterState(MonsterBase monster);
    void UpdateState(MonsterBase monster);
    void ExitState(MonsterBase monster);
}
public class MonsterMovingState : IMonsterState
{
    private Vector3 targetNodePosition;
    private bool isTargetNodeCached = false;

    public void EnterState(MonsterBase monster)
    {
        isTargetNodeCached = false;
    }

    public void UpdateState(MonsterBase monster)
    {
        if (monster.targetSoldier != null)
        {
            // Look at soldier when chasing
            monster.LookAtTarget(monster.targetSoldier.transform.position);
            
            if (Vector2.Distance(monster.transform.position, monster.targetSoldier.transform.position) <= monster.attackRange)
            {
                monster.ChangeState(MonsterState.Attack);
                return;
            }
            
            monster.transform.position = Vector2.MoveTowards(
                monster.transform.position,
                monster.targetSoldier.transform.position + new Vector3(0.1f, 0.1f, 0),
                monster.Speed * Time.deltaTime);
                
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
                
                // Update orientation when getting a new target node
                monster.UpdatePathNode(targetNodePosition);
            }

            // Look at target while moving
            monster.LookAtTarget(targetNodePosition);

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

    public void ExitState(MonsterBase monster)
    {
    }
}

public class MonsterAttackingState : IMonsterState
{
    private float _attackTimer = 0;
    private float _attackCooldown = 1;
    private static readonly int Attack = Animator.StringToHash("Attack");

    public void EnterState(MonsterBase monster)
    {
        _attackTimer = 0;
    }

    public void UpdateState(MonsterBase monster)
    {
        if (monster.targetSoldier == null)
        {
            monster.ChangeState(MonsterState.Moving);
            return;
        }
        
        monster.LookAtTarget(monster.targetSoldier.transform.position);
        
        float distanceToSoldier = Vector2.Distance(monster.transform.position, monster.targetSoldier.transform.position);
        
        if (distanceToSoldier <= monster.attackRange)
        {
            _attackTimer += Time.deltaTime;
            if (_attackTimer >= _attackCooldown)
            {
                Animator animator = monster.GetComponent<Animator>();
                if (animator != null)
                {
                    animator.SetTrigger(Attack);
                }
                
                if (monster.targetSoldier != null)
                {
                }
                
                _attackTimer = 0;
            }
        }
        else if (distanceToSoldier > monster.attackRange * 1.5f)
        {
            monster.ChangeState(MonsterState.Moving);
        }
    }

    public void ExitState(MonsterBase monster)
    {
        // Reset attack animation if needed
        Animator animator = monster.GetComponent<Animator>();
        if (animator != null)
        {
            animator.ResetTrigger(Attack);
        }
    }
}

public class PathCompleteState : IMonsterState
{
    public void EnterState(MonsterBase monster)
    {
        monster.OnPathComplete();
    }

    public void UpdateState(MonsterBase monster)
    {
    }

    public void ExitState(MonsterBase monster)
    {
    }
}

public enum MonsterState
{
    Moving,
    Attack,
    PathComplete
}