using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class SCR_AnimEventManager : MonoBehaviour {

    //AnimEvent stores all the data we need per animation in order to do a crossfade between animations.
    [HideInInspector]
    public struct AnimEvent
    {
        public string AnimStateName;
        public float CrossFadeTime;
        //Time to execution allows the programmer to include a pause or downtime between the playing of animations. 
        public float TimeToExecution;
    }
    //Reference to CharacterAnimator passed by CharacterManager
    [HideInInspector]
    public Animator CharacterAnimator;
    //This lock is used to prevent animations from being placed into the AnimList more often than necessay
    [HideInInspector]
    public bool AnimLock = false;
    //AnimList functions like a queue in that the next anim to be played will always be stored at the 0 index of the array.
    private AnimEvent[] AnimList;
    //Int to help us keep track of the current number of anims tracked in array.
    private int AnimListLength = 0;

    // Use this for initialization
    void Start () {
        AnimList = new AnimEvent[10];
	}
	
	// Update is called once per frame
	void Update () {
        ManageAnimations(Time.deltaTime);
	}

    private int GetAnimListLength()
    {
        return AnimListLength;
    }

    //NewAnimEvent will be called in the CharacterManager in order to add events to our AnimList.

    /// <summary>
    /// Queues an animation for play in our AnimEventManager
    /// </summary>
    /// <param name="Name">Name of the animation state we want to play</param>
    /// <param name="CrossFade">Over how long we should fade this animation into being played</param>
    /// <param name="TimeBeforePlay">The amount of downtime we want to have before playing this animation</param>
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

    //Manage animations gets called in update
    private void ManageAnimations(float DeltaTime)
    {
        if (AnimListLength >= 10) Debug.LogError("Our AnimList is too long!");
        if(!(AnimListLength == 0))
        {
            for (int i = 0; i < AnimListLength; ++i)
            {
                //Decrement time left to execution per item in list
                AnimList[i].TimeToExecution -= DeltaTime;
            }

            //If the time to execution has passed, execute the crossfade, remove animation event from the list and reorder the array.
            if (AnimList[0].TimeToExecution <= 0.0f)
            {
                if (CharacterAnimator == null) Debug.LogError("The reference to the animator required in AnimEventManager is null!");
                CharacterAnimator.CrossFade(AnimList[0].AnimStateName, AnimList[0].CrossFadeTime);
                --AnimListLength;
                ReorderArray();
            }
        }

    }

    //Puts the next animation to be played at the 0 index of the array.
    private void ReorderArray()
    {
        for (int i = 0; i < AnimListLength; ++i)
        {
            AnimList[i] = AnimList[i + 1];
        }
    }
}
