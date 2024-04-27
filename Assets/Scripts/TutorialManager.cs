using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialManager : MonoBehaviour
{
    public GameManager gameManager;
    public Target restaurantTarget;
    private string[] lines = 
    {
        "Welcome to Dine and Dash!\n(click to proceed)",
        "Your goal is to quickly deliver pizzas throughout the city.",
        "Faster deliveries earn more tips, which can be seen in the top right.",
        "Pick up locations are marked with blue arrows/boxes.",
        "Follow the blue arrow to the pickup location marked on the screen!",
        "Use [W] / [S] to move, [A] / [D] to turn and [SPACE] to drift!",
        "This is a pickup location.",
        "Throughout the day, you will have to pick up pizzas from these spots to deliver to drop-off locations.",
        "Each time a pizza spawns, you'll hear this sound",
        "When a pizza spawns, a delivery location also spawns.",
        "These locations are marked with different colored arrows/boxes that match the timer display on the right.",
        "Your job is to pick up and deliver pizzas BEFORE the timer runs out!",
        "Give it a try! Pick up a pizza from the delivery location by driving over it and deliver to the drop-off spot!",
        "Congrats! You completed your first day!",
        "After each day, you'll be given upgrade options to improve the stats of your car.",
        "Select an upgrade and get ready for your first real day. Good Luck!"
        
    };
    public TextMeshProUGUI text;
    public AudioSource typingSound;
    public TutorialTrigger restaurantTrigger;
    public TutorialTrigger deliveryTrigger;
    public GameObject upgradeButtons;

    private int index;
    private float charDelay = 0.032f;
    private bool canSkip = false;
    private float timer;

    private void Start()
    {
        text.text = string.Empty;
        gameManager.PauseAll();
        gameManager.PauseGame();
        Invoke(nameof(StartDialogue), 2);
    }
    private void Update()
    {
        timer += Time.deltaTime;
        if (transform.gameObject.activeInHierarchy)
        {
            if (index == 5 && restaurantTrigger.hit)
            {
                gameManager.PauseAll();
                NextLine();
            }
            else if (index == 8 && text.text == lines[index])
            {
                text.text += ':';
                gameManager.PauseGame();
                StartCoroutine(SpawnDropoffLocation());
            }
            else if (index == 12 && deliveryTrigger.hit)
            {
                StopAllCoroutines();
                text.text = string.Empty;
                StartCoroutine(DelayedDisplay());
            }
            else if (index == 15)
            {
                upgradeButtons.SetActive(true);
                canSkip = false;
                index += 1;
            }
            
            else if (Input.GetMouseButtonDown(0) && timer > 0.5f && index != 5 && index != 8 && index != 12 && index != 13 && index <= 15 && canSkip)
            {
                timer = 0;
                if (text.text == lines[index])
                {
                    NextLine();
                }
                else
                {
                    StopAllCoroutines();
                    text.text = lines[index];
                }
            }
        }
    }

    private void StartDialogue()
    {
        canSkip = true; 
        StartCoroutine(TypeLine());
    }

    private IEnumerator TypeLine()
    {
        foreach (char c in lines[index])
        {
            text.text += c;
            if(c != ' ') typingSound.Play();
            yield return new WaitForSecondsRealtime(charDelay);
        }
    }

    private void NextLine()
    {
        if (index < lines.Length - 1)
        {
            index++;
            if (index == 3)
            {
                restaurantTarget.enabled = true;
            }
            else if (index == 5)
            {
                gameManager.PauseAll();
            }
            else if (index == 12)
            {
                gameManager.PauseAll();
                gameManager.PauseGame();
            }
            text.text = string.Empty;
            StartCoroutine(TypeLine());
        }
    }

    private IEnumerator SpawnDropoffLocation()
    {
        index = 8; 
        yield return new WaitForSecondsRealtime(1);
        gameManager.StartNewDay();
        gameManager.PauseGame();
        gameManager.deliveriesToday = 1;
        NextLine();
    }

    private IEnumerator DelayedDisplay()
    {
        NextLine();
        yield return new WaitForSecondsRealtime(4);
        NextLine();
    }
}
