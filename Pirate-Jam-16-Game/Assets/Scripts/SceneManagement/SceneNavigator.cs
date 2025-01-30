using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigator : MonoBehaviour
{
    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            //Make the cursor visible again
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    public void PlayGame()
    {
        Debug.Log("Going to scene 1.");
        SceneManager.LoadScene("Lvl_1");
    }
    public void OpenSettings()
    {
        Debug.Log("Opening settings");
    }
    public void QuitGame()
    {
        Debug.Log("Quitting game");
        Application.Quit();
    }

    public void StartExitRoutine()
    {
        StartCoroutine(ExitMission());
    }

    private IEnumerator ExitMission()
    {
        yield return new WaitForSeconds(7f);

        SceneManager.LoadScene("MainMenu");
    }
}
