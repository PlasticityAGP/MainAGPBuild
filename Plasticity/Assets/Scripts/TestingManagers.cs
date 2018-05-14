using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/*
 * This script is temporary, and only exists so that I could test and confirm that the InputManager and EventManager are functioning
 * properly. This functionality will eventually be ported over to the character controller and this script deleted. 
 */

public class TestingManagers : MonoBehaviour {
    private UnityAction<int> UpListener;
    private UnityAction<int> DownListener;
    private UnityAction<int> LeftListener;
    private UnityAction<int> RightListener;
    private UnityAction<int> InteractListener;

    private bool Up = false;
    private bool Down = false;
    private bool Left = false;
    private bool Right = false;
    private bool Interact = false;

    private void Awake()
    {
        UpListener = new UnityAction<int>(UpPressed);
        DownListener = new UnityAction<int>(DownPressed);
        LeftListener = new UnityAction<int>(LeftPressed);
        RightListener = new UnityAction<int>(RightPressed);
        InteractListener = new UnityAction<int>(InteractPressed);
    }

    private void OnEnable()
    {
        EventManager.StartListening("UpKey", UpListener);
        EventManager.StartListening("DownKey", DownListener);
        EventManager.StartListening("LeftKey", LeftListener);
        EventManager.StartListening("RightKey", RightListener);
        EventManager.StartListening("InteractKey", InteractListener);
    }
    private void OnDisable()
    {
        EventManager.StopListening("UpKey", UpListener);
        EventManager.StopListening("DownKey", DownListener);
        EventManager.StopListening("LeftKey", LeftListener);
        EventManager.StopListening("RightKey", RightListener);
        EventManager.StopListening("InteractKey", InteractListener);
    }

    private void UpPressed(int value)
    {
        if(value == 1)
        {
            Up = true;
        }
        else
        {
            Up = false;
        }
    }

    private void DownPressed(int value)
    {
        if (value == 1)
        {
            Down = true;
        }
        else
        {
            Down = false;
        }
    }

    private void LeftPressed(int value)
    {
        if (value == 1)
        {
            Left = true;
        }
        else
        {
            Left = false;
        }
    }

    private void RightPressed(int value)
    {
        if (value == 1)
        {
            Right = true;
        }
        else
        {
            Right = false;
        }
    }

    private void InteractPressed(int value)
    {
        if (value == 1)
        {
            Interact = true;
        }
        else
        {
            Interact = false;
        }
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        //if (Up) Debug.Log("UP is pressed");
        //if (Down) Debug.Log("DOWN is pressed");
        //if (Right) Debug.Log("RIGHT is pressed");
        //if (Left) Debug.Log("LEFT is pressed");
        //if (Interact) Debug.Log("INTERACT is pressed");
    }
}
