using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionLogger : MonoBehaviour
{
    public void VRControllerClothEnter()
    {
        Debug.Log("VRControllerClothEnter");
    }

    public void VRControllerClothExit()
    {   
        Debug.Log("VRControllerClothExit");
    }
}
