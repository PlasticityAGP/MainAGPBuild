using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_ZDrag : MonoBehaviour {

    private SCR_DragDrop ScriptOfParent;

    // Use this for initialization
    void Start () {
        ScriptOfParent = gameObject.transform.parent.GetComponentInChildren<SCR_DragDrop>();
	}

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Character")
        {
            ScriptOfParent.InTrigger(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Character")
        {
            ScriptOfParent.FreezeAll();
        }
    }



    // Update is called once per frame
    void Update () {
		
	}
}
