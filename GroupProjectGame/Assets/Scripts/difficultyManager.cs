using UnityEngine;
using System.Collections.Generic;
public class difficultyManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject[] carparkSpawners;
    public GameObject[] terminalSpawners;

    public List<GameObject> spawners;

    public overhauledPassengerSpawner spawnerScript;

    public void updateWaitTimes(float newWaitTime)
    {
        // Clear the spawners list to avoid duplicates if this function is called multiple times
        spawners.Clear();

        // Find all spawner GameObjects with the appropriate tags
        GameObject[] carparkSpawners = GameObject.FindGameObjectsWithTag("CarPark");
        GameObject[] terminalSpawners = GameObject.FindGameObjectsWithTag("Terminal");

        // Add found spawners to the list
        spawners.AddRange(carparkSpawners);
        spawners.AddRange(terminalSpawners);

        // Iterate through the spawners list and update the `passengerWaitTime` variable
        foreach (GameObject spawner in spawners)
        {
            // Get the `overhauledPassengerSpawner` script from the spawner
            overhauledPassengerSpawner spawnerScript = spawner.GetComponent<overhauledPassengerSpawner>();

            // Check if the script exists on the GameObject
            if (spawnerScript != null)
            {
                // Update the wait time
                spawnerScript.passengerWaitTime = newWaitTime;
                //Debug.Log($"Updated passengerWaitTime for spawner: {spawner.name} to {newWaitTime}");
            }
            else
            {
                // Warn if the GameObject doesn't have the expected component
                //Debug.LogWarning($"No overhauledPassengerSpawner script attached to: {spawner.name}");
            }
        }
    }

    void Start()
    {
        
    }

    public void TutorialDifficulty()
    {
        //Debug.Log("setting wait times to 20");
        updateWaitTimes(20f);
    }

    public void ImpossibleDifficulty()
    {
        //Debug.Log("setting wait times to 20");
        updateWaitTimes(1.5f);
        //set the wait times so low they have to fail
    }

    public void startingDifficulty()
    {
        //Debug.Log("setting wait times to 15");
        updateWaitTimes(15f);
        //set the wait times to something normal for start of the endless section
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
