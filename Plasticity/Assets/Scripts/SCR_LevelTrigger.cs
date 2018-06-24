﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_LevelTrigger : MonoBehaviour {

    [SerializeField]
    private int TriggerID;

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
            SCR_EventManager.TriggerEvent("LevelTrigger", TriggerID);
        }
    }
}
