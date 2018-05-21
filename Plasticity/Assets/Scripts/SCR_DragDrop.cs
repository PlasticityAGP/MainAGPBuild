using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SCR_DragDrop : MonoBehaviour {

    private UnityAction<int> InteractListener;
    private bool Interact = false;
    private Rigidbody RBody;
    [SerializeField]
    [Tooltip("Reference to the character that can interact with this object")]
    private GameObject Character;

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
        //Tell the EventManager we are no longer listening as the CharacterManager gets disabled.
        SCR_EventManager.StopListening("InteractKey", InteractListener);
    }

    private void InteractPressed(int value)
    {
        //The value passed by the event indicates whether or not the key is pressed down.
        if (value == 1)
            Interact = true;
        else
        {
            Interact = false;
            FreezeAll();
        }            
    }


    // Use this for initialization
    void Start () {
        if (GetComponent<Rigidbody>()) RBody = GetComponent<Rigidbody>();
        else Debug.LogError("This interactable object needs a rigidbody attached to it");
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.Equals(Character))
        {
            if (Character.GetComponent<SCR_CharacterManager>().IsGrounded())
            {
                if (Interact)
                {
                    UnfreezeXY();
                    //Debug.Log("Entered + interacted");
                    RBody.velocity = Character.GetComponent<Rigidbody>().velocity;
                }
            }
            else
            {
                FreezeAll();
            }
            
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.Equals(Character))
        {
            FreezeAll();
        }
    }

    void FreezeAll()
    {
        RBody.constraints =  RigidbodyConstraints.FreezeAll;
    }

    void UnfreezeXY()
    {
        RBody.constraints = ~(RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY);
    }


    // Update is called once per frame
    void Update () {
		
	}
}
