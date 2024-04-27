using System;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;
using Vector3 = UnityEngine.Vector3;

public class CameraScript : MonoBehaviour
{
    public CinemachineVirtualCamera forwardCam;
    public CinemachineVirtualCamera revCam;
    [SerializeField] private Rigidbody rb;
    private float velocity;

    public PlayerInputActions playerInput;
    private InputAction move;

    private void Awake()
    {
        playerInput = new PlayerInputActions();
    }

    private void OnEnable()
    {
        move = playerInput.Player.Move;
        move.Enable();
    }
    

    private void OnDisable()
    {
        move.Disable();
    }

    // Start is called before the first frame update
    private void Start()
    {
        forwardCam.Priority = 11;
    }

    // Update is called once per frame
    private void Update()
    {
        velocity = Vector3.Dot(rb.velocity, transform.forward);
        if (velocity < -1 & move.ReadValue<Vector2>().y < 0)
        {
            revCam.Priority = 11;
            forwardCam.Priority = 10;
        }
        else 
        {
            revCam.Priority = 10;
            forwardCam.Priority = 11;
        }
        
    }
}
