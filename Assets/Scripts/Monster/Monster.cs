using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour
{
    public MonsterSO monsterSO { get; private set; }
    private List<PathNode> pathNodes;
    public int currentNodeIndex = 0;
    private float speed;

    public void Initialize(MonsterSO so, List<PathNode> nodes, float movementSpeed)
    {
        monsterSO = so;
        pathNodes = nodes;
        speed = movementSpeed;

        if (pathNodes.Count > 0)
        {
            transform.position = pathNodes[0].transform.position; // Start at the first node
            currentNodeIndex = 1; // Set the next target node
        }
    }

    private void Update()
    {
        if (pathNodes == null || currentNodeIndex >= pathNodes.Count)
            return;

        Vector3 targetPosition = pathNodes[currentNodeIndex].transform.position;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
        
        if (Vector3.Distance(transform.position, targetPosition) <= 0.04f)
        {
            // Snap to the exact position of the node
            transform.position = targetPosition;
        
            currentNodeIndex++;
            if (currentNodeIndex >= pathNodes.Count)
            {
                OnPathComplete();
            }
        }
    }

    private void OnPathComplete()
    {
        Debug.Log("Monster reached the end of the path.");
        Destroy(gameObject); // Destroy the monster or handle it as needed
    }
}