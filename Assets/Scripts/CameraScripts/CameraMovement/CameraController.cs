using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float normalSpeed;
    public float fastSpeed;
    private float movementSpeed;
    public float movementTime;
    public float rotationAmount;

    public Vector3 newPosition;
    public Vector3 newZoom;
    public Vector3 zoomAmount;
    public Quaternion newRotation;

    public Transform cameraTransform;


    public Vector3 dragStartPosition;
    public Vector3 dragCurrentPosition;
    public Vector3 rotateStartPosition;
    public Vector3 rotateCurrentPosition;

    private float leftCameraLimit;
    private float rightCameraLimit;
    private float topCameraLimit;
    private float bottomCameraLimit;


    private Vector3 maxZoom = new Vector3(0, 3, -3);
    private Vector3 minZoom = new Vector3(0, 1, -1);
    void Start()
    {
        newPosition = transform.position;
        newRotation = transform.rotation;
        newZoom = cameraTransform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        
        HandleMovementInput();
        HandleMouseInput();
        checkIfCameraPositionIsOutsideBundary();

        if (newZoom.y >= maxZoom.y && newZoom.z <= maxZoom.z)
        {
            newZoom = maxZoom;
        }
        if (newZoom.y <= minZoom.y && newZoom.z >= minZoom.z)
        {
            newZoom = minZoom;
        }
    }

    void HandleMovementInput()
    {
        if (Input.GetKey(KeyCode.LeftShift)) {
            movementSpeed = fastSpeed;
        }
        else
        {
            movementSpeed = normalSpeed;
        }
        
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            newPosition += transform.forward * movementSpeed;
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            newPosition += transform.forward * -movementSpeed;
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            newPosition += transform.right * movementSpeed;
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            newPosition += transform.right * -movementSpeed;
        }

        if (Input.GetKey(KeyCode.Q))
        {
            newRotation *= Quaternion.Euler(Vector3.up * rotationAmount);
        }
        if (Input.GetKey(KeyCode.E))
        {
            newRotation *= Quaternion.Euler(Vector3.up * -rotationAmount);
        }
        if (Input.GetKey(KeyCode.R))
        {
            newZoom += zoomAmount;

           
        }
        if (Input.GetKey(KeyCode.F))
        {
            newZoom -= zoomAmount;
        }

        transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * movementTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, Time.deltaTime * movementTime);
        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, newZoom, Time.deltaTime * movementTime);

       
    }

    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(2))
        {
            rotateStartPosition = Input.mousePosition;
        }
        if (Input.GetMouseButton(2))
        {
            rotateCurrentPosition = Input.mousePosition;
            Vector3 diff = rotateCurrentPosition - rotateStartPosition;

            rotateStartPosition = rotateCurrentPosition;

            newRotation *= Quaternion.Euler(Vector3.up * (-diff.x / 5f));
        }

        if (Input.mouseScrollDelta.y != 0)
        {
            newZoom += Input.mouseScrollDelta.y * zoomAmount;
        }
        
        if (Input.GetMouseButtonDown(1))
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            float entry;

            if (plane.Raycast(ray, out entry))
            {
                dragStartPosition = ray.GetPoint(entry);
            }
        }
        if (Input.GetMouseButton(1))
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            float entry;

            if (plane.Raycast(ray, out entry))
            {
                dragCurrentPosition = ray.GetPoint(entry);

                newPosition = transform.position + dragStartPosition - dragCurrentPosition;
            }
        }
        transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * movementTime);
    }

    public void setCameraLimits((float, float) xmin_ymin, (float, float) xmin_ymax, (float, float) xmax_ymax, (float, float) xmax_ymin)
    {
        leftCameraLimit = Min(xmin_ymin, xmin_ymax).Item1;
        rightCameraLimit = Max(xmax_ymax, xmax_ymin).Item1;
        topCameraLimit = xmin_ymax.Item2;
        bottomCameraLimit = Min(xmin_ymin, xmax_ymin, true).Item2;

        Debug.LogFormat("leftCameraLimit {0}", leftCameraLimit);
        Debug.LogFormat("rightCameraLimit {0}", rightCameraLimit);
        Debug.LogFormat("topCameraLimit {0}", topCameraLimit);
        Debug.LogFormat("bottomCameraLimit {0}", bottomCameraLimit);
    }

    private (float, float) Min((float, float) a, (float, float) b, bool axe = false) //False - x, True - y
    {
        switch(axe)
        {
            case false:
                {
                    return a.Item1 < b.Item1 ? a : b; 
                }
            case true:
                {
                    return a.Item2 < b.Item2 ? a : b;
                }
        }
    }
    private (float, float) Max((float, float) a, (float, float) b, bool axe = false) //False - x, True - y
    {
        switch (axe)
        {
            case false:
                {
                    return a.Item1 > b.Item1 ? a : b;
                }
            case true:
                {
                    return a.Item2 > b.Item2 ? a : b;
                }
        }
    }

    private void checkIfCameraPositionIsOutsideBundary()
    {
        if (transform.position.x < leftCameraLimit)
        {
            newPosition = new Vector3(leftCameraLimit, transform.position.y, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * movementTime);
        }
        if (transform.position.x > rightCameraLimit)
        {
            newPosition = new Vector3(rightCameraLimit, transform.position.y, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * movementTime);
        }

        if (transform.position.z > topCameraLimit)
        {
            newPosition = new Vector3(transform.position.x, transform.position.y, topCameraLimit);
            transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * movementTime);
        }
        if (transform.position.z < bottomCameraLimit)
        {
            newPosition = new Vector3(transform.position.x, transform.position.y, bottomCameraLimit);
            transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * movementTime);
        }
    }

    public void setCameraStartPosition()
    {
        float center_x = rightCameraLimit + (rightCameraLimit - leftCameraLimit) / 2;
        float center_y = topCameraLimit - (topCameraLimit - bottomCameraLimit) / 2;

        transform.position = new Vector3(center_x, transform.position.y, center_y);

        newZoom = maxZoom;
    }
}
