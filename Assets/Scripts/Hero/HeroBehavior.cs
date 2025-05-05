using System;
using System.Collections.Generic;
using UnityEngine;

public class HeroBehavior : Unit, FlagAreaTrigger.IFlagAreaListener
{
    #region Fields
    [Header("Direction")] 
    [SerializeField] private FacingDirection currentDirection = FacingDirection.None;
    [SerializeField] private Material outlineMaterial;

    #region Animation Hash IDs
    public readonly int MovingDown = Animator.StringToHash("Down");
    public readonly int MovingUp = Animator.StringToHash("Up");
    public readonly int MovingHorizon = Animator.StringToHash("Horizon");
    public readonly int AttackTrigger = Animator.StringToHash("Attack");
    public readonly int DieState = Animator.StringToHash("Die");
    public readonly int SkillTrigger = Animator.StringToHash("Skill");
    public readonly int IdleTrigger = Animator.StringToHash("Idle");
    #endregion

    #region State Management
    public HeroState state;
    private IHeroState currentState;
    public HeroMovingToFlagState moveFlagState;
    public HeroAttackState attackState;
    public HeroIdleState idleState;
    public HeroMovingToMonsterState moveToMonsterState;
    #endregion

    #region Hero Properties
    public Vector2 targetPosition;
    private bool isMoving = false;
    public bool hasFlagTarget = false;
    #endregion

    #region Enemy Detection
    protected FlagAreaTrigger flagAreaDetector;
    public List<MonsterBase> monstersInDetectionArea = new List<MonsterBase>();
    public HeroSO heroData;
    #endregion
    #endregion

    #region Unity Lifecycle
    protected override void Awake()
    {
        base.Awake();
        InitializeStateObjects();
    }

    protected override void Start()
    {
        base.Start();
        ChangeState(HeroState.Idle);
    }

    protected override void Update()
    {
        base.Update();
    }
    #endregion

    #region Initialization
    private void InitializeStateObjects()
    {
        moveFlagState = new HeroMovingToFlagState();
        attackState = new HeroAttackState();
        idleState = new HeroIdleState();
        moveToMonsterState = new HeroMovingToMonsterState();
    }

    public override void Initialize(UnitSO data)
    {
        base.Initialize(data);

        if (data is HeroSO heroSO)
        {
            heroData = heroSO;
            id = heroSO.unitID;
            InitializeFlagCollider();
        }
    }

    protected void InitializeFlagCollider()
    {
        if (flagAreaDetector != null)
        {
            return;
        }

        var detectionObject = new GameObject("FlagAreaDetector");
        detectionObject.transform.parent = transform;
        detectionObject.transform.position = transform.position;

        CircleCollider2D collider = detectionObject.AddComponent<CircleCollider2D>();
        collider.radius = heroData.detectionRadius;
        collider.isTrigger = true;

        Rigidbody2D rb = detectionObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.simulated = true;

        FlagAreaTrigger trigger = detectionObject.AddComponent<FlagAreaTrigger>();
        flagAreaDetector = trigger;
        flagAreaDetector.Initialize(this);
    }
    #endregion

    #region State Management
    public void ChangeState(HeroState newState)
    {
        if (state == newState && currentState != null)
            return;

        currentState?.ExitState(this);
        state = newState;

        switch (newState)
        {
            case HeroState.Idle:
                currentState = idleState;
                break;
            case HeroState.MovingToMonster:
                currentState = moveToMonsterState;
                break;
            case HeroState.MovingToFlag:
                currentState = moveFlagState;
                break;
            case HeroState.Attack:
                currentState = attackState;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }

        currentState.EnterState(this);
    }

    protected override void InitializeStateMachine()
    {
        ChangeState(HeroState.Idle);
    }

    protected override void UpdateStateMachine()
    {
        currentState?.UpdateState(this);
    }
    #endregion

    #region IFlagAreaListener Implementation
    public void AddMonsterToArea(MonsterBase monster)
    {
        if (!monstersInDetectionArea.Contains(monster))
        {
            monstersInDetectionArea.Add(monster);
        }

        if (state == HeroState.Idle)
        {
            ChangeTarget(monster);
            ChangeState(HeroState.MovingToMonster);
        }
    }

    public void RemoveMonsterFromArea(MonsterBase monster)
    {
        if (monster != null)
        {
            monstersInDetectionArea.Remove(monster);
        }

        if (monster == monsterTarget)
        {
            monsterTarget = null;

            if (state == HeroState.Attack || state == HeroState.MovingToMonster)
            {
                MonsterBase newTarget = FindClosestMonster();
                if (newTarget != null)
                {
                    ChangeTarget(newTarget);
                    ChangeState(HeroState.MovingToMonster);
                }
                else if (hasFlagTarget)
                {
                    ChangeState(HeroState.MovingToFlag);
                }
                else
                {
                    ChangeState(HeroState.Idle);
                }
            }
        }
    }
    #endregion

    #region Target Management
    public MonsterBase FindClosestMonster()
    {
        if (monstersInDetectionArea.Count == 0) return null;

        MonsterBase closest = null;
        float closestDistance = float.MaxValue;

        foreach (var monster in monstersInDetectionArea)
        {
            if (monster == null) continue;

            float distance = Vector2.Distance(transform.position, monster.transform.position);
            if (distance < closestDistance)
            {
                closest = monster;
                closestDistance = distance;
            }
        }

        return closest;
    }

    protected override void HandleTargetChange(MonsterBase target)
    {
        if (target != null && state == HeroState.Idle)
        {
            ChangeState(HeroState.MovingToMonster);
        }
    }

    protected override void CleanupMonsterTarget()
    {
        base.CleanupMonsterTarget();
    }

    public void SetTargetPosition(Vector2 position)
    {
        targetPosition = position;
        isMoving = true;
        hasFlagTarget = true;
        
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        if (direction.x < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);

            if (currentDirection == FacingDirection.Right)
            {
                currentDirection = FacingDirection.Left;
            }
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);
            if (currentDirection == FacingDirection.Left)
            {
                currentDirection = FacingDirection.Right;
            }
        }
        float angle = CalculateDirectionAngle(direction);
        outlineMaterial.SetFloat("_OutlineEnabled", 0);
        UpdateAnimationDirectionFromAngle(angle);
        ChangeState(HeroState.MovingToFlag);
    }
    #endregion

    #region Animation
    private float CalculateDirectionAngle(Vector2 direction)
    {
       
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        angle -= 90f;
        if (angle > 180f) angle -= 360f;
        if (angle < -180f) angle += 360f;
        return angle;
    }

    public void UpdateAnimationDirectionFromAngle(float angle)
    {
        ResetMovementTriggers();
        
        if (angle >= -60f && angle <= 60f)
        {
            anim.SetTrigger(MovingUp);
            currentDirection = FacingDirection.Up;
        }
        else if ((angle > 60f && angle <= 150f) || (angle < -60f && angle >= -150f))
        {
            anim.SetTrigger(MovingHorizon);
            currentDirection = transform.localScale.x < 0 ? FacingDirection.Left : FacingDirection.Right;
        }
        else
        {
            anim.SetTrigger(MovingDown);
            currentDirection = FacingDirection.Down;
        }
    }

    private void ResetMovementTriggers()
    {
        anim.ResetTrigger(MovingUp);
        anim.ResetTrigger(MovingDown);
        anim.ResetTrigger(MovingHorizon);
    }

    public void PlayAttackAnimation()
    {
        anim.SetTrigger(AttackTrigger);
    }

    public void PlayIdleAnimation()
    {
        anim.SetTrigger(IdleTrigger);
    }
    #endregion

    #region UI Interaction
    public void OnSelected()
    {
        outlineMaterial.SetFloat("_OutlineEnabled", 1);
    }
    #endregion

    #region Combat
    public bool IsInAttackRange(MonsterBase target)
    {
        if (target == null) return false;
        return Vector2.Distance(transform.position, target.transform.position) <= attackRange;
    }
    #endregion
}