using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Cinemachine;

public class SCR_CameraManager : MonoBehaviour {

    //This is where our CameraManager script will eventually go. In here we will listen to events fired by triggers in order to blend between cameras
    //with our Cinemachine Brain. Will edit once world is built out and we have a better sense of what we will need from our Virtual Cameras.

#pragma warning disable 0414
    private CinemachineBrain CameraBrain;
#pragma warning restore 0414

    // Use this for initialization
    void Start () {
        if (gameObject.GetComponent<CinemachineBrain>()) CameraBrain = gameObject.GetComponent<CinemachineBrain>();
        else Debug.LogError("We need to have a Cinemachine brain attached to the main camera and we currently don't!");
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
