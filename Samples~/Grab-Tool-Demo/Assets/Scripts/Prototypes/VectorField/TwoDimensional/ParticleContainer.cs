using System;
using System.Linq;
using UnityEngine;

namespace Prototypes.VectorField.TwoDimensional
{
    public class ParticleContainer : MonoBehaviour
    {
        public Vector3[] Points { get; private set; }
        [SerializeField] private Transform[] particles;

        public Transform[] Particles => particles;
        
        private void Start()
        {
            Points = particles.Select(x => x.position).ToArray();
        }
    }
}