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
    [HideIf("TransformX", false)]
    private float XValue;
    [SerializeField]
    [HideIf("TransformY", false)]
    private float YValue;
    [SerializeField]
    [HideIf("TransformZ", false)]
    private float ZValue;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
