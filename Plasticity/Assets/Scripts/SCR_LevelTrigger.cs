using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[System.Serializable]
public struct DataPair
{
    public string Tag;
    public int IndexToWrite;
    public string Value;
};

public class SCR_LevelTrigger : MonoBehaviour {

    private enum TypeOfTrigger {Loader, StateChange, CharacterTransform};
    [SerializeField]
    private TypeOfTrigger ThisTrigger;

    [SerializeField]
    [ShowIf("ThisTrigger", TypeOfTrigger.Loader)]
    private string TriggerName;
    [SerializeField]
    [ShowIf("ThisTrigger", TypeOfTrigger.Loader)]
    private bool ShouldFreezeBoxOnEnter;
    [SerializeField]
    [ShowIf("ThisTrigger", TypeOfTrigger.StateChange)]
    private int PuzzleNumber;
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
                SCR_EventManager.TriggerEvent("LevelTrigger", TriggerName);
                if (ShouldFreezeBoxOnEnter && other.gameObject.GetComponent<SCR_DragDrop>())
                {
                    other.gameObject.GetComponentInChildren<SCR_DragDrop>().Lockout();
                }
            }
        }
        else if(ThisTrigger == TypeOfTrigger.StateChange)
        {
            if (IsIn(other.tag, TagsOfTriggeringObject))
            {
                SCR_EventManager.TriggerEvent("SceneStateTrigger", PuzzleNumber);
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
                if(other.tag == StatesToChange[i].Tag)
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
