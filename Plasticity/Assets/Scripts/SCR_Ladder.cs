﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SCR_Ladder : MonoBehaviour {
    private SCR_CharacterManager CharacterManager;
    private GameObject Character;

    [SerializeField]
    private GameObject[] EffectorTargets;
    [SerializeField]
    private AnimationCurve[] CurveOfEffectors;
    [SerializeField]
    private float ClamberSpeed;
    private SCR_IKToolset IkTools;
    private UnityAction<int> UpListener;
    private UnityAction<int> HorizontalListener;
    private bool climbing;
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
            CharacterManager.Ladder = gameObject;
            SCR_EventManager.StartListening("UpKey", UpListener);
        }
    }

    public void Clamber(int direction)
    {
        CharacterManager.AmClambering = true;
        CharacterManager.FreezeVelocity();
        CharacterManager.IsClimbing = false;
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

    private void EndLerp()
    {
        climbing = false;
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

    void Update()
    {
        if (AmLerpingCharacter) LerpCharacter(Time.deltaTime);
    }
}
