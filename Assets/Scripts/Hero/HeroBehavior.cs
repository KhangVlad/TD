using System;
using UnityEngine;

public class HeroBehavior : MonoBehaviour
{
    [Header("Direction")]
    [SerializeField] private FacingDirection currentDirection = FacingDirection.None;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private Material mat;
    public Animator animator;
    public UnitID heroID;
    public HeroSO data;
    private Vector2 targetPosition;
    private bool isMoving = false;
    public readonly int MovingDown = Animator.StringToHash("Down");
    public readonly int MovingUp = Animator.StringToHash("Up");
    public readonly int MovingHorizon = Animator.StringToHash("Horizon");
    public readonly int AttackTrigger = Animator.StringToHash("Attack");
    public readonly int DieState = Animator.StringToHash("Die");
    public readonly int SkillTrigger = Animator.StringToHash("Skill");
    public readonly int IdleTrigger = Animator.StringToHash("Idle");
    public HeroMovingState moveState;
    public HeroAttackState attackState;
    public HeroIdleState idleState;
    private IHeroState currentState;

    private void Awake()
    {
        moveState = new HeroMovingState();
        attackState = new HeroAttackState();
        idleState = new HeroIdleState();
    }

    private void Start()
    {
        SetState(idleState);
    }

    private void Update()
    {
        if (currentState != null)
        {
            currentState.UpdateState(this);
        }
    }

    public void SetState(IHeroState newState)
    {
        if (currentState != null)
        {
            currentState.ExitState(this);
        }

        currentState = newState;

        if (currentState != null)
        {
            currentState.EnterState(this);
        }
    }

    public void SetTargetPosition(Vector2 position)
    {
        targetPosition = position;
        isMoving = true;
        Vector2 direction = new Vector2(
            targetPosition.x - transform.position.x,
            targetPosition.y - transform.position.y
        ).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        angle -= 90f;
        if (angle > 180f) angle -= 360f;
        if (angle < -180f) angle += 360f;
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
        mat.SetFloat("_OutlineEnabled", 0);
        UpdateAnimationDirectionFromAngle(angle);
    }

    public void MoveTowardsTarget()
    {
        if (!isMoving) return;
        transform.position = Vector2.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );
        // Check if we've reached the destination
        if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
        {
            isMoving = false;
            SetState(idleState);
        }
    }

    protected void UpdateAnimationDirectionFromAngle(float angle)
    {
        if (animator == null || !animator.runtimeAnimatorController)
        {
            Debug.LogWarning("Animator missing or has no controller assigned");
            return;
        }

        try
        {
            animator.ResetTrigger(MovingUp);
            animator.ResetTrigger(MovingDown);
            animator.ResetTrigger(MovingHorizon);
            if (angle >= -60f && angle <= 60f)
            {
                // Up direction
                animator.SetTrigger(MovingUp);
                currentDirection = FacingDirection.Up;
            }
            else if ((angle > 60f && angle < 150f) || (angle < -60f && angle > -150f))
            {
                animator.SetTrigger(MovingHorizon);
                if (transform.localScale.x < 0)
                {
                    currentDirection = FacingDirection.Left;
                }
                else
                {
                    currentDirection = FacingDirection.Right;
                }
            }
            else
            {
                animator.SetTrigger(MovingDown);
                currentDirection = FacingDirection.Down;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error setting animation trigger: {e.Message}");
        }
    }

    public void ListenClickEvent()
    {
        mat.SetFloat("_OutlineEnabled", 1);
    }
}

public interface IHeroState
{
    void EnterState(HeroBehavior soldier);
    void UpdateState(HeroBehavior soldier);
    void ExitState(HeroBehavior soldier);
}

public class HeroMovingState : IHeroState
{
    public void EnterState(HeroBehavior h)
    {
        // Start movement animation
        if (h.animator != null)
        {
            h.animator.SetBool("IsMoving", true);
        }
    }

    public void UpdateState(HeroBehavior h)
    {
        // Move towards target
        h.MoveTowardsTarget();
    }

    public void ExitState(HeroBehavior h)
    {
        // Stop movement animation
        if (h.animator != null)
        {
            h.animator.SetBool("IsMoving", false);
        }
    }
}

public class HeroAttackState : IHeroState
{
    public void EnterState(HeroBehavior h)
    {
        // Start attack animation
        if (h.animator != null)
        {
            h.animator.SetTrigger("Attack");
        }
    }

    public void UpdateState(HeroBehavior h)
    {
        // Handle attack logic
        // When attack animation is complete, you might want to return to idle state
    }

    public void ExitState(HeroBehavior h)
    {
        // Reset any attack-related variables
    }
}

public class HeroIdleState : IHeroState
{
    public void EnterState(HeroBehavior h)
    {
        h.animator.SetTrigger(h.IdleTrigger);
    }

    public void UpdateState(HeroBehavior h)
    {
        // Nothing much to do in idle state except wait for next command
    }

    public void ExitState(HeroBehavior h)
    {
        // Nothing specific to clean up
    }
}