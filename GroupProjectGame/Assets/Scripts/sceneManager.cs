
/* normal camera angles
 * 33.822 normal gameplay
 * -11.178 menu angle
 * 
 * normal camera positions
 * start position 0, 7.5, -11.2
 * end 0f, 24.7f, -36.9f
 * 
 * spawn area sizes
 * start size scale = 1,1,1
 * end size scale = 2.5026, 2.5026, 2.728835
 */

/*menu bus transform positions
             * menu spot :101, -157, -68
             * start spot:913, -341, 908
             */

using System.Collections;
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class sceneManager : MonoBehaviour
{
    public int level = 0; // 0 is the main menu, 1 is the tutorial section, 2 is the first failure zone, 3 is the endless mode, 4 is the settings page, 5 is the endscreen/leaderboard
    public SpawnManager spawnManager;
    public BusController busController;
    public scoreTracker ScoreTracker;
    public difficultyManager DifficultyManager;
    public overhauledPassengerSpawner OverhauledPassengerSpawner;
    public GameObject spawnArea;
    public GameObject endScreen;
    public GameObject cameraFollower;
    public Transform UI;
    public Transform menuUI;
    public Vector3 menuBusPosition = new Vector3(0.4367003f, 7.311564f, -9.746392f);
    public Vector3 targetMenuPosition;
    public Vector3 startMenuPosition;
    public Vector3 returnPosition;

    public GameObject menuBus;

    public TMP_Text endScreenTitle;
    public TMP_Text finalScoreText;

    public Camera mainCamera;

    public bool cameraRotated = false;
    public bool isRotating = false;
    public bool firstFailure = true;
    public bool firstTimeAtTwo = true;
    public bool firstTimeAtThree = true;
    public bool justRotated = false;
    public bool moveCamera = false;
    public bool growSpawnArea = false;
    public bool paused = false;
    public bool gameStarted = false;
    public bool firstTimeHere = true;
    public bool gameOver = false;

    float lerpTime = 0.25f;
    float elapsedTime = 0f;
    float elapsedTime2 = 0f;
    float instructionTime = 30f;

    public bool gamePaused = false;
    public int pausedLevel = 1;

    public float rotationDuration = 2f;

    public bool busMoved = false;

    public float startingDifficulty = 15f;

    public float angleMarginOfError = 0.001f;

    public Light light;                // Assign your directional light here
    public float cycleDuration = 120f;   // Duration for one full rotation (in seconds)

    private float sunElapsedTime = 0f;      // Tracks elapsed time

    public float endGameTimers = 30f;

    public Quaternion targetRotation;
    public Quaternion startRotation;
    public Quaternion currentRotation;
    public Quaternion pausedRotation;
    public Quaternion unpausedRotation;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {

        cameraFollower = GameObject.Find("cameraFollower");
        endScreen = GameObject.Find("endScreen");
        //cameraFollower.SetActive(true);
        endScreen.SetActive(false);
        spawnManager = GameObject.Find("locationSpawner").GetComponent<SpawnManager>();
        DifficultyManager = GameObject.Find("difficultyManager").GetComponent<difficultyManager>();
        spawnArea = GameObject.Find("spawnArea");
        spawnManager.gameObject.SetActive(false); // No prefabs will spawn yet
        pausedLevel = 1;
        Time.timeScale = 0;
    }

    void Start()
    {


        endGameTimers = 30f;

        firstTimeHere = true;

        lerpTime = 3f;
        elapsedTime = 0f;
        elapsedTime2 = 0f;
        instructionTime = 30f;
        busMoved = false;
        gameStarted = false;
        pausedLevel = 1;
        firstFailure = true;
        firstTimeAtTwo = true;
        firstTimeAtThree = true;
        startRotation = mainCamera.transform.rotation;
        targetMenuPosition = new Vector3(20f, 0f, 0f);
        cameraRotated = false;
        level = 0;
        busController = GameObject.FindFirstObjectByType<BusController>();
        ScoreTracker = GameObject.FindFirstObjectByType<scoreTracker>();
        menuUI = GameObject.Find("menuUi").GetComponent<Transform>();
        startMenuPosition = menuUI.transform.position;
        targetRotation = Quaternion.Euler(startRotation.eulerAngles.x + 45f,
                                           startRotation.eulerAngles.y,
                                           startRotation.eulerAngles.z);
        pausedRotation = startRotation;
        unpausedRotation = targetRotation;
    }

    IEnumerator MoveCameraSmoothly(Vector3 targetPosition, float duration)
    {
        // Store the camera's initial position
        Vector3 startPosition = mainCamera.transform.position;
        float timeElapsed = 0f;

        // Smooth transition using Lerp
        while (timeElapsed < duration)
        {
            mainCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure the camera reaches the target position
        mainCamera.transform.position = targetPosition;
    }

    IEnumerator ScaleSpawnAreaSmoothly(Vector3 targetScale, Vector3 targetPosition, float duration)
    {
        // Store the initial scale of the spawn area
        Vector3 startScale = spawnArea.transform.localScale;
        Vector3 startPosition = spawnArea.transform.position;
        float timeElapsed = 0f;

        // Smoothly scale the object using Lerp
        while (timeElapsed < duration)
        {
            spawnArea.transform.localScale = Vector3.Lerp(startScale, targetScale, timeElapsed / duration);
            spawnArea.transform.position = Vector3.Lerp(startPosition, targetPosition, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure the spawn area reaches the target scale
        spawnArea.transform.localScale = targetScale;
    }

    public IEnumerator RotateCameraSmoothly(float rotationAmount)
    {
        isRotating = true;
        Quaternion startRotation = mainCamera.transform.rotation;  // Get the current rotation
        Quaternion targetRotation = Quaternion.Euler(startRotation.eulerAngles + new Vector3(rotationAmount, 0f, 0f));  // Rotate by 45 degrees (up or down)

        float timeElapsed = 0f;
        float duration = 1f;  // You can change this to control how fast the camera rotates

        while (timeElapsed < duration)
        {
            mainCamera.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, timeElapsed / duration);
            timeElapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        
        // Ensure camera reaches the exact target rotation
        mainCamera.transform.rotation = targetRotation;
        isRotating = false;
    }

    void switchTimescale()
    {

        //Time.timeScale = Time.timeScale == 1? 0:1;

        //Debug.Log("switching timescale");
        if (Time.timeScale == 1)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }
    }

    public void pauseTheGame()
    {
        //Debug.Log("Pausing the game");
        busController.secondAudioSource.volume = 0f;
        if (!isRotating) // Ensure that the camera rotation hasn't started yet
        {
            //Debug.Log("rotating Camera");
            StartCoroutine(RotateCameraSmoothly(-45f));  // Rotate camera downwards before pausing
            cameraRotated = false;
            justRotated = true;
        }
        paused = true;
    }

    public void unpauseTheGame()
    {
        //Debug.Log("Unpausing the game");
        //Time.timeScale = 1;
        //Debug.Log("Timescale set to 1");
        level = pausedLevel; // Resume level after unpause
        //Debug.Log("Level set to " + level);
        gamePaused = false; // Game is no longer paused

        if (!isRotating && !cameraRotated)  // Ensure that camera has rotated first
        {
            StartCoroutine(RotateCameraSmoothly(45f));  // Rotate camera back to menu position before unpausing
            cameraRotated = true;
            justRotated = true;
        }
        paused = false;
    }


    // Update is called once per frame
    void Update()
    {

        sunElapsedTime += Time.deltaTime;

        // Normalize time into a 0-1 range
        float normalizedTime = (sunElapsedTime % cycleDuration) / cycleDuration;

        // Convert normalized time into a rotation angle (0 to 360 degrees)
        float sunAngle = normalizedTime * 360f;

        // Apply rotation around the X-axis
        light.transform.rotation = Quaternion.Euler(sunAngle, -30f, 0f);
        // Move bus smoothly over time until the target position is reached
        if (elapsedTime < lerpTime)
        {
            // Gradually interpolate the position based on elapsed time
            menuBus.transform.position = Vector3.Lerp(menuBus.transform.position, new Vector3(0.4367003f, 7.311564f, -9.746392f), elapsedTime / lerpTime);
            elapsedTime += Time.unscaledDeltaTime; // Increment elapsed time by frame time
        }
        else
        {
            // Ensure the bus is exactly at the target position once the interpolation is complete
            if(level != 3)
            {
                menuBus.transform.position = new Vector3(0.4367003f, 7.311564f, -9.746392f);
            }

        }
        if (gameStarted)
        {
            //Debug.Log("game has started");
            if (instructionTime < elapsedTime2)
            {
                elapsedTime2 += Time.unscaledTime;
            }
            
        }
        if (ScoreTracker.totalScore >= ScoreTracker.maxValue && level != 1)
        {
            if (!gameOver)
            {
                float finishScore = ScoreTracker.scoreTotal;
                endScreenTitle.text = "You're Getting Promoted!";
                finalScoreText.text = ("Final Score: " + finishScore.ToString());
                cameraFollower.SetActive(false);
                endScreen.SetActive(true);
                gameOver = true;
            }
            
        }
        if (ScoreTracker.totalScore <= ScoreTracker.minValue && level != 1)
        {
            if (!gameOver)
            {
                float finishScore = ScoreTracker.scoreTotal;
                endScreenTitle.text = "You're Fired!";
                finalScoreText.text = ("Final Score: " + finishScore.ToString());
                cameraFollower.SetActive(false);
                endScreen.SetActive(true);
                gameOver = true;
            }
            
        }
        {
            //win screen
        }
        if (moveCamera)
        {
            StartCoroutine(MoveCameraSmoothly(new Vector3(0f, 24.7f, -36.9f), 120));
        }
        if(growSpawnArea)
        {
            StartCoroutine(ScaleSpawnAreaSmoothly(new Vector3(2.5026f, 2.5026f, 2.728835f),new Vector3(0f, 0.01f, 1.19f), 120));
        }
        if (justRotated)
        {
            justRotated = false;
            switchTimescale();
        }
        //Debug.Log(level);
        //Debug.Log(cameraRotated);
        /*
        Quaternion currentCameraRotation = mainCamera.transform.rotation;
        Debug.Log(currentCameraRotation.eulerAngles);
        if(currentCameraRotation == Quaternion.Euler(348.82f,0f,0f))
        {
            //level = 0;
            Time.timeScale = 0;
            //Debug.Log("setting timescale to 0");
        } else
        {
            //level = 0;
            Time.timeScale = 1;
            //Debug.Log("setting timescale to 1");
        }
        */
        //UpdateTimeScaleBasedOnRotation();
        if (ScoreTracker.firstMissedPassenger)
        {
            level = 3;
        }
        if (ScoreTracker.scoreTotal >= 5 && firstFailure)
        {
            firstFailure = false;
            level = 2;
            Debug.Log("level 2");
        }

        // Checking for input to change levels or pause the game
        /*
        if (Input.GetKeyDown("space"))
        {
            level += 1;
        }
        */
        if (Input.GetKeyDown("escape"))
        {
            
            if(!paused)
            {
                pausedLevel = level;
                pauseTheGame();
            } else
            {
                unpauseTheGame();
            }
            
        }

        // The different behaviors for different levels
        if (level == 0) // Level 0 is the menu
        {
            /*menu bus transform positions
             * menu spot :101, -157, -68
             * start spot:913, -341, 908
             */
            

        }
        else if (level == 1) // Tutorial level
        {
            if(!busController.firstAudioSource.isPlaying)
            {
                busController.firstAudioSource.Play();
            }

            DifficultyManager.TutorialDifficulty();

            if (spawnManager != null)
            {
                spawnManager.maxLocations = 2;
                spawnManager.gameObject.SetActive(true);
            }
            else
            {
                Debug.Log("No spawn manager");
            }
        }
        else if (level == 2) // Failure zone
        {
            DifficultyManager.ImpossibleDifficulty();

            if (firstTimeAtTwo)
            {
                Debug.Log("level 2 again");
                spawnManager.maxLocations = 4;
                firstTimeAtTwo = false;
            }
        }
        else if (level == 3) // Endless mode
        {
            if (firstTimeAtThree)
            {
                Debug.Log("arrived at level 3");
                DifficultyManager.startingDifficulty();
                ScoreTracker.minValue = -20f;
                ScoreTracker.maxValue = 300f;
                firstTimeAtThree = false;
                moveCamera = true;
                growSpawnArea = true;
                busController.upgradingBus = true;
                spawnManager.spawnInterval = 30;
                spawnManager.maxLocations = 14;
            }
            if (spawnManager.numberOfLocations >= 14)
            {
                Debug.Log("spawned locations = 14");
                Debug.Log("time until reduced wait :" + endGameTimers);
                if(endGameTimers <= 0 && startingDifficulty > 0)
                {
                    endGameTimers = 20f;
                    startingDifficulty = Mathf.Max(0, startingDifficulty);
                    DifficultyManager.updateWaitTimes(startingDifficulty);
                    startingDifficulty -= 1.5f;
                    Debug.Log("updated wait times");
                } else
                {
                    endGameTimers -= Time.deltaTime;
                }
            }
        }
        else if (level == 4) // Settings page
        {
            // Open settings page
        }
        else if (level == 5) // End screen
        {
            // Show end screen
        }
    }
}

