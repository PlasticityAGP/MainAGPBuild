using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelStateData", menuName = "Level Data Storage", order = 1)]
public class SCR_LevelStates : ScriptableObject {

    [Tooltip("A list of scenes we want to load in an additive and asynchronous manner to the current scene")]
    public string[] LevelArray;
    [Tooltip("A string name of the current level")]
    public string CurrentLevel;
    [Tooltip("The current state of the 0th puzzle")]
    public int StatePuzzle0;
    [Tooltip("The current state of the 1st puzzle")]
    public int StatePuzzle1;
    [Tooltip("The current state of the 2nd puzzle")]
    public int StatePuzzle2;
    [Tooltip("The current state of the 3rd puzzle")]
    public int StatePuzzle3;
    [Tooltip("The current state of the 4th puzzle")]
    public int StatePuzzle4;
    [Tooltip("The current state of the 5th puzzle")]
    public int StatePuzzle5;

}
