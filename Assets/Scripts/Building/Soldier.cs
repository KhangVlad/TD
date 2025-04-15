using System;
using UnityEngine;
using UnityEngine.Serialization;


public class Soldier : MonoBehaviour
{
    [Header("Components")] [SerializeField]
    private Animator anim;

    [SerializeField] private SpriteRenderer soldierSprite;

    private ISoldierState currentState;
    public Monster monsterTarget;
    public Vector2 flagPos;
    public float attackRange { get; set; }
    public float MovementSpeed { get; set; } = 2f;
    public SoldierState State;
    private Vector2 offsetWithFlag;
    private UIHealthBar _healthBar;

    [Header("State")] public IdleState idleState;
    public MovingState movingState;
    public AttackState attackState;
    public MovingFlagState movingFlagState;


    [Header("Data and Stats")] public SoldierSO soldierSO;


    private void Awake()
    {
        idleState = new IdleState();
        movingState = new MovingState();
        attackState = new AttackState();
        movingFlagState = new MovingFlagState();
    }

    private void Start()
    {
        _healthBar = UIHealthBarManager.Instance.CreateHealthBarForTarget(transform);
    }
    private void OnDestroy()
    {
        if (_healthBar != null)
        {
            UIHealthBarManager.Instance.ReleaseHealthBar( _healthBar);
        }
    }


    public void InitializeWithFlagPosition(Vector2 flagPosition, Vector2 offset, Barracks b, SoldierSO s)
    {
        offsetWithFlag = offset;
        attackRange = 0.2f;
        Vector2 targetPos = flagPosition + offset;
        flagPos = targetPos;
        soldierSO = s;
        ChangeState(SoldierState.MovingFlag);
        b.OnFlagPositionChanged += OnFlagPositionChanged;
    }


    private void Update()
    {
        currentState?.UpdateState(this);
    }

    public void LookAtDirection(Vector2 dir)
    {
        if (dir.x < transform.position.x)
            soldierSprite.flipX = true;
        else
            soldierSprite.flipX = false;
    }


    #region State Machine

    public void ChangeState(SoldierState newState)
    {
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

        currentState.EnterState(this);
    }


    private void OnFlagPositionChanged(Vector2 newFlagPos)
    {
        Vector2 targetPos = newFlagPos + offsetWithFlag;
        flagPos = targetPos;
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