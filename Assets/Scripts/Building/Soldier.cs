using System;
using UnityEngine;


public class Soldier : MonoBehaviour 
{
    [Header( "Components")]
    [SerializeField] private Animator anim;
    [SerializeField] private SpriteRenderer soldierSprite;

    private ISoldierState currentState;
    public Monster monsterTarget;
    public Vector2 TargetPosition;
    public float attackRange { get; set; }
    
    public float MovementSpeed { get; set; } = 2f;
    public SoldierState State;
    private Vector2 offsetWithFlag;
    
    [Header( "State")]
    public IdleState idleState;
    public MovingState movingState;
    public AttackState attackState;
    public MovingFlagState movingFlagState;


    [Header("Data and Stats")] 
    public SoldierSO soldierSO;
    
    private Vector2 lastPosition;
   
    
    

    private void Awake()
    {
        idleState = new IdleState();
        movingState = new MovingState();
        attackState = new AttackState();
        movingFlagState = new MovingFlagState();
    }
    
   
    
    public void InitializeWithFlagPosition(Vector2 flagPosition, Vector2 offset, Barracks b,SoldierSO s)
    {
        offsetWithFlag = offset;
        attackRange = 0.2f;
        Vector2 targetPos = flagPosition + offset;
        TargetPosition = targetPos;
        soldierSO = s;
        ChangeState(SoldierState.MovingFlag);
        b.OnFlagPositionChanged += OnFlagPositionChanged;
    }
    
 
    private void Update()
    {
        currentState?.UpdateState(this);
        Vector2 currentPosition = transform.position;
        Vector2 movementDirection = currentPosition - lastPosition;
        
        // Only flip if there's significant movement
        if (movementDirection.magnitude > 0.01f)
        {
            if (movementDirection.x > 0)
            {
                soldierSprite.flipX = false; // Moving right
            }
            else if (movementDirection.x < 0)
            {
                soldierSprite.flipX = true; // Moving left
            }
        }
        
        // Store current position for next frame
        lastPosition = currentPosition;
    }


    #region State Machine

    public void ChangeState(SoldierState newState)
    {
        currentState?.ExitState(this);
        State = newState;
        switch ( newState)
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
        currentState.EnterState(this);
        
    }



    private void OnFlagPositionChanged(Vector2 newFlagPos)
    {
        Vector2 targetPos = newFlagPos + offsetWithFlag;
        TargetPosition = targetPos;
        ChangeState(SoldierState.MovingFlag);
    }

    #endregion


    #region Detect Enemy

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Monster mon = other.GetComponent<Monster>();
            if (mon != null && monsterTarget == null)
            {
                monsterTarget = mon;
            }
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Monster mon = other.GetComponent<Monster>();
            if (mon != null && monsterTarget == mon)
            {
                monsterTarget = null;
            }
        }
    }

    #endregion

    #region Animation Controller

    public void PlayAttackAnimation()
    {
        anim.SetTrigger("Attack");
        anim.SetFloat("Blend", UnityEngine.Random.Range(0f, 1f));
    }
    
    public void PlayRunAnimation(bool active)
    {
        anim.SetBool("Run", active);
    }
    
    public void PlayIdleAnimation()
    {
        anim.SetTrigger("Idle");
    }

    #endregion
   
}


public enum SoldierState
{
    Idle,
    Moving,
    MovingFlag,
    Attacking
    
}