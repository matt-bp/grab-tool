using UnityEngine;

public class ClothInteractionPresenter : MonoBehaviour
{
    [Header("View")] [SerializeField] private SphereCollider leftHand;
    [SerializeField] private GameObject colliderVisualization;

    public void OnHoverEnter()
    {
        Debug.Log("ClothInteractionPresenter.OnHoverEnter()");
        
        colliderVisualization.SetActive(true);
    }

    public void OnHoverExit()
    {
        Debug.Log("ClothInteractionPresenter.OnHoverExit()");
        
        colliderVisualization.SetActive(false);
    }
}