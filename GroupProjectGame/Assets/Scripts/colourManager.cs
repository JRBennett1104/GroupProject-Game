using UnityEngine;
using System.Collections.Generic;

public class ColorManager : MonoBehaviour
{
    // List of colors to be used for car parks, terminals, and passengers
    public List<Color> availableColors = new List<Color>();

    private int colorIndex = 0; // Tracks the color that will be used next

    // Assigns the next color from the list and wraps around once it goes through all colors
    public Color GetNextColor()
    {
        if (availableColors.Count == 0)
        {
            Debug.LogWarning("No colors available in the list.");
            return Color.white; // Return default if no colors are available
        }

        // Get the next color and increment the index
        Color color = availableColors[colorIndex];

        // Increment the index and reset if it's out of bounds
        colorIndex = (colorIndex + 1) % availableColors.Count;

        return color;
    }
}