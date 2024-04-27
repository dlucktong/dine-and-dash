using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarAudio : MonoBehaviour
{
    [SerializeField] private AudioSource carAudio;
    [SerializeField] private List<AudioSource> driftAudio;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private TrailRenderer trail;
    [SerializeField] private CarController car;

    [SerializeField] private AnimationCurve gearCurve;
    
    private bool mute = false;
    private float timer = 0;
    private bool paused = false;

    public PlayerInputActions playerInput;
    private InputAction move;

    private void Awake()
    {
        playerInput = new PlayerInputActions();
    }
    
    private void OnEnable()
    {
        GameManager.OnGameOver += MuteAudio;
        GameManager.OnRoundEnd += MuteAudio;
        GameManager.OnRoundStart += StartAudio;
        GameManager.OnPause += Pause;

        move = playerInput.Player.Move;
        move.Enable();
    }
    private void OnDisable()
    {
        GameManager.OnGameOver -= MuteAudio;
        GameManager.OnRoundEnd -= MuteAudio;
        GameManager.OnRoundStart -= StartAudio; 
        GameManager.OnPause -= Pause;
        
        move.Disable();
    }


    private void FixedUpdate()
    {
        if (mute)
        {
            timer += Time.deltaTime / Time.timeScale;
            if (timer < 2.6)
            {
                carAudio.pitch = Mathf.Lerp(carAudio.pitch, 0.35f, 3 * (Time.deltaTime / Time.timeScale));
            }
            else
            {
                carAudio.pitch = Mathf.Lerp(carAudio.pitch, 0.8f, 4 * (Time.deltaTime / Time.timeScale));
                carAudio.volume = Mathf.Lerp(carAudio.volume, 0f, 2.5f * (Time.deltaTime / Time.timeScale));
            }

        }
        else if(!paused)
        {
            float currentSpeed = Vector3.Dot(rb.velocity, rb.transform.forward);
            carAudio.volume = Mathf.Clamp(move.ReadValue<Vector2>().y, 0.5f, 0.8f);
            carAudio.pitch = gearCurve.Evaluate(Mathf.Abs(currentSpeed) / car.maxSpeed);
        }
        else
        {
            carAudio.volume = 0.5f;
            carAudio.pitch = 0.7f;
        }

        foreach (AudioSource audio in driftAudio)
        {
            audio.mute = !trail.emitting;
        }
    }

    private void MuteAudio()
    {
        mute = true;
        timer = 0;
    }


    private void StartAudio()
    {
        mute = false;
        timer = 0;
    }

    private void Pause()
    {
        paused = !paused;
    }
    
}
