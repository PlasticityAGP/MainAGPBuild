using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;



public class SCR_TimelineManager : MonoBehaviour {

    [SerializeField]
    private string TimelineObject;
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
        if(AssetToPlay == TimelineObject) ThisDirector.Play();
    }

    // Use this for initialization
    void Start () {
        ThisDirector = gameObject.GetComponent<PlayableDirector>();
	}
}
