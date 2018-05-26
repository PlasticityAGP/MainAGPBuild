using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_ZJump : MonoBehaviour {

    [SerializeField]
    [Tooltip("A reference point whose z value will be used to move the player in the z axis")]
    private GameObject ReferencePoint;
    private Vector3 NewPosition;
    private float LerpValue;
    private Vector3 InitialPosition;
    private bool LerpBack;
    private GameObject LeavingObj;



	// Use this for initialization
	void Start () {

        LerpValue = 0.0f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Character")
        {
            InitialPosition = other.gameObject.transform.position;
            LerpValue = 0.0f;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject.tag == "Character")
        {
            Vector3 A = other.transform.position - gameObject.transform.position;
            Vector3 B = ReferencePoint.transform.position - gameObject.transform.position;
            if(Vector3.Dot(A, B.normalized) > B.magnitude)
            {
                LerpValue += (Time.deltaTime * 2.0f);
                if (LerpValue >= 1.0f) LerpValue = 1.0f;
                float ZComp = Mathf.Lerp(other.gameObject.transform.position.z, ReferencePoint.transform.position.z, LerpValue);
                Vector3 FinalPosition = new Vector3(other.gameObject.transform.position.x, other.gameObject.transform.position.y, ZComp);
                other.gameObject.transform.position = FinalPosition;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Character")
        {
            LerpBack = true;
            LerpValue = 0.0f;
            LeavingObj = other.gameObject;
        }
    }

    private void ReturnLerp(float DeltaTime)
    {
        LerpValue += (Time.deltaTime * 2.0f);
        if (LerpValue >= 1.0f)
        {
            LerpBack = false;
            LerpValue = 0.0f;
        }
        else
        {
            float ZComp = Mathf.Lerp(LeavingObj.transform.position.z, InitialPosition.z, LerpValue);
            Vector3 FinalPosition = new Vector3(LeavingObj.transform.position.x, LeavingObj.transform.position.y, ZComp);
            LeavingObj.transform.position = FinalPosition; 
        }
        
    }

    // Update is called once per frame
    void Update () {
        if (LerpBack) ReturnLerp(Time.deltaTime);
	}
}
