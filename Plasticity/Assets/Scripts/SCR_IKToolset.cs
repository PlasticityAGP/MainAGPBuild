using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;

//The purpose of this script is to provide tools for lerping effector weights in FinalIK. Currently only supports left and right hand effectors

struct QueueObject
{
    public float duration;
    public AnimationCurve curve;
}

public class SCR_IKToolset : MonoBehaviour {

    //Reference to the IK component we have attached to our character model
    private FullBodyBipedIK Ik;
    private Coroutine LatestLeftHand;
    private Coroutine LatestRightHand;

    // Use this for initialization
    void Start () {
        //Find the instance of the IK component attached to our character
        if (gameObject.GetComponentInChildren<FullBodyBipedIK>()) Ik = gameObject.GetComponentInChildren<FullBodyBipedIK>();
        else Debug.LogError("We need a a FullBodyBipedIK component attached to one of the Character's child Game Objects");
    }

    /// <summary>
    /// Sets a specific effector position to a fixed location in world space
    /// </summary>
    /// <param name="ID">What effector is having its location set</param>
    /// <param name="Loc">What location you would like to set the effector to</param>
    public void SetEffectorLocation(string ID, Vector3 Loc)
    {
        if (ID.Equals("LeftHand") || ID.Equals("lefthand") || ID.Equals("Left Hand") || ID.Equals("left hand"))
        {
            Ik.solver.leftHandEffector.position = Loc;
        }

        else if (ID.Equals("RightHand") || ID.Equals("righthand") || ID.Equals("Right Hand") || ID.Equals("right hand"))
        {
            Ik.solver.rightHandEffector.position = Loc;
        }
        else Debug.LogError("We could not find the specified effector!");
    }

    /// <summary>
    /// Tells a specific effector update its location to that of a tracked GameObject
    /// </summary>
    /// <param name="ID">Which effector you would like to have target a GameObject</param>
    /// <param name="obj">The GameObject you would like the effector to follow</param>
    public void SetEffectorTarget(string ID, GameObject obj)
    {
        if (ID.Equals("LeftHand") || ID.Equals("lefthand") || ID.Equals("Left Hand") || ID.Equals("left hand"))
        {
            if (obj == null) Ik.solver.leftHandEffector.target = null;
            else Ik.solver.leftHandEffector.target = obj.transform;
        }

        else if (ID.Equals("RightHand") || ID.Equals("righthand") || ID.Equals("Right Hand") || ID.Equals("right hand"))
        {
            if (obj == null) Ik.solver.rightHandEffector.target = null;
            else Ik.solver.rightHandEffector.target = obj.transform;
        }
        else Debug.LogError("We could not find the specified effector!");
    }

    /// <summary>
    /// Forces a specific effector to snap to a specific IK weight instantly instead of interpolating along an animation curve
    /// </summary>
    /// <param name="ID">The ID of the effector who's weight you need to force</param>
    /// <param name="weight">The new weight of the effector</param>
    public void ForceEffectorWeight(string ID, float weight)
    {
        if (ID.Equals("LeftHand") || ID.Equals("lefthand") || ID.Equals("Left Hand") || ID.Equals("left hand"))
        {
            if(LatestLeftHand != null) StopCoroutine(LatestLeftHand);
            Ik.solver.leftHandEffector.positionWeight = weight;
            Ik.solver.leftHandEffector.rotationWeight = weight;
        }

        else if (ID.Equals("RightHand") || ID.Equals("righthand") || ID.Equals("Right Hand") || ID.Equals("right hand"))
        {
            if(LatestRightHand != null) StopCoroutine(LatestRightHand);
            Ik.solver.rightHandEffector.positionWeight = weight;
            Ik.solver.rightHandEffector.rotationWeight = weight;
        }
        else Debug.LogError("We could not find the specified effector!");
    }

    /// <summary>
    /// Initiate the interpolation of the weight of a specific effector along an animation curve
    /// </summary>
    /// <param name="ID">The effector who's weight will be changed</param>
    /// <param name="curve">The animation curve along which the effector's weight will be interpolated</param>
    /// <param name="duration">How long the interpolation will take</param>
    public void StartEffectorLerp(string ID, AnimationCurve curve, float duration)
    {
        QueueObject InsertObject = new QueueObject();
        InsertObject.duration = duration;
        InsertObject.curve = curve;
        if(ID == "LeftHand") LatestLeftHand = StartCoroutine(IKCoroutine(ID, InsertObject));
        else if (ID == "RightHand") LatestRightHand = StartCoroutine(IKCoroutine(ID, InsertObject));
    } 

    /// <summary>
    /// Returns the current effector weight for a given effector 
    /// </summary>
    /// <param name="ID">The effector you would like to get the weight of</param>
    /// <returns></returns>
    public float GetEffectorWeight(string ID)
    {
        if (ID.Equals("LeftHand") || ID.Equals("lefthand") || ID.Equals("Left Hand") || ID.Equals("left hand"))
            return Ik.solver.leftHandEffector.positionWeight;
        else if (ID.Equals("RightHand") || ID.Equals("righthand") || ID.Equals("Right Hand") || ID.Equals("right hand"))
            return Ik.solver.rightHandEffector.positionWeight;
        else
        {
            Debug.LogError("We could not find the specified effector!");
            return 0.0f;
        }
    }

    IEnumerator IKCoroutine(string Effector, QueueObject obj)
    {
        float IkTimer = 0.0f;
        while (IkTimer < obj.duration)
        {
            IkTimer += Time.deltaTime;
            if(Effector == "LeftHand")
            {
                Ik.solver.leftHandEffector.positionWeight = obj.curve.Evaluate(IkTimer);
                Ik.solver.leftHandEffector.rotationWeight = obj.curve.Evaluate(IkTimer);
            }
            else if(Effector == "RightHand")
            {
                Ik.solver.rightHandEffector.positionWeight = obj.curve.Evaluate(IkTimer);
                Ik.solver.rightHandEffector.rotationWeight = obj.curve.Evaluate(IkTimer);
            }
            else if(Effector == "LeftFoot")
            {
                Ik.solver.leftFootEffector.positionWeight = obj.curve.Evaluate(IkTimer);
                Ik.solver.leftFootEffector.rotationWeight = obj.curve.Evaluate(IkTimer);
            }
            else if(Effector == "RightFoot")
            {
                Ik.solver.rightFootEffector.positionWeight = obj.curve.Evaluate(IkTimer);
                Ik.solver.rightFootEffector.rotationWeight = obj.curve.Evaluate(IkTimer);
            }
            else Debug.LogError("We could not find the specified effector!");
            yield return null;
        }
    }
}
