using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigator : MonoBehaviour
{
    public void PlayGame()
    {
        Debug.Log("Going to scene 1.");
        SceneManager.LoadScene("SampleScene");
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
}
