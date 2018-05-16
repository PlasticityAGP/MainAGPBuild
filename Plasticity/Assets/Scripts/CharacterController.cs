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
    [Tooltip("Pass in a reference to the model for this character")]
    private GameObject RefToModel;
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
    //Hit info will store the hit result of the raycast that is shot from the player towards the ground
    private RaycastHit HitInfo;
    //CharacterAnimator will store a reference to the Animator of our Character.
    private Animator CharacterAnimator;

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
        if(RefToModel == null) Debug.LogError("You need to pass in a reference to the model you wish the character controller to use");

        //Make sure the current character has a Rigidbody component
        if (GetComponent<Rigidbody>()) RBody = GetComponent<Rigidbody>();
        else Debug.LogError("There is currently not a rigidbody attached to this character");

        //Make sure the model has an animator component
        if (RefToModel.GetComponent<Animator>()) CharacterAnimator = RefToModel.GetComponent<Animator>();
        else Debug.LogError("There is currently not an animator attached to this character's model");

        //Our first move direction will just be the forward vector of the player
        InitialDir = transform.forward;
        SpeedModifier = 0.0f;
        //Make sure whoever is editing acceleration in the inspector uses a non negative value. At values higher 
        //than 100.0f, the acceleration is effectiviely instant
        Acceleration = Mathf.Clamp(Acceleration, 0.0f, 100.0f);


    }

    private void FixedUpdate()
    {
        //Do movement calculations. Needs to be in FixedUpdate and not Update because we are messing with physics.
        CalculateMoveVec();
        CalculateGroundAngle();
        Jump(Time.deltaTime);
        MoveCharacter(Time.deltaTime);
        ManageAnimations();
    }

    private bool IsGrounded()
    {
        //Create two locations to trace from so that we can have a little bit of 'dangle' as to whether
        //or not the character is on an object.
        Vector3 RightPosition = transform.position + (InitialDir.normalized * 0.15f);
        Vector3 LeftPosition = transform.position + (InitialDir.normalized * -0.15f);
        RaycastHit Result;
        //Raycast to find slope of ground beneath us. Needs to extend lower than our raycast that decides if grounded 
        //because we want our velocity to match the slope of the surface slightly before we return true.
        //This prevents a weird bouncing effect that can happen after a player lands. 
        if (Physics.Raycast(RightPosition, Vector3.down, out Result, GroundTraceDistance + 0.3f, GroundLayer))
        {
            if (MoveDir)
            {
                HitInfo = Result;
            }
        }
        if (Physics.Raycast(LeftPosition, Vector3.down, out Result, GroundTraceDistance + 0.3f, GroundLayer))
        {
            if (!MoveDir)
            {
                HitInfo = Result;
            }
        }
        return (Physics.Raycast(LeftPosition, Vector3.down, GroundTraceDistance, GroundLayer)) || 
            (Physics.Raycast(RightPosition, Vector3.down, GroundTraceDistance, GroundLayer));

    }

    private void TurnCharacter()
    {
        //If we turn, we flip the boolean the signifies player direction
        MoveDir = !MoveDir;
        Vector3 TurnAround = new Vector3(0.0f, 180.0f, 0.0f);
        RefToModel.transform.Rotate(TurnAround);
        if (IsGrounded())
        {
            SpeedModifier = 0.0f;
        }
    }

    private void CalculateMoveVec()
    {
        //If we are on a slope, we need our velocity to be parallel to the slope. We find this through 
        //a cross product of the normal of that slope, and our right and left vectors.
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
        //If we are flying and MoveWhileJumping is active, we need to change just the X and Z components of our velocity.
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
            //If we are in the air, act like the ground is flat
            GroundAngle = 90.0f;
        }
        else
        {
            //Else, find angle of ground from HitInfo calculated by IsGrounded();
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
        CalculateMoveVec();
        //If the player has pressed an UP key and the player is currently standing on the ground
        if (Up && IsGrounded())
        {
            //Jump
            if (DidAJump) OnEndJump();
            if (Up)
            {
                if ((GroundAngle - 90 ) > 0 && (GroundAngle < MaxGroundAngle)) MoveVec.y = JumpForce + ((GroundAngle - 90)/100.0f);
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
        if (!JumpWhileHeld) Up = false;
    }

    //This function gets called whenever a player lands after falling or jumping.
    private void OnEndJump()
    {
        DidAJump = false;
        //If we aren't continuously jumping, we need to reset the Up boolean so that the player
        //only jumps once per press of an UP key.
        if(!JumpWhileHeld) Up = false;
    }

    private void MoveCharacter(float DeltaTime)
    {

        //If our LEFT button is held, and we are supposed to be moving left, and if move while jumping is on or we are grounded, Accelerate
        if ((Left && !MoveDir) && !(!MoveWhileJumping && !IsGrounded()))
        {
            SpeedModifier += Acceleration * DeltaTime;
            if (SpeedModifier >= 1.0f) SpeedModifier = 1.0f;
        }
        //If our Right button is held, and we are supposed to be moving Right, and if move while jumping is on or we are grounded, Accelerate
        else if ((Right && MoveDir) && !(!MoveWhileJumping && !IsGrounded()))
        {
            SpeedModifier += Acceleration * DeltaTime;
            if (SpeedModifier >= 1.0f) SpeedModifier = 1.0f;
        }
        //In all other cases we want to decelerate, but by how much is dependent on if we are grounded or not.
        else
        {
            //If we are grounded, decelerate quickly
            if (IsGrounded())
            {
                SpeedModifier -= Acceleration * DeltaTime;
                if (SpeedModifier <= 0.0f) SpeedModifier = 0.0f;
            }
            //If we aren't decelerate more quickly
            else
            {
                SpeedModifier -= (Acceleration/10.0f) * DeltaTime;
                if (SpeedModifier <= 0.0f) SpeedModifier = 0.0f;
            }
        }

        //Set velocity of player
        Vector3 FinalVel;
        //If we are grounded and we didn't just jump move along slope of surface we are on.
        if (IsGrounded() && !DidAJump)
        {
            FinalVel = new Vector3(MoveVec.x * MoveSpeed * SpeedModifier, MoveVec.y * MoveSpeed * SpeedModifier, MoveVec.z * MoveSpeed * SpeedModifier);
            //If the slope is too high, don't move
            if ((GroundAngle > MaxGroundAngle)) FinalVel = Vector3.zero;

        }
        //Otherwise move, but with a y component of velocity that is determined by jumping/falling, and not the slope of the surface we are on.
        else
        {
            FinalVel = new Vector3(MoveVec.x * MoveSpeed * SpeedModifier, MoveVec.y, MoveVec.z * MoveSpeed * SpeedModifier);
        }
        RBody.velocity = FinalVel;
    }

    private void ManageAnimations()
    {
        if((Left || Right) && IsGrounded())
        {
            CharacterAnimator.Play("Running (1)");
        }
        else
        {
            CharacterAnimator.Play("Idle");
        }
    }

}
