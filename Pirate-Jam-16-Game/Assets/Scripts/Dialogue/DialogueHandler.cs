using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class DialogueHandler : MonoBehaviour
{
    //serializable objs for dialogue canvas.
    [SerializeField] private GameObject AngelImage;
    [SerializeField] private GameObject dialogueTextObject;
    [SerializeField] private TextMeshProUGUI dialogueText;
    private float timeToLive = 6f;
    private float typingSpeed = 0.015f; // Time between each character changed from 0.01f on 9/26 to 0.015f
    private bool showingDialogue = false;
    private Coroutine typingCoroutine;
    [SerializeField] private string ourCharactersName;

    void Start()
    {
        AngelImage.SetActive(false);
        dialogueTextObject.SetActive(false);
        Invoke(nameof(DelayedStartDialogue), 2f);
    }

    private void DecideOnStringForIntroDialogue()
    {
        if (SceneManager.GetActiveScene().name == "Lvl_Tutorial")
        {
            ShowDialogue("Im going to have to familiarize myself with the controls of this mech again. Running through the test facility wouldn't hurt.");
            return;
        }
        //Debug.Log($"Selected party member: {memberName}");

        float randomDecider = Random.value;
        if (randomDecider <= 0.25f)
        {
            ShowDialogue("Time to make this look easy!");
        }
        else if(randomDecider <= 0.5f)
        {
            ShowDialogue("I'll make them pay for what they did.");
        }
        else if(randomDecider <= 0.75f)
        {
            ShowDialogue("They don't stand a chance against me.");
        }
        else
        {
            ShowDialogue("I'm going to make them pay.");
        }
        //ShowDialogue(memberName, "Alright, let's do this!");
    }

    public void IntroTutorial1()
    {
        ShowDialogue("I can keep my momentum going by pressing space to jump, and then the W key along with either A or D together to combine their movement forces.");
    }
    public void IntroTutorial2()
    {
        ShowDialogue("If I remember correctly, those blue pads boost me up into the air...");
    }
    public void IntroTutorial3()
    {
        ShowDialogue("I know I can swap weapons with my mouse wheel... Left click to fire, R to reload.. its coming back to me.");
    }
    public void IntroTutorial4()
    {
        ShowDialogue("Time to practice on some of the older generations of bots. Do they even still use those things?");
    }

    public void ExitTutorialMessage()
    {
        ShowDialogue("Alright. I think I'm ready to take on the miltary.");
    }

    private void ShowDialogue(string dialogueText)
    {
        CancelInvoke("HideDialogue");
        showingDialogue = true;
        AngelImage.SetActive(true);
        dialogueTextObject.SetActive(true);

        // Stop any ongoing typing animation
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        // Start the new typing animation
        typingCoroutine = StartCoroutine(AnimateText(dialogueText));

        Invoke(nameof(HideDialogue), timeToLive);
    }

    private IEnumerator AnimateText(string text)
    {
        dialogueText.text = "";
        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    private void HideDialogue()
    {
        showingDialogue = false;
        AngelImage.SetActive(false);
        dialogueTextObject.SetActive(false);
        dialogueText.text = "";
    }

    private void CelebrateEnemyKill(GameObject deadObjReference)
    {
        float randomDecider = Random.value;
        if (randomDecider <= 0.25f)
        {
            ShowDialogue("Another one down!");
        }
        else if(randomDecider <= 0.5f)
        {
            ShowDialogue("I've got them!");
        }
        else if(randomDecider <= 0.75f)
        {
            ShowDialogue("They didn't stand a chance against me.");
        }
        else
        {
            ShowDialogue("Wooo hooo! Got one!");
        }
    }

    public void ReactToBossSpawning()
    {
        ShowDialogue("Time to take that thing out of commission!");
    }

    public void StartPlayerDefeatedDialogue()
    {
        ShowDialogue("Noooo!");
    }

    public void StartBossDefeatedDialogue()
    {
        ShowDialogue("That'll show em!");
    }

    public void PlayHardCodedDialogueExternally(string textToDisplay)
    {
        ShowDialogue(textToDisplay);
    }

    private void DelayedStartDialogue()
    {
        DecideOnStringForIntroDialogue();
    }

    public void CompleteTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            dialogueText.text = dialogueText.text; //Set the full text immediately
        }
    }
}
