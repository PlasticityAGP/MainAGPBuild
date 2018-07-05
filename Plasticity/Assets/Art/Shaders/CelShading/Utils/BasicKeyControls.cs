using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicKeyControls : MonoBehaviour {

    public float moveSpeed = 1;
    public float rotateSpeed = 1;
    public float sprintBoost = 2;
    public bool sprintChangesRotationToo = true;

    void Start()
    {
        moveSpeed *= Time.deltaTime;
    }

	void Update () {
        float tempMoveSpeed = moveSpeed;
        float tempRotateSpeed = rotateSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            tempMoveSpeed *= sprintBoost;
            if (sprintChangesRotationToo)
                tempRotateSpeed *= sprintBoost;
        }

        if (Input.GetKey(KeyCode.W)) transform.Translate(0, 0, tempMoveSpeed);
        if (Input.GetKey(KeyCode.S)) transform.Translate(0, 0, -tempMoveSpeed);
        if (Input.GetKey(KeyCode.D)) transform.Translate(tempMoveSpeed, 0, 0);
        if (Input.GetKey(KeyCode.A)) transform.Translate(-tempMoveSpeed, 0, 0);
        if (Input.GetKey(KeyCode.Space)) transform.Translate(0, tempMoveSpeed, 0);
        if (Input.GetKey(KeyCode.LeftControl)) transform.Translate(0, -tempMoveSpeed, 0);

        if (Input.GetKey(KeyCode.RightArrow)) transform.Rotate(0, tempRotateSpeed, 0, Space.World);
        if (Input.GetKey(KeyCode.LeftArrow)) transform.Rotate(0, -tempRotateSpeed, 0, Space.World);
        if (Input.GetKey(KeyCode.UpArrow)) transform.Rotate(-tempRotateSpeed, 0, 0);
        if (Input.GetKey(KeyCode.DownArrow)) transform.Rotate(tempRotateSpeed, 0, 0);


    }
}
