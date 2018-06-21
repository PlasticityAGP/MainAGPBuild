using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;

//The purpose of this script is to provide tools for lerping effector weights in FinalIK. Currently only supports left and right hand effectors

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
    private Queue<float> LeftHandFrom;
    private Queue<float> RightHandFrom;
    private Queue<float> LeftHandTo;
    private Queue<float> RightHandTo;

    // Use this for initialization
    void Start () {
        //Find the instance of the IK component attached to our character
        if (gameObject.GetComponentInChildren<FullBodyBipedIK>()) Ik = gameObject.GetComponentInChildren<FullBodyBipedIK>();
        else Debug.LogError("We need a a FullBodyBipedIK component attached to one of the Character's child Game Objects");
        LerpLeftHand = false;
        LerpRightHand = false;
        LeftSpeed = 4.0f;
        RightSpeed = 4.0f;
        LeftHandFrom = new Queue<float>();
        LeftHandTo = new Queue<float>();
        RightHandFrom = new Queue<float>();
        RightHandTo = new Queue<float>();
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
    /// Commence lerping between the specified from effector weight to the to effector weight for a given effector, ID at speed speed
    /// </summary>
    /// <param name="ID">The name of the effector you would like to start lerping</param>
    /// <param name="from">The initial effector weight value at the beginning of the lerp</param>
    /// <param name="to">The target effector weight that is being lerped towards</param>
    /// <param name="speed">The speed at which the lerp will occur</param>
    public void StartEffectorLerp(string ID, float from, float to, float speed)
    {
        if (ID.Equals("LeftHand") || ID.Equals("lefthand") || ID.Equals("Left Hand") || ID.Equals("left hand"))
        {
            LerpLeftHand = true;
            LeftSpeed = speed;
            Ik.solver.leftHandEffector.positionWeight = from;
            Ik.solver.leftHandEffector.rotationWeight = from;
            LeftHandFrom.Enqueue(from);
            LeftHandTo.Enqueue(to);
        }
        else if (ID.Equals("RightHand") || ID.Equals("righthand") || ID.Equals("Right Hand") || ID.Equals("right hand"))
        {
            LerpRightHand = true;
            RightSpeed = speed;
            Ik.solver.rightHandEffector.positionWeight = from;
            Ik.solver.rightHandEffector.rotationWeight = from;
            RightHandFrom.Enqueue(from);
            RightHandTo.Enqueue(to);
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
    private void LeftEffectorDoLerp(float DeltaTime, float from, float to)
    {
        //If from is greater than to, gradually decrease the effector weight based on DeltaTime
        if (from > to)
        {
            float temp = Ik.solver.leftHandEffector.positionWeight;
            temp -= (DeltaTime * LeftSpeed);
            if (temp <= to)
            {
                //When done, make sure weights = to, and call end effector lerp
                Ik.solver.leftHandEffector.positionWeight = to;
                Ik.solver.leftHandEffector.rotationWeight = to;
                EndLeftEffectorLerp();
            }
            else
            {
                Ik.solver.leftHandEffector.positionWeight = temp;
                Ik.solver.leftHandEffector.rotationWeight = temp;
            }
        }
        //If to is greater than from, gradually increase the effector weight based on DeltaTime
        else
        {
            float temp = Ik.solver.leftHandEffector.positionWeight;
            temp += (DeltaTime * LeftSpeed);
            if (temp >= to)
            {
                //When done, make sure weights = to, and call end effector lerp
                Ik.solver.leftHandEffector.positionWeight = to;
                Ik.solver.leftHandEffector.rotationWeight = to;
                EndLeftEffectorLerp();
            }
            else
            {
                Ik.solver.leftHandEffector.positionWeight = temp;
                Ik.solver.leftHandEffector.rotationWeight = temp;
            }
        }
    }

    //Execute the lerp between from and to
    private void RightEffectorDoLerp(float DeltaTime, float from, float to)
    {
        //If from is greater than to, gradually decrease the effector weight based on DeltaTime
        if (from > to)
        {
            float temp = Ik.solver.rightHandEffector.positionWeight;
            temp -= (DeltaTime * RightSpeed);
            if (temp <= to)
            {
                //When done, make sure weights = to, and call end effector lerp
                Ik.solver.rightHandEffector.positionWeight = to;
                Ik.solver.rightHandEffector.rotationWeight = to;
                EndRightEffectorLerp();
            }
            else
            {
                Ik.solver.rightHandEffector.positionWeight = temp;
                Ik.solver.rightHandEffector.rotationWeight = temp;
            }
        }
        //If to is greater than from, gradually increase the effector weight based on DeltaTime
        else
        {
            float temp = Ik.solver.rightHandEffector.positionWeight;
            temp += (DeltaTime * RightSpeed);
            if (temp >= to)
            {
                //When done, make sure weights = to, and call end effector lerp
                Ik.solver.rightHandEffector.positionWeight = to;
                Ik.solver.rightHandEffector.rotationWeight = to;
                EndRightEffectorLerp();
            }
            else
            {
                Ik.solver.rightHandEffector.positionWeight = temp;
                Ik.solver.rightHandEffector.rotationWeight = temp;
            }
        }
    }

    private void EndLeftEffectorLerp()
    {
        //Make sure we aren't trying to dequeue from an empty queue and end lerping
        if ((LeftHandFrom.Count + LeftHandTo.Count) == 0) LerpLeftHand = false;
        LeftHandFrom.Dequeue();
        LeftHandTo.Dequeue();
        //Reset speed to baseline speed
        LeftSpeed = 4.0f;
    }

    private void EndRightEffectorLerp()
    {
        //Make sure we aren't trying to dequeue from an empty queue and end lerping
        if ((LeftHandFrom.Count + LeftHandTo.Count) == 0) LerpLeftHand = false;
        RightHandFrom.Dequeue();
        RightHandTo.Dequeue();
        //Reset speed to baseline speed
        RightSpeed = 4.0f;
    }

    private void FixedUpdate()
    {
        if (LerpLeftHand && (LeftHandFrom.Count + LeftHandTo.Count) != 0) LeftEffectorDoLerp(Time.deltaTime, LeftHandFrom.Peek(), LeftHandTo.Peek());
        if (LerpRightHand && (RightHandFrom.Count + RightHandTo.Count) != 0) RightEffectorDoLerp(Time.deltaTime, RightHandFrom.Peek(), RightHandTo.Peek());
    }

}
