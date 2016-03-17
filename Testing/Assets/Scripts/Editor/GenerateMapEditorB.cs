using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(GenerateMapB))]
public class GenerateMapEditorB : Editor {

    public override void OnInspectorGUI() {

        base.OnInspectorGUI();

        if (GUI.changed) {
            GenerateMapB map = target as GenerateMapB;
            map.CreateMap();
        }
    }
}
