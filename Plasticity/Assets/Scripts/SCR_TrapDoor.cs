using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;

public class SCR_TrapDoor : SCR_GameplayStatics {

    private enum ThingLiftingTrapDoor { Character, AI, Both };
    [SerializeField]
    private ThingLiftingTrapDoor TrapDoorLifter;
    private ThingLiftingTrapDoor WhoLifted;

    [SerializeField]
    [Tooltip("Reference to the trapdoor object with the hinge component attached")]
    [ValidateInput("IsNull", "We must have a reference to the trapdoor child game object")]
    private GameObject TrapDoor;
    [SerializeField]
    [Tooltip("The animation curve that defines how much torque the player puts on the door over time")]
    private AnimationCurve TorqueOverTime;
    [SerializeField]
    [Tooltip("How long you want the player to push the door up")]
    [ValidateInput("GreaterThanZero", "Must be an amount of time greater than zero")]
    private float Duration;
    [SerializeField]
    [Tooltip("Determines whether or not the interaction with the trap door can only happen once")]
    private bool DoOnce;
    [SerializeField]
    [Tooltip("Game object defining the IK target of the Left Hand Effector")]
    [ValidateInput("IsNull", "We must have a Left Hand Effector Game Object")]
    private GameObject LeftHandEffector;
    [SerializeField]
    [Tooltip("Game object defining the IK target of the Right Hand Effector")]
    [ValidateInput("IsNull", "We must have a Right Hand Effector Game Object")]
    private GameObject RightHandEffector;
    [SerializeField]
    [Tooltip("The animation curve that defines how FinalIK will drag the character's left hand to the TrapDoor")]
    [ValidateInput("NotEmpty", "We must have some animation curves specified")]
    private AnimationCurve[] LeftHandCurves;
    [SerializeField]
    [Tooltip("The animation curve that defines how FinalIK will drag the character's right hand to the TrapDoor")]
    [ValidateInput("NotEmpty", "We must have some animation curves specified")]
    private AnimationCurve[] RightHandCurves;
    [SerializeField]
    [Tooltip("Specifies whether or not we want to fire an event when the player opens the trap door")]
    private bool TriggerOnOpen;
    [SerializeField]
    [ShowIf("TriggerOnOpen")]
    [Tooltip("This is the ID of the setting in the SceneLoader that we would like to load")]
    private string OpenTriggerName;
    
    private bool Done;
    private float CalculationDuration;
    private float PreDuration;
    private Rigidbody TrapDoorRB;
    private HingeJoint TrapDoorHJ;
    private UnityAction<int> InteractListener;
    private bool Interact;
    private bool Inside;
    private bool LiftDoor;
    private SCR_CharacterManager CharacterManager;
    private SCR_IKToolset IkTools;
    private bool CharacterCanLift;
    private bool AICanLift;

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

    private void InteractPressed(int input)
    {
        if(input == 1)
        {
            Interact = true;
            if (Inside && CharacterManager.InteractingWith == null && !Done && CharacterCanLift)
            {
                WhoLifted = ThingLiftingTrapDoor.Character;
                BeginLift();
            }
        }
        else Interact = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Character")
        {
            Inside = true;
            CharacterManager = other.gameObject.GetComponent<SCR_CharacterManager>();
            IkTools = other.gameObject.GetComponent<SCR_IKToolset>();
            if (Interact && CharacterManager.InteractingWith == null && !Done && CharacterCanLift)
            {
                WhoLifted = ThingLiftingTrapDoor.Character;
                BeginLift();
            }
        }
        else if (other.tag == "AI")
        {
            if (AICanLift)
            {
                WhoLifted = ThingLiftingTrapDoor.AI;
                BeginLift();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Character")
        {
            Inside = false;
            IkTools.SetEffectorTarget("LeftHand", null);
            IkTools.SetEffectorTarget("RightHand", null);
        }
    }

    private void BeginLift()
    {
        if (WhoLifted == ThingLiftingTrapDoor.Character)
        {
            CharacterManager.InteractingWith = gameObject;
            CharacterManager.FreezeVelocity(SCR_CharacterManager.CharacterStates.Idling);
            IkTools.SetEffectorTarget("LeftHand", LeftHandEffector);
            IkTools.SetEffectorTarget("RightHand", RightHandEffector);
            IkTools.StartEffectorLerp("LeftHand", LeftHandCurves[0], 0.5f);
            IkTools.StartEffectorLerp("RightHand", RightHandCurves[0], 0.5f);
        }
        LiftDoor = true;

        //Do effector calculations
    }

    private void EndLift()
    {
        if (WhoLifted == ThingLiftingTrapDoor.Character)
        {
            IkTools.StartEffectorLerp("LeftHand", LeftHandCurves[1], 0.5f);
            IkTools.StartEffectorLerp("RightHand", RightHandCurves[1], 0.5f);
            CharacterManager.InteractingWith = null;
            CharacterManager.UnfreezeVelocity();
        }
        LiftDoor = false;
        if (TriggerOnOpen) SCR_EventManager.TriggerEvent("LevelTrigger", OpenTriggerName);
        if (DoOnce) Done = true;
        CalculationDuration = Duration;
        PreDuration = 0.5f;
        //Do effector calculations
    }

    // Use this for initialization
    void Start () {
        TrapDoorRB = TrapDoor.GetComponent<Rigidbody>();
        TrapDoorHJ = TrapDoor.GetComponent<HingeJoint>();
        CalculationDuration = Duration;
        PreDuration = 0.5f;
        if (TrapDoorLifter == ThingLiftingTrapDoor.Character || TrapDoorLifter == ThingLiftingTrapDoor.Both) CharacterCanLift = true;
        if (TrapDoorLifter == ThingLiftingTrapDoor.AI || TrapDoorLifter == ThingLiftingTrapDoor.Both) AICanLift = true;
	}

    private void FixedUpdate()
    {
        if (LiftDoor) TorqueCalculations(Time.deltaTime);
    }

    private void TorqueCalculations(float DeltaTime)
    {
        if (PreDuration <= 0.0f)
        {
            if (CalculationDuration <= 0.0f) EndLift();
            else
            {
                CalculationDuration -= DeltaTime;
                TrapDoorRB.AddTorque(TrapDoorHJ.axis * (TorqueOverTime.Evaluate(Duration-CalculationDuration)));
            }
        }
        else PreDuration -= DeltaTime;
    }
}
