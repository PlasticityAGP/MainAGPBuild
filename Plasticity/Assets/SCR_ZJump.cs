using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_ZJump : MonoBehaviour {
    [SerializeField]
    [Tooltip("Reference to the Character GameObject")]
    private GameObject Character;
    [SerializeField]
    [Tooltip("A reference point whose z value will be used to move the player in the z axis")]
    private GameObject ReferencePoint;
    private Vector3 NewPosition;
    private SCR_CharacterManager CharacterManager;


	// Use this for initialization
	void Start () {
        if (Character.GetComponent<SCR_CharacterManager>()) CharacterManager = Character.GetComponent<SCR_CharacterManager>();
        else Debug.LogError("We need a reference to a Character GameObject with an attached SCR_CharacterManager script in the DragDrop script");
    }

    private void OnTriggerStay(Collider other)
    {
        if (CharacterManager.NearZenith())
        {
            //NewPosition.x = Character.transform.position.x;
            //NewPosition.y = Character.transform.position.y;
            //NewPosition.z = ReferencePoint.transform.position.z;
            //Character.transform.position = NewPosition;
        }
    }

    // Update is called once per frame
    void Update () {
		
	}
}
