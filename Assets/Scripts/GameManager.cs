using System;
using System.Collections;
using Cinemachine;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private SpawnManager spawnManager;
    [SerializeField] private RestaurantManager primaryRestaurantManager;

    [SerializeField] private CinemachineVirtualCamera UnfocusedCam;

    [SerializeField] private TextMeshProUGUI tipsDisplay;
    
    [Header("Game Attributes")]
    public float spawnInterval = 0;
    public float deliveriesToday = 0;
    public float deliveriesMade = 0;
    public float deliveryTime = 30;
    public float day = 0;
    
    [Header("Tips")]
    public float tips = 0;
    public int[] tipValues = { 1, 2, 3 };
    public AudioSource deliverySound;

    public bool tutorial = false; 
    
    private float _deliveriesSpawned = 1;
    private float _timer = 0;
    private bool _playing = false;

    public Transform timerContainer;

    private Settings _defaultSettings;
    
    public delegate void OnGameStart();
    public static event OnGameStart GameStart;
    
    public delegate void OnGameOver();
    public static event OnGameOver GameOver;
    
    public delegate IEnumerator OnGameOverTimed();
    public static event OnGameOverTimed GameOverTimed;
    
    public delegate void OnRoundEnd();
    public static event OnRoundEnd RoundEnd;

    public delegate IEnumerator OnRoundEndTimed();
    public static event OnRoundEndTimed RoundEndTimed;
    
    public delegate void OnRoundStart();
    public static event OnRoundStart RoundStart;

    public delegate void OnPause();
    public static event OnPause Pause;


    void Start()
    {
        _defaultSettings = new Settings(spawnInterval, deliveriesToday, deliveryTime);
        if (!tutorial)
        {
            Invoke(nameof(StartNewDay), 2);
        }
    }

    void FixedUpdate()
    {
        if (_playing)
        {
            tipsDisplay.text = "Tips: $" + (tips >= 1000 ? Math.Round(tips / 1000f, 2) + "K" : tips);
            
            if (deliveriesMade == deliveriesToday)
            {
                _playing = false;
                StartCoroutine(HandleRoundEnd());
            }

            _timer += Time.deltaTime;

            if (_timer >= (spawnInterval * (timerContainer.childCount)) && _deliveriesSpawned < deliveriesToday)
            {
                _timer = 0;
                spawnManager.SpawnDropoffLocation();
                primaryRestaurantManager.SpawnFoodAtAllLocations();
                _deliveriesSpawned += 1;
            }
        }
    }

    public void SetupDay()
    {
        RoundStart?.Invoke();
        UnfocusedCam.Priority = 1;
        Invoke(nameof(StartNewDay), 6);
    }
    
    // Need to update to reflect changes 
    public void SetupNewGame()
    {
        GameStart?.Invoke();
        spawnInterval = _defaultSettings.spawnInterval;
        deliveriesToday = _defaultSettings.deliveriesToday;
        deliveryTime = _defaultSettings.deliveryTime;
        deliveriesMade = 0;
        day = 0;
        tips = 0;

        Stats.Days = 0;
        Stats.Distance = 0;
        Stats.Deliveries = 0;
        
        RoundStart?.Invoke();
        UnfocusedCam.Priority = 1;
        Invoke(nameof(StartNewDay), 6);
    }
    
    public void StartNewDay()
    {
        day += 1;
        _timer = 0;
        
        deliveriesMade = 0;
        deliveriesToday = day * 2;
        
        spawnManager.SpawnDropoffLocation();
        primaryRestaurantManager.SpawnFoodAtAllLocations();
        
        _playing = true;
        
        _deliveriesSpawned = 1;
    }

    public IEnumerator handleGameOver()
    {
        GameOver?.Invoke();
        DoSlowMotion();
        _playing = false; 
        yield return new WaitForSecondsRealtime(2);
        Time.timeScale = 1;
        Time.fixedDeltaTime = Time.timeScale * .02f;
        StartCoroutine(GameOverTimed?.Invoke());
        UnfocusedCam.Priority = 100;
    }

    private IEnumerator HandleRoundEnd()
    {
        // Stop moving
        Stats.Days += 1;
        RoundEnd?.Invoke();
        DoSlowMotion();
        yield return new WaitForSecondsRealtime(2);
        Time.timeScale = 1;
        Time.fixedDeltaTime = Time.timeScale * .02f;
        
        // Pull up UI/UX
        StartCoroutine(RoundEndTimed?.Invoke());
        UnfocusedCam.Priority = 100;
    }

    private void DoSlowMotion()
    {
        Time.timeScale = 0.08f;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;
    }

    public void PauseAll()
    {
        Pause?.Invoke();
    }

    public void PauseGame()
    {
        _playing = !_playing;
    }
}
