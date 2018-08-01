using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;

public class SCR_Ladder : SCR_GameplayStatics {

    [SerializeField]
    [Tooltip("Flags whether or not the player will clamber at the top of the ladder")]
    private bool ClamberEnabled;
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
    [SerializeField]
    [Tooltip("Determines if we will fire an event after letting go of the ladder")]
    private bool ReleaseLadderDoTrigger;
    [Tooltip("The event ID we are going to fire")]
    [ValidateInput("GreaterThanOrEqualZero", "Clamber Speed needs to be greater than zero")]
    [ShowIf("ReleaseLadderDoTrigger")]
    public int ReleaseTriggerID;


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



    // Sets up a listener which calls Up() and Side() when the respective keys are pressed.
    private void Awake()
    {
        UpListener = new UnityAction<int>(Up);
        HorizontalListener = new UnityAction<int>(Side);
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
            Character = other.gameObject;
            if (IkTools == null) IkTools = other.GetComponentInChildren<SCR_IKToolset>();
            if (CharacterManager == null) CharacterManager = other.GetComponent<SCR_CharacterManager>();
            SCR_EventManager.StartListening("UpKey", UpListener);
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
            CharacterManager.FreezeVelocity();
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
            SCR_EventManager.StopListening("UpKey", UpListener);
            CharacterManager.AmClambering = false;
        }
            
    }

    // The "up" key is pressed while the player is inside the ladder's trigger.
    private void Up(int val)
    {
        
        if (reaching)
            OffLadder();
        else if (val == 1 && !climbing)
        {
            OnLadder();
        }
    }

    // Either the "Left" or "Right" keys are pressed while the player is on the ladder.
    private void Side(int val)
    {
        if (val == 1)
            reaching = true;
        else
            reaching = false;
    }

    // The player hops on the ladder.
    private void OnLadder()
    {
        CharacterManager.Ladder = gameObject;
        CharacterManager.InteractingWith = gameObject;
        climbing = true;
        SCR_EventManager.StartListening("LeftKey", HorizontalListener);
        SCR_EventManager.StartListening("RightKey", HorizontalListener);
        //Debug.Log("started climbing");
        float maxclimb = ((transform.position.y + transform.localScale.y) * transform.up.normalized).y;
        float minclimb = ((transform.position.y - transform.localScale.y) * transform.up.normalized).y;
        CharacterManager.OnClimbable(maxclimb, minclimb);
    }

    // The player hops off the ladder.
    private void OffLadder()
    {
        climbing = false;
        ReleaseTrigger();
        reaching = false;
        SCR_EventManager.StopListening("LeftKey", HorizontalListener);
        SCR_EventManager.StopListening("RightKey", HorizontalListener);
        //Debug.Log("stopped climbing");
        CharacterManager.InteractingWith = null;
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
        {
            SCR_EventManager.TriggerEvent("LevelTrigger", ReleaseTriggerID);
        }
    }

    void Update()
    {
        if (AmLerpingCharacter) LerpCharacter(Time.deltaTime);
    }
}
