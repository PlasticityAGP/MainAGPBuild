using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;

public class SCR_InputManager : SCR_GameplayStatics
{

    /*
     * The Input manager is the first place player input is registered. Key bindings can be adjusted in the editor without 
     * needing to update any code. The Input Manager fires events that are managed by the Event Manager and listened to by the 
     * Character Manager. This way, any object in the game can listen to input events, and the back end of where we are listening 
     * for input is all in the same place. 
     */

    // Singleton reference
    private static SCR_InputManager instance;

    [SerializeField]
    [Tooltip("Keys that will be bound to the move up action")]
    [ValidateInput("NotEmpty", "We must have at least one key specified for moving the player up")]
    private KeyCode[] MoveUp;
    [SerializeField]
    [Tooltip("Keys that will be bound to the move left action")]
    [ValidateInput("NotEmpty", "We must have at least one key specified for moving the player left")]
    private KeyCode[] MoveLeft;
    [SerializeField]
    [Tooltip("Keys that will be bound to the move right action")]
    [ValidateInput("NotEmpty", "We must have at least one key specified for moving the player right")]
    private KeyCode[] MoveRight;
    [SerializeField]
    [Tooltip("Keys that will be bound to the move down action")]
    [ValidateInput("NotEmpty", "We must have at least one key specified for moving the player down")]
    private KeyCode[] MoveDown;
    [SerializeField]
    [Tooltip("Keys that will be bound to the interact action")]
    [ValidateInput("NotEmpty", "We must have at least one key specified for player interaction")]
    private KeyCode[] Interact;

    /*
     * Need integers to store number of presses active at one time. If both W and UP ARROW are pressed at the same
     * time, but only one is released, we don't want the player to stop moving. This way, the key event only triggers with
     * value 1 when we want the player to start moving, and triggers with value 0 when we want them to stop, completely
     * independent of whether one specific key is up or down. 
     */
    private int UpPresses = 0;
    private int DownPresses = 0;
    private int LeftPresses = 0;
    private int RightPresses = 0;
    private int InteractPresses = 0;

    // Singleton reference accessor
    /// <summary>
    /// Returns an instance of the InputManager in the scene
    /// </summary>
    /// <returns>Instance of the InputManager</returns>
    public static SCR_InputManager GetInstance() { return instance; }

    void Start()
    {
        // Singleton logic
        if (!instance) instance = this;
        else
        {
            Debug.LogError("Multiple singleton instances.");
            Destroy(this);
        }
    }

    void Update()
    {
        UpKeys();
        LeftKeys();
        RightKeys();
        DownKeys();
        InteractKeys();
    }


    private void UpKeys()
    {
        for (int i = 0; i < MoveUp.Length; ++i)
        {
            if (Input.GetKeyDown(MoveUp[i]))
            {
                //If no other keys have triggered the up event and an up key is pressed, trigger pressed event
                if (UpPresses == 0)
                {
                    SCR_EventManager.TriggerEvent("UpKey", 1);
                    Bolt.CustomEvent.Trigger(gameObject, "UpPressed");
                }
                ++UpPresses;
            }

            if (Input.GetKeyUp(MoveUp[i]))
            {
                //If there aren't any other pressed up keys when we release an up key, trigger released event
                if (UpPresses == 1)
                {
                    SCR_EventManager.TriggerEvent("UpKey", 0);
                    Bolt.CustomEvent.Trigger(gameObject, "UpReleased");
                }
                --UpPresses;
            }
        }
    }

    private void LeftKeys()
    {
        for (int i = 0; i < MoveLeft.Length; ++i)
        {
            if (Input.GetKeyDown(MoveLeft[i]))
            {
                //If no other keys have triggered the left event and a left key is pressed, trigger pressed event
                if (LeftPresses == 0)
                {
                    SCR_EventManager.TriggerEvent("LeftKey", 1);
                    Bolt.CustomEvent.Trigger(gameObject, "LeftPressed");
                }
                ++LeftPresses;
            }

            if (Input.GetKeyUp(MoveLeft[i]))
            {
                //If there aren't any other pressed left keys when we release a left key, trigger released event
                if (LeftPresses == 1)
                {
                    SCR_EventManager.TriggerEvent("LeftKey", 0);
                    Bolt.CustomEvent.Trigger(gameObject, "LeftReleased");
                }
                --LeftPresses;
            }
        }
    }

    private void RightKeys()
    {
        for (int i = 0; i < MoveRight.Length; ++i)
        {
            if (Input.GetKeyDown(MoveRight[i]))
            {
                //If no other keys have triggered the right event and a right key is pressed, trigger pressed event
                if (RightPresses == 0)
                {
                    SCR_EventManager.TriggerEvent("RightKey", 1);
                    Bolt.CustomEvent.Trigger(gameObject, "RightPressed");
                }
                ++RightPresses;
            }

            if (Input.GetKeyUp(MoveRight[i]))
            {
                //If there aren't any other pressed right keys when we release a right key, trigger released event
                if (RightPresses == 1)
                {
                    SCR_EventManager.TriggerEvent("RightKey", 0);
                    Bolt.CustomEvent.Trigger(gameObject, "RightReleased");
                }
                --RightPresses;
            }
        }
    }

    private void DownKeys()
    {
        for (int i = 0; i < MoveDown.Length; ++i)
        {
            if (Input.GetKeyDown(MoveDown[i]))
            {
                //If no other keys have triggered the down event and a down key is pressed, trigger pressed event
                if (DownPresses == 0)
                {
                    SCR_EventManager.TriggerEvent("DownKey", 1);
                    Bolt.CustomEvent.Trigger(gameObject, "DownPressed");
                }
                ++DownPresses;
            }

            if (Input.GetKeyUp(MoveDown[i]))
            {
                //If there aren't any other pressed down keys when we release a down key, trigger released event
                if (DownPresses == 1)
                {
                    SCR_EventManager.TriggerEvent("DownKey", 0);
                    Bolt.CustomEvent.Trigger(gameObject, "DownReleased");
                }
                --DownPresses;
            }
        }
    }

    private void InteractKeys()
    {
        for (int i = 0; i < Interact.Length; ++i)
        {
            if (Input.GetKeyDown(Interact[i]))
            {
                //If no other keys have triggered the interact event and an interact key is pressed, trigger pressed event
                if (InteractPresses == 0)
                {
                    SCR_EventManager.TriggerEvent("InteractKey", 1);
                    Bolt.CustomEvent.Trigger(gameObject, "InteractPressed");
                }
                ++InteractPresses;
            }

            if (Input.GetKeyUp(Interact[i]))
            {
                //If there aren't any other pressed interact keys when we release an interact key, trigger released event
                if (InteractPresses == 1)
                {
                    SCR_EventManager.TriggerEvent("InteractKey", 0);
                    Bolt.CustomEvent.Trigger(gameObject, "InteractReleased");
                }
                --InteractPresses;
            }
        }
    }
}
