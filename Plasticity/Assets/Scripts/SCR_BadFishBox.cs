using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;

public class SCR_BadFishBox : SCR_GameplayStatics {
    private UnityAction<int> LeftListener;
    private UnityAction<int> InteractListener;
    private bool Left;
    private bool Interact;
    [SerializeField]
    private float[] Timers;
    [SerializeField]
    private float SlowdownSpeed;
    private int CurrentIndex = 0;
    private bool Done = false;
    private GameObject Character;
    private SCR_CharacterManager CharacterManager;
    private float OriginalSpeed = -1.0f;
    private Animator BoxAnimator;
    [SerializeField]
    private bool FireEventAtFinish;
    [SerializeField]
    [ShowIf("FireEventAtFinish")]
    private string EventName;
    [SerializeField]
    [ShowIf("FireEventAtFinish")]
    private string EventValue;

    private void Awake()
    {
        LeftListener = new UnityAction<int>(LeftPressed);
        InteractListener = new UnityAction<int>(InteractPressed);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Character")
        {
            SCR_EventManager.StartListening("LeftKey", LeftListener);
            SCR_EventManager.StartListening("InteractKey", InteractListener);
            Character = other.gameObject;
            CharacterManager = Character.GetComponent<SCR_CharacterManager>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.tag == "Character")
        {
            SCR_EventManager.StopListening("LeftKey", LeftListener);
            SCR_EventManager.StopListening("InteractKey", InteractListener);
            Left = false;
            Interact = false;
            if(OriginalSpeed != -1.0f)
            {
                CharacterManager.SetSpeed(OriginalSpeed);
                CharacterManager.StopPushing();
            }
        }
    }


    private void LeftPressed(int value)
    {
        if (value == 1)
        {
            Left = true;
            DoIfBothPressed();
        }
        else
        {
            Left = false;
            DoIfReleased();
        }
    }

    private void InteractPressed(int value)
    {
        if (value == 1)
        {
            Interact = true;
            OriginalSpeed = CharacterManager.GetSpeed();
            CharacterManager.SetSpeed(SlowdownSpeed);
            CharacterManager.StartPushing();
            DoIfBothPressed();
        }
        else
        {
            Interact = false;
            CharacterManager.SetSpeed(OriginalSpeed);
            CharacterManager.StopPushing();
            DoIfReleased();
        }
    }

    private void DoIfBothPressed()
    {
        if(Left && Interact && !Done)
        {
            StartCoroutine(InterruptableTimer());
            BoxAnimator.ResetTrigger("Interrupted");
            switch (CurrentIndex)
            {
                case 0:
                    BoxAnimator.SetTrigger("FirstPull");
                    break;
                case 1:
                    BoxAnimator.SetTrigger("SecondPull");
                    break;
                case 2:
                    BoxAnimator.SetTrigger("ThirdPull");
                    break;
                default:
                    break;
            }
            
        }
    }

    private void DoIfReleased()
    {
        StopAllCoroutines();
        BoxAnimator.SetTrigger("Interrupted");
    }

    // Use this for initialization
    void Start () {
        BoxAnimator = gameObject.GetComponent<Animator>();
	}

    IEnumerator InterruptableTimer()
    {
        float ThisTimer = Timers[CurrentIndex];
        float DeltaT = 0.0f;
        while (DeltaT < ThisTimer)
        {
            DeltaT += Time.deltaTime;
            yield return null;
        }
        if (CurrentIndex + 1 < Timers.Length)
        {
            ++CurrentIndex;
        }
        else
        {
            Done = true;
            if(FireEventAtFinish) SCR_EventManager.TriggerEvent(EventName, EventValue);
        }
        CharacterManager.SetSpeed(OriginalSpeed);
        CharacterManager.StopPushing();
    }
}
