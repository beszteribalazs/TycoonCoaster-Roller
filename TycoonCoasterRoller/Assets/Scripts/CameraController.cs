using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Plane = UnityEngine.Plane;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class CameraController : MonoBehaviour
{
    public Transform cameraTransform;
    public float normalSpeed;
    public float fastSpeed;
    public float movementSpeed;
    public float movementTime;
    public float rotationAmount;
    public Vector3 zoomAmount;
    private Vector3 newPosition;
    private Quaternion newRotation;
    private Vector3 newZoom;
    public float maxZoom; 
    public float minZoom;

    private Vector3 dragStartPosition;
    private Vector3 dragCurrentPosition;

    void Start()
    {
        newPosition = transform.position;
        newRotation = transform.rotation;
        newZoom = cameraTransform.localPosition;
    }
    
    void LateUpdate()
    {
        HandleMovementInput();
        HandleMouseInput();
    }

    
    void HandleMouseInput()
    {
        //scroll
        if(Input.mouseScrollDelta.y!=0)
        {
            if(Input.mouseScrollDelta.y>0 && newZoom.z < maxZoom)
            {
                newZoom += Input.mouseScrollDelta.y * zoomAmount;
            }
            if (Input.mouseScrollDelta.y < 0 && newZoom.z > minZoom)
            {
                newZoom += Input.mouseScrollDelta.y * zoomAmount;
            }
        }
        
        //drag
        if (Input.GetMouseButtonDown(2))
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            float entry;
            if (plane.Raycast(ray, out entry))
            {
                dragStartPosition = ray.GetPoint(entry);
            }
        }
        if (Input.GetMouseButton(2))
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
        
        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition,newZoom,Time.deltaTime*movementTime);
    }

    void HandleMovementInput()
    {
        //fast-normal speed
        if (Input.GetKey(KeyCode.LeftControl))
        {
            movementSpeed = fastSpeed;
        }
        else
        {
            movementSpeed = normalSpeed;
        }

        Vector3 helpVector = transform.TransformPoint(newPosition);
        //movement
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            if (((GameManager.instance.Height*3)+20) > helpVector.z)
            {
                newPosition += (transform.forward * movementSpeed);
            }
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            if (-10 < helpVector.z)
            {
                newPosition += (transform.forward * -movementSpeed);
            }
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            if (((GameManager.instance.Width*3)+40) > helpVector.x)
            {
                newPosition += (transform.right * movementSpeed);
            }
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            if (-20 < helpVector.x)
            {
                newPosition += (transform.right * -movementSpeed);
            }
        }
        
        //rotation
        if (Input.GetKey(KeyCode.Q))
        {
            newRotation *= Quaternion.Euler(Vector3.up*rotationAmount);
        }
        if (Input.GetKey(KeyCode.E))
        {
            newRotation *= Quaternion.Euler(Vector3.up*-rotationAmount);
        }
        
        transform.position = Vector3.Lerp(transform.position,newPosition,Time.deltaTime*movementTime);
        transform.rotation = Quaternion.Lerp(transform.rotation,newRotation,Time.deltaTime*movementTime);
    }
    
}
