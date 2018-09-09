using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_Elevator : MonoBehaviour {
    [Tooltip("Which floor will the player start on? 1 is the lowest floor, and the highest floor is the same as numFloors")]
    public int startFloor;
    public int numFloors;
    public float distanceBetweenFloors;
    [Tooltip("The time taken to move one full floor is about 1/speed seconds")]
    public float speed;
    bool actuatedUp;
    bool actuatedDown;
    float[] floors;
    float nextFloor;
    float currentHeight;
    int currentFloor;

    // -1 for down, 0 for stationary, 1 for up
    int direction;

	// Use this for initialization
	void Start () {
        // Makes sure the program doesn't break
        if (startFloor < 1) startFloor = 1;
        if (startFloor > numFloors) startFloor = numFloors;
        currentFloor = startFloor - 1;

        currentHeight = this.transform.position.y;

        direction = 0;
        actuatedUp = false;
        actuatedDown = false;

        floors = new float[numFloors];
        floors[startFloor - 1] = currentHeight;
        nextFloor = floors[startFloor - 1];

        // Determining the height of the floors
        for (int i = 0; i < startFloor; i++)
            floors[i] = floors[startFloor - 1] + (i - (startFloor - 1)) * distanceBetweenFloors;
	}
	
	// Update is called once per frame
	void Update () {
        // Stop if they reach the top or bottom.
        if(currentHeight >= floors[numFloors - 1])
            if (!actuatedDown) direction = 0;
        if(currentHeight <= floors[0])
            if (!actuatedUp) direction = 0;

        // Going up
        if(direction == 1)
        {
            if (actuatedUp || currentHeight < nextFloor)
                this.transform.Translate(new Vector3(0, distanceBetweenFloors * speed * Time.deltaTime, 0));
            else if (currentHeight >= nextFloor)
                direction = 0;
        }

        // Going down
        if(direction == -1)
        {
            if (actuatedUp || currentHeight > nextFloor)
                this.transform.Translate(new Vector3(0, -1 * distanceBetweenFloors * speed * Time.deltaTime, 0));
            else if (currentHeight <= nextFloor)
                direction = 0;
        }

        currentHeight = this.transform.position.y;
        DetermineNextFloor();
	}

    // Configures the nextFloor variable
    void DetermineNextFloor()
    {
        if (direction == 1 && actuatedUp)
            if (currentHeight >= nextFloor)
            {
                currentFloor++;
                nextFloor = floors[currentFloor];
            }

        if (direction == -1 && actuatedDown)
            if (currentHeight <= nextFloor)
            {
                currentFloor--;
                nextFloor = floors[currentFloor];
            }
    }

    // Public function to make the elevator rise
    public void Up()
    {
        actuatedUp = true;
        actuatedDown = false;
        if(currentFloor < numFloors - 1)
        {
            direction = 1;
            currentFloor++;
            nextFloor = floors[currentFloor];
        }
    }

    // Public function to make the elevator sink
    public void Down()
    {
        actuatedDown = true;
        actuatedUp = false;
        if (currentFloor > 0)
        {
            direction = -1;
            currentFloor--;
            nextFloor = floors[currentFloor];
        }
    }

    public void Neutral()
    {
        actuatedUp = false;
        actuatedDown = false;
    }
}
