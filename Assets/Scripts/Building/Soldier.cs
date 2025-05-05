
using System;
using UnityEngine;

public class Soldier : Unit
{
    [Header("Soldier State")]
    public IdleState idleState;
    public MovingState movingState;
    public AttackState attackState;
    public MovingFlagState movingFlagState;
    public ISoldierState currentState;
    public SoldierState State;

    // Position and movement data
    public Vector2 flagPos;
    private Vector2 offsetWithFlag;
    private Tower parentBarracks;

    private void Awake()
    {
        idleState = new IdleState();
        movingState = new MovingState();
        attackState = new AttackState();
        movingFlagState = new MovingFlagState();
    }

    protected override void Start()
    {
        base.Start();
    }

    public void InitializeWithFlagPosition(Vector2 flagPosition, Vector2 offset, Tower barracks, SoldierSO data)
    {
        base.Initialize(data);
        
        parentBarracks = barracks;
        offsetWithFlag = offset;
        flagPos = flagPosition + offset;
        
        barracks.OnFlagPositionChanged += OnFlagPositionChanged;
        ChangeState(SoldierState.MovingFlag);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        // Unregister from barracks
        if (parentBarracks != null)
        {
            parentBarracks.OnFlagPositionChanged -= OnFlagPositionChanged;
        }
    }

    protected override void CleanupMonsterTarget()
    {
        if (monsterTarget != null)
        {
            monsterTarget.SetSoldierTarget(null);
        }
    }

    protected override void HandleTargetChange(MonsterBase target)
    {
        if (target != null)
        {
            ChangeState(SoldierState.Moving);
        }
        else if (State != SoldierState.MovingFlag)
        {
            ChangeState(SoldierState.MovingFlag);
        }
    }

    protected override void InitializeStateMachine()
    {
        ChangeState(SoldierState.MovingFlag);
    }

    protected override void UpdateStateMachine()
    {
        currentState?.UpdateState(this);
    }

    public void ChangeState(SoldierState newState)
    {
        // Exit the current state
        currentState?.ExitState(this);
        State = newState;

        switch (newState)
        {
            case SoldierState.Idle:
                currentState = idleState;
                break;
            case SoldierState.Moving:
                currentState = movingState;
                break;
            case SoldierState.Attacking:
                currentState = attackState;
                break;
            case SoldierState.MovingFlag:
                currentState = movingFlagState;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }

        // Enter the new state
        currentState.EnterState(this);
    }

    // Flag position change handler
    private void OnFlagPositionChanged(Vector2 newFlagPos)
    {
        flagPos = newFlagPos + offsetWithFlag;
        ChangeState(SoldierState.MovingFlag);
    }
    
    public  void PlayAttackAnimation()  
    {
        anim.SetFloat("Blend", UnityEngine.Random.Range(0f, 1f));
        anim.SetTrigger("Attack");
    }
    


    public  void PlayRunAnimation(bool active)
    {
        anim.SetBool("Run", active);
    }
    
    public  void PlayIdleAnimation()
    {
        anim.SetTrigger("Idle");
    }
}


public enum SoldierState
{
    Idle,
    Moving,
    MovingFlag,
    Attacking
}