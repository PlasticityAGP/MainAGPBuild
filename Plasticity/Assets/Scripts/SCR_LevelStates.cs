using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "LevelStateData", menuName = "Level Data Storage", order = 1)]
public class SCR_LevelStates : ScriptableObject {

    [Tooltip("A list of scenes we want to load in an additive and asynchronous manner to the current scene")]
    public string[] LevelArray;
}
