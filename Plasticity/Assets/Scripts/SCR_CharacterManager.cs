using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;

public class SCR_CharacterManager : SCR_GameplayStatics
{
    [HideInInspector]
    public enum CharacterStates { Running, Falling, Jumping, Swimming, Idling, SwimIdling, Lying, Pushing, Pulling, ClimbingUp, ClimbingDown, Paused}
    private CharacterStates PlayerState;

    //Event listeners per action for receiving events fired by the Input Manager
    private UnityAction<int> UpListener;
    private UnityAction<int> DownListener;
    private UnityAction<int> LeftListener;
    private UnityAction<int> RightListener;

    //Booleans that will signify what input is being held down. For example, Up is true whenever an Up
    //Key is held down, and so on
    private bool Up = false;
    private bool Down = false; // Uncommented by Matt for testing
    private bool Left = false;
    private bool Right = false;
    [HideInInspector]
    public bool PlayerGrounded;

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

    [Title("Movement")]
    [SerializeField]
    [Tooltip("Determines the maximum speed our character can move.")]
    [ValidateInput("LessThanZero", "We cannot have a max move speed <= 0.0")]
    private float MaxMoveSpeed;
    [SerializeField]
    private float MaxFallVelocity;
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
    private float HardFallDistance;
    [SerializeField]
    private float LengthOfHardFallAnim;
    [SerializeField]
    private float LengthOfSlowdown;
    [SerializeField]
    private float PostHardFallSpeed;
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
    private float MoveSpeedPercentage = 1.0f;
    private bool LastKeypress = true;
    private bool NoAnimUpdate = false;
    private bool IsTurnRestricted = false;
    private int MoveDirAtRestricted = 0;
    private bool PushingAllowed;
    [HideInInspector]
    public bool MovingInZ = false;
    [HideInInspector]
    public bool CanJump = true;
    [HideInInspector]
    public bool AmClambering = false;
    [HideInInspector]
    public GameObject InteractingWith;
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
    private SCR_IKToolset IkTools;

    //Reference to the character's rigidbody
    [HideInInspector]
    public Rigidbody RBody;

    //Boolean used in Jump() to determine when to call OnBeginJump() and OnEndJump()
    private bool DidAJump = false;
    //Boolean used to shut off velocity calculations 
    private bool VelocityAllowed = true;
    //Dictates whether the Character manager should behave like the player is climbing
    [HideInInspector]
    public bool IsClimbing = false;
    private bool JumpingOff = false;
    private bool FallingOff = false;
    //Defines if the player is in the process of hanging from a ledge
    private bool CurrentlyLedging = false;
    public float ClimbSpeed = 3.0f;
    [SerializeField]
    private GameObject BackTarget;
    //Defines how high or low a player can climb up a ladder relative to the ladder's scale
    private float HighClimb;
    private float LowClimb;
    [HideInInspector]
    public GameObject Ladder;
    private bool DoLedgeLerp = false;
    private float LedgeYTarget;
    private float LedgeXTarget;

	[SerializeField]
	[Tooltip("How quickly the player decelerates while swimming")]
	private float SwimSlowdown = 4f;
    private Vector3 swimspeed;
    public float maxSwimSpeed;
    //public float swimAcceleration;
    public float maxTimeUnderWater = 2;
    public float timeUnderWater;
    public float waterHeight;
    private bool InAnimationOverride = false;
    [HideInInspector]
    public bool StateChangeLocked = false;
    private string LastAnim;
    private float FallStartHeight;
    private bool IsLockedAnims;
    private bool TurnOverride = false;
    private bool AmInHardFall = false;
    private bool ForcedAnim = false;

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

    private void ChangePlayerState(CharacterStates state)
    {
        if (!IsLockedAnims || ForcedAnim)
        {
            if (LastAnim != null) ResetAnim();
            if (PlayerState == CharacterStates.Paused)
            {
                CharacterAnimator.speed = 1.0f;
            }
            if (CharacterAnimator.GetCurrentAnimatorStateInfo(0).IsName("Running"))
            {
                if (state == CharacterStates.Idling) SetAnim("RunningToIdle");
                else if (state == CharacterStates.Falling) SetAnim("RunningToFalling");
                else if (state == CharacterStates.Jumping) SetAnim("RunningToJump");
                else if (state == CharacterStates.Lying) SetAnim("RunningToLying");
                else if (state == CharacterStates.Pushing) SetAnim("RunningToPush");
                else if (state == CharacterStates.Pulling) SetAnim("RunningToPull");
				else if (state == CharacterStates.SwimIdling) SetAnim("RunningToSwimIdle");

            }
            else if (CharacterAnimator.GetCurrentAnimatorStateInfo(0).IsName("Falling"))
            {
                if (state == CharacterStates.Idling) SetAnim("FallingToIdle");
                else if (state == CharacterStates.Running) SetAnim("FallingToRunning");
                else if (state == CharacterStates.Lying) SetAnim("FallingToLying");
                else if (state == CharacterStates.ClimbingUp) SetAnim("FallingToClimbingUp");
                else if (state == CharacterStates.ClimbingDown) SetAnim("FallingToClimbingDown");
				else if (state == CharacterStates.SwimIdling) SetAnim("FallingToSwimIdle");
			}
            else if (CharacterAnimator.GetCurrentAnimatorStateInfo(0).IsName("Jump"))
            {
                if (state == CharacterStates.Falling) SetAnim("JumpToFalling");
                else if (state == CharacterStates.Idling) SetAnim("JumpToIdle");
                else if (state == CharacterStates.Running) SetAnim("JumpToRunning");
                else if (state == CharacterStates.ClimbingUp) SetAnim("JumpToClimbingUp");
                else if (state == CharacterStates.ClimbingDown) SetAnim("JumpToClimbingDown");
            }
            else if (CharacterAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
            {
                if (state == CharacterStates.Running) SetAnim("IdleToRunning");
                else if (state == CharacterStates.Falling) SetAnim("IdleToFalling");
                else if (state == CharacterStates.Jumping) SetAnim("IdleToJump");
                else if (state == CharacterStates.Lying) SetAnim("IdleToLying");
                else if (state == CharacterStates.Pushing) SetAnim("IdleToPush");
                else if (state == CharacterStates.Pulling) SetAnim("IdleToPull");
                else if (state == CharacterStates.ClimbingUp) SetAnim("IdleToClimbingUp");
                else if (state == CharacterStates.ClimbingDown) SetAnim("IdleToClimbingDown");
            }
            else if (CharacterAnimator.GetCurrentAnimatorStateInfo(0).IsName("GettingUp"))
            {
                if (state == CharacterStates.Running) SetAnim("LyingToRunning");
                else if (state == CharacterStates.Idling) SetAnim("LyingToIdle");
                else if (state == CharacterStates.Jumping) SetAnim("LyingToJumping");                
            }
            else if (CharacterAnimator.GetCurrentAnimatorStateInfo(0).IsName("Push"))
            {
                if (state == CharacterStates.Running) SetAnim("PushToRunning");
                else if (state == CharacterStates.Idling) SetAnim("PushToIdle");
                else if (state == CharacterStates.Pulling) SetAnim("PushToPull");
            }
            else if (CharacterAnimator.GetCurrentAnimatorStateInfo(0).IsName("Pull"))
            {
                if (state == CharacterStates.Running) SetAnim("PullToRunning");
                else if (state == CharacterStates.Idling) SetAnim("PullToIdle");
                else if (state == CharacterStates.Pushing) SetAnim("PullToPush");
            }
            else if (CharacterAnimator.GetCurrentAnimatorStateInfo(0).IsName("ClimbingUp"))
            {
                if (state == CharacterStates.Idling) SetAnim("ClimbingUpToIdle");
                else if (state == CharacterStates.Jumping) SetAnim("ClimbingUpToJump");
                else if (state == CharacterStates.Falling) SetAnim("ClimbingUpToFalling");
                else if (state == CharacterStates.ClimbingDown) SetAnim("ClimbingUpToClimbingDown");
            }
            else if (CharacterAnimator.GetCurrentAnimatorStateInfo(0).IsName("ClimbingDown"))
            {
                if (state == CharacterStates.Idling) SetAnim("ClimbingDownToIdle");
                else if (state == CharacterStates.Jumping) SetAnim("ClimbingDownToJump");
                else if (state == CharacterStates.Falling) SetAnim("ClimbingDownToFalling");
                else if (state == CharacterStates.ClimbingDown) SetAnim("ClimbingDownToClimbingUp");
            }
			else if (CharacterAnimator.GetCurrentAnimatorStateInfo(0).IsName("SwimIdle")) {
				if (state == CharacterStates.Swimming)
					SetAnim("SwimIdleToSwimMove");
			}
			else if (CharacterAnimator.GetCurrentAnimatorStateInfo(0).IsName("SwimMove")) {
				if (state == CharacterStates.SwimIdling)
					SetAnim("SwimMoveToSwimIdle");
			}
			if (state == CharacterStates.Paused) CharacterAnimator.speed = 0.0f;
            PlayerState = state;
            ForcedAnim = false;
        }
    }

    private void ForcePlayerState(CharacterStates state)
    {
        ForcedAnim = true;
        ChangePlayerState(state);
    }

    private void LockAnim(CharacterStates anim)
    {
        ChangePlayerState(anim);
        IsLockedAnims = true;
    }

    private void UnlockAnim()
    {
        IsLockedAnims = false;
    }

    private void SetAnim(string input)
    {
        LastAnim = input;
        CharacterAnimator.SetTrigger(input);
    }

    private void ResetAnim()
    {
        CharacterAnimator.ResetTrigger(LastAnim);
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
        return PlayerGrounded && !IsClimbing && !InAnimationOverride;
    }

    private void LeftPressed(int value)
    {
        
        //The value passed by the event indicates whether or not the key is pressed down.
        if (value == 1)
        {
            Left = true;
            LastKeypress = false;
            //If we are currently running right when we recieve this left keypress, turn the character.
            if (MoveDir && !CurrentlyLedging && PlayerGrounded)
                TurnCharacter();
        }
        else
        {
            Left = false;
        }

        
    }
    private void RightPressed(int value)
    {
        //The value passed by the event indicates whether or not the key is pressed down.
        if (value == 1)
        {
            Right = true;
            LastKeypress = true;
            //If we are currently running right when we recieve this left keypress, turn the character.
            if (!MoveDir && !CurrentlyLedging && PlayerGrounded)
                TurnCharacter();
        }
        else
        {
            Right = false;
        }
        
    }


    // Use this for initialization
    void Start()
    {

        if (RefToModel == null) Debug.LogError("You need to pass in a reference to the model you wish the character manager to use");

        //Make sure the current character has a Rigidbody component
        if (GetComponent<Rigidbody>()) RBody = GetComponent<Rigidbody>();
        else Debug.LogError("There is currently not a rigidbody attached to this character");

        if (gameObject.GetComponent<SCR_IKToolset>()) IkTools = gameObject.GetComponent<SCR_IKToolset>();
        else Debug.LogError("We need a SCR_IKToolset script attached to one of the Character's child Game Objects");

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
        if (PlayerState == CharacterStates.Swimming || PlayerState == CharacterStates.SwimIdling)
        {
            Swim();
			MoveCharacter(Time.deltaTime);
			return;
        }
        if (!InAnimationOverride)
        {
            //Do movement calculations. Needs to be in FixedUpdate and not Update because we are messing with physics.
            Grounded();
            CalculateGroundAngle();
            if (!JumpingOff) CalculateMoveVec();
            if (!IsClimbing) Jump(Time.deltaTime);
            if (DidAJump && !CurrentlyLedging && !MovingInZ) CheckForLedges();
            if (DoLedgeLerp) LedgeLerp(Time.deltaTime);
            MoveCharacter(Time.deltaTime);
            DeterminePlayerState();
        }
    }

    /// <summary>
    /// Allows any water bodies to declare the player as swimming, and share its surface height.
    /// </summary>
    /// <param name="inwater"></param> Is the player entering water?
    /// <param name="sealevel"></param> The surface height of the water body.
    public void IsInWater(bool inwater, float sealevel)
    {
		if (inwater) {
			PlayerState = CharacterStates.SwimIdling;
			ChangePlayerState(CharacterStates.SwimIdling);
		}
		else
			DeterminePlayerState();
		waterHeight = sealevel;
    }

    // If the player is touching water, this will determine if they are considered swimming
    // or if they are just walking in shallow water/jumping into water.
    /*private void InWater()
    {
        if (PlayerGrounded)
        {
            if (waterHeight > this.transform.position.y + this.transform.localScale.y * 1.25f)
                swimming = true;
            else
                swimming = false;
        }
        else if (DidAJump && RBody.velocity.y >= 0)
            swimming = false;
        else
        {
            if (waterHeight < this.transform.position.y)
                swimming = false;
            else
                swimming = true;
        }
    }*/

    // Controls for making the player swim
    // TODO: should we allow them to jump if they're treading water?
    private void Swim()
    {
		/*
        // Determining whether the player is at the water surface or not.
        underWater = false;
        if (waterHeight > this.transform.position.y + this.transform.localScale.y * 1.25f)
        {
            underWater = true; //Head is below water surface.
        }

        // Setting base MoveVec
        MoveVec = RBody.velocity;

        // Records how long the player has been underwater.
        if (!underWater)
            timeUnderWater = 0;
        else
            timeUnderWater += Time.deltaTime;

        // Calculates vertical swimming movement under normal conditions.
        maxSwimSpeed *= swimAcceleration;

        if (Up && !Down)
            MoveVec.y += maxSwimSpeed * Time.deltaTime;
        else if (Down && !Up)
            MoveVec.y -= maxSwimSpeed * Time.deltaTime;
        else if (MoveVec.y > 0.05f)
            MoveVec.y -= maxSwimSpeed * Time.deltaTime;
        else if (MoveVec.y < 0.05f)
            MoveVec.y += maxSwimSpeed * Time.deltaTime;

        // Calculates vertical movement during edge cases.
        if (!underWater && !DidAJump)
        {
            if (MoveVec.y > 0)
                MoveVec.y = 0;
        }

        // TODO: Bring them back to surface. For now it's just going to push them upwards.
        if (timeUnderWater > maxTimeUnderWater && underWater)
        {
            
        }

        if (Left && !Right)
            MoveVec.x -= maxSwimSpeed * Time.deltaTime;
        else if (Right && !Left)
            MoveVec.x += maxSwimSpeed * Time.deltaTime;
        else if (MoveVec.x < (0 - maxSwimSpeed / swimAcceleration * Time.deltaTime))
            MoveVec.x += maxSwimSpeed * Time.deltaTime / 3f;
        else if (MoveVec.x > (0 + maxSwimSpeed / swimAcceleration * Time.deltaTime))
            MoveVec.x -= maxSwimSpeed * Time.deltaTime / 3f;

        maxSwimSpeed /= swimAcceleration;

        // Forces the player to return to the surface if they've been underwater for too long.
        if (timeUnderWater > maxTimeUnderWater && underWater)
        {
            MoveVec.y += maxSwimSpeed * Time.deltaTime;
            if (MoveVec.y > maxSwimSpeed) MoveVec.y = maxSwimSpeed;
        }*/
        maxSwimSpeed *= 5;
        float swimval = maxSwimSpeed * Time.deltaTime;
        float reverseAcc = 10;
        if (Up && !Down)
        {
            if (MoveVec.y < 0) swimval *= reverseAcc;
            else if (MoveVec.y > maxSwimSpeed) swimval = 0;
            ChangePlayerState(CharacterStates.Swimming);
        }
        else if (Down && !Up)
        {
            swimval *= -1;
            if (MoveVec.y > 0) swimval *= reverseAcc;
            if (MoveVec.y < maxSwimSpeed * -1) swimval = 0;
            ChangePlayerState(CharacterStates.Swimming);
        }
        else if (MoveVec.y > 0.05f)
        {
            swimval *= -2;
            ChangePlayerState(CharacterStates.SwimIdling);
        }
        else if (MoveVec.y < 0.05f)
        {
            swimval *= 2;
            ChangePlayerState(CharacterStates.SwimIdling);
        }
            

        MoveVec.y += swimval;
        swimval = maxSwimSpeed * Time.deltaTime;
        if (Left && !Right)
        {
            swimval *= -1;
            if (MoveVec.x > 0) swimval *= reverseAcc;
            if (MoveVec.x < maxSwimSpeed * -1) swimval = 0;
            ChangePlayerState(CharacterStates.Swimming);
        }
        else if (Right && !Left)
        {
            if (MoveVec.x < 0) swimval *= reverseAcc;
            if (MoveVec.x > maxSwimSpeed) swimval = 0;
            ChangePlayerState(CharacterStates.Swimming);
        }
        else if (MoveVec.x < 0.05f)
        {
            swimval *= 2;
            ChangePlayerState(CharacterStates.SwimIdling);
        }
            
        else if (MoveVec.x > 0.05f)
        {
            swimval *= -2;
            ChangePlayerState(CharacterStates.SwimIdling);
        }
            

        MoveVec.x += swimval;
        maxSwimSpeed /= 5;
        RBody.velocity = MoveVec;
    }

    // Controls for a player that is climbing a ladder.
    private void Climb()
    {
        Vector3 LadderUp = IkTools.LadderSlope.normalized;
        IkTools.BodyPos = BackTarget;
        ChangePlayerState(CharacterStates.Idling);
        if (Up)
        {
            if (this.transform.position.y > HighClimb || IkTools.DisableUp)
            {
                MoveVec = Vector3.zero;
                ForcePlayerState(CharacterStates.Paused);
                IkTools.Still();
            }
            else
            {
                MoveVec = ClimbSpeed * LadderUp;
                ForcePlayerState(CharacterStates.Idling);
                IkTools.ClimbingUp();
            }
                
            // Play animation
        }
        else if (Down)
        {
            if (this.transform.position.y < LowClimb || IkTools.DisableDown)
            {
                IkTools.FlushIk();
                NoAnimUpdate = false;
                SCR_Ladder LadderScript = Ladder.GetComponent<SCR_Ladder>();
                LadderScript.climbing = false;
                LadderScript.ReleaseTrigger();
                if (LadderUp.x > 0.0f && !LastKeypress) TurnCharacter();
                else if (LadderUp.x < 0.0f && LastKeypress) TurnCharacter();
                LadderScript.DoRotationCalculations(false);
                IsClimbing = false;
                InteractingWith = null;
            }
            else
            {
                if (!IkTools.DisableDown)
                {
                    MoveVec = (ClimbSpeed * LadderUp.normalized) * -1.0f;
                    ForcePlayerState(CharacterStates.Idling);
                    IkTools.ClimbingDown();
                }
            }
                
            // Play animation
        }
        else
        {
            ForcePlayerState(CharacterStates.Paused);
            IkTools.Still();
            MoveVec = Vector3.zero;
        }

        if(MoveVec == Vector3.zero && Ladder.GetComponent<SCR_Ladder>().ClamberEnabled)
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
    /// Causes the player to jump off of a ladder that they are climbing on.
    /// </summary>
    public void JumpOff()
    {
        IkTools.FlushIk();
        NoAnimUpdate = false;
        if (Ladder.GetComponent<SCR_Ladder>().ReleaseLadderDoTrigger) Ladder.GetComponent<SCR_Ladder>().ReleaseTrigger();
        MoveVec.y = JumpForce + 5.0f;
        if (Left && !Right)
        {
            MoveVec.x = JumpForce / 2 * -1;
            if (Ladder.transform.up.x > 0.0f) ForceTurnCharacter();
        } 
        else if (!Left && Right)
        {
            MoveVec.x = JumpForce / 2;
            if (Ladder.transform.up.x < 0.0f) ForceTurnCharacter();
        }
        JumpingOff = true;
        Jump(Time.deltaTime);
    }

    /// <summary>
    /// This is a temporary summary TODO: MATT
    /// </summary>
    /// <param name="high"></param>
    /// <param name="low"></param>
    public void OnClimbable(float high, float low)
    {
        FallStartHeight = 0.0f;
        HighClimb = high;
        LowClimb = low;
        IsClimbing = true;
        NoAnimUpdate = true;
        MoveVec.x = 0;
        ForcePlayerState(CharacterStates.ClimbingUp);
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
    public void Grounded()
    {
        bool isGroundedResult = false;
        //Create two locations to trace from so that we can have a little bit of 'dangle' as to whether
        //or not the character is on an object.
        Vector3 YOffset = new Vector3(0.0f, YTraceOffset, 0.0f);
        Vector3 RightPosition = transform.position + (InitialDir.normalized * 0.1f) + YOffset;
        Vector3 LeftPosition = transform.position + (InitialDir.normalized * -0.1f) + YOffset;
        RaycastHit LeftResult;
        RaycastHit RightResult;
        float raycastExtension = 0.3f;
        //Raycast to find slope of ground beneath us. Needs to extend lower than our raycast that decides if grounded 
        //because we want our velocity to match the slope of the surface slightly before we return true.
        //This prevents a weird bouncing effect that can happen after a player lands. 
        if (Physics.Raycast(RightPosition, Vector3.down, out RightResult, GroundTraceDistance + raycastExtension, GroundLayer))
        {
            if (RightResult.distance <= GroundTraceDistance)
            {
                isGroundedResult = true;
            }
            if (MoveDir)
            {
                HitInfo = RightResult;
            }
        }
        if (Physics.Raycast(LeftPosition, Vector3.down, out LeftResult, GroundTraceDistance + raycastExtension, GroundLayer))
        {
            if (LeftResult.distance <= GroundTraceDistance)
            {
                isGroundedResult = true;
            }
            if (!MoveDir)
            {
                HitInfo = LeftResult;
            }
        }
        if(((RightResult.distance < 0.96f * GroundTraceDistance) && (LeftResult.distance < 0.96f * GroundTraceDistance))
            && isGroundedResult)
        {
            Vector3 NewPos = gameObject.transform.position;
            NewPos.y += (0.04f * GroundTraceDistance);
            gameObject.transform.position = NewPos; 
        }
        PlayerGrounded = isGroundedResult;
    }
    
    public void ForceTurnCharacter()
    {
        TurnOverride = true;
        LastKeypress = !MoveDir;
        TurnCharacter();
    }

    private void TurnCharacter()
    {
        if ((!InAnimationOverride && !IsLockedAnims) || TurnOverride)
        {
            //If we turn, we flip the boolean the signifies player direction
            MoveDir = !MoveDir;
            if (MoveDir) SCR_EventManager.TriggerEvent("CharacterTurn", 1);
            else SCR_EventManager.TriggerEvent("CharacterTurn", 0);
            if (!IsTurnRestricted || TurnOverride)
            {
                Vector3 TurnAround = new Vector3(0.0f, 180.0f, 0.0f);
                RefToModel.transform.Rotate(TurnAround);
                if (PlayerGrounded)
                {
                    SpeedModifier = 0.0f;
                }
                if (TurnOverride) TurnOverride = false;
            }
        }
    }

    private void CalculateMoveVec()
    {
        /*
        //Determine whether the player is swimming or not.
        if (inWater || DidAJump) InWater();
        else swimming = false;
        if (swimming)
        {
            Swim();
            return;
        }
        */

        //If we are on a slope, we need our velocity to be parallel to the slope. We find this through 
        //a cross product of the normal of that slope, and our right and left vectors.
        if (PlayerGrounded)
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
        if (!PlayerGrounded)
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

    public GameObject GetRefToModel()
    {
        return RefToModel;
    }

    public void SetSpeedPercentage(float modifier)
    {
        MoveSpeedPercentage = modifier;
    }

    public void SetSpeed(float speed)
    {
        MaxMoveSpeed = speed;
    }

    public float GetSpeed()
    {
        return MaxMoveSpeed;
    }

    public void StartPushing()
    {
        PushingAllowed = true;
        RestrictTurning();
    }

    public void StopPushing()
    {
        PushingAllowed = false;
        UnrestrictTurning();
    }

    public void RestrictTurning()
    {
        if (MoveDir) MoveDirAtRestricted = 1;
        else MoveDirAtRestricted = 2;
        IsTurnRestricted = true;
    }

    public void UnrestrictTurning()
    {
        IsTurnRestricted = false;
        if ((MoveDirAtRestricted == 1 && !MoveDir) || (MoveDirAtRestricted == 2 && MoveDir))
        {
            Vector3 TurnAround = new Vector3(0.0f, 180.0f, 0.0f);
            RefToModel.transform.Rotate(TurnAround);
        }
        MoveDirAtRestricted = 0;
    }

    public void StopAnimationChange()
    {
        NoAnimUpdate = true;
    }

    public void ResumeAnimationChange()
    {
        NoAnimUpdate = false;
    }

    /// <summary>
    /// Sets the character's velocity to Vector3.Zero, and prevents the CharacterManager from updating velocity unitl UnfreezeVelocity() is called
    /// </summary>
    public void FreezeVelocity(CharacterStates AnimState)
    {
        VelocityAllowed = false;
        LockAnim(AnimState);
    }

    public void FreezeVelocity()
    {
        VelocityAllowed = false;
        LockAnim(PlayerState);
    
    }

    /// <summary>
    /// Allows the CharacterManager to begin updating velocity again.
    /// </summary>
    public void UnfreezeVelocity()
    {
        VelocityAllowed = true;
        UnlockAnim();
        if (LastKeypress != MoveDir) TurnCharacter();
        if ((Left || Right) && PlayerGrounded) ChangePlayerState(CharacterStates.Running);
        else if (PlayerGrounded) ChangePlayerState(CharacterStates.Idling);
        else ChangePlayerState(CharacterStates.Falling);
    }

    private void Jump(float DeltaTime)
    {
        // If the player is climbing a ladder and wants to jump off, then they will jump off.
        if (IsClimbing || JumpingOff)
        {
            MoveVec.y = JumpForce;
            //Tell anim manager to play a jump animation.
            ChangePlayerState(CharacterStates.Jumping);
            OnBeginJump();
            IsClimbing = false;
            return;
        }
        else // Otherwise, they also need to have their movement determined.
        {
            CalculateMoveVec();
        }

        //If the player has pressed an UP key and the player is currently standing on the ground
        if (Up && PlayerGrounded && !StateChangeLocked)
        {
            if (CanJump)
            {
                FallingOff = false;
                if (DidAJump) OnEndJump();
                //Need a different jump force if we are moving uphill while jumping.
                if ((GroundAngle - 90) > 0 && (GroundAngle < MaxGroundAngle)) MoveVec.y = JumpForce + ((GroundAngle - 90) / 100.0f);
                else MoveVec.y = JumpForce;
                OnBeginJump();
            }
        }
        //If UP has not been pressed and the player is currently on the ground, the y component of their velocity should be zero
        else if (!Up && PlayerGrounded)
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
            if (!PlayerGrounded)
            {
                if (Mathf.Abs(MoveVec.y) < 0.1f && !IsClimbing) FallStartHeight = gameObject.transform.position.y;
                if (MoveVec.y > 0.0f) MoveVec.y -= UpGravityOnPlayer * DeltaTime;
                else if (MoveVec.y > -MaxFallVelocity)
                {
                    MoveVec.y -= DownGravityOnPlayer * DeltaTime;
                }
            }
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
        if (LastKeypress && !MoveDir) TurnCharacter();
        else if (!LastKeypress && MoveDir) TurnCharacter();
        if (FallStartHeight - gameObject.transform.position.y >= HardFallDistance)
        {
            FallStartHeight = 0.0f;
            AmInHardFall = true;
            FreezeVelocity(CharacterStates.Lying);
            StartCoroutine(Timer(LengthOfHardFallAnim, UnfreezeVelocity));
            MoveSpeedPercentage = PostHardFallSpeed;
            StartCoroutine(Timer(LengthOfSlowdown, 1.0f, SetSpeedPercentage));
            StartCoroutine(Timer(LengthOfHardFallAnim + 0.4f, DisableHardFall));

        }
    }

    private void DisableHardFall()
    {
        AmInHardFall = false;
    }

    public bool IsCharacterInHardFall()
    {
        return AmInHardFall;
    }

    private void MoveCharacter(float DeltaTime)
    {

        //If our LEFT button is held, and we are supposed to be moving left, and if move while jumping is on or we are grounded, Accelerate
        if ((Left && !MoveDir) && !(!MoveWhileJumping && !PlayerGrounded))
        {
            SpeedModifier += Acceleration * DeltaTime;
            if (SpeedModifier >= 1.0f) SpeedModifier = 1.0f;
        }
        //If our Right button is held, and we are supposed to be moving Right, and if move while jumping is on or we are grounded, Accelerate
        else if ((Right && MoveDir) && !(!MoveWhileJumping && !PlayerGrounded))
        {
            SpeedModifier += Acceleration * DeltaTime;
            if (SpeedModifier >= 1.0f) SpeedModifier = 1.0f;
        }
        //In all other cases we want to decelerate, but by how much is dependent on if we are grounded or not.
        else
        {
            //If we are grounded, decelerate quickly
            if (PlayerGrounded)
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
        float MoveSpeed = MaxMoveSpeed * MoveSpeedPercentage;

        if (FallingOff)
        {
            if (MoveSpeed == 0) MoveSpeed = 1;
            if (SpeedModifier == 0) SpeedModifier = 1;
            MoveVec.x = RBody.velocity.x / MoveSpeed / SpeedModifier;
        }

        // If the player is climbing on a ladder, they move in the direction of the ladder.
        if (IsClimbing)
        {
            Climb();
            FinalVel = MoveVec;
        }
        // If the player is jumping off of a ladder.
        else if (JumpingOff)
        {
            FinalVel = new Vector3(MoveVec.x, MoveVec.y, 0);
            JumpingOff = false;
            FallingOff = true;
        }
        else if (PlayerState == CharacterStates.Swimming || PlayerState == CharacterStates.SwimIdling)
        {
            if (MoveVec.magnitude > maxSwimSpeed)
                MoveVec = MoveVec.normalized * maxSwimSpeed;
            if (MoveVec.magnitude < maxSwimSpeed / 10 && (!Up && !Down && !Left && !Right))
                MoveVec = new Vector3(0, 0, 0);

            FinalVel = MoveVec;
        }
        //If we are grounded and we didn't just jump move along slope of surface we are on.
        else if (PlayerGrounded && !DidAJump) // Changed by Matt for Testing from "if" to "else if"
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
        if (VelocityAllowed) RBody.velocity = FinalVel;
        else RBody.velocity = Vector3.zero;
    }

    private void DeterminePlayerState()
    {
        if (!NoAnimUpdate)
        {
            if (!PlayerGrounded && MoveVec.y > 0.0f) ChangePlayerState(CharacterStates.Jumping);
            if (PlayerGrounded && (Left || Right))
            {
                if (PushingAllowed && ((MoveDir && MoveDirAtRestricted == 1) || (!MoveDir && MoveDirAtRestricted == 2)))
                    ChangePlayerState(CharacterStates.Pushing);
                else if (PushingAllowed && ((MoveDir && MoveDirAtRestricted == 2) || (!MoveDir && MoveDirAtRestricted == 1)))
                    ChangePlayerState(CharacterStates.Pulling);
                else ChangePlayerState(CharacterStates.Running);
            }
            if (!PlayerGrounded && MoveVec.y < 0.0f) ChangePlayerState(CharacterStates.Falling);
            if (PlayerGrounded && !(Left || Right)) ChangePlayerState(CharacterStates.Idling);
        }
    }

    //All of the following is trash and I feel bad :(

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
        FreezeVelocity(CharacterStates.Idling);
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
