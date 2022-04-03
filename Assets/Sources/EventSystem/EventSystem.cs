using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class EventSystem
{ 
    private static Dictionary<WorldEvent, List<Action<object[]>>> eventListeners = new Dictionary<WorldEvent, List<Action<object[]>>>();

    public static void Subscribe(WorldEvent worldEvent, Action<object[]> listenerFunction)
    {
        if (!eventListeners.ContainsKey(worldEvent))
        {
            eventListeners.Add(worldEvent, new List<Action<object[]>>());
        }
        eventListeners[worldEvent].Add(listenerFunction);
    }

    public static void RaiseEvent(WorldEvent worldEvent, object[] functionArguments)
    {
        for(int i = 0; i < eventListeners[worldEvent]?.Count; i++)
        {
            // TODO: Need to check and handle if delegate class still exists
            eventListeners[worldEvent][i]?.Invoke(functionArguments);
        }
        
    }
}
public enum WorldEvent
{
    OnCollisionEnter
}