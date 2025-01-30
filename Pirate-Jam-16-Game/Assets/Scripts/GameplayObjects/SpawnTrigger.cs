using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnTrigger : MonoBehaviour
{
    [SerializeField] private List<GameObject> entitiesToSpawn;
    [SerializeField] private int typeOfSpawner = 0;
    [SerializeField] private DialogueHandler dialogueHandler;
    private bool doneSpawn = false;
    [SerializeField] private SceneNavigator sceneNavigator;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !doneSpawn)
        {
            //Debug.Log("spawning objects!");
            ShowObjects();
        }
    }

    private void ShowObjects()
    {
        doneSpawn = true;
        
        switch (typeOfSpawner)
        {
            case 0:
                //Debug.Log("starting music 1");
                AudioManager.instance.StartBossFightMusic1();
                break;
            case 1:
                dialogueHandler.ReactToBossSpawning();
                AudioManager.instance.StartBossFightMusic2();
                break;
            case 2:
                dialogueHandler.IntroTutorial1();
                break;
            case 3:
                dialogueHandler.IntroTutorial2();
                break;
            case 4:
                dialogueHandler.IntroTutorial3();
                break;
            case 5:
                dialogueHandler.IntroTutorial4();
                break;
            case 6:
                dialogueHandler.ExitTutorialMessage();
                sceneNavigator.StartExitRoutineTutorial();
                break;
        }

        foreach (var enemy in entitiesToSpawn)
        {
            enemy.SetActive(true);
        }
    }
}
