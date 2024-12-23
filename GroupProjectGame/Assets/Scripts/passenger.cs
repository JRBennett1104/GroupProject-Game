
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Passenger : MonoBehaviour
{
    public BusController busController;    // Reference to the BusController
    private LocationScript locationScript;  // Reference to the LocationScript
    public overhauledPassengerSpawner overhauledPassengerSpawner;
    public scoreTracker ScoreTracker;
    private Renderer parentRenderer;
    public sceneManager SceneManager;
    private int seatIndex = -1;             // To track the seat assigned to the passenger
    public float cooldownPeriod = 2f;       // Cooldown time before the location can spawn a new passenger
    public Color myColor;
    private Vector3 spawnScale;             // To store the passenger's initial scale
    private Transform assignedSeat;         // Reference to the assigned seat
    private bool isAssignedToSeat = false;  // Flag to track if the passenger is seated
    private List<Color> allowedColors;
    public int mySeat;
    private Color parentColor;
    public int passengersDroppedOff = 0;
    public float cooldownMin = 0f;
    public float cooldownMax = 5f;
    public Image timerImage;
    public Image floatingTimer;
    public Image floatingTimerBg1;
    public Image floatingTimerBg2;
    public Transform largeTimerTransform;
    public Transform displayCanvasSmall;
    public Transform displayCanvasFloating;
    public float currentWaitTime = 15f;
    public float waitingTime = 15f;
    public float ridingTime = 15f;

    public Color passengerColor;            // Color of the passenger for matching with destinations

    void Start()
    {
        SceneManager = GameObject.Find("sceneManager").GetComponent<sceneManager>();
        displayCanvasSmall = transform.Find("smallTimer");
        displayCanvasFloating = transform.Find("largeTimer");
        displayCanvasFloating.gameObject.SetActive(true);
        displayCanvasSmall.gameObject.SetActive(false);
        if (displayCanvasSmall != null)
        {
            timerImage = displayCanvasSmall.Find("topTimerSmall").GetComponent<Image>();


            floatingTimer = displayCanvasFloating.Find("largeFloatingTimer").GetComponent<Image>();
            floatingTimerBg1 = displayCanvasFloating.Find("largeTimerBackground").GetComponent<Image>();
            floatingTimerBg2 = displayCanvasFloating.Find("largeTimerBackground (1)").GetComponent<Image>();

        }
        UpdateTimerColor(Color.green);
        overhauledPassengerSpawner = transform.parent.GetComponent<overhauledPassengerSpawner>();
        ScoreTracker = GameObject.FindFirstObjectByType<scoreTracker>();
        parentRenderer = transform.parent.GetComponent<Renderer>();
        MeshRenderer renderer = transform.GetChild(0).GetComponent<MeshRenderer>();
        myColor = renderer.material.color;
        // Save the initial scale of the passenger at spawn
        spawnScale = transform.localScale;
        busController = GameObject.FindFirstObjectByType<BusController>();  // Find the BusController


        //Debug.Log("I am : " + transform.GetChild(0).GetComponent<Renderer>().material.color);

    }

    void UpdateTimerColor(Color newColor)
    {
        // Change the color of the timer images
        if (timerImage != null)
        {
            timerImage.color = newColor;
        }

        if (floatingTimer != null)
        {
            floatingTimer.color = newColor;
        }
    }

    /*
    void RotateTimerTowardsBus()
    {
        if (busController != null && timerTransform != null)
        {
            // Get the position of the bus
            Vector3 targetPosition = busController.transform.position;

            // Make sure the image stays at the same height as the passenger
            targetPosition.y = transform.position.y;  // Don't affect the Y-axis rotation of the image

            // Calculate direction from the timer (image) to the bus
            Vector3 directionToBus = targetPosition - timerTransform.position;

            // Get the rotation that faces the bus
            Quaternion lookRotation = Quaternion.LookRotation(directionToBus);

            // Apply only Y rotation so the timer image faces the bus horizontally
            timerTransform.rotation = Quaternion.Euler(0, lookRotation.eulerAngles.y, 0);
        }
    }
    */

    void OnTriggerEnter(Collider other)
    {

        // Ensure we're colliding with the bus (having the bus set as a trigger)
        if (other.CompareTag("Bus"))
        {
            // Get the next available seat on the bus
            seatIndex = busController.GetNextAvailableSeat();
            //Debug.Log("Seat Index: " + seatIndex);

            // If a seat is available, move the passenger to the seat's position and track it
            if (seatIndex != -1)
            {
                // Find the seat and move the passenger to the seat's position
                mySeat = seatIndex;
                assignedSeat = busController.seats[seatIndex];

                // Ensure the passenger stays the same size it was when it spawned
                transform.localScale = spawnScale;  // Set the scale back to its original size

                // Set the initial position of the passenger to the seat's position
                transform.position = assignedSeat.position;

                // Optionally adjust the passenger’s rotation based on the seat’s rotation
                transform.rotation = assignedSeat.rotation;

                // Detach the passenger from the location prefab (no longer a child of location)
                transform.SetParent(null); // Make it independent of the location prefab

                isAssignedToSeat = true;  // Mark as assigned to a seat
                currentWaitTime = ridingTime;
                displayCanvasFloating.gameObject.SetActive(false);
                displayCanvasSmall.gameObject.SetActive(true);
                
            }
            else
            {
                Debug.LogWarning("No available seats on the bus!");
            }
        }
    }

    void decreaseTimer(float currentTimer)
    {
        //Debug.Log(currentTimer);
        currentWaitTime -= Time.deltaTime;
        float timePercentage = currentWaitTime / currentTimer;
        timerImage.fillAmount = timePercentage;
        floatingTimer.fillAmount = timePercentage;
        Color newColor = Color.Lerp(Color.red, Color.green, timePercentage);
        UpdateTimerColor(newColor);

    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(currentWaitTime);
        // If the passenger is seated and assigned to a seat, follow the seat
        if (currentWaitTime > 0)
        {
            if (!assignedSeat)
            {
                decreaseTimer(waitingTime);
            }
            else
            {
                decreaseTimer(ridingTime);
            }
        } else
        {
            Destroy(gameObject);
            ScoreTracker.firstMissedPassenger = true;
            overhauledPassengerSpawner.SpawnTime = Random.Range(cooldownMin, cooldownMax);
            overhauledPassengerSpawner.PassengerDestroyed = true;
            if (SceneManager.level == 3 && ScoreTracker.totalScore <= 0)
            {
                ScoreTracker.totalScore -= 5;
            } else
            {
                ScoreTracker.totalScore -= 1;
            }


            if (isAssignedToSeat)
            {
                busController.FreeSeat(mySeat);
            }
        }
        


        if (isAssignedToSeat && assignedSeat != null)
        {
            // Maintain the position relative to the seat while the bus moves
            transform.position = assignedSeat.position;
            transform.rotation = assignedSeat.rotation;

            // The passenger stays the same size and aligned with the seat
            transform.localScale = spawnScale;  // Ensure it's the same size as before
        }
        if (busController.isColliding && myColor == busController.collidedLocationColor && isAssignedToSeat)
        {
            if (overhauledPassengerSpawner != null)
            {
                overhauledPassengerSpawner.SpawnTime = Random.Range(cooldownMin, cooldownMax);
                overhauledPassengerSpawner.PassengerDestroyed = true;
                //Debug.Log("I have arrived at" + busController.collidedLocationColor);
                ScoreTracker.scoreTotal += 1;
                ScoreTracker.totalScore += 1;
                Destroy(gameObject);
                busController.FreeSeat(mySeat);
            }
            else
            {
                Debug.LogError("no overhauled spawner script");
            }

        }
    }
}