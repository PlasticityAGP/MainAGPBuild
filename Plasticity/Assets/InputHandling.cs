using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandling : MonoBehaviour {

    [SerializeField]
    [Tooltip("Keys that will be bound to the move up action")]
    private KeyCode[] MoveUp;
    [SerializeField]
    [Tooltip("Keys that will be bound to the move left action")]
    private KeyCode[] MoveLeft;
    [SerializeField]
    [Tooltip("Keys that will be bound to the move right action")]
    private KeyCode[] MoveRight;
    [SerializeField]
    [Tooltip("Keys that will be bound to the move down action")]
    private KeyCode[] MoveDown;
    [SerializeField]
    [Tooltip("Keys that will be bound to the interact action")]
    private KeyCode[] Interact;

    void Start () {

	}
	
	void Update () {
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
                //do an event thingy
                //Debug.Log("Pressing an UP key");
                return;
            }
        }
    }

    private void LeftKeys()
    {
        for (int i = 0; i < MoveLeft.Length; ++i)
        {
            if (Input.GetKeyDown(MoveLeft[i]))
            {
                //do an event thingy
                //Debug.Log("Pressing a LEFT key");
                return;
            }
        }
    }

    private void RightKeys()
    {
        for (int i = 0; i < MoveRight.Length; ++i)
        {
            if (Input.GetKeyDown(MoveRight[i]))
            {
                //do an event thingy
                //Debug.Log("Pressing a RIGHT key");
                return;
            }
        }
    }

    private void DownKeys()
    {
        for (int i = 0; i < MoveDown.Length; ++i)
        {
            if (Input.GetKeyDown(MoveDown[i]))
            {
                //do an event thingy
                //Debug.Log("Pressing a DOWN key");
                return;
            }
        }
    }

    private void InteractKeys()
    {
        for (int i = 0; i < Interact.Length; ++i)
        {
            if (Input.GetKeyDown(Interact[i]))
            {
                //do an event thingy
                //Debug.Log("Pressing an INTERACT key");
                return;
            }
        }
    }
}
