using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;

//The purpose of this script is to provide tools for lerping effector weights in FinalIK. Currently only supports left and right hand effectors

struct QueueObject
{
    public float from;
    public float to;
    public float timer;
    public AnimationCurve curve;
}

public class SCR_IKToolset : MonoBehaviour {

    //Reference to the IK component we have attached to our character model
    private FullBodyBipedIK Ik;
    //Boolean that tells us when to start lerping the left hand effector weight
    private bool LerpLeftHand;
    //Boolean that tells us when to start lerping the right hand effector weight
    private bool LerpRightHand;
    //Dictates how quickly the left hand lerp will occur
    [HideInInspector]
    public float LeftSpeed;
    //Dictates how quickly the right hand lerp will occur
    [HideInInspector]
    public float RightSpeed;
    //Queues for storing effector weight targets to iterate through multiple lerps in succession
    private Queue<QueueObject> LeftHandQueue;
    private Queue<QueueObject> RightHandQueue;
    // Use this for initialization
    void Start () {
        //Find the instance of the IK component attached to our character
        if (gameObject.GetComponentInChildren<FullBodyBipedIK>()) Ik = gameObject.GetComponentInChildren<FullBodyBipedIK>();
        else Debug.LogError("We need a a FullBodyBipedIK component attached to one of the Character's child Game Objects");
        LerpLeftHand = false;
        LerpRightHand = false;
        LeftSpeed = 4.0f;
        RightSpeed = 4.0f;
        LeftHandQueue = new Queue<QueueObject>();
        RightHandQueue = new Queue<QueueObject>();

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

    public void StartEffectorLerp(string ID, AnimationCurve curve, float from, float to, float speed)
    {
        if (ID.Equals("LeftHand") || ID.Equals("lefthand") || ID.Equals("Left Hand") || ID.Equals("left hand"))
        {
            LerpLeftHand = true;
            LeftSpeed = speed;
            Ik.solver.leftHandEffector.positionWeight = from;
            Ik.solver.leftHandEffector.rotationWeight = from;
            QueueObject InsertObject = new QueueObject();
            InsertObject.from = from;
            InsertObject.to = to;
            InsertObject.timer = 0.0f;
            InsertObject.curve = curve;
            LeftHandQueue.Enqueue(InsertObject);
        }
        else if (ID.Equals("RightHand") || ID.Equals("righthand") || ID.Equals("Right Hand") || ID.Equals("right hand"))
        {
            LerpRightHand = true;
            RightSpeed = speed;
            Ik.solver.rightHandEffector.positionWeight = from;
            Ik.solver.rightHandEffector.rotationWeight = from;
            QueueObject InsertObject = new QueueObject();
            InsertObject.from = from;
            InsertObject.to = to;
            InsertObject.timer = 0.0f;
            InsertObject.curve = curve;
            RightHandQueue.Enqueue(InsertObject);
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
        obj.timer += DeltaTime;
        if (obj.timer >= obj.to)
        {
            EndLeftEffectorLerp();
        }
        else
        {
            Ik.solver.leftHandEffector.positionWeight = obj.curve.Evaluate(obj.timer);
        }
    }

    //Execute the lerp between from and to
    private void RightEffectorDoLerp(float DeltaTime, QueueObject obj)
    {

    }

    private void EndLeftEffectorLerp()
    {
        //Make sure we aren't trying to dequeue from an empty queue and end lerping
        if (LeftHandQueue.Count == 0) LerpLeftHand = false;
        LeftHandQueue.Dequeue();
        //Reset speed to baseline speed
        LeftSpeed = 4.0f;
    }

    private void EndRightEffectorLerp()
    {
        //Make sure we aren't trying to dequeue from an empty queue and end lerping
        if (RightHandQueue.Count == 0) LerpLeftHand = false;
        RightHandQueue.Dequeue();
        //Reset speed to baseline speed
        RightSpeed = 4.0f;
    }

    private void FixedUpdate()
    {
        if (LerpLeftHand && LeftHandQueue.Count != 0) LeftEffectorDoLerp(Time.deltaTime, LeftHandQueue.Peek());
        if (LerpRightHand && RightHandQueue.Count != 0) RightEffectorDoLerp(Time.deltaTime, RightHandQueue.Peek());
    }

}
