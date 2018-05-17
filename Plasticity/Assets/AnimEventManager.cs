using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimEventManager : MonoBehaviour {

    [HideInInspector]
    public struct AnimEvent
    {
        public string AnimStateName;
        public float CrossFadeTime;
        public float TimeToExecution;
    }
    [HideInInspector]
    public Animator CharacterAnimator;
    [HideInInspector]
    public bool AnimLock = false;
    private AnimEvent[] AnimList;
    private int AnimListLength = 0;

    // Use this for initialization
    void Start () {
        AnimList = new AnimEvent[10];
	}
	
	// Update is called once per frame
	void Update () {
        ManageAnimations(Time.deltaTime);
	}

    [HideInInspector]
    public void NewAnimEvent(string Name, float CrossFade, float TimeBeforePlay)
    {
        AnimEvent Event = new AnimEvent();
        Event.AnimStateName = Name;
        Event.CrossFadeTime = CrossFade;
        Event.TimeToExecution = TimeBeforePlay;
        AnimList[AnimListLength] = Event;
        ++AnimListLength;
        AnimLock = false;
        
    }

    private void ManageAnimations(float DeltaTime)
    {
        if (AnimListLength >= 10) Debug.LogError("Our AnimList is too long!");
        if(!(AnimListLength == 0))
        {
            for (int i = 0; i < AnimListLength; ++i)
            {
                AnimList[i].TimeToExecution -= DeltaTime;
            }

            if (AnimList[0].TimeToExecution <= 0.0f)
            {
                if (CharacterAnimator == null) Debug.LogError("The reference to the animator required in AnimEventManager is null!");
                CharacterAnimator.CrossFade(AnimList[0].AnimStateName, AnimList[0].CrossFadeTime);
                --AnimListLength;
                ReorderArray();
            }
        }

    }

    private void ReorderArray()
    {
        for (int i = 0; i < AnimListLength; ++i)
        {
            AnimList[i] = AnimList[i + 1];
        }
    }
}
