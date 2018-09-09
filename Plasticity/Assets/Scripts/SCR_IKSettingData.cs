using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "IKSettingData", menuName = "IK Setting", order = 2)]
public class SCR_IKSettingData : ScriptableObject
{
    [Title("Head and Body")]
    public float BodyEffectorPositionWeight;
    public float SpineStifness;
    public float BodyPullVertical;
    public float BodyPullHorizontal;
    public float SpineTwistWeight;
    public float HeadMaintainRot;
    [Title("Left Arm")]
    public float LeftHandMaintainRelativePos;
    public float LeftArmPull;
    public float LeftArmReach;
    public float LeftArmPush;
    public float LeftArmPushParent;
    public float LeftArmMaintainRelativeRot;
    public float LeftArmBendGoal;
    [Title("Right Arm")]
    public float RightHandMaintainRelativePos;
    public float RightArmPull;
    public float RightArmReach;
    public float RightArmPush;
    public float RightArmPushParent;
    public float RightArmMaintainRelativeRot;
    public float RightArmBendGoal;
    [Title("Left Leg")]
    public float LeftFootMaintainRelativePos;
    public float LeftLegPull;
    public float LeftLegReach;
    public float LeftLegPush;
    public float LeftLegPushParent;
    public float LeftLegMaintainRelativeRot;
    [Title("Right Leg")]
    public float RightFootMaintainRelativePos;
    public float RightLegPull;
    public float RightLegReach;
    public float RightLegPush;
    public float RightLegPushParent;
    public float RightLegMaintainRelativeRot;
}
