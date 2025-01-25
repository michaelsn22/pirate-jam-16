using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class DialogueHandler : MonoBehaviour
{
    //serializable objs for dialogue canvas.
    [SerializeField] private GameObject dialogueCanvas;
    [SerializeField] private TextMeshProUGUI SpeakerText;
    [SerializeField] private TextMeshProUGUI ActualText;
    private float timeToLive = 6f;
    private float typingSpeed = 0.015f; // Time between each character changed from 0.01f on 9/26 to 0.015f
    private bool showingDialogue = false;
    private Coroutine typingCoroutine;
    [SerializeField] private string ourCharactersName;
    void Start()
    {
        dialogueCanvas.SetActive(false);
        Invoke(nameof(DelayedStartDialogue), 2f);
    }

    private void DecideOnStringForIntroDialogue()
    {
        //Debug.Log($"Selected party member: {memberName}");

        float randomDecider = Random.value;
        if (randomDecider <= 0.25f)
        {
            ShowDialogue(ourCharactersName, "Time to make this look easy!");
        }
        else if(randomDecider <= 0.5f)
        {
            ShowDialogue(ourCharactersName, "This should be fun!");
        }
        else if(randomDecider <= 0.75f)
        {
            ShowDialogue(ourCharactersName, "They don't stand a chance against me.");
        }
        else
        {
            ShowDialogue(ourCharactersName, "Don't get in my way, I'll handle this.");
        }
        //ShowDialogue(memberName, "Alright, let's do this!");
    }

    private void ShowDialogue(string Speaker, string actualText)
    {
        CancelInvoke("HideDialogue");
        showingDialogue = true;
        dialogueCanvas.SetActive(true);

        SpeakerText.text = Speaker;

        // Stop any ongoing typing animation
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        // Start the new typing animation
        typingCoroutine = StartCoroutine(AnimateText(actualText));

        Invoke(nameof(HideDialogue), timeToLive);
    }

    private IEnumerator AnimateText(string text)
    {
        ActualText.text = "";
        foreach (char c in text)
        {
            ActualText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    private void HideDialogue()
    {
        showingDialogue = false;
        dialogueCanvas.SetActive(false);
        SpeakerText.text = "";
        ActualText.text = "";
    }

    private void CelebrateEnemyKill(string memberName, GameObject deadObjReference)
    {
        float randomDecider = Random.value;
        if (randomDecider <= 0.25f)
        {
            ShowDialogue(ourCharactersName, "Another one down!");
        }
        else if(randomDecider <= 0.5f)
        {
            ShowDialogue(ourCharactersName, "I've got them!");
        }
        else if(randomDecider <= 0.75f)
        {
            ShowDialogue(ourCharactersName, "They didn't stand a chance against me.");
        }
        else
        {
            ShowDialogue(ourCharactersName, "Wooo hooo! Got one!");
        }
    }

    private void PlayHardCodedDialogue(string nameOfEntity)
    {
        ShowDialogue(nameOfEntity, "Idk put some hard coded value here if needed.");
    }

    public void PlayHardCodedDialogueExternally(string nameOfSpeaker, string textToDisplay)
    {
        ShowDialogue(nameOfSpeaker, textToDisplay);
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
            ActualText.text = ActualText.text; //Set the full text immediately
        }
    }
}
