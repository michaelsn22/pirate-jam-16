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
        SceneManager.LoadScene("Lvl_Tutorial");
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
        StartCoroutine(ExitMissionFailure());
    }

    public void StartVictoryExitRoutine()
    {
        StartCoroutine(ExitMissionSuccess());
    }

    private IEnumerator ExitMissionFailure()
    {
        yield return new WaitForSeconds(7f);

        if (AudioManager.instance.CheckIfPlayerPassedTutorial())
        {
            SceneManager.LoadScene("Lvl_1");
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
        }
    }

    private IEnumerator ExitMissionSuccess()
    {
        yield return new WaitForSeconds(7f);

        SceneManager.LoadScene("MainMenu");
    }

    public void StartExitRoutineTutorial()
    {
        AudioManager.instance.SetPlayerPassedTutorial();
        StartCoroutine(EnterMission1());
    }

    private IEnumerator EnterMission1()
    {
        yield return new WaitForSeconds(7f);

        SceneManager.LoadScene("Lvl_1");
    }
}
