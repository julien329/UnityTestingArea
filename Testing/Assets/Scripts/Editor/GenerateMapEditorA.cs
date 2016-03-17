using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(GenerateMapA))]
public class GenerateMapEditorA : Editor {

    public override void OnInspectorGUI() {

        base.OnInspectorGUI();

        if (GUI.changed) {
            GenerateMapA map = target as GenerateMapA;
            map.CreateMap();
        }
    }
}
