using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_ZDrag : MonoBehaviour {

    private SCR_DragDrop ScriptOfParent;
    [HideInInspector]
    public bool IsInside = false; 

    // Use this for initialization
    void Start () {
        ScriptOfParent = gameObject.transform.parent.GetComponentInChildren<SCR_DragDrop>();
	}

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Character")
        {
            IsInside = true;
        }
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
            IsInside = false;
            ScriptOfParent.FreezeAll();
        }
    }



    // Update is called once per frame
    void Update () {
		
	}
}
