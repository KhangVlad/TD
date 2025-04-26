using System;
using UnityEngine;

// Base class for all units (Soldier, Archer, etc.)
public abstract class Unit : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] protected Animator anim;
    [SerializeField] protected SpriteRenderer unitSprite;

    [Header("Data and Stats")]
    public UnitSO unitSO;

    // Common state tracking
    public MonsterBase monsterTarget;

    // Combat stats
    public float attackRange;
    public float MovementSpeed { get; protected set; } = 2f;

    // UI
    protected UIHealthBar _healthBar;

    protected virtual void Start()
    {
        _healthBar = UIHealthBarManager.Instance?.CreateHealthBarForTarget(transform);
        InitializeStateMachine();
    }

    protected virtual void OnDestroy()
    {
        if (_healthBar != null && UIHealthBarManager.Instance != null)
        {
            UIHealthBarManager.Instance.ReleaseHealthBar(_healthBar);
        }
        if (monsterTarget != null)
        {
            CleanupMonsterTarget();
        }
    }

    protected virtual void Update()
    {
        UpdateStateMachine();
    }

    public virtual void Initialize(UnitSO data)
    {
        unitSO = data;
        if (data != null)
        {
            attackRange = data.attackRange;
            MovementSpeed = data.speed;
        }
    }

    public virtual void ChangeTarget(MonsterBase target)
    {
        MonsterBase old = monsterTarget;
        if(old != null)
        {
            CleanupMonsterTarget();
        }
        monsterTarget = target;
        HandleTargetChange(target);
    }

    protected abstract void HandleTargetChange(MonsterBase target);

    protected virtual void CleanupMonsterTarget()
    {
       
    }

    public virtual void LookAtDirection(Vector2 direction)
    {
        if (direction.x < 0)
            transform.localScale = new Vector3(-1, 1, 1); 
        else
            transform.localScale = new Vector3(1, 1, 1); 
    }
    public virtual void LookAtTarget(Vector2 targetPosition)
    {
        Vector2 direction = new Vector2(
            targetPosition.x - transform.position.x,
            targetPosition.y - transform.position.y
        ).normalized;
        if (direction.x < 0)
            transform.localScale = new Vector3(-1, 1, 1);
        else
            transform.localScale = new Vector3(1, 1, 1);  
    }
    protected abstract void InitializeStateMachine();
    protected abstract void UpdateStateMachine();

    #region Animation Controller

    public virtual void PlayAttackAnimation()
    {
        anim.SetTrigger("Attack");
      
    }

    public virtual void PlayRunAnimation(bool active)
    {
        anim.SetBool("Run", active);
    }
    
    public virtual void PlayIdleAnimation()
    {
        anim.SetTrigger("Idle");
    }

    #endregion
}

