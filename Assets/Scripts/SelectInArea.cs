using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

class SelectInArea : ScriptableWizard
{
    public int xMax, xMin, zMax, zMin = 0;

    [MenuItem("SpawnPoints/Select in Area...")]
    static void SelectAllOfTagWizard()
    {
        DisplayWizard(
            "Select in Area...",
            typeof(SelectInArea),
            "Make Selection");
    }

    void OnWizardCreate()
    {
        GameObject[] gos = UnityEngine.Object.FindObjectsOfType<GameObject>();

        List<GameObject> selections = new List<GameObject>();
        
        foreach (GameObject go in gos)
        {
            float objX = go.transform.position.x;
            float objZ = go.transform.position.z;


            if (objX > xMax || objX < xMin || objZ > zMax || objZ < zMin)
            {
                selections.Add(go);
            }
        }

        
        
        Selection.objects = selections.ToArray();
    }
}

// -56.0,-104
// 360, 520