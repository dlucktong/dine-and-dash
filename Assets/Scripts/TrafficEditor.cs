using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TrafficController))]
public class TrafficEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TrafficController tc = (TrafficController)target;
        if (GUILayout.Button("Bake Roads"))
        {
            tc.GenerateLineSet();
        }
        if (GUILayout.Button("Generate Connected List"))
        {
            tc.GenerateConnectedList();
        }

    }
}
