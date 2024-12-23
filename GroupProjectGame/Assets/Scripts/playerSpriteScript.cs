
using UnityEngine;

public class SpriteRotationController : MonoBehaviour
{
    public float maxTiltAngle = 15f;          // Maximum tilt angle of the sprite in degrees
    public float parentDriftAngle = 0f;       // Drift angle received from the parent script
    public BusController BusController;      // Reference to the BusController to access driftAngle
    public float resetSpeed = 5f;             // Speed of reset when drift angle decreases or mouse is released

    private float currentRotationZ = 0f;     // Store the current Z rotation

    void Update()
    {
        // Detect if the mouse button is pressed or released
        /*
        if (Input.GetMouseButton(0)) // Left mouse button held down
        {
            isMouseHeld = true;
            parentDriftAngle = BusController.driftAngle;  // Update drift angle when moving the bus
        }
        else if (Input.GetMouseButtonUp(0)) // Left mouse button released
        {
            isMouseHeld = false; // Start resetting rotation when the mouse is released
        }
        */
        // Call the method to rotate the sprite based on the drift angle
        parentDriftAngle = BusController.driftAngle;  // Update drift angle when moving the bus
        RotateBasedOnDrift();
    }

    void RotateBasedOnDrift()
    {
        // Convert the drift angle to a percentage of the 180-degree drift range
        float driftPercentage = Mathf.Abs(parentDriftAngle) / 180f * 100f;

        // Calculate the Z rotation to apply as a percentage of the max tilt angle
        float tiltAmount = (driftPercentage / 100f) * maxTiltAngle;

        // If the drift angle is non-zero and mouse is held down, gradually tilt the sprite

        if (parentDriftAngle > 0)  // Clockwise rotation
        {
            currentRotationZ = Mathf.MoveTowards(currentRotationZ, tiltAmount, (maxTiltAngle * Time.deltaTime));
        } else  // Counterclockwise rotation
        {
            currentRotationZ = Mathf.MoveTowards(currentRotationZ, -tiltAmount, (maxTiltAngle * Time.deltaTime));
        }

        // If the mouse is released or the drift angle is zero, reset the rotation gradually
        if (parentDriftAngle == 0)
        {
            // Reset rotation gradually when the mouse is released or drift angle is near zero
            currentRotationZ = Mathf.MoveTowards(currentRotationZ, 0f, resetSpeed * Time.deltaTime);
        }

        // Get the current local Euler angles of the sprite
        Vector3 currentEuler = transform.localEulerAngles;

        // Reset and update the local Z rotation
        currentEuler.z = currentRotationZ;

        // Apply the adjusted Euler angles to the sprite
        transform.localEulerAngles = currentEuler;
    }
}
