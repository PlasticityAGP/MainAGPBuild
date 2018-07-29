using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class SCR_TransformTrigger : MonoBehaviour {

    [SerializeField]
    private bool TransformX;
    [SerializeField]
    private bool TransformY;
    [SerializeField]
    private bool TransformZ;
    [SerializeField]
    [ShowIf("TransformX")]
    private float XPosition;
    [SerializeField]
    [ShowIf("TransformY")]
    private float YPosition;
    [SerializeField]
    [ShowIf("TransformZ")]
    private float ZPosition;
    [SerializeField]
    [ShowIf("TransformX")]
    private float XLerpSpeed;
    [SerializeField]
    [ShowIf("TransformY")]
    private float YLerpSpeed;
    [SerializeField]
    [ShowIf("TransformZ")]
    private float ZLerpSpeed;
    private bool LerpBool;
    SCR_CharacterManager Manager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Character")
        {
            Manager = other.GetComponentInChildren<SCR_CharacterManager>();
            LerpBool = true;
        }
    }


    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (LerpBool) DoLerp(Time.deltaTime);
	}

    private void DoLerp(float DeltaTime)
    {
        float MagX = XPosition - Manager.gameObject.transform.position.x;
        float MagY = YPosition - Manager.gameObject.transform.position.y;
        float MagZ = ZPosition - Manager.gameObject.transform.position.z;
        bool DoneX = false;
        bool DoneY = false;
        bool DoneZ = false;
        if(Mathf.Abs(MagX) < 0.01f) DoneX = true;
        else
        {
            if (TransformX)
            {
                float Dir = MagX / Mathf.Abs(MagX);
                Vector3 NewPos = Manager.gameObject.transform.position;
                NewPos.x += Dir * XLerpSpeed * DeltaTime;
                Manager.gameObject.transform.position = NewPos;
            }
            else DoneX = true;
        }
        if (Mathf.Abs(MagY) < 0.01f) DoneY = true;
        else
        {
            if (TransformY)
            {
                float Dir = MagY / Mathf.Abs(MagY);
                Vector3 NewPos = Manager.gameObject.transform.position;
                NewPos.y += Dir * YLerpSpeed * DeltaTime;
                Manager.gameObject.transform.position = NewPos;
            }
            else DoneY = true;
        }
        if (Mathf.Abs(MagZ) < 0.01f) DoneZ = true;
        else
        {
            if (TransformZ)
            {
                float Dir = MagZ / Mathf.Abs(MagZ);
                Vector3 NewPos = Manager.gameObject.transform.position;
                NewPos.z += Dir * ZLerpSpeed * DeltaTime;
                Manager.gameObject.transform.position = NewPos;
            }
            else DoneZ = true;
        }
        if(DoneX && DoneY && DoneZ)
        {
            LerpBool = false;
        }
    }
}
