using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

public class AIController : MonoBehaviour
{
    private Line currentLine;
    private char currentDirection;
    private Line nextLine;
    private char nextLineDir;
    
    private Dictionary<Line, List<Line>> adjList;
        
    // private char nextDirection;
    [SerializeField] private Rigidbody rb;

    [Header("Suspension")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform[] rayPoints;
    public float springStiffness = 3000;
    public float damperStiffness = 300;
    public float restLength = 0.45f;
    public float springTravel = 0.25f;
    public float wheelRadius = 0.3f;
    
    public float turnRadius;
    private Transform frontLeft;
    private Transform frontRight; 
    private Transform backLeft;
    private Transform backRight;
    
    public float speed = 3;
    
    private Vector3 intersectPoint;
    private bool grounded;
    private bool[] groundedTires = {false, false, false, false};
    private float speedMultiplier = 1;

    private TrafficController _tc;

    public LayerMask car;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(currentLine.StartPos(), currentLine.EndPos());
    }

    void Start()
    {
        _tc = FindObjectOfType<TrafficController>();
        adjList = _tc.AdjList;
        frontLeft = transform.GetChild(1);
        frontRight = transform.GetChild(2);
        backLeft = transform.GetChild(3);
        backRight = transform.GetChild(4);

        var minDist = float.MaxValue;

        var keyList = adjList.Keys.ToList();

        foreach (Line line in keyList)
        {
            var distToLine = FindClosestLine(line.StartPos(), line.EndPos(), transform.position);
            // coin flip for equally distanced lines
            if (Mathf.Approximately(distToLine, minDist) && Random.Range(0, 2) == 1)
            {
                currentLine = line;
            }
            if (distToLine < minDist)
            {
                currentLine = line;
                minDist = distToLine;
            }
        }
            
        currentDirection = currentLine.Dir;
        GetNextLine();
    }
    private void FixedUpdate()
    {
        grounded = false;
        Move();
        Suspension();
        RotateWheels();
    }
    
    private float FindClosestLine(Vector3 origin, Vector3 end, Vector3 point)
    {
        // Get direction of line
        Vector3 heading = (end - origin);
        float magnitudeMax = heading.magnitude;
        heading.Normalize();

        //Do projection from the point but clamp it
        Vector3 lhs = point - origin;
        float dotP = Vector3.Dot(lhs, heading);
        dotP = Mathf.Clamp(dotP, 0f, magnitudeMax);
        return Vector3.Distance(origin + heading * dotP, point);
    }

    void Move()
    {
        Vector3 pos = transform.position;
        Quaternion rot = transform.rotation;
        
        intersectPoint = new Vector3(currentDirection is 'E' or 'W' ? nextLine.Center : pos.x, pos.y,
            currentDirection is 'N' or 'S' ? nextLine.Center : pos.z);
        
        float dist = Vector3.Distance(pos, intersectPoint);
        Vector3 velocity = transform.rotation * new Vector3(0, 0, speedMultiplier * speed);
        rb.velocity =  new Vector3(velocity.x, rb.velocity.y, velocity.z);
        
        var turnComp = nextLineDir is 'E' or 'N' && dist < turnRadius ? Mathf.Lerp(turnRadius, 1, dist / turnRadius) :
            nextLineDir is 'S' or 'W' && dist < turnRadius ? -1 * Mathf.Lerp(turnRadius, 1, dist / turnRadius) : 0;
        
        if (pos.z < intersectPoint.z && currentDirection == 'N')
        {
            var lookPos = new Vector3(currentLine.Center + turnComp, pos.y, pos.z + Mathf.Min(dist, turnRadius)) - pos;
            
            Quaternion lookRot = Quaternion.LookRotation(lookPos);
            lookRot.eulerAngles = new Vector3(transform.rotation.eulerAngles.x, lookRot.eulerAngles.y, rot.eulerAngles.z);
            transform.rotation = Quaternion.Slerp(rot, lookRot, Time.deltaTime * speed);
        }
        else if (pos.z > intersectPoint.z && currentDirection == 'S')
        {
            var lookPos = new Vector3(currentLine.Center + turnComp , pos.y, pos.z - Mathf.Min(dist, turnRadius)) - pos;
            
            Quaternion lookRot = Quaternion.LookRotation(lookPos);
            lookRot.eulerAngles = new Vector3(transform.rotation.eulerAngles.x, lookRot.eulerAngles.y, rot.eulerAngles.z);
            transform.rotation = Quaternion.Slerp(rot, lookRot, Time.deltaTime * speed);
    
        }
        else if (pos.x < intersectPoint.x && currentDirection == 'E')
        {
            var lookPos = new Vector3(pos.x + Mathf.Min(dist, turnRadius), pos.y, currentLine.Center + turnComp) - pos;
            
            Quaternion lookRot = Quaternion.LookRotation(lookPos);
            lookRot.eulerAngles = new Vector3(transform.rotation.eulerAngles.x, lookRot.eulerAngles.y, rot.eulerAngles.z);
            transform.rotation = Quaternion.Slerp(rot, lookRot, Time.deltaTime * speed);
        }
        else if (pos.x > intersectPoint.x && currentDirection == 'W')
        {
            var lookPos = new Vector3(pos.x - Mathf.Min(dist, turnRadius), pos.y, currentLine.Center + turnComp) - pos;
            
            Quaternion lookRot = Quaternion.LookRotation(lookPos);
            lookRot.eulerAngles = new Vector3(transform.rotation.eulerAngles.x, lookRot.eulerAngles.y, rot.eulerAngles.z);
            transform.rotation = Quaternion.Slerp(rot, lookRot, Time.deltaTime * speed);
        }
        // If on a new line, select next from list
        else
        {
            currentLine = nextLine;
            currentDirection = currentLine.Dir;
            
            GetNextLine();
        }
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
                
                float springCompression = (restLength - currentSpringLength) / springTravel;

                float springVelocity = Vector3.Dot(rb.GetPointVelocity(rayPoints[i].position), rayPoints[i].up);
                float dampForce = damperStiffness * springVelocity;

                float springForce = springStiffness * springCompression;

                float netForce = springForce - dampForce;

                rb.AddForceAtPosition(netForce * rayPoints[i].up, rayPoints[i].position);
                
                groundedTires[i] = true;

            }
            else
            {
                groundedTires[i] = false;
            }
        }
        // back left and right tires
        grounded = groundedTires[0] && groundedTires[1] && groundedTires[2] && groundedTires[3];
    }

    void GetNextLine()
    {
        // Random index in range (0, len list at currentLine)
        var idx = Random.Range(0, adjList[currentLine].Count);
        // North or south = Z, E/W = x
        var relevantPos = currentDirection is 'N' or 'S' ? transform.position.z : transform.position.x;
        var sign = currentDirection is 'N' or 'E' ? 1 : -1;
        
        // If now moving vertically North, next line must have a center point > your current z position + 5
        // If now moving horizontally East, next line must have a center point > current x position + 5 
        // If now moving vertically South, next line must have a center point < current position - 5
        
        // Adj list is sorted by centers (which would be the intersect in the relevant direction)
        // i.e if you're on a vertical line, center of next line is going to be Z
        if (sign * adjList[currentLine][idx].Center < sign * relevantPos)
        {
            // If you're traveling N or East, you need a random number b/t [idx+1, count)
            // Else need a number b/t [0, idx)
            idx = currentDirection is 'N' or 'E' ? adjList[currentLine].Count - 1 : 0;

        }

        nextLine = adjList[currentLine][idx];
        nextLineDir = nextLine.Dir;
    }

    void RotateWheels()
    {
        frontLeft.localEulerAngles += new Vector3(Vector3.Dot(rb.velocity, transform.forward) / 0.3f, 0, 0);
        frontRight.localEulerAngles += new Vector3(Vector3.Dot(rb.velocity, transform.forward) / 0.3f, 0, 0);
        backLeft.localEulerAngles -= new Vector3(Vector3.Dot(rb.velocity, transform.forward) / 0.3f, 0, 0);
        backRight.localEulerAngles -= new Vector3(Vector3.Dot(rb.velocity, transform.forward) / 0.3f, 0, 0);
    }
    
}
