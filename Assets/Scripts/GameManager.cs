using System;
using System.Collections;
using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    [SerializeField] private SpawnManager spawnManager;
    [SerializeField] private RestaurantManager primaryRestaurantManager;

    [SerializeField] private CinemachineVirtualCamera unfocusedCam;

    [SerializeField] private TextMeshProUGUI tipsDisplay;
    
    [Header("Game Attributes")]
    public float spawnInterval = 0;
    public float deliveriesToday = 0;
    public float day = 0;
    
    public static float Tips = 0;
    public static readonly int[] tipValues = { 1, 2, 3 };
    public static AudioSource DeliverySound;
    
    public static float DeliveriesMade = 0;
    public static float DeliveryTime = 120;

    public bool tutorial = false; 
    
    private float deliveriesSpawned = 1;
    private float timer = 0;
    private bool playing = false;

    public Transform timerContainer;

    private Settings defaultSettings;
    
    public delegate void GameStart();
    public static event GameStart OnGameStart;
    
    public delegate void GameOver();
    public static event GameOver OnGameOver;
    
    public delegate IEnumerator GameOverTimed();
    public static event GameOverTimed OnGameOverTimed;
    
    public delegate void RoundEnd();
    public static event RoundEnd OnRoundEnd;

    public delegate IEnumerator RoundEndTimed();
    public static event RoundEndTimed OnRoundEndTimed;
    
    public delegate void RoundStart();
    public static event RoundStart OnRoundStart;

    public delegate void Pause();
    public static event Pause OnPause;

    void Start()
    {
        DeliverySound = GetComponent<AudioSource>();
        defaultSettings = new Settings(spawnInterval, deliveriesToday, DeliveryTime);
        if (!tutorial)
        {
            Invoke(nameof(StartNewDay), 2);
        }
    }

    void FixedUpdate()
    {
        if (playing)
        {
            tipsDisplay.text = "Tips: $" + (Tips >= 1000 ? Math.Round(Tips / 1000f, 2) + "K" : Tips);
            
            if (DeliveriesMade == deliveriesToday)
            {
                playing = false;
                StartCoroutine(HandleRoundEnd());
            }

            timer += Time.deltaTime;

            if (timer >= (spawnInterval * (timerContainer.childCount)) && deliveriesSpawned < deliveriesToday)
            {
                timer = 0;
                spawnManager.SpawnDropoffLocation();
                primaryRestaurantManager.SpawnFoodAtAllLocations();
                deliveriesSpawned += 1;
            }
        }
    }

    public void SetupDay()
    {
        OnRoundStart?.Invoke();
        unfocusedCam.Priority = 1;
        Invoke(nameof(StartNewDay), 6);
    }
    
    // Need to update to reflect changes 
    public void SetupNewGame()
    {
        OnGameStart?.Invoke();
        spawnInterval = defaultSettings.SpawnInterval;
        deliveriesToday = defaultSettings.DeliveriesToday;
        DeliveryTime = defaultSettings.DeliveryTime;
        DeliveriesMade = 0;
        day = 0;
        Tips = 0;

        Stats.Days = 0;
        Stats.Distance = 0;
        Stats.Deliveries = 0;
        
        OnRoundStart?.Invoke();
        unfocusedCam.Priority = 1;
        Invoke(nameof(StartNewDay), 6);
    }
    
    public void StartNewDay()
    {
        day += 1;
        timer = 0;
        
        DeliveriesMade = 0;
        deliveriesToday = day * 2;
        
        spawnManager.SpawnDropoffLocation();
        primaryRestaurantManager.SpawnFoodAtAllLocations();
        
        playing = true;
        
        deliveriesSpawned = 1;
    }

    public IEnumerator handleGameOver()
    {
        OnGameOver?.Invoke();
        DoSlowMotion();
        playing = false; 
        yield return new WaitForSecondsRealtime(2);
        Time.timeScale = 1;
        Time.fixedDeltaTime = Time.timeScale * .02f;
        StartCoroutine(OnGameOverTimed?.Invoke());
        unfocusedCam.Priority = 100;
    }

    private IEnumerator HandleRoundEnd()
    {
        // Stop moving
        Stats.Days += 1;
        OnRoundEnd?.Invoke();
        DoSlowMotion();
        yield return new WaitForSecondsRealtime(2);
        Time.timeScale = 1;
        Time.fixedDeltaTime = Time.timeScale * .02f;
        
        // Pull up UI/UX
        StartCoroutine(OnRoundEndTimed?.Invoke());
        unfocusedCam.Priority = 100;
    }

    private static void DoSlowMotion()
    {
        Time.timeScale = 0.08f;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;
    }

    public void PauseAll()
    {
        OnPause?.Invoke();
    }

    public void PauseGame()
    {
        playing = !playing;
    }
}
