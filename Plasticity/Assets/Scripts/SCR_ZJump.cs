
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;


public class SCR_ZJump : MonoBehaviour {

    //Listens for the up key to be pressed
    private UnityAction<int> UpListener;

    [SerializeField]
    [Tooltip("A boolean that switches between our two methods of z jumping. If true, if the character is above the box and in the trigger " +
        "thhey will automatically slide over. If flase, they can only get on the box by being beside it and clambering up")]
    private bool ZTransformMethod;
    [SerializeField]
    [Tooltip("A reference point whose z value will be used to move the player in the z axis")]
    [ValidateInput("IsNull", "There must be a reference point referenced in this script!")]
    private GameObject ReferencePoint;

    //Value that dictates the linear interpolation of z position
    private float LerpValue;

    //Value that dictates the linear interpolation of the clambering mechanic
    private float ClamberLerpValue;
    //Stores the position of the player before they jumped on the box.
    private Vector3 InitialPosition;
    //Checked in Update to see if we should lerp to original z position
    private bool LerpBack = false;
    //Checked in Update to see if we should be moving the character in a clamber action
    private bool Clamber = false;
    //Used in ClamberLerp to see if we can move in Z direction yet
    private bool LerpingY = true;
    //Reference to a script that countains information about a different trigger within this prefab. Used for clambering
    private SCR_ZDrag OtherTriggerScript;
    //Reference to character and it's manager
    private GameObject Character;
    private SCR_CharacterManager CharacterManager;

    private bool IsNull(GameObject thing)
    {
        try
        {
            return thing.scene.IsValid();
        }
        catch
        {
            return false;
        }
    }

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
        //If we want to do the clambering method, allow for a clamber if up is pressed and the player is in the other trigger.
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
    void Start ()
    {
        //Get a reference to the script that stores info about the other trigger
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
            //Set Character as a character enters our trigger
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
            //This is a little vector math that just checks if the player is higher than the reference point in the
            //direction of the normal of the surface they will be walking on
            Vector3 A = other.transform.position - gameObject.transform.position;
            Vector3 B = ReferencePoint.transform.position - gameObject.transform.position;
            if(Vector3.Dot(A, B.normalized) > B.magnitude)
            {
                //Linearly interpolate player position in Z axis
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
            //As the player exits the trigger they should return to their original Z plane via ReturnLerp
            LerpBack = true;
            LerpValue = 0.0f;
            Character = other.gameObject;
            CharacterManager = Character.GetComponent<SCR_CharacterManager>();
        }
    }

    //Lerps the player back to the z plane they originated in
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
        //If the player is clambering, we don't want the CharacterManager to update velocity
        if (ClamberLerpValue == 0.0f) CharacterManager.FreezeVelocity();

        ClamberLerpValue += (DeltaTime * 4.0f);

        if (ClamberLerpValue >= 1.0f)
        {
            if(Mathf.Abs(Character.transform.position.z - ReferencePoint.transform.position.z) < 0.01f)
            {
                //If the player is done clambering, dont allow more clambering and allow the player to have velocity again
                Clamber = false;
                CharacterManager.UnfreezeVelocity();
                LerpingY = true;
                ClamberLerpValue = 0.0f;
            }
            else
            {
                //This occurs when we have finished lerping in Y direction and must now lerp in Z
                ClamberLerpValue = 0.0f;
                LerpingY = false;
            }
        }
        else
        {
            //Lerp in Y direction to ReferencePoint's Y position
            if (LerpingY)
            {
                float YComp = Mathf.Lerp(Character.transform.position.y, ReferencePoint.transform.position.y, ClamberLerpValue);
                Vector3 FinalPosition = new Vector3(Character.transform.position.x, YComp, Character.transform.position.z);
                Character.transform.position = FinalPosition;
            }
            //Lerp in Z direction to ReferencePoint's Z position
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
