using UnityEngine;
using UnityEngine.UI;

public class DisplayFPS : MonoBehaviour
{
    private float deltaTime = 0.0f;

    void Update()
    {
        // Accumulate time for calculating FPS
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
    }

    void OnGUI()
    {
        int width = Screen.width, height = Screen.height;

        // Create a style for the FPS display text
        GUIStyle style = new GUIStyle();

        Rect rect = new Rect(0, 0, width, height * 2 / 100);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = height * 2 / 100;
        style.normal.textColor = Color.white;

        // Calculate and display FPS
        float fps = 1.0f / deltaTime;
        string text = string.Format("{0:0.} FPS", fps);
        GUI.Label(rect, text, style);
    }
}
