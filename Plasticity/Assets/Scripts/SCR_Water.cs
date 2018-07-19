using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_Water : MonoBehaviour {
    //private float height;
    public Transform trans;

	// Use this for initialization
	void Start () {
        //height = trans.position.y + trans.localScale.y/2;
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Character")
        {
            //other.GetComponent<SCR_CharacterManager>().IsInWater(true, height);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Character")
        {
            //other.GetComponent<SCR_CharacterManager>().IsInWater(false, -1);
        }
    }
}
