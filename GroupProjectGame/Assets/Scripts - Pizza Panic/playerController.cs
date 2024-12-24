/*
using UnityEngine;
using Unity.Netcode;

public class playerController : NetworkBehaviour
{
            //References//

    //GameObjects

    //Components
    [SerializeField] private Rigidbody rb;

    //ints

            //Floats//
    //movement
    public float rotationSpeed = 175f;
    public float moveSpeed = 7f;
    public float acceleration = 15f;

    //Bools

    //Network
    private NetworkVariable<int> randomNumber = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    void Start()
    {
        rb = transform.GetComponent<Rigidbody>(); //referencing the player objects rigidbody
        if (!IsServer)
        {
            rb.isKinematic = true;
        } 
        
    }

    void Update()
    {
        if (!IsOwner) return; 
        else
        {
            if (IsMouseInGameWindow())
            {
                (Vector3 targetVelocity, Quaternion targetRotation) = PlayerInput();
                MovementServerRpc(targetVelocity, targetRotation);
            }
        }
        
        

    }
    
    [ServerRpc]
    private void MovementServerRpc(Vector3 targetVelocity, Quaternion targetRotation)
    {
        rb.rotation = Quaternion.RotateTowards(rb.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        rb.linearVelocity = Vector3.MoveTowards(rb.linearVelocity, targetVelocity, acceleration * Time.deltaTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (IsServer)
        {
            Rigidbody otherRigidbody = collision.rigidbody;

            if (otherRigidbody != null)
            {
                Vector3 force = collision.contacts[0].normal * -10f;
                otherRigidbody.AddForce(force, ForceMode.Impulse);
            }
        }
    }

    (Vector3, Quaternion) PlayerInput()
    {
        Vector3 mouseWorldPosition = GetMouseWorldPosition(); //Current mouse position
        Vector3 targetDirection = (mouseWorldPosition - transform.position).normalized; //where we want to go
        targetDirection.y = 0; //ignore all y axis inputs, only controlled on the x-z plane

        Quaternion targetRotation = Quaternion.LookRotation(targetDirection); //where we want the player to point towards

        Vector3 targetVelocity = transform.forward * moveSpeed;

        return (targetVelocity, targetRotation);
    }

    Vector3 GetMouseWorldPosition()
    {
        // Cast a ray from the camera to the mouse position on the screen
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            return hit.point;
        }
        return transform.position;
    }

    bool IsMouseInGameWindow()
    {
        Vector3 mousePosition = Input.mousePosition;

        // Check if mouse is within screen bounds
        return mousePosition.x >= 0 &&
               mousePosition.y >= 0 &&
               mousePosition.x <= Screen.width &&
               mousePosition.y <= Screen.height;
    }
}
*/
using UnityEngine;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{
    // References
    [SerializeField] private Rigidbody rb;

    // Movement parameters
    public float rotationSpeed = 175f;       // Base rotation speed
    public float moveSpeed = 7f;             // Max move speed
    public float moveSpeedDecrease = 4f;
    public float acceleration = 10f;         // Acceleration (how fast the player picks up speed)
    public float rotationSpeedIncrease = 250f; // Rotation speed increase when right-clicking

    private Vector3 targetVelocity;
    private Quaternion targetRotation;

    private Vector3 currentVelocity;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();  // Get reference to the Rigidbody

        // On clients, we don’t directly handle physics so we make the Rigidbody kinematic
        if (!IsServer)
        {
            rb.isKinematic = true;
        }
    }

    private void Update()
    {
        // Only allow input handling if this is the owner (client who owns the player object)
        if (IsOwner)
        {
            // Check if the right mouse button is held down
            if (Input.GetMouseButton(1))  // Right mouse button
            {
                rotationSpeed = rotationSpeedIncrease; // Increase rotation speed
                moveSpeed = moveSpeedDecrease;
            }
            else
            {
                rotationSpeed = 175f; // Revert back to base rotation speed
                moveSpeed = 7f;
            }

            // If the mouse is in the game window, allow player to move
            if (IsMouseInGameWindow())
            {
                (Vector3 newTargetVelocity, Quaternion newTargetRotation) = PlayerInput();
                targetVelocity = newTargetVelocity;
                targetRotation = newTargetRotation;

                MovementServerRpc(targetVelocity, targetRotation);
            }
            else
            {
                // If the mouse is off-screen, stop the movement
                targetVelocity = Vector3.zero;
                MovementServerRpc(Vector3.zero, targetRotation);
            }
        }
    }

    // ServerRpc to process the player movement and rotation
    [ServerRpc]
    private void MovementServerRpc(Vector3 targetVelocity, Quaternion targetRotation)
    {
        // Only process physics-related movement on the server (authoritative control)
        if (!IsServer) return;

        // Rotate the player towards the target rotation
        rb.rotation = Quaternion.RotateTowards(rb.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        // Gradually adjust velocity using smooth acceleration
        if (targetVelocity.magnitude > 0)
        {
            currentVelocity = Vector3.MoveTowards(currentVelocity, targetVelocity, acceleration * Time.deltaTime);
            rb.linearVelocity = currentVelocity; // Apply the smoothed velocity
        }
        else
        {
            // Stop movement by setting velocity to zero
            currentVelocity = Vector3.zero;
            rb.linearVelocity = currentVelocity;
        }
    }

    // Detect mouse input within the game window
    bool IsMouseInGameWindow()
    {
        Vector3 mousePosition = Input.mousePosition;

        // Check if mouse is within screen bounds
        return mousePosition.x >= 0 && mousePosition.y >= 0 &&
               mousePosition.x <= Screen.width && mousePosition.y <= Screen.height;
    }

    // Get world position from the mouse pointer
    Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            return hit.point;
        }
        return transform.position;
    }

    // Calculate player input from mouse movement, determining the target velocity and rotation
    (Vector3, Quaternion) PlayerInput()
    {
        Vector3 mouseWorldPosition = GetMouseWorldPosition();  // Current mouse position
        Vector3 targetDirection = (mouseWorldPosition - transform.position).normalized;  // Direction to the mouse
        targetDirection.y = 0;  // Ignore vertical movement on the y-axis

        // Calculate the rotation to face the mouse
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

        // Always move forward at a constant speed (in forward direction)
        Vector3 targetVelocity = transform.forward * moveSpeed;

        return (targetVelocity, targetRotation);
    }
}
