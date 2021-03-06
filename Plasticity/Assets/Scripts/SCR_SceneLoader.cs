﻿#if UNITY_EDITOR
    using UnityEditor.SceneManagement;
#endif

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;

[System.Serializable]
public class PairObject
{
    public GameObject ObjToSet;
    public int State;
    public string Flag;
}

[System.Serializable]
public class SettingsArray
{
    public string SettingName;
    public PairObject[] Settings;
}

public class SCR_SceneLoader : MonoBehaviour {

    [SerializeField]
    [Tooltip("What level state data we want the scene loader to operate based on")]
    private SCR_LevelStates LevelData; 
    private string[] LevelArray;
    private UnityAction<string> TriggerListener;
    private UnityAction<int> LevelStateListener;
    [SerializeField]
    private SettingsArray[] LevelSettings;
    private float GameTimer = 0.0f;

    private void Awake()
    {
        //Register the callback functions related to each listener. These will be called as
        //the events these listeners are listening to get invoked 
        TriggerListener = new UnityAction<string>(TriggerEntered);
        LevelStateListener = new UnityAction<int>(LevelStateChange);
    }
    private void OnEnable()
    {
        //Register listeners with their events in the EventManager
        SCR_EventManager.StartListening("LevelTrigger", TriggerListener);
        SCR_EventManager.StartListening("SceneStateTrigger", LevelStateListener);
    }

    private void OnDisable()
    {
        SCR_EventManager.StopListening("LevelTrigger", TriggerListener);
        SCR_EventManager.StopListening("SceneStateTrigger", LevelStateListener);
    }

    private void LevelStateChange (int ID)
    {
        SCR_EventManager.TriggerEvent("GameTimerResult", GameTimer);
        string[] PuzzleStates = LevelData.PuzzleStates;
        ActivateOnTrigger(LevelSettings[FindByName(PuzzleStates[ID])]);
    }

    private void TriggerEntered(string ID)
    {
        ActivateOnTrigger(LevelSettings[FindByName(ID)]);
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
            else if(Data.Settings[i].State == 2)
            {
                SCR_EventManager.TriggerEvent("Timeline", Data.Settings[i].Flag);
                SCR_EventManager.TriggerEvent("TimelineInstruction", "Resume");
            }
            else if(Data.Settings[i].State == 3)
            {
                if (Data.Settings[i].Flag == "DisableBox") SCR_EventManager.TriggerEvent("DisableBox", Data.Settings[i].ObjToSet.name);
                Data.Settings[i].ObjToSet.GetComponentInChildren<PlayableDirector>().Play();
            }
        }
    }

    private int FindByName(string Name)
    {
        for(int i = 0; i < LevelSettings.Length; ++i)
        {
            if (LevelSettings[i].SettingName == Name) return i;
        }
        Debug.LogError("Setting name not found");
        return -1;
    }

    void Start () {
#if UNITY_EDITOR
        if (EditorSceneManager.loadedSceneCount > 1) {
            foreach (var scene in EditorSceneManager.GetAllScenes()) {
                if (!scene.name.Contains("Base")) {
                    EditorSceneManager.UnloadSceneAsync(scene);
                }
            }
        }
#endif
        
        //Grab the array from our saved data asset.
        LevelArray = LevelData.LevelArray;
        //Below is an example of how we can edit data asset at runtime.
        //LevelData.CurrentLevel = "NotCharacter";

        LoadScenes();
	}
	

	void Update () {
        GameTimer += Time.deltaTime;
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
