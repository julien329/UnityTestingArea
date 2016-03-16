using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(GenerateMap))]
public class GenerateMapEditor : Editor {

    public override void OnInspectorGUI() {

        base.OnInspectorGUI();

        GenerateMap map = target as GenerateMap;
        map.CreateMap();
    }
}
