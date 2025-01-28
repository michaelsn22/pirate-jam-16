using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnTrigger : MonoBehaviour
{
    [SerializeField] private List<GameObject> entitiesToSpawn;
    [SerializeField] private int typeOfSpawner = 0;
    [SerializeField] private DialogueHandler dialogueHandler;
    private bool doneSpawn = false;

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
                //dialogueHandler.ReactToBossSpawning();
                AudioManager.instance.StartBossFightMusic2();
                break;
        }

        foreach (var enemy in entitiesToSpawn)
        {
            enemy.SetActive(true);
        }
    }
}
