using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class overhauledPassengerSpawner : MonoBehaviour
{
    public GameObject passengerPrefab; //this is what is spawned as a passenger
    public GameObject spawnPoint; //this is the empty transform attached to the spawner prefab so the passenger appears within view
    public GameObject spawnedPassenger; //this is where the newly spawned passenger will be instantiated

   private SpawnManager spawnManager; //this references the spawnManager script to pull required data
    public BusController busController; //this references when the bus is colliding with a location and what color the location is

    private Color myColor;
    private Color previousColor;
    public Color passengerColor;

    public ColorManager colorManager; //this references the color manager script so all assets utilise the same colors for checks
    public List<Color> myColorsToUse = new List<Color>();
    private List<Color> usedColors = new List<Color>(); //this list is used as reference to know which colours can be assigned to passengers

    public GameObject panel1;
    public GameObject panel2;
    public Light light;

    public bool _passengerDestroyed = true; //this flag is to be used whenever the spawner must wait before spawning again
    public bool hasPassenger = false;

    public float SpawnTime = 10f; //this is to be used to determine how long a new spawner will appear before creating a passenger
    public float dropoffCooldown = 15f; //this is the time it takes for another passenger to spawn after a dropoff has been made

    public float passengerWaitTime;

    void Awake()
    {
        spawnManager = FindFirstObjectByType<SpawnManager>();
        


        passengerWaitTime = 15f;

        if (spawnManager == null)
        {
            Debug.LogError("SpawnManager not found in the scene! Make sure it exists.");
        }
    }

    public bool PassengerDestroyed
    {
        get { return _passengerDestroyed; }
        set
        {
            //Debug.Log("PassengerDestroyed set to: " + value);
            _passengerDestroyed = value;
        }
    }

    void Start()
    {
        //Debug.Log("SpawnManager instance reference: " + spawnManager.GetInstanceID());
        //aassign references
        Renderer renderer = GetComponent<Renderer>();
        myColor = renderer.material.color;
        light.color = myColor;

        if (panel1 != null)
        {
            panel1.GetComponent<MeshRenderer>().material.color = myColor;
        }
        if (panel2 != null)
        {
            panel2.GetComponent<MeshRenderer>().material.color = myColor;
        }
        
        

        //determine available colours to spawn
    }


    void Update()
    {
        /*
        usedColors = new List<Color>(spawnManager.usedColors);
        myColorsToUse = new List<Color>(usedColors);
        myColorsToUse.Remove(myColor);
        //Debug.Log(spawnManager.numberOfLocations);
        if (spawnManager != null)
        {
            PassengerCheck();
            if (SpawnTime <= 0 && spawnManager.numberOfLocations > 1 && !hasPassenger && _passengerDestroyed)
            {
                SpawnPassenger();
                //Debug.Log("spawning passenger");
            }
            else
            {
                //Debug.Log("passenger destroyed: " + passengerDestroyed);
                SpawnTime -= Time.deltaTime;
            }
        }
        else
        {
            Debug.Log("no spawn manager");
        }
        */
        usedColors = new List<Color>(spawnManager.usedColors);
        myColorsToUse.Clear();

        if (gameObject.CompareTag("CarPark"))
        {
            // Only use colors that were assigned to terminals
            myColorsToUse = new List<Color>(spawnManager.terminalUsedColors);
        }
        else if (gameObject.CompareTag("Terminal"))
        {
            // Only use colors that were assigned to car parks
            myColorsToUse = new List<Color>(spawnManager.parkUsedColors);
        }

        // Ensure the current object's color is not in the list (i.e., avoid assigning the same color to a passenger as the spawner)
        myColorsToUse.Remove(myColor);

        if (SpawnTime <= 0 && spawnManager.numberOfLocations > 1 && !hasPassenger && _passengerDestroyed)
        {
            SpawnPassenger();
        }
        else
        {
            SpawnTime -= Time.deltaTime;
        }
    }


    /*
    void SpawnPassenger() //this is what will kickoff the logic used to manage the spawn times
    {
        //spawn passenger
        if (myColorsToUse.Count <= 0)
        {
            Debug.Log("no available colours");
        }
        else
        {
            //myColorsToUse.RemoveAll(color => AreColorsSimilar(color, myColor));
            if (gameObject.CompareTag("CarPark"))
            {
                passengerColor = spawnManager.terminalUsedColors[Random.Range(0, spawnManager.terminalUsedColors.Count)];
            } else
            {
                passengerColor = spawnManager.parkUsedColors[Random.Range(0, spawnManager.parkUsedColors.Count)];
            }
            
            previousColor = passengerColor;
            if (spawnPoint != null)
            {
                Debug.Log("I can use :" + myColorsToUse);
                spawnedPassenger = Instantiate(passengerPrefab, spawnPoint.transform.position, Quaternion.identity);
                spawnedPassenger.transform.SetParent(this.transform);
                //Debug.Log("set passengers parent");
                if (spawnedPassenger != null)
                {
                    spawnedPassenger.GetComponent<Passenger>().waitingTime = passengerWaitTime;
                    //Debug.Log("just set waiting time to " + passengerWaitTime);
                    spawnedPassenger.GetComponent<Passenger>().ridingTime = passengerWaitTime;
                    //Debug.Log("just set riding time to " + passengerWaitTime);
                    spawnedPassenger.GetComponent<Passenger>().currentWaitTime = passengerWaitTime;
                    //Debug.Log("just set currentWaitTime to " + passengerWaitTime);
                } else
                {
                    Debug.Log("no spawned passenger object");
                }


                MeshRenderer body = spawnedPassenger.transform.GetChild(0).GetComponent<MeshRenderer>();
                MeshRenderer hat = spawnedPassenger.transform.GetChild(1).GetComponent<MeshRenderer>();
                if (body != null)
                {
                    body.material.color = passengerColor;
                } else
                {
                    Debug.Log("no body renderer");
                }
                if (hat != null)
                {
                    hat.material.color = passengerColor;
                } else
                {
                    Debug.Log("no hat renderer");
                }
                PassengerDestroyed = false;
            }
        }
    }
    */
    void SpawnPassenger()
    {
        if (myColorsToUse.Count <= 0)
        {
            Debug.Log("No available colours");
            return;
        }

        // Assign a color from the opposite list (if car park, use terminal's used colors; if terminal, use car park's used colors)
        if (gameObject.CompareTag("CarPark"))
        {
            if (spawnManager.terminalUsedColors.Count > 0)
            {
                passengerColor = spawnManager.terminalUsedColors[Random.Range(0, spawnManager.terminalUsedColors.Count)];
            }
            else
            {
                Debug.LogWarning("No terminal colors available.");
                return;
            }
        }
        else // "Terminal"
        {
            if (spawnManager.parkUsedColors.Count > 0)
            {
                passengerColor = spawnManager.parkUsedColors[Random.Range(0, spawnManager.parkUsedColors.Count)];
            }
            else
            {
                Debug.LogWarning("No car park colors available.");
                return;
            }
        }

        previousColor = passengerColor;

        // Now assign the color to the passenger
        if (spawnPoint != null)
        {
            spawnedPassenger = Instantiate(passengerPrefab, spawnPoint.transform.position, Quaternion.identity);
            spawnedPassenger.transform.SetParent(this.transform);

            // Set the waiting times (passenger-specific logic)
            if (spawnedPassenger != null)
            {
                spawnedPassenger.GetComponent<Passenger>().waitingTime = passengerWaitTime;
                spawnedPassenger.GetComponent<Passenger>().ridingTime = passengerWaitTime;
                spawnedPassenger.GetComponent<Passenger>().currentWaitTime = passengerWaitTime;
            }

            MeshRenderer body = spawnedPassenger.transform.GetChild(0).GetComponent<MeshRenderer>();
            MeshRenderer hat = spawnedPassenger.transform.GetChild(1).GetComponent<MeshRenderer>();

            if (body != null)
            {
                body.material.color = passengerColor;
            }
            else
            {
                Debug.Log("No body renderer found for passenger!");
            }

            if (hat != null)
            {
                hat.material.color = passengerColor;
            }
            else
            {
                Debug.Log("No hat renderer found for passenger!");
            }

            PassengerDestroyed = false;
        }
    }
}
