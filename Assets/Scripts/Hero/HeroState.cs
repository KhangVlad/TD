using UnityEngine;

// State interface for Hero
public interface IHeroState
{
    void EnterState(HeroBehavior hero);
    void UpdateState(HeroBehavior hero);
    void ExitState(HeroBehavior hero);
}

// Enum to track current hero state
public enum HeroState
{
    Idle,
    MovingToMonster,
    MovingToFlag,
    Attack
}

// Idle state implementation
public class HeroIdleState : IHeroState
{
    public void EnterState(HeroBehavior hero)
    {
        hero.PlayIdleAnimation();
    }

    public void UpdateState(HeroBehavior hero)
    {
        // Check if there are any monsters in detection area to target
        MonsterBase newTarget = hero.FindClosestMonster();
        if (newTarget != null)
        {
            hero.ChangeTarget(newTarget);
            hero.ChangeState(HeroState.MovingToMonster);
            return;
        }
    }

    public void ExitState(HeroBehavior hero)
    {
        // Nothing specific to clean up in idle state
    }
}

// Moving to monster state implementation
public class HeroMovingToMonsterState : IHeroState
{
    public void EnterState(HeroBehavior hero)
    {
        // Set the appropriate animation for movement
        Vector2 direction = hero.monsterTarget != null
            ? (Vector2)(hero.monsterTarget.transform.position - hero.transform.position).normalized
            : Vector2.zero;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        angle -= 90f;
        if (angle > 180f) angle -= 360f;
        if (angle < -180f) angle += 360f;

        hero.UpdateAnimationDirectionFromAngle(angle);

        if (hero.monsterTarget != null)
        {
            hero.LookAtTarget(hero.monsterTarget.transform.position);
        }
    }

    public void UpdateState(HeroBehavior hero)
    {
        if (hero.monsterTarget == null)
        {
            // Target is gone, find a new one or go back to flag/idle
            MonsterBase newTarget = hero.FindClosestMonster();
            if (newTarget != null)
            {
                hero.ChangeTarget(newTarget);
                return; // Keep moving to the new target
            }
            else if (hero.hasFlagTarget) // Assuming this is a property you can add
            {
                hero.ChangeState(HeroState.MovingToFlag);
                return;
            }
            else
            {
                hero.ChangeState(HeroState.Idle);
                return;
            }
        }

        // Move towards the target
        hero.transform.position = Vector2.MoveTowards(
            hero.transform.position,
            hero.monsterTarget.transform.position,
            hero.MovementSpeed * Time.deltaTime
        );

        // Check if in attack range
        if (hero.IsInAttackRange(hero.monsterTarget))
        {
            hero.ChangeState(HeroState.Attack);
        }
    }

    public void ExitState(HeroBehavior hero)
    {
        // Nothing specific to clean up
    }
}

// Moving to flag state implementation
public class HeroMovingToFlagState : IHeroState
{
    private const float ARRIVAL_THRESHOLD = 0.04f;

    public void EnterState(HeroBehavior hero)
    {
        // Already handling animation in SetTargetPosition
        // But we need to ensure the movement animation continues
        Vector2 direction = hero.targetPosition - (Vector2)hero.transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        angle -= 90f;
        if (angle > 180f) angle -= 360f;
        if (angle < -180f) angle += 360f;
        hero.LookAtDirection(direction);
        hero.UpdateAnimationDirectionFromAngle(angle);
    }

    public void UpdateState(HeroBehavior hero)
    {
       
        // Move towards flag position
        hero.transform.position = Vector2.MoveTowards(
            hero.transform.position,
            hero.targetPosition,
            hero.MovementSpeed * Time.deltaTime
        );

        // Check if we've arrived at the flag
        if (Vector2.Distance(hero.transform.position, hero.targetPosition) <= ARRIVAL_THRESHOLD)
        {
            hero.ChangeState(HeroState.Idle);
        }
    }

    public void ExitState(HeroBehavior hero)
    {
        // Nothing specific to clean up
    }
}

// Attack state implementation
public class HeroAttackState : IHeroState
{
    private float _attackTimer = 0;

    public void EnterState(HeroBehavior hero)
    {
        _attackTimer = 0;

        if (hero.monsterTarget != null)
        {
            // Look at the monster and play attack animation
            hero.LookAtTarget(hero.monsterTarget.transform.position);
            hero.PlayAttackAnimation();
            hero.monsterTarget.SetSoldierTarget(hero);
        }
    }

    public void UpdateState(HeroBehavior hero)
    {
        if (hero.monsterTarget == null)
        {
            // Target is gone, find a new one or go back to flag/idle
            MonsterBase newTarget = hero.FindClosestMonster();
            if (newTarget != null)
            {
                hero.ChangeTarget(newTarget);
                hero.ChangeState(HeroState.MovingToMonster);
                return;
            }
            else if (hero.hasFlagTarget)
            {
                hero.ChangeState(HeroState.MovingToFlag);
            }
            else
            {
                hero.ChangeState(HeroState.Idle);
            }

            return;
        }

        // Check if target moved out of range
        if (!hero.IsInAttackRange(hero.monsterTarget))
        {
            hero.ChangeState(HeroState.MovingToMonster);
            return;
        }

        // Handle attack timing
        _attackTimer += Time.deltaTime;
        if (_attackTimer >= hero.heroData.attackSpeed) // Using heroData from HeroBehavior
        {
            if (hero.heroData != null && hero.monsterTarget != null)
            {
                hero.monsterTarget.TakeDamage(hero.heroData.attackDamage);
            }

            hero.PlayAttackAnimation();
            _attackTimer = 0;
        }
    }

    public void ExitState(HeroBehavior hero)
    {
        if (hero.monsterTarget != null)
        {
            hero.monsterTarget.SetSoldierTarget(null);
        }
    }
}