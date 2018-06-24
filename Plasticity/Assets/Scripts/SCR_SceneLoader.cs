using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[System.Serializable]
public class PairObject
{
    public GameObject ObjToSet;
    public int State;
}

[System.Serializable]
public class SettingsArray
{
    public PairObject[] Settings;
}

public class SCR_SceneLoader : MonoBehaviour {

    [SerializeField]
    [Tooltip("What level state data we want the scene loader to operate based on")]
    private SCR_LevelStates LevelData; 
    private string[] LevelArray;
    private UnityAction<int> TriggerListener;
    [SerializeField]
    private SettingsArray[] LevelSettings;

    private void Awake()
    {
        //Register the callback functions related to each listener. These will be called as
        //the events these listeners are listening to get invoked 
        TriggerListener = new UnityAction<int>(TriggerEntered);
    }
    private void OnEnable()
    {
        //Register listeners with their events in the EventManager
        SCR_EventManager.StartListening("LevelTrigger", TriggerListener);
    }

    private void OnDisable()
    {
        SCR_EventManager.StopListening("LevelTrigger", TriggerListener);
    }

    private void TriggerEntered(int ID)
    {
        ActivateOnTrigger(LevelSettings[ID]);
    }

    private void ActivateOnTrigger(SettingsArray Data)
    {
        for (int i = 0; i < Data.Settings.Length; ++i)
        {
            if(Data.Settings[i].State == 1)
            {
                Data.Settings[i].ObjToSet.SetActive(true);
            }
            else if(Data.Settings[i].State == 0)
            {
                Data.Settings[i].ObjToSet.SetActive(false);
            }
        }
    }

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
