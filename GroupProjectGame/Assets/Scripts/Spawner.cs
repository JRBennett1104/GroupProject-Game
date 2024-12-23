
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnManager : MonoBehaviour
{
    public GameObject carParkPrefab;
    public GameObject terminalPrefab;
    public float spawnInterval = 3f;  // How often a new car park or terminal spawns
    public float minDistanceFromOther = 20f;  // Minimum distance between spawned objects
    public List<Color> allowedColors;  // List of allowed colors
    private List<Vector3> spawnedPositions = new List<Vector3>();  // To track spawned positions

    public GameObject spawnPlane;  // The plane prefab from the hierarchy that defines the spawn area
    private Vector3 planeCenter;
    private Vector3 planeSize;

    public float rotationStartY = -50f; // Start of the range
    public float rotationEndY = 55f; // End of the range
    public float rotationStepY = 10f; // Step size

    public float margin = 1f; // Margin to avoid spawning directly on the edges of the plane
    private int maxRetries = 100; // Limit on retries to prevent infinite loops
    public int numberOfLocations = 0;

    private bool spawnCarParkNext = true; // Track which prefab should spawn next

    public int maxLocations = 2;  // Max number of locations (car parks/terminals) that can be spawned at a time

    // List of used colors to avoid using them again
    public List<Color> usedColors = new List<Color>();
    public List<Color> terminalUsedColors = new List<Color>();
    public List<Color> parkUsedColors = new List<Color>();
    private List<Color> availableColors = new List<Color>();

    public List<GameObject> passengerSpawners = new List<GameObject>();

    public GameObject prefabToSpawn;

    private ColorManager colorManager; // Reference to ColorManager

    public static SpawnManager Instance { get; private set; }

    private void Awake()
    {
        // Singleton enforcement
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional: Prevent this manager from being destroyed on scene reload
        }
        else
        {
            Debug.LogWarning("Duplicate SpawnManager detected! Destroying...");
            Destroy(gameObject); // Kill any extra instances
        }
    }

    void Start()
    {
        //Debug.Log("SpawnManager instance reference: " + gameObject.GetInstanceID());
        colorManager = FindFirstObjectByType<ColorManager>();
        availableColors = new List<Color>(colorManager.availableColors);

        if (spawnPlane != null)
        {
            // Get the size of the spawn area from the plane's mesh renderer bounds
            planeCenter = spawnPlane.transform.position;
            Renderer planeRenderer = spawnPlane.GetComponent<Renderer>();

            if (planeRenderer != null)
            {
                planeSize = planeRenderer.bounds.size;
            }
            else
            {
                Debug.LogError("The spawnPlane doesn't have a Renderer component!");
                return;
            }

            if (planeSize.x <= 0 || planeSize.z <= 0)
            {
                Debug.LogError("Spawn plane is too small, cannot spawn objects.");
                return;
            }

            SpawnInitialObjects();
            StartCoroutine(SpawnObjects());
        }
        else
        {
            Debug.LogWarning("Spawn plane is not assigned. Please assign a plane prefab in the Inspector.");
        }
    }

    // Spawn initial car park and terminal
    void SpawnInitialObjects()
    {
        SpawnCarParkOrTerminal();  // First spawn
    }

    void UpdateSpawnPlaneSize()
    {
        if (spawnPlane != null)
        {
            // Get the size of the spawn area from the plane's mesh renderer bounds
            Renderer planeRenderer = spawnPlane.GetComponent<Renderer>();
            if (planeRenderer != null)
            {
                planeSize = planeRenderer.bounds.size; // Updates the size every time this is called
            }
        }
        else
        {
            Debug.LogWarning("Spawn plane is not assigned. Please assign a plane prefab in the Inspector.");
        }
    }

    // Coroutine to spawn objects at intervals
    IEnumerator SpawnObjects()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            UpdateSpawnPlaneSize();
            // Only spawn if the number of locations is less than maxLocations
            if (spawnedPositions.Count < maxLocations)
            {
                SpawnCarParkOrTerminal();
            }
        }
    }

    // Choose which prefab to spawn (alternating between car park and terminal)

    
    void SpawnCarParkOrTerminal()
    {
        // Decide which prefab to spawn based on the flag
        prefabToSpawn = spawnCarParkNext ? carParkPrefab : terminalPrefab;

        // Find a valid position within the defined spawn plane, avoiding close spawn areas
        Vector3 spawnPosition = GetValidSpawnPosition();

        if (spawnPosition != Vector3.zero) // Avoid instantiating if no valid position was found
        {
            float randomAngle = Random.Range(-50f,50f);
            GameObject spawnedObject = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
            spawnedObject.transform.rotation = Quaternion.Euler(0,randomAngle,0);
            passengerSpawners.Add(spawnedObject);
            numberOfLocations += 1;

            // Assign a random color from the available colors, avoiding used colors
            AssignRandomColor(spawnedObject);

            // Save the spawn position to track it
            spawnedPositions.Add(spawnPosition);
        }
        else
        {
            Debug.LogWarning("Failed to find a valid spawn position. Skipping spawn.");
        }

        // Toggle the prefab to spawn next time
        spawnCarParkNext = !spawnCarParkNext;
    }
   

    // Get a valid spawn position within the defined plane, ensuring it's far from other objects
    Vector3 GetValidSpawnPosition()
    {
        Vector3 spawnPosition = Vector3.zero;
        bool isPositionValid = false;
        int retries = 0;

        // Continuously try to find a valid position within the plane (with retries to avoid infinite loop)
        while (!isPositionValid && retries < maxRetries)
        {
            retries++;

            // Randomly generate a spawn position within the defined spawn plane, based on its size and center
            float randomX = Random.Range(planeCenter.x - planeSize.x / 2 + margin, planeCenter.x + planeSize.x / 2 - margin);
            float randomY = Random.Range(planeCenter.z - planeSize.z / 2 + margin, planeCenter.z + planeSize.z / 2 - margin);

            spawnPosition = new Vector3(randomX, 0f, randomY); // We use Y = 0 for a 2D spawn on the plane

            // Check if the spawn position is far enough away from existing positions
            isPositionValid = true;
            foreach (var pos in spawnedPositions)
            {
                if (Vector3.Distance(spawnPosition, pos) < minDistanceFromOther)
                {
                    isPositionValid = false;
                    break;  // Stop the check if too close to another object
                }
            }
        }

        // Return Vector3.zero if failed to find a position after maxRetries
        return retries < maxRetries ? spawnPosition : Vector3.zero;
    }

    // Assign a random color to the spawned object, ensuring it is not in the usedColors list
    /*
    void AssignRandomColor(GameObject spawnedObject)
    {
        // Find the available colors that have not been used yet
        availableColors.RemoveAll(color => usedColors.Contains(color));

        if (availableColors.Count == 0)
        {
            Debug.LogWarning("No available colors left!");
        }

        // Select a random available color
        Color chosenColor = availableColors[Random.Range(0, availableColors.Count)];

        // Set the color to the object
        spawnedObject.GetComponent<Renderer>().material.color = chosenColor;
        Debug.Log("Assigned colour is : " + chosenColor);
        //Debug.Log("Assigned " + chosenColor);
        // Mark this color as used and add to the usedColors list
        if (prefabToSpawn == carParkPrefab)
        {
            parkUsedColors.Add(chosenColor);
        }
        else
        {
            terminalUsedColors.Add(chosenColor);
        }
        usedColors.Add(chosenColor);
        availableColors.Remove(chosenColor);
        //Debug.Log(chosenColor + "added to list of used colours");
    }

    // Function to gradually increase maxLocations
    public void IncreaseMaxLocations(int increment)
    {
        maxLocations += increment;  // Increase max locations by the given increment
    }
    */

    void AssignRandomColor(GameObject spawnedObject)
    {
        // Find the available colors that have not been used yet
        availableColors.RemoveAll(color => usedColors.Contains(color));

        if (availableColors.Count == 0)
        {
            Debug.LogWarning("No available colors left!");
            return;
        }

        Color chosenColor = availableColors[Random.Range(0, availableColors.Count)];

        // Set the color to the object
        //spawnedObject.transform.Find("Plane.001").GetComponent<MeshRenderer>().material.color = chosenColor;
        //spawnedObject.transform.Find("Plane.002").GetComponent<MeshRenderer>().material.color = chosenColor;
        spawnedObject.GetComponent<Renderer>().material.color = chosenColor;
        //Debug.Log("Assigned colour is: " + chosenColor);

        // Add color to appropriate list
        if (prefabToSpawn == carParkPrefab)
        {
            parkUsedColors.Add(chosenColor);  // Track colors used for car parks
        }
        else
        {
            terminalUsedColors.Add(chosenColor);  // Track colors used for terminals
        }

        // Mark this color as used and add it to the general used colors
        usedColors.Add(chosenColor);
        availableColors.Remove(chosenColor);
    }
}
