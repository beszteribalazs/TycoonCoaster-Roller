using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Plane = UnityEngine.Plane;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class CameraController : MonoBehaviour
{
    [SerializeField] Transform cameraTransform;
    [SerializeField] float normalSpeed;
    [SerializeField] float fastSpeed;
    [SerializeField] float movementSpeed;
    [SerializeField] float movementTime;
    [SerializeField] float rotationAmount;
    [SerializeField] Vector3 zoomAmount;
    private Vector3 newPosition;
    private Quaternion newRotation;
    private Vector3 newZoom;
    [SerializeField] float maxZoom; 
    [SerializeField] float minZoom;
    [SerializeField] float limitMinX;
    [SerializeField] float limitMaxX;
    [SerializeField] float limitMinZ;
    [SerializeField] float limitMaxZ;

    private Vector3 dragStartPosition;
    private Vector3 dragCurrentPosition;

    void Start()
    {
        limitMinX = 0;
        limitMaxX = GameManager.instance.Width * 3;
        limitMinZ = 0;
        limitMaxZ = GameManager.instance.Width * 3;
        
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
            movementSpeed = fastSpeed*Time.deltaTime;
        }
        else
        {
            movementSpeed = normalSpeed*Time.deltaTime;
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
            newRotation *= Quaternion.Euler(Vector3.up * rotationAmount * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.E))
        {
            newRotation *= Quaternion.Euler(Vector3.up * -rotationAmount * Time.deltaTime);
        }
        
        transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * movementTime);
        transform.position = new Vector3(Mathf.Clamp(transform.position.x, limitMinX, limitMaxX), transform.position.y, Mathf.Clamp(transform.position.z, limitMinZ, limitMaxZ));
        newPosition = new Vector3(Mathf.Clamp(newPosition.x, limitMinX, limitMaxX), transform.position.y, Mathf.Clamp(newPosition.z, limitMinZ, limitMaxZ));
        
        transform.rotation = Quaternion.Lerp(transform.rotation,newRotation,Time.deltaTime * movementTime);
    }

}
