using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LerpBetweenPoints : MonoBehaviour {

    private Vector3 startingPosition;
    public Transform otherPosition;

    public bool canPause = false;
    public KeyCode pauseKey = KeyCode.P;
    private bool isPaused = false;

    public float speed = 1;
    private float myTime = 0;
    public bool canChangeSpeed = false;
    public float speedChangeAmount = 0.2f;
    public KeyCode speedUpKey = KeyCode.RightBracket;
    public KeyCode speedDownKey = KeyCode.LeftBracket;

    private void Start()
    {
        startingPosition = transform.position;
        if (otherPosition == null)
            otherPosition = GetComponentInChildren<Transform>();
        otherPosition.parent = null;
    }

    private void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            isPaused = !isPaused;
        }


        if(canChangeSpeed)
        {
            if(Input.GetKeyDown(speedUpKey))
            {
                speed += speedChangeAmount;
            }
            if(Input.GetKeyDown(speedDownKey))
            {
                speed -= speedChangeAmount;
            }
        }

        if (!isPaused || !canPause)
        {
            transform.position = Vector3.Lerp(startingPosition, otherPosition.position, Mathf.PingPong(myTime, 1.0f));
            myTime += Time.deltaTime * speed;
        }
    }

}
