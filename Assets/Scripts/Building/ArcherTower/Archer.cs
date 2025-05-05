using System;
using UnityEngine;

public class Archer : Unit
{
    [Header("Archer State")] public ArcherIdleState idleState;
    public ArcherAttackState attackState;
    private IArcherState currentState;
    public ArcherState State;

    [Header("Archer-specific Components")] [SerializeField]
    private Transform arrowSpawnPoint;

    [Header("Miss Chance Settings")] [Range(0f, 1f)] [SerializeField]
    private float missChance = 0.5f; // 20% chance to miss by default

    [SerializeField] private float missOffsetRange = 1.5f; // How far off target a miss can be
    [SerializeField] private float brokenArrowLifetime = 10f; // How long broken arrows remain

    // Archer-specific properties
    public float arrowSpeed = 10f;
    public float arrowLifetime = 3f;

    private void Awake()
    {
        idleState = new ArcherIdleState();
        attackState = new ArcherAttackState();
    }

    protected override void Start()
    {
        base.Start();
    }

    public void Initialize(ArcherSO data)
    {
        base.Initialize(data);
        if (data != null)
        {
            arrowSpeed = data.arrowSpeed;
            arrowLifetime = data.arrowLifetime;
            // You could also add missChance to your ArcherSO if desired
        }

        ChangeState(ArcherState.Idle);
    }

    protected override void CleanupMonsterTarget()
    {
        if (monsterTarget != null)
        {
            // monsterTarget.SetSoldierTarget(null);
        }
    }

    protected override void HandleTargetChange(MonsterBase target)
    {
        if (target != null)
        {
            ChangeState(ArcherState.Attacking);
        }
        else
        {
            ChangeState(ArcherState.Idle);
        }
    }

    protected override void InitializeStateMachine()
    {
        ChangeState(ArcherState.Idle);
    }

    protected override void UpdateStateMachine()
    {
        currentState?.UpdateState(this);
    }

    public void ChangeState(ArcherState newState)
    {
        // Exit the current state
        currentState?.ExitState(this);
        State = newState;

        switch (newState)
        {
            case ArcherState.Idle:
                currentState = idleState;
                break;
            case ArcherState.Attacking:
                currentState = attackState;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }

        // Enter the new state
        currentState.EnterState(this);
    }

    public void PlayAttackAnimation()
    {
        if (monsterTarget != null)
        {
            Vector2 direction = (monsterTarget.transform.position - transform.position).normalized;
            LookAtDirection(direction);
            anim.SetFloat("DirX", direction.x);
            anim.SetFloat("DirY", direction.y);
            anim.SetTrigger("Attack");
        }
    }

    public void PlayRunAnimation(bool active)
    {
        anim.SetBool("Run", active);
    }

    public void PlayIdleAnimation()
    {
        anim.SetTrigger("Idle");
    }

    public void FireArrow()
    {
        if (monsterTarget == null || ProjectileManager.Instance == null)
        {
            return;
        }

        float damage = 10f;
        if (unitSO != null)
        {
            damage = unitSO.attackDamage;
        }
        else
        {
            Debug.LogWarning("unitSO is null, using default damage value");
        }

        // Check if the shot will miss
        bool isMiss = UnityEngine.Random.value < missChance;
        Vector3 targetPosition;
        Transform targetTransform = monsterTarget.transform;
        if (isMiss)
        {
            Vector3 offsetPos = monsterTarget.transform.position + new Vector3(
                UnityEngine.Random.Range(-missOffsetRange, missOffsetRange),
                UnityEngine.Random.Range(-missOffsetRange, missOffsetRange),
                0
            );

            Vector3 missPosition = offsetPos;
            damage = 0;

            // Fire the arrow at the monster anyway (we'll handle the miss visually)
            Projectile arrow = ProjectileManager.Instance.FireProjectile(
                ProjectileID.Arrow,
                arrowSpawnPoint.position,
                targetTransform,
                damage,
                arrowSpeed,
                arrowLifetime
            );

            if (arrow != null)
            {
            }
        }
        else
        {
            Projectile arrow = ProjectileManager.Instance.FireProjectile(
                ProjectileID.Arrow,
                arrowSpawnPoint.position,
                targetTransform,
                damage,
                arrowSpeed,
                arrowLifetime
            );
        }
    }

    public void SetMissChance(float chance)
    {
        missChance = Mathf.Clamp01(chance);
    }
}