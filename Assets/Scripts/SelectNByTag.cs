using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

class SelectNByTag : ScriptableWizard
{
    public string tagName = "ExampleTag";
    public int selections = 0;
    public GameObject yessir;

    [MenuItem("SpawnPoints/Select n By Tag...")]
    static void SelectAllOfTagWizard()
    {
        DisplayWizard(
            "Select n By Tag...",
            typeof(SelectNByTag),
            "Make Selection");
    }

    void OnWizardCreate()
    {
        GameObject[] gos = GameObject.FindGameObjectsWithTag(tagName);

        int len = gos.Length;
        GameObject[] randSample = new GameObject[selections];
        
        List<int> listNumbers = new List<int>();
        int number;
        for (int i = 0; i < selections; i++)
        {
            do {
                number = Random.Range(0, len);
            } while (listNumbers.Contains(number));
            listNumbers.Add(number);
            randSample[i] = gos[number];
        }
        
        Selection.objects = randSample;
    }
}
