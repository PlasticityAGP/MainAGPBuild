using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Events;

public class SCR_CameraTrigger : MonoBehaviour {

    [SerializeField]
    private string EnterTagToPlay;
    [SerializeField]
    private bool UsingOnEnter;
    [SerializeField]
    private bool UsingOnExit;

    private void Start()
    {
        //SCR_EventManager.TriggerEvent("Timeline", EnterTagToPlay);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (UsingOnEnter)
        {
            if (other.tag == "Character")
            {
                SCR_EventManager.TriggerEvent("TimelineInstruction", "Forward");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (UsingOnExit)
        {
            if (other.tag == "Character")
            {
                SCR_EventManager.TriggerEvent("TimelineInstruction", "Rewind");
            }
        }
    }
}
