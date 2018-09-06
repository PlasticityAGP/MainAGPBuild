using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_Elevator : MonoBehaviour {
    public bool StartsDown;
    public float distance;
    public float speed;
    bool actuated;
    float start;
    float end;

	// Use this for initialization
	void Start () {
        if (StartsDown) {
            actuated = false;
            start = this.transform.position.y;
            end = start + distance;
        }
        else
        {
            actuated = true;
            end = this.transform.position.y;
            start = end - distance;
        }
        
	}
	
	// Update is called once per frame
	void Update () {
        if (actuated && this.transform.position.y <= end)
        {
            this.transform.y += distance * speed * Time.deltaTime;
        }
        else if (!actuated && this.transform.position.y >= start)
        {
            this.transform.y -= distance * speed * Time.deltaTime;
        }
	}

    // Public function to make the elevator rise
    public void Up()
    {
        actuated = true;
    }

    // Public function to make the elevator sink
    public void Down()
    {
        actuated = false;
    }
}
