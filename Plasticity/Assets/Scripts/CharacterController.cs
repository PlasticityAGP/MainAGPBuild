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

    [SerializeField]
    [Tooltip("Determines the maximum speed our character can move.")]
    private float MoveSpeed = 6;
    [SerializeField]
    [Tooltip("Acceleration factor. This effects how quickly the player can start moving, stop moving, and change direction.")]
    private float Acceleration = 4;
    [SerializeField]
    [Tooltip("Determines if you want the player to be able to ajust their velocity mid air")]
    private bool MoveWhileJumping = false;
    [SerializeField]
    [Tooltip("If true, allows the player to continuously jump while UP is held down")]
    private bool JumpWhileHeld = false;
    [SerializeField]
    [Tooltip("How much force you want the player to jump with.")]
    private float JumpForce = 5;
    [SerializeField]
    [Tooltip("How quickly do you want the player to fall after jumping.")]
    private float GravityOnPlayer = 9;
    [SerializeField]
    [Tooltip("Trace distance for determining if the player has landed.")]
    private float GroundTraceDistance = 1.05f;
    [SerializeField]
    [Tooltip("The maximum angle between player and ground that is walkable by player")]
    private float MaxGroundAngle = 120;
    [SerializeField]
    [Tooltip("Layermask that signifies what objects are considered to be the ground.")]
    private LayerMask GroundLayer;

    //MoveDir is a boolean that signifies what direction the player is moving in, Right(true) or Left(false).
    private bool MoveDir = true;
    //MoveVec is the vector we are moving along. Will flip as MoveDir changes value
    private Vector3 MoveVec;
    //InitialDir vector is used for determining what direction player velocity should be in if they turn
    //mid jump while MoveWhileJumping is set to false
    private Vector3 InitialDir;
    private float SpeedModifier;
    //The current angle of the ground the player is walking on. Will be checked against MaxGroundAngle to
    //determine if a surface is walkable.
    private float GroundAngle = 90;
    private RaycastHit HitInfo;

    //Reference to the character's rigidbody
    private Rigidbody RBody;
    //Boolean used in Jump() to determine when to call OnBeginJump() and OnEndJump()
    private bool DidAJump = false;

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
        InitialDir = transform.forward;
        SpeedModifier = 0.0f;
        //Make sure whoever is editing acceleration in the inspector uses a non negative value. At values higher 
        //than 100.0f, the acceleration is effectiviely instant
        //Acceleration = Mathf.Clamp(Acceleration, 0.0f, 100.0f);
    }

    private void FixedUpdate()
    {
        //Do movement calculations. Needs to be in FixedUpdate and not Update because we are messing with physics.
        CalculateMoveVec();
        CalculateGroundAngle();
        Jump(Time.deltaTime);
        MoveCharacter(Time.deltaTime);
    }

    private bool IsGrounded()
    {
        //Create two locations to trace from so that we can have a little bit of 'dangle' as to whether
        //or not the character is on an object.
        Vector3 RightPosition = transform.position + (InitialDir.normalized * 0.25f);
        Vector3 LeftPosition = transform.position + (InitialDir.normalized * -0.25f);
        bool RightHit = false;
        bool LeftHit = false;
        RaycastHit Result;
        if (Physics.Raycast(RightPosition, Vector3.down, out Result, GroundTraceDistance, GroundLayer))
        {
            RightHit = true;
            if (MoveDir)
            {
                HitInfo = Result;
            }
        }
        if (Physics.Raycast(LeftPosition, Vector3.down, out Result, GroundTraceDistance, GroundLayer))
        {
            LeftHit = true;
            if (!MoveDir)
            {
                HitInfo = Result;
            }
        }
        return RightHit||LeftHit;

    }

    private void TurnCharacter()
    {
        //If we turn, we flip the boolean the signifies player direction
        MoveDir = !MoveDir;
    }

    private void CalculateMoveVec()
    {
        if (IsGrounded())
        {
            if (MoveDir)
            {
                MoveVec = Vector3.Cross(HitInfo.normal, -transform.right);
            }
            else
            {
                MoveVec = Vector3.Cross(HitInfo.normal, transform.right);
            }
        }
        else if (MoveWhileJumping)
        {
            float R = (MoveVec.x / InitialDir.x) + (MoveVec.z / InitialDir.z);
            //If the player should be moving in the direction they started towards, but their velocity is in the opposite of the start direction,
            //flip the direction of their velocity.
            if (MoveDir && (R < 0))
            {
                MoveVec.x = -MoveVec.x;
                MoveVec.z = -MoveVec.z;
            }
            //If the player should be moving away from the direction they started towards, but their velocity is the same direction as InitialDir,
            //flip the direction of their velocity. 
            else if (!MoveDir && (R > 0))
            {
                MoveVec.x = -MoveVec.x;
                MoveVec.z = -MoveVec.z;
            }
        }
    }

    private void CalculateGroundAngle()
    {
        if (!IsGrounded())
        {
            GroundAngle = 90.0f;
        }
        else
        {
            if (MoveDir)
            {
                GroundAngle = Vector3.Angle(HitInfo.normal, InitialDir);
            }
            else
            {
                GroundAngle = Vector3.Angle(HitInfo.normal, -InitialDir);
            }
            
        }
    }

    private void Jump(float DeltaTime)
    {
        //If the player has pressed an UP key and the player is currently standing on the ground
        if (Up && IsGrounded())
        {
            //Jump
            if (DidAJump) OnEndJump();
            if (Up)
            {
                if ((GroundAngle - 90) > 0) MoveVec.y = JumpForce + ((GroundAngle - 90)/6.0f);
                else MoveVec.y = JumpForce; 
            }
            
            OnBeginJump();
        }
        //If UP has not been pressed and the player is currently on the ground, the y component of their velocity should be zero
        else if (!Up && IsGrounded())
        {

            //Zero out velocity
            if (DidAJump) OnEndJump();
            
        }
        //In all other cases the player is falling. Here we calculate the y component of velocity while falling.
        else
        {
            //Falling
            if (!DidAJump) OnBeginJump();
            MoveVec.y -= GravityOnPlayer * DeltaTime;
            
        }
    }

    //This function gets called whenever a player begins jumping or begins falling.
    private void OnBeginJump()
    {
        DidAJump = true;
    }

    //This function gets called whenever a player lands after falling or jumping.
    private void OnEndJump()
    {
        DidAJump = false;
        //If we aren't continuously jumping, we need to reset the Up boolean so that the player
        //only jumps once per press of an UP key.
        if(!JumpWhileHeld) Up = false;

        //The following section is used to set the direction of the velocity of the player when they land and MoveWhileJumping isn't enabled
        //if (!MoveWhileJumping)
        //{
        //    //R is used to determine if the player's velocity vector is in the same direction as the InitialDir vector
        //    float R = (MoveVec.x / InitialDir.x) + (MoveVec.z / InitialDir.z);
        //    //If the player should be moving in the direction they started towards, but their velocity is in the opposite of the start direction,
        //    //flip the direction of their velocity.
        //    if (MoveDir && (R < 0))
        //    {
        //        MoveVec.x = -MoveVec.x;
        //        MoveVec.y = -MoveVec.y;
        //        MoveVec.z = -MoveVec.z;

        //    }
        //    //If the player should be moving away from the direction they started towards, but their velocity is the same direction as InitialDir,
        //    //flip the direction of their velocity. 
        //    else if(!MoveDir && (R > 0))
        //    {
        //        MoveVec.x = -MoveVec.x;
        //        MoveVec.y = -MoveVec.y;
        //        MoveVec.z = -MoveVec.z;

        //    }

        //}
    }

    private void MoveCharacter(float DeltaTime)
    {
        //Accelerate the player if a direction key is pressed and NOT when the player is in the air and MoveWhileJumping is disabled.
        //if (((Left && !MoveDir) || (Right && MoveDir)) && !(!MoveWhileJumping && !IsGrounded()))
        //{
        //    if (LerpValue < 1.0f)
        //    {
        //        //Increase LerpValue to 1.0f at the rate determined by Acceleration so that the player ramps up to max velocity.
        //        LerpValue += Acceleration * DeltaTime;
        //    }
        //    else
        //    {
        //        LerpValue = 1.0f;
        //    }
        //}
        ////Decellerate the player.
        //else
        //{
        //    if (LerpValue > 0.0f)
        //    {
        //        //Decrease LerpValue to 0.0f at the rate determined by Acclereation so that the player ramps down to zero velocity.
        //        if (!MoveWhileJumping && !IsGrounded()) LerpValue -= (Acceleration / 10.0f) * DeltaTime;
        //        else LerpValue -= Acceleration * DeltaTime;

        //    }
        //    else
        //    {
        //        LerpValue = 0.0f;
        //    }
        //}
        if ((Left && !MoveDir) && !(!MoveWhileJumping && !IsGrounded()))
        {
            SpeedModifier = 1.0f;
        }
        else if ((Right && MoveDir) && !(!MoveWhileJumping && !IsGrounded()))
        {
            SpeedModifier = 1.0f;
        }
        else
        {
            if (IsGrounded())
            {
                SpeedModifier -= Acceleration * DeltaTime;
                if (SpeedModifier <= 0.0f) SpeedModifier = 0.0f;
            }
            else
            {
                SpeedModifier -= (Acceleration/10.0f) * DeltaTime;
                if (SpeedModifier <= 0.0f) SpeedModifier = 0.0f;
            }
        }

        //Set velocity of player
        Debug.DrawRay(transform.position, MoveVec, Color.red);
        Vector3 FinalVel;
        if (IsGrounded() && !DidAJump)
        {
            FinalVel = new Vector3(MoveVec.x * MoveSpeed * SpeedModifier, MoveVec.y * MoveSpeed * SpeedModifier, MoveVec.z * MoveSpeed * SpeedModifier);
            if ((GroundAngle > MaxGroundAngle)) FinalVel = Vector3.zero;

        }
        else
        {
            FinalVel = new Vector3(MoveVec.x * MoveSpeed * SpeedModifier, MoveVec.y, MoveVec.z * MoveSpeed * SpeedModifier);
        }
        RBody.velocity = FinalVel;
    }

}
