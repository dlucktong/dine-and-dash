using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class CarController : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private TextMeshProUGUI speedDisplay;


    [Header("Car Settings")] 
    public float forwardAcceleration = 5;
    public float reverseAcceleration = 4;
    public float maxSpeed = 50;
    public float turnStrength = 45;
    public float maxWheelTurn = 30;
    public float drag = 6;
    public float slip = 45;
    public float torqueconst = 100;
    public float sideslipFric = 0.2f;
    public float flipConst = 10000f;
    public AnimationCurve engineTorqueCurve;

    [Header("Suspension Settings")] [SerializeField]
    private Transform[] rayPoints;
    public float springStiffness = 4000;
    public float damperStiffness = 400;
    public float restLength = 0.5f;
    public float springTravel = 0.25f;
    public float wheelRadius = 0.3f;

    [Header("Tires")]
    [SerializeField] private Transform frontLeft;
    [SerializeField] private Transform frontRight;
    [SerializeField] private Transform backLeft;
    [SerializeField] private Transform backRight;
    [SerializeField] private TrailRenderer backLeftTrail;
    [SerializeField] private TrailRenderer backRightTrail;
    public float brakeMultiplier = -1;
    
    [Header("Ground")]
    [SerializeField] private LayerMask groundLayer;

    private float _speedInput, _turnInput;
    private float _brakeInput;
    private bool _grounded;
    private float _driftMultiplier;
    private float _driftAngle;
    private float _velocity;
    private bool _canMove = true;
    private float _frontWheelArm;
    private float _backWheelArm;
    private bool[] _groundedTires = {false, false, false, false};
    private Vector3 _lastPos;
    private List<GameObject> _roads;
    
    

    void Start()
    {
        rb.transform.parent = null;
        transform.eulerAngles = rb.transform.eulerAngles;
        Vector3 flatVec = new Vector3(1, 0, 1);

        // Moment arms from body CM to front and back wheels
        // Note these are not perfect, since they're 3d vectors they overshoot actual distances by a bit
        _frontWheelArm = (((frontLeft.position + frontRight.position) / 2) - rb.worldCenterOfMass).magnitude;
        _backWheelArm = (((backLeft.position + backRight.position) / 2) - rb.worldCenterOfMass).magnitude;
        _lastPos = transform.position;
        
        _roads = GameObject.FindGameObjectsWithTag("Road").ToList();
    }

    void OnEnable()
    {
        GameManager.GameStart += ResetPosition;
        GameManager.GameOver += DisablePlayerInput;
        GameManager.RoundEnd += DisablePlayerInput;
        GameManager.RoundStart += EnablePlayerInput;
        GameManager.Pause += Pause;
    }

    private void OnDisable()
    {
        GameManager.GameStart -= ResetPosition;
        GameManager.GameOver -= DisablePlayerInput;
        GameManager.RoundEnd -= DisablePlayerInput;
        GameManager.RoundStart -= EnablePlayerInput;
        GameManager.Pause -= Pause;
    }

    void Update()
    {
        _velocity = Vector3.Dot(rb.velocity, transform.forward);

        speedDisplay.text = "Speed: " + Math.Round(Math.Abs(_velocity), 2);

        _speedInput = 0;
        // Accelerate
        if (Input.GetAxis("Vertical") > 0)
        {
            _speedInput += Input.GetAxis("Vertical") * forwardAcceleration * 300;
        }

        // Reverse
        else if (Input.GetAxis("Vertical") < 0)
        {
            _speedInput += Input.GetAxis("Vertical") * reverseAcceleration * 300;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            // Sort the roads by closest, take the first one
            // Surprisingly not slow in my testing?
            _roads.Sort((a,b) => Vector3.Distance(a.transform.position, transform.position) < Vector3.Distance(b.transform.position, transform.position) ? -1 : 1);
            rb.position = _roads[0].transform.position + new Vector3(0, 2, 0);
            rb.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        }

        // Inputs for turn and handbrake
        _turnInput = Input.GetAxis("Horizontal");
        _brakeInput = Input.GetAxis("Jump");

        // Turn left and right tires
        frontLeft.localRotation = Quaternion.Euler(frontLeft.localRotation.eulerAngles.x,
            (_turnInput * maxWheelTurn) - 180, frontLeft.localRotation.eulerAngles.z);

        frontRight.localRotation = Quaternion.Euler(frontRight.localRotation.eulerAngles.x,
            (_turnInput * maxWheelTurn) - 180, frontRight.localRotation.eulerAngles.z);

        backLeft.localEulerAngles -= _brakeInput > 0 ? Vector3.zero : new Vector3(0, 0, _velocity / 0.36f);
        backRight.localEulerAngles -= _brakeInput > 0 ? Vector3.zero : new Vector3(0, 0, _velocity / 0.36f);
        frontLeft.localEulerAngles += new Vector3(0, 0, _velocity / 0.36f);
        frontRight.localEulerAngles += new Vector3(0, 0, _velocity / 0.36f);

        // Debugging
        Debug.DrawRay(transform.position, rb.velocity);
        Debug.DrawRay(transform.position, transform.forward, Color.blue);
        
    }

    private void FixedUpdate()
    {
        transform.position = rb.transform.position;
        rb.velocity = Vector3.MoveTowards(rb.velocity, Vector3.zero, drag * Time.deltaTime);
        rb.velocity = Vector3.RotateTowards(rb.velocity, transform.forward * Mathf.Sign(_velocity),
            slip * Time.deltaTime * Mathf.Deg2Rad, 0);

        _grounded = false;
        Suspension();
        
        _driftAngle = Mathf.Abs(Vector3.Angle((_velocity < 0 ? -1 : (_velocity > 0 ? 1 : 0)) * transform.forward, rb.velocity.normalized));
        
        if (_grounded && _canMove)
        {
            // Normal drag
            rb.drag = 0;
            drag = 12 * (1+_driftAngle/90);
            
       //     print("drift angle:" + _driftAngle);
         //   print("velo:" + _velocity);
        //    print("brake: " + _brakeInput);
            
            backLeftTrail.emitting = _driftAngle > 10 || (Math.Abs(_velocity) > 0.5f && _brakeInput > 0);
            backRightTrail.emitting = _driftAngle > 10 || (Math.Abs(_velocity) > 0.5f && _brakeInput > 0);
            
            // Turning is reversed if backing up
            Turn(_velocity < 0);

            if (Mathf.Abs(_speedInput) > 0)
            {
                Accelerate();
            }

            // If handbrake is pulled
            if (_brakeInput > 0)
            {
                Drift();
                drag *= 1.5f;
            }

        }
        // If the car is not grounded
        else
        {
            if (Input.GetKey("x") && _velocity < 0.1)
            {
                //rb.AddTorque(transform.forward * flipConst * Mathf.Sign(Vector3.SignedAngle(Vector3.up, transform.up, transform.forward)));
                if (Vector3.Angle(Vector3.up, transform.forward) < 5 ||
                    Vector3.Angle(Vector3.down, transform.forward) < 5)
                {
                    rb.AddTorque(transform.right * flipConst);
                }
                else
                {
                    rb.AddTorque(transform.forward * flipConst);
                }
            }
            // Should apply different drag and gravity force but idt this actually works atm.           
            rb.AddForce(Vector3.up * -3000);
        
        }
        
        UpdateTotalDistance();

    }

    void DisablePlayerInput()
    {
        _canMove = false;
        backLeftTrail.emitting = false;
        backRightTrail.emitting = false;
    }

    void EnablePlayerInput()
    {
        _canMove = true;
    }

    void Suspension()
    {
        for(int i = 0; i < rayPoints.Length; i++)
        {
            RaycastHit hit;
            float maxLength = restLength + springTravel;

            if (Physics.Raycast(rayPoints[i].position, -rayPoints[i].up, out hit, maxLength + wheelRadius, groundLayer))
            {
                float currentSpringLength = hit.distance - wheelRadius;
                
                Vector3 wheelPos = rayPoints[i].position;
                wheelPos.y -= currentSpringLength;

                rayPoints[i].GetChild(0).position = wheelPos;

                float springCompression = (restLength - currentSpringLength) / springTravel;

                float springVelocity = Vector3.Dot(rb.GetPointVelocity(rayPoints[i].position), rayPoints[i].up);
                float dampForce = damperStiffness * springVelocity;

                float springForce = springStiffness * springCompression;

                float netForce = springForce - dampForce;

                rb.AddForceAtPosition(netForce * rayPoints[i].up, rayPoints[i].position);

                Debug.DrawLine(rayPoints[i].position, hit.point, Color.red);

                _groundedTires[i] = true;

            }
            else
            {
                Debug.DrawLine(rayPoints[i].position, rayPoints[i].position + (wheelRadius + maxLength) * -rayPoints[i].up,
                    Color.green);
                _groundedTires[i] = false;
            }
        }
        // back left and right tires
        _grounded = _groundedTires[0] && _groundedTires[1] && _groundedTires[2] && _groundedTires[3];
    }

    void Accelerate()
    {
        rb.AddForce(_speedInput * engineTorqueCurve.Evaluate(Math.Abs(_velocity)) * Mathf.Cos(transform.rotation.x) * transform.forward);

        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);
        }
    }

    void Turn(bool reversed)
    {
        if (reversed)
        {
            transform.Rotate( -_turnInput * turnStrength *
                              ((Math.Abs(_velocity) > 1 || Mathf.Abs(Input.GetAxis("Horizontal")) > 1) ? 1 : 0) *
                              Time.deltaTime * transform.up);
        }
        else
        {
            transform.Rotate( _turnInput * turnStrength *
                              ((Math.Abs(_velocity) > 1 || Mathf.Abs(Input.GetAxis("Horizontal")) > 1) ? 1 : 0) *
                              Time.deltaTime * transform.up);
        }
    }

    void Drift()
        {
            // Calculate wheel vector offset from forward of car
            float t1 = (float)Math.Cos((Math.PI / 180) * -_turnInput * maxWheelTurn) * transform.forward.x;
            float t2 = (float)Math.Sin((Math.PI / 180) * -_turnInput * maxWheelTurn) * transform.forward.z;
            float t3 = (float)Math.Sin((Math.PI / 180) * -_turnInput * maxWheelTurn) * transform.forward.x;
            float t4 = (float)Math.Cos((Math.PI / 180) * -_turnInput * maxWheelTurn) * transform.forward.z;
            
            Vector3 wheelAngle = new Vector3(t1 - t2, 0, t3 + t4);
            
            // Show vector on car
            Debug.DrawRay(transform.position, wheelAngle, Color.red);

            // Get angle between front wheels and velocity vector
            float frontWheelVelAngle = Vector3.SignedAngle(rb.velocity, wheelAngle, transform.up);
            // Get angle between back wheels and velocity vector
            float backWheelVelAngle = Vector3.SignedAngle(rb.velocity, transform.forward, transform.up);

            //TODO: Calculate torque on car based on relative angles of wheels.

            // Initiate the drift by increasing the rotation of the car
            if (_driftAngle < 180)
            {
                rb.AddTorque(_backWheelArm * (float)Math.Sin((Math.PI / 180)* backWheelVelAngle) * (float)(rb.mass/2) * 9.8f * sideslipFric * torqueconst * -transform.up );
                rb.AddTorque(_frontWheelArm * (float)Math.Sin((Math.PI / 180) * frontWheelVelAngle) * (float)(rb.mass/2) * 9.8f * sideslipFric * torqueconst * transform.up);
                // Last term should be replaced with m*g*coeff of fric         
            }

            // Slow down force of back wheels always opposes velocity if handbrake is down
            rb.AddForce((rb.mass / 2f) * 9.8f * sideslipFric * brakeMultiplier * rb.velocity);
            // Slow down force of front wheels is only normal component

        }

    private void UpdateTotalDistance()
    {
        Stats.Distance += Vector3.Distance(transform.position, _lastPos);
        _lastPos = transform.position;
    }

    private void ResetPosition()
    {
        transform.position = new Vector3(-185.800003f, 51.5134163f, -259.98999f);
        transform.rotation = new Quaternion(0f, 0f, 0f, 1);
        rb.velocity = new Vector3(0f, 0f, 0f);
    }

    private void Pause()
    {
        _canMove = !_canMove;
        rb.velocity = Vector3.zero;
    }
    
    public void ApplyUpgrade(string upgradeName)
    {
        switch (upgradeName)
        {
            case "Engine Boost":
                maxSpeed += 10;  // Increase max speed by 10
                break;
            case "Advanced Drivetrain":
                turnStrength += 5;  // Improve responsiveness of drifting
                break;
            case "Turbocharger":
                forwardAcceleration += 1;  // Increase forward acceleration
                break;
            case "Reinforced Tires":
                brakeMultiplier -= 0.25f;  // Increase brake multiplier to make hand-break more effective
                break;
            case "High-Performance Brakes":
                reverseAcceleration += 1;  // Increases breaking power
                break;
            default:
                Debug.Log("Upgrade not recognized.");
                break;
        }
    }
}