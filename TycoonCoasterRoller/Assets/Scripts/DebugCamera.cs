using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugCamera : MonoBehaviour{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float rotationSpeed = 5f;
    [SerializeField] float scrollSpeed = 1f;
    
    void LateUpdate(){
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        transform.position += Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized * (Time.deltaTime * horizontal * moveSpeed);
        transform.position += Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized * (Time.deltaTime * vertical * moveSpeed);

        transform.position += Vector3.up * (Input.mouseScrollDelta.y * (scrollSpeed * Time.deltaTime));
        
        if (Input.GetKey(KeyCode.Q)){
            transform.RotateAround(transform.position, Vector3.up, rotationSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.E)){
            transform.RotateAround(transform.position, Vector3.up, -rotationSpeed * Time.deltaTime);
        }
    }
}