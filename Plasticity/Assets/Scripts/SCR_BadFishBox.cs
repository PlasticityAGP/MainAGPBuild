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
    private AnimationCurve[] IKCurves;
    [SerializeField]
    private GameObject[] IKTargets;
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
    [SerializeField]
    bool WriteToStateData;
    [SerializeField]
    [ShowIf("WriteToStateData")]
    private SCR_LevelStates LevelData;
    [SerializeField]
    [ShowIf("WriteToStateData")]
    private DataPair[] StatesToChange;
    private SCR_IKToolset IKTools;
    private bool IKBlocker = false;

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
            IKTools = Character.GetComponentInChildren<SCR_IKToolset>();
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
            if ((CharacterManager.InteractingWith == null || CharacterManager.InteractingWith == gameObject) && !Done)
            {
                CharacterManager.InteractingWith = gameObject;
                IKTools.SetEffectorTarget("LeftHand", IKTargets[CurrentIndex]);
                IKTools.SetEffectorTarget("RightHand", IKTargets[CurrentIndex]);
                IKTools.StartEffectorLerp("LeftHand", IKCurves[0], 0.5f);
                IKTools.StartEffectorLerp("RightHand", IKCurves[0], 0.5f);
                IKBlocker = true;
                OriginalSpeed = CharacterManager.GetSpeed();
                CharacterManager.SetSpeed(SlowdownSpeed);
                CharacterManager.StartPushing();
                DoIfBothPressed();
            }
        }
        else if (!Done)
        {
            Interact = false;
            CharacterManager.SetSpeed(OriginalSpeed);
            CharacterManager.StopPushing();
            StopAllCoroutines();
            IKTools.StartEffectorLerp("LeftHand", IKCurves[1], 0.5f);
            IKTools.StartEffectorLerp("RightHand", IKCurves[1], 0.5f);
            IKBlocker = false;
            CharacterManager.InteractingWith = null;
            Timer(0.5f, NullHands);
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
        if (!Done)
        {
            BoxAnimator.SetTrigger("Interrupted");
        }
    }

    // Use this for initialization
    void Start () {
        BoxAnimator = gameObject.GetComponent<Animator>();
	}

    private void NullHands()
    {
        if (!IKBlocker)
        {
            IKTools.ForceEffectorWeight("LeftHand", 0.0f);
            IKTools.ForceEffectorWeight("RightHand", 0.0f);
            IKTools.SetEffectorTarget("LeftHand", null);
            IKTools.SetEffectorTarget("RightHand", null);
        }
    }

    IEnumerator InterruptableTimer()
    {
        float ThisTimer = Timers[CurrentIndex];
        float DeltaT = 0.0f;
        bool DoIK = true;
        while (DeltaT < ThisTimer)
        {
            if(DeltaT >= ThisTimer - 0.3f && DoIK)
            {
                DoIK = false;
                IKTools.StartEffectorLerp("LeftHand", IKCurves[1], 0.5f);
                IKTools.StartEffectorLerp("RightHand", IKCurves[1], 0.5f);
                IKBlocker = false;
                CharacterManager.InteractingWith = null;
                Timer(0.5f, NullHands);
            } 
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
        if (WriteToStateData)
        {
            for (int i = 0; i < StatesToChange.Length; ++i)
            {
                    LevelData.PuzzleStates[StatesToChange[i].IndexToWrite] = StatesToChange[i].Value;
            }
        }
        CharacterManager.SetSpeed(OriginalSpeed);
        CharacterManager.StopPushing();
    }
}
