using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using RootMotion.FinalIK;

public class SCR_DragDrop : SCR_GameplayStatics {

    //Listener to tell when character wants to interact
    private UnityAction<int> InteractListener;
    private UnityAction<int> UpListener;
    private UnityAction<int> TurnListener;
    private UnityAction<string> DisableListener;
    private bool Interact = false;
    private SCR_IKToolset IkTools;
    //The rigidbody of the object we will be moving
    private Rigidbody RBody;
    [HideInInspector]
    public bool IsZ;
    private bool Inside;
    private float Weight;
    private float InitialSpeed;
    private bool Lerping;
    [SerializeField]
    private bool LerpWhilePlayerClose;
    [SerializeField]
    [ShowIf("LerpWhilePlayerClose")]
    [ValidateInput("GreaterThanZero", "Our lerp speed must be greater than zero")]
    private float LerpSpeed;
    [SerializeField]
    [ShowIf("LerpWhilePlayerClose")]
    [ValidateInput("IsNull", "We must have a reference to the point on the box we would like to lerp towards")]
    private GameObject ReferencePoint;
    [SerializeField]
    [Tooltip("The farthest left a left hand effector is allowed to go on this object")]
    [ValidateInput("IsNull", "There must be a reference to the Left Endpoint Game Object!")]
    private GameObject ZLeftEndPoint;
    [SerializeField]
    [Tooltip("The farthest right a left hand effector is allowed to go on this object")]
    [ValidateInput("IsNull", "There must be a reference to the Left Endpoint Game Object!")]
    private GameObject ZRightEndPoint;
    [SerializeField]
    [Tooltip("Left hand IK effector on box when dragging from Z")]
    [ValidateInput("IsNull", "There must be a reference to the Left Effector Game Object!")]
    private GameObject ZEffectorLeft;
    [SerializeField]
    [Tooltip("Right hand IK effector on box when dragging from Z")]
    [ValidateInput("IsNull", "There must be a reference to the Right Effector Game Object!")]
    private GameObject ZEffectorRight;
    [SerializeField]
    [ValidateInput("IsNull", "There must be a reference to the Left Effector Game Object!")]
    private GameObject NegativeXEffectorLeft;
    [SerializeField]
    [ValidateInput("IsNull", "There must be a reference to the Right Effector Game Object!")]
    private GameObject NegativeXEffectorRight;
    [SerializeField]
    [ValidateInput("IsNull", "There must be a reference to the Left Effector Game Object!")]
    private GameObject PositiveXEffectorLeft;
    [SerializeField]
    [ValidateInput("IsNull", "There must be a reference to the Right Effector Game Object!")]
    private GameObject PositiveXEffectorRight;
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
    private float ClamberLerpThreshold;
    [SerializeField]
    private GameObject MeshCollider;
    [SerializeField]
    private bool BoxCanFall;
    [SerializeField]
    private GameObject[] Siblings;
    [SerializeField]
    [ShowIf("BoxCanFall")]
    private float LandedYAdjustment;
    [SerializeField]
    [ShowIf("BoxCanFall")]
    private float LagBeforeFall;
    [SerializeField]
    [ShowIf("BoxCanFall")]
    private float LagAfterGrounded;


    [HideInInspector]
    public SCR_CharacterManager CharacterManager;
    private bool LockedOut;
    private bool ClamberAllowed = true;
    private bool Moving;
    private bool BoxGrounded = true;
    private RaycastHit Output;
    private GameObject OnTopOfBox;
    Vector3 PositionDifferences;

    private void Awake()
    {
        //Register the callback functions related to each listener. These will be called as
        //the events these listeners are listening to get invoked 
        InteractListener = new UnityAction<int>(InteractPressed);
        UpListener = new UnityAction<int>(UpPressed);
        TurnListener = new UnityAction<int>(CharacterTurned);
        DisableListener = new UnityAction<string>(DisableSiblingGameObjects);
    }

    private void OnEnable()
    {
        //Register listeners with their events in the EventManager
        SCR_EventManager.StartListening("InteractKey", InteractListener);
        SCR_EventManager.StartListening("UpKey", UpListener);
        SCR_EventManager.StartListening("CharacterTurn", TurnListener);
        SCR_EventManager.StartListening("DisableBox", DisableListener);
    }

    private void OnDisable()
    {
        //Tell the EventManager we are no longer listening as the CharacterManager gets disabled.
        SCR_EventManager.StopListening("InteractKey", InteractListener);
        SCR_EventManager.StopListening("UpKey", UpListener);
        SCR_EventManager.StopListening("CharacterTurn", TurnListener);
        SCR_EventManager.StopListening("DisableBox", DisableListener);
    }

    private void FallBoxColliders()
    {
        MeshCollider.GetComponent<BoxCollider>().enabled = true;
        gameObject.transform.parent.gameObject.GetComponent<BoxCollider>().enabled = false;
    }
    private void DragBoxColliders()
    {
        MeshCollider.GetComponent<BoxCollider>().enabled = false;
        gameObject.transform.parent.gameObject.GetComponent<BoxCollider>().enabled = true;
    }

    private void InteractPressed(int value)
    {
        //The value passed by the event indicates whether or not the key is pressed down.
        if (value == 1)
        {
            Interact = true;
            if (Inside && !LockedOut && BoxGrounded)
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
            CharacterManager.StopPushing();
            if (Inside && !LockedOut)
            {
                IkTools.StartEffectorLerp("LeftHand", LeftHandCurves[2], 0.75f, true);
                IkTools.StartEffectorLerp("RightHand", LeftHandCurves[2], 0.75f, true);
            }
            FreezeAll();
        }            
    }

    private void WithinThreshold()
    {
        if ((gameObject.transform.position.y - Character.transform.position.y) > ClamberLerpThreshold) ClamberAllowed = false;
        else ClamberAllowed = true;
    }

    private void DisableSiblingGameObjects(string BoxName)
    {
        if(BoxName == gameObject.transform.parent.name)
        {
            ReleaseHands();
            CharacterManager.StopPushing();
            RBody.velocity = new Vector3(0.0f, 0.0f, 0.0f);
            FreezeAll();
            for (int i = 0; i < Siblings.Length; ++i)
            {
                Siblings[i].SetActive(false);
            }
            FallBoxColliders();
            gameObject.SetActive(false);
            SCR_EventManager.TriggerEvent("LeftKey", 0);
            SCR_EventManager.TriggerEvent("RightKey", 0);
        }
    }

    private void UpPressed(int value)
    {
        if (BoxCanFall) WithinThreshold();
        if (value == 1 && Inside && LerpWhilePlayerClose && (CharacterManager.InteractingWith == null || CharacterManager.InteractingWith == gameObject) && !Lerping && ClamberAllowed)
        {
            if(!CharacterManager.InteractingWith == gameObject)
            {
                SetXEffectors();
                IkTools.StartEffectorLerp("LeftHand", LeftHandCurves[0], 0.75f, false);
                IkTools.StartEffectorLerp("RightHand", LeftHandCurves[0], 0.75f, false);
            }
            CharacterManager.InteractingWith = gameObject;
            Lerping = true;
            StartCoroutine(ClamberLerp());
            CharacterManager.FreezeVelocity(SCR_CharacterManager.CharacterStates.Idling);
        }
    }

    private void ReleaseHands()
    {
        IkTools.StartEffectorLerp("LeftHand", LeftHandCurves[2], 0.75f, false);
        IkTools.StartEffectorLerp("RightHand", LeftHandCurves[2], 0.75f, false);
    }

    private void CharacterTurned(int value)
    {
        if (IsZ && Interact)
        {
            //Lerp effectors when the character is in the propper trigger and has pressed the interact key down while turning
            IkTools.StartEffectorLerp("LeftHand", LeftHandCurves[3], 0.75f, false);
            IkTools.StartEffectorLerp("RightHand", RightHandCurves[3], 0.75f, false);
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
        Inside = false;
        if (BoxCanFall) FallBoxColliders();
        else DragBoxColliders();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Character")
        {
            Inside = true;
            if (Interact && (CharacterManager.InteractingWith == null || CharacterManager.InteractingWith == gameObject))
            {
                if (!LockedOut)
                {
                    IkTools.StartEffectorLerp("LeftHand", LeftHandCurves[0], 0.75f, false);
                    IkTools.StartEffectorLerp("RightHand", RightHandCurves[0], 0.75f, false);
                }
                if(BoxGrounded) EnteredAndInteracted();
            }
        }
        else Inside = false;
    }

    private void SetXEffectors()
    {
        if((CharacterManager.InteractingWith == null || CharacterManager.InteractingWith == gameObject) && Inside)
        {
            if (gameObject.transform.parent.transform.position.x - Character.transform.position.x > 0.0f)
            {
                IkTools.SetEffectorTarget("LeftHand", PositiveXEffectorLeft);
                IkTools.SetEffectorTarget("RightHand", PositiveXEffectorRight);
            }
            else
            {
                IkTools.SetEffectorTarget("LeftHand", NegativeXEffectorLeft);
                IkTools.SetEffectorTarget("RightHand", NegativeXEffectorRight);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Character")
        {
            if(BoxCanFall && !(OnTopOfBox == null))
            {
                if (OnTopOfBox.GetComponentInChildren<SCR_DragDrop>().Inside && !LockedOut)
                {
                    LockedOut = true;
                }
                else if (!OnTopOfBox.GetComponentInChildren<SCR_DragDrop>().Inside && LockedOut)
                {
                    LockedOut = false;
                }
            }
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
        FreezeAll();

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Character")
        {
            
            Inside = false;
            CharacterManager.InteractingWith = null;
            FreezeAll();
        }
    }

    private void EnteredAndInteracted()
    {
        if((CharacterManager.InteractingWith == null || CharacterManager.InteractingWith == gameObject) && !LockedOut)
        {
            //If the character is grounded, in the trigger, and pressing an interact key. Allow the box to move, limit player speed
            //and set the velocity of the object to be moved
            CharacterManager.StartPushing();
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
                        IkTools.StartEffectorLerp("LeftHand", LeftHandCurves[0], 0.75f, false);
                        IkTools.StartEffectorLerp("RightHand", RightHandCurves[0], 0.75f, false);
                    }
                    else
                    {
                        IkTools.StartEffectorLerp("LeftHand", LeftHandCurves[1], 0.75f, false);
                        IkTools.StartEffectorLerp("RightHand", RightHandCurves[1], 0.75f, false);
                    }
                }
            }
            else
            {
                SetXEffectors();
                IkTools.StartEffectorLerp("LeftHand", LeftHandCurves[0], 0.75f, false);
                IkTools.StartEffectorLerp("RightHand", RightHandCurves[0], 0.75f, false);
            }
        }
    }

    private void CalculateDir()
    {
        if (CharacterManager.PlayerGrounded)
        {
            if (BoxGrounded)
            {
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
    }


    private void FireTraceDown()
    {
        Vector3 YOffset = new Vector3(0.0f, YTraceOffset, 0.0f);
        Vector3 CenterPosition = MeshCollider.transform.position + YOffset;
        RaycastHit Result;
        if (Physics.Raycast(CenterPosition, Vector3.down, out Result, GroundTraceDistance, GroundLayer))
        {
            if (BoxCanFall)
            {
                if (Result.collider.gameObject.tag == "PuzzleBox")
                {
                    if ((OnTopOfBox == null))
                    {
                        OnTopOfBox = Result.collider.gameObject;
                        SCR_DragDrop Current = OnTopOfBox.GetComponentInChildren<SCR_DragDrop>();
                        Current.LockedOut = true;
                        Current.ClamberAllowed = false;
                    }
                    else
                    {
                        if (OnTopOfBox.name != Result.collider.gameObject.name)
                        {
                            Debug.Log("Switch Happened");
                            SCR_DragDrop Current = Result.collider.gameObject.GetComponentInChildren<SCR_DragDrop>();
                            Current.LockedOut = true;
                            Current.ClamberAllowed = false;
                            SCR_DragDrop Old = OnTopOfBox.GetComponentInChildren<SCR_DragDrop>();
                            Old.LockedOut = false;
                            Old.ClamberAllowed = true;
                            OnTopOfBox = Result.collider.gameObject;
                        }
                    }
                }
                if (!BoxGrounded) StartCoroutine(Timer(LagAfterGrounded, PositionDifferences, BoxStopFall));
                BoxGrounded = true;
            }

        }
        else
        {
            if (BoxCanFall)
            {
                if (BoxGrounded)
                {
                    PositionDifferences = MeshCollider.transform.localPosition;
                    //PositionDifferences = new Vector3[Siblings.Length];
                    //for (int i = 0; i < PositionDifferences.Length; ++i)
                    //{
                    //    PositionDifferences[i] = Siblings[i].transform.position - MeshCollider.transform.position;
                    //}
                    StartCoroutine(Timer(LagBeforeFall, BoxStartFall));
                }
                BoxGrounded = false;
            }
        }
        Output = Result;
    }

    private void BoxStartFall()
    {
        CharacterManager.FreezeVelocity(SCR_CharacterManager.CharacterStates.Idling);
        StartCoroutine(Timer(0.75f, CharacterManager.UnfreezeVelocity));
        Interact = false;
        CharacterManager.StopPushing();
        if (Inside && !LockedOut)
        {
            IkTools.StartEffectorLerp("LeftHand", LeftHandCurves[2], 0.75f, true);
            IkTools.StartEffectorLerp("RightHand", LeftHandCurves[2], 0.75f, true);
        }
        Rigidbody MeshBody = MeshCollider.GetComponent<Rigidbody>();
        RBody.velocity = Vector3.zero;
        MeshBody.velocity = Vector3.zero;
        MeshBody.constraints = (~RigidbodyConstraints.FreezePositionX & ~RigidbodyConstraints.FreezePositionZ & ~RigidbodyConstraints.FreezePositionY) &
                (RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | ~RigidbodyConstraints.FreezeRotationZ);
        MeshBody.useGravity = true;
    }

    private void BoxStopFall(Vector3 Diff)
    {
        SCR_DragDrop Old = OnTopOfBox.GetComponentInChildren<SCR_DragDrop>();
        Old.LockedOut = false;
        Old.ClamberAllowed = true;
        Vector3 Adjustment = new Vector3(0.0f, LandedYAdjustment, 0.0f);
        gameObject.transform.parent.position = MeshCollider.transform.position + Adjustment;
        MeshCollider.transform.localPosition = Diff;

        Rigidbody MeshBody = MeshCollider.GetComponent<Rigidbody>();
        MeshBody.constraints = (RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezePositionY) |
                (RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ);
        MeshBody.useGravity = false;
        FreezeAll();
        LockedOut = false;
        ClamberAllowed = true;
        BoxCanFall = false;
        DragBoxColliders();
    }

    private void NullHands()
    {
        IkTools.ForceEffectorWeight("LeftHand", 0.0f);
        IkTools.ForceEffectorWeight("RightHand", 0.0f);
        IkTools.SetEffectorTarget("LeftHand", null);
        IkTools.SetEffectorTarget("RightHand", null);
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
        if(CharacterManager.InteractingWith == gameObject)
        {
            Moving = false;
            if (!IsZ && Interact)
            {
                CharacterManager.InteractingWith = null;
            }
            if (IsZ)
            {
                CharacterManager.InteractingWith = null;
                IkTools.StartEffectorLerp("LeftHand", LeftHandCurves[2], 0.75f, true);
                IkTools.StartEffectorLerp("RightHand", LeftHandCurves[2], 0.75f, true);
            }

            //Freeze rigidbody via constraints, and return the player to their original speed
            CharacterManager.SetSpeed(InitialSpeed);
            RBody.constraints = (RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezePositionY) |
                (RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ);
        }
    }

    public void Lockout()
    {
        ReleaseHands();
        CharacterManager.StopPushing();
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
                | (RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ);
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
            if (Vector3.Magnitude(Left - Right) > Vector3.Magnitude(ZLeftEndPoint.transform.position - Right))
            {
                if(IsZ) IkTools.SetEffectorTarget("LeftHand", ZLeftEndPoint);
            }
            else
            {
                if (IsZ) IkTools.SetEffectorTarget("LeftHand", ZEffectorLeft);
            }
            if (Vector3.Magnitude(Right - Left) > Vector3.Magnitude(ZRightEndPoint.transform.position - Left))
            {
                if (IsZ) IkTools.SetEffectorTarget("RightHand", ZRightEndPoint);
            }
            else
            {
                if (IsZ) IkTools.SetEffectorTarget("RightHand", ZEffectorRight);
            }
        }
    }

    IEnumerator ClamberLerp()
    {
        float TimeSlice = 0.0f;
        Vector3 StartPoint = Character.transform.position;
        Vector3 MidPoint = ReferencePoint.transform.position;
        Vector3 EndPoint = MidPoint;
        bool DoOnce = false;
        MidPoint.x = Character.transform.position.x;
        while (TimeSlice < 1.0f)
        {
            TimeSlice += Time.deltaTime * LerpSpeed;
            Character.transform.position = Vector3.Lerp(StartPoint, MidPoint, TimeSlice);
            if(TimeSlice < 0.5f && !DoOnce) ReleaseHands();
            yield return null;
        }
        TimeSlice = 0.0f;
        while (TimeSlice < 1.0f)
        {
            TimeSlice += Time.deltaTime * LerpSpeed;
            Character.transform.position = Vector3.Lerp(MidPoint, EndPoint, TimeSlice);
            yield return null;
        }
        Lerping = false;
        CharacterManager.StopPushing();
        CharacterManager.InteractingWith = null;
        CharacterManager.UnfreezeVelocity();
        StartCoroutine(Timer(0.15f, NullHands));
    }

    private void FixedUpdate()
    {
        FireTraceDown();
        EffectorCalculations();
        if(!LockedOut && (Moving || !BoxGrounded)) CalculateDir();
    }
}
