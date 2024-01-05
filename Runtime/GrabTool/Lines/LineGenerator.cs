using UnityEditor;
using UnityEngine;

namespace GrabTool.Lines
{
    [RequireComponent(typeof(LineRenderer))]
    // [ExecuteInEditMode]
    public class LineGenerator : MonoBehaviour
    {
        public int numPoints;
        
        private void Awake()
        {
            Generate();
        }

        public void Generate()
        {
            Debug.Log($"GOInG");
            var renderer = GetComponent<LineRenderer>();
            renderer.positionCount = numPoints + 1;

            var positions = new Vector3[renderer.positionCount];

            var anglePerThing = 360 / (renderer.positionCount - 1);
            var vectorToRotate = Vector3.left;

            for (int i = 0, currentAngle = 0; i < renderer.positionCount; i++, currentAngle += anglePerThing)
            {
                positions[i] = Quaternion.AngleAxis(currentAngle, Vector3.forward) * vectorToRotate;
            }

            // positions[positions.Length - 1] = vectorToRotate;

            renderer.SetPositions(positions);
        }
    }
}
