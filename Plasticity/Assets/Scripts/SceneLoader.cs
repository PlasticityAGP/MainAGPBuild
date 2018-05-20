﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour {

    [SerializeField]
    [Tooltip("What level state data we want the scene loader to operate based on")]
    private LevelStates LevelData; 
    private string[] LevelArray;

	void Start () {
        LevelArray = LevelData.LevelArray;
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
