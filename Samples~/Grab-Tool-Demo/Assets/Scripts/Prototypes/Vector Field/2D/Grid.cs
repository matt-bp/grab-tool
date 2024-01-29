using System;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Prototypes.Vector_Field._2D
{
    public class Grid : MonoBehaviour
    {
        public int xSize, ySize;
        public Vector3[] Points { get; private set; }
        
        public Vector3[] Velocities { get; private set; }
        public Color[] Colors { get; private set; }

        public int densityPerMeter = 10;
        public bool useZAsLength;

        private void Awake()
        {
            Generate();
        }

        private void Generate()
        {
            var centerX = xSize / 2.0f + 0.5f;
            var centerY = ySize / 2.0f + 0.5f;
            
            
            var countX = xSize * densityPerMeter;
            var countY = ySize * densityPerMeter;
            
            var increment = 1.0f / densityPerMeter;

            Points = new Vector3[(countX + 1) * (countY + 1)];
            Velocities = new Vector3[(countX + 1) * (countY + 1)]; 
            Colors = new Color[(countX + 1) * (countY + 1)];

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

        private void OnDrawGizmos()
        {
            if (Points == null) return;

            Gizmos.color = Color.white;
            Gizmos.DrawSphere(Vector3.zero, 0.1f);

            Gizmos.color = Color.black;

            
            foreach (var p in Points.Select((v, i) => new {v, i}))
            {
                Handles.color = Colors[p.i];
                
                if (Velocities[p.i] == Vector3.zero)
                {
                    Handles.ArrowHandleCap(0, p.v, Quaternion.LookRotation(Vector3.left), 0.1f, EventType.Repaint);    
                    continue;
                };

                var arrowLenght = useZAsLength ? Mathf.Abs(Velocities[p.i].z) : 0.2f;
                
                Handles.ArrowHandleCap(0, p.v, Quaternion.LookRotation(Velocities[p.i]), arrowLenght, EventType.Repaint);
            }
            
            // foreach (var p in Points.Select((v, i) => new {v, i}))
            // {
            //     Gizmos.DrawSphere(p.v, 0.1f);
            // }
            

        }
    }
}