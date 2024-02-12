using System.Linq;
using UnityEditor;
#if UNITY_EDITOR
using UnityEngine;
#endif

namespace GrabTool.Visualization
{
    [AddComponentMenu("Grab Tool/Visualization/3D Grid")]
    public class Grid3D : MonoBehaviour
    {
        public int xSize, ySize, zSize;
        public Vector3[] Points { get; private set; }

        public Vector3[] Velocities { get; private set; }
        public Color[] Colors { get; private set; }

        public int densityPerMeter = 10;

        public bool inXYPlane;

        private void Awake()
        {
            Generate();
        }

        private void Generate()
        {
            var centerX = xSize / 2.0f;
            var centerY = ySize / 2.0f;
            var centerZ = zSize / 2.0f;

            var countX = xSize * densityPerMeter;
            var countY = ySize * densityPerMeter;
            var countZ = zSize * densityPerMeter;

            var increment = 1.0f / densityPerMeter;

            if (inXYPlane)
            {
                var arraySize = (countX + 1) * (countY + 1) * 2; // * (countZ + 1);
                Points = new Vector3[arraySize];
                Velocities = new Vector3[arraySize];
                Colors = new Color[arraySize];

                var i = 0;

                for (float y = 0; y <= countY; y++)
                {
                    for (var x = 0; x <= countX; x++, i++)
                    {
                        var actualX = x * increment;
                        var actualY = y * increment;

                        Points[i] = new Vector3(centerX - actualX, centerY - actualY);
                    }
                }
            }
            else // In XZ Plane
            {
                var arraySize = (countX + 1) * (countZ + 1) * 2; // * (countZ + 1);
                Points = new Vector3[arraySize];
                Velocities = new Vector3[arraySize];
                Colors = new Color[arraySize];

                var i = 0;

                for (float z = 0; z <= countZ; z++)
                {
                    for (var x = 0; x <= countX; x++, i++)
                    {
                        var actualX = x * increment;
                        var actualZ = z * increment;

                        Points[i] = new Vector3(centerX - actualX, 0, centerZ - actualZ);
                    }
                }
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (Points == null) return;

            foreach (var p in Points.Select((v, i) => new { v, i }))
            {
                var velocity = Velocities[p.i];
                if (velocity == Vector3.zero)
                {
                    // Handles.color = Color.gray;
                    // Handles.ArrowHandleCap(0, p.v, Quaternion.LookRotation(Vector3.left), 0.1f, EventType.Repaint);    
                    continue;
                }

                if (float.IsNaN(velocity.x) || float.IsNaN(velocity.y) || float.IsNaN(velocity.z))
                {
                    Debug.Log($"nans! at {p.i}, {p.v}");
                    continue;
                }

                var rotation = Quaternion.LookRotation(velocity);
                Handles.color = Colors[p.i];
                Handles.ArrowHandleCap(0, transform.TransformPoint(p.v), rotation, 0.2f, EventType.Repaint);
            }

            // Gizmos.color = Color.black;
            // foreach (var p in Points.Select((v, i) => new {v, i}))
            // {
            //     Gizmos.DrawSphere(p.v, 0.1f);
            // }
        }
#endif
    }
}