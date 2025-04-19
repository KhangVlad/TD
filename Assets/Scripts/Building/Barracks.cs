// using UnityEngine;
// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.Linq;
// using DG.Tweening;
//
// public class Barracks : BaseTower
// {
//     public event Action<Vector2> OnFlagPositionChanged;
//
//     [Header("Barracks Settings")] [SerializeField]
//     private int maxSoldiers = 3;
//
//     [SerializeField] private float spawnTime = 5f;
//     [SerializeField] private GameObject flagPrefab;
//     [SerializeField] private TowerSO towerData;
//     [SerializeField] private float _detectRange = 1f;
//     [SerializeField] private float flagAnimationDuration = 0.5f;
//
//     [Header("Flag Area")] 
//     [SerializeField] private float _activityRange = 1.5f;
//     private readonly Vector2[] _triangleOffsets = new Vector2[]
//     {
//         new Vector2(0f, 0.49f),
//         new Vector2(-0.42f, -0.2555f),
//         new Vector2(0.42f, -0.2555f),
//     };
//
//     // Private fields
//     private GameObject _flagInstance;
//     private GameObject _flagAreaCollider;
//     [SerializeField]private SoldierWithTarget[] _soldiers;
//     private List<Monster> _monstersInArea = new List<Monster>();
//     private Coroutine _flagCoroutine;
//     private Vector2 _flagPosition;
//     private float _spawnTimer = 0f;
//     private int _soldierCount = 0;
//
//     #region Unity Lifecycle
//
//     protected override void Start()
//     {
//         base.Start();
//         _soldiers = new SoldierWithTarget[maxSoldiers];
//         InitializeFlagCollider();
//     }
//
//     protected override void Update()
//     {
//         base.Update();
//         TrySpawnSoldier();
//     }
//
//     private void OnDrawGizmos()
//     {
//         if (_flagPosition != Vector2.zero)
//         {
//             Gizmos.color = Color.green;
//             Gizmos.DrawWireSphere(_flagPosition, _detectRange);
//         }
//     }
//
//     #endregion
//
//     #region Tower Initialization
//
//     private void InitializeFlagCollider()
//     {
//         if (_flagAreaCollider != null)
//         {
//             return;
//         }
//
//         _flagAreaCollider = new GameObject("FlagAreaCollider");
//         _flagAreaCollider.transform.parent = transform;
//
//         CircleCollider2D collider = _flagAreaCollider.AddComponent<CircleCollider2D>();
//         collider.radius = _detectRange;
//         collider.isTrigger = true;
//
//         Rigidbody2D rb = _flagAreaCollider.AddComponent<Rigidbody2D>();
//         rb.bodyType = RigidbodyType2D.Kinematic;
//         rb.simulated = true;
//         FlagAreaTrigger trigger = _flagAreaCollider.AddComponent<FlagAreaTrigger>();
//     }
//
//     public void InitializeTower(TowerSO data)
//     {
//         towerData = data;
//
//         // Set initial flag position above the barracks
//         _flagPosition = transform.position + new Vector3(0, 0.7f, 0);
//         _soldierCount = 0;
//         _spawnTimer = 0;
//
//         UpdateFlagColliderPosition();
//
//         // Initialize soldier array with proper size
//         _soldiers = new SoldierWithTarget[data.maxUnits];
//     }
//
//     #endregion
//
//     #region Soldier Management
//
//     private void TrySpawnSoldier()
//     {
//         if (_soldierCount >= maxSoldiers)
//         {
//             return;
//         }
//         _spawnTimer += Time.deltaTime;
//
//         if (_spawnTimer >= spawnTime)
//         {
//             _spawnTimer = 0f;
//             SpawnSoldier();
//         }
//     }
//
//     private void SpawnSoldier()
//     {
//         if (_soldierCount >= maxSoldiers)
//         {
//             return;
//         }
//
//         int slotIndex = -1;
//         for (int i = 0; i < _soldiers.Length; i++)
//         {
//             if (_soldiers[i] == null || _soldiers[i].soldier == null)
//             {
//                 slotIndex = i;
//                 break;
//             }
//         }
//
//         if (slotIndex == -1)
//         {
//             return;
//         }
//
//         // Determine formation position
//         int formationIndex = slotIndex % _triangleOffsets.Length;
//
//         // Instantiate soldier
//         GameObject soldierObj = Instantiate(towerData.soldierPrefab, transform.position, Quaternion.identity);
//         Soldier soldier = soldierObj.GetComponent<Soldier>();
//
//         // Initialize soldier and add to our array
//         _soldiers[slotIndex] = new SoldierWithTarget(soldier);
//         soldier.InitializeWithFlagPosition(_flagPosition, _triangleOffsets[formationIndex], this,
//             towerData.soldierData);
//
//         // Update soldier count
//         _soldierCount++;
//
//         // Try to assign a target immediately if monsters are present
//         if (_monstersInArea.Count > 0)
//         {
//             UpdateSoldierTargets(null);
//         }
//     }
//
//     private void UpdateSoldierCount()
//     {
//         int activeCount = 0;
//
//         for (int i = 0; i < _soldiers.Length; i++)
//         {
//             if (_soldiers[i]?.soldier != null)
//             {
//                 activeCount++;
//             }
//             else
//             {
//                 _soldiers[i] = null;
//             }
//         }
//
//         _soldierCount = activeCount;
//     }
//
//     #endregion
//
//     #region Monster Targeting
//
//     private void UpdateSoldierTargets(Monster entranceMonster)
//     {
//         // Remove null monsters from the list
//         _monstersInArea.RemoveAll(monster => monster == null);
//
//         // Update soldier count and exit if no monsters or soldiers
//         UpdateSoldierCount();
//         if (_monstersInArea.Count == 0 || _soldierCount == 0)
//         {
//             return;
//         }
//
//         Dictionary<Monster, List<SoldierWithTarget>> monsterToSoldiers =
//             new Dictionary<Monster, List<SoldierWithTarget>>();
//         List<SoldierWithTarget> soldiersNeedingTargets = new List<SoldierWithTarget>();
//
//         // First pass: categorize soldiers by their current target
//         ProcessCurrentTargets(monsterToSoldiers, soldiersNeedingTargets);
//
//         // Find untargeted monsters
//         List<Monster> untargetedMonsters = _monstersInArea
//             .Where(monster => !monsterToSoldiers.ContainsKey(monster))
//             .ToList();
//
//         // If we have multiple monsters, rebalance soldiers
//         if (_monstersInArea.Count > 1)
//         {
//             RebalanceSoldiers(monsterToSoldiers, untargetedMonsters);
//         }
//
//         // Assign any soldiers that still need targets
//         AssignRemainingTargets(soldiersNeedingTargets, monsterToSoldiers, untargetedMonsters, entranceMonster);
//     }
//
//     private void ProcessCurrentTargets(
//         Dictionary<Monster, List<SoldierWithTarget>> monsterToSoldiers,
//         List<SoldierWithTarget> soldiersNeedingTargets)
//     {
//         foreach (var soldierWithTarget in _soldiers)
//         {
//             // Skip null soldiers
//             if (soldierWithTarget?.soldier == null) continue;
//
//             if (soldierWithTarget.monster != null)
//             {
//                 // Check if the monster is still in the area
//                 if (_monstersInArea.Contains(soldierWithTarget.monster))
//                 {
//                     // Add to the tracking dictionary
//                     if (!monsterToSoldiers.ContainsKey(soldierWithTarget.monster))
//                     {
//                         monsterToSoldiers[soldierWithTarget.monster] = new List<SoldierWithTarget>();
//                     }
//
//                     monsterToSoldiers[soldierWithTarget.monster].Add(soldierWithTarget);
//                 }
//                 else
//                 {
//                     soldierWithTarget.monster = null;
//                     // soldierWithTarget.soldier.ChangeTarget(null);
//                     soldiersNeedingTargets.Add(soldierWithTarget);
//                 }
//             }
//             else
//             {
//                 soldiersNeedingTargets.Add(soldierWithTarget);
//             }
//         }
//     }
//
//     private void RebalanceSoldiers(
//         Dictionary<Monster, List<SoldierWithTarget>> monsterToSoldiers,
//         List<Monster> untargetedMonsters)
//     {
//         // Only rebalance if there are untargeted monsters
//         if (untargetedMonsters.Count == 0) return;
//
//         // Find monsters that have multiple soldiers assigned
//         List<Monster> monstersWithMultipleSoldiers = monsterToSoldiers
//             .Where(entry => entry.Value.Count > 1)
//             .Select(entry => entry.Key)
//             .ToList();
//
//         // For each monster with multiple soldiers, reassign the excess soldiers
//         foreach (var monster in monstersWithMultipleSoldiers)
//         {
//             if (untargetedMonsters.Count == 0) break;
//
//             List<SoldierWithTarget> soldiersOnMonster = monsterToSoldiers[monster];
//
//             // Sort soldiers by distance, keep the closest one
//             soldiersOnMonster.Sort((a, b) =>
//             {
//                 float distA = Vector2.Distance(a.soldier.transform.position, monster.transform.position);
//                 float distB = Vector2.Distance(b.soldier.transform.position, monster.transform.position);
//                 return distA.CompareTo(distB); // Sort ascending (closest first)
//             });
//
//             // Skip the closest soldier (index 0), reassign the rest
//             for (int i = 1; i < soldiersOnMonster.Count; i++)
//             {
//                 if (untargetedMonsters.Count == 0) break;
//
//                 SoldierWithTarget soldierToReassign = soldiersOnMonster[i];
//                 Monster newTarget = untargetedMonsters[0];
//
//                 // Reassign soldier to new target
//                 AssignSoldierToMonster(soldierToReassign, newTarget, monsterToSoldiers);
//
//                 // Remove from the list for the old monster
//                 monsterToSoldiers[monster].Remove(soldierToReassign);
//
//                 // Remove from untargeted list
//                 untargetedMonsters.RemoveAt(0);
//             }
//         }
//     }
//
//     private void AssignRemainingTargets(
//         List<SoldierWithTarget> soldiersNeedingTargets,
//         Dictionary<Monster, List<SoldierWithTarget>> monsterToSoldiers,
//         List<Monster> untargetedMonsters,
//         Monster entranceMonster)
//     {
//         foreach (var soldierWithTarget in soldiersNeedingTargets)
//         {
//             // First priority: entrance monster if available
//             if (entranceMonster != null && _monstersInArea.Contains(entranceMonster) &&
//                 (!monsterToSoldiers.ContainsKey(entranceMonster) || monsterToSoldiers[entranceMonster].Count == 0))
//             {
//                 AssignSoldierToMonster(soldierWithTarget, entranceMonster, monsterToSoldiers);
//                 continue;
//             }
//
//             // Second priority: untargeted monsters
//             if (untargetedMonsters.Count > 0)
//             {
//                 Monster target = untargetedMonsters[0];
//                 AssignSoldierToMonster(soldierWithTarget, target, monsterToSoldiers);
//                 untargetedMonsters.RemoveAt(0);
//                 continue;
//             }
//
//             // Last priority: find monster with fewest soldiers
//             Monster bestTarget = FindMonsterWithFewestSoldiers(monsterToSoldiers);
//             if (bestTarget != null)
//             {
//                 AssignSoldierToMonster(soldierWithTarget, bestTarget, monsterToSoldiers);
//             }
//         }
//     }
//
//     private Monster FindMonsterWithFewestSoldiers(Dictionary<Monster, List<SoldierWithTarget>> monsterToSoldiers)
//     {
//         Monster bestTarget = null;
//         int lowestCount = int.MaxValue;
//
//         foreach (var monster in _monstersInArea)
//         {
//             int targetCount = monsterToSoldiers.ContainsKey(monster) ? monsterToSoldiers[monster].Count : 0;
//             if (targetCount < lowestCount)
//             {
//                 lowestCount = targetCount;
//                 bestTarget = monster;
//             }
//         }
//
//         return bestTarget;
//     }
//
//     private void AssignSoldierToMonster(
//         SoldierWithTarget soldier,
//         Monster monster,
//         Dictionary<Monster, List<SoldierWithTarget>> monsterToSoldiers)
//     {
//         soldier.monster = monster;
//         soldier.SetTarget(monster);
//
//         if (!monsterToSoldiers.ContainsKey(monster))
//         {
//             monsterToSoldiers[monster] = new List<SoldierWithTarget>();
//         }
//
//         monsterToSoldiers[monster].Add(soldier);
//     }
//
//     #endregion
//
//     #region Flag Management
//
//     public void OnFlagPlaced(Vector2 position)
//     {
//         ChangeFlagPosition(position);
//
//         // Start flag animation
//         if (_flagCoroutine != null)
//         {
//             StopCoroutine(_flagCoroutine);
//         }
//
//         _flagCoroutine = StartCoroutine(AnimateFlagAppearance(position));
//         ReleaseAllMonsters();
//     }
//
//     private void ReleaseAllMonsters()
//     {
//         // Guard against uninitialized or empty array
//         if (_soldiers == null || _soldiers.Length <= 0) 
//             return;
//             
//         for (int i = 0; i < _soldiers.Length; i++)
//         {
//             // Check if the soldier and its monster are not null
//             if (_soldiers[i] != null && _soldiers[i].monster != null)
//             {
//                 _soldiers[i].monster.SetSoldierTarget(null);
//                 _soldiers[i].monster = null;
//                 
//                 // Also update soldier's target if it exists
//                 if (_soldiers[i].soldier != null)
//                 {
//                     _soldiers[i].soldier.ChangeTarget(null);
//                 }
//             }
//         }
//     }
//
//     private void ChangeFlagPosition(Vector2 newPosition)
//     {
//         // Check if the position is within the allowed range
//         if (Vector2.Distance(transform.position, newPosition) > _activityRange)
//         {
//             return;
//         }
//
//         _flagPosition = newPosition;
//         UpdateFlagColliderPosition();
//
//         // Notify soldiers about the flag position change
//         OnFlagPositionChanged?.Invoke(newPosition);
//     }
//
//     private void UpdateFlagColliderPosition()
//     {
//         if (_flagAreaCollider != null)
//         {
//             _flagAreaCollider.transform.position = _flagPosition;
//         }
//     }
//
//     private IEnumerator AnimateFlagAppearance(Vector2 position)
//     {
//         // Create or reuse flag instance
//         if (_flagInstance == null)
//         {
//             _flagInstance = Instantiate(flagPrefab, position, Quaternion.identity);
//         }
//         else
//         {
//             _flagInstance.transform.position = position;
//             _flagInstance.SetActive(true);
//         }
//
//         SpriteRenderer spriteRenderer = _flagInstance.GetComponent<SpriteRenderer>();
//         if (spriteRenderer != null)
//         {
//             // Fade in
//             spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);
//             yield return spriteRenderer.DOFade(1f, flagAnimationDuration).WaitForCompletion();
//
//             // Hold
//             yield return new WaitForSeconds(flagAnimationDuration);
//
//             // Fade out
//             yield return spriteRenderer.DOFade(0f, flagAnimationDuration).WaitForCompletion();
//         }
//
//         _flagInstance.SetActive(false);
//     }
//
//     #endregion
//
//     #region Public Methods
//
//     public override void Upgrade()
//     {
//         base.Upgrade();
//     }
//
//     // Called by FlagAreaTrigger
//     public void AddMonsterToArea(Monster monster)
//     {
//         if (monster != null && !_monstersInArea.Contains(monster))
//         {
//             Debug.Log("New monster entered the area");
//             _monstersInArea.Add(monster);
//             UpdateSoldierTargets(monster);
//         }
//     }
//
//     // Called by FlagAreaTrigger
//     public void RemoveMonsterFromArea(Monster monster)
//     {
//         if (monster != null)
//         {
//             _monstersInArea.Remove(monster);
//
//             // Clear this monster from any soldiers targeting it
//             foreach (var soldierWithTarget in _soldiers)
//             {
//                 if (soldierWithTarget?.monster == monster)
//                 {
//                     soldierWithTarget.monster = null;
//                     if (soldierWithTarget.soldier != null)
//                     {
//                         soldierWithTarget.SetTarget(null);
//                     }
//                 }
//             }
//             UpdateSoldierTargets(null);
//         }
//     }
//
//     #endregion
// }
//
// [Serializable]
// public class SoldierWithTarget
// {
//     public Soldier soldier;
//     public Monster monster;
//
//     public SoldierWithTarget(Soldier s)
//     {
//         soldier = s;
//         monster = null;
//     }
//
//     public void SetTarget(Monster m)
//     {
//         monster = m;
//         soldier.ChangeTarget(m);
//     }
// }

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

public class Barracks : BaseTower
{
    public event Action<Vector2> OnFlagPositionChanged;

    [Header("Barracks Settings")] [SerializeField]
    private int maxSoldiers = 3;

    [SerializeField] private float spawnTime = 5f;
    [SerializeField] private GameObject flagPrefab;
    [SerializeField] private TowerSO towerData;
    [SerializeField] private float _detectRange = 1f;
    [SerializeField] private float flagAnimationDuration = 0.5f;

    [Header("Flag Area")] 
    [SerializeField] private float _activityRange = 1.5f;
    private readonly Vector2[] _triangleOffsets = new Vector2[]
    {
        new Vector2(0f, 0.49f),
        new Vector2(-0.42f, -0.2555f),
        new Vector2(0.42f, -0.2555f),
    };

    // Private fields
    private GameObject _flagInstance;
    private GameObject _flagAreaCollider;
    [SerializeField]private SoldierWithTarget[] _soldiers;
    private List<Monster> _monstersInArea = new List<Monster>();
    private Coroutine _flagCoroutine;
    private Vector2 _flagPosition;
    private float _spawnTimer = 0f;
    private int _soldierCount = 0;

    #region Unity Lifecycle

    protected override void Start()
    {
        base.Start();
        _soldiers = new SoldierWithTarget[maxSoldiers];
        InitializeFlagCollider();
    }

    protected override void Update()
    {
        base.Update();
        TrySpawnSoldier();
    }

    private void OnDrawGizmos()
    {
        if (_flagPosition != Vector2.zero)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(_flagPosition, _detectRange);
        }
    }

    #endregion

    #region Tower Initialization

    private void InitializeFlagCollider()
    {
        if (_flagAreaCollider != null)
        {
            return;
        }

        _flagAreaCollider = new GameObject("FlagAreaCollider");
        _flagAreaCollider.transform.parent = transform;

        CircleCollider2D collider = _flagAreaCollider.AddComponent<CircleCollider2D>();
        collider.radius = _detectRange;
        collider.isTrigger = true;

        Rigidbody2D rb = _flagAreaCollider.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.simulated = true;
        FlagAreaTrigger trigger = _flagAreaCollider.AddComponent<FlagAreaTrigger>();
    }

    public void InitializeTower(TowerSO data)
    {
        towerData = data;

        // Set initial flag position above the barracks
        _flagPosition = transform.position + new Vector3(0, 0.7f, 0);
        _soldierCount = 0;
        _spawnTimer = 0;

        UpdateFlagColliderPosition();

        // Initialize soldier array with proper size
        _soldiers = new SoldierWithTarget[data.maxUnits];
    }

    #endregion

    #region Soldier Management

    private void TrySpawnSoldier()
    {
        if (_soldierCount >= maxSoldiers)
        {
            return;
        }
        _spawnTimer += Time.deltaTime;

        if (_spawnTimer >= spawnTime)
        {
            _spawnTimer = 0f;
            SpawnSoldier();
        }
    }

    private void SpawnSoldier()
    {
        if (_soldierCount >= maxSoldiers)
        {
            return;
        }

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
        {
            return;
        }

        // Determine formation position
        int formationIndex = slotIndex % _triangleOffsets.Length;

        // Instantiate soldier
        GameObject soldierObj = Instantiate(towerData.soldierPrefab, transform.position, Quaternion.identity);
        Soldier soldier = soldierObj.GetComponent<Soldier>();

        // Initialize soldier and add to our array
        _soldiers[slotIndex] = new SoldierWithTarget(soldier);
        soldier.InitializeWithFlagPosition(_flagPosition, _triangleOffsets[formationIndex], this,
            towerData.soldierData);

        // Update soldier count
        _soldierCount++;

        // Try to assign a target immediately if monsters are present
        if (_monstersInArea.Count > 0)
        {
            UpdateSoldierTargets(null);
        }
    }

    private void UpdateSoldierCount()
    {
        int activeCount = 0;

        for (int i = 0; i < _soldiers.Length; i++)
        {
            if (_soldiers[i]?.soldier != null)
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

    #endregion

    #region Monster Targeting

    private void UpdateSoldierTargets(Monster entranceMonster)
    {
        // Remove null monsters from the list
        _monstersInArea.RemoveAll(monster => monster == null);

        // Update soldier count and exit if no monsters or soldiers
        UpdateSoldierCount();
        if (_monstersInArea.Count == 0 || _soldierCount == 0)
        {
            return;
        }

        // Get all active soldiers
        List<SoldierWithTarget> activeSoldiers = new List<SoldierWithTarget>();
        foreach (var soldierWithTarget in _soldiers)
        {
            if (soldierWithTarget?.soldier != null)
            {
                activeSoldiers.Add(soldierWithTarget);
            }
        }

        // Clear current targets first to ensure clean reassignment
        foreach (var soldier in activeSoldiers)
        {
            soldier.monster = null;
        }

        // Case: More monsters than soldiers - ensure one monster per soldier
        if (_monstersInArea.Count >= activeSoldiers.Count)
        {
            AssignOneMonsterPerSoldier(activeSoldiers, entranceMonster);
        }
        // Case: More soldiers than monsters - distribute soldiers
        else
        {
            DistributeSoldiersToMonsters(activeSoldiers, entranceMonster);
        }
    }

    private void AssignOneMonsterPerSoldier(List<SoldierWithTarget> soldiers, Monster entranceMonster)
    {
        // Copy the monster list to work with
        List<Monster> availableMonsters = new List<Monster>(_monstersInArea);
        
        // Prioritize entrance monster if available
        if (entranceMonster != null && availableMonsters.Contains(entranceMonster))
        {
            // Assign the first soldier to the entrance monster
            soldiers[0].SetTarget(entranceMonster);
            availableMonsters.Remove(entranceMonster);
            
            // Assign the rest to different monsters
            for (int i = 1; i < soldiers.Count; i++)
            {
                if (availableMonsters.Count > 0)
                {
                    Monster target = availableMonsters[0];
                    soldiers[i].SetTarget(target);
                    availableMonsters.RemoveAt(0);
                }
                else
                {
                    // If we run out of unassigned monsters, start over from the beginning
                    // (This shouldn't happen if monsters >= soldiers, but just in case)
                    Monster target = _monstersInArea[i % _monstersInArea.Count];
                    soldiers[i].SetTarget(target);
                }
            }
        }
        else
        {
            // No entrance monster, distribute evenly
            for (int i = 0; i < soldiers.Count; i++)
            {
                if (i < availableMonsters.Count)
                {
                    // Each soldier gets a unique monster until we run out
                    soldiers[i].SetTarget(availableMonsters[i]);
                }
                else
                {
                    // If we run out of monsters, wrap around (this shouldn't happen in this case)
                    soldiers[i].SetTarget(_monstersInArea[i % _monstersInArea.Count]);
                }
            }
        }
    }

    private void DistributeSoldiersToMonsters(List<SoldierWithTarget> soldiers, Monster entranceMonster)
    {
        // Calculate how many soldiers per monster (at least 1)
        int soldiersPerMonster = Mathf.Max(1, soldiers.Count / _monstersInArea.Count);
        
        // Special case: if entranceMonster exists, prioritize it
        int currentMonsterIndex = 0;
        int currentSoldierIndex = 0;
        
        if (entranceMonster != null && _monstersInArea.Contains(entranceMonster))
        {
            // Assign soldiers to entrance monster first
            for (int i = 0; i < soldiersPerMonster && currentSoldierIndex < soldiers.Count; i++)
            {
                soldiers[currentSoldierIndex].SetTarget(entranceMonster);
                currentSoldierIndex++;
            }
            
            // Remove entrance monster from consideration for other assignments
            List<Monster> remainingMonsters = new List<Monster>(_monstersInArea);
            remainingMonsters.Remove(entranceMonster);
            
            // Distribute remaining soldiers among remaining monsters
            while (currentSoldierIndex < soldiers.Count && remainingMonsters.Count > 0)
            {
                Monster currentMonster = remainingMonsters[currentMonsterIndex];
                
                // Assign soldiersPerMonster to this monster
                for (int i = 0; i < soldiersPerMonster && currentSoldierIndex < soldiers.Count; i++)
                {
                    soldiers[currentSoldierIndex].SetTarget(currentMonster);
                    currentSoldierIndex++;
                }
                
                // Move to next monster
                currentMonsterIndex = (currentMonsterIndex + 1) % remainingMonsters.Count;
            }
            
            // If there are any remaining unassigned soldiers, distribute them round-robin
            if (currentSoldierIndex < soldiers.Count)
            {
                currentMonsterIndex = 0; // Start from the beginning of the monster list
                while (currentSoldierIndex < soldiers.Count)
                {
                    soldiers[currentSoldierIndex].SetTarget(_monstersInArea[currentMonsterIndex]);
                    currentSoldierIndex++;
                    currentMonsterIndex = (currentMonsterIndex + 1) % _monstersInArea.Count;
                }
            }
        }
        else
        {
            // No entrance monster, distribute evenly
            for (int i = 0; i < soldiers.Count; i++)
            {
                Monster target = _monstersInArea[currentMonsterIndex];
                soldiers[i].SetTarget(target);
                
                // Move to next monster after assigning soldiersPerMonster
                if ((i + 1) % soldiersPerMonster == 0)
                {
                    currentMonsterIndex = (currentMonsterIndex + 1) % _monstersInArea.Count;
                }
            }
        }
    }
    
    #endregion

    #region Flag Management

    public void OnFlagPlaced(Vector2 position)
    {
        ChangeFlagPosition(position);

        // Start flag animation
        if (_flagCoroutine != null)
        {
            StopCoroutine(_flagCoroutine);
        }

        _flagCoroutine = StartCoroutine(AnimateFlagAppearance(position));
    }
    
    private void ChangeFlagPosition(Vector2 newPosition)
    {
        // Check if the position is within the allowed range
        if (Vector2.Distance(transform.position, newPosition) > _activityRange)
        {
            return;
        }

        _flagPosition = newPosition;
        UpdateFlagColliderPosition();

        // Notify soldiers about the flag position change
        OnFlagPositionChanged?.Invoke(newPosition);
    }

    private void UpdateFlagColliderPosition()
    {
        if (_flagAreaCollider != null)
        {
            _flagAreaCollider.transform.position = _flagPosition;
        }
    }

    private IEnumerator AnimateFlagAppearance(Vector2 position)
    {
        // Create or reuse flag instance
        if (_flagInstance == null)
        {
            _flagInstance = Instantiate(flagPrefab, position, Quaternion.identity);
        }
        else
        {
            _flagInstance.transform.position = position;
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

    #endregion

    #region Public Methods

    public override void Upgrade()
    {
        base.Upgrade();
    }

    // Called by FlagAreaTrigger
    public void AddMonsterToArea(Monster monster)
    {
        if (monster != null && !_monstersInArea.Contains(monster))
        {
            Debug.Log("New monster entered the area");
            _monstersInArea.Add(monster);
            UpdateSoldierTargets(monster);
        }
    }

    // Called by FlagAreaTrigger
    public void RemoveMonsterFromArea(Monster monster)
    {
        if (monster != null)
        {
            _monstersInArea.Remove(monster);

            // Clear this monster from any soldiers targeting it
            foreach (var soldierWithTarget in _soldiers)
            {
                if (soldierWithTarget?.monster == monster)
                {
                    soldierWithTarget.monster = null;
                    if (soldierWithTarget.soldier != null)
                    {
                        soldierWithTarget.SetTarget(null);
                    }
                }
            }
            UpdateSoldierTargets(null);
        }
    }

    #endregion
}

[Serializable]
public class SoldierWithTarget
{
    public Soldier soldier;
    public Monster monster;

    public SoldierWithTarget(Soldier s)
    {
        soldier = s;
        monster = null;
    }

    public void SetTarget(Monster m)
    {
        monster = m;
        soldier.ChangeTarget(m);
    }
}