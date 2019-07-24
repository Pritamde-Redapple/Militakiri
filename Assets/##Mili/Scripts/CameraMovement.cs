using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour {

    public Camera cam;
    public Transform target;
    private static readonly float ZoomSpeedMouse = 4f;
    private static readonly float ZoomSpeedTouch = 0.01f;
    private static readonly float[] ZoomBounds = new float[] { 45f, 60f };

    private Vector3 lastPanPosition;
    private int panFingerId; // Touch mode only

    private bool wasZoomingLastFrame; // Touch mode only
    private Vector2[] lastZoomPositions; // Touch mode only


    private float xAngle; //angle for axes x for rotation
    private float yAngle = 60f;
    private float xAngTemp; //temp variable for angle
    private float yAngTemp;

    private void Start()
    {
        cam = GetComponent<Camera>();
    }

    private void Update()
    {
        if (Input.touchSupported && Application.platform != RuntimePlatform.WebGLPlayer)
        {
            HandleTouch();
        }
        else
        {
            HandleMouse();
        }

    }

    private void HandleTouch()
    {
        /* if (Input.touchCount > 0)
         {
             //Touch began, save position
             if (Input.GetTouch(0).phase == TouchPhase.Began)
             {
                 firstpoint = Input.GetTouch(0).position;
                 xAngTemp = xAngle;
                 yAngTemp = yAngle;
             }
             //Move finger by screen
             if (Input.GetTouch(0).phase == TouchPhase.Moved)
             {
                 secondpoint = Input.GetTouch(0).position;
                 //Mainly, about rotate camera. For example, for Screen.width rotate on 180 degree
                 xAngle = xAngTemp + (secondpoint.x - firstpoint.x) * 180.0f / Screen.width;
                 yAngle = yAngTemp - (secondpoint.y - firstpoint.y) * 90.0f / Screen.height;
                 //Rotate camera
                 this.transform.rotation = Quaternion.Euler(yAngle, xAngle, 0.0f);
             }
         }*/

        switch (Input.touchCount)
        {

            case 1: // Panning
                wasZoomingLastFrame = false;

                // If the touch began, capture its position and its finger ID.
                // Otherwise, if the finger ID of the touch doesn't match, skip it.
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    lastPanPosition = touch.position;
                    panFingerId = touch.fingerId;
                    xAngTemp = xAngle;
                    yAngTemp = yAngle;
                }
                else if (touch.fingerId == panFingerId && touch.phase == TouchPhase.Moved)
                {
                    RotateCamera(touch.position);
                }
                break;

            case 2: // Zooming
                Vector2[] newPositions = new Vector2[] { Input.GetTouch(0).position, Input.GetTouch(1).position };
                if (!wasZoomingLastFrame)
                {
                    lastZoomPositions = newPositions;
                    wasZoomingLastFrame = true;
                }
                else
                {
                    // Zoom based on the distance between the new positions compared to the 
                    // distance between the previous positions.
                    float newDistance = Vector2.Distance(newPositions[0], newPositions[1]);
                    float oldDistance = Vector2.Distance(lastZoomPositions[0], lastZoomPositions[1]);
                    float offset = newDistance - oldDistance;

                    ZoomCamera(offset, ZoomSpeedTouch);

                    lastZoomPositions = newPositions;
                }
                break;

            default:
                wasZoomingLastFrame = false;
                break;
        }
    }

    private void RotateCamera(Vector2 position)
    {
        Vector3 secondpoint = position;
        //Mainly, about rotate camera. For example, for Screen.width rotate on 180 degree
        xAngle = xAngTemp + (secondpoint.x - lastPanPosition.x) * 180.0f / Screen.width;
        yAngle = yAngTemp - (secondpoint.y - lastPanPosition.y) * 90.0f / Screen.height;
        //Rotate camera
        this.transform.rotation = Quaternion.Euler(yAngle, xAngle, 0.0f);
        transform.LookAt(target.position, Vector3.up);
    }

    void HandleMouse()
    {
        // On mouse down, capture it's position.
        // Otherwise, if the mouse is still down, pan the camera.
        if (Input.GetMouseButtonDown(0))
        {
            lastPanPosition = Input.mousePosition;
            xAngTemp = xAngle;
            yAngTemp = yAngle;
        }
        else if (Input.GetMouseButton(0))
        {
            RotateCamera(Input.mousePosition);
        }

        // Check for scrolling to zoom the camera
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        ZoomCamera(scroll, ZoomSpeedMouse);
    }

    void ZoomCamera(float offset, float speed)
    {
        if (offset == 0)
        {
            return;
        }

        cam.fieldOfView = Mathf.Clamp(cam.fieldOfView - (offset * speed), ZoomBounds[0], ZoomBounds[1]);
    }
}
