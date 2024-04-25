using UnityEngine;
using Cinemachine;
using Vector3 = UnityEngine.Vector3;

public class CameraScript : MonoBehaviour
{
    public CinemachineVirtualCamera forwardCam;
    public CinemachineVirtualCamera revCam;
    [SerializeField] private Rigidbody rb;
    private float _velocity;

    // Start is called before the first frame update
    void Start()
    {
        forwardCam.Priority = 11;
    }

    // Update is called once per frame
    void Update()
    {
        _velocity = Vector3.Dot(rb.velocity, transform.forward);
        if (_velocity < -1 & Input.GetAxis("Vertical") < 0)
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