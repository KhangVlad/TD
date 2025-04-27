
    using System;
    using UnityEngine;

    public class HeroBehavior : MonoBehaviour
    {
        [Header("Direction")]
        [SerializeField] public FacingDirection currentDirection = FacingDirection.None;
        [SerializeField] public Animator anim;


        private void Start()
        {
            
        }

        private void ListenClickEvent(HeroBehavior h)
        {
            
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
        
    }

    public void UpdateState(HeroBehavior h)
    {
        
    }

    public void ExitState(HeroBehavior h)
    {
        
    }
}