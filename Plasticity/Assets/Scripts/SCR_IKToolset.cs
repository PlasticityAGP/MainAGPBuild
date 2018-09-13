using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;

//The purpose of this script is to provide tools for lerping effector weights in FinalIK. Currently only supports left and right hand effectors

public struct QueueObject
{
    public float duration;
    public AnimationCurve curve;
}

public class ToolsetCoroutine : SCR_GameplayStatics
{
    public float IkTimer;
    private bool Done;
    private FullBodyBipedIK Ik;
    private SCR_IKToolset IkTools;
    private string EffectorName;
    private QueueObject ObjectRef;
    private bool DoExit;

    public void Initialize(FullBodyBipedIK Passed, SCR_IKToolset Tools)
    {
        Done = true;
        DoExit = false;
        Ik = Passed;
        IkTimer = 0.0f;
        IkTools = Tools;
    }

    public void StartIkCoroutine(string Effector, QueueObject obj)
    {
        DoExit = false;
        if(Done)
        {
            EffectorName = Effector;
            ObjectRef = obj;
            StartCoroutine(IKCoroutine());
        }   
        else
        {
            float OldHeight = ObjectRef.curve.Evaluate(IkTimer);
            EffectorName = Effector;
            ObjectRef = obj;
            GetTimeAtHeight(OldHeight);
        }
    }

    private void GetTimeAtHeight(float HeightValue)
    { 
        float Dif = 100.0f;
        float TempTime = 0.0f;
        for (float i = 0.0f; i < ObjectRef.duration; i += 0.01f)
        {
            if (Mathf.Abs(ObjectRef.curve.Evaluate(i) - HeightValue) < Dif)
            {
                Dif = Mathf.Abs(ObjectRef.curve.Evaluate(i) - HeightValue);
                TempTime = i;
            }
        }
        IkTimer = TempTime;
    }

    public void Exiting(string EffectorName, float TimeLength)
    {
        DoExit = true;
        Timer(TimeLength, EffectorName, NullEffector);
    }

    private void NullEffector(string EffectorName)
    {
        if(DoExit)
        {
            IkTools.ForceEffectorWeight(EffectorName, 0.0f);
            IkTools.SetEffectorTarget(EffectorName, null);
        }
    }

    public void HaltRoutine()
    {
        StopAllCoroutines();
        Done = true;
        IkTimer = 0.0f;
    }


    IEnumerator IKCoroutine()
    {
        Done = false;
        while (IkTimer < ObjectRef.duration)
        {
            IkTimer += Time.deltaTime;
            if (EffectorName == "LeftHand")
            {
                Ik.solver.leftHandEffector.positionWeight = ObjectRef.curve.Evaluate(IkTimer);
                Ik.solver.leftHandEffector.rotationWeight = ObjectRef.curve.Evaluate(IkTimer);
            }
            else if (EffectorName == "RightHand")
            {
                Ik.solver.rightHandEffector.positionWeight = ObjectRef.curve.Evaluate(IkTimer);
                Ik.solver.rightHandEffector.rotationWeight = ObjectRef.curve.Evaluate(IkTimer);
            }
            else if (EffectorName == "LeftFoot")
            {
                Ik.solver.leftFootEffector.positionWeight = ObjectRef.curve.Evaluate(IkTimer);
                Ik.solver.leftFootEffector.rotationWeight = ObjectRef.curve.Evaluate(IkTimer);
            }
            else if (EffectorName == "RightFoot")
            {
                Ik.solver.rightFootEffector.positionWeight = ObjectRef.curve.Evaluate(IkTimer);
                Ik.solver.rightFootEffector.rotationWeight = ObjectRef.curve.Evaluate(IkTimer);
            }
            else if (EffectorName == "Body")
            {
                Ik.solver.bodyEffector.positionWeight = ObjectRef.curve.Evaluate(IkTimer);
                Ik.solver.bodyEffector.rotationWeight = ObjectRef.curve.Evaluate(IkTimer);
            }
            else Debug.LogError("We could not find the specified effector!");
            yield return null;
        }
        Done = true;
        IkTimer = 0.0f;
    }
}

public class SCR_IKToolset : SCR_GameplayStatics {

    //Reference to the IK component we have attached to our character model
    [SerializeField]
    private AnimationCurve OnLadderCurveHands;
    [SerializeField]
    private AnimationCurve OnLadderCurveFeet;
    [SerializeField]
    private AnimationCurve LadderTransition;
    private FullBodyBipedIK Ik;
    private ToolsetCoroutine LatestLeftHand;
    private ToolsetCoroutine LatestRightHand;
    private ToolsetCoroutine LatestLeftFoot;
    private ToolsetCoroutine LatestRightFoot;
    private ToolsetCoroutine LatestBody;
    private Coroutine LadderCycle;
    private GameObject[] LadderRungs;
    private int ClimbState;
    private bool Direction;
    private bool LastCycleDirection = false;
    private bool LadderCycleOn;
    private bool LadderMounted;
    private int[] HandRungs;
    private int[] FeetRungs;
    private float Period = 0.2f;
    private bool SideOfLadder;
    [HideInInspector]
    public bool DisableDown;
    [HideInInspector]
    public bool DisableUp;
    [HideInInspector]
    public Vector3 LadderSlope;
    [HideInInspector]
    public GameObject BodyPos;
    private bool InitiationComplete = false;
    private bool ShutdownComplete = false;
    [SerializeField]
    private SCR_IKSettingData ClimbingData;
    [SerializeField]
    private SCR_IKSettingData DraggingData;
    private bool Reverse;


    // Use this for initialization
    void Start() {
        //Find the instance of the IK component attached to our character
        if (gameObject.GetComponentInChildren<FullBodyBipedIK>()) Ik = gameObject.GetComponentInChildren<FullBodyBipedIK>();
        else Debug.LogError("We need a a FullBodyBipedIK component attached to one of the Character's child Game Objects");
        gameObject.AddComponent<ToolsetCoroutine>();
        LatestLeftHand = gameObject.AddComponent<ToolsetCoroutine>();
        LatestRightHand = gameObject.AddComponent<ToolsetCoroutine>(); 
        LatestLeftFoot = gameObject.AddComponent<ToolsetCoroutine>();
        LatestRightFoot = gameObject.AddComponent<ToolsetCoroutine>();
        LatestBody = gameObject.AddComponent<ToolsetCoroutine>();
        LatestLeftHand.Initialize(Ik, this);
        LatestRightHand.Initialize(Ik, this);
        LatestLeftFoot.Initialize(Ik, this);
        LatestRightFoot.Initialize(Ik, this);
        LatestBody.Initialize(Ik, this);
        HandRungs = new int[2];
        FeetRungs = new int[2];
        LoadDraggingData();
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
            LatestLeftHand.HaltRoutine();
            Ik.solver.leftHandEffector.positionWeight = weight;
            Ik.solver.leftHandEffector.rotationWeight = weight;
        }

        else if (ID.Equals("RightHand") || ID.Equals("righthand") || ID.Equals("Right Hand") || ID.Equals("right hand"))
        {
            LatestRightHand.HaltRoutine();
            Ik.solver.rightHandEffector.positionWeight = weight;
            Ik.solver.rightHandEffector.rotationWeight = weight;
        }

        else if (ID.Equals("LeftFoot") || ID.Equals("leftfoot") || ID.Equals("Left Foot") || ID.Equals("left foot"))
        {
            LatestLeftFoot.HaltRoutine();
            Ik.solver.leftFootEffector.positionWeight = weight;
            Ik.solver.leftFootEffector.rotationWeight = weight;
        }

        else if (ID.Equals("RightFoot") || ID.Equals("rightfoot") || ID.Equals("Right Foot") || ID.Equals("right foot"))
        {
            LatestRightFoot.HaltRoutine();
            Ik.solver.rightFootEffector.positionWeight = weight;
            Ik.solver.rightFootEffector.rotationWeight = weight;
        }
        else if (ID.Equals("Body") || ID.Equals("body"))
        {
            LatestBody.HaltRoutine();
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
        if (ID == "LeftHand" || ID.Equals("lefthand") || ID.Equals("Left Hand") || ID.Equals("left hand")) LatestLeftHand.StartIkCoroutine(ID, InsertObject);
        else if (ID == "RightHand" || ID.Equals("righthand") || ID.Equals("Right Hand") || ID.Equals("right hand")) LatestRightHand.StartIkCoroutine(ID, InsertObject);
        else if (ID == "LeftFoot" || ID.Equals("leftfoot") || ID.Equals("Left Foot") || ID.Equals("left foot")) LatestLeftFoot.StartIkCoroutine(ID, InsertObject);
        else if (ID == "RightFoot" || ID.Equals("rightfoot") || ID.Equals("Right Foot") || ID.Equals("right foot")) LatestRightFoot.StartIkCoroutine(ID, InsertObject);
        else if (ID == "Body" || ID.Equals("body")) LatestBody.StartIkCoroutine(ID, InsertObject);
    }

    public void StartEffectorLerp(string ID, AnimationCurve curve, float duration, bool exiting)
    {
        QueueObject InsertObject = new QueueObject();
        InsertObject.duration = duration;
        InsertObject.curve = curve;
        if (ID == "LeftHand" || ID.Equals("lefthand") || ID.Equals("Left Hand") || ID.Equals("left hand"))
        {
            LatestLeftHand.StartIkCoroutine(ID, InsertObject);
            if(exiting) LatestLeftHand.Exiting(ID, duration);
        }
        else if (ID == "RightHand" || ID.Equals("righthand") || ID.Equals("Right Hand") || ID.Equals("right hand"))
        {
            LatestRightHand.StartIkCoroutine(ID, InsertObject);
            if (exiting) LatestRightHand.Exiting(ID, duration);
        }
        else if (ID == "LeftFoot" || ID.Equals("leftfoot") || ID.Equals("Left Foot") || ID.Equals("left foot"))
        {
            LatestLeftFoot.StartIkCoroutine(ID, InsertObject);
            if (exiting) LatestLeftFoot.Exiting(ID, duration);
        }
        else if (ID == "RightFoot" || ID.Equals("rightfoot") || ID.Equals("Right Foot") || ID.Equals("right foot"))
        {
            LatestRightFoot.StartIkCoroutine(ID, InsertObject);
            if (exiting) LatestRightFoot.Exiting(ID, duration);
        }
        else if (ID == "Body" || ID.Equals("body"))
        {
            LatestBody.StartIkCoroutine(ID, InsertObject);
            if (exiting) LatestBody.Exiting(ID, duration);
        }
        else
        {
            Debug.LogError("We could not find the specified effector!");
        }
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

    public void InitiateLadderIK(GameObject[] Rungs)
    {
        LadderRungs = Rungs;
    }

    public void MountLadderIK(bool side, bool ZLadder)
    {
        if (!ZLadder)
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
            ForceEffectorWeight("LeftHand", 1.0f);
            ForceEffectorWeight("RightHand", 1.0f);
            ForceEffectorWeight("LeftFoot", 1.0f);
            ForceEffectorWeight("RightFoot", 1.0f);

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
            ShutdownComplete = false;
        }
    }

    private GameObject[] FindClosestRungs()
    {
        float ShoulderDistance = 1000.0f;
        float FootDistance = 1000.0f;
        GameObject ShoulderRung = null;
        GameObject FootRung = null;
        int TopValue = 0;
        int BotValue = 0;
        for (int i = 0; i < LadderRungs.Length; ++i)
        {
            if ((Ik.solver.leftArmMapping.bone1.position - LadderRungs[i].transform.position).magnitude < ShoulderDistance)
            {
                ShoulderDistance = (Ik.solver.leftArmMapping.bone1.position - LadderRungs[i].transform.position).magnitude;
                ShoulderRung = LadderRungs[i];
                HandRungs[0] = i;
                HandRungs[1] = i;
                TopValue = i;
            }
            if ((Ik.solver.leftLegMapping.bone3.position - LadderRungs[i].transform.position).magnitude < FootDistance)
            {
                FootDistance = (Ik.solver.leftLegMapping.bone3.position - LadderRungs[i].transform.position).magnitude;
                FootRung = LadderRungs[i];
                FeetRungs[0] = i;
                FeetRungs[1] = i;
                BotValue = i;
            }
            if ((TopValue - BotValue) >= 5)
            {
                ShoulderRung = LadderRungs[TopValue - 1];
                HandRungs[0] = TopValue - 1;
                HandRungs[1] = TopValue - 1;
            }
            if ((TopValue - BotValue) <= 2)
            {
                ShoulderRung = LadderRungs[TopValue + 1];
                HandRungs[0] = TopValue + 1;
                HandRungs[1] = TopValue + 1;
            }
        }
        LadderSlope = ShoulderRung.transform.position - FootRung.transform.position;
        InitiationComplete = true;
        return new GameObject[] { ShoulderRung, FootRung };
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
        if (LadderMounted && !DisableUp)
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
        if (LadderMounted && !DisableDown)
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
            if (InitiationComplete && !ShutdownComplete)
            {
                if (LastCycleDirection != Direction)
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

            }
            yield return new WaitForSeconds(Period);
        }
    }

    public void LoadClimbingData()
    {
        LoadIKSetting(ClimbingData);
    }

    public void LoadDraggingData()
    {
        LoadIKSetting(DraggingData);
    }

    private void LoadIKSetting(SCR_IKSettingData Data)
    {
        Ik.solver.bodyEffector.positionWeight = Data.BodyEffectorPositionWeight;
        Ik.solver.spineStiffness = Data.SpineStifness;
        Ik.solver.pullBodyVertical = Data.BodyPullVertical;
        Ik.solver.pullBodyHorizontal = Data.BodyPullHorizontal;
        Ik.solver.spineMapping.twistWeight = Data.SpineTwistWeight;
        Ik.solver.headMapping.maintainRotationWeight = Data.HeadMaintainRot;

        Ik.solver.leftHandEffector.maintainRelativePositionWeight = Data.LeftHandMaintainRelativePos;
        Ik.solver.leftArmChain.pull = Data.LeftArmPull;
        Ik.solver.leftArmChain.reach = Data.LeftArmReach;
        Ik.solver.leftArmChain.push = Data.LeftArmPush;
        Ik.solver.leftArmChain.pushParent = Data.LeftArmPushParent;
        Ik.solver.leftArmMapping.maintainRotationWeight = Data.LeftArmMaintainRelativeRot;
        Ik.solver.leftArmChain.bendConstraint.weight = Data.LeftArmBendGoal;

        Ik.solver.rightHandEffector.maintainRelativePositionWeight = Data.RightHandMaintainRelativePos;
        Ik.solver.rightArmChain.pull = Data.RightArmPull;
        Ik.solver.rightArmChain.reach = Data.RightArmReach;
        Ik.solver.rightArmChain.push = Data.RightArmPush;
        Ik.solver.rightArmChain.pushParent = Data.RightArmPushParent;
        Ik.solver.rightArmMapping.maintainRotationWeight = Data.RightArmMaintainRelativeRot;
        Ik.solver.rightArmChain.bendConstraint.weight = Data.RightArmBendGoal;

        Ik.solver.leftFootEffector.maintainRelativePositionWeight = Data.LeftFootMaintainRelativePos;
        Ik.solver.leftLegChain.pull = Data.LeftLegPull;
        Ik.solver.leftLegChain.reach = Data.LeftLegReach;
        Ik.solver.leftLegChain.push = Data.LeftLegPush;
        Ik.solver.leftLegChain.pushParent = Data.LeftLegPushParent;
        Ik.solver.leftLegMapping.maintainRotationWeight = Data.LeftLegMaintainRelativeRot;

        Ik.solver.rightFootEffector.maintainRelativePositionWeight = Data.RightFootMaintainRelativePos;
        Ik.solver.rightLegChain.pull = Data.RightLegPull;
        Ik.solver.rightLegChain.reach = Data.RightLegReach;
        Ik.solver.rightLegChain.push = Data.RightLegPush;
        Ik.solver.rightLegChain.pushParent = Data.RightLegPushParent;
        Ik.solver.rightLegMapping.maintainRotationWeight = Data.RightLegMaintainRelativeRot;
    }

    private void MoveHands()
    {
        if (Direction)
        {
            switch (ClimbState)
            {
                case 0:
                    if (HandRungs[0] == LadderRungs.Length - 1)
                    {
                        if(HandRungs[1] == LadderRungs.Length - 1)
                        {
                            DisableUp = true;
                            Still();
                        }
                        else
                        {
                            Vector3 From = FindOffsetPoints(LadderRungs[HandRungs[1]], false, false)[1];
                            HandRungs[1] = HandRungs[0];
                            Vector3 To = FindOffsetPoints(LadderRungs[HandRungs[1]], false, false)[1];
                            LadderSlope = To - From;
                            StartCoroutine(LerpLocation(From, To, "RightHand"));
                            StartEffectorLerp("RightHand", LadderTransition, Period);
                        }
                    }
                    else
                    {
                        Vector3 From = FindOffsetPoints(LadderRungs[HandRungs[1]], false, false)[1];
                        HandRungs[1] = HandRungs[0] + 1;
                        Vector3 To = FindOffsetPoints(LadderRungs[HandRungs[1]], false, false)[1];
                        LadderSlope = To - From;
                        StartCoroutine(LerpLocation(From, To, "RightHand"));
                        StartEffectorLerp("RightHand", LadderTransition, Period);
                      
                    }
                    break;
                case 1:
                    if (!(FeetRungs[1] == LadderRungs.Length - 1))
                    {
                        Vector3 From = FindOffsetPoints(LadderRungs[FeetRungs[1]], true, false)[1];
                        FeetRungs[1] = FeetRungs[0] + 1;
                        Vector3 To = FindOffsetPoints(LadderRungs[FeetRungs[1]], true, false)[1];
                        LadderSlope = To - From;
                        StartCoroutine(LerpLocation(From, To, "RightFoot"));
                        StartEffectorLerp("RightFoot", LadderTransition, Period);
                        DisableDown = false;

                    }
                    break;
                case 2:
                    if (HandRungs[1] == LadderRungs.Length - 1)
                    {
                        if (HandRungs[0] == LadderRungs.Length - 1)
                        {
                            DisableUp = true;
                            Still();
                        }
                        else
                        {
                            Vector3 From = FindOffsetPoints(LadderRungs[HandRungs[0]], false, false)[0];
                            HandRungs[0] = HandRungs[1];
                            Vector3 To = FindOffsetPoints(LadderRungs[HandRungs[0]], false, false)[0];
                            LadderSlope = To - From;
                            StartCoroutine(LerpLocation(From, To, "LeftHand"));
                            StartEffectorLerp("Lefthand", LadderTransition, Period);
                        }
                    }
                    else
                    {
                        Vector3 From = FindOffsetPoints(LadderRungs[HandRungs[0]], false, false)[0];
                        HandRungs[0] = HandRungs[1] + 1 ;
                        Vector3 To = FindOffsetPoints(LadderRungs[HandRungs[0]], false, false)[0];
                        LadderSlope = To - From;
                        StartCoroutine(LerpLocation(From, To, "LeftHand"));
                        StartEffectorLerp("Lefthand", LadderTransition, Period);

                    }
                    break;
                case 3:
                    if (!(FeetRungs[0] == LadderRungs.Length - 1))
                    {
                        Vector3 From = FindOffsetPoints(LadderRungs[FeetRungs[0]], true, false)[0];
                        FeetRungs[0] = FeetRungs[1] + 1;
                        Vector3 To = FindOffsetPoints(LadderRungs[FeetRungs[0]], true, false)[0];
                        LadderSlope = To - From;
                        StartCoroutine(LerpLocation(From, To, "LeftFoot"));
                        StartEffectorLerp("LeftFoot", LadderTransition, Period);
                        DisableDown = false;
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
                    if (!(HandRungs[1] == 0)) 
                    {
                        Vector3 From = FindOffsetPoints(LadderRungs[HandRungs[1]], false, false)[1];
                        HandRungs[1] = HandRungs[0] - 1;
                        Vector3 To = FindOffsetPoints(LadderRungs[HandRungs[1]], false, false)[1];
                        LadderSlope = From - To;
                        StartCoroutine(LerpLocation(From, To, "RightHand"));
                        StartEffectorLerp("RightHand", LadderTransition, Period);
                        DisableUp = false;

                    }
                    break;
                case 1:
                    if (FeetRungs[0] == 0)
                    {
                        if(FeetRungs[1] == 0)
                        {
                            DisableDown = true;
                            Still();
                        }
                        else
                        {
                            Vector3 From = FindOffsetPoints(LadderRungs[FeetRungs[1]], true, false)[1];
                            FeetRungs[1] = FeetRungs[0];
                            Vector3 To = FindOffsetPoints(LadderRungs[FeetRungs[1]], true, false)[1];
                            LadderSlope = From - To;
                            StartCoroutine(LerpLocation(From, To, "RightFoot"));
                            StartEffectorLerp("RightFoot", LadderTransition, Period);
                            StartEffectorLerp("LeftFoot", LadderTransition, Period);
                        }
                    }
                    else
                    {
                        Vector3 From = FindOffsetPoints(LadderRungs[FeetRungs[1]], true, false)[1];
                        FeetRungs[1] = FeetRungs[0] - 1;
                        Vector3 To = FindOffsetPoints(LadderRungs[FeetRungs[1]], true, false)[1];
                        LadderSlope = From - To;
                        StartCoroutine(LerpLocation(From, To, "RightFoot"));
                        StartEffectorLerp("RightFoot", LadderTransition, Period);
                        StartEffectorLerp("LeftFoot", LadderTransition, Period);
                    }
                    break;
                case 2:
                    if (!(HandRungs[0] == 0)) 
                    {
                        Vector3 From = FindOffsetPoints(LadderRungs[HandRungs[0]], false, false)[0];
                        HandRungs[0] = HandRungs[1] - 1;
                        Vector3 To = FindOffsetPoints(LadderRungs[HandRungs[0]], false, false)[0];
                        LadderSlope = From - To;
                        StartCoroutine(LerpLocation(From, To, "LeftHand"));
                        StartEffectorLerp("Lefthand", LadderTransition, Period);
                        DisableUp = false;

                    }
                    break;
                case 3:
                    if (FeetRungs[1] == 0)
                    {
                        if (FeetRungs[0] == 0)
                        {
                            DisableDown = true;
                            Still();
                        }
                        else
                        {
                            Vector3 From = FindOffsetPoints(LadderRungs[FeetRungs[0]], true, false)[0];
                            FeetRungs[0] = FeetRungs[1];
                            Vector3 To = FindOffsetPoints(LadderRungs[FeetRungs[0]], true, false)[0];
                            LadderSlope = From - To;
                            StartCoroutine(LerpLocation(From, To, "LeftFoot"));
                            StartEffectorLerp("LeftFoot", LadderTransition, Period);
                        }
                    }
                    else
                    {
                        Vector3 From = FindOffsetPoints(LadderRungs[FeetRungs[0]], true, false)[0];
                        FeetRungs[0] = FeetRungs[1] -1;
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

    public void FlushIk()
    {
        ShutdownComplete = true;
        DisableUp = false;
        DisableDown = false;
        InitiationComplete = false;
        ForceEffectorWeight("LeftHand", 0.0f);
        ForceEffectorWeight("RightHand", 0.0f);
        ForceEffectorWeight("LeftFoot", 0.0f);
        ForceEffectorWeight("RightFoot", 0.0f);
        ForceEffectorWeight("Body", 0.0f);
        SetEffectorTarget("LeftHand", null);
        SetEffectorTarget("RightHand", null);
        SetEffectorTarget("LeftFoot", null);
        SetEffectorTarget("RightFoot", null);
        SetEffectorTarget("Body", null);
    }
}
