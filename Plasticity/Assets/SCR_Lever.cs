using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SCR_Lever : MonoBehaviour {
    //Input event listeners
    private UnityAction<int> InteractListener;
    private UnityAction<int> UpListener;
    private UnityAction<int> DownListener;
    private int Up;
    private int Down;
    private bool Interact;
    
    private int direction; // -1 for down, 0 for neutral, 1 for up
    private bool Inside;
    private GameObject Character;
    private SCR_IKToolset IkTools;
    private SCR_CharacterManager CharacterManager;

    public GameObject LeverHandle;
    public SCR_Elevator elevator;

    private void Awake()
    {
        InteractListener = new UnityAction<int>(InteractPressed);
        UpListener = new UnityAction<int>(UpPressed);
        DownListener = new UnityAction<int>(DownPressed);
    }


    private void OnEnable()
    {
        SCR_EventManager.StartListening("InteractKey", InteractListener);
        SCR_EventManager.StartListening("UpKey", UpListener);
        SCR_EventManager.StartListening("DownKey", DownListener);
    }

    private void OnDisable()
    {
        SCR_EventManager.StopListening("InteractKey", InteractListener);
        SCR_EventManager.StopListening("UpKey", UpListener);
        SCR_EventManager.StopListening("DownKey", DownListener);
    }

    private void InteractPressed(int value)
    {
        if (value == 1)
        {
            Interact = true;
            if (Inside)
                GrabLever();
        }
        else
        {
            Interact = false;
            GrabLever();
            direction = 0;
            elevator.Neutral();
        }
    }

    private void UpPressed(int value)
    {
        Up = value;
        Actuate();
    }

    private void DownPressed(int value)
    {
        Down = value;
        Actuate();
    }

    // Use this for initialization
    void Start () {
        direction = 0;
        Up = 0;
        Down = 0;
        Interact = false;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Character")
        {
            Inside = true;
            if (Character == null)
            {
                Character = other.gameObject;
                IkTools = Character.GetComponent<SCR_IKToolset>();
                CharacterManager = Character.GetComponent<SCR_CharacterManager>();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.tag == "Character")
        {
            Inside = false;
            CharacterManager.StateChangeLocked = false;
        }
    }

    // TODO: IK stuff
    private void GrabLever()
    {
        //Set the character to interact with this.
        if (Interact && CharacterManager.InteractingWith == null && CharacterManager.PlayerGrounded && !CharacterManager.IsCharacterInHardFall())
        {
            CharacterManager.InteractingWith = gameObject;
            CharacterManager.StateChangeLocked = true;
            CharacterManager.FreezeVelocity();
        }
        if (!Interact)
        {
            CharacterManager.InteractingWith = null;
            CharacterManager.StateChangeLocked = false;
            CharacterManager.UnfreezeVelocity();
        }
            
    }

    private void Actuate()
    {
        if (CharacterManager.InteractingWith == gameObject)
        {
            if (Up == 1 && Down == 1)
            {
                direction = 0;
                elevator.Neutral();
            }
            else if (Up == 0 && Down == 1)
            {
                direction = -1;
                elevator.Down();
            }
            else if (Up == 1 && Down == 0)
            {
                direction = 1;
                elevator.Up();
            }
            else if (Up == 0 && Down == 0)
            {
                direction = 0;
                elevator.Neutral();
            }
        }
    }
}
