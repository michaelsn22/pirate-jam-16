using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    [SerializeField] public State currentState;
    private Dictionary<string, State> states = new Dictionary<string, State>();

    private void Awake()
    {
        // Populate the dictionary with child states
        foreach (Transform child in transform)
        {
            State state = child.GetComponent<State>();
            if (state != null)
            {
                states[child.name] = state;
                state.OnTransition += OnChildTransition;
            }
            else
            {
                Debug.LogWarning($"State machine contains an incompatible child node: {child.name}");
            }
        }
    }

    private void Start()
    {
        // Ensure the owner is ready (in Unity this happens after Awake/Start)
        if (currentState != null)
        {
            currentState.Enter();
        }
        else
        {
            Debug.LogWarning("StateMachine has no initial state set!");
        }
    }

    private void Update()
    {
        currentState?.UpdateState(Time.deltaTime);
    }

    private void OnChildTransition(string newStateName)
    {
        if (states.TryGetValue(newStateName, out State newState))
        {
            if (newState != currentState)
            {
                currentState?.Exit();
                newState.Enter();
                currentState = newState;
            }
        }
        else
        {
            Debug.LogWarning($"State does not exist: {newStateName}");
        }
    }
}
