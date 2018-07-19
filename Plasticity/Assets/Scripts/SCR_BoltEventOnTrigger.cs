using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_BoltEventOnTrigger : MonoBehaviour
{

    [SerializeField]
    private GameObject TargetGameObject;

    [SerializeField]
    private string OnTriggerEnterEvent = "";

    [SerializeField]
    private string OnTriggerStayEvent = "";

    [SerializeField]
    private string OnTriggerExitEvent = "";
    

    private void FireBoltEvent(string eventName)
    {
        if(eventName != "")
        {
            if(TargetGameObject)
            {
                Bolt.CustomEvent.Trigger(TargetGameObject, eventName);
            }
            else
            {
                Debug.LogError("Tried to trigger event on null GameObject.");
            }
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        FireBoltEvent(OnTriggerEnterEvent);
    }

    void OnTriggerStay(Collider other)
    {
        FireBoltEvent(OnTriggerStayEvent);
    }

    void OnTriggerExit(Collider other)
    {
        FireBoltEvent(OnTriggerExitEvent);
    }


}
