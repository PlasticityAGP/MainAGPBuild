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
    private bool LerpToEffector;
    private float Weight;
    private Queue<float> FromQueue;
    private Queue<float> ToQueue;
    private float InitialSpeed;
    [SerializeField]
    [Tooltip("The farthest left a left hand effector is allowed to go on this object")]
    [ValidateInput("IsNull", "There must be a reference to the Left Endpoint Game Object!")]
    private GameObject LeftEndPoint;
    [SerializeField]
    [Tooltip("The farthest right a left hand effector is allowed to go on this object")]
    [ValidateInput("IsNull", "There must be a reference to the Left Endpoint Game Object!")]
    private GameObject RightEndPoint;
    [SerializeField]
    [Tooltip("Left hand IK effector on box when dragging from Z")]
    [ValidateInput("IsNull", "There must be a reference to the Left Effector Game Object!")]
    private GameObject ZEffectorLeft;
    [SerializeField]
    [Tooltip("Right hand IK effector on box when dragging from Z")]
    [ValidateInput("IsNull", "There must be a reference to the Right Effector Game Object!")]
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
        LerpToEffector = false;
        FromQueue = new Queue<float>();
        ToQueue = new Queue<float>();
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
            Ik.solver.leftHandEffector.target = null;
            Ik.solver.rightHandEffector.target = null;
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
            Ik.solver.rightHandEffector.target = ZEffectorRight.transform;
            StartEffectorLerp(0.0f, 1.0f);

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
            StartEffectorLerp(1.0f, 0.0f);
        }
    }

    private void UnfreezeXY()
    {
        //Bitwise boolean logic that essentially only allows the boc to move in x and y directions. 
        RBody.constraints = ~(RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY);
    }

    private void EffectorCalculations()
    {
        if (IsZ)
        {
            Vector3 A = ZEffectorRight.transform.position - ZEffectorLeft.transform.position;
            float Mag = A.magnitude;
            Vector3 Midpoint = ZEffectorLeft.transform.position + (A.normalized * (Mag / 2.0f));
            if (Weight == -0.4f && CharacterManager.MoveDir && Interact)
            {
                if (!(Ik.solver.leftHandEffector.positionWeight == 0.0f))
                {
                    StartEffectorLerp(1.0f, 0.5f);
                    StartEffectorLerp(0.5f, 1.0f);
                }
            }
            if (Weight == -0.1f && !CharacterManager.MoveDir && Interact)
            {
                if (!(Ik.solver.leftHandEffector.positionWeight == 0.0f))
                {
                    StartEffectorLerp(1.0f, 0.5f);
                    StartEffectorLerp(0.5f, 1.0f);
                }
            }
            if (CharacterManager.MoveDir) Weight = -0.1f;
            else Weight = -0.4f;
            Vector3 Adjust = (A.normalized * Weight);
            Vector3 B = ZEffectorRight.transform.position - Midpoint;
            Vector3 C = Character.transform.position - ZEffectorLeft.transform.position;

            Vector3 Offset = Vector3.Dot(C, B.normalized) * B.normalized;
            Vector3 Left = ZEffectorLeft.transform.position + Offset + Adjust;
            Vector3 Right = ZEffectorRight.transform.position + Offset + Adjust;
            ZEffectorLeft.transform.position = Left;
            ZEffectorRight.transform.position = Right;
            if (Vector3.Magnitude(Left - Right) > Vector3.Magnitude(LeftEndPoint.transform.position - Right))
            {
                Ik.solver.leftHandEffector.target = LeftEndPoint.transform;
            }
            else
            {

                Ik.solver.leftHandEffector.target = ZEffectorLeft.transform;
            }
            if (Vector3.Magnitude(Right - Left) > Vector3.Magnitude(RightEndPoint.transform.position - Left))
            {
                Ik.solver.rightHandEffector.target = RightEndPoint.transform;
            }
            else
            {
                Ik.solver.rightHandEffector.target = ZEffectorRight.transform;
            }
        }
    }

    private void StartEffectorLerp(float from, float to)
    {
        LerpToEffector = true;
        Ik.solver.leftHandEffector.positionWeight = from;
        Ik.solver.leftHandEffector.rotationWeight = from;
        Ik.solver.rightHandEffector.positionWeight = from;
        Ik.solver.rightHandEffector.rotationWeight = from;
        FromQueue.Enqueue(from);
        ToQueue.Enqueue(to);
    }

    private void EndEffectorLerp()
    {
        if ((FromQueue.Count + ToQueue.Count) == 0) LerpToEffector = false;
        FromQueue.Dequeue();
        ToQueue.Dequeue();
    }

    private void EffectorDoLerp(float DeltaTime, float from, float to)
    {
        if(from > to)
        {
            float temp = Ik.solver.leftHandEffector.positionWeight;
            temp -= (DeltaTime * 4.0f);
            if(temp <= to)
            {
                EndEffectorLerp();
                Ik.solver.leftHandEffector.positionWeight = to;
                Ik.solver.leftHandEffector.rotationWeight = to;
                Ik.solver.rightHandEffector.positionWeight = to;
                Ik.solver.rightHandEffector.rotationWeight = to;
            }
            else
            {
                Ik.solver.leftHandEffector.positionWeight = temp;
                Ik.solver.leftHandEffector.rotationWeight = temp;
                Ik.solver.rightHandEffector.positionWeight = temp;
                Ik.solver.rightHandEffector.rotationWeight = temp;
            }
        }
        else
        {
            float temp = Ik.solver.leftHandEffector.positionWeight;
            temp += (DeltaTime * 4.0f);
            if (temp >= to)
            {
                EndEffectorLerp();
                Ik.solver.leftHandEffector.positionWeight = to;
                Ik.solver.leftHandEffector.rotationWeight = to;
                Ik.solver.rightHandEffector.positionWeight = to;
                Ik.solver.rightHandEffector.rotationWeight = to;
            }
            else
            {
                Ik.solver.leftHandEffector.positionWeight = temp;
                Ik.solver.leftHandEffector.rotationWeight = temp;
                Ik.solver.rightHandEffector.positionWeight = temp;
                Ik.solver.rightHandEffector.rotationWeight = temp;
            }
        }
    }

    private void FixedUpdate()
    {
        EffectorCalculations();
        if (LerpToEffector && (FromQueue.Count + ToQueue.Count) != 0) EffectorDoLerp(Time.deltaTime, FromQueue.Peek(), ToQueue.Peek());
    }
}
