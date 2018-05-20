using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SCR_SceneLoader : MonoBehaviour {

    [SerializeField]
    [Tooltip("What level state data we want the scene loader to operate based on")]
    private SCR_LevelStates LevelData; 
    private string[] LevelArray;

	void Start () {
        //Grab the array from our saved data asset.
        LevelArray = LevelData.LevelArray;
        //Below is an example of how we can edit data asset at runtime.
        //LevelData.CurrentLevel = "NotCharacter";
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
            SceneManager.LoadSceneAsync(LevelArray[i], LoadSceneMode.Additive);
        }
        
    }
}
