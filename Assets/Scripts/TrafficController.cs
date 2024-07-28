using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DefaultNamespace;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

public class TrafficController : MonoBehaviour
{

    public LayerMask road;

    private List<GameObject> junctions;
    private List<GameObject> tsplits;
    private List<GameObject> curves;

    private HashSet<KeyValuePair<Vector3, Vector3>> top;
    private HashSet<KeyValuePair<Vector3, Vector3>> bottom;
    private HashSet<KeyValuePair<Vector3, Vector3>> left;
    private HashSet<KeyValuePair<Vector3, Vector3>> right;

    private List<Line> lines = new List<Line>();
    private Dictionary<Line, List<Line>> dictionary = new Dictionary<Line, List<Line>>();
    private List<Line> vertical = new List<Line>();
    private List<Line> horizontal = new List<Line>();
    private List<Vector3> h2v = new List<Vector3>();
    private List<Vector3> v2h = new List<Vector3>();
    private List<Vector3> both = new List<Vector3>();

    public bool drawVertical = true;
    public bool drawHorizontal = true;
    
    public bool drawH2v = true;
    public bool drawV2h = true; 

    
    private void OnDrawGizmosSelected()
    {
        foreach (var line in vertical)
        {
            if (!drawVertical)
            {
                break;
            } 
            Gizmos.color = (line.Dir == 'N' ? Color.red : Color.green);
            Gizmos.DrawLine(line.StartPos(), line.EndPos());
        }

        foreach (var line in horizontal)
        {
            if (!drawHorizontal)
            {
                break;
            } 
            Gizmos.color = (line.Dir == 'E' ? Color.yellow : Color.cyan);
            Gizmos.DrawLine(line.StartPos(), line.EndPos());
        }

        Gizmos.color = Color.blue;
        foreach (var inter in h2v)
        {
            if (!drawH2v)
            {
                break;
            } 
            Gizmos.DrawSphere(inter, 1);
        }

        Gizmos.color = Color.magenta;
        foreach (var inter in v2h)
        {
            if (!drawV2h)
            {
                break;
            } 
            Gizmos.DrawSphere(inter, 1);
        }

        Gizmos.color = Color.black;
        foreach (var inter in both)
        {
            if (!drawV2h)
            {
                break;
            } 
            Gizmos.DrawSphere(inter, 1);
        }
    }

    public void GenerateConnectedList()
    {
        
        List<Line> verticalLines = new List<Line>();
        List<Line> horizontalLines = new List<Line>();
        List<float[]> intervalList = new List<float[]>();
        
        foreach (var line in lines)
        {
            if (line.Start == line.End)
            {
                continue;
            }
            if (line.Type == 'H')
            {
                horizontalLines.Add(line);
            }
            else
            {
                verticalLines.Add(line);
            }
        }

        var vertQuery = verticalLines.GroupBy(line => line.Center, line => line.AsInterval()).OrderBy(line => line.Key);
        
        int iter = 1;
        foreach (var intervalGroup in vertQuery)
        {
            intervalList.Clear();
            foreach(var interval in intervalGroup.OrderBy(x => x[0]))
            {
                if (intervalList.Count == 0 || intervalList[^1][1] < interval[0])
                {
                    intervalList.Add(interval);
                }
                
                else
                {
                    intervalList[^1][1] = Mathf.Max(intervalList[^1][1], interval[1]);
                }
                
            }

            foreach (var e in intervalList)
            {
                if (iter % 2 == 0)
                {
                    vertical.Add(new Line(e[0], e[1], intervalGroup.Key, 'V', 'N'));
                }
                else
                {
                    
                    vertical.Add(new Line(e[1], e[0], intervalGroup.Key, 'V', 'S'));
                }
            }

            iter++;

        }
        
        
        var horizQuery = horizontalLines.GroupBy(line => line.Center, line => line.AsInterval()).OrderBy(line => line.Key);
        
        iter = 1;
        foreach (var intervalGroup in horizQuery)
        {
            intervalList.Clear();
            foreach(var interval in intervalGroup.OrderBy(x => x[0]))
            {
                if (intervalList.Count == 0 || intervalList[^1][1] < interval[0])
                {
                    intervalList.Add(interval);
                }
                
                else
                {
                    intervalList[^1][1] = Mathf.Max(intervalList[^1][1], interval[1]);
                }
                
            }

            foreach (var e in intervalList)
            {
                if (iter % 2 == 0)
                {
                    horizontal.Add(new Line(e[1], e[0], intervalGroup.Key, 'H',  'W'));
                }
                else
                {
                    
                    horizontal.Add(new Line(e[0], e[1], intervalGroup.Key, 'H', 'E'));
                }
            }

            iter++;

        }

        bool green = true;
        // intersection handler
        Queue<Line> greenLines = new Queue<Line>();
        foreach (var vert in vertical)
        {
            if (greenLines.Count > 0 && Mathf.Approximately(vert.Center - 5, greenLines.Peek().Center))
            {
                green = false;
                greenLines.Dequeue();
            }
            else
            {
                green = true;
                greenLines.Enqueue(vert);
            }
            foreach (var horiz in horizontal)
            {
                Vector3 inter = FindIntersection(vert.StartPos(), vert.EndPos(), horiz.StartPos(), horiz.EndPos());
                var dir = horiz.Dir;
                if (!inter.Equals(Vector3.zero))
                {
                    // Green lines
                    if (green)
                    {
                        if (inter == vert.StartPos())
                        {
                            if (!dictionary.ContainsKey(horiz))
                            {
                                dictionary.Add(horiz, new List<Line>());
                            }
                            dictionary[horiz].Add(vert);
                            h2v.Add(inter);
                        }
                        else if (inter == vert.EndPos())
                        {
                            if (!dictionary.ContainsKey(vert))
                            {
                                dictionary.Add(vert, new List<Line>());
                            }
                            dictionary[vert].Add(horiz);
                            v2h.Add(inter);
                        }
                        
                        // blue lines are West
                        else if (dir == 'W')
                        {
                            if (inter == horiz.EndPos())
                            {
                                if (!dictionary.ContainsKey(horiz))
                                {
                                    dictionary.Add(horiz, new List<Line>());
                                }
                                dictionary[horiz].Add(vert);
                                h2v.Add(inter);
                            }
                            else
                            {
                                if (!dictionary.ContainsKey(vert))
                                {
                                    dictionary.Add(vert, new List<Line>());
                                }
                                dictionary[vert].Add(horiz);
                                v2h.Add(inter);
                            }
                            
                        }
                        // yellow
                        else
                        {
                            if (inter == horiz.StartPos())
                            {
                                if (!dictionary.ContainsKey(vert))
                                {
                                    dictionary.Add(vert, new List<Line>());
                                }
                                dictionary[vert].Add(horiz);
                                v2h.Add(inter);
                            }
                            else
                            {
                                if (!dictionary.ContainsKey(horiz))
                                {
                                    dictionary.Add(horiz, new List<Line>());
                                }
                                dictionary[horiz].Add(vert);
                                h2v.Add(inter);
                            }
                        }
                    }
                    
                    // Red lines
                    else
                    {
                        if (inter == vert.StartPos())
                        {
                            if (!dictionary.ContainsKey(horiz))
                            {
                                dictionary.Add(horiz, new List<Line>());
                            }
                            dictionary[horiz].Add(vert);
                            h2v.Add(inter);
                        }
                        
                        else if (inter == vert.EndPos())
                        {
                            if (!dictionary.ContainsKey(vert))
                            {
                                dictionary.Add(vert, new List<Line>());
                            }
                            dictionary[vert].Add(horiz);
                            v2h.Add(inter);
                        }
                        
                        // Yellow lines are East
                        else if (dir == 'E')
                        {
                            if (inter == horiz.EndPos())
                            {
                                if (!dictionary.ContainsKey(horiz))
                                {
                                    dictionary.Add(horiz, new List<Line>());
                                }
                                dictionary[horiz].Add(vert);
                                h2v.Add(inter);
                            }
                            else
                            {
                                if (!dictionary.ContainsKey(vert))
                                {
                                    dictionary.Add(vert, new List<Line>());
                                }
                                dictionary[vert].Add(horiz);
                                v2h.Add(inter);
                            }
                            
                        }
                        // blue
                        else
                        {
                            if (inter == horiz.StartPos())
                            {
                                if (!dictionary.ContainsKey(vert))
                                {
                                    dictionary.Add(vert, new List<Line>());
                                }
                                dictionary[vert].Add(horiz);
                                v2h.Add(inter);
                            }
                            else
                            {
                                if (!dictionary.ContainsKey(horiz))
                                {
                                    dictionary.Add(horiz, new List<Line>());
                                }
                                dictionary[horiz].Add(vert);
                                h2v.Add(inter);
                                
                            }
                            
                        }
                    }
                    
                }

            }

        }

        string docPath = "Assets/Pathing";
        using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, "adjlist.txt"), false))
        {
            foreach (var kvp in dictionary)
            {
                outputFile.Write(kvp.Key + ":" + String.Join("/", kvp.Value));
                outputFile.WriteLine();
            }
        }
    }

    private Vector3 FindIntersection(Vector3 v1s, Vector3 v1e, Vector3 v2s, Vector3 v2e)
    {

        float t = ((v1s.x - v2s.x) * (v2s.z - v2e.z) - (v1s.z - v2s.z) * (v2s.x - v2e.x)) / ((v1s.x - v2e.x) * (v2s.z - v2e.z) - (v1s.z - v1e.z) * (v2s.x - v2e.x));
        float u = -((v1s.x - v1e.x) * (v1s.z - v2s.z) - (v1s.z - v1e.z) * (v1s.x - v2s.x)) / ((v1s.x - v2e.x) * (v2s.z - v2e.z) - (v1s.z - v1e.z) * (v2s.x - v2e.x));
        
        if (0 <= t && t <= 1 && 0 <= u && u <= 1)
        {
            float x = v1s.x + t * (v1e.x - v1s.x);
            float z = v1s.z + t * (v1e.z - v1s.z);

            return new Vector3(x, 100, z);
        }
        return Vector3.zero;
    }

    public void GenerateLineSet()
    {
        lines.Clear();
        h2v.Clear();
        v2h.Clear();
        both.Clear();
        top = new HashSet<KeyValuePair<Vector3, Vector3>>();
        bottom = new HashSet<KeyValuePair<Vector3, Vector3>>();
        left = new HashSet<KeyValuePair<Vector3, Vector3>>();
        right = new HashSet<KeyValuePair<Vector3, Vector3>>();
        
        junctions = new List<GameObject> (GameObject.FindGameObjectsWithTag("Junction"));
        tsplits = new List<GameObject>(GameObject.FindGameObjectsWithTag("Tsplit"));
        curves = new List<GameObject>(GameObject.FindGameObjectsWithTag("Curve"));

        float[,] directions = { { 16, 0 }, { -16, 0 }, { 0, 16 }, { 0, -16 } };
        
        foreach (var go in junctions.ToArray())
        {
            for (int i = 0; i < directions.GetLength(0); i++)
            {
                var pos = go.transform.position;
                // pos.y += 2;
                var searchPos = pos;
                searchPos.x += directions[i, 0];
                searchPos.z += directions[i, 1];

                Collider[] hitColliders = Physics.OverlapSphere(searchPos, 7, road);

                while (hitColliders.Length > 0)
                {
                    var t = hitColliders[0].transform.position;
                    // searchPos.x = t.x;
                    searchPos.y = t.y;
                    // searchPos.z = t.z;

                    if (hitColliders[0].CompareTag("Tsplit"))
                    {
                        break;
                    }
                    
                    if (hitColliders[0].CompareTag("Junction"))
                    {
                        pos.y = 100;
                        searchPos.y = 100;
                        // traveling + z
                        if (directions[i, 1] > 0)
                        {
                            pos.x += 2.5f;
                            searchPos.x += 2.5f;

                            lines.Add(new Line(Mathf.Min(pos.z, searchPos.z), Mathf.Max(pos.z, searchPos.z), pos.x, 'V', 'N'));
                            pos.x -= 5;
                            searchPos.x -= 5;
                             
                            lines.Add(new Line(Mathf.Max(pos.z, searchPos.z), Mathf.Min(pos.z, searchPos.z), pos.x, 'V', 'S'));
                            
                        }
                        // traveling - z
                        else if (directions[i, 1] < 0)
                        {
                            pos.x += 2.5f;
                            searchPos.x += 2.5f;
                            
                            lines.Add(new Line(Mathf.Min(pos.z, searchPos.z), Mathf.Max(pos.z, searchPos.z), pos.x, 'V', 'N'));
                            pos.x -= 5;
                            searchPos.x -= 5;
                            
                            lines.Add(new Line(Mathf.Max(pos.z, searchPos.z), Mathf.Min(pos.z, searchPos.z), pos.x, 'V', 'S'));
                        }
                        // traveling + x
                        else if (directions[i, 0] > 0)
                        {
                            pos.z += 2.5f;
                            searchPos.z += 2.5f;
                            
                            lines.Add(new Line(Mathf.Min(pos.x, searchPos.x), Mathf.Max(pos.x, searchPos.x), pos.z, 'H', 'W'));
                            pos.z -= 5;
                            searchPos.z -= 5;
                            
                            lines.Add(new Line(Mathf.Min(pos.x, searchPos.x), Mathf.Max(pos.x, searchPos.x), pos.z, 'H', 'E'));
                        }
                        // traveling - x
                        else
                        {
                            pos.z += 2.5f;
                            searchPos.z += 2.5f;
                            
                            lines.Add(new Line(Mathf.Min(pos.x, searchPos.x), Mathf.Max(pos.x, searchPos.x), pos.z, 'H', 'W'));
                            pos.z -= 5;
                            searchPos.z -= 5;
                            
                            lines.Add(new Line(Mathf.Min(pos.x, searchPos.x), Mathf.Max(pos.x, searchPos.x), pos.z, 'H', 'E'));
                        }
                        break;
                    }

                    if (hitColliders[0].CompareTag("Curve"))
                    {
                        pos.y = 100;
                        searchPos.y = 100;
                        // traveling + z
                        if (directions[i, 1] > 0)
                        {
                            pos.x += 2.5f;
                            searchPos.x += 2.5f;
                            searchPos.z += Mathf.Approximately(hitColliders[0].transform.rotation.eulerAngles.y, 270) ? 2.5f : -2.5f;
                            
                            lines.Add(new Line(Mathf.Min(pos.z, searchPos.z), Mathf.Max(pos.z, searchPos.z), pos.x, 'V', 'N'));
                            pos.x -= 5;
                            searchPos.x -= 5;
                            searchPos.z += Mathf.Approximately(hitColliders[0].transform.rotation.eulerAngles.y, 270) ? -5 : 5;
                            
                            lines.Add(new Line(Mathf.Min(pos.z, searchPos.z), Mathf.Max(pos.z, searchPos.z), pos.x, 'V', 'S'));
                            
                        }
                        // traveling - z
                        else if (directions[i, 1] < 0)
                        {
                            pos.x += 2.5f;
                            searchPos.x += 2.5f;
                            searchPos.z += 2.5f;
                            lines.Add(new Line(Mathf.Min(pos.z, searchPos.z), Mathf.Max(pos.z, searchPos.z), pos.x, 'V', 'N'));
                            pos.x -= 5;
                            searchPos.x -= 5;
                            searchPos.z -= 5;
                            
                            lines.Add(new Line(Mathf.Min(pos.z, searchPos.z), Mathf.Max(pos.z, searchPos.z), pos.x, 'V', 'S'));
                        }
                        // traveling + x
                        else if (directions[i, 0] > 0)
                        {
                            pos.z += 2.5f;
                            searchPos.z += 2.5f;
                            searchPos.x -= 2.5f;
                            
                            lines.Add(new Line(Mathf.Min(pos.x, searchPos.x), Mathf.Max(pos.x, searchPos.x), pos.z, 'H', 'W'));
                            pos.z -= 5;
                            searchPos.z -= 5;
                            searchPos.x += 5;
                            
                            lines.Add(new Line(Mathf.Min(pos.x, searchPos.x), Mathf.Max(pos.x, searchPos.x), pos.z, 'H', 'E'));
                        }
                        // traveling - x
                        else
                        {
                            pos.z += 2.5f;
                            searchPos.z += 2.5f;
                            searchPos.x += 2.5f;
                            
                            lines.Add(new Line(Mathf.Min(pos.x, searchPos.x), Mathf.Max(pos.x, searchPos.x), pos.z, 'H', 'W'));
                            pos.z -= 5;
                            searchPos.z -= 5;
                            searchPos.x -= 5;
                            
                            lines.Add(new Line(Mathf.Min(pos.x, searchPos.x), Mathf.Max(pos.x, searchPos.x), pos.z, 'H', 'E'));
                        }
                        break;
                    }
                    searchPos.x += directions[i, 0];
                    searchPos.z += directions[i, 1];
                    hitColliders = Physics.OverlapSphere(searchPos, 7, road);
                }
            }
        }
        
        // Junction, +x is straight edge default
        // 90deg, -z
        // 180, -x
        // 270, +z
        foreach (var go in tsplits.ToArray())
        {
            for (int i = 0; i < directions.GetLength(0); i++)
            {
                var pos = go.transform.position;
                var rot = go.transform.rotation.eulerAngles.y;
                // pos.y += 2;
                var searchPos = pos;
                searchPos.x += directions[i, 0];
                searchPos.z += directions[i, 1];

                Collider[] hitColliders = Physics.OverlapSphere(searchPos, 7, road);

                while (hitColliders.Length > 0)
                {
                    searchPos.y = hitColliders[0].transform.position.y;
                    
                    if (hitColliders[0].CompareTag("Junction"))
                    {
                        pos.y = 100;
                        searchPos.y = 100;
                        // traveling + z
                        if (directions[i, 1] > 0)
                        {
                            if (Mathf.Approximately(rot, 90))
                            {
                                pos.z -= 2.5f;
                            }
                            pos.x += 2.5f;
                            searchPos.x += 2.5f;
                            
                            lines.Add(new Line(Mathf.Min(pos.z, searchPos.z), Mathf.Max(pos.z, searchPos.z), pos.x, 'V', 'N'));
                            pos.x -= 5;
                            searchPos.x -= 5;
                            
                            lines.Add(new Line(Mathf.Min(pos.z, searchPos.z), Mathf.Max(pos.z, searchPos.z), pos.x, 'V', 'S'));
                            
                        }
                        // traveling - z
                        else if (directions[i, 1] < 0)
                        {
                            if (Mathf.Approximately(rot, 270))
                            {
                                pos.z += 2.5f;
                            }
                            pos.x += 2.5f;
                            searchPos.x += 2.5f;
                            
                            lines.Add(new Line(Mathf.Min(pos.z, searchPos.z), Mathf.Max(pos.z, searchPos.z), pos.x, 'V', 'N'));
                            
                            pos.x -= 5;
                            searchPos.x -= 5;
                            
                            
                            lines.Add(new Line(Mathf.Min(pos.z, searchPos.z), Mathf.Max(pos.z, searchPos.z), pos.x, 'V', 'S'));
                        }
                        // traveling + x
                        else if (directions[i, 0] > 0)
                        {
                            if (Mathf.Approximately(rot, 180))
                            {
                                pos.x -= 2.5f;
                            }
                            pos.z += 2.5f;
                            searchPos.z += 2.5f;
                            
                            lines.Add(new Line(Mathf.Min(pos.x, searchPos.x), Mathf.Max(pos.x, searchPos.x), pos.z, 'H', 'W'));
                            pos.z -= 5;
                            searchPos.z -= 5;
                            
                            lines.Add(new Line(Mathf.Min(pos.x, searchPos.x), Mathf.Max(pos.x, searchPos.x), pos.z, 'H', 'E'));
                        }
                        // traveling - x
                        else
                        {
                            if (Mathf.Approximately(rot, 0))
                            {
                                pos.x += 2.5f;
                            }
                            pos.z += 2.5f;
                            searchPos.z += 2.5f;
                            
                            lines.Add(new Line(Mathf.Min(pos.x, searchPos.x), Mathf.Max(pos.x, searchPos.x), pos.z, 'H', 'W'));
                            pos.z -= 5;
                            searchPos.z -= 5;
                            
                            lines.Add(new Line(Mathf.Min(pos.x, searchPos.x), Mathf.Max(pos.x, searchPos.x), pos.z, 'H', 'E'));
                        }
                        break;
                    }

                    if (hitColliders[0].CompareTag("Curve"))
                    {
                        pos.y = 100;
                        searchPos.y = 100;
                        // traveling + z
                        if (directions[i, 1] > 0)
                        {
                            if (Mathf.Approximately(rot, 90))
                            {
                                pos.z -= 2.5f;
                            }

                            pos.x += 2.5f;
                            searchPos.x += 2.5f;
                            searchPos.z += Mathf.Approximately(hitColliders[0].transform.rotation.eulerAngles.y, 270) ? 2.5f : -2.5f;
                            
                            lines.Add(new Line(Mathf.Min(pos.z, searchPos.z), Mathf.Max(pos.z, searchPos.z), pos.x, 'V', 'N'));
                            pos.x -= 5;
                            searchPos.x -= 5;
                            searchPos.z += Mathf.Approximately(hitColliders[0].transform.rotation.eulerAngles.y, 270) ? -5 : 5;
                            
                            lines.Add(new Line(Mathf.Min(pos.z, searchPos.z), Mathf.Max(pos.z, searchPos.z), pos.x, 'V', 'S'));
                            
                        }
                        // traveling - z
                        else if (directions[i, 1] < 0)
                        {
                            if (Mathf.Approximately(rot, 270))
                            {
                                pos.z += 2.5f;
                            }
                            pos.x += 2.5f;
                            searchPos.x += 2.5f;
                            searchPos.z += Mathf.Approximately(hitColliders[0].transform.rotation.eulerAngles.y, 90) ? 2.5f : -2.5f;
                            
                            lines.Add(new Line(Mathf.Min(pos.z, searchPos.z), Mathf.Max(pos.z, searchPos.z), pos.x, 'V', 'N'));
                            pos.x -= 5;
                            searchPos.x -= 5;
                            searchPos.z += Mathf.Approximately(hitColliders[0].transform.rotation.eulerAngles.y, 90) ? -5 : 5;
                            
                            lines.Add(new Line(Mathf.Min(pos.z, searchPos.z), Mathf.Max(pos.z, searchPos.z), pos.x, 'V', 'S'));
                        }
                        // traveling + x
                        else if (directions[i, 0] > 0)
                        {
                            if (Mathf.Approximately(rot, 180))
                            {
                                pos.x -= 2.5f;
                            }
                            pos.z += 2.5f;
                            searchPos.z += 2.5f;
                            searchPos.x += Mathf.Approximately(hitColliders[0].transform.rotation.eulerAngles.y, 270) ? 2.5f : -2.5f;
                            
                            lines.Add(new Line(Mathf.Min(pos.x, searchPos.x), Mathf.Max(pos.x, searchPos.x), pos.z, 'H', 'W'));
                            pos.z -= 5;
                            searchPos.z -= 5;
                            searchPos.x += Mathf.Approximately(hitColliders[0].transform.rotation.eulerAngles.y, 270) ? -5 : 5;
                            
                            lines.Add(new Line(Mathf.Min(pos.x, searchPos.x), Mathf.Max(pos.x, searchPos.x), pos.z, 'H', 'E'));
                        }
                        // traveling - x
                        else
                        {
                            if (Mathf.Approximately(rot, 0))
                            {
                                pos.x += 2.5f;
                            }
                            pos.z += 2.5f;
                            searchPos.z += 2.5f;
                            searchPos.x += Mathf.Approximately(hitColliders[0].transform.rotation.eulerAngles.y, 180) ? -2.5f : 2.5f;
                            
                            lines.Add(new Line(Mathf.Min(pos.x, searchPos.x), Mathf.Max(pos.x, searchPos.x), pos.z, 'H', 'W'));
                            pos.z -= 5;
                            searchPos.z -= 5;
                            searchPos.x += Mathf.Approximately(hitColliders[0].transform.rotation.eulerAngles.y, 180) ? 5 : -5;
                            
                            lines.Add(new Line(Mathf.Min(pos.x, searchPos.x), Mathf.Max(pos.x, searchPos.x), pos.z, 'H', 'E'));
                        }
                        break;
                    }

                    if (hitColliders[0].CompareTag("Tsplit"))
                    {
                        pos.y = 100;
                        searchPos.y = 100;
                        // traveling + z
                        if (directions[i, 1] > 0)
                        {
                            if (Mathf.Approximately(rot, 90))
                            {
                                pos.z -= 2.5f;
                            }
                            if (Mathf.Approximately(hitColliders[0].transform.rotation.eulerAngles.y, 270))
                            {
                                searchPos.z += 2.5f;
                            }
                            pos.x += 2.5f;
                            searchPos.x += 2.5f;
                            
                            lines.Add(new Line(Mathf.Min(pos.z, searchPos.z), Mathf.Max(pos.z, searchPos.z), pos.x, 'V', 'N'));
                            pos.x -= 5;
                            searchPos.x -= 5;
                            
                            lines.Add(new Line(Mathf.Min(pos.z, searchPos.z), Mathf.Max(pos.z, searchPos.z), pos.x, 'V', 'S'));
                            
                        }
                        // traveling - z
                        else if (directions[i, 1] < 0)
                        {
                            if (Mathf.Approximately(rot, 270))
                            {
                                pos.z += 2.5f;
                            }
                            if (Mathf.Approximately(hitColliders[0].transform.rotation.eulerAngles.y, 90))
                            {
                                searchPos.z -= 2.5f;
                            }
                            pos.x += 2.5f;
                            searchPos.x += 2.5f;
                            
                            lines.Add(new Line(Mathf.Min(pos.z, searchPos.z), Mathf.Max(pos.z, searchPos.z), pos.x, 'V', 'N'));
                            pos.x -= 5;
                            searchPos.x -= 5;
                            
                            lines.Add(new Line(Mathf.Min(pos.z, searchPos.z), Mathf.Max(pos.z, searchPos.z), pos.x, 'V', 'S'));
                        }
                        // traveling + x
                        else if (directions[i, 0] > 0)
                        {
                            if (Mathf.Approximately(rot, 180))
                            {
                                pos.x -= 2.5f;
                            }
                            if (Mathf.Approximately(hitColliders[0].transform.rotation.eulerAngles.y, 0))
                            {
                                searchPos.x += 2.5f;
                            }
                            pos.z += 2.5f;
                            searchPos.z += 2.5f;
                            
                            lines.Add(new Line(Mathf.Min(pos.x, searchPos.x), Mathf.Max(pos.x, searchPos.x), pos.z, 'H', 'W'));
                            pos.z -= 5;
                            searchPos.z -= 5;
                            
                            lines.Add(new Line(Mathf.Min(pos.x, searchPos.x), Mathf.Max(pos.x, searchPos.x), pos.z, 'H', 'E'));
                        }
                        // traveling - x
                        else
                        {
                            if (Mathf.Approximately(rot, 0))
                            {
                                pos.x -= 2.5f;
                            }
                            if (Mathf.Approximately(hitColliders[0].transform.rotation.eulerAngles.y, 180))
                            {
                                searchPos.x += 2.5f;
                            }
                            pos.z += 2.5f;
                            searchPos.z += 2.5f;
                            
                            lines.Add(new Line(Mathf.Min(pos.x, searchPos.x), Mathf.Max(pos.x, searchPos.x), pos.z, 'H', 'W'));
                            pos.z -= 5;
                            searchPos.z -= 5;
                            
                            lines.Add(new Line(Mathf.Min(pos.x, searchPos.x), Mathf.Max(pos.x, searchPos.x), pos.z, 'H', 'E'));
                        }
                        break;
                    }
                    
                    searchPos.x += directions[i, 0];
                    searchPos.z += directions[i, 1];
                    hitColliders = Physics.OverlapSphere(searchPos, 7, road);
                }
            }
        }

        foreach (var go in curves.ToArray())
        {
            for (int i = 0; i < directions.GetLength(0); i++)
            {
                var pos = go.transform.position;
                var rot = go.transform.rotation.eulerAngles.y;
                // pos.y += 2;
                var searchPos = pos;
                searchPos.x += directions[i, 0];
                searchPos.z += directions[i, 1];
                
                Collider[] hitColliders = Physics.OverlapSphere(searchPos, 7, road);

                while (hitColliders.Length > 0)
                {
                    searchPos.y = hitColliders[0].transform.position.y;

                    if (hitColliders[0].CompareTag("Junction"))
                    {
                        break;
                    }

                    if (hitColliders[0].CompareTag("Tsplit"))
                    {
                        break;
                    }

                    if (hitColliders[0].CompareTag("Curve"))
                    {
                        var searchRot = hitColliders[0].transform.rotation.eulerAngles.y;
                        pos.y = 100;
                        searchPos.y = 100;
                        // traveling + z
                        if (directions[i, 1] > 0)
                        {
                            pos.x += 2.5f;
                            searchPos.x += 2.5f;
                            
                            // if the origin is 0
                            if (Mathf.Approximately(rot, 0))
                            {
                                // search is 180
                                if(Mathf.Approximately(searchRot, 180)) {
                                    pos.z -= 2.5f;
                                    searchPos.z -= 2.5f;
                                    lines.Add(new Line(Mathf.Min(pos.z, searchPos.z), Mathf.Max(pos.z, searchPos.z), pos.x, 'V', 'N'));
                                    pos.x -= 5;
                                    searchPos.x -= 5;
                                    pos.z += 5f;
                                    searchPos.z += 5f;
                                }
                                // else, 90
                                else
                                {
                                    pos.z -= 2.5f;
                                    searchPos.z += 2.5f;
                                    lines.Add(new Line(Mathf.Min(pos.z, searchPos.z), Mathf.Max(pos.z, searchPos.z), pos.x, 'V', 'N'));
                                    pos.x -= 5;
                                    searchPos.x -= 5;
                                    pos.z += 5f;
                                    searchPos.z -= 5f;
                                }
                            }
                            
                            // if the origin is 90 
                            else
                            {
                                //search is 270
                                if (Mathf.Approximately(searchRot, 270))
                                {
                                    pos.z += 2.5f;
                                    searchPos.z += 2.5f;
                                    lines.Add(new Line(Mathf.Min(pos.z, searchPos.z), Mathf.Max(pos.z, searchPos.z), pos.x, 'V', 'N'));
                                    pos.x -= 5;
                                    searchPos.x -= 5;
                                    pos.z -= 5f;
                                    searchPos.z -= 5f;
                                }
                                //else, 90
                                else
                                {
                                    pos.z += 2.5f;
                                    searchPos.z -= 2.5f;

                                    lines.Add(new Line(Mathf.Min(pos.z, searchPos.z), Mathf.Max(pos.z, searchPos.z), pos.x, 'V', 'N'));
                                    pos.x -= 5;
                                    searchPos.x -= 5;
                                    pos.z -= 5f;
                                    searchPos.z += 5f;
                                }
                            }
                            
                            lines.Add(new Line(Mathf.Min(pos.z, searchPos.z), Mathf.Max(pos.z, searchPos.z), pos.x, 'V', 'S'));
                            
                        }
                        // traveling - z
                        else if (directions[i, 1] < 0)
                        {
                            pos.x += 2.5f;
                            searchPos.x += 2.5f;
                            // if the origin is 180
                            if (Mathf.Approximately(rot, 180))
                            {
                                // search is 0
                                if(Mathf.Approximately(searchRot, 0)) {
                                    pos.z -= 2.5f;
                                    searchPos.z -= 2.5f;
                                    lines.Add(new Line(Mathf.Min(pos.z, searchPos.z), Mathf.Max(pos.z, searchPos.z), pos.x, 'V', 'N'));
                                    pos.x -= 5;
                                    searchPos.x -= 5;
                                    pos.z += 5f;
                                    searchPos.z += 5f;
                                }
                                // else, 90
                                else
                                {
                                    pos.z -= 2.5f;
                                    searchPos.z += 2.5f;
                                    lines.Add(new Line(Mathf.Min(pos.z, searchPos.z), Mathf.Max(pos.z, searchPos.z), pos.x, 'V', 'N'));
                                    pos.x -= 5;
                                    searchPos.x -= 5;
                                    pos.z += 5f;
                                    searchPos.z -= 5f;
                                }
                            }
                            
                            // if the origin is 270 
                            else
                            {
                                //search is 90
                                if (Mathf.Approximately(searchRot, 90))
                                {
                                    pos.z += 2.5f;
                                    searchPos.z += 2.5f;
                                    lines.Add(new Line(Mathf.Min(pos.z, searchPos.z), Mathf.Max(pos.z, searchPos.z), pos.x, 'V', 'N'));
                                    pos.x -= 5;
                                    searchPos.x -= 5;
                                    pos.z -= 5f;
                                    searchPos.z -= 5f;
                                }
                                //else, 90
                                else
                                {
                                    pos.z += 2.5f;
                                    searchPos.z -= 2.5f;
                                    lines.Add(new Line(Mathf.Min(pos.z, searchPos.z), Mathf.Max(pos.z, searchPos.z), pos.x, 'V', 'N'));
                                    pos.x -= 5;
                                    searchPos.x -= 5;
                                    pos.z -= 5f;
                                    searchPos.z += 5f;
                                }
                            }
                            
                            lines.Add(new Line(Mathf.Min(pos.z, searchPos.z), Mathf.Max(pos.z, searchPos.z), pos.x, 'V', 'S'));
                        }
                        // traveling + x
                        else if (directions[i, 0] > 0)
                        {
                            pos.z += 2.5f;
                            searchPos.z += 2.5f;
                            if (Mathf.Approximately(rot, 90))
                            {
                                // search is 270
                                if(Mathf.Approximately(searchRot, 270)) {
                                    pos.x += 2.5f;
                                    searchPos.x += 2.5f;
                                    lines.Add(new Line(Mathf.Min(pos.x, searchPos.x), Mathf.Max(pos.x, searchPos.x), pos.z, 'H', 'W'));
                                    pos.z -= 5;
                                    searchPos.z -= 5;
                                    pos.x -= 5f;
                                    searchPos.x -= 5f;
                                }
                                // else, 0
                                else
                                {
                                    pos.x += 2.5f;
                                    searchPos.x -= 2.5f;
                                    lines.Add(new Line(Mathf.Min(pos.x, searchPos.x), Mathf.Max(pos.x, searchPos.x), pos.z, 'H', 'W'));
                                    pos.z -= 5;
                                    searchPos.z -= 5;
                                    pos.x -= 5f;
                                    searchPos.x += 5f;
                                }
                            }
                            
                            // the origin is 180
                            else
                            {
                                //search is 0
                                if (Mathf.Approximately(searchRot, 0))
                                {
                                    pos.x -= 2.5f;
                                    searchPos.x -= 2.5f;
                                    lines.Add(new Line(Mathf.Min(pos.x, searchPos.x), Mathf.Max(pos.x, searchPos.x), pos.z, 'H', 'W'));
                                    pos.z -= 5;
                                    searchPos.z -= 5;
                                    pos.x += 5f;
                                    searchPos.x += 5f;
                                }
                                //else, 270
                                else
                                {
                                    pos.x -= 2.5f;
                                    searchPos.x += 2.5f;
                                    lines.Add(new Line(Mathf.Min(pos.x, searchPos.x), Mathf.Max(pos.x, searchPos.x), pos.z, 'H', 'W'));
                                    pos.z -= 5;
                                    searchPos.z -= 5;
                                    pos.x += 5f;
                                    searchPos.x -= 5f;
                                }
                            }
                            lines.Add(new Line(Mathf.Min(pos.x, searchPos.x), Mathf.Max(pos.x, searchPos.x), pos.z, 'H', 'E'));
                        }
                        // traveling - x
                        else
                        {
                            pos.z += 2.5f;
                            searchPos.z += 2.5f;
                            if (Mathf.Approximately(rot, 270))
                            {
                                // search is 180
                                if(Mathf.Approximately(searchRot, 180)) {
                                    pos.x += 2.5f;
                                    searchPos.x -= 2.5f;
                                    lines.Add(new Line(Mathf.Min(pos.x, searchPos.x), Mathf.Max(pos.x, searchPos.x), pos.z, 'H', 'W'));
                                    pos.z -= 5;
                                    searchPos.z -= 5;
                                    pos.x -= 5f;
                                    searchPos.x += 5f;
                                }
                                // else, 90
                                else
                                {
                                    pos.x += 2.5f;
                                    searchPos.x += 2.5f;
                                    lines.Add(new Line(Mathf.Min(pos.x, searchPos.x), Mathf.Max(pos.x, searchPos.x), pos.z, 'H', 'W'));
                                    pos.z -= 5;
                                    searchPos.z -= 5;
                                    pos.x -= 5f;
                                    searchPos.x -= 5f;
                                }
                            }
                            
                            // the origin is 0
                            else
                            {
                                //search is 180
                                if (Mathf.Approximately(searchRot, 180))
                                {
                                    pos.x -= 2.5f;
                                    searchPos.x -= 2.5f;
                                    lines.Add(new Line(Mathf.Min(pos.x, searchPos.x), Mathf.Max(pos.x, searchPos.x), pos.z, 'H', 'W'));
                                    pos.z -= 5;
                                    searchPos.z -= 5;
                                    pos.x += 5f;
                                    searchPos.x += 5f;
                                }
                                //else, 90
                                else
                                {
                                    pos.x -= 2.5f;
                                    searchPos.x += 2.5f;
                                    lines.Add(new Line(Mathf.Min(pos.x, searchPos.x), Mathf.Max(pos.x, searchPos.x), pos.z, 'H', 'W'));
                                    pos.z -= 5;
                                    searchPos.z -= 5;
                                    pos.x += 5f;
                                    searchPos.x -= 5f;
                                }
                            }
                            lines.Add(new Line(Mathf.Min(pos.z, searchPos.z), Mathf.Max(pos.z, searchPos.z), pos.z, 'H', 'E'));
                        }
                        break;
                    }
                    
                    searchPos.x += directions[i, 0];
                    searchPos.z += directions[i, 1];
                    hitColliders = Physics.OverlapSphere(searchPos, 7, road);
                }
            }
        }
        // WriteToFile();
    }
}
