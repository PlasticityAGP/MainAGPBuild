using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[System.Serializable]
public struct DataPair
{
    public string Name;
    public int IndexToWrite;
    public string Value;
};

[System.Serializable]
public struct TriggerPair
{
    public string TriggerName;
    public string TriggeringObjectName;
    public int PuzzleNumber;
};

public class SCR_LevelTrigger : MonoBehaviour {

    private enum TypeOfTrigger {Loader, StateChange, CharacterTransform};
    [SerializeField]
    private TypeOfTrigger ThisTrigger;
    [SerializeField]
    [HideIf("ThisTrigger", TypeOfTrigger.CharacterTransform)]
    private TriggerPair[] TriggerPairs;
    [SerializeField]
    [ShowIf("ThisTrigger", TypeOfTrigger.Loader)]
    private bool ShouldFreezeBoxOnEnter;
    [SerializeField]
    bool WriteToStateData;
    [SerializeField]
    [ShowIf("WriteToStateData")]
    private SCR_LevelStates LevelData;
    [SerializeField]
    [ShowIf("WriteToStateData")]
    private DataPair[] StatesToChange;
    [SerializeField]
    [ShowIf("ThisTrigger", TypeOfTrigger.CharacterTransform)]
    private bool TransformX;
    [SerializeField]
    [ShowIf("ThisTrigger", TypeOfTrigger.CharacterTransform)]
    private bool TransformY;
    [SerializeField]
    [ShowIf("ThisTrigger", TypeOfTrigger.CharacterTransform)]
    private bool TransformZ;
    [SerializeField]
    [ShowIf("TransformX")]
    private float XPosition;
    [SerializeField]
    [ShowIf("TransformY")]
    private float YPosition;
    [SerializeField]
    [ShowIf("TransformZ")]
    private float ZPosition;
    [SerializeField]
    [ShowIf("TransformX")]
    private float XLerpSpeed;
    [SerializeField]
    [ShowIf("TransformY")]
    private float YLerpSpeed;
    [SerializeField]
    [ShowIf("TransformZ")]
    private float ZLerpSpeed;
    private bool LerpBool;
    [SerializeField]
    [Tooltip("Specifies whether or not we want to fire an event when the player begins changing plane")]
    private bool TriggerOnLerping;
    [SerializeField]
    [ShowIf("TriggerOnLerping")]
    [Tooltip("This is the ID of the setting in the SceneLoader that we would like to load")]
    private string LerpingTriggerName;
    SCR_CharacterManager Manager;
	
	// Update is called once per frame
	void Update () {
        if (LerpBool) DoLerp(Time.deltaTime);
    }

    private int IsIn(string GameObjectName, TriggerPair[] Array)
    {
        for (int i = 0; i < Array.Length; ++i)
        {
            if (Array[i].TriggeringObjectName == GameObjectName) return i;
        }
        return -1;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(ThisTrigger == TypeOfTrigger.Loader)
        {
            int Index = IsIn(other.gameObject.name, TriggerPairs);
            if (Index != -1)
            {
                SCR_EventManager.TriggerEvent("LevelTrigger", TriggerPairs[Index].TriggerName);
                if (ShouldFreezeBoxOnEnter && other.gameObject.GetComponentInChildren<SCR_DragDrop>())
                {
                    other.gameObject.GetComponentInChildren<SCR_DragDrop>().Lockout();
                }
            }
        }
        else if(ThisTrigger == TypeOfTrigger.StateChange)
        {
            int Index = IsIn(other.gameObject.name, TriggerPairs);
            if (Index != -1)
            {
                SCR_EventManager.TriggerEvent("SceneStateTrigger", TriggerPairs[Index].PuzzleNumber);
            }
        }
        else if(ThisTrigger == TypeOfTrigger.CharacterTransform)
        {
            if (other.tag == "Character")
            {
                Manager = other.GetComponentInChildren<SCR_CharacterManager>();
                LerpBool = true;
            }
        }

        if (WriteToStateData)
        {
            for (int i = 0; i < StatesToChange.Length; ++i)
            {
                if(other.gameObject.name == StatesToChange[i].Name)
                    LevelData.PuzzleStates[StatesToChange[i].IndexToWrite] = StatesToChange[i].Value;
            }
        }
    }

    private void DoLerp(float DeltaTime)
    {
        float MagX = XPosition - Manager.gameObject.transform.position.x;
        float MagY = YPosition - Manager.gameObject.transform.position.y;
        float MagZ = ZPosition - Manager.gameObject.transform.position.z;
        bool DoneX = false;
        bool DoneY = false;
        bool DoneZ = false;
        if (Mathf.Abs(MagX) < 0.05f) DoneX = true;
        else
        {
            if (TransformX)
            {
                float Dir = MagX / Mathf.Abs(MagX);
                Vector3 NewPos = Manager.gameObject.transform.position;
                NewPos.x += Dir * XLerpSpeed * DeltaTime;
                Manager.gameObject.transform.position = NewPos;
            }
            else DoneX = true;
        }
        if (Mathf.Abs(MagY) < 0.05f) DoneY = true;
        else
        {
            if (TransformY)
            {
                float Dir = MagY / Mathf.Abs(MagY);
                Vector3 NewPos = Manager.gameObject.transform.position;
                NewPos.y += Dir * YLerpSpeed * DeltaTime;
                Manager.gameObject.transform.position = NewPos;
            }
            else DoneY = true;
        }
        if (Mathf.Abs(MagZ) < 0.05f) DoneZ = true;
        else
        {
            if (TransformZ)
            {
                float Dir = MagZ / Mathf.Abs(MagZ);
                Vector3 NewPos = Manager.gameObject.transform.position;
                NewPos.z += Dir * ZLerpSpeed * DeltaTime;
                Manager.gameObject.transform.position = NewPos;
            }
            else DoneZ = true;
        }
        if (DoneX && DoneY && DoneZ)
        {
            LerpBool = false;
            if (TriggerOnLerping) SCR_EventManager.TriggerEvent("LevelTrigger", LerpingTriggerName);
        }
    }
}
