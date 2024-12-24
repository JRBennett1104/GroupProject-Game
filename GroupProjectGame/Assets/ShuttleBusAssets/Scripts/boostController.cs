using UnityEngine;
using UnityEngine.UI;

public class BoostTimerUI : MonoBehaviour
{
    public Transform cameraTransform; // Reference to the camera transform
    public Transform timerFillTransform; // The fill part of your radial timer UI (e.g. Image or RawImage)

    public BusController busController; // Reference to the BusController script

    void Update()
    {
        // Ensure the timer always faces the camera
        Vector3 lookDirection = cameraTransform.position - transform.position;
        lookDirection.y = 0; // Lock to horizontal plane to prevent vertical tilting
        transform.rotation = Quaternion.LookRotation(lookDirection);

        // Update the radial fill based on boost time and cooldown
    }

    // Update the radial fill based on the boost duration and cooldown

    public void SetRadialFillAmount(float amount, float fullAmount)
    {
        if (timerFillTransform != null)
        {
            var image = timerFillTransform.GetComponent<Image>();
            if (image != null)
            {
                image.fillAmount = amount / fullAmount;
            }
        }
    }
}
