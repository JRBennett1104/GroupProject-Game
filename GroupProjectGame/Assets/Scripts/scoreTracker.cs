
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class scoreTracker : MonoBehaviour
{
    public TMP_Text passengerCount;  // UI Text to display score
    public Image goodRing;           // Radial bar (Image component)
    public int scoreTotal;           // Current score
    public int totalScore;           // Total score

    public float minValue = -10f;    // Minimum score value
    public float maxValue = 10f;     // Maximum score value

    public Color positiveColor = Color.white;   // Color for positive values
    public Color negativeColor = Color.red;     // Color for negative values

    public bool firstMissedPassenger = false;

    // Start is called once before the first execution of Update
    void Start()
    {
        scoreTotal = 0;  // Initialize score
        UpdateRadialBar(totalScore);  // Update radial bar on start
    }

    void UpdateRadialBar(int currentScore)
    {
        if (currentScore >= 0f)  // Positive score
        {
            // Update clockwise behavior
            //Debug.Log(maxValue);
            goodRing.fillAmount = (Mathf.Clamp01(currentScore / maxValue));  // Fill based on positive score
            goodRing.fillClockwise = true; // Clockwise fill for positive values
            goodRing.color = positiveColor;  // Color to green for positive values
        }
        else  // Negative score
        {
            // Update counterclockwise behavior
            goodRing.fillAmount = (Mathf.Clamp01(Mathf.Abs(currentScore) / Mathf.Abs(minValue)));  // Inverse the value for counterclockwise
            goodRing.fillClockwise = false; // Counterclockwise fill for negative values
            goodRing.color = negativeColor;  // Color to red for negative values
        }
    }

    // Update is called once per frame
    void Update()
    {
        
        passengerCount.text = "Successful Dropoffs : " + scoreTotal.ToString();  // Update text with score
        //Debug.Log("Current Score Total: " + scoreTotal);  // Debug log to check the score
        UpdateRadialBar(totalScore);  // Update the radial bar each frame
    }
}
