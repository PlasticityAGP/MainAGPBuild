using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SCR_DragDrop : MonoBehaviour {

    //Listener to tell when character wants to interact
    private UnityAction<int> InteractListener;
    private bool Interact = false;

    //The rigidbody of the object we will be moving
    private Rigidbody RBody;

    private float InitialSpeed;
    [SerializeField]
    [Tooltip("Speed we want to slow down the player to when they drag an object")]
    private float MaxDragSpeed;

    [Tooltip("Reference to the character that can interact with this object")]
    public GameObject Character;

    [HideInInspector]
    public SCR_CharacterManager CharacterManager;

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
        //Get the rigidbody component we will be moving
        if (gameObject.transform.parent.GetComponent<Rigidbody>()) RBody = gameObject.transform.parent.GetComponent<Rigidbody>();
        else Debug.LogError("This interactable object needs a rigidbody attached to it");
        //Get the character manager so that we can set the character's speed when they start moving the object
        if (Character.GetComponent<SCR_CharacterManager>()) CharacterManager = Character.GetComponent<SCR_CharacterManager>();
        else Debug.LogError("We need a reference to a Character GameObject with an attached SCR_CharacterManager script in the DragDrop script");
        //Define an initial speed that we want to return the character to after they finish moving the object
        InitialSpeed = CharacterManager.MoveSpeed;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Character")
        {
            //Debug.Log("Testing");

            //Call method that allows box movement.
            InTrigger(other.gameObject);
            
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Character")
        {
            //If the character is not in range to move the box, don't allow the box to move.
            FreezeAll();
        }
    }

    [HideInInspector]
    public void InTrigger(GameObject Other)
    {
        if (CharacterManager.IsGrounded())
        {
            if (Interact)
            {
                //If the character is grounded, in the trigger, and pressing an interact key. Allow the box to move, limit player speed
                //and set the velocity of the object to be moved
                UnfreezeXY();
                CharacterManager.MoveSpeed = MaxDragSpeed;
                //Debug.Log("Entered + interacted");
                RBody.velocity = Other.GetComponent<Rigidbody>().velocity;
            }
        }
        else
        {
            //In all other cases we want the character to be still
            FreezeAll();
        }
    }

    [HideInInspector]
    public void FreezeAll()
    {
        //Freeze rigidbody via constraints, and return the player to their original speed
        CharacterManager.MoveSpeed = InitialSpeed;
        RBody.constraints =  RigidbodyConstraints.FreezeAll;
    }

    void UnfreezeXY()
    {
        //Bitwise boolean logic that essentially only allows the boc to move in x and y directions. 
        RBody.constraints = ~(RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY);
    }

}
