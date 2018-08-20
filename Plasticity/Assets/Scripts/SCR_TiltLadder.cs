using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;

public class SCR_TiltLadder : SCR_GameplayStatics {
    [SerializeField]
    [Tooltip("How quickly the girl can apply torque to the ladder that she is pushing")]
    private float StrengthOfGirl;
    [SerializeField]
    [Tooltip("A game object defining the location that the character will place their left hand on the ladder")]
    [ValidateInput("IsNull", "We must have a Left Hand Effector Game Object")]
    private GameObject LeftHandEffector;
    [SerializeField]
    [Tooltip("A game object defining the location that the character will place their right hand on the ladder")]
    [ValidateInput("IsNull", "We must have a Right Hand Effector Game Object")]
    private GameObject RightHandEffector;
    [SerializeField]
    [Tooltip("The animation curve that defines how FinalIK will drag the character's left hand to the ladder")]
    [ValidateInput("NotEmpty", "We must have some animation curves specified")]
    private AnimationCurve[] LeftHandCurves;
    [SerializeField]
    [Tooltip("The nimation curve that defines how FinalIK will drag the character's right hand to the ladder")]
    [ValidateInput("NotEmpty", "We must have some animation curves specified")]
    private AnimationCurve[] RightHandCurves;
    [SerializeField]
    [Tooltip("The movement speed the player will be capped at once they start tilting the ladder")]
    private float SlowDownSpeed;
    [SerializeField]
    [Tooltip("The game object located at the position of the anchor of the ladder's hinge joint")]
    private GameObject Anchor;
    [SerializeField]
    [Tooltip("How fast the player will shift into position when changing z plane")]
    private float LerpSpeed;
    [SerializeField]
    [Tooltip("How far the player will slide to the left and right before changing z plane")]
    private float LeftRightOffset;
    [SerializeField]
    private float TimerLength;
    [SerializeField]
    [Tooltip("Specifies whether or not we want to fire an event when the player begins changing plane")]
    private bool TriggerOnLerping;
    [SerializeField]
    [ShowIf("TriggerOnLerping")]
    [Tooltip("This is the ID of the setting in the SceneLoader that we would like to load")]
    private string LerpingTriggerName;


    //Input event listeners
    private UnityAction<int> InteractListener;
    private UnityAction<int> UpListener;

    //Is true while the player has the interact key pressed down
    private bool Interact;
    //Is true while the player is in the side grabbing trigger
    private bool Inside;
    //Is true while the player is physically moving the ladder
    private bool PushEnabled = false;

    //Vectors defining how far left and right the player will move when shifting
    private Vector3 LeftTarget;
    private Vector3 RightTarget;

    //Reference to character
    private GameObject Character;
    //Reference to our IkToolset
    private SCR_IKToolset IkTools;
    //Reference to the character managers
    private SCR_CharacterManager CharacterManager;
    private Rigidbody LadderRB;
    private HingeJoint LadderHJ;

    //How fast was the player moving before slowing down to grab
    private float InitialSpeed;
    //When we trigger an event, we need to store the ID of the event that the Ladder script triggers
    private string LadderTriggerName;
    //Which direction are we currently lerping the player
    private int LerpDir = 0;
    private float TiltTimer = 0.0f;
    private int Up;

    private void Awake()
    {
        //Register the callback functions related to each listener. These will be called as
        //the events these listeners are listening to get invoked 
        InteractListener = new UnityAction<int>(InteractPressed);
        UpListener = new UnityAction<int>(UpPressed);
    }


    private void OnEnable()
    {
        //Register listeners with their events in the EventManager
        SCR_EventManager.StartListening("InteractKey", InteractListener);
        SCR_EventManager.StartListening("UpKey", UpListener);
    }

    private void OnDisable()
    {
        SCR_EventManager.StopListening("InteractKey", InteractListener);
        SCR_EventManager.StopListening("UpKey", UpListener);
    }

    private void Start()
    {
        LadderRB = gameObject.transform.parent.GetComponent<Rigidbody>();
        LadderHJ = gameObject.transform.parent.GetComponent<HingeJoint>();
    }

    private void InteractPressed(int value)
    {
        //The value passed by the event indicates whether or not the key is pressed down.
        if (value == 1)
        {
            Interact = true;
            if (Inside)
            {
                GrabLadder();
            }
        }
      else
        {
            Interact = false;
            if (Inside)
            {
                ReleaseLadder();
            }
        }
    }

    private void UpPressed(int value)
    {
        Up = value;
        if (value == 1 && Inside)
        {
            //When we begin shifting the player, we need to check if we are supposed to fire an event, and then tell ladder to fire our event
            DeactivateLadderZone();

            //If the character is moving to the right or left , define where we need to lerp to relative to the anchor, and then define Lerp dir
            //to allow lerp to be called in update
            if (transform.up.x > 0.0f && CharacterManager.MoveDir)
            {
                CharacterManager.FreezeVelocity(SCR_CharacterManager.CharacterStates.Idling);
                LerpDir = 1;
                Vector3 FirstTarget = new Vector3();
                FirstTarget.x = Anchor.transform.position.x - LeftRightOffset;
                FirstTarget.y = CharacterManager.gameObject.transform.position.y;
                FirstTarget.z = CharacterManager.gameObject.transform.position.z;
                LeftTarget = FirstTarget;
            }
            if (transform.up.x < 0.0f && !CharacterManager.MoveDir)
            {
                CharacterManager.FreezeVelocity(SCR_CharacterManager.CharacterStates.Idling);
                LerpDir = 2;
                Vector3 FirstTarget = new Vector3();
                FirstTarget.x = Anchor.transform.position.x + LeftRightOffset;
                FirstTarget.y = CharacterManager.gameObject.transform.position.y;
                FirstTarget.z = CharacterManager.gameObject.transform.position.z;
                RightTarget = FirstTarget;
            }
        }
    }

    private void FixedUpdate()
    {
        ManageTimer(Time.deltaTime);
        if (LerpDir != 0)
        {
            DoLerp(Time.deltaTime);
            
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Character")
        {
            Inside = true;
            Character = other.gameObject;
            IkTools = Character.GetComponent<SCR_IKToolset>();
            CharacterManager = Character.GetComponent<SCR_CharacterManager>();
            InitialSpeed = CharacterManager.GetSpeed();
            CharacterManager.StateChangeLocked = true;
            //Need to structure it this way in case interact is held down by the player before entering trigger
            if (Interact)
            {
                GrabLadder();
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        //If the character is pushing the ladder every tick, add torque in the appropriate direction
        if (other.tag == "Character" && PushEnabled)
        {
            if (CharacterManager.MoveDir)
            {
                LadderRB.AddTorque(LadderHJ.axis * -(StrengthOfGirl));
            }
            else
            {
                LadderRB.AddTorque(LadderHJ.axis * (StrengthOfGirl));
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Character")
        {
            //Make sure the hand effectors are freed for other interactions
            Inside = false;
            CharacterManager.StateChangeLocked = false;
            if (Interact)
            {
                IkTools.SetEffectorTarget("LeftHand", null);
                IkTools.SetEffectorTarget("RightHand", null);
                ReleaseLadder();
            }
        }
    }

    private void GrabLadder()
    {
        //Move hands to the ladder, slow the player, and make sure they can interact with nothing else
        if (CharacterManager.InteractingWith == null)
        {
            IkTools.SetEffectorTarget("LeftHand", LeftHandEffector);
            IkTools.SetEffectorTarget("RightHand", RightHandEffector);
            IkTools.StartEffectorLerp("LeftHand", LeftHandCurves[0], 0.5f);
            IkTools.StartEffectorLerp("RightHand", RightHandCurves[0], 0.5f);
            CharacterManager.SetSpeed(SlowDownSpeed);
            CharacterManager.InteractingWith = gameObject;
            PushEnabled = true;
        }
    }

    private void ReleaseLadder()
    {
        //Interpolate hands back from their weighted locations and free the character to interact with other objects
        if (CharacterManager.InteractingWith == gameObject)
        {
            IkTools.SetEffectorTarget("LeftHand", LeftHandEffector);
            IkTools.SetEffectorTarget("RightHand", RightHandEffector);
            IkTools.StartEffectorLerp("LeftHand", LeftHandCurves[1], 0.5f);
            IkTools.StartEffectorLerp("RightHand", RightHandCurves[1], 0.5f);
            CharacterManager.SetSpeed(InitialSpeed);
            CharacterManager.InteractingWith = null;
            PushEnabled = false;
        }
    }

    private void DeactivateLadderZone()
    {
        if (TriggerOnLerping)
        {
            SCR_EventManager.TriggerEvent("LevelTrigger", LerpingTriggerName);
        }
    }

    private void EndLerp()
    {
        CharacterManager.UnfreezeVelocity();
        LerpDir = 0;
        TiltTimer = TimerLength;
        if(Up == 0)
        {
            SCR_EventManager.TriggerEvent("UpKey", 1);
            Up = 0;
        }
        else SCR_EventManager.TriggerEvent("UpKey", 1);


    }

    private void ManageTimer(float DeltaTime)
    {
        if (TiltTimer < 0.0f)
        {
            if(Up == 0) SCR_EventManager.TriggerEvent("UpKey", 0);
            TiltTimer = 0.0f;
        }
        else if (TiltTimer > 0.0f)
        {
            TiltTimer -= DeltaTime;
            if (TiltTimer == 0.0f) TiltTimer -= 0.1f;
        }
        else
        {
            return;
        }
    }

    //Actually calculate every tick where the player's position should be as they slide across z planes.
    private void DoLerp(float DeltaTime)
    {
        if(LerpDir == 1)
        {
            if (CharacterManager.gameObject.transform.position.x <= LeftTarget.x)
            {
                if (CharacterManager.gameObject.transform.position.z >= Anchor.transform.position.z) EndLerp();
                else
                {
                    Vector3 NewPos = CharacterManager.gameObject.transform.position;
                    NewPos.z += LerpSpeed * DeltaTime;
                    CharacterManager.gameObject.transform.position = NewPos;
                }
            }
            else
            {
                Vector3 NewPos = CharacterManager.gameObject.transform.position;
                NewPos.x -= LerpSpeed * DeltaTime;
                CharacterManager.gameObject.transform.position = NewPos;
            }
        }
        else
        {
            if (CharacterManager.gameObject.transform.position.x >= RightTarget.x)
            {
                if (CharacterManager.gameObject.transform.position.z >= Anchor.transform.position.z) EndLerp();
                else
                {
                    Vector3 NewPos = CharacterManager.gameObject.transform.position;
                    NewPos.z += LerpSpeed * DeltaTime;
                    CharacterManager.gameObject.transform.position = NewPos;
                }
            }
            else
            {
                Vector3 NewPos = CharacterManager.gameObject.transform.position;
                NewPos.x += LerpSpeed * DeltaTime;
                CharacterManager.gameObject.transform.position = NewPos;
            }
        }
    }
}
