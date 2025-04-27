using UnityEngine;

public class SkeletonMonster : MonsterBase
{

    private bool hasResurrected = false;
    private float boneThrowTimer = 0f;
    
    protected override void Awake()
    {
        base.Awake();
     
    }
    
    protected override void Update()
    {
     base.Update(); 
    }
    
   
}