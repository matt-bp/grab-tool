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

            GUILayout.Label ("Generate a circle made up of line segments.");
            generator.numPoints = EditorGUILayout.IntField("Number of line segments", generator.numPoints);
            if (GUILayout.Button("Generate"))
            {
                Debug.Log("Generate!");
                generator.Generate();
            }
        }
    }
}