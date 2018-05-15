using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CharacterController : MonoBehaviour {

    //Event listeners per action for receiving events fired by the Input Manager
    private UnityAction<int> UpListener;
    private UnityAction<int> DownListener;
    private UnityAction<int> LeftListener;
    private UnityAction<int> RightListener;
    private UnityAction<int> InteractListener;

    //Booleans that will signify what input is being held down. For example, Up is true whenever an Up
    //Key is held down, and so on
    private bool Up = false;
    private bool Down = false;
    private bool Left = false;
    private bool Right = false;
    private bool Interact = false;

    //MoveDir is a boolean that signifies what direction the player is moving in, Right(true) or Left(false).
    private bool MoveDir = true;
    //MoveVec is the vector we are moving along. Will flip as MoveDir changes value
    private Vector3 MoveVec;
    //Reference to the character's rigidbody
    private Rigidbody RBody;
    [SerializeField]
    [Tooltip("Determines the maximum speed our character can move.")]
    private float MoveSpeed;
    [SerializeField]
    [Tooltip("Acceleration factor. This effects how quickly the player can start moving, stop moving, and change direction.")]
    private float Acceleration;
    //Value used in accelerating player
    private float LerpValue = 0.0f;


    private void Awake()
    {
        //Register the callback functions related to each listener. These will be called as
        //the events these listeners are listening to get invoked 
        UpListener = new UnityAction<int>(UpPressed);
        DownListener = new UnityAction<int>(DownPressed);
        LeftListener = new UnityAction<int>(LeftPressed);
        RightListener = new UnityAction<int>(RightPressed);
        InteractListener = new UnityAction<int>(InteractPressed);
    }

    private void OnEnable()
    {
        //Register listeners with their events in the EventManager
        EventManager.StartListening("UpKey", UpListener);
        EventManager.StartListening("DownKey", DownListener);
        EventManager.StartListening("LeftKey", LeftListener);
        EventManager.StartListening("RightKey", RightListener);
        EventManager.StartListening("InteractKey", InteractListener);
    }

    private void OnDisable()
    {
        //Tell the EventManager we are no longer listening as the CharacterController gets disabled.
        EventManager.StopListening("UpKey", UpListener);
        EventManager.StopListening("DownKey", DownListener);
        EventManager.StopListening("LeftKey", LeftListener);
        EventManager.StopListening("RightKey", RightListener);
        EventManager.StopListening("InteractKey", InteractListener);
    }

    //The following 5 functions are callbacks that get called by the event listeners
    private void UpPressed(int value)
    {
        //The value passed by the event indicates whether or not the key is pressed down. 
        if (value == 1)
            Up = true;
        else
            Up = false;
    }
    private void DownPressed(int value)
    {
        //The value passed by the event indicates whether or not the key is pressed down.
        if (value == 1)
            Down = true;
        else
            Down = false;
    }
    private void LeftPressed(int value)
    {
        //The value passed by the event indicates whether or not the key is pressed down.
        if (value == 1)
        {
            Left = true;
            //If we are currently running right when we recieve this left keypress, turn the character.
            if (MoveDir)
                TurnCharacter();
        }
        else
            Left = false;
    }
    private void RightPressed(int value)
    {
        //The value passed by the event indicates whether or not the key is pressed down.
        if (value == 1)
        {
            Right = true;
            //If we are currently running right when we recieve this left keypress, turn the character.
            if (!MoveDir)
                TurnCharacter();
        }
        else
            Right = false;
    }
    private void InteractPressed(int value)
    {
        //The value passed by the event indicates whether or not the key is pressed down.
        if (value == 1)
            Interact = true;
        else
            Interact = false;
    }

    // Use this for initialization
    void Start () {
        //Make sure the current character has a Rigidbody component
        if (GetComponent<Rigidbody>())
        {
            RBody = GetComponent<Rigidbody>();
        }
        else
        {
            Debug.LogError("There is currently not a rigidbody attached to this character");
        }
        //Our first move direction will just be the forward vector of the player
        MoveVec = transform.forward;
        //Make sure whoever is editing acceleration in the inspector uses a non negative value. At values higher 
        //than 100.0f, the acceleration is effectiviely instant
        Acceleration = Mathf.Clamp(Acceleration, 0.0f, 100.0f);
	}


    private void FixedUpdate()
    {
        //Do movement calculations. Needs to be in FixedUpdate and not Update because we are messing with physics.
        MoveCharacter(Time.deltaTime);
    }

    private void TurnCharacter()
    {
        //Reverse the move direction
        MoveDir = !MoveDir;
        MoveVec = -MoveVec;
        //Reset LerpValue so the player has to accelerate up to speed again when they turn. 
        LerpValue = 0.0f;
    }

    private void MoveCharacter(float DeltaTime)
    {
        if (Left || Right)
        {
            if(LerpValue < 1.0f)
            {
                //Increase LerpValue to 1.0f at the rate determined by Acceleration so that the player ramps up to max velocity.
                LerpValue += Acceleration * DeltaTime;
            }
            else
            {
                LerpValue = 1.0f;
            }
        }
        else
        {
            if (LerpValue > 0.0f)
            {
                //Decrease LerpValue to 0.0f at the rate determined by Acclereation so that the player ramps down to zero velocity.
                LerpValue -= Acceleration * DeltaTime;
            }
            else
            {
                LerpValue = 0.0f;
            }
        }
        //Calculate velocity of player
        RBody.velocity = MoveVec * MoveSpeed * LerpValue;
    }

    //Might need later, idk. 
    //void Update () {

    //}
}
