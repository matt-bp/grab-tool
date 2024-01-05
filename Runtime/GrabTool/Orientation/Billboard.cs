using UnityEngine;

namespace GrabTool.Orientation
{
    public class Billboard : MonoBehaviour
    {
        private Camera _camera;
    
        private void Start()
        {
            _camera = Camera.main;
        }
    
        private void Update()
        {
            transform.rotation = _camera.transform.rotation;
        }
    }
}