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
    //Boolean that tells us when to start lerping the left hand effector weight
    private bool LerpLeftHand;
    //Boolean that tells us when to start lerping the right hand effector weight
    private bool LerpRightHand;
    //Queues for storing effector weight targets to iterate through multiple lerps in succession
    private QueueObject LeftHandQueue;
    float LeftHandTimer;
    private QueueObject RightHandQueue;
    float RightHandTimer;
    // Use this for initialization
    void Start () {
        //Find the instance of the IK component attached to our character
        if (gameObject.GetComponentInChildren<FullBodyBipedIK>()) Ik = gameObject.GetComponentInChildren<FullBodyBipedIK>();
        else Debug.LogError("We need a a FullBodyBipedIK component attached to one of the Character's child Game Objects");
        LerpLeftHand = false;
        LerpRightHand = false;
        LeftHandTimer = 0.0f;
        RightHandTimer = 0.0f;

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
            Ik.solver.leftHandEffector.positionWeight = weight;
            Ik.solver.leftHandEffector.rotationWeight = weight;
            LerpLeftHand = false;
        }

        else if (ID.Equals("RightHand") || ID.Equals("righthand") || ID.Equals("Right Hand") || ID.Equals("right hand"))
        {
            Ik.solver.rightHandEffector.positionWeight = weight;
            Ik.solver.rightHandEffector.rotationWeight = weight;
            LerpRightHand = false;
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
        if (ID.Equals("LeftHand") || ID.Equals("lefthand") || ID.Equals("Left Hand") || ID.Equals("left hand"))
        {
            LerpLeftHand = true;
            QueueObject InsertObject = new QueueObject();
            InsertObject.duration = duration;
            InsertObject.curve = curve;
            LeftHandQueue = InsertObject;
            LeftHandTimer = 0.0f;
        }
        else if (ID.Equals("RightHand") || ID.Equals("righthand") || ID.Equals("Right Hand") || ID.Equals("right hand"))
        {
            LerpRightHand = true;
            QueueObject InsertObject = new QueueObject();
            InsertObject.duration = duration;
            InsertObject.curve = curve;
            RightHandQueue = InsertObject;
            RightHandTimer = 0.0f;
        }
        else Debug.LogError("We could not find the specified effector!");
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

    //Execute the lerp between from and to
    private void LeftEffectorDoLerp(float DeltaTime, QueueObject obj)
    {
        LeftHandTimer += DeltaTime;
        if (LeftHandTimer >= obj.duration)
        {
            EndLeftEffectorLerp();
        }
        else
        {
            Ik.solver.leftHandEffector.positionWeight = obj.curve.Evaluate(LeftHandTimer);
            Ik.solver.leftHandEffector.rotationWeight = obj.curve.Evaluate(LeftHandTimer);
        }
    }

    //Execute the lerp between from and to
    private void RightEffectorDoLerp(float DeltaTime, QueueObject obj)
    {
        RightHandTimer += DeltaTime;
        if (RightHandTimer >= obj.duration)
        {
            EndRightEffectorLerp();
        }
        else
        {
            Ik.solver.rightHandEffector.positionWeight = obj.curve.Evaluate(RightHandTimer);
            Ik.solver.rightHandEffector.rotationWeight = obj.curve.Evaluate(RightHandTimer);
        }
    }

    private void EndLeftEffectorLerp()
    {
        //Make sure we aren't trying to dequeue from an empty queue and end lerping
        LerpLeftHand = false;
        LeftHandTimer = 0.0f;
    }

    private void EndRightEffectorLerp()
    {
        //Make sure we aren't trying to dequeue from an empty queue and end lerping
        LerpRightHand = false;
        RightHandTimer = 0.0f;
    }

    private void FixedUpdate()
    {
        if (LerpLeftHand) LeftEffectorDoLerp(Time.deltaTime, LeftHandQueue);
        if (LerpRightHand) RightEffectorDoLerp(Time.deltaTime, RightHandQueue);
    }

}
