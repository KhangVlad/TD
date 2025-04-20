// using System;
// using UnityEngine;
//
// public class Soldier : Unit
// {
//     [Header("Components")]
//     [SerializeField] private Animator anim;
//     [SerializeField] private SpriteRenderer soldierSprite;
//
//     [Header("State")]
//     public IdleState idleState;
//     public MovingState movingState;
//     public AttackState attackState;
//     public MovingFlagState movingFlagState;
//     private ISoldierState currentState;
//
//     [Header("Data and Stats")]
//     public SoldierSO soldierSO;
//
//     // State tracking
//   
//     public SoldierState State;
//     public Monster monsterTarget;
//
//     // Position and movement data
//     public Vector2 flagPos;
//     private Vector2 offsetWithFlag;
//     private Tower parentBarracks;
//
//     // Stats
//     public float attackRange;
//     public float MovementSpeed { get; private set; } = 2f;
//
//     // UI
//     private UIHealthBar _healthBar;
//
//     private void Awake()
//     {
//         idleState = new IdleState();
//         movingState = new MovingState();
//         attackState = new AttackState();
//         movingFlagState = new MovingFlagState();
//     }
//
//     private void Start()
//     {
//         _healthBar = UIHealthBarManager.Instance?.CreateHealthBarForTarget(transform);
//     }
//
//     private void OnDestroy()
//     {
//         // Clean up health bar
//         if (_healthBar != null && UIHealthBarManager.Instance != null)
//         {
//             UIHealthBarManager.Instance.ReleaseHealthBar(_healthBar);
//         }
//
//         // Make sure monster no longer follows this soldier
//         if (monsterTarget != null)
//         {
//             monsterTarget.SetSoldierTarget(null);
//         }
//
//         // Unregister from barracks
//         if (parentBarracks != null)
//         {
//             parentBarracks.OnFlagPositionChanged -= OnFlagPositionChanged;
//         }
//     }
//
//     public void InitializeWithFlagPosition(Vector2 flagPosition, Vector2 offset, Tower barracks, SoldierSO data)
//     {
//         // Set references
//         parentBarracks = barracks;
//         soldierSO = data;
//         offsetWithFlag = offset;
//         flagPos = flagPosition + offset;
//         if (data != null)
//         {
//             attackRange = data.attackRange;
//             MovementSpeed = data.speed;
//         }
//         barracks.OnFlagPositionChanged += OnFlagPositionChanged;
//         ChangeState(SoldierState.MovingFlag);
//     }
//
//     private void Update()
//     {
//         currentState?.UpdateState(this);
//     }
//
//     public void ChangeTarget(Monster target)
//     {
//         Monster old = monsterTarget;
//         if(old!=null)
//         {
//             old.SetSoldierTarget(null);
//         }
//         monsterTarget = target;
//         if (target != null)
//         {
//             ChangeState(SoldierState.Moving);
//         }
//         else if (State != SoldierState.MovingFlag)
//         {
//             ChangeState(SoldierState.MovingFlag);
//         }
//     }
//     protected override void HandleTargetChange(Monster target)
//     {
//         if (target != null)
//         {
//             ChangeState(SoldierState.Moving);
//         }
//         else
//         {
//             ChangeState(SoldierState.Idle);
//         }
//     }
//     protected override void InitializeStateMachine()
//     {
//         // Initial state
//         ChangeState(SoldierState.Idle);
//     }
//     
//     protected override void UpdateStateMachine()
//     {
//         currentState?.UpdateState(this);
//     }
//
//     public void LookAtDirection(Vector2 targetPosition)
//     {
//         if (targetPosition.x < transform.position.x)
//             soldierSprite.flipX = true;
//         else
//             soldierSprite.flipX = false;
//     }
//
//     #region State Machine
//
//     public void ChangeState(SoldierState newState)
//     {
//         // Before exiting current state, ensure we handle monster targeting correctly
//         if (State == SoldierState.Moving || State == SoldierState.Attacking)
//         {
//             if (newState == SoldierState.MovingFlag && monsterTarget != null)
//             {
//             
//             }
//         }
//
//         // Exit the current state
//         currentState?.ExitState(this);
//         State = newState;
//
//         switch (newState)
//         {
//             case SoldierState.Idle:
//                 currentState = idleState;
//                 break;
//             case SoldierState.Moving:
//                 currentState = movingState;
//                 break;
//             case SoldierState.Attacking:
//                 currentState = attackState;
//                 break;
//             case SoldierState.MovingFlag:
//                 currentState = movingFlagState;
//                 break;
//             default:
//                 throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
//         }
//
//         // Enter the new state
//         currentState.EnterState(this);
//     }
//
//     // Flag position change handler
//     private void OnFlagPositionChanged(Vector2 newFlagPos)
//     {
//         // Update flag position
//         flagPos = newFlagPos + offsetWithFlag;
//         ChangeState(SoldierState.MovingFlag);
//     }
//
//     #endregion
//
//     #region Animation Controller
//
//     public void PlayAttackAnimation()
//     {
//         anim.SetTrigger("Attack");
//         anim.SetFloat("Blend", UnityEngine.Random.Range(0f, 1f));
//     }
//
//     public void PlayRunAnimation(bool active)
//     {
//         anim.SetBool("Run", active);
//     }
//     
//     public void PlayIdleAnimation()
//     {
//         anim.SetTrigger("Idle");
//     }
//
//     #endregion
// }
//
// public enum SoldierState
// {
//     Idle,
//     Moving,
//     MovingFlag,
//     Attacking
// }
//
//
//


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
        // Additional soldier-specific setup
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

    protected override void HandleTargetChange(Monster target)
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
        // Initial state
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
        // Update flag position
        flagPos = newFlagPos + offsetWithFlag;
        ChangeState(SoldierState.MovingFlag);
    }
}


public enum SoldierState
{
    Idle,
    Moving,
    MovingFlag,
    Attacking
}