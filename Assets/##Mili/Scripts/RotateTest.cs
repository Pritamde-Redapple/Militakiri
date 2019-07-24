using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateTest : MonoBehaviour {


    public bool canClampYAngle = true;
    public bool canRotateXAngle = true;

    public Vector2 initialPos;
    public float speed = 3f;

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            initialPos =Input.mousePosition;
        }
        if(Input.GetMouseButton(0))
        {
            // Calculate(Input.mousePosition);
        }
        if(Input.GetMouseButtonUp(0))
        {

        }
    }

    public void Calculate(Vector3 finalPos)
    {
        float disX = Mathf.Abs(initialPos.x - finalPos.x);
        float disY = Mathf.Abs(initialPos.y - finalPos.y);

        Debug.Log(disX + " : " + disY);
        if (disX > 0 || disY > 0)
        {
            if (disX > disY)
            {
                if (initialPos.x > finalPos.x)
                {
                    RotateIt(new Vector3(0, -1, 0));
                }
                else
                {
                    RotateIt(new Vector3(0, 1, 0));
                }
            }
            else if (canRotateXAngle)
            {
                if (initialPos.y > finalPos.y)
                {
                    RotateIt(new Vector3(-1, 0, 0));
                }
                else
                {
                    RotateIt(new Vector3(1, 0, 0));
                }
            }
        }
    }

    void RotateIt(Vector3 dir)
    {
        this.transform.Rotate(dir * speed * Time.deltaTime);
       
        Vector3 angle = transform.localRotation.eulerAngles;

        angle.x = AngleClamp(angle.x, -25, 25);
        if (canClampYAngle) angle.y = AngleClamp(angle.y, -90, 90);  //(angle.y, -65, 65);
        angle.z = 0;

        transform.localRotation = Quaternion.Euler(angle);
    }

    float AngleClamp(float angle, float min, float max)
    {

        if (angle > 180)
            angle -= 360;
        angle = Mathf.Max(Mathf.Min(angle, max), min);
        if (angle < 0)
            angle += 360;
        return angle;
    }
}
