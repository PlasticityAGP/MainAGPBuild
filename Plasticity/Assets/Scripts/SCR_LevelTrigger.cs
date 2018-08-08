using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[System.Serializable]
public struct DataPair
{
    public string Tag;
    public int IndexToWrite;
    public int Value;
};

public class SCR_LevelTrigger : MonoBehaviour {

    private enum TypeOfTrigger {Loader, StateChange, CharacterTransform};
    [SerializeField]
    private TypeOfTrigger ThisTrigger;

    [SerializeField]
    [HideIf("ThisTrigger", TypeOfTrigger.CharacterTransform)]
    private int TriggerID;
    [SerializeField]
    [HideIf("ThisTrigger", TypeOfTrigger.CharacterTransform)]
    private string[] TagsOfTriggeringObject;
    [SerializeField]
    bool WriteToStateData;
    [SerializeField]
    [ShowIf("WriteToStateData")]
    private SCR_LevelStates LevelData;
    [SerializeField]
    [ShowIf("WriteToStateData")]
    private DataPair[] StatesToChange;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private bool IsIn(string Item, string[] Array)
    {
        for (int i = 0; i < Array.Length; ++i)
        {
            if (Array[i] == Item) return true;
        }
        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(ThisTrigger == TypeOfTrigger.Loader)
        {
            if (IsIn(other.tag, TagsOfTriggeringObject))
            {
                SCR_EventManager.TriggerEvent("LevelTrigger", TriggerID);
            }
        }
        else if(ThisTrigger == TypeOfTrigger.StateChange)
        {
            if (IsIn(other.tag, TagsOfTriggeringObject))
            {
                SCR_EventManager.TriggerEvent("SceneStateTrigger", TriggerID);
            }
        }

        if (WriteToStateData)
        {
            for (int i = 0; i < StatesToChange.Length; ++i)
            {
                if(other.tag == StatesToChange[i].Tag)
                    LevelData.PuzzleStates[StatesToChange[i].IndexToWrite] = StatesToChange[i].Value;
            }
        }

    }
}
