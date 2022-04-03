using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine
{
    Dictionary<Type, State> stateDictionary = new Dictionary<Type, State>();
    public State currentState { get; private set; }
    State queuedState;
    public object owner { get; private set; }

    public StateMachine(object owner, List<State> states)
    {
        this.owner = owner;
        foreach(State state in states)
        {
            State instantiatedState = GameObject.Instantiate(state);
            instantiatedState.Initialize(this);
            stateDictionary.Add(instantiatedState.GetType(), instantiatedState);
            if(currentState == null)
                currentState = instantiatedState;
        }
        currentState.Enter();
    }
    
    public void Run()
    {
        if (queuedState && queuedState != currentState)
        {
            currentState.Exit();
            currentState = queuedState;
            queuedState = null;
            currentState.Enter();
        }
        currentState.Update();
        currentState.EvaluateTransition();
    }

    public void TransitionTo<T>() where T : State
    {
        queuedState = stateDictionary[typeof(T)];
    }
}
