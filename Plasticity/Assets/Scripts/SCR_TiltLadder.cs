using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SCR_TiltLadder : MonoBehaviour {
    [SerializeField]
    private float StrengthOfGirl;
    private UnityAction<int> InteractListener;
    private bool Interact;
    private Vector3 ZVec = new Vector3(0.0f, 0.0f, 1.0f);
    [SerializeField]
    private AnimationCurve[] LeftHandCurves;
    [SerializeField]
    private AnimationCurve[] RightHandCurves;
    private GameObject Character;
    private SCR_IKToolset IkTools;
    private SCR_CharacterManager CharacterManager;
    [SerializeField]
    private GameObject LeftHandEffector;
    [SerializeField]
    private GameObject RightHandEffector;
    private bool Inside;
    [SerializeField]
    private float SlowDownSpeed;
    private float InitialSpeed;
    private bool PushEnabled = false;

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

    private void InteractPressed(int value)
    {
        //The value passed by the event indicates whether or not the key is pressed down.
        if (value == 1)
        {
            Interact = true;
            if (Inside)
            {
                GrabLadder();
            }
        }
      else
        {
            Interact = false;
            if (Inside)
            {
                ReleaseLadder();
            }
        }
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Character")
        {
            Inside = true;
            Character = other.gameObject;
            IkTools = Character.GetComponent<SCR_IKToolset>();
            CharacterManager = Character.GetComponent<SCR_CharacterManager>();
            InitialSpeed = CharacterManager.MoveSpeed;
            if (Interact)
            {
                GrabLadder();
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.tag == "Character" && PushEnabled)
        {
            if (CharacterManager.MoveDir)
            {
                gameObject.transform.parent.GetComponent<Rigidbody>().AddTorque(ZVec * -(1.0f * StrengthOfGirl));
            }
            else
            {
                gameObject.transform.parent.GetComponent<Rigidbody>().AddTorque(ZVec * (1.0f * StrengthOfGirl));
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.tag == "Character")
        {
            Inside = false;
            if (Interact)
            {
                IkTools.SetEffectorTarget("LeftHand", null);
                IkTools.SetEffectorTarget("RightHand", null);
                ReleaseLadder();
            }
        }
    }

    private void GrabLadder()
    {
        if(CharacterManager.InteractingWith == null)
        {
            IkTools.SetEffectorTarget("LeftHand", LeftHandEffector);
            IkTools.SetEffectorTarget("RightHand", RightHandEffector);
            IkTools.StartEffectorLerp("LeftHand", LeftHandCurves[0], 0.5f);
            IkTools.StartEffectorLerp("RightHand", RightHandCurves[0], 0.5f);
            CharacterManager.MoveSpeed = SlowDownSpeed;
            CharacterManager.InteractingWith = gameObject;
            PushEnabled = true;
        }
    }

    private void ReleaseLadder()
    {
        if (CharacterManager.InteractingWith == gameObject)
        {
            IkTools.SetEffectorTarget("LeftHand", LeftHandEffector);
            IkTools.SetEffectorTarget("RightHand", RightHandEffector);
            IkTools.StartEffectorLerp("LeftHand", LeftHandCurves[1], 0.5f);
            IkTools.StartEffectorLerp("RightHand", RightHandCurves[1], 0.5f);
            CharacterManager.MoveSpeed = InitialSpeed;
            CharacterManager.InteractingWith = null;
            PushEnabled = false;
        }
    }
}
