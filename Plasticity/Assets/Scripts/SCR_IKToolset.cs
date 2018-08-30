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

public class SCR_IKToolset : SCR_GameplayStatics {

    //Reference to the IK component we have attached to our character model
    [SerializeField]
    private AnimationCurve OnLadderCurve;
    [SerializeField]
    private AnimationCurve LadderTransition;
    private FullBodyBipedIK Ik;
    private Coroutine LatestLeftHand;
    private Coroutine LatestRightHand;
    private Coroutine LatestLeftFoot;
    private Coroutine LatestRightFoot;
    private Coroutine LatestBody;
    private Coroutine LadderCycle;
    private GameObject[] LadderRungs;
    private int ClimbState;
    private bool Direction;
    private bool LastCycleDirection = false;
    private bool LadderCycleOn;
    private bool LadderMounted;
    private int[] HandRungs;
    private int[] FeetRungs;
    private float Period = 0.25f;
    private bool SideOfLadder;
    [HideInInspector]
    public Vector3 LadderSlope;
    [HideInInspector]
    public GameObject BodyPos;


    // Use this for initialization
    void Start () {
        //Find the instance of the IK component attached to our character
        if (gameObject.GetComponentInChildren<FullBodyBipedIK>()) Ik = gameObject.GetComponentInChildren<FullBodyBipedIK>();
        else Debug.LogError("We need a a FullBodyBipedIK component attached to one of the Character's child Game Objects");
        HandRungs = new int[2];
        FeetRungs = new int[2];
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

        else if (ID.Equals("LeftFoot") || ID.Equals("leftfoot") || ID.Equals("Left Foot") || ID.Equals("left foot"))
        {
            Ik.solver.leftFootEffector.position = Loc;
        }

        else if (ID.Equals("RightFoot") || ID.Equals("rightfoot") || ID.Equals("Right Foot") || ID.Equals("right foot"))
        {
            Ik.solver.rightFootEffector.position = Loc;
        }
        else if (ID.Equals("Body") || ID.Equals("body"))
        {
            Ik.solver.bodyEffector.position = Loc;
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

        else if (ID.Equals("LeftFoot") || ID.Equals("leftfoot") || ID.Equals("Left Foot") || ID.Equals("left foot"))
        {
            if (obj == null) Ik.solver.leftFootEffector.target = null;
            else Ik.solver.leftFootEffector.target = obj.transform;
        }

        else if (ID.Equals("RightFoot") || ID.Equals("rightfoot") || ID.Equals("Right Foot") || ID.Equals("right foot"))
        {
            if (obj == null) Ik.solver.rightFootEffector.target = null;
            else Ik.solver.rightFootEffector.target = obj.transform;
        }
        else if (ID.Equals("Body") || ID.Equals("body"))
        {
            if (obj == null) Ik.solver.bodyEffector.target = null;
            else Ik.solver.bodyEffector.target = obj.transform;
        }

        else Debug.LogError("We could not find the specified effector!");
    }

    /// <summary>
    /// Tells a specific effector update its location to that of a tracked GameObject
    /// </summary>
    /// <param name="ID">Which effector you would like to have target a GameObject</param>
    /// <param name="obj">The GameObject you would like the effector to follow</param>
    public void SetEffectorTargetTransform(string ID, Transform obj)
    {
        if (ID.Equals("LeftHand") || ID.Equals("lefthand") || ID.Equals("Left Hand") || ID.Equals("left hand"))
        {
            if (obj == null) Ik.solver.leftHandEffector.target = null;
            else Ik.solver.leftHandEffector.target = obj;
        }

        else if (ID.Equals("RightHand") || ID.Equals("righthand") || ID.Equals("Right Hand") || ID.Equals("right hand"))
        {
            if (obj == null) Ik.solver.rightHandEffector.target = null;
            else Ik.solver.rightHandEffector.target = obj;
        }

        else if (ID.Equals("LeftFoot") || ID.Equals("leftfoot") || ID.Equals("Left Foot") || ID.Equals("left foot"))
        {
            if (obj == null) Ik.solver.leftFootEffector.target = null;
            else Ik.solver.leftFootEffector.target = obj;
        }

        else if (ID.Equals("RightFoot") || ID.Equals("rightfoot") || ID.Equals("Right Foot") || ID.Equals("right foot"))
        {
            if (obj == null) Ik.solver.rightFootEffector.target = null;
            else Ik.solver.rightFootEffector.target = obj;
        }
        else if (ID.Equals("Body") || ID.Equals("body"))
        {
            if (obj == null) Ik.solver.bodyEffector.target = null;
            else Ik.solver.bodyEffector.target = obj;
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

        else if (ID.Equals("LeftFoot") || ID.Equals("leftfoot") || ID.Equals("Left Foot") || ID.Equals("left foot"))
        {
            if (LatestLeftFoot != null) StopCoroutine(LatestLeftFoot);
            Ik.solver.leftFootEffector.positionWeight = weight;
            Ik.solver.leftFootEffector.rotationWeight = weight;
        }

        else if (ID.Equals("RightFoot") || ID.Equals("rightfoot") || ID.Equals("Right Foot") || ID.Equals("right foot"))
        {
            if (LatestRightHand != null) StopCoroutine(LatestRightFoot);
            Ik.solver.rightFootEffector.positionWeight = weight;
            Ik.solver.rightFootEffector.rotationWeight = weight;
        }
        else if (ID.Equals("Body") || ID.Equals("body"))
        {
            if (LatestBody != null) StopCoroutine(LatestBody);
            Ik.solver.bodyEffector.positionWeight = weight;
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
        if(ID == "LeftHand" || ID.Equals("lefthand") || ID.Equals("Left Hand") || ID.Equals("left hand")) LatestLeftHand = StartCoroutine(IKCoroutine(ID, InsertObject));
        else if (ID == "RightHand" || ID.Equals("righthand") || ID.Equals("Right Hand") || ID.Equals("right hand")) LatestRightHand = StartCoroutine(IKCoroutine(ID, InsertObject));
        else if (ID == "LeftFoot" || ID.Equals("leftfoot") || ID.Equals("Left Foot") || ID.Equals("left foot")) LatestLeftFoot = StartCoroutine(IKCoroutine(ID, InsertObject));
        else if (ID == "RightFoot" || ID.Equals("rightfoot") || ID.Equals("Right Foot") || ID.Equals("right foot")) LatestRightFoot = StartCoroutine(IKCoroutine(ID, InsertObject));
        else if (ID == "Body" || ID.Equals("body")) LatestBody = StartCoroutine(IKCoroutine(ID, InsertObject));
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
        else if (ID.Equals("LeftFoot") || ID.Equals("leftfoot") || ID.Equals("Left Foot") || ID.Equals("left foot"))
            return Ik.solver.leftFootEffector.positionWeight;
        else if (ID.Equals("RightFoot") || ID.Equals("rightfoot") || ID.Equals("Right Foot") || ID.Equals("right foot"))
            return Ik.solver.rightFootEffector.positionWeight;
        else if (ID.Equals("Body") || ID.Equals("body"))
            return Ik.solver.bodyEffector.positionWeight;
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
            else if (Effector == "Body")
            {
                Ik.solver.bodyEffector.positionWeight = obj.curve.Evaluate(IkTimer);
                Ik.solver.bodyEffector.rotationWeight = obj.curve.Evaluate(IkTimer);
            }
            else Debug.LogError("We could not find the specified effector!");
            yield return null;
        }
    }

    public void InitiateLadderIK(GameObject[] Rungs)
    {
        LadderRungs = Rungs;
    }

    public void MountLadderIK(bool side, bool ZLadder)
    {
        LadderMounted = true;
        SideOfLadder = side;
        GameObject[] ReturnRungs = FindClosestRungs();
        SetEffectorTarget("LeftHand", null);
        SetEffectorTarget("RightHand", null);
        SetEffectorTarget("Body", null);
        Vector3[] OffsetHandPoints = FindOffsetPoints(ReturnRungs[0], false, ZLadder);
        Vector3[] OffsetFeetPoints = FindOffsetPoints(ReturnRungs[1], true, ZLadder);
        SetEffectorLocation("LeftHand", OffsetHandPoints[0]);
        SetEffectorLocation("RightHand", OffsetHandPoints[1]);
        SetEffectorLocation("LeftFoot", OffsetFeetPoints[0]);
        SetEffectorLocation("RightFoot", OffsetFeetPoints[1]);
        SetEffectorTarget("Body", BodyPos);
        StartEffectorLerp("LeftHand", OnLadderCurve, 1.0f);
        StartEffectorLerp("RightHand", OnLadderCurve, 1.0f);
        StartEffectorLerp("LeftFoot", OnLadderCurve, 1.0f);
        StartEffectorLerp("RightFoot", OnLadderCurve, 1.0f);
        ForceEffectorWeight("Body", 0.1f);

        Ik.solver.leftHandEffector.rotation = ReturnRungs[0].transform.rotation;
        Ik.solver.rightHandEffector.rotation = ReturnRungs[0].transform.rotation;
        if (SideOfLadder)
        {
            Vector3 Foot = new Vector3();
            Foot.x = 562.294f;
            Foot.y = 270.977f;
            Foot.z = -0.367981f;
            Ik.solver.leftFootEffector.rotation.eulerAngles = Foot;
            Ik.solver.rightFootEffector.rotation.eulerAngles = Foot;
        }
        else
        {
            Vector3 Foot = new Vector3();
            Foot.x = -63.609f;
            Foot.y = -445.793f;
            Foot.z = -187.84f;
            Ik.solver.leftFootEffector.rotation.eulerAngles = Foot;
            Ik.solver.rightFootEffector.rotation.eulerAngles = Foot;
        }
    }

    private GameObject[] FindClosestRungs()
    {
        float ShoulderDistance = 1000.0f;
        float FootDistance = 1000.0f;
        GameObject ShoulderRung = null;
        GameObject FootRung = null;
        for(int i = 0; i < LadderRungs.Length; ++i)
        {
            if((Ik.solver.leftArmMapping.bone1.position - LadderRungs[i].transform.position).magnitude < ShoulderDistance)
            {
                ShoulderDistance = (Ik.solver.leftArmMapping.bone1.position - LadderRungs[i].transform.position).magnitude;
                ShoulderRung = LadderRungs[i];
                HandRungs[0] = i;
                HandRungs[1] = i;
            }
            if((Ik.solver.leftLegMapping.bone3.position - LadderRungs[i].transform.position).magnitude < FootDistance)
            {
                FootDistance = (Ik.solver.leftLegMapping.bone3.position - LadderRungs[i].transform.position).magnitude;
                FootRung = LadderRungs[i];
                FeetRungs[0] = i;
                FeetRungs[1] = i;
            }
        }
        LadderSlope = ShoulderRung.transform.position - FootRung.transform.position;
        return new GameObject[] {ShoulderRung, FootRung};
    }

    private Vector3[] FindOffsetPoints(GameObject Rung, bool feet, bool ZLadder)
    {
        Vector3 PositionLeft = Rung.transform.position;
        Vector3 PositionRight = Rung.transform.position;
        if (!ZLadder)
        {
            if (SideOfLadder)
            {
                PositionLeft.z += 0.2f;
                PositionRight.z -= 0.2f;
            }
            else
            {
                PositionLeft.z -= 0.2f;
                PositionRight.z += 0.2f;
            }
            if (feet)
            {
                PositionLeft.y += 0.1f;
                PositionRight.y += 0.1f;
            }
        }
        return new Vector3[] { PositionLeft, PositionRight };
    }

    private void UpState()
    {
        if (ClimbState == 3) ClimbState = 0;
        else ++ClimbState;
    }

    private void DownState()
    {
        if (ClimbState == 0) ClimbState = 3;
        else --ClimbState;
    }

    public void ClimbingUp()
    {
        if (LadderMounted)
        {
            Direction = true;
            if (!LadderCycleOn)
            {
                LadderCycle = StartCoroutine(ClimbCycle());
                LadderCycleOn = true;
            }
        }
    }

    public void ClimbingDown()
    {
        if (LadderMounted)
        {
            Direction = false;
            if (!LadderCycleOn)
            {
                LadderCycle = StartCoroutine(ClimbCycle());
                LadderCycleOn = true;
            }
        }
    }

    public void Still()
    {
        if (LadderMounted)
        {
            if (LadderCycleOn)
            {
                StopCoroutine(LadderCycle);
                LadderCycleOn = false;
            }
        }
    }

    IEnumerator ClimbCycle()
    {
        while (true)
        {
            if(LastCycleDirection != Direction)
            {
                if (Direction) DownState();
                else UpState();
                LastCycleDirection = Direction;
            }
            if (Direction)
            {
                UpState();
            }
            else
            {
                DownState();
            }
            MoveHands();
            yield return new WaitForSeconds(Period-0.1f);
        }
    }

    private void MoveHands()
    {
        if (Direction)
        {
            switch (ClimbState)
            {
                case 0:
                    if (HandRungs[1] == LadderRungs.Length - 1) Still();
                    else
                    {
                        Vector3 From = FindOffsetPoints(LadderRungs[HandRungs[1]], false, false)[1];
                        ++HandRungs[1];
                        Vector3 To = FindOffsetPoints(LadderRungs[HandRungs[1]], false, false)[1];
                        LadderSlope = To - From;
                        StartCoroutine(LerpLocation(From, To, "RightHand"));
                        StartEffectorLerp("RightHand", LadderTransition, Period);
                      
                    }
                    break;
                case 1:
                    if (FeetRungs[1] == LadderRungs.Length - 1) Still();
                    else
                    {
                        Vector3 From = FindOffsetPoints(LadderRungs[FeetRungs[1]], true, false)[1];
                        ++FeetRungs[1];
                        Vector3 To = FindOffsetPoints(LadderRungs[FeetRungs[1]], true, false)[1];
                        LadderSlope = To - From;
                        StartCoroutine(LerpLocation(From, To, "RightFoot"));
                        StartEffectorLerp("RightFoot", LadderTransition, Period);

                    }
                    break;
                case 2:
                    if (HandRungs[0] == LadderRungs.Length - 1) Still();
                    else
                    {
                        Vector3 From = FindOffsetPoints(LadderRungs[HandRungs[0]], false, false)[0];
                        ++HandRungs[0];
                        Vector3 To = FindOffsetPoints(LadderRungs[HandRungs[0]], false, false)[0];
                        LadderSlope = To - From;
                        StartCoroutine(LerpLocation(From, To, "LeftHand"));
                        StartEffectorLerp("Lefthand", LadderTransition, Period);

                    }
                    break;
                case 3:
                    if (FeetRungs[0] == LadderRungs.Length - 1) Still();
                    else
                    {
                        Vector3 From = FindOffsetPoints(LadderRungs[FeetRungs[0]], true, false)[0];
                        ++FeetRungs[0];
                        Vector3 To = FindOffsetPoints(LadderRungs[FeetRungs[0]], true, false)[0];
                        LadderSlope = To - From;
                        StartCoroutine(LerpLocation(From, To, "LeftFoot"));
                        StartEffectorLerp("LeftFoot", LadderTransition, Period);
                    }
                    break;
                default:
                    break;
            }
        }
        else
        {
            switch (ClimbState)
            {
                case 0:
                    if (HandRungs[1] == 0) Still();
                    else
                    {
                        Vector3 From = FindOffsetPoints(LadderRungs[HandRungs[1]], false, false)[1];
                        --HandRungs[1];
                        Vector3 To = FindOffsetPoints(LadderRungs[HandRungs[1]], false, false)[1];
                        LadderSlope = From - To;
                        StartCoroutine(LerpLocation(From, To, "RightHand"));
                        StartEffectorLerp("RightHand", LadderTransition, Period);

                    }
                    break;
                case 1:
                    if (FeetRungs[1] == 0) Still();
                    else
                    {
                        Vector3 From = FindOffsetPoints(LadderRungs[FeetRungs[1]], true, false)[1];
                        --FeetRungs[1];
                        Vector3 To = FindOffsetPoints(LadderRungs[FeetRungs[1]], true, false)[1];
                        LadderSlope = From - To;
                        StartCoroutine(LerpLocation(From, To, "RightFoot"));
                        StartEffectorLerp("RightFoot", LadderTransition, Period);
                        StartEffectorLerp("LeftFoot", LadderTransition, Period);
                    }
                    break;
                case 2:
                    if (HandRungs[0] == 0) Still();
                    else
                    {
                        Vector3 From = FindOffsetPoints(LadderRungs[HandRungs[0]], false, false)[0];
                        --HandRungs[0];
                        Vector3 To = FindOffsetPoints(LadderRungs[HandRungs[0]], false, false)[0];
                        LadderSlope = From - To;
                        StartCoroutine(LerpLocation(From, To, "LeftHand"));
                        StartEffectorLerp("Lefthand", LadderTransition, Period);

                    }
                    break;
                case 3:
                    if (FeetRungs[0] == 0) Still();
                    else
                    {
                        Vector3 From = FindOffsetPoints(LadderRungs[FeetRungs[0]], true, false)[0];
                        --FeetRungs[0];
                        Vector3 To = FindOffsetPoints(LadderRungs[FeetRungs[0]], true, false)[0];
                        LadderSlope = From - To;
                        StartCoroutine(LerpLocation(From, To, "LeftFoot"));
                        StartEffectorLerp("LeftFoot", LadderTransition, Period);

                    }
                    break;
                default:
                    break;
            }
        }
    }

    IEnumerator LerpLocation(Vector3 PointA, Vector3 PointB, string Effector)
    {
        float CurrentTime = 0.0f;
        while (CurrentTime <= Period)
        {
            CurrentTime += Time.deltaTime;
            float Modifier = CurrentTime * (1.0f / Period);
            Vector3 OutputPos = Vector3.Lerp(PointA, PointB, Modifier);
            SetEffectorLocation(Effector, OutputPos);
            yield return null;
        }
    }
}
