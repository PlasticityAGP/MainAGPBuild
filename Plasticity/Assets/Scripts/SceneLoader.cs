using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLoader : MonoBehaviour {

    [SerializeField]
    [Tooltip("A list of scenes we want to load in an additive and asynchronous manner to the current scene")]
    private string[] LevelArray;


	void Start () {
        LoadScenes();
	}
	

	void Update () {
		
	}


    //Called in Start(), loads specified scenes.
    private void LoadScenes()
    {
        //If no scenes are specified in the inspector, we don't do anything and return
        if (LevelArray.Length == 0) return;

        //Loop through LevelArray scenes specified in the inspector and load all scenes in an 
        //additive and asynchronous manner to the current level 
        for (int i = 0; i < LevelArray.Length; ++i)
        {
            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(LevelArray[i], UnityEngine.SceneManagement.LoadSceneMode.Additive);
        }
        
    }
}
