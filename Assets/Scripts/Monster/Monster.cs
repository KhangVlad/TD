// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// public class Monster : MonoBehaviour
// {
//     public MonsterSO monsterSO { get; private set; }
//     
//     
//     public MonsterMovingState _movingState;
//     public PathCompleteState _pathCompleteState;
//
//
//     private void Awake()
//     {
//         _movingState = new MonsterMovingState();
//         _pathCompleteState = new PathCompleteState();
//     }
//
//     public int currentNodeIndex = 0;
//     public float Speed { get; private set; }
//     public float DetectSoldierRange = 1f;
//     
//     // Current state
//     public IMonsterState CurrentState { get; private set; }
//     
//     public void Initialize(MonsterSO so)
//     {
//         monsterSO = so;
//         Speed = so.speed;
//         MonsterManager.Instance.RegisterMonster(this);
//     }
//     
//     public void ChangeState(MonsterState newState)
//     {
//         // CurrentState?.ExitState(this);
//         // CurrentState = newState;
//         // CurrentState.EnterState(this);
//         switch (newState)
//         {
//             case MonsterState.Moving:
//                 CurrentState = _movingState;
//                 break;
//             case MonsterState.PathComplete:
//                 CurrentState = _pathCompleteState;
//                 break;
//             default:
//                 throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
//         }
//         CurrentState.EnterState(this);
//  
//         
//     }
//     
//     public void OnPathComplete()
//     {
//         Destroy(gameObject);
//     }
//     
//     private void OnDestroy()
//     {
//         MonsterManager.Instance.RemoveMonster(this);
//     }
// }
//
// // State interface - same as your original
// public interface IMonsterState
// {
//     void EnterState(Monster monster);
//     void UpdateState(Monster monster);
//     void ExitState(Monster monster);
// }
//
// public class MonsterMovingState : IMonsterState
// {
//     public void EnterState(Monster monster)
//     {
//    
//     }
//     
//     public void UpdateState(Monster monster)
//     {
//         if (MonsterManager.Instance.IsPathComplete(monster.currentNodeIndex))
//         {
//             monster.ChangeState(MonsterState.PathComplete);
//             return;
//         }
//         Vector3 targetPosition = MonsterManager.Instance.GetNodePosition(monster.currentNodeIndex);
//         monster.transform.position = Vector3.MoveTowards(
//             monster.transform.position, 
//             targetPosition, 
//             monster.Speed * Time.deltaTime);
//         if (Vector3.Distance(monster.transform.position, targetPosition) <= 0.04f)
//         {
//             monster.transform.position = targetPosition;
//             monster.currentNodeIndex++;
//             if (MonsterManager.Instance.IsPathComplete(monster.currentNodeIndex))
//             {
//                 monster.ChangeState(MonsterState.PathComplete);
//             }
//         }
//     }
//     
//     public void ExitState(Monster monster)
//     {
//     }
// }
//
// // Path complete state
// public class PathCompleteState : IMonsterState
// {
//     public void EnterState(Monster monster)
//     {
//         MonsterManager.Instance.RemoveMonster(monster);
//         monster.OnPathComplete();
//     }
//     
//     public void UpdateState(Monster monster)
//     {
//     }
//     
//     public void ExitState(Monster monster)
//     {
//     }
// }
//
// public enum MonsterState
// {
//     Moving,
//     Attack,
//     PathComplete
// }

using System;
using UnityEngine;

public class Monster : MonoBehaviour
{
    public MonsterSO monsterSO { get; private set; }
    public int currentNodeIndex = 0;
    public float Speed { get; private set; }
    public float attackRange  { get; private set; }

    public MonsterMovingState _movingState;
    public PathCompleteState _pathCompleteState;
    public MonsterAttackingState _attackState;
    public IMonsterState CurrentState { get; private set; }
    public bool showDetectionRadius = true;

    public Soldier targetSoldier;

    private void Awake()
    {
        _movingState = new MonsterMovingState();
        _pathCompleteState = new PathCompleteState();
        _attackState = new MonsterAttackingState();
    }

    private void Start()
    {
        ChangeState(MonsterState.Moving);
    }

    private void Update()
    {
        CurrentState?.UpdateState(this);
    }

    public void Initialize(MonsterSO so)
    {
        monsterSO = so;
        Speed = so.speed;
        attackRange = 0.2f;
        MonsterManager.Instance.RegisterMonster(this);
    }

    public void ChangeState(MonsterState newState)
    {
        CurrentState?.ExitState(this);
        switch (newState)
        {
            case MonsterState.Moving:
                CurrentState = _movingState;
                break;
            case MonsterState.Attack:
                CurrentState = _attackState;
                break;
            case MonsterState.PathComplete:
                CurrentState = _pathCompleteState;
                break;
            default:
                throw new System.ArgumentOutOfRangeException(nameof(newState), newState, null);
        }

        CurrentState.EnterState(this);
    }

    public void OnPathComplete()
    {
        Debug.Log("Monster reached the end of the path.");
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        MonsterManager.Instance.RemoveMonster(this);
    }


    private void OnDrawGizmos()
    {
        if (showDetectionRadius)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }


    // on enter collision trigger

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Soldier"))
        {
            Soldier soldier = other.GetComponent<Soldier>();
            if (soldier != null && targetSoldier == null)
            {
                targetSoldier = soldier;
            }
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Soldier"))
        {
            Soldier soldier = other.GetComponent<Soldier>();
            if (soldier != null)
            {
                targetSoldier = null;
            }
        }
    }
}


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
        if (monster.targetSoldier is not null)
        {
            monster.transform.position = Vector2.MoveTowards(
                monster.transform.position,
                monster.targetSoldier.transform.position+ new Vector3(0.1f, 0.1f,0),
                monster.Speed * Time.deltaTime);
            if (Vector2.Distance(monster.transform.position, monster.targetSoldier.transform.position) <= monster.attackRange)
            {
                monster.ChangeState(MonsterState.Attack);
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

            // Move toward target
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
        if (monster.targetSoldier == null)
        {
            monster.ChangeState(MonsterState.Moving);
            return;
        }
        if (Vector2.Distance(monster.transform.position, monster.targetSoldier.transform.position) <= monster.attackRange)
        {
            _attackTimer += Time.deltaTime;
            if (_attackTimer >= _attackCooldown)
            {
                _attackTimer = 0;
            }
        }
        
        if( (Vector2.Distance(monster.transform.position, monster.targetSoldier.transform.position) > monster.attackRange *1.5f))
        {
            monster.ChangeState(MonsterState.Moving);
        }
        
    }


    public void ExitState(Monster monster)
    {
        Debug.Log("Monster exited Attacking state");
    }
}

public class PathCompleteState : IMonsterState
{
    public void EnterState(Monster monster)
    {
        Debug.Log("Monster entered Path Complete state");
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