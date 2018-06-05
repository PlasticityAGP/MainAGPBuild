using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SCR_Ladder : MonoBehaviour {
    private SCR_CharacterManager CharacterManager;

    private UnityAction<int> UpListener;
    private UnityAction<int> HorizontalListener;
    private bool climbing;
    private bool reaching;

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
            if (CharacterManager == null) CharacterManager = other.GetComponent<SCR_CharacterManager>();
            SCR_EventManager.StartListening("UpKey", UpListener);
        }
    }

    // The listener stops paying attention.
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Character")
            SCR_EventManager.StopListening("UpKey", UpListener);
    }

    // The "up" key is pressed while the player is inside the ladder's trigger.
    private void Up(int val)
    {
        if (reaching)
            OffLadder();
        else if (val == 1 && !climbing)
            OnLadder();
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
        climbing = true;
        SCR_EventManager.StartListening("LeftKey", HorizontalListener);
        SCR_EventManager.StartListening("RightKey", HorizontalListener);
        //Debug.Log("started climbing");
        float maxclimb = this.transform.position.y + this.transform.localScale.y/2;
        float minclimb = this.transform.position.y - this.transform.localScale.y/2;
        CharacterManager.OnClimbable(maxclimb, minclimb);
    }

    // The player hops off the ladder.
    private void OffLadder()
    {
        climbing = false;
        reaching = false;
        SCR_EventManager.StopListening("LeftKey", HorizontalListener);
        SCR_EventManager.StopListening("RightKey", HorizontalListener);
        //Debug.Log("stopped climbing");
        CharacterManager.JumpOff();
    }

    // Not used.
    void Update()
    {

    }
}
