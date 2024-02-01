using System;
using System.Linq;
using GrabTool.Math;
using UnityEngine;
using UnityEngine.Assertions;

namespace Prototypes.VectorField.TwoDimensional
{
    [RequireComponent(typeof(ParticleContainer), typeof(Grid))]
    public class VectorFieldIntegrator : MonoBehaviour
    {
        [Tooltip("The outer loop cutoff")] [SerializeField]
        private float rO;

        [Tooltip("The inner loop cutoff")] [SerializeField]
        private float rI;
        
        private Grid _grid;

        private ParticleContainer _container;

        private readonly VectorField2D _vectorField2D = new();

        private bool doUpdate;

        private float d;
        
        private void Start()
        {
            _container = GetComponent<ParticleContainer>();
            _grid = GetComponent<Grid>();
        }

        private void Update()
        {
            _vectorField2D.Ri = rI;
            _vectorField2D.Ro = rO;
            
            Assert.IsTrue(rI < rO);
            
            foreach (var v in _grid.Points.Select((v, i) => new { v, i }))
            {
                var value = _vectorField2D.GetVelocity(v.v.x, v.v.y);
                _grid.Velocities[v.i] = value;
                _grid.Colors[v.i] = Color.blue;
            }
        }

        public void HandleMouseMove((Vector3 DesiredTranslation, Vector3 Center) input)
        {
            _vectorField2D.C = input.Center;
            _vectorField2D.DesiredTranslation = input.DesiredTranslation;
            d = Vector3.Magnitude(input.DesiredTranslation);
            doUpdate = true;
        }
        
        private void FixedUpdate()
        {
            if (!doUpdate) return;
            
            foreach (var particle in _container.Particles)
            {
                var position = particle.position;
                var v = _vectorField2D.GetVelocity(position.x, position.y);

                if (v.magnitude == 0) continue;
                
                var t = d / v.magnitude;
                
                // Debug.Log($"Time update is {t}");

                position += t * new Vector3(v.x, v.y, 0);
                
                particle.position = position;
            }

            doUpdate = false;
        }
    }
}