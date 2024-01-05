using GrabTool.Lines;
using UnityEditor;
using UnityEngine;

namespace Editor.GrabTool.Lines
{
    [CustomEditor(typeof(LineGenerator)), CanEditMultipleObjects]
    public class LineGeneratorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (target is not LineGenerator generator) return;
            
            GUILayout.Label ("This is a Label in a Custom Editor");
            if (GUILayout.Button("Generate"))
            {
                Debug.Log("Generate!");
                generator.Generate();
            }

            generator.numPoints = EditorGUILayout.IntField("Number of Points", generator.numPoints);
        }
    }
}