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
    private float LerpSpeed;
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
            if (!SideGrabbingScript.IsLerping() && !SideGrabbingScript.IsInside() && Up && CharacterManager.InteractingWith == null)
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

    IEnumerator LerpVector(Vector3 From, Vector3 To)
    {
        float TimeModifier = 0.0f;
        while (TimeModifier < 1.0f)
        {
            TimeModifier += Time.deltaTime * LerpSpeed;
            Character.transform.position = Vector3.Lerp(From, To, TimeModifier);
            yield return true;
        }
        EndLerp();
    }

    private void BeginLerp()
    {
        if (TriggerOnLerping) SCR_EventManager.TriggerEvent("LevelTrigger", LerpingTriggerName);
        CharacterManager.InteractingWith = gameObject;
        CharacterManager.FreezeVelocity();
        StartCoroutine(LerpVector(Character.transform.position, GoalPoint));
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
        CharacterManager.InteractingWith = null;
        CharacterManager.UnfreezeVelocity();
    }
}
