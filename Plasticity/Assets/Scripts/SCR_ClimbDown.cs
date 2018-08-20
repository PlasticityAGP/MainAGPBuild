using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;

public class SCR_ClimbDown : MonoBehaviour {

    [SerializeField]
    private GameObject ReferencePoint;
    [SerializeField]
    private float SpeedOfLerpHorizontal;
    [SerializeField]
    private float SpeedOfLerpVertical;
    [SerializeField]
    private bool FireEventOnPressDown;
    [SerializeField]
    [ShowIf("FireEventOnPressDown")]
    private string EventName; 
    private SCR_CharacterManager CharManager;
    private GameObject Character;
    private UnityAction<int> DownListener;
    private UnityAction<int> UpListener;
    private bool Lerping = false;
    private bool Inside;
    private bool DoneWithX = false;
    private float Interpolant = 0.0f;
    int Up;

    private void Awake()
    {
        DownListener = new UnityAction<int>(DownPressed);
        UpListener = new UnityAction<int>(UpPressed);
    }

    private void OnEnable()
    {
        SCR_EventManager.StartListening("DownKey", DownListener);
        SCR_EventManager.StartListening("UpKey", UpListener);
    }

    private void OnDisable()
    {
        SCR_EventManager.StopListening("DownKey", DownListener);
        SCR_EventManager.StopListening("UpKey", UpListener);
    }

    private void DownPressed(int value)
    {
        if (value == 1 && Inside) BeginLerp();
    }

    private void UpPressed(int value)
    {
        Up = value;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Character")
        {
            Character = other.gameObject;
            CharManager = Character.GetComponent<SCR_CharacterManager>();
            Inside = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Character") Inside = false;

    }

    private void BeginLerp()
    {
        Lerping = true;
        CharManager.FreezeVelocity(SCR_CharacterManager.CharacterStates.Idling);
        if (FireEventOnPressDown)
            SCR_EventManager.TriggerEvent("LevelTrigger", EventName);
    }

    private void LerpPlayer(float DeltaTime)
    {

        if (DoneWithX) Interpolant += DeltaTime * SpeedOfLerpVertical;
        else Interpolant += DeltaTime * SpeedOfLerpHorizontal;
        if(Interpolant >= 1.0f)
        {
            if (!DoneWithX)
            {
                DoneWithX = true;
                Interpolant = 0.0f;
            }
            else GrabLadder();
        }
        else
        {
            Vector3 Pos = Character.transform.position;
            if (!DoneWithX)
            {
                Pos.x = Mathf.Lerp(Character.transform.position.x, ReferencePoint.transform.position.x, Interpolant);
                Character.transform.position = Pos;
            }
            else
            {
                Pos.y = Mathf.Lerp(Character.transform.position.y, ReferencePoint.transform.position.y, Interpolant);
                Character.transform.position = Pos;
            }
        }
       

    }

    private void GrabLadder()
    {
        Lerping = false;
        DoneWithX = false;
        Interpolant = 0.0f;
        int temp = Up;
        SCR_EventManager.TriggerEvent("UpKey", 1);
        if(temp == 0) SCR_EventManager.TriggerEvent("UpKey", 0);
        CharManager.UnfreezeVelocity();
    }

    private void FixedUpdate()
    {
        if (Lerping) LerpPlayer(Time.deltaTime);
    }
}
