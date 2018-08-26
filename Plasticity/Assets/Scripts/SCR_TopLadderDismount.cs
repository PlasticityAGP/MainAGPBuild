using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_TopLadderDismount : MonoBehaviour {

    [SerializeField]
    private GameObject Trigger;
    private SCR_Ladder LadderScript;
    
	// Use this for initialization
	void Start () {
        LadderScript = Trigger.GetComponent<SCR_Ladder>();
	}

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Character")
        {
            LadderScript.InsideTop = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Character")
        {
            LadderScript.InsideTop = false;
        }
    }

    // Update is called once per frame
    void Update () {
		
	}
}
