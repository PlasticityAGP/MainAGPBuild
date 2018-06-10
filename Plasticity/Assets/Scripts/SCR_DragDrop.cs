using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using RootMotion.FinalIK;

public class SCR_DragDrop : MonoBehaviour {

    //Listener to tell when character wants to interact
    private UnityAction<int> InteractListener;
    [HideInInspector]
    public bool Interact = false;
    private FullBodyBipedIK Ik;
    //The rigidbody of the object we will be moving
    private Rigidbody RBody;
    [HideInInspector]
    public bool IsZ;
    private bool OverlapTrigger;
    private float InitialSpeed;
    [SerializeField]
    [Tooltip("Left hand IK effector on box when dragging from Z")]
    private GameObject ZEffectorLeft;
    [SerializeField]
    [Tooltip("Right hand IK effector on box when dragging from Z")]
    private GameObject ZEffectorRight;
    [SerializeField]
    [Tooltip("Speed we want to slow down the player to when they drag an object")]
    [ValidateInput("GreaterThanZero", "The Drag speed cannot be zero or a negative number!")]
    private float MaxDragSpeed;
    [Tooltip("Reference to the character that can interact with this object")]
    [ValidateInput("IsNull", "There must be a reference to the Character!")]
    public GameObject Character;

    [HideInInspector]
    public SCR_CharacterManager CharacterManager;

    private bool GreaterThanZero(float input)
    {
        return input > 0.0f;
    }

    private bool IsNull(GameObject thing)
    {
        try
        {
            return thing.scene.IsValid();
        }
        catch
        {
            return false;
        }
    }

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
        {
            Interact = true;
            if (OverlapTrigger)
            {
                EnteredAndInteracted();
            }
            if (IsZ)
            {
                EnteredAndInteracted();
            }
        }
        
            
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
        if (Character.GetComponentInChildren<FullBodyBipedIK>()) Ik = Character.GetComponentInChildren<FullBodyBipedIK>();
        else Debug.LogError("We need a a FullBodyBipedIK component attached to one of the Character's child Game Objects");
        //Define an initial speed that we want to return the character to after they finish moving the object
        InitialSpeed = CharacterManager.MoveSpeed;
        IsZ = false;
        OverlapTrigger = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Character")
        {
            OverlapTrigger = true;
            if (Interact)
            {
                EnteredAndInteracted();
            }
        }
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
            OverlapTrigger = false;
            FreezeAll();
        }
    }

    public void EnteredAndInteracted()
    {
        //If the character is grounded, in the trigger, and pressing an interact key. Allow the box to move, limit player speed
        //and set the velocity of the object to be moved
        UnfreezeXY();
        CharacterManager.MoveSpeed = MaxDragSpeed;
        if (IsZ)
        {
            //Debug.Log("SET IK EFFECTORS");
            Ik.solver.leftHandEffector.target = ZEffectorLeft.transform;
            Ik.solver.leftHandEffector.positionWeight = 1.0f;
            Ik.solver.leftHandEffector.rotationWeight = 1.0f;
            Ik.solver.rightHandEffector.target = ZEffectorRight.transform;
            Ik.solver.rightHandEffector.positionWeight = 1.0f;
            Ik.solver.rightHandEffector.rotationWeight = 1.0f;

        }
    }

    /// <summary>
    /// Should be called when the character is overlapping a trigger to pull a DragOBJ
    /// </summary>
    /// <param name="Other">Other should be whatever GameObject is overlapping the trigger</param>
    public void InTrigger(GameObject Other)
    {
        if (CharacterManager.IsGrounded())
        {
            if (Interact)
            {
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

    /// <summary>
    /// FreezeAll sets all of the Rigidbody constraints to true on the draggable object
    /// </summary>
    [HideInInspector]
    public void FreezeAll()
    {
        //Freeze rigidbody via constraints, and return the player to their original speed
        CharacterManager.MoveSpeed = InitialSpeed;
        RBody.constraints =  RigidbodyConstraints.FreezeAll;
        if (IsZ)
        {
            //Debug.Log("RESET EFFECTORS");
            Ik.solver.leftHandEffector.target = null;
            Ik.solver.leftHandEffector.positionWeight = 0.0f;
            Ik.solver.leftHandEffector.rotationWeight = 0.0f;
            Ik.solver.rightHandEffector.target = null;
            Ik.solver.rightHandEffector.positionWeight = 0.0f;
            Ik.solver.rightHandEffector.rotationWeight = 0.0f;
        }
    }

    private void UnfreezeXY()
    {
        //Bitwise boolean logic that essentially only allows the boc to move in x and y directions. 
        RBody.constraints = ~(RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY);
    }

    private void FixedUpdate()
    {
        if (IsZ)
        {
            Vector3 A = ZEffectorRight.transform.position - ZEffectorLeft.transform.position;
            float Mag = A.magnitude;
            Vector3 Midpoint = ZEffectorLeft.transform.position + (A.normalized * (Mag / 2.0f));
            Vector3 Adjust =  (A.normalized * -0.5f);
            Vector3 B = ZEffectorRight.transform.position - Midpoint;
            Vector3 C = Character.transform.position - ZEffectorLeft.transform.position;

            Vector3 Offset = Vector3.Dot(C, B.normalized) * B.normalized;
            ZEffectorLeft.transform.position = ZEffectorLeft.transform.position + Offset + Adjust;
            ZEffectorRight.transform.position = ZEffectorRight.transform.position + Offset + Adjust;
        }
    }
}
