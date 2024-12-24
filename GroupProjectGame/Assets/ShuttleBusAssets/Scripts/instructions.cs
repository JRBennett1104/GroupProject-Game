using UnityEngine;

public class CanvasLerpMover : MonoBehaviour
{
    [SerializeField]
    public RectTransform canvasRectTransform; // Reference to the RectTransform of the Canvas.

    [SerializeField]
    public Vector3 targetPosition;

    public GameObject target;

    public bool gameStarted;

    [SerializeField]
    private float lerpDuration = 720f; // Duration for the lerp (in seconds).

    private Vector3 startingPosition; // Canvas's initial position.
    private float timeElapsed = 0f; // Tracks how much time has passed.
    private bool isMoving = false; // Indicates whether the lerp is active.

    void Start()
    {
        gameStarted = false;
        target = GameObject.Find("instTarget");
        if (canvasRectTransform == null)
        {
            canvasRectTransform = GetComponent<RectTransform>();
        }
        targetPosition = canvasRectTransform.InverseTransformPoint(target.transform.position); // Convert to local space
        startingPosition = canvasRectTransform.localPosition; // Store the initial local position
    }

    void Update()
    {
        if(canvasRectTransform.position != targetPosition)
        {
            if (isMoving)
            {
                // Update elapsed time
                timeElapsed += Time.deltaTime;

                // Calculate the interpolation ratio, making sure it's clamped between 0 and 1
                float lerpRatio = timeElapsed / lerpDuration;

                // Lerp between the starting and target positions based on elapsed time.
                canvasRectTransform.localPosition = Vector3.Lerp(
                    startingPosition, // Start from the initial position
                    targetPosition,   // Move to the target position
                    Mathf.Clamp01(lerpRatio) // Ensure the ratio is between 0 and 1
                );

                // Stop moving if the time is up.
                if (timeElapsed >= lerpDuration)
                {
                    isMoving = false;
                    canvasRectTransform.localPosition = targetPosition; // Snap to the target position.
                }
            }
        }
        
    }

    // Public method to trigger the move.
    public void MoveCanvas()
    {
        timeElapsed = 0f; // Reset time tracking
        isMoving = true;  // Start the move
    }

    // Optionally, reset the canvas back to the starting position.
    public void ResetCanvas()
    {
        canvasRectTransform.localPosition = startingPosition;
        isMoving = false;
    }
}
