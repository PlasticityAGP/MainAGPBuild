using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_ZDrag : MonoBehaviour {

    //Reference to the DragDrop script. Since the actual moving of the object is the same, we call functions in DragDrop
    private SCR_DragDrop ScriptOfParent;

    //IsInside lets the SCR_ZJump script know if the character is within the trigger associated with this script. This is a necessary
    //workaround because Unity does not allow two triggers on one game object
    [HideInInspector]
    public bool IsInside = false; 

    // Use this for initialization
    void Start () {
        //Get reference to script
        ScriptOfParent = gameObject.transform.parent.GetComponentInChildren<SCR_DragDrop>();
	}

    private void OnTriggerEnter(Collider other)
    {
        //If the object that has just entered is a character, let the Jump script
        if (other.gameObject.tag == "Character")
        {
            ScriptOfParent.IsZ = true;
            IsInside = true;
            if (ScriptOfParent.Interact) ScriptOfParent.EnteredAndInteracted();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        //If the obj is trigger, call the In trigger function in the Drag Drop script
        if (other.gameObject.tag == "Character")
        {
            ScriptOfParent.InTrigger(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //We want to freeze the movement of the box if we are not in range to push it
        if (other.gameObject.tag == "Character")
        {
            ScriptOfParent.IsZ = false;
            IsInside = false;
            ScriptOfParent.FreezeAll();
        }
    }



    // Update is called once per frame
    void Update () {
		
	}
}
