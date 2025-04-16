using System;
using UnityEngine;

public class Soldier : MonoBehaviour
{
    [Header("Components")] [SerializeField]
    private Animator anim;

    [SerializeField] private SpriteRenderer soldierSprite;

    [Header("State")] public IdleState idleState;
    public MovingState movingState;
    public AttackState attackState;
    public MovingFlagState movingFlagState;

    [Header("Data and Stats")] public SoldierSO soldierSO;

    // State tracking
    private ISoldierState currentState;
    public SoldierState State { get; private set; }
    public Monster monsterTarget { get; private set; }

    // Position and movement data
    public Vector2 flagPos;
    private Vector2 offsetWithFlag;
    private Barracks parentBarracks;

    // Stats
    public float attackRange { get; private set; }
    public float MovementSpeed { get; private set; } = 2f;

    // UI
    private UIHealthBar _healthBar;

    private void Awake()
    {
        idleState = new IdleState();
        movingState = new MovingState();
        attackState = new AttackState();
        movingFlagState = new MovingFlagState();
    }

    private void Start()
    {
        _healthBar = UIHealthBarManager.Instance?.CreateHealthBarForTarget(transform);
    }

    private void OnDestroy()
    {
        // Clean up health bar
        if (_healthBar != null && UIHealthBarManager.Instance != null)
        {
            UIHealthBarManager.Instance.ReleaseHealthBar(_healthBar);
        }

        // Unregister from barracks
        if (parentBarracks != null)
        {
            parentBarracks.OnFlagPositionChanged -= OnFlagPositionChanged;
        }
    }

    public void InitializeWithFlagPosition(Vector2 flagPosition, Vector2 offset, Barracks barracks, SoldierSO data)
    {
        // Set references
        parentBarracks = barracks;
        soldierSO = data;
        offsetWithFlag = offset;

        // Set position data
        flagPos = flagPosition + offset;

        // Set stats from scriptable object
        if (data != null)
        {
            attackRange = data.attackRange;
            MovementSpeed = data.speed;
        }
        else
        {
            // Default values if data is missing
            attackRange = 0.2f;
            MovementSpeed = 2f;
        }

        // Register for flag updates
        barracks.OnFlagPositionChanged += OnFlagPositionChanged;

        // Start in MovingFlag state to get to position
        ChangeState(SoldierState.MovingFlag);
    }

    private void Update()
    {
        currentState?.UpdateState(this);
    }

    public void SetTarget(Monster target)
    {
        if (monsterTarget != null && monsterTarget != target)
        {
            monsterTarget.SetSoldierTarget(null);
        }

        monsterTarget = target;
        if (target != null)
        {
            monsterTarget.SetSoldierTarget(this);
        }

        if (target != null)
        {
            ChangeState(SoldierState.Moving);
        }
    }

    public void LookAtDirection(Vector2 targetPosition)
    {
        if (targetPosition.x < transform.position.x)
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

        // Enter the new state
        currentState.EnterState(this);
    }

    // Flag position change handler
    private void OnFlagPositionChanged(Vector2 newFlagPos)
    {
        // Update flag position
        flagPos = newFlagPos + offsetWithFlag;

        // Go to new position (priority over combat)
        ChangeState(SoldierState.MovingFlag);
    }

    #endregion

    #region Animation Controller

    public void PlayAttackAnimation()
    {
        if (anim != null)
        {
            anim.SetTrigger("Attack");
            anim.SetFloat("Blend", UnityEngine.Random.Range(0f, 1f));
        }
    }

    public void PlayRunAnimation(bool active)
    {
        if (anim != null)
        {
            anim.SetBool("Run", active);
        }
    }

    public void PlayIdleAnimation()
    {
        if (anim != null)
        {
            anim.SetTrigger("Idle");
        }
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