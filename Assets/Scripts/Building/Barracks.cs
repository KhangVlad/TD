using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class Barracks : BaseTower
{
    public event Action<Vector2> OnFlagPositionChanged;

    [Header("Barracks Settings")]
    [SerializeField] private int maxSoldiers = 3;
    [SerializeField] private float spawnTime = 5f;
    [SerializeField] private GameObject flagPrefab;
    [SerializeField] private GameObject _flagInstance;
    [SerializeField] private TowerSO towerData;
    [SerializeField] private float _detectRange = 1f;
    [SerializeField] private float flagAnimationDuration = 0.5f;
    
    [Header("Flag Area")]
    [SerializeField] private GameObject flagAreaCollider;
    [SerializeField] private float _activityRange = 1.5f;
    
    [SerializeField] private SoldierWithTarget[] _soldiers = new SoldierWithTarget[3];
    public List<Monster> monstersInArea = new List<Monster>();
    private Coroutine _flagCoroutine;

    [Header("Debug")]
    [SerializeField] private int _soldierCount = 0;
    [SerializeField] private float _spawnTimer = 0f;
    private Vector2 _flagPosition;

    private Vector2[] triangleOffsets = new Vector2[]
    {
        new Vector2(0f, 0.49f),
        new Vector2(-0.42f, -0.2555f),
        new Vector2(0.42f, -0.2555f),
    };

    protected override void Start()
    {
        base.Start();
        InitializeFlagCollider();
    }

    private void InitializeFlagCollider()
    {
        // Create flag area collider if it doesn't exist
        if (flagAreaCollider == null)
        {
            flagAreaCollider = new GameObject("FlagAreaCollider");
            flagAreaCollider.transform.parent = transform;
            CircleCollider2D collider = flagAreaCollider.AddComponent<CircleCollider2D>();
            collider.radius = _detectRange;
            collider.isTrigger = true;
            
            Rigidbody2D rb = flagAreaCollider.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.simulated = true;
            
            flagAreaCollider.AddComponent<FlagAreaTrigger>();
        }
    }

    public void InitializeTower(TowerSO data)
    {
        towerData = data;
        _flagPosition = transform.position + new Vector3(0, 0.7f, 0);
        _soldierCount = 0;
        _spawnTimer = 0;
        UpdateFlagColliderPosition();
    }

    protected override void Update()
    {
        base.Update();
        
        // Spawn soldiers if needed
        if (_soldierCount < maxSoldiers)
        {
            _spawnTimer += Time.deltaTime;

            if (_spawnTimer >= spawnTime)
            {
                _spawnTimer = 0f;
                SpawnSoldier();
            }
        }

        // Update soldier count (to handle destroyed soldiers)
        UpdateSoldierCount();
        
        // Always check and update targeting
        UpdateSoldierTargets();
    }

    private void UpdateSoldierCount()
    {
        // Count active soldiers and clean up null references
        int activeCount = 0;
        for (int i = 0; i < _soldiers.Length; i++)
        {
            if (_soldiers[i] != null && _soldiers[i].soldier != null)
            {
                activeCount++;
            }
            else
            {
                _soldiers[i] = null;
            }
        }
        
        _soldierCount = activeCount;
    }

    private void UpdateSoldierTargets()
    {
        // Clean up any destroyed monsters from the list
        for (int i = monstersInArea.Count - 1; i >= 0; i--)
        {
            if (monstersInArea[i] == null)
            {
                monstersInArea.RemoveAt(i);
            }
        }
        
        // If no monsters in area, clear all soldier targets
        if (monstersInArea.Count == 0)
        {
            foreach (var soldierWithTarget in _soldiers)
            {
                if (soldierWithTarget != null && soldierWithTarget.soldier != null)
                {
                    if (soldierWithTarget.monster != null)
                    {
                        // Clear target
                        soldierWithTarget.monster = null;
                        soldierWithTarget.soldier.SetTarget(null);
                    }
                }
            }
            return;
        }
        
        // Map monsters to the soldiers targeting them
        Dictionary<Monster, List<SoldierWithTarget>> monsterToSoldiers = new Dictionary<Monster, List<SoldierWithTarget>>();
        List<SoldierWithTarget> soldiersNeedingTargets = new List<SoldierWithTarget>();
        
        // Count active soldiers
        int activeSoldierCount = 0;
        
        // Process all soldiers and their current targets
        foreach (var soldierWithTarget in _soldiers)
        {
            if (soldierWithTarget != null && soldierWithTarget.soldier != null)
            {
                activeSoldierCount++;
                
                if (soldierWithTarget.monster != null)
                {
                    // Is the monster still valid and in the area?
                    if (monstersInArea.Contains(soldierWithTarget.monster))
                    {
                        // Add to our tracking dictionary
                        if (!monsterToSoldiers.ContainsKey(soldierWithTarget.monster))
                        {
                            monsterToSoldiers[soldierWithTarget.monster] = new List<SoldierWithTarget>();
                        }
                        monsterToSoldiers[soldierWithTarget.monster].Add(soldierWithTarget);
                    }
                    else
                    {
                        // Monster is gone - clear target
                        soldierWithTarget.monster = null;
                        soldierWithTarget.soldier.SetTarget(null);
                        soldiersNeedingTargets.Add(soldierWithTarget);
                    }
                }
                else
                {
                    // Soldier needs a target
                    soldiersNeedingTargets.Add(soldierWithTarget);
                }
            }
        }
        
        // Find untargeted monsters
        List<Monster> untargetedMonsters = new List<Monster>();
        foreach (var monster in monstersInArea)
        {
            if (!monsterToSoldiers.ContainsKey(monster))
            {
                untargetedMonsters.Add(monster);
            }
        }
        
        // Check if we need to rebalance targets (when all soldiers target one monster and new monsters arrive)
        bool needRebalance = untargetedMonsters.Count > 0 && // New monsters available
                           monsterToSoldiers.Count == 1 && // All soldiers on same monster
                           activeSoldierCount > 1; // More than one soldier
        
        if (needRebalance)
        {
            // Get the currently targeted monster
            Monster currentTarget = null;
            foreach (var monster in monsterToSoldiers.Keys)
            {
                currentTarget = monster;
                break;
            }
            
            // Calculate ideal distribution
            int totalMonsters = monstersInArea.Count;
            int soldiersPerMonster = activeSoldierCount / totalMonsters;
            int remainder = activeSoldierCount % totalMonsters;
            
            // How many soldiers should stay with current target
            int soldiersToKeep = soldiersPerMonster;
            if (remainder > 0)
            {
                soldiersToKeep++; // First monster gets one extra
                remainder--;
            }
            
            // Move excess soldiers to untargeted monsters
            int soldiersToMove = monsterToSoldiers[currentTarget].Count - soldiersToKeep;
            
            if (soldiersToMove > 0)
            {
                // Sort soldiers by distance to their current target (move furthest first)
                monsterToSoldiers[currentTarget].Sort((a, b) => {
                    float distA = Vector2.Distance(a.soldier.transform.position, currentTarget.transform.position);
                    float distB = Vector2.Distance(b.soldier.transform.position, currentTarget.transform.position);
                    return distB.CompareTo(distA); // Sort descending (furthest first)
                });
                
                // Reassign furthest soldiers to new monsters
                int monsterIndex = 0;
                for (int i = 0; i < soldiersToMove && i < untargetedMonsters.Count; i++)
                {
                    SoldierWithTarget soldierToMove = monsterToSoldiers[currentTarget][i];
                    
                    // Clear old target
                    soldierToMove.monster = null;
                    soldierToMove.soldier.SetTarget(null);
                    
                    // Assign to new monster
                    Monster newTarget = untargetedMonsters[monsterIndex];
                    soldierToMove.monster = newTarget;
                    soldierToMove.soldier.SetTarget(newTarget);
                    
                    // Setup tracking for this new assignment
                    if (!monsterToSoldiers.ContainsKey(newTarget))
                    {
                        monsterToSoldiers[newTarget] = new List<SoldierWithTarget>();
                    }
                    monsterToSoldiers[newTarget].Add(soldierToMove);
                    
                    // Move to next monster with wrap-around
                    monsterIndex = (monsterIndex + 1) % untargetedMonsters.Count;
                }
                
                // Remove reassigned soldiers from original target's list
                if (soldiersToMove > 0)
                {
                    monsterToSoldiers[currentTarget].RemoveRange(0, Math.Min(soldiersToMove, monsterToSoldiers[currentTarget].Count));
                }
                
                // Update untargeted monsters
                untargetedMonsters.Clear();
                foreach (var monster in monstersInArea)
                {
                    if (!monsterToSoldiers.ContainsKey(monster))
                    {
                        untargetedMonsters.Add(monster);
                    }
                }
            }
        }
        
        // Handle any soldiers that still need targets
        if (soldiersNeedingTargets.Count > 0)
        {
            // Try to assign to untargeted monsters first
            for (int i = 0; i < soldiersNeedingTargets.Count && untargetedMonsters.Count > 0; i++)
            {
                // Pick the first untargeted monster
                Monster target = untargetedMonsters[0];
                
                // Assign soldier to this monster
                soldiersNeedingTargets[i].monster = target;
                soldiersNeedingTargets[i].soldier.SetTarget(target);
                
                // Update our tracking
                if (!monsterToSoldiers.ContainsKey(target))
                {
                    monsterToSoldiers[target] = new List<SoldierWithTarget>();
                }
                monsterToSoldiers[target].Add(soldiersNeedingTargets[i]);
                
                // Remove from untargeted list
                untargetedMonsters.RemoveAt(0);
            }
            
            // If we still have soldiers needing targets, assign to monsters with fewest soldiers
            if (soldiersNeedingTargets.Count > 0 && monstersInArea.Count > 0)
            {
                // Sort monsters by how many soldiers are targeting them
                List<Monster> sortedMonsters = new List<Monster>(monstersInArea);
                sortedMonsters.Sort((a, b) => {
                    int countA = monsterToSoldiers.ContainsKey(a) ? monsterToSoldiers[a].Count : 0;
                    int countB = monsterToSoldiers.ContainsKey(b) ? monsterToSoldiers[b].Count : 0;
                    return countA.CompareTo(countB); // Sort ascending (fewest targets first)
                });
                
                // Start assigning from first soldier that still needs a target
                int startIndex = soldiersNeedingTargets.Count - untargetedMonsters.Count;
                if (startIndex < 0) startIndex = 0;
                
                for (int i = startIndex; i < soldiersNeedingTargets.Count; i++)
                {
                    // Get monster with fewest soldiers
                    Monster target = sortedMonsters[0];
                    
                    // Assign soldier to this monster
                    soldiersNeedingTargets[i].monster = target;
                    soldiersNeedingTargets[i].soldier.SetTarget(target);
                    
                    // Update our tracking
                    if (!monsterToSoldiers.ContainsKey(target))
                    {
                        monsterToSoldiers[target] = new List<SoldierWithTarget>();
                    }
                    monsterToSoldiers[target].Add(soldiersNeedingTargets[i]);
                    
                    // Re-sort the list
                    sortedMonsters.Sort((a, b) => {
                        int countA = monsterToSoldiers.ContainsKey(a) ? monsterToSoldiers[a].Count : 0;
                        int countB = monsterToSoldiers.ContainsKey(b) ? monsterToSoldiers[b].Count : 0;
                        return countA.CompareTo(countB);
                    });
                }
            }
        }
    }

    private void SpawnSoldier()
    {
        if (_soldierCount >= maxSoldiers)
            return;
            
        // Find empty slot in soldiers array
        int slotIndex = -1;
        for (int i = 0; i < _soldiers.Length; i++)
        {
            if (_soldiers[i] == null || _soldiers[i].soldier == null)
            {
                slotIndex = i;
                break;
            }
        }
        
        if (slotIndex == -1)
            return;
            
        int formationIndex = slotIndex % triangleOffsets.Length;
        GameObject soldierObj = Instantiate(towerData.soldierPrefab, transform.position, Quaternion.identity);
        Soldier soldier = soldierObj.GetComponent<Soldier>();
        
        _soldiers[slotIndex] = new SoldierWithTarget(soldier);
        soldier.InitializeWithFlagPosition(_flagPosition, triangleOffsets[formationIndex], this, towerData.soldierData);
        
        // Increment soldier count
        _soldierCount++;
        
        // Target assignment will happen in the next UpdateSoldierTargets() call
    }

    public override void Upgrade()
    {
        base.Upgrade();
        if (towerData != null)
        {
            maxSoldiers = towerData.maxUnits;
            spawnTime = towerData.spawnTime;
        }
    }

    private void ChangeFlagPosition(Vector2 newPosition)
    {
        // Check if the position is within the allowed range
        if (Vector2.Distance(transform.position, newPosition) > _activityRange)
            return;

        _flagPosition = newPosition;
        
        // Update flag collider position
        UpdateFlagColliderPosition();
        
        // Notify soldiers about the flag position change
        OnFlagPositionChanged?.Invoke(newPosition);
    }

    private void UpdateFlagColliderPosition()
    {
        if (flagAreaCollider != null)
        {
            flagAreaCollider.transform.position = _flagPosition;
        }
    }

    public void OnFlagPlaced(Vector2 position)
    {
        ChangeFlagPosition(position);
        StartCoroutine(AnimateFlagAppearance(position));
    }

    private IEnumerator AnimateFlagAppearance(Vector2 mousePos)
    {
        if (_flagCoroutine != null)
        {
            StopCoroutine(_flagCoroutine);
        }

        if (_flagInstance == null)
        {
            _flagInstance = Instantiate(flagPrefab, mousePos, Quaternion.identity);
        }
        else
        {
            _flagInstance.transform.position = mousePos;
            _flagInstance.SetActive(true);
        }

        SpriteRenderer spriteRenderer = _flagInstance.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            // Fade in
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);
            yield return spriteRenderer.DOFade(1f, flagAnimationDuration).WaitForCompletion();

            // Hold
            yield return new WaitForSeconds(flagAnimationDuration);

            // Fade out
            yield return spriteRenderer.DOFade(0f, flagAnimationDuration).WaitForCompletion();
        }

        _flagInstance.SetActive(false);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(_flagPosition, _detectRange);
    }
    
    // Method to add a monster to the area (called by FlagAreaTrigger)
    public void AddMonsterToArea(Monster monster)
    {
        if (monster != null && !monstersInArea.Contains(monster))
        {
            monstersInArea.Add(monster);
            
            // Immediately recalculate targets when a new monster enters the area
            UpdateSoldierTargets();
        }
    }
    
    // Method to remove a monster from the area (called by FlagAreaTrigger)
    public void RemoveMonsterFromArea(Monster monster)
    {
        if (monster != null)
        {
            monstersInArea.Remove(monster);
            
            // Clear this monster from any soldiers targeting it
            foreach (var soldierWithTarget in _soldiers)
            {
                if (soldierWithTarget != null && soldierWithTarget.monster == monster)
                {
                    soldierWithTarget.monster = null;
                    if (soldierWithTarget.soldier != null)
                    {
                        soldierWithTarget.soldier.SetTarget(null);
                    }
                }
            }
            
            // Target reassignment will happen in the next Update via UpdateSoldierTargets
        }
    }
}

[Serializable]
public class SoldierWithTarget
{
    public Soldier soldier;
    public Monster monster;

    public SoldierWithTarget(Soldier s)
    {
        this.soldier = s;
        this.monster = null;
    }
}

// Separate component to handle trigger events
public class FlagAreaTrigger : MonoBehaviour
{
    private Barracks _barracks;
    
    private void Awake()
    {
        // Get the parent Barracks component
        _barracks = GetComponentInParent<Barracks>();
        if (_barracks == null)
        {
            Debug.LogError("FlagAreaTrigger must be a child of a Barracks object!");
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Monster monster = other.GetComponent<Monster>();
            if (monster != null && _barracks != null)
            {
                _barracks.AddMonsterToArea(monster);
            }
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Monster monster = other.GetComponent<Monster>();
            if (monster != null && _barracks != null)
            {
                _barracks.RemoveMonsterFromArea(monster);
            }
        }
    }
}