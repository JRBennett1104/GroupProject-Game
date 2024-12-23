/*
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;  // For loading scenes
using UnityEngine.UI;             // For interacting with UI buttons

public class MenuController : MonoBehaviour
{

    public sceneManager SceneManager;

    private void Start()
    {
        SceneManager = GameObject.Find("sceneManager").GetComponent<sceneManager>();
    }

    private IEnumerator UnpauseAfterRotation()
    {
        yield return SceneManager.RotateCameraSmoothly(SceneManager.startRotation, SceneManager.targetRotation);
        SceneManager.cameraRotated = true;
        ResumeGame();
    }

    private void ResumeGame()
    {
        Time.timeScale = 1;
        SceneManager.level = SceneManager.pausedLevel;
        SceneManager.gamePaused = false;
    }

    // Function to load a specific scene (e.g. "Main Game" scene)
    public void StartGame()
    {
        if (!SceneManager.cameraRotated)
        {
            StartCoroutine(UnpauseAfterRotation());
        }
        else
        {
            ResumeGame();
        }
    }

    // Function to load the options menu
    public void ShowOptions()
    {
        SceneManager.level = 4;
    }

    // Function to quit the game
    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();

        // For testing in the editor, use this instead of Application.Quit()
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
*/
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public sceneManager sceneManager;

    public CanvasLerpMover canvasLerpMover;

    private void Start()
    {
        canvasLerpMover = GameObject.Find("instructions").GetComponent<CanvasLerpMover>();
        sceneManager = GameObject.Find("sceneManager").GetComponent<sceneManager>();
    }

    // Start the game and trigger the camera rotation down
    public void StartGame()
    {
        sceneManager.unpauseTheGame();
        if(!sceneManager.gameStarted)
        {
            canvasLerpMover.MoveCanvas();
        }
        sceneManager.gameStarted = true;


    }

    // Show options (For now just handle settings page transition)
    public void ShowOptions()
    {
        sceneManager.level = 4;
    }

    // Quit the game with a clean exit
    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void restartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        canvasLerpMover.ResetCanvas();
    }

}
