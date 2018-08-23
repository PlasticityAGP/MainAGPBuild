using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Events;

public class SCR_CameraTrigger : MonoBehaviour {

    [SerializeField]
    private bool UsingOnEnter;
    [SerializeField]
    [ShowIf("UsingOnEnter")]
    private string EnterTagToPlay;
    [SerializeField]
    private bool UsingOnExit;
    [SerializeField]
    [ShowIf("UsingOnExit")]
    private string ExitTagToPlay;

    private void OnTriggerEnter(Collider other)
    {
        if (UsingOnEnter)
        {
            if (other.tag == "Character")
            {
                SCR_EventManager.TriggerEvent("Timeline", EnterTagToPlay);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (UsingOnExit)
        {
            if (other.tag == "Character")
            {
                SCR_EventManager.TriggerEvent("Timeline", ExitTagToPlay);
            }
        }
    }
}
