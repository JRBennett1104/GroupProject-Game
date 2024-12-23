
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationScript : MonoBehaviour
{
    public GameObject passengerPrefab; // Reference to the passenger prefab
    public Color locationColor;        // Color assigned to this location
    private GameObject currentPassenger; // To track if a passenger is already spawned
    public bool canSpawn = true;
    public int initialSpawnTime = 5;

    public GameObject passengerSpawnPoint; // Reference to the empty GameObject for the spawn point

    private bool isCooldownActive = false;

    // New variables for delay before passenger spawn
    public float minSpawnDelay = 10f;  // Minimum time (seconds) before spawning a passenger
    public float maxSpawnDelay = 25f;  // Maximum time (seconds) before spawning a passenger
    public float minPickupCooldown = 30f;  // Minimum time (seconds) after pickup before spawning another passenger
    public float maxPickupCooldown = 60f;  // Maximum time (seconds) after pickup before spawning another passenger

    private ColorManager colorManager; // Reference to the shared ColorManager
    private static List<Color> usedColors = new List<Color>(); // To keep track of used colors

    // New static lists to track all spawned carparks and terminals
    public static List<LocationScript> allLocations = new List<LocationScript>(); // To keep track of spawned locations


    public SpawnManager spawnManager; // Reference to the SpawnManager script

    void Start()
    {
        // Initialize the ColorManager


        if (colorManager == null || spawnManager == null)
        {
            Debug.LogError("ColorManager or SpawnManager is missing in the scene!");
            return;
        }

        Renderer renderer = GetComponent<Renderer>();

        // Start the process by waiting a random time before spawning a passenger
        StartCoroutine(InitialSpawnDelay());
    }

    // Coroutine for waiting for a random delay before spawning a passenger
    IEnumerator InitialSpawnDelay()
    {
        float delay = Random.Range(minSpawnDelay, maxSpawnDelay);
        yield return new WaitForSeconds(delay); // Wait for the chosen delay time
        SpawnPassenger(locationColor); // Spawn the passenger after the delay
    }

    // Cooldown logic for delaying passenger spawn after a pickup
    public IEnumerator PickupCooldown()
    {
        // Choose a random cooldown time between the min and max range after passenger pickup
        float cooldownTime = Random.Range(minPickupCooldown, maxPickupCooldown);
        yield return new WaitForSeconds(cooldownTime); // Wait for the cooldown period

        // Allow the spawning process again
        canSpawn = true;

        // Spawn a new passenger after cooldown
        SpawnPassenger(locationColor);
    }

    // Coroutine to continuously check if the passenger is still a child of the location
    IEnumerator CheckPassengerStatus()
    {
        while (true)
        {
            // Check if currentPassenger exists and is still a child of the location prefab
            if (currentPassenger != null && currentPassenger.transform.parent != transform)
            {
                // Passenger has been removed from this location, start cooldown
                currentPassenger = null; // Nullify currentPassenger as it's no longer the child
                StartCoroutine(PickupCooldown());
                yield break; // Stop checking as passenger is picked up
            }

            // Wait a bit before checking again
            yield return new WaitForSeconds(1f);
        }
    }

    // Method to spawn the passenger prefab directly above the location prefab
    void SpawnPassenger(Color prefabColor)
    {
        if (currentPassenger == null && !isCooldownActive && canSpawn) // Ensure no passenger and no cooldown
        {
            canSpawn = false; // Prevent spawning a new passenger until cooldown ends

            // Get the list of used colors from the SpawnManager
            List<Color> availableColors = new List<Color>(spawnManager.usedColors);

            // Remove duplicates to ensure unique colors
            availableColors = RemoveDuplicateColors(availableColors);

            // Debugging: Log available colors before removing
            Debug.Log("Available Colors Before Removal: " + string.Join(", ", availableColors));

            // Ensure the location’s color is not picked for the passenger (compare accurately)
            availableColors.RemoveAll(color => AreColorsEqual(color, prefabColor));
            Debug.Log("Removed Location Color: " + prefabColor);

            // If no available colors remain, fallback to white
            if (availableColors.Count == 0)
            {
                Debug.LogWarning("No available unique colors for the passenger. Defaulting to white.");
                availableColors.Add(Color.white);
            }

            // Pick a random color for the passenger
            Color passengerColor = availableColors[Random.Range(0, availableColors.Count)];

            // Check if the passengerSpawnPoint is set
            if (passengerSpawnPoint != null)
            {
                Vector3 spawnPosition = passengerSpawnPoint.transform.position;

                currentPassenger = Instantiate(passengerPrefab, spawnPosition, Quaternion.identity);
                currentPassenger.transform.SetParent(this.transform);

                // Set the color of the passenger
                currentPassenger.GetComponent<Renderer>().material.color = passengerColor;

                Debug.Log("Passenger spawned with color: " + passengerColor);

                StartCoroutine(CheckPassengerStatus());
            }
            else
            {
                Debug.LogError("PassengerSpawnPoint is not assigned to this location prefab!");
            }
        }
        else
        {
            Debug.Log("A passenger already exists at this location.");
        }
    }

    bool AreColorsEqual(Color a, Color b)
    {
        return Mathf.Approximately(a.r, b.r) &&
               Mathf.Approximately(a.g, b.g) &&
               Mathf.Approximately(a.b, b.b) &&
               Mathf.Approximately(a.a, b.a);
    }

    // Helper Method: Remove Duplicate Colors from the List
    List<Color> RemoveDuplicateColors(List<Color> colors)
    {
        List<Color> uniqueColors = new List<Color>();

        foreach (Color color in colors)
        {
            bool alreadyExists = uniqueColors.Exists(c => AreColorsEqual(c, color));
            if (!alreadyExists)
                uniqueColors.Add(color);
        }

        return uniqueColors;
    }


    // You may want to update the location color dynamically (for example, in case colors change at runtime)
    public static void RemoveLocationColor(Color color)
    {
        if (usedColors.Contains(color))
        {
            usedColors.Remove(color);
        }

        // Remove the location from the list of allLocations as well
        LocationScript locationToRemove = allLocations.Find(loc => loc.locationColor == color);
        if (locationToRemove != null)
        {
            allLocations.Remove(locationToRemove);
        }
    }
}
