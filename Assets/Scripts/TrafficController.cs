using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using DefaultNamespace;
using Random = UnityEngine.Random;

public class TrafficController : MonoBehaviour
{

    public Dictionary<Line, List<Line>> AdjList;
    public Transform[] cars;
    public int numCars;
    public List<Transform> roads;

    public Transform AiCarContainer;
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Road"))
        {
            roads.Remove(other.transform);
        }

        if (other.CompareTag("AI"))
        {
            Destroy(other.gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var cameraFrustrum = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        if (other.CompareTag("Road"))
        {
            // Ensure new cars are spawned within the radius
            var spawnPos = transform.position + Vector3.ClampMagnitude(other.transform.position - transform.position, 100); 
            roads.Add(other.transform);
            if (GeometryUtility.TestPlanesAABB(cameraFrustrum, other.bounds))
            {
                RaycastHit hit;
                var camPos = Camera.main.transform.position;
                if(Physics.Raycast(camPos, other.transform.position - camPos, out hit, 100)) {
                    if (hit.transform.CompareTag("Building"))
                    {
                        print("spawning in hidden");
                        Instantiate(cars[Random.Range(0, cars.Length)], spawnPos, Quaternion.identity, AiCarContainer);
                    }
                }
            }
            else
            {
                Instantiate(cars[Random.Range(0, cars.Length)], spawnPos, Quaternion.identity, AiCarContainer);
            }
        }
        
    }

    // Start is called before the first frame update
    void Start()
    {
        print("Start");
        AdjList = new Dictionary<Line, List<Line>>();
        GetDictionaryFromFile();
        // for (int i = 0; i < numCars; i++)
        // {
        //     
        // }
    }

    void GetDictionaryFromFile()
    {
        var list = new List<Line>();
        var lines = File.ReadLines("Assets/Pathing/adjlist.txt");
        foreach (var line in lines)
        {
            if (line == "")
            {
                continue;
            }

            string[] kv = line.Split(":");
            int idx = list.FindIndex(l => l.ToString() == kv[0]);

            Line key;
            if (idx != -1)
            {
                key = list[idx];
            }
            else
            {
                key = new Line(kv[0]);
                list.Add(key);
            }

            AdjList.Add(key, new List<Line>());

            string[] adjLines = kv[1].Split("/");
            foreach (var adj in adjLines)
            {
                idx = list.FindIndex(l => l.ToString() == adj);
                if (idx != -1)
                {
                    AdjList[key].Add(list[idx]);
                }
                else
                {
                    Line itm = new Line(adj);
                    AdjList[key].Add(itm);
                    list.Add(itm);
                }

                AdjList[key].Sort((l1, l2) => l1.Center < l2.Center ? -1 : 1);
            }

        }

    }
}