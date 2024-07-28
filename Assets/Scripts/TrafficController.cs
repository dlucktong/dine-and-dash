using System.Collections.Generic;
using System.IO;
using UnityEngine;
using DefaultNamespace;

public class TrafficController : MonoBehaviour
{

    public Dictionary<Line, List<Line>> AdjList;
    public Transform[] cars;

    // Start is called before the first frame update
    void Start()
    {
        AdjList = new Dictionary<Line, List<Line>>();
        GetDictionaryFromFile();
        for (int i = 0; i < AdjList.Count/2; i++)
        {
            Instantiate(cars[Random.Range(0, cars.Length)], transform);
        }
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