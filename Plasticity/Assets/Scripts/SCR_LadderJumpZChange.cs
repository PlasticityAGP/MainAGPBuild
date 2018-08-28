using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;

public class SCR_LadderJumpZChange : MonoBehaviour {

    [SerializeField]
    private GameObject RightReferencePoint;
    [SerializeField]
    private GameObject LeftReferencePoint;
    [SerializeField]
    private GameObject SideGrabbingTrigger;
    [SerializeField]
    private float SpeedOfLerp;
    [SerializeField]
    [Tooltip("Specifies whether or not we want to fire an event when the player begins changing plane")]
    private bool TriggerOnLerping;
    [SerializeField]
    [ShowIf("TriggerOnLerping")]
    [Tooltip("This is the ID of the setting in the SceneLoader that we would like to load")]
    private string LerpingTriggerName;
    private SCR_TiltLadder SideGrabbingScript;
    private GameObject Ladder;
    private GameObject Character;
    private SCR_CharacterManager CharacterManager;
    private UnityAction<int> UpListener;
    private bool Up;
    private Vector3 GoalPoint;
    private bool Lerping = false;

    private void Awake()
    {
        UpListener = new UnityAction<int>(UpPressed);
    }


    private void OnEnable()
    {
        SCR_EventManager.StartListening("UpKey", UpListener);
    }

    private void OnDisable()
    {
        SCR_EventManager.StopListening("UpKey", UpListener);
    }

    private void UpPressed(int value)
    {
        if (value == 1) Up = true;
        else Up = false;
    }

    // Use this for initialization
    void Start () {
        Ladder = gameObject.transform.parent.gameObject;
        SideGrabbingScript = SideGrabbingTrigger.GetComponent<SCR_TiltLadder>();
	}

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Character")
        {
            Character = other.gameObject;
            CharacterManager = Character.GetComponent<SCR_CharacterManager>();
            if (!SideGrabbingScript.IsLerping() && !SideGrabbingScript.IsInside() && Up)
            {
                if (DetermineSide(other) && CharacterManager.MoveDir && LadderLean())
                {
                    CalculateGoal(true);
                    BeginLerp();
                }
                else if (!DetermineSide(other) && !CharacterManager.MoveDir && !LadderLean())
                {
                    CalculateGoal(false);
                    BeginLerp();
                }
            }
        }
    }

    private void CalculateGoal(bool side)
    {
        if (side)
        {
            float t = (Character.transform.position.y - LeftReferencePoint.transform.position.y) / gameObject.transform.up.normalized.y;
            GoalPoint = LeftReferencePoint.transform.position + (gameObject.transform.up.normalized * t);
        }
        else
        {
            float t = (Character.transform.position.y - RightReferencePoint.transform.position.y) / gameObject.transform.up.normalized.y;
            GoalPoint = RightReferencePoint.transform.position + (gameObject.transform.up.normalized * t);
        }
    }

    private bool LadderLean()
    {
        float ZValue = Vector3.Cross(Vector3.up, gameObject.transform.up).z;
        if (ZValue < 0.0f) return true;
        else return false;
    }

    private bool DetermineSide(Collider other)
    {
        Vector3 u = Ladder.transform.up.normalized;
        Vector3 v = (other.transform.position - gameObject.transform.position).normalized;
        float ZValue = Vector3.Cross(u,v).z;
        if (ZValue > 0.0f) return true;
        else return false;
    }

    private void FixedUpdate()
    {
        if (Lerping) DoLerp(Time.deltaTime);
    }

    private void BeginLerp()
    {
        if (TriggerOnLerping) SCR_EventManager.TriggerEvent("LevelTrigger", LerpingTriggerName);
        CharacterManager.FreezeVelocity();
        Lerping = true;
    }

    private void EndLerp()
    {
        if (Up)
        {
            SCR_EventManager.TriggerEvent("UpKey", 1);
            Up = false;
        }
        else SCR_EventManager.TriggerEvent("UpKey", 1);
        if (Up) SCR_EventManager.TriggerEvent("UpKey", 0);
        CharacterManager.UnfreezeVelocity();
        Lerping = false;
    }

    private void DoLerp(float DeltaTime)
    {
        Vector3 LerpVec = Character.transform.position;
        bool X, Y, Z;
        if (Mathf.Abs(Character.transform.position.x - GoalPoint.x) > 0.1f)
        {
            if((GoalPoint.normalized.x - Character.transform.position.normalized.x) > 0.0f)
                LerpVec.x +=  DeltaTime * SpeedOfLerp;
            else
                LerpVec.x -= DeltaTime * SpeedOfLerp;
            X = false;
        }
        else X = true;
        if (Mathf.Abs(Character.transform.position.y - GoalPoint.y) > 0.1f)
        {
            if((GoalPoint.normalized.y - Character.transform.position.normalized.y) > 0.0f)
                LerpVec.y += DeltaTime * SpeedOfLerp;
            else
                LerpVec.y -= DeltaTime * SpeedOfLerp;
            Y = false;
        }
        else Y = true;
        if (Mathf.Abs(Character.transform.position.z - GoalPoint.z) > 0.1f)
        {
            if((GoalPoint.normalized.z - Character.transform.position.normalized.z) > 0.0f)
                LerpVec.z +=  DeltaTime * SpeedOfLerp;
            else
                LerpVec.z -= DeltaTime * SpeedOfLerp;
            Z = false;
        }
        else Z = true;
        Character.transform.position = LerpVec;
        if (X && Y && Z) EndLerp();
    }
}
