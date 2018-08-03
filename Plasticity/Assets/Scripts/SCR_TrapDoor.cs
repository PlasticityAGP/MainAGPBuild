using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;

public class SCR_TrapDoor : SCR_GameplayStatics {

    [SerializeField]
    [Tooltip("Reference to the trapdoor object with the hinge component attached")]
    [ValidateInput("IsNull", "We must have a reference to the trapdoor child game object")]
    private GameObject TrapDoor;
    [SerializeField]
    [Tooltip("How long you want the player to push the door up")]
    [ValidateInput("GreaterThanZero", "Must be an amount of time greater than zero")]
    private float Duration;
    [SerializeField]
    [Tooltip("A multiplier on how much torque the girl applies to the trap door")]
    [ValidateInput("GreaterThanZero", "Must be a force multiplier that is greater than zero")]
    private float StrengthOfGirl;
    [SerializeField]
    [Tooltip("Determines whether or not the interaction with the trap door can only happen once")]
    private bool DoOnce;
    [SerializeField]
    [Tooltip("Specifies whether or not we want to fire an event when the player opens the trap door")]
    private bool TriggerOnOpen;
    [SerializeField]
    [ShowIf("TriggerOnOpen")]
    [Tooltip("This is the ID of the setting in the SceneLoader that we would like to load")]
    [ValidateInput("GreaterThanOrEqualZero", "Must be a non negative integer")]
    private int OpenTriggerID;
    
    private bool Done;
    private float CalculationDuration;
    private Rigidbody TrapDoorRB;
    private HingeJoint TrapDoorHJ;
    private UnityAction<int> InteractListener;
    private bool Interact;
    private bool Inside;
    private bool LiftDoor;
    private SCR_CharacterManager CharacterManager;
    //private SCR_IKToolset IkTools;

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
            if (Inside && CharacterManager.InteractingWith == null && !Done) BeginLift();
        }
        else Interact = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Character")
        {
            Inside = true;
            CharacterManager = other.gameObject.GetComponent<SCR_CharacterManager>();
            //IkTools = other.gameObject.GetComponent<SCR_IKToolset>();
            if (Interact && CharacterManager.InteractingWith == null && !Done) BeginLift();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.tag == "Character") Inside = false;
    }

    private void BeginLift()
    {
        CharacterManager.InteractingWith = gameObject;
        CharacterManager.FreezeVelocity();
        LiftDoor = true;

        //Do effector calculations
    }

    private void EndLift()
    {
        CharacterManager.InteractingWith = null;
        CharacterManager.UnfreezeVelocity();
        LiftDoor = false;
        if (TriggerOnOpen) SCR_EventManager.TriggerEvent("LevelTrigger", OpenTriggerID);
        if (DoOnce) Done = true;
        //Do effector calculations
    }

    // Use this for initialization
    void Start () {
        TrapDoorRB = TrapDoor.GetComponent<Rigidbody>();
        TrapDoorHJ = TrapDoor.GetComponent<HingeJoint>();
        CalculationDuration = Duration;
	}

    private void FixedUpdate()
    {
        if (LiftDoor) TorqueCalculations(Time.deltaTime);
    }

    private void TorqueCalculations(float DeltaTime)
    {
        CalculationDuration -= DeltaTime;
        if (CalculationDuration <= 0.0f) EndLift();
        else
        {
            TrapDoorRB.AddTorque(TrapDoorHJ.axis * (StrengthOfGirl));
        }
    }
}
