
using UnityEngine;
using TMPro; // Import TextMeshPro namespace
using UnityEngine.UI;

public class BusController : MonoBehaviour
{
    public float acceleration = 20f;       // How quickly the bus accelerates
    public float maxAcceleration = 22f;
    public float normalSpeed = 10f;          // Maximum speed
    public float boostSpeed = 15f;
    public float maxNormalSpeed = 12f;
    public float maxBoostSpeed = 15f;
    public float rotationSpeed = 200f;    // Speed of rotation
    public float maxRotationSpeed = 210f;

    public float currentSpeed;

    public float minPitch = 0.8f;
    public float maxPitch = 3f;

    public float minTyreVol = 0f;
    public float maxTyreVol = 0.3f;

    private float speedIncreaseRate;
    private float accelerationIncreaseRate;
    private float turnSpeedIncreaseRate;

    public float boostDuration = 5f;         // Max duration of boost (in seconds)
    public float boostCooldown = 10f;       // Time it takes to recharge boost (in seconds)
    private bool canBoost = true;           // Whether boost is available

    public float timeToMaxValue = 300f;

    public TMP_Text driftAngleText;       // Reference to TextMeshPro Text for displaying drift angle
    public float maxTiltAngle = 15f;      // Maximum allowed rotation along Z-axis (in world space)

    public Rigidbody rb;                 // Rigidbody component
    private Vector3 targetDirection;      // Direction pointing toward the mouse
    private Vector3 currentDirection;     // Direction the bus is currently moving
    public float driftAngle = 0f;         // Drift angle of the bus (signed)

    public AudioSource[] sources;
    public AudioSource firstAudioSource;
    public AudioSource secondAudioSource;
    public AudioSource musicSource;

    //public BoostTimerUI boostTimerUI;

    public GameObject musicPlayer;

    public sceneManager SceneManager;

    public TrailRenderer FL;
    public TrailRenderer FR;
    public TrailRenderer RL;
    public TrailRenderer RR;

    public float remainingBoost;
    public float cooldownTimer;

    public Slider engineVolumeSlider;
    public Slider tyreVolumeSlider;
    public Slider musicVolumeSlider;

    public bool boostOnCooldown = false;

    public Transform[] seats; // Array of seat positions on the bus
    private bool[] seatOccupied; // Track whether seats are occupied
    public bool upgradingBus = false;
    private LocationScript currentLocation; // Reference to the current location (assumes you have this reference in the bus)

    // New variable to store the color of the carpark or terminal
    public Color collidedLocationColor = Color.white;

    // Flag indicating whether the bus is colliding with a carpark or terminal
    public bool isColliding = false;

    // Method to get the color of the collided location
    public Color GetCollidedLocationColor()
    {
        return collidedLocationColor;
    }

    void Start()
    {

        //boostTimerUI = GameObject.Find("busBoost").GetComponent<BoostTimerUI>();

        remainingBoost = boostDuration;
        cooldownTimer = boostCooldown;

        musicPlayer = GameObject.Find("musicPlayer");

        musicSource = musicPlayer.GetComponent<AudioSource>();

        FL = gameObject.transform.Find("EffectsFrontLeft").GetComponent<TrailRenderer>();
        FR = gameObject.transform.Find("EffectsFrontRight").GetComponent<TrailRenderer>();
        RL = gameObject.transform.Find("EffectsRearLeft").GetComponent<TrailRenderer>();
        RR = gameObject.transform.Find("EffectsRearRight").GetComponent<TrailRenderer>();

        sources = gameObject.GetComponents<AudioSource>();
        firstAudioSource = sources[0];
        secondAudioSource = sources[1];

        engineVolumeSlider.value = firstAudioSource.volume;
        tyreVolumeSlider.value = maxTyreVol;
        musicVolumeSlider.value = musicSource.volume;

        speedIncreaseRate = (maxNormalSpeed - normalSpeed) / timeToMaxValue;
        accelerationIncreaseRate = (maxAcceleration - acceleration) / timeToMaxValue;
        turnSpeedIncreaseRate = (maxRotationSpeed - rotationSpeed) / timeToMaxValue;

        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody component is missing!");
        }
        if (driftAngleText == null)
        {
            Debug.LogError("TextMeshPro Text for Drift Angle is not assigned!");
        }
        // Initialize the seat array and occupancy status
        seatOccupied = new bool[seats.Length];

        // Optionally, if this is a bus at the start, assume all seats are empty
        for (int i = 0; i < seatOccupied.Length; i++)
        {
            seatOccupied[i] = false;
        }
    }

    public int GetNextAvailableSeat()
    {
        for (int i = 0; i < seatOccupied.Length; i++)
        {
            if (!seatOccupied[i])
            {
                seatOccupied[i] = true; // Mark this seat as occupied
                return i;
            }
        }
        return -1; // No seat available
    }

    // Updates when passenger leaves
    public void FreeSeat(int index)
    {
        if (index >= 0 && index < seatOccupied.Length)
        {
            seatOccupied[index] = false;  // Mark seat as available again
            //Debug.Log("Seat " + index + " is now free.");
        }
    }

    /*
    public void moveBus()
    {
        Vector3 mouseWorldPosition = GetMouseWorldPosition();
        targetDirection = (mouseWorldPosition - transform.position).normalized;
        targetDirection.y = 0; // Ignore vertical axis for 2D movement

        // Rotate the bus smoothly towards the target direction
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        rb.rotation = Quaternion.RotateTowards(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);

        // Apply acceleration and cap at max speed
        Vector3 targetVelocity = transform.forward * normalSpeed;
        rb.linearVelocity = Vector3.MoveTowards(rb.linearVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
    }
    */
    public void moveBus()
    {
        Vector3 mouseWorldPosition = GetMouseWorldPosition();
        targetDirection = (mouseWorldPosition - transform.position).normalized;
        targetDirection.y = 0; // Ignore vertical axis for 2D movement

        // Rotate the bus smoothly towards the target direction
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        rb.rotation = Quaternion.RotateTowards(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);

        // Default speed
        currentSpeed = normalSpeed;
        float currentAcceleration = acceleration;
        

        // Check if the player is boosting
        if (Input.GetMouseButton(0) && canBoost) // Left mouse button is pressed, and boost is available
        {
            currentSpeed += 5;
            currentAcceleration += 5;
            /*
            if (!boostOnCooldown)
            {
                //boostTimerUI.SetRadialFillAmount(remainingBoost, boostDuration);
                if (remainingBoost > 0)
                {
                    currentSpeed = boostSpeed;       // Increase speed
                    isBoosting = true;               // Mark boosting as active
                    remainingBoost -= Time.deltaTime;
                }
                else
                {
                    remainingBoost = boostDuration;
                    cooldownTimer = boostCooldown;
                    boostOnCooldown = true;
                }
            } else
            {
                //boostTimerUI.SetRadialFillAmount(cooldownTimer, boostCooldown);
                if (cooldownTimer > boostCooldown)
                {
                    boostOnCooldown = false;
                    cooldownTimer = 0;
                    remainingBoost = boostDuration;
                } else
                {
                    cooldownTimer += Time.deltaTime;
                }
            }
            */
        }
        // Apply the calculated target velocity
        Vector3 targetVelocity = transform.forward * currentSpeed;
        rb.linearVelocity = Vector3.MoveTowards(rb.linearVelocity, targetVelocity, currentAcceleration * Time.fixedDeltaTime);
    }

    void FixedUpdate()
    {

        

        firstAudioSource.volume = engineVolumeSlider.value;
        musicSource.volume = musicVolumeSlider.value;

        if (driftAngle != 0)
        {
            if(driftAngle > 0)
            {
                FL.emitting = true;
                RL.emitting = true;

                FR.emitting = false;
                RR.emitting = false;
            } else
            {
                FR.emitting = true;
                RR.emitting = true;

                FL.emitting = false;
                RL.emitting = false;
            }
        } else
        {
            FL.emitting = false;
            FR.emitting = false;
            RL.emitting = false;
            RR.emitting = false;
        }

        moveBus();
        UpdateDriftAngle();
        ConstrainWorldZRotation();

        // Calculate the pitch based on the speed of the bus
        float speed = rb.linearVelocity.magnitude; // Get the speed
        float boostSpeed = rb.linearVelocity.magnitude + 5;
        float pitch = Mathf.Lerp(minPitch, maxPitch, speed / normalSpeed); // Map speed to pitch
        float driftPercentage = (Mathf.Abs(driftAngle)) / 180;
        float tyreVol = Mathf.Lerp(minTyreVol, tyreVolumeSlider.value, driftPercentage);
        if(!SceneManager.paused)
        {
            secondAudioSource.volume = Mathf.Clamp(tyreVol, minTyreVol, tyreVolumeSlider.value);
        }
        firstAudioSource.pitch = Mathf.Clamp(pitch, minPitch, maxPitch);     // Clamp the pitch value
        if (upgradingBus)
        {
            if (speed < maxNormalSpeed)
            {
                speed += speedIncreaseRate * Time.deltaTime;  // Increment top speed over time
                boostSpeed += speedIncreaseRate * Time.deltaTime;
                speed = Mathf.Min(speed, maxNormalSpeed); // Ensure it doesn't exceed the max
            }

            if (acceleration < maxAcceleration)
            {
                acceleration += accelerationIncreaseRate * Time.deltaTime; // Increment acceleration
                acceleration = Mathf.Min(acceleration, maxAcceleration); // Ensure it doesn't exceed the max
            }
            //a
            if (rotationSpeed < maxRotationSpeed)
            {
                rotationSpeed += turnSpeedIncreaseRate * Time.deltaTime; // Increment turn speed
                rotationSpeed = Mathf.Min(rotationSpeed, maxRotationSpeed); // Ensure it doesn't exceed the max
            }
        }
        
        /*
        if (Input.GetMouseButton(0)) // Left mouse button pressed
        {
            //moveBus();
            
        }
        else
        {
            // Gradually slow down if no mouse input
            rb.linearVelocity = Vector3.MoveTowards(rb.linearVelocity, Vector3.zero, acceleration * Time.fixedDeltaTime);
        }
        */
        // Update the drift angle based on current and target directions
        
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

    public void UpdateDriftAngle()
    {
        if (rb.linearVelocity.magnitude > 0.1f) // Only calculate drift angle if moving
        {
            currentDirection = rb.linearVelocity.normalized; // Direction the bus is currently moving
            float angle = Vector3.Angle(targetDirection, currentDirection); // Get the unsigned angle

            // Determine if the drift is clockwise or counterclockwise using cross product
            float cross = Vector3.Cross(currentDirection, targetDirection).y;
            driftAngle = cross < 0 ? -angle : angle;

            // Update the TextMeshPro text with the drift angle (formatted)
            if (driftAngleText != null)
            {
                driftAngleText.text = $"Drift Angle: {Mathf.Floor(Mathf.Abs(driftAngle))}°";
            }
        }
        else
        {
            // Set drift angle to 0 when the bus is not moving
            driftAngle = 0;
            if (driftAngleText != null)
            {
                driftAngleText.text = "Drift Angle: 0°";
            }
        }
    }

    public void ConstrainWorldZRotation()
    {
        float driftAngleAbs = Mathf.Abs(driftAngle); // Absolute drift angle (non-negative)

        // Calculate Z rotation range as a percentage of the max tilt (in world space)
        float zRotationRange = Mathf.Lerp(0f, maxTiltAngle, driftAngleAbs / 180f);

        // Get current Euler angles (in world space)
        Vector3 euler = rb.rotation.eulerAngles;

        // Normalize the rotation angle for world Z-axis: -180 to 180 degrees range.
        if (euler.z > 180f) euler.z -= 360f;

        // Clamp the Z rotation within the calculated world-space range
        euler.z = Mathf.Clamp(euler.z, -zRotationRange, zRotationRange);

        // Reapply the constrained rotation back to the Rigidbody
        rb.rotation = Quaternion.Euler(euler.x, euler.y, euler.z);
    }

    // Handle the collisions with car park or terminal
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("CarPark"))
        {
            //Debug.Log("Bus collided with a CarPark.");
            // Set the color of the CarPark and set the flag
            collidedLocationColor = other.GetComponent<Renderer>().material.color;
            isColliding = true; // Flag set to indicate collision with CarPark
        }
        else if (other.CompareTag("Terminal"))
        {
            //Debug.Log("Bus collided with a Terminal.");
            // Set the color of the Terminal and set the flag
            collidedLocationColor = other.GetComponent<Renderer>().material.color;
            isColliding = true; // Flag set to indicate collision with Terminal
        }
    }

    // Optionally, you can reset the flag when the collision ends (if needed)
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("CarPark") || other.CompareTag("Terminal"))
        {
            isColliding = false; // Reset flag when the collision ends
        }
    }
}

