
using UnityEngine;


public abstract class State : MonoBehaviour
{
    // Event to signal a state transition
    public delegate void TransitionHandler(string newStateName);
    public event TransitionHandler OnTransition;


    public virtual void Enter()
    {
        // Override this method in derived classes to implement enter logic
    }


    public virtual void Exit()
    {
        // Override this method in derived classes to implement exit logic
    }


    public virtual void UpdateState(float deltaTime)
    {
        // Override this method in derived classes to implement frame-based logic
    }


    protected void SignalTransition(string newStateName)
    {
        OnTransition?.Invoke(newStateName);
    }
}
