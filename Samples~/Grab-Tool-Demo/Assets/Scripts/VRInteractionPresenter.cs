using Unity.XR.CoreUtils;
using UnityEngine;

public class VRInteractionPresenter : MonoBehaviour
{
    [SerializeField] private XROrigin xrOrigin;

    public void ResetPosition()
    {
        xrOrigin.transform.position = Vector3.zero;
    }

    public void OnPressed()
    {
        Debug.Log("Pressed!");
    }

    public void OnReleased()
    {
        Debug.Log("Released!");
    }
    
    
}