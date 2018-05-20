using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// This script is based on the tutorial, "Events: Creating a Simple Messaging System" made by Adam Buckner.
// https://unity3d.com/learn/tutorials/topics/scripting/events-creating-simple-messaging-system

//Need to create an event class that extends UnityEvent<int> so that we can create events that actually pass information
[System.Serializable]
public class InputEvent : UnityEvent<int>
{

}


public class SCR_EventManager : MonoBehaviour
{
    //Event dictionary will associate a string name with a specific InputEvent object
    private Dictionary<string, InputEvent> EventDictionary;

    //CurrentManager is an instance of the event manager script
    private static SCR_EventManager CurrentManager;

    public static SCR_EventManager instance
    {
        get
        {
            //If we do not have a current instance of the event manager...
            if (!CurrentManager)
            {
                //Look for an instance of the event manager in our scene
                CurrentManager = FindObjectOfType(typeof(SCR_EventManager)) as SCR_EventManager;
                if (!CurrentManager)
                {
                    //If we cannot find an event manager in the scene, we got a problem
                    Debug.LogError("There is not currently an event manager in the scene");
                }
                else
                {
                    //If we find an instance of the event manager, initialize it
                    CurrentManager.Init();
                }
            }
            //return the instance of the event manager
            return CurrentManager;
        }
    }

    void Init()
    {
        if(EventDictionary == null)
        {
            //If we don't have a dictionary defined in EventDictionary, create a new one
            EventDictionary = new Dictionary<string, InputEvent>();
        }
    }

    public static void StartListening(string EventName, UnityAction<int> Listener)
    {
        InputEvent ThisEvent = null;
        //If the we are looking for exists within our dictionary, put it in the variable ThisEvent
        if (instance.EventDictionary.TryGetValue(EventName, out ThisEvent))
        {
            //Add the new listener to the event
            ThisEvent.AddListener(Listener);
        }
        //If the event we are looking for does not yet exist in our dictionary...
        else
        {
            //Create a new event, add a listener, and place it in our EventDictionary
            ThisEvent = new InputEvent();
            ThisEvent.AddListener(Listener);
            instance.EventDictionary.Add(EventName, ThisEvent);
        }
    }

    public static void StopListening(string EventName, UnityAction<int> Listener)
    {
        //If the current instance of the manager doesn't exist we don't need to do anything
        if (CurrentManager == null) return;
        InputEvent ThisEvent = null;
        if (instance.EventDictionary.TryGetValue(EventName, out ThisEvent))
        {
            //Need to remove listener from event in dictionary if we find it 
            ThisEvent.RemoveListener(Listener);
        }
    }

    public static void TriggerEvent(string EventName, int value)
    {
        InputEvent ThisEvent = null;
        if (instance.EventDictionary.TryGetValue(EventName, out ThisEvent))
        {
            //If the event exists in our dictionary, invoke it
            ThisEvent.Invoke(value);
        }
    }
}
