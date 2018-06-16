using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;

public class SCR_IKToolset : MonoBehaviour {

    private FullBodyBipedIK Ik;
    private bool LerpLeftHand;
    private bool LerpRightHand;
    [HideInInspector]
    public float LeftSpeed;
    [HideInInspector]
    public float RightSpeed;
    private Queue<float> LeftHandFrom;
    private Queue<float> RightHandFrom;
    private Queue<float> LeftHandTo;
    private Queue<float> RightHandTo;

    // Use this for initialization
    void Start () {
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

    public void SetEffector(string ID, GameObject obj)
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

    public void StartEffectorLerp(string ID, float from, float to)
    {
        if (ID.Equals("LeftHand") || ID.Equals("lefthand") || ID.Equals("Left Hand") || ID.Equals("left hand"))
        {
            LerpLeftHand = true;
            Ik.solver.leftHandEffector.positionWeight = from;
            Ik.solver.leftHandEffector.rotationWeight = from;
            LeftHandFrom.Enqueue(from);
            LeftHandTo.Enqueue(to);
        }
        else if (ID.Equals("RightHand") || ID.Equals("righthand") || ID.Equals("Right Hand") || ID.Equals("right hand"))
        {
            LerpRightHand = true;
            Ik.solver.rightHandEffector.positionWeight = from;
            Ik.solver.rightHandEffector.rotationWeight = from;
            RightHandFrom.Enqueue(from);
            RightHandTo.Enqueue(to);
        }
        else Debug.LogError("We could not find the specified effector!");
    } 

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

    public void SetEffectorWeightSpeed(string ID, float speed)
    {
        if (ID.Equals("LeftHand") || ID.Equals("lefthand") || ID.Equals("Left Hand") || ID.Equals("left hand"))
        {
            LeftSpeed = speed;
        }

        else if (ID.Equals("RightHand") || ID.Equals("righthand") || ID.Equals("Right Hand") || ID.Equals("right hand"))
        {
            RightSpeed = speed;
        }

        else Debug.LogError("We could not find the specified effector!");
    }

    private void LeftEffectorDoLerp(float DeltaTime, float from, float to)
    {
        if (from > to)
        {
            float temp = Ik.solver.leftHandEffector.positionWeight;
            temp -= (DeltaTime * LeftSpeed);
            if (temp <= to)
            {
                EndLeftEffectorLerp();
                Ik.solver.leftHandEffector.positionWeight = to;
                Ik.solver.leftHandEffector.rotationWeight = to;
            }
            else
            {
                Ik.solver.leftHandEffector.positionWeight = temp;
                Ik.solver.leftHandEffector.rotationWeight = temp;
            }
        }
        else
        {
            float temp = Ik.solver.leftHandEffector.positionWeight;
            temp += (DeltaTime * LeftSpeed);
            if (temp >= to)
            {
                EndLeftEffectorLerp();
                Ik.solver.leftHandEffector.positionWeight = to;
                Ik.solver.leftHandEffector.rotationWeight = to;
            }
            else
            {
                Ik.solver.leftHandEffector.positionWeight = temp;
                Ik.solver.leftHandEffector.rotationWeight = temp;
            }
        }
    }

    private void RightEffectorDoLerp(float DeltaTime, float from, float to)
    {
        if (from > to)
        {
            float temp = Ik.solver.rightHandEffector.positionWeight;
            temp -= (DeltaTime * RightSpeed);
            if (temp <= to)
            {
                EndRightEffectorLerp();
                Ik.solver.rightHandEffector.positionWeight = to;
                Ik.solver.rightHandEffector.rotationWeight = to;
            }
            else
            {
                Ik.solver.rightHandEffector.positionWeight = temp;
                Ik.solver.rightHandEffector.rotationWeight = temp;
            }
        }
        else
        {
            float temp = Ik.solver.rightHandEffector.positionWeight;
            temp += (DeltaTime * RightSpeed);
            if (temp >= to)
            {
                EndRightEffectorLerp();
                Ik.solver.rightHandEffector.positionWeight = to;
                Ik.solver.rightHandEffector.rotationWeight = to;
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
        if ((LeftHandFrom.Count + LeftHandTo.Count) == 0) LerpLeftHand = false;
        LeftHandFrom.Dequeue();
        LeftHandTo.Dequeue();
        LeftSpeed = 4.0f;
    }

    private void EndRightEffectorLerp()
    {
        if ((LeftHandFrom.Count + LeftHandTo.Count) == 0) LerpLeftHand = false;
        RightHandFrom.Dequeue();
        RightHandTo.Dequeue();
        RightSpeed = 4.0f;
    }

    private void FixedUpdate()
    {
        if (LerpLeftHand && (LeftHandFrom.Count + LeftHandTo.Count) != 0) LeftEffectorDoLerp(Time.deltaTime, LeftHandFrom.Peek(), LeftHandTo.Peek());
        if (LerpRightHand && (RightHandFrom.Count + RightHandTo.Count) != 0) RightEffectorDoLerp(Time.deltaTime, RightHandFrom.Peek(), RightHandTo.Peek());
    }

}
