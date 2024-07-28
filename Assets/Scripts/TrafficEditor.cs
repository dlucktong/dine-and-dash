using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TrafficMapper))]
public class TrafficEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TrafficMapper tc = (TrafficMapper)target;
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
