
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class SCR_ZJump : MonoBehaviour {

    private UnityAction<int> UpListener;

    [SerializeField]
    [Tooltip("A reference point whose z value will be used to move the player in the z axis")]
    private bool ZTransformMethod;
    [SerializeField]
    [Tooltip("A reference point whose z value will be used to move the player in the z axis")]
    private GameObject ReferencePoint;
    private Vector3 NewPosition;
    private float LerpValue;
    private float ClamberLerpValue;
    private Vector3 InitialPosition;
    private bool LerpBack = false;
    private bool Clamber = false;
    private bool LerpingY = true;
    private SCR_ZDrag OtherTriggerScript;
    private GameObject Character;
    private SCR_CharacterManager CharacterManager;

    private void Awake()
    {
        //Register the callback functions related to each listener. These will be called as
        //the events these listeners are listening to get invoked 
        UpListener = new UnityAction<int>(UpPressed);
    }
    private void OnEnable()
    {
        //Register listeners with their events in the EventManager
        SCR_EventManager.StartListening("UpKey", UpListener);
    }

    private void UpPressed(int value)
    {
        //The value passed by the event indicates whether or not the key is pressed down. 
        if (!ZTransformMethod)
        {
            if (value == 1)
            {
                if (OtherTriggerScript.IsInside)
                {
                    Clamber = true;
                }
            }
            else
            {

            }
        }

    }



    // Use this for initialization
    void Start () {
        if (gameObject.transform.parent.GetComponentInChildren<SCR_ZDrag>())
            OtherTriggerScript = gameObject.transform.parent.GetComponentInChildren<SCR_ZDrag>();
        else Debug.LogError("There needs to be a SCR_ZDRAG script attached to one of the child game objects in the dragable object prefab");
        ClamberLerpValue = 0.0f;
        LerpValue = 0.0f;
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Character")
        {
            Character = other.gameObject;
            CharacterManager = Character.GetComponent<SCR_CharacterManager>();
            InitialPosition = Character.transform.position;
            LerpValue = 0.0f;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject.tag == "Character" && ZTransformMethod)
        {
            Vector3 A = other.transform.position - gameObject.transform.position;
            Vector3 B = ReferencePoint.transform.position - gameObject.transform.position;
            if(Vector3.Dot(A, B.normalized) > B.magnitude)
            {
                LerpValue += (Time.deltaTime * 2.0f);
                if (LerpValue >= 1.0f) LerpValue = 1.0f;
                float ZComp = Mathf.Lerp(Character.transform.position.z, ReferencePoint.transform.position.z, LerpValue);
                Vector3 FinalPosition = new Vector3(Character.transform.position.x, Character.transform.position.y, ZComp);
                Character.transform.position = FinalPosition;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Character")
        {
            LerpBack = true;
            LerpValue = 0.0f;
            Character = other.gameObject;
            CharacterManager = Character.GetComponent<SCR_CharacterManager>();
        }
    }

    private void ReturnLerp(float DeltaTime)
    {
        LerpValue += (DeltaTime * 2.0f);
        if (LerpValue >= 1.0f)
        {
            LerpBack = false;
            LerpValue = 0.0f;
        }
        else
        {
            float ZComp = Mathf.Lerp(Character.transform.position.z, InitialPosition.z, LerpValue);
            Vector3 FinalPosition = new Vector3(Character.transform.position.x, Character.transform.position.y, ZComp);
            Character.transform.position = FinalPosition; 
        }
        
    }

    private void ClamberLerp(float DeltaTime)
    {
        if (ClamberLerpValue == 0.0f) CharacterManager.FreezeVelocity();

        ClamberLerpValue += (DeltaTime * 4.0f);

        if (ClamberLerpValue >= 1.0f)
        {
            if(Mathf.Abs(Character.transform.position.z - ReferencePoint.transform.position.z) < 0.01f)
            {
                Clamber = false;
                CharacterManager.UnfreezeVelocity();
                LerpingY = true;
                ClamberLerpValue = 0.0f;
            }
            else
            {
                ClamberLerpValue = 0.0f;
                LerpingY = false;
            }
        }
        else
        {
            if (LerpingY)
            {
                float YComp = Mathf.Lerp(Character.transform.position.y, ReferencePoint.transform.position.y, ClamberLerpValue);
                Vector3 FinalPosition = new Vector3(Character.transform.position.x, YComp, Character.transform.position.z);
                Character.transform.position = FinalPosition;
            }
            else
            {
                float ZComp = Mathf.Lerp(Character.transform.position.z, ReferencePoint.transform.position.z, ClamberLerpValue);
                Vector3 FinalPosition = new Vector3(Character.transform.position.x, Character.transform.position.y, ZComp);
                Character.transform.position = FinalPosition;
            }
        }
    }

    // Update is called once per frame
    void Update () {
        if (LerpBack) ReturnLerp(Time.deltaTime);
        if (!ZTransformMethod && Clamber) ClamberLerp(Time.deltaTime);

        
	}
}
