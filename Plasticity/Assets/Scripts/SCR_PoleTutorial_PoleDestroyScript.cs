using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_PoleTutorial_PoleDestroyScript : MonoBehaviour {

    public Transform ladderRot;
    public float destroyRot;
	
	void Update () {
        if (ladderRot.rotation.eulerAngles.z > destroyRot)
            gameObject.SetActive(false);
	}
}
