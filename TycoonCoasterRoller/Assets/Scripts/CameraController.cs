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
    
    public Vector3 newPosition;
    public Quaternion newRotation;
    public Vector3 newZoom;
    
    void Start()
    {
        newPosition = transform.position;
        newRotation = transform.rotation;
        newZoom = cameraTransform.localPosition;
    }
    
    void LateUpdate()
    {
        HandleMovementInput();
    }
    
    void HandleMovementInput()
    {
        //fast-normal speed
        if (Input.GetKey(KeyCode.LeftShift))
        {
            movementSpeed = fastSpeed;
        }
        else
        {
            movementSpeed = normalSpeed;
        }
        
        //movement
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            newPosition += (transform.forward * movementSpeed);
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            newPosition += (transform.forward * -movementSpeed);
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            newPosition += (transform.right * movementSpeed);
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            newPosition += (transform.right * -movementSpeed);
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
        
        //scroll
        if (Input.mouseScrollDelta.y != 0)
        {
            newZoom += Input.mouseScrollDelta.y * zoomAmount;
        }
        
        
        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition,newZoom,Time.deltaTime*movementTime);
        transform.position = Vector3.Lerp(transform.position,newPosition,Time.deltaTime*movementTime);
        transform.rotation = Quaternion.Lerp(transform.rotation,newRotation,Time.deltaTime*movementTime);
    }
    
}
