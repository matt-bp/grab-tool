using UnityEditor;
using UnityEngine;

namespace GrabTool.Lines
{
    [RequireComponent(typeof(LineRenderer))]
    public class LineGenerator : MonoBehaviour
    {
        public int numPoints;

        private void Awake()
        {
            Generate();
        }

        public void Generate()
        {
            var lineRenderer = GetComponent<LineRenderer>();
            var count = numPoints + 1;
            lineRenderer.positionCount = count;

            var positions = new Vector3[count];

            var anglePerThing = 360 / (count - 1);
            var vectorToRotate = Vector3.left;

            for (int i = 0, currentAngle = 0; i < lineRenderer.positionCount; i++, currentAngle += anglePerThing)
            {
                positions[i] = Quaternion.AngleAxis(currentAngle, Vector3.forward) * vectorToRotate;
            }

            lineRenderer.SetPositions(positions);
        }
    }
}