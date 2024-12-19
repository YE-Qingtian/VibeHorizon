using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 3.0f; // Speed of movement
    public float rotateSpeed = 90.0f; // Speed of rotation

    void Start()
    {
        
    }

    void Update()
    {
        // Get input from the primary thumbstick (left stick) for X and Z axis movement
        Vector2 thumbstickInput = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);

        // Get input from the secondary thumbstick (right stick) for Y-axis movement
        float verticalInput = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).y;

        // Set the move direction to include x, y, and z
        Vector3 moveDirection = new Vector3(thumbstickInput.x, verticalInput, thumbstickInput.y);
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);

        // Rotate the player using the horizontal input of the right thumbstick
        float rotation = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).x * rotateSpeed * Time.deltaTime;
        transform.Rotate(0, rotation, 0);
    }
}
