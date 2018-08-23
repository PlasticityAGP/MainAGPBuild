using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

[System.Serializable]
struct TimelinePair
{
    public PlayableAsset TimelineAsset;
    public string Tag;
};

public class SCR_TimelineManager : MonoBehaviour {

    [SerializeField]
    private TimelinePair[] TimelineAssets;
    [SerializeField]
    private float MaxLength = 2.0f;
    private UnityAction<string> TimelineListener;
    private UnityAction<string> TimelineInstructionListener;
    private PlayableDirector ThisDirector;
    private float DirectorTime = 0.0f;

    private void Awake()
    {
        //Register the callback functions related to each listener. These will be called as
        //the events these listeners are listening to get invoked 
        TimelineListener = new UnityAction<string>(PlayTimeline);
        TimelineInstructionListener = new UnityAction<string>(DoInstructions);

    }

    IEnumerator TimelineControl(bool direction)
    {
        bool flag = true;
        while (flag)
        {
            if (direction)
            {
                DirectorTime += Time.deltaTime;
                DirectorTime = Mathf.Clamp(DirectorTime, 0.0f, MaxLength);
            }
            else
            {
                DirectorTime -= Time.deltaTime;
                DirectorTime = Mathf.Clamp(DirectorTime, 0.0f, MaxLength);
                ThisDirector.time = (double)DirectorTime;
            }
            if (DirectorTime == MaxLength || DirectorTime == 0.0f) flag = false;
            yield return null;
        }
        if (!direction) ThisDirector.Pause();
    }

    private void OnEnable()
    {
        //Register listeners with their events in the EventManager
        SCR_EventManager.StartListening("Timeline", TimelineListener);
        SCR_EventManager.StartListening("TimelineInstruction", TimelineInstructionListener);

    }

    private void OnDisable()
    {
        //Tell the EventManager we are no longer listening as the CharacterManager gets disabled.
        SCR_EventManager.StopListening("Timeline", TimelineListener);
        SCR_EventManager.StopListening("TimelineInstruction", TimelineInstructionListener);
    }

    private void DoInstructions(string Instruction)
    {
        if (Instruction == "Forward")
        {
            StopAllCoroutines();
            if (ThisDirector.state == PlayState.Paused) ThisDirector.Resume();
            StartCoroutine("TimelineControl", true);
        }
        else if (Instruction == "Rewind")
        {
            StopAllCoroutines();
            if (ThisDirector.state == PlayState.Paused) ThisDirector.Resume();
            StartCoroutine("TimelineControl", false);
        }
        else if (Instruction == "Pause")
        {
            ThisDirector.Pause();
        }
        else if (Instruction == "Resume")
        {
            ThisDirector.Resume();
        }
    }

    private void PlayTimeline(string AssetToPlay)
    {
        for (int i = 0; i < TimelineAssets.Length; ++i)
        {
            if (AssetToPlay == TimelineAssets[i].Tag)
            {
                ThisDirector.Play(TimelineAssets[i].TimelineAsset);
                ThisDirector.Pause();
            }
        }
    }

    // Use this for initialization
    void Start () {
        ThisDirector = gameObject.GetComponent<PlayableDirector>();
        ThisDirector.Pause();
	}
}
