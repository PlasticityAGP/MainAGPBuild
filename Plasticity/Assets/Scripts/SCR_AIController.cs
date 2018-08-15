using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

public class SCR_AIController : MonoBehaviour {

    [SerializeField]
    private bool OnlyTriggerOnce;
    private UnityAction<int> InteractListener;
    private bool Inside;
    private PlayableDirector AIDirector;
    private bool CanIDo;

    private void Awake()
    {
        InteractListener = new UnityAction<int>(InteractPressed);
    }

    private void InteractPressed(int value)
    {
        if (value == 1 && Inside && CanIDo)
        {
            DoTimeline();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Character")
        {
            Inside = true;
            SCR_EventManager.StartListening("InteractKey", InteractListener);
        }    
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Character")
        {
            Inside = false;
            SCR_EventManager.StopListening("InteractKey", InteractListener);
        }
    }

    private void DoTimeline()
    {
        if (OnlyTriggerOnce)
        {
            Inside = false;
            CanIDo = false;
            gameObject.GetComponent<BoxCollider>().enabled = false;
            AIDirector.Play();
        }
        else
        {
            AIDirector.Play();
        }
        
    }

    // Use this for initialization
    void Start () {
        AIDirector = gameObject.GetComponentInChildren<PlayableDirector>();
        CanIDo = true;
	}
}
