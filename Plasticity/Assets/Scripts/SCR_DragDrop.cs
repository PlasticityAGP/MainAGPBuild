using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using RootMotion.FinalIK;

public class SCR_DragDrop : SCR_GameplayStatics {

    //Listener to tell when character wants to interact
    private UnityAction<int> InteractListener;
    private UnityAction<int> TurnListener;
    private bool Interact = false;
    private SCR_IKToolset IkTools;
    //The rigidbody of the object we will be moving
    private Rigidbody RBody;
    [HideInInspector]
    public bool IsZ;
    private bool OverlapTrigger;
    private float Weight;
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
    [SerializeField]
    [Tooltip("Ik animations curves for left hand effector")]
    [ValidateInput("NotEmpty", "We need at least a couple of anim curves to define IK behavior")]
    private AnimationCurve[] LeftHandCurves;
    [SerializeField]
    [Tooltip("IK animation curves for right hand effector")]
    [ValidateInput("NotEmpty", "We need at least a couple of anim curves to define IK behavior")]
    private AnimationCurve[] RightHandCurves;
    [SerializeField]
    [Tooltip("Trace distance for determining if the player has landed.")]
    [ValidateInput("LessThanZero", "We cannot have a trace distance <= 0.0")]
    private float GroundTraceDistance;
    [SerializeField]
    [Tooltip("How much higher above origin IsGrounded trace should occur")]
    [ValidateInput("LessThanZero", "We cannot have a trace distance <= 0.0")]
    private float YTraceOffset;
    [SerializeField]
    [Tooltip("The maximum angle between player and ground that is walkable by player")]
    private float MaxGroundAngle;
    [SerializeField]
    [Tooltip("Layermask that signifies what objects are considered to be the ground.")]
    private LayerMask GroundLayer;
    [SerializeField]


    [HideInInspector]
    public SCR_CharacterManager CharacterManager;
    private bool LockedOut;
    private bool Moving;

    private void Awake()
    {
        //Register the callback functions related to each listener. These will be called as
        //the events these listeners are listening to get invoked 
        InteractListener = new UnityAction<int>(InteractPressed);
        TurnListener = new UnityAction<int>(CharacterTurned);
    }

    private void OnEnable()
    {
        //Register listeners with their events in the EventManager
        SCR_EventManager.StartListening("InteractKey", InteractListener);
        SCR_EventManager.StartListening("CharacterTurn", TurnListener);
    }

    private void OnDisable()
    {
        //Tell the EventManager we are no longer listening as the CharacterManager gets disabled.
        SCR_EventManager.StopListening("InteractKey", InteractListener);
        SCR_EventManager.StopListening("CharacterTurn", TurnListener);
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

    private void CharacterTurned(int value)
    {
        if(IsZ && Interact)
        {
            //Lerp effectors when the character is in the propper trigger and has pressed the interact key down while turning
            IkTools.StartEffectorLerp("LeftHand", LeftHandCurves[3], 0.75f);
            IkTools.StartEffectorLerp("RightHand", RightHandCurves[3], 0.75f);
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
        if (Character.GetComponent<SCR_IKToolset>()) IkTools = Character.GetComponent<SCR_IKToolset>();
        else Debug.LogError("We need a SCR_IKToolset script attached to one of the Character's child Game Objects");
        //Define an initial speed that we want to return the character to after they finish moving the object
        InitialSpeed = CharacterManager.GetSpeed();
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

    /// <summary>
    /// Called by SCR_ZDrag when a player has entered the trigger volume in the Z direction of the Draggable object
    /// </summary>
    public void OnZTriggerEnter()
    {
        IsZ = true;
        if (Interact) EnteredAndInteracted();
    }

    /// <summary>
    /// Called by SCR_ZDrag when a player has exited the trigger volume in the Z direction of the Draggable object
    /// </summary>
    public void OnZTriggerExit()
    {
        //Reset effectors when the character leaves the dragable object zone
        IkTools.SetEffectorTarget("LeftHand", null);
        IkTools.SetEffectorTarget("RightHand", null);
        FreezeAll();

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Character")
        {
            CharacterManager.InteractingWith = null;
            OverlapTrigger = false;
            IkTools.SetEffectorTarget("LeftHand", null);
            IkTools.SetEffectorTarget("RightHand", null);
            FreezeAll();
        }
    }

    private void EnteredAndInteracted()
    {
        if(CharacterManager.InteractingWith == null)
        {
            //If the character is grounded, in the trigger, and pressing an interact key. Allow the box to move, limit player speed
            //and set the velocity of the object to be moved
            UnfreezeXY();
            CharacterManager.SetSpeed(MaxDragSpeed);
            CharacterManager.InteractingWith = gameObject;
            if (IsZ)
            {
                if (Interact)
                {
                    IkTools.SetEffectorTarget("LeftHand", ZEffectorLeft);
                    IkTools.SetEffectorTarget("RightHand", ZEffectorRight);
                    if (CharacterManager.MoveDir)
                    {
                        IkTools.StartEffectorLerp("LeftHand", LeftHandCurves[0], 0.75f);
                        IkTools.StartEffectorLerp("RightHand", RightHandCurves[0], 0.75f);
                    }
                    else
                    {
                        IkTools.StartEffectorLerp("LeftHand", LeftHandCurves[1], 0.75f);
                        IkTools.StartEffectorLerp("RightHand", RightHandCurves[1], 0.75f);
                    }
                }
            }
        }
    }

    private void CalculateDir()
    {
        if (CharacterManager.PlayerGrounded)
        {
            RaycastHit Output = FireTrace();
            Vector3 VelocityDir;
            float GroundAngle;
            if (CharacterManager.MoveDir)
            {
                VelocityDir = Vector3.Cross(Output.normal, gameObject.transform.parent.forward).normalized * CharacterManager.RBody.velocity.magnitude;
                GroundAngle = Vector3.Angle(Output.normal, new Vector3(-1.0f, 0.0f, 0.0f));
            }
            else
            {
                VelocityDir = Vector3.Cross(Output.normal, gameObject.transform.parent.forward).normalized * CharacterManager.RBody.velocity.magnitude * -1.0f;
                GroundAngle = Vector3.Angle(Output.normal, new Vector3(-1.0f, 0.0f, 0.0f));
                GroundAngle = Vector3.Angle(Output.normal, new Vector3(-1.0f, 0.0f, 0.0f));
            }
            gameObject.transform.parent.localEulerAngles = new Vector3(0.0f, 0.0f, -(GroundAngle - 90.0f));
            RBody.velocity = VelocityDir;
        }
    }

    public RaycastHit FireTrace()
    {
        //Create two locations to trace from so that we can have a little bit of 'dangle' as to whether
        //or not the character is on an object.
        Vector3 YOffset = new Vector3(0.0f, YTraceOffset, 0.0f);
        Vector3 CenterPosition = transform.position + YOffset;
        RaycastHit Result;
        Vector3 End = CenterPosition;
        End.y -= GroundTraceDistance;
        Physics.Raycast(CenterPosition, Vector3.down, out Result, GroundTraceDistance, GroundLayer);
        return Result;
    }

    /// <summary>
    /// Should be called when the character is overlapping a trigger to pull a DragOBJ
    /// </summary>
    /// <param name="Other">Other should be whatever GameObject is overlapping the trigger</param>
    public void InTrigger(GameObject Other)
    {
        if (!CharacterManager.PlayerGrounded)
        {
            FreezeAll();
        }
    }

    /// <summary>
    /// FreezeAll sets all of the Rigidbody constraints to true on the draggable object
    /// </summary>
    [HideInInspector]
    public void FreezeAll()
    {
        Moving = false;
        if (!IsZ && Interact)
        {
            IkTools.ForceEffectorWeight("LeftHand", 0.0f);
            IkTools.ForceEffectorWeight("RightHand", 0.0f);
            CharacterManager.InteractingWith = null;
        }
        else if(IsZ)
        {
            CharacterManager.InteractingWith = null;
            if (CharacterManager.MoveDir)
            {
                IkTools.StartEffectorLerp("LeftHand", LeftHandCurves[2], 0.75f);
                IkTools.StartEffectorLerp("RightHand", LeftHandCurves[2], 0.75f);
            }
            else
            {
                IkTools.StartEffectorLerp("LeftHand", LeftHandCurves[2], 0.75f);
                IkTools.StartEffectorLerp("RightHand", LeftHandCurves[2], 0.75f);
            }
        }

        //Freeze rigidbody via constraints, and return the player to their original speed
        CharacterManager.SetSpeed(InitialSpeed);
        RBody.constraints = RBody.constraints = (RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezePositionY) | 
            (RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ);
    }

    public void Lockout()
    {
        LockedOut = true;
        RBody.velocity = new Vector3(0.0f, 0.0f, 0.0f);
        FreezeAll();
    }

    private void UnfreezeXY()
    {
        if (!LockedOut)
        {
            //Bitwise boolean logic that essentially only allows the boc to move in x and y directions. 
            RBody.constraints = ~(RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY)
                | (RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ); ;
            Moving = true;
        }
    }
    
    //Calculate where effectors should be whenever the character is within the z trigger
    private void EffectorCalculations()
    {
        if (IsZ)
        {
            //Do vector math to find points along a line defined by the right and left effector positions
            Vector3 A = ZEffectorRight.transform.position - ZEffectorLeft.transform.position;
            float Mag = A.magnitude;
            //Midpoint between the two hands
            Vector3 Midpoint = ZEffectorLeft.transform.position + (A.normalized * (Mag / 2.0f));
            //Adjust where hands should be relative to the character based on the direction the player is moving
            if (CharacterManager.MoveDir) Weight = -0.2f;
            else Weight = -0.4f;
            Vector3 Adjust = (A.normalized * Weight);
            Vector3 B = ZEffectorRight.transform.position - Midpoint;
            Vector3 C = Character.transform.position - ZEffectorLeft.transform.position;

            Vector3 Offset = Vector3.Dot(C, B.normalized) * B.normalized;
            Vector3 Left = ZEffectorLeft.transform.position + Offset + Adjust;
            Vector3 Right = ZEffectorRight.transform.position + Offset + Adjust;
            //Set effector locations based on calculations
            ZEffectorLeft.transform.position = Left;
            ZEffectorRight.transform.position = Right;
            //If our effector locations lie outside of our box, set them to the corner of the box so the player isn't grabbing empty air
            if (Vector3.Magnitude(Left - Right) > Vector3.Magnitude(LeftEndPoint.transform.position - Right))
            {
                if(IsZ) IkTools.SetEffectorTarget("LeftHand", LeftEndPoint);
            }
            else
            {
                if (IsZ) IkTools.SetEffectorTarget("LeftHand", ZEffectorLeft);
            }
            if (Vector3.Magnitude(Right - Left) > Vector3.Magnitude(RightEndPoint.transform.position - Left))
            {
                if (IsZ) IkTools.SetEffectorTarget("RightHand", RightEndPoint);
            }
            else
            {
                if (IsZ) IkTools.SetEffectorTarget("RightHand", ZEffectorRight);
            }
        }
    }

    private void FixedUpdate()
    {
        EffectorCalculations();
        if(Moving && !LockedOut) CalculateDir();
    }
}
