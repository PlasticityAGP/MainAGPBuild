using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SCR_CharacterManager : MonoBehaviour
{

    //Event listeners per action for receiving events fired by the Input Manager
    private UnityAction<int> UpListener;
    private UnityAction<int> DownListener;
    private UnityAction<int> LeftListener;
    private UnityAction<int> RightListener;
    private UnityAction<int> InteractListener;

    //Booleans that will signify what input is being held down. For example, Up is true whenever an Up
    //Key is held down, and so on
    private bool Up = false;
    //private bool Down = false;
    private bool Left = false;
    private bool Right = false;

    [SerializeField]
    [Tooltip("Pass in a reference to the model for this character")]
    private GameObject RefToModel;
    [SerializeField]
    [Tooltip("An array of string names for running animations for the character")]
    private string[] IdleAnimations;
    [SerializeField]
    [Tooltip("An array of string names for idle animations for the character")]
    private string[] RunAnimations;
    [SerializeField]
    [Tooltip("An array of string names for jumping animations for the character")]
    private string[] JumpAnimations;
    [Tooltip("Determines the maximum speed our character can move.")]
    public float MoveSpeed = 6;
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
    [Tooltip("Impact of gravity as the player rises to zenith of jump.")]
    private float UpGravityOnPlayer = 9;
    [SerializeField]
    [Tooltip("Impact of gravity as the player falls from the zenith of their jump")]
    private float DownGravityOnPlayer = 14;
    [SerializeField]
    [Tooltip("Trace distance for determining if the player has landed.")]
    private float GroundTraceDistance = 1.05f;
    [SerializeField]
    [Tooltip("The maximum angle between player and ground that is walkable by player")]
    private float MaxGroundAngle = 120;
    [SerializeField]
    [Tooltip("Layermask that signifies what objects are considered to be the ground.")]
    private LayerMask GroundLayer;
    [SerializeField]
    [Tooltip("Reference to the LevelData asset for the game")]
    private SCR_LevelStates LevelData;

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
    SCR_AnimEventManager AnimManager;

    //Reference to the character's rigidbody
    public Rigidbody RBody;
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
    }

    private void OnEnable()
    {
        //Register listeners with their events in the EventManager
        SCR_EventManager.StartListening("UpKey", UpListener);
        SCR_EventManager.StartListening("DownKey", DownListener);
        SCR_EventManager.StartListening("LeftKey", LeftListener);
        SCR_EventManager.StartListening("RightKey", RightListener);
    }

    private void OnDisable()
    {
        //Tell the EventManager we are no longer listening as the CharacterManager gets disabled.
        SCR_EventManager.StopListening("UpKey", UpListener);
        SCR_EventManager.StopListening("DownKey", DownListener);
        SCR_EventManager.StopListening("LeftKey", LeftListener);
        SCR_EventManager.StopListening("RightKey", RightListener);
    }

    //The following 5 functions are callbacks that get called by the event listeners
    private void UpPressed(int value)
    {
        //The value passed by the event indicates whether or not the key is pressed down. 
        if (value == 1) Up = true;
        else Up = false;
    }
    private void DownPressed(int value)
    {
        //The value passed by the event indicates whether or not the key is pressed down.
        //if (value == 1)
        //    Down = true;
        //else
        //    Down = false;

        //Allows us to print name of current level for testing
        //Debug.Log(LevelData.CurrentLevel);
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
            if (IsGrounded())
            {
                //Tell the animation manager to play a running animation is left is pressed and player is on ground.
                AnimManager.NewAnimEvent(RunAnimations[Random.Range(0, RunAnimations.Length - 1)], 0.15f, 0.0f);
            }

        }
        else Left = false;
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
            if (IsGrounded())
            {
                //Tell the animation manager to play a running animation is left is pressed and player is on ground.
                AnimManager.NewAnimEvent(RunAnimations[Random.Range(0, RunAnimations.Length - 1)], 0.15f, 0.0f);
            }
        }
        else Right = false;
    }


    // Use this for initialization
    void Start()
    {

        if (RefToModel == null) Debug.LogError("You need to pass in a reference to the model you wish the character manager to use");

        //Make sure the current character has a Rigidbody component
        if (GetComponent<Rigidbody>()) RBody = GetComponent<Rigidbody>();
        else Debug.LogError("There is currently not a rigidbody attached to this character");

        //Make sure the character has an animation manager
        if (gameObject.GetComponent<SCR_AnimEventManager>()) AnimManager = gameObject.GetComponent<SCR_AnimEventManager>();
        else Debug.LogError("There is currently not an AnimEventManager attached to the Character GameObject");

        //Make sure the model has an animator component
        if (RefToModel.GetComponent<Animator>()) CharacterAnimator = RefToModel.GetComponent<Animator>();
        else Debug.LogError("There is currently not an animator attached to this character's model");
        //Pass the instance of the CharacterAnimator obtained in the CharacterManager to the AnimManager.
        AnimManager.CharacterAnimator = CharacterAnimator;

        //Make sure we have at least one of each type of animation specified.
        if (RunAnimations.Length == 0) Debug.LogError("You need at least one run animation specified in the CharacterManager");
        if (IdleAnimations.Length == 0) Debug.LogError("You need at least one idle animation specified in the CharacterManager");
        if (JumpAnimations.Length == 0) Debug.LogError("You need at least one jump animation specified in the CharacterManager");


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
        CalculateGroundAngle();
        CalculateMoveVec();
        Jump(Time.deltaTime);
        MoveCharacter(Time.deltaTime);
        PerTickAnimations();
    }

    [HideInInspector]
    public bool IsGrounded()
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
            if (DidAJump) OnEndJump();
            //Need a different jump force if we are moving uphill while jumping.
            if ((GroundAngle - 90) > 0 && (GroundAngle < MaxGroundAngle)) MoveVec.y = JumpForce + ((GroundAngle - 90) / 100.0f);
            else MoveVec.y = JumpForce;
            //Tell anim manager to play a jump animation.
            AnimManager.NewAnimEvent(JumpAnimations[Random.Range(0, JumpAnimations.Length - 1)], 0.15f, 0.0f);

            OnBeginJump();
        }
        //If UP has not been pressed and the player is currently on the ground, the y component of their velocity should be zero
        else if (!Up && IsGrounded())
        {
            if (DidAJump) OnEndJump();
        }
        //In all other cases the player is falling. Here we calculate the y component of velocity while falling.
        else
        {
            //Falling
            if (!DidAJump) OnBeginJump();

            //Different strengths of gravity depending on if player is rising or falling. This can help prevent floaty feeling of jumps
            if(MoveVec.y > 0.0f) MoveVec.y -= UpGravityOnPlayer * DeltaTime;
            else MoveVec.y -= DownGravityOnPlayer * DeltaTime;



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
        if (!JumpWhileHeld) Up = false;

        AnimManager.NewAnimEvent("SoftLanding", 0.15f, 0.0f);
        if(Left||Right) AnimManager.NewAnimEvent(RunAnimations[Random.Range(0, RunAnimations.Length - 1)], 0.15f, 0.15f);
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
                SpeedModifier -= (Acceleration / 10.0f) * DeltaTime;
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

    //This function makes calls to the AnimManager for animations that need to be updated or calculated per update cycle
    private void PerTickAnimations()
    {
        //This lock allows crossfadeing of animations to only be called once per transition
        if (!AnimManager.AnimLock)
        {
            //If the player is on the ground but not moving, play the idle animation.
            if (IsGrounded() && !(Left || Right) && !DidAJump)
            {
                AnimManager.NewAnimEvent(IdleAnimations[Random.Range(0, IdleAnimations.Length - 1)], 0.15f, 0.0f);
                AnimManager.AnimLock = true;
            }
            //If the player is falling, play the falling animation
            else if (!IsGrounded() && MoveVec.y < 0.0f)
            {
                AnimManager.NewAnimEvent("Falling", 0.45f, 0.0f);
                AnimManager.AnimLock = true;
            }
        }
    }
}
