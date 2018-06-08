using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class SCR_SeeSaw : MonoBehaviour
{
    [SerializeField]
    [Tooltip("This float impacts how much the SeeSaw reacts to the player's weight. The higher the number, the quicker it changes pitch")]
    [ValidateInput("GreaterThanOrEqualZero", "The EffectOfWeightOnSeeSaw cannot be a negative number!")]
    private float EffectOfWeightOnSeeSaw = 1.0f;
    //Reference to the hinge joint on our parent game object
    private HingeJoint SeeSawJoint;
    //Reference to our parent's rigidbody so that we can add torque to the seesaw
    private Rigidbody ParentRbody;
    //Predefined axis along which the seesaw will rotate
    private Vector3 ZVec = new Vector3(0.0f, 0.0f, 1.0f);

    private bool GreaterThanOrEqualZero(float input)
    {
        return input >= 0.0f;
    }

    // Use this for initialization
    void Start()
    {
        //Check to see if parent has rigidbody
        if (gameObject.transform.parent.GetComponent<Rigidbody>()) ParentRbody = gameObject.transform.parent.GetComponent<Rigidbody>();
        else Debug.LogError("The SeeSaw object is either missing a Rigidbody, or the Rigidbody is not located on the Surface Object");
        //Check to see if parent has hinge joint
        if (gameObject.transform.parent.GetComponent<HingeJoint>())
        {
            //Some hard coded defaultls in order to prevent people from accidentally breaking the prefab by messing with fields in hinge joint in\
            //the inspector. Essentially we always want to rotate around z and we always want to rotate about our center point
            SeeSawJoint = gameObject.transform.parent.GetComponent<HingeJoint>();
            SeeSawJoint.anchor = gameObject.transform.parent.transform.localPosition;
            SeeSawJoint.axis = ZVec;
        }
        else Debug.LogError("There is not a hinge joint attached to the SeeSaw game object");
    }

    //While we are overlapped
    private void OnTriggerStay(Collider other)
    {
        //Check to see if we are overlapped by the character
        if(other.gameObject.tag == "Character")
        {
            //Get distance from character to the point we rotate about
            Vector3 Distance = (other.gameObject.transform.position - gameObject.transform.parent.position);
            //Add torque based on that distance and the float that is set in the inspector 
            if (Distance.x > 0.0f)
            {
                ParentRbody.AddTorque(ZVec * -(Distance.magnitude * EffectOfWeightOnSeeSaw));
            }
            else
            {
                ParentRbody.AddTorque(ZVec * (Distance.magnitude * EffectOfWeightOnSeeSaw));
            }
        }
    }
}
