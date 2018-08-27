using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;

public class SCR_Ladder : SCR_GameplayStatics {

    [Tooltip("Flags whether or not the player will clamber at the top of the ladder")]
    public bool ClamberEnabled;
    [SerializeField]
    [Tooltip("The effectors the players will reach towards as they clamber at the top of the ladder")]
    [ValidateInput("NotEmpty", "We need to have a nonzero number of effectors defined")]
    [ShowIf("ClamberEnabled")]
    private GameObject[] EffectorTargets;
    [SerializeField]
    [Tooltip("The animation curves defining IK behavior while clambering")]
    [ValidateInput("NotEmpty", "We need to have a nonzero number of animation curves defined")]
    [ShowIf("ClamberEnabled")]
    private AnimationCurve[] CurveOfEffectors;
    [SerializeField]
    [Tooltip("The Speed the player will Lerp upwards while clambering")]
    [ValidateInput("GreaterThanZero", "Clamber Speed needs to be greater than zero")]
    [ShowIf("ClamberEnabled")]
    private float ClamberSpeed;
    [Tooltip("Determines if we will fire an event after letting go of the ladder")]
    public bool ReleaseLadderDoTrigger;
    [SerializeField]
    private bool UsingLowerBarrier;
    [Tooltip("The event ID we are going to fire")]
    [ShowIf("ReleaseLadderDoTrigger")]
    public string ReleaseTriggerName;
    [SerializeField]
    private GameObject LowerBarrier;
    [SerializeField]
    private float OffTheTop;
    [SerializeField]
    private float OffTheBottom;


    private SCR_CharacterManager CharacterManager;
    private GameObject Character;
    private SCR_IKToolset IkTools;
    private UnityAction<int> UpListener;
    private UnityAction<int> HorizontalListener;
    [HideInInspector]
    public bool climbing;
    private bool reaching;
    private bool ClamberDir;
    private bool AmLerpingCharacter;
    private bool Inside;
    public bool InsideTop;



    // Sets up a listener which calls Up() and Side() when the respective keys are pressed.
    private void Awake()
    {
        UpListener = new UnityAction<int>(Up);
        HorizontalListener = new UnityAction<int>(Side);
    }

    private void OnEnable()
    {
        SCR_EventManager.StartListening("UpKey", UpListener);
    }

    private void OnDisable()
    {
        SCR_EventManager.StopListening("UpKey", UpListener);
    }

    // Use this for initialization.
    void Start () {
        climbing = false;
        reaching = false;
	}

    // Gives the ladder a reference to the character if it does not have one and 
    // causes the listener to start paying attention.
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Character")
        {
            Inside = true;
            Character = other.gameObject;
            if (IkTools == null) IkTools = other.GetComponentInChildren<SCR_IKToolset>();
            if (CharacterManager == null) CharacterManager = other.GetComponent<SCR_CharacterManager>();
        }
    }

    /// <summary>
    /// Initiates clambering at the top of the ladder depending on what direction the player is relative to the ladder
    /// </summary>
    /// <param name="direction">1 for moving to the right, 2 for moving to the left</param>
    public void Clamber(int direction)
    {
        if (ClamberEnabled)
        {
            CharacterManager.AmClambering = true;
            CharacterManager.FreezeVelocity(SCR_CharacterManager.CharacterStates.Idling);
            CharacterManager.IsClimbing = false;
            ReleaseTrigger();
            if (direction == 1)
            {
                ClamberDir = true;
                IkTools.SetEffectorTarget("LeftHand", EffectorTargets[0]);
                IkTools.SetEffectorTarget("RightHand", EffectorTargets[1]);
            }
            else
            {
                ClamberDir = false;
                IkTools.SetEffectorTarget("LeftHand", EffectorTargets[2]);
                IkTools.SetEffectorTarget("RightHand", EffectorTargets[3]);
            }
            IkTools.StartEffectorLerp("LeftHand", CurveOfEffectors[0], 1.0f);
            IkTools.StartEffectorLerp("RightHand", CurveOfEffectors[0], 1.0f);
            AmLerpingCharacter = true;
        }
        else
        {
            CharacterManager.IsClimbing = false;
            EndLerp();
        }
    }

    // The listener stops paying attention.
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Character")
        {
            Inside = false;
            CharacterManager.AmClambering = false;
        }
            
    }

    // The "up" key is pressed while the player is inside the ladder's trigger.
    private void Up(int val)
    {
        if (InsideTop && reaching) OffLadder();
        if (Inside)
        {
            if (reaching)
            {
                OffLadder();
            }
            else if (val == 1 && !climbing && !InsideTop)
            {
                InitiateClimb();
            }
        }
    }

    public void InitiateClimb()
    {
        if (UsingLowerBarrier) LowerBarrier.SetActive(false);
        OnLadder();
    }

    // Either the "Left" or "Right" keys are pressed while the player is on the ladder.
    private void Side(int val)
    {
        if (val == 1 && climbing)
            reaching = true;
        else
            reaching = false;
    }

    // The player hops on the ladder.
    private void OnLadder()
    {
        CharacterManager.Ladder = gameObject;
        CharacterManager.InteractingWith = gameObject;
        CharacterManager.StopAnimationChange();
        climbing = true;
        SCR_EventManager.StartListening("LeftKey", HorizontalListener);
        SCR_EventManager.StartListening("RightKey", HorizontalListener);
        float maxclimb = ((gameObject.transform.position.y + gameObject.transform.lossyScale.y) * gameObject.transform.up.normalized).y - (OffTheTop * gameObject.transform.up.normalized).y;
        float minclimb = ((gameObject.transform.position.y - gameObject.transform.lossyScale.y) * gameObject.transform.up.normalized).y + (OffTheBottom * gameObject.transform.up.normalized).y;
        CharacterManager.OnClimbable(maxclimb, minclimb);
    }

    // The player hops off the ladder.
    private void OffLadder()
    {
        if (UsingLowerBarrier) LowerBarrier.SetActive(true);
        ReleaseTrigger();
        climbing = false;
        reaching = false;
        SCR_EventManager.StopListening("LeftKey", HorizontalListener);
        SCR_EventManager.StopListening("RightKey", HorizontalListener);
        CharacterManager.InteractingWith = null;
        CharacterManager.ResumeAnimationChange();
        CharacterManager.JumpOff();
    }

    private void EndLerp()
    {
        climbing = false;
        ReleaseTrigger();
        CharacterManager.InteractingWith = null;
        reaching = false;
        IkTools.SetEffectorTarget("LeftHand", null);
        IkTools.SetEffectorTarget("RightHand", null);
        CharacterManager.UnfreezeVelocity();
        CharacterManager.AmClambering = false;
        AmLerpingCharacter = false;
    }

    private void LerpCharacter(float DeltaTime)
    {
        if(Character.transform.position.y < EffectorTargets[0].transform.position.y)
        {
            Vector3 newpos = new Vector3(Character.transform.position.x, Character.transform.position.y + (DeltaTime*ClamberSpeed), Character.transform.position.z);
            Character.transform.position = newpos;
        }
        else if (ClamberDir && Character.transform.position.x < EffectorTargets[0].transform.position.x)
        {
            Vector3 newpos = new Vector3(Character.transform.position.x + (DeltaTime * ClamberSpeed), Character.transform.position.y, Character.transform.position.z);
            Character.transform.position = newpos;
        }
        else if (!ClamberDir && Character.transform.position.x > EffectorTargets[2].transform.position.x)
        {
            Vector3 newpos = new Vector3(Character.transform.position.x - (DeltaTime * ClamberSpeed), Character.transform.position.y, Character.transform.position.z);
            Character.transform.position = newpos;
        }
        else
        {
            EndLerp();
        }
    }

    /// <summary>
    /// Will fire a LevelTrigger event with the ID defined by ReleaseTriggerID
    /// </summary>
    [HideInInspector]
    public void ReleaseTrigger()
    {
        if (ReleaseLadderDoTrigger)
            SCR_EventManager.TriggerEvent("LevelTrigger", ReleaseTriggerName);

    }

    void Update()
    {
        if (AmLerpingCharacter) LerpCharacter(Time.deltaTime);
    }
}
