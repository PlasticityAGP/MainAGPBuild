using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;

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
    private bool Down = false; // Uncommented by Matt for testing
    private bool Left = false;
    private bool Right = false;

    [Title("Character Model")]
    [SerializeField]
    [Tooltip("Pass in a reference to the model for this character")]
    [ValidateInput("IsNull", "There must be a reference to the character model")]
    private GameObject RefToModel;
    [SerializeField]
    [Tooltip("Pass in a reference to the Game Object representing the top bound of the ledging window")]
    [ValidateInput("IsNull", "There must be a reference to the Game Object representing the top bound of the ledging window")]
    private GameObject LedgeTopBound;
    [SerializeField]
    [Tooltip("Pass in a reference to the Game Object representing the bottom bound of the ledging window")]
    [ValidateInput("IsNull", "There must be a reference to the Game Object representing the bottom bound of the ledging window")]
    private GameObject LedgeBottomBound;
    [Title("Animations")]
    [InfoBox("Each of the type of animations stores an array of Animation State names that are associated with the Character Model child Game Object. These categories of animation will randomly choose" +
        " one of the level states in its array to play. This adds a little randomness/lifelike feeling to the way our character moves")]
    [SerializeField]
    [Tooltip("An array of string names for running animations for the character")]
    [ValidateInput("NotEmpty", "There must be at least one idle animation specified")]
    private string[] IdleAnimations;
    [SerializeField]
    [Tooltip("An array of string names for idle animations for the character")]
    [ValidateInput("NotEmpty", "There must be at least one run animation specified")]
    private string[] RunAnimations;
    [SerializeField]
    [Tooltip("An array of string names for jumping animations for the character")]
    [ValidateInput("NotEmpty", "There must be at least one jump animation specified")]
    private string[] JumpAnimations;
    [Title("Movement")]
    [Tooltip("Determines the maximum speed our character can move.")]
    [ValidateInput("LessThanZero", "We cannot have a max move speed <= 0.0")]
    public float MoveSpeed;
    [SerializeField]
    [Tooltip("Acceleration factor. This effects how quickly the player can start moving, stop moving, and change direction.")]
    [ValidateInput("LessThanZero", "We cannot have an acceleration value <= 0.0")]
    private float Acceleration;
    [SerializeField]
    [Tooltip("Determines if you want the player to be able to ajust their velocity mid air")]
    private bool MoveWhileJumping;
    [SerializeField]
    [Tooltip("If true, allows the player to continuously jump while UP is held down")]
    private bool JumpWhileHeld;
    [SerializeField]
    [Tooltip("How much force you want the player to jump with.")]
    [ValidateInput("LessThanZero", "We cannot have a jump force <= 0.0")]
    private float JumpForce;
    [SerializeField]
    [Tooltip("Impact of gravity as the player rises to zenith of jump.")]
    [ValidateInput("LessThanZero", "We cannot have an up gravity component <= 0.0")]
    private float UpGravityOnPlayer;
    [SerializeField]
    [Tooltip("Impact of gravity as the player falls from the zenith of their jump")]
    [ValidateInput("LessThanZero", "We cannot have a down gravity component <= 0.0")]
    private float DownGravityOnPlayer;
    [SerializeField]
    [Tooltip("The distance away from a wall the player is allowed to be before snapping to the wall via ledging")]
    [ValidateInput("LessThanZero", "We cannot have a ledging distance <= 0.0f")]
    private float LedgingAllowedDistance;
    [SerializeField]
    [Tooltip("The left hand animation curves that dictate the speed that IK will lerp the left hand")]
    private AnimationCurve[] LeftHandLedgingCurves;
    [SerializeField]
    [Tooltip("The right hand animation curves that dictate the speed that IK will lerp the right hand")]
    private AnimationCurve[] RightHandLedgingCurves;

    [Title("Environment")]
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
    [Tooltip("Layermask that signifies what objects are considered to be walls.")]
    private LayerMask WallLayer;

    //MoveDir is a boolean that signifies what direction the player is moving in, Right(true) or Left(false).
    [HideInInspector]
    public bool MoveDir = true;
    [HideInInspector]
    public bool MovingInZ = false;
    [HideInInspector]
    public bool CanJump = true;
    [HideInInspector]
    public bool AmClambering = false;

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
    private RaycastHit LedgingWall;
    //CharacterAnimator will store a reference to the Animator of our Character.
    private Animator CharacterAnimator;
    private SCR_AnimEventManager AnimManager;
    private SCR_IKToolset IkTools;

    //Reference to the character's rigidbody
    [HideInInspector]
    public Rigidbody RBody;
    //Boolean used in Jump() to determine when to call OnBeginJump() and OnEndJump()

    private bool DidAJump = false;
    private bool VelocityAllowed = true;
    [HideInInspector]
    public bool IsClimbing = false;
    private bool JumpingOff = false;
    private bool FallingOff = false;
    private bool CurrentlyLedging = false;
    public float ClimbSpeed = 3.0f;
    private float HighClimb;
    private float LowClimb;
    [HideInInspector]
    public GameObject Ladder;
    private bool DoLedgeLerp = false;
    private float LedgeYTarget;
    private float LedgeXTarget;
    private bool InAnimationOverride = false;

    private bool NotEmpty(string[] array)
    {
        if (array.Length == 0) return false;
        else return true;
    }
    private bool LessThanZero(float input)
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
        if (value == 1)
        {
            Up = true;
            if (CurrentlyLedging)
            {
                LedgeMount();
            }
        }
        else Up = false;
    }
    private void DownPressed(int value) // Uncommented by Matt For Testing
    {
        //The value passed by the event indicates whether or not the key is pressed down.
        if (value == 1)
        {
            Down = true;
            if (CurrentlyLedging)
            {
                LedgeDismount(false);
            }
        }
        else
            Down = false;
    }

    private bool CanSendRunAnimations()
    {
        return IsGrounded() && !IsClimbing && !InAnimationOverride;
    }

    private void LeftPressed(int value)
    {
        //The value passed by the event indicates whether or not the key is pressed down.
        if (value == 1)
        {
            Left = true;
            //If we are currently running right when we recieve this left keypress, turn the character.
            if (MoveDir && !CurrentlyLedging)
                TurnCharacter();
            if (CanSendRunAnimations())
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
            if (!MoveDir && !CurrentlyLedging)
                TurnCharacter();
            if (CanSendRunAnimations())
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

        if (gameObject.GetComponent<SCR_IKToolset>()) IkTools = gameObject.GetComponent<SCR_IKToolset>();
        else Debug.LogError("We need a SCR_IKToolset script attached to one of the Character's child Game Objects");

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
        if (!InAnimationOverride)
        {
            //Do movement calculations. Needs to be in FixedUpdate and not Update because we are messing with physics.
            CalculateGroundAngle();
            CalculateMoveVec();
            if (!IsClimbing) Jump(Time.deltaTime); // Changed by Matt for testing from "Jump(Time.deltaTime);"
            if (DidAJump && !CurrentlyLedging && !MovingInZ) CheckForLedges();
            if (DoLedgeLerp) LedgeLerp(Time.deltaTime);
            MoveCharacter(Time.deltaTime);
            PerTickAnimations();
        }
    }

    private void Climb()
    {
        if (Up)
        {
            if (this.transform.position.y > HighClimb)
                MoveVec.y = 0;
            else
                MoveVec.y = ClimbSpeed;
            // Play animation
        }
        else if (Down)
        {
            if (this.transform.position.y < LowClimb)
                MoveVec.y = 0;
            else
                MoveVec.y = ClimbSpeed * -1;
            // Play animation
        }
        else
            MoveVec.y = 0;

        if(MoveVec.y == 0)
        {
            if (Ladder.transform.position.x - gameObject.transform.position.x < 0.0f)
            {
                if(this.transform.position.y > HighClimb)
                    Clamber(0); //Clamber to the left
                else if(this.transform.position.y < LowClimb)
                {
                    IsClimbing = false;
                }
            }
            else
            {
                if (this.transform.position.y > HighClimb)
                    Clamber(1); //Clamber to the right
                else if (this.transform.position.y < LowClimb)
                {
                    IsClimbing = false;
                }
            }
        }
    }

    // Empty Function called in climb
    private void Clamber(int direction)
    {
        if (!AmClambering)
        {
            Ladder.GetComponent<SCR_Ladder>().Clamber(direction);
        }


    }

    /// <summary>
    /// This is a temporary summary TODO: MATT
    /// </summary>
    public void JumpOff()
    {
        if (Up)
        {
            MoveVec.y = JumpForce;
            if (Left && !Right)
            {
                MoveVec.x = JumpForce / 2 * -1;
                //Debug.Log("Jumped off Leftwards");
            }
            else if (!Left && Right) MoveVec.x = JumpForce / 2;
            //else Debug.Log("freaky stuff");
            JumpingOff = true;
            Jump(Time.deltaTime);
        }
    }

    /// <summary>
    /// This is a temporary summary TODO: MATT
    /// </summary>
    /// <param name="high"></param>
    /// <param name="low"></param>
    public void OnClimbable(float high, float low)
    {
        HighClimb = high- 1.5f;
        LowClimb = low;
        IsClimbing = true;
        MoveVec.x = 0;
        // TODO: have to change the player's x velocity accordingly.
    }

    public void SetInAnimationOverride(bool InOverride)
    {
        InAnimationOverride = InOverride;
    }

    /// <summary>
    /// Draws traces down from the character to determine if they are resting on the ground or not. Returns true if on the ground, returns false 
    /// in the air
    /// </summary>
    /// <returns>Whether the player is on the ground or not</returns>
    public bool IsGrounded()
    {
        bool isGroundedResult = false;
        //Create two locations to trace from so that we can have a little bit of 'dangle' as to whether
        //or not the character is on an object.
        Vector3 YOffset = new Vector3(0.0f, YTraceOffset, 0.0f);
        Vector3 RightPosition = transform.position + (InitialDir.normalized * 0.15f) + YOffset;
        Vector3 LeftPosition = transform.position + (InitialDir.normalized * -0.15f) + YOffset;
        RaycastHit Result;
        float raycastExtension = 0.3f;
        //Raycast to find slope of ground beneath us. Needs to extend lower than our raycast that decides if grounded 
        //because we want our velocity to match the slope of the surface slightly before we return true.
        //This prevents a weird bouncing effect that can happen after a player lands. 
        if (Physics.Raycast(RightPosition, Vector3.down, out Result, GroundTraceDistance + raycastExtension, GroundLayer))
        {
            if (Result.distance <= GroundTraceDistance)
            {
                isGroundedResult = true;
            }
            if (MoveDir)
            {
                HitInfo = Result;
            }
        }
        if (Physics.Raycast(LeftPosition, Vector3.down, out Result, GroundTraceDistance + raycastExtension, GroundLayer))
        {
            if (Result.distance <= GroundTraceDistance)
            {
                isGroundedResult = true;
            }
            if (!MoveDir)
            {
                HitInfo = Result;
            }
        }
        return isGroundedResult;
    }
    
    private void TurnCharacter()
    {
        if (!InAnimationOverride)
        {
            //If we turn, we flip the boolean the signifies player direction
            MoveDir = !MoveDir;
            if (MoveDir) SCR_EventManager.TriggerEvent("CharacterTurn", 1);
            else SCR_EventManager.TriggerEvent("CharacterTurn", 0);
            Vector3 TurnAround = new Vector3(0.0f, 180.0f, 0.0f);
            RefToModel.transform.Rotate(TurnAround);
            if (IsGrounded())
            {
                SpeedModifier = 0.0f;
            }
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

    /// <summary>
    /// Sets the character's velocity to Vector3.Zero, and prevents the CharacterManager from updating velocity unitl UnfreezeVelocity() is called
    /// </summary>
    public void FreezeVelocity()
    {
        VelocityAllowed = false;
    }

    /// <summary>
    /// Allows the CharacterManager to begin updating velocity again.
    /// </summary>
    public void UnfreezeVelocity()
    {
        VelocityAllowed = true;
    }

    private void Jump(float DeltaTime)
    {
        if (IsClimbing || JumpingOff)
        {
            MoveVec.y = JumpForce;
            //Tell anim manager to play a jump animation.
            AnimManager.NewAnimEvent(JumpAnimations[Random.Range(0, JumpAnimations.Length - 1)], 0.15f, 0.0f);

            OnBeginJump();
            IsClimbing = false;
            return;
        }
        else
        {
            CalculateMoveVec();
        }


        //If the player has pressed an UP key and the player is currently standing on the ground
        if (Up && IsGrounded())
        {
            if (CanJump)
            {
                FallingOff = false;
                if (DidAJump) OnEndJump();
                //Need a different jump force if we are moving uphill while jumping.
                if ((GroundAngle - 90) > 0 && (GroundAngle < MaxGroundAngle)) MoveVec.y = JumpForce + ((GroundAngle - 90) / 100.0f);
                else MoveVec.y = JumpForce;
                //Tell anim manager to play a jump animation.
                AnimManager.NewAnimEvent(JumpAnimations[Random.Range(0, JumpAnimations.Length - 1)], 0.15f, 0.0f);
                OnBeginJump();
            }

        }
        //If UP has not been pressed and the player is currently on the ground, the y component of their velocity should be zero
        else if (!Up && IsGrounded())
        {
            FallingOff = false;
            if (DidAJump) OnEndJump();
        }
        //In all other cases the player is falling. Here we calculate the y component of velocity while falling.
        else
        {
            //Falling
            if (!DidAJump) OnBeginJump();

            //Different strengths of gravity depending on if player is rising or falling. This can help prevent floaty feeling of jumps
            if (MoveVec.y > 0.0f) MoveVec.y -= UpGravityOnPlayer * DeltaTime;
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
        CurrentlyLedging = false;
        //If we aren't continuously jumping, we need to reset the Up boolean so that the player
        //only jumps once per press of an UP key.
        if (!JumpWhileHeld) Up = false;

        AnimManager.NewAnimEvent("SoftLanding", 0.15f, 0.0f);
        if(Left||Right) AnimManager.NewAnimEvent(RunAnimations[Random.Range(0, RunAnimations.Length - 1)], 0.15f, 0.05f);
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
                SpeedModifier -= (Acceleration / 20.0f) * DeltaTime;
                if (SpeedModifier <= 0.0f) SpeedModifier = 0.0f;
            }
        }

        //Set velocity of player
        Vector3 FinalVel;

        if (FallingOff)
        {
            if (MoveSpeed == 0) MoveSpeed = 1;
            if (SpeedModifier == 0) SpeedModifier = 1;
            MoveVec.x = RBody.velocity.x / MoveSpeed / SpeedModifier;
        }

        if (IsClimbing) // Added by Matt for testing
        {
            Climb();
            FinalVel = new Vector3(0, MoveVec.y, 0);
        }
        else if (JumpingOff)
        {
            FinalVel = new Vector3(MoveVec.x, MoveVec.y, 0);
            JumpingOff = false;
            FallingOff = true;
        }
        //If we are grounded and we didn't just jump move along slope of surface we are on.
        else if (IsGrounded() && !DidAJump) // Changed by Matt for Testing from "if" to "else if"
        {
            FinalVel = new Vector3(MoveVec.x * MoveSpeed * SpeedModifier, MoveVec.y * MoveSpeed * SpeedModifier, MoveVec.z * MoveSpeed * SpeedModifier);
            //If the slope is too high, don't move
            //if ((GroundAngle > MaxGroundAngle)) FinalVel = Vector3.zero;
            if(gameObject.transform.position.y - HitInfo.point.y < (GroundTraceDistance - YTraceOffset) * 0.5)
            {
                float difference = (GroundTraceDistance - YTraceOffset) - (gameObject.transform.position.y - HitInfo.point.y);
                gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + (difference * 0.95f), gameObject.transform.position.z);
            }

        }
        //Otherwise move, but with a y component of velocity that is determined by jumping/falling, and not the slope of the surface we are on.
        else
        {
            FinalVel = new Vector3(MoveVec.x * MoveSpeed * SpeedModifier, MoveVec.y, MoveVec.z * MoveSpeed * SpeedModifier);
        }
        if (VelocityAllowed) RBody.velocity = FinalVel;
        else RBody.velocity = Vector3.zero;
    }

    //This function makes calls to the AnimManager for animations that need to be updated or calculated per update cycle
    private void PerTickAnimations()
    {
        //This lock allows crossfadeing of animations to only be called once per transition
        if (!AnimManager.AnimLock)
        {
            if (IsClimbing)
            {
                AnimManager.NewAnimEvent(IdleAnimations[Random.Range(0, IdleAnimations.Length - 1)], 0.15f, 0.0f);
                AnimManager.AnimLock = true;
            }
            //If the player is on the ground but not moving, play the idle animation.
            else if (IsGrounded() && !(Left || Right) && !DidAJump)
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

    //This function fires two raycasts out from the character to try and hit a wall. If the bottom hits and the top misses, it starts
    //ledging
    private void CheckForLedges()
    {
        Vector3 RayCastDir;
        if (MoveDir) RayCastDir = gameObject.transform.forward;
        else RayCastDir = -1.0f * gameObject.transform.forward;
        RaycastHit TopResult;
        RaycastHit BottomResult;
        bool TopHit;
        bool BottomHit;
        //Fire the bottom raycast and see if it registered a hit
        if (Physics.Raycast(LedgeBottomBound.transform.position, RayCastDir, out BottomResult, LedgingAllowedDistance, WallLayer))
            BottomHit = true;
        else BottomHit = false;
        //Fire the top raycast and see if it registered a hit
        if (Physics.Raycast(LedgeTopBound.transform.position, RayCastDir, out TopResult, LedgingAllowedDistance, WallLayer))
            TopHit = true;
        else TopHit = false;
        //If the bottom raycast hit and the top didn't, call the StartLedging function and pass it the hitinfo of the wall it hit
        if (BottomHit && !TopHit) StartLedging(BottomResult);
    }

    private void StartLedging(RaycastHit other)
    {
        LedgingWall = other;
        //Set currently ledging so that our player is not effected by gravity in the MoveCharacter function
        CurrentlyLedging = true;
        float XValue;
        float ZValueLeft;
        float ZValueRight;
        //Depending on what direction we are looking, find two locations based on the width of the wall we are ledging on
        //for the left hand and right hand effector to be set to.
        if (MoveDir)
        {
            XValue = other.transform.position.x - (other.transform.lossyScale.x / 2.0f);
            ZValueLeft = gameObject.transform.position.z + 0.3f;
            ZValueRight = gameObject.transform.position.z - 0.3f;
        }
        else
        {
            XValue = other.transform.position.x + (other.transform.lossyScale.x / 2.0f);
            ZValueLeft = gameObject.transform.position.z - 0.3f;
            ZValueRight = gameObject.transform.position.z + 0.3f;
        }
        //Find the y value of the effector locations based on the height of the wall
        float YValue = other.transform.position.y + (other.transform.lossyScale.y / 2.0f);
        Vector3 LeftHandPoint = new Vector3(XValue, YValue, ZValueLeft);
        Vector3 RightHandPoint = new Vector3(XValue, YValue, ZValueRight);
        //Set effectors to new locations and begin lerping them to create that hanging visual
        IkTools.SetEffectorLocation("LeftHand", LeftHandPoint);
        IkTools.SetEffectorLocation("RightHand", RightHandPoint);
        IkTools.StartEffectorLerp("LeftHand", LeftHandLedgingCurves[0], 0.5f);
        IkTools.StartEffectorLerp("RightHand", RightHandLedgingCurves[0], 0.5f);
        FreezeVelocity();
    }

    //Called when we want to drop down from the ledge
    private void LedgeDismount(bool AtTop)
    {
        //Set velocity to zero and unfreeze it
        MoveVec = Vector3.zero;
        UnfreezeVelocity();
        //If our effectors have weight, need to lerp them from their current weights back down to zero
        if(!AtTop)
        {
            IkTools.StartEffectorLerp("LeftHand", LeftHandLedgingCurves[1], 0.5f);
            IkTools.StartEffectorLerp("RightHand", RightHandLedgingCurves[1], 0.5f);
        }

    }

    //Called when we want to clamber up on top of the ledge.
    private void LedgeMount()
    {
        //Define Y and X checkpoints we need to reach when we move our character to climb up on top of the wall
        LedgeYTarget = LedgingWall.transform.position.y + (LedgingWall.transform.lossyScale.y / 2.0f);
        if (transform.position.x - LedgingWall.transform.position.x > 0.0f)
            LedgeXTarget = LedgingWall.transform.position.x + (LedgingWall.transform.lossyScale.x / 2.0f);
        else
            LedgeXTarget = LedgingWall.transform.position.x - (LedgingWall.transform.lossyScale.x / 2.0f);
        //Slowly lerp effectors back to zero weight
        IkTools.StartEffectorLerp("LeftHand", LeftHandLedgingCurves[2], 1.0f);
        IkTools.StartEffectorLerp("RightHand", RightHandLedgingCurves[2], 1.0f);
        DoLedgeLerp = true;

    }

    //Execute a lerp with values defined in LedgeMount
    private void LedgeLerp(float DeltaTime)
    {
        Vector3 temp;
        //If we aren't as high as the ledge yet, keep lerping up in Y
        if (LedgeYTarget - transform.position.y > 0.0f)
        {
            temp = new Vector3(transform.position.x, transform.position.y + (DeltaTime * 2.0f), transform.position.z);
            transform.position = temp;
        }
        //If we have finished lerping up in Y, lerp in X according to the current move direction of the character.
        else if (MoveDir && LedgeXTarget - transform.position.x > 0.0f)
        {
            temp = new Vector3(transform.position.x + (DeltaTime * 2.0f), transform.position.y, transform.position.z);
            transform.position = temp;
        }
        else if (!MoveDir && LedgeXTarget - transform.position.x < 0.0f)
        {
            temp = new Vector3(transform.position.x - (DeltaTime * 2.0f), transform.position.y, transform.position.z);
            transform.position = temp;
        }
        else
        {
            //Dismount from ledge once we have make it on top
            DoLedgeLerp = false;
            LedgeDismount(true);
        }
    }

}
