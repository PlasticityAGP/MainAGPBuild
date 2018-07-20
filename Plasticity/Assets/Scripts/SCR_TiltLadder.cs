using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SCR_TiltLadder : MonoBehaviour {
    [SerializeField]
    private float StrengthOfGirl;
    private UnityAction<int> InteractListener;
    private bool Interact;
    private Vector3 ZVec = new Vector3(0.0f, 0.0f, 1.0f);

    private void Awake()
    {
        //Register the callback functions related to each listener. These will be called as
        //the events these listeners are listening to get invoked 
        InteractListener = new UnityAction<int>(InteractPressed);
    }


    private void OnEnable()
    {
        //Register listeners with their events in the EventManager
        SCR_EventManager.StartListening("InteractKey", InteractListener);
    }

    private void OnDisable()
    {
        SCR_EventManager.StopListening("InteractKey", InteractListener);
    }

    private void InteractPressed(int value)
    {
        //The value passed by the event indicates whether or not the key is pressed down.
        if (value == 1)
        {
            Interact = true;
        }
      else
        {
            Interact = false;
        }
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Character")
        {
            //Debug.Log("Character Entered");
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.tag == "Character" && Interact)
        {
            bool temp = other.GetComponent<SCR_CharacterManager>().MoveDir;
            if (temp)
            {
                gameObject.transform.parent.GetComponent<Rigidbody>().AddTorque(ZVec * -(1.0f * StrengthOfGirl));
            }
            else
            {
                gameObject.transform.parent.GetComponent<Rigidbody>().AddTorque(ZVec * (1.0f * StrengthOfGirl));
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.tag == "Character")
        {
            //Debug.Log("Character Exited");
        }
    }
}
