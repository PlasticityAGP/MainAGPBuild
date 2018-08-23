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
    private UnityAction<string> TimelineListener;
    private PlayableDirector ThisDirector;

    private void Awake()
    {
        //Register the callback functions related to each listener. These will be called as
        //the events these listeners are listening to get invoked 
        TimelineListener = new UnityAction<string>(PlayTimeline);

    }

    private void OnEnable()
    {
        //Register listeners with their events in the EventManager
        SCR_EventManager.StartListening("Timeline", TimelineListener);

    }

    private void OnDisable()
    {
        //Tell the EventManager we are no longer listening as the CharacterManager gets disabled.
        SCR_EventManager.StopListening("Timeline", TimelineListener);
    }

    private void PlayTimeline(string AssetToPlay)
    {
        for (int i = 0; i < TimelineAssets.Length; ++i)
        {
            if (AssetToPlay == TimelineAssets[i].Tag) ThisDirector.Play(TimelineAssets[i].TimelineAsset);
        }
    }

    // Use this for initialization
    void Start () {
        ThisDirector = gameObject.GetComponent<PlayableDirector>();
	}
}
