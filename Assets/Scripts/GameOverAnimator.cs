using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class GameOverAnimator : MonoBehaviour
{
    // Start is called before the first frame update

    public TextMeshProUGUI titleText;
    public AudioSource typingSound;
    public GameObject buttons;
    
    [Header("Stats")]
    public TextMeshProUGUI deliveries;
    public TextMeshProUGUI days;
    public TextMeshProUGUI distance;

    private string startingTitle;
    private string startingDeliveries;
    private string startingDays;
    private string startingDistance;

    private float charDelay = 0.065f;
    private float statDelay = 0.15f;
    private float sectionDelay = 0.5f; 
    
    
    private void OnEnable()
    {
        startingTitle = titleText.text;
        startingDeliveries = deliveries.text + Stats.Deliveries;
        startingDays = days.text + Stats.Days;
        startingDistance = distance.text + Math.Round(Stats.Distance / 1000f, 2) + "km";
        
        titleText.text = "";
        deliveries.text = "";
        days.text = "";
        distance.text = "";

        StartCoroutine(DisplayInformation());
        
    }

    private void OnDisable()
    {
        deliveries.text = "Deliveries Made: ";
        days.text = "Days Completed: ";
        distance.text = "Distance Traveled: ";
        buttons.SetActive(false);
    }

    IEnumerator DisplayInformation()
    {
        typingSound.pitch = 1.5f;
        yield return new WaitForSecondsRealtime(1);
        foreach (var c in startingTitle)
        {
            titleText.text += c;
            if(c != ' ') typingSound.Play();
            yield return new WaitForSecondsRealtime(charDelay);
        }
        
        yield return new WaitForSecondsRealtime(sectionDelay);
        
        foreach (var c in startingDeliveries)
        {
            deliveries.text += c;
            if(c != ' ') typingSound.Play();
            if (c == ':') { 
                yield return new WaitForSecondsRealtime(statDelay);
                typingSound.pitch = 2;
            }
            else
            {
                yield return new WaitForSecondsRealtime(charDelay);
            }
        }
        yield return new WaitForSecondsRealtime(sectionDelay);
        typingSound.pitch = 1.5f;
        foreach (var c in startingDays)
        {
            days.text += c;
            if(c != ' ') typingSound.Play();
            if (c == ':') { 
                yield return new WaitForSecondsRealtime(statDelay);
                typingSound.pitch = 2;
            }
            else
            {
                yield return new WaitForSecondsRealtime(charDelay);
            }
        }
        yield return new WaitForSecondsRealtime(sectionDelay);
        typingSound.pitch = 1.5f;
        foreach (var c in startingDistance)
        {
            distance.text += c;
            if(c != ' ') typingSound.Play();
            if (c == ':') { 
                yield return new WaitForSecondsRealtime(statDelay);
                typingSound.pitch = 2;
            }
            else
            {
                yield return new WaitForSecondsRealtime(charDelay);
            }
        }

        buttons.SetActive(true);

        yield return null;

    }

    private void Update()
    {
        if (transform.gameObject.activeInHierarchy)
        {
            if (Input.GetMouseButton(0))
            {
                charDelay = 0.045f;
                statDelay = 0.09f;
                sectionDelay = 0.3f; 
            }
            else
            {
                charDelay = 0.065f;
                statDelay = 0.15f;
                sectionDelay = 0.5f; 
            }
        }
    }

    
}
