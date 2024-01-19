using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionLogger : MonoBehaviour
{
    public void LogEnter()
    {
        Debug.Log("Entered");
    }

    public void LogExit()
    {
        Debug.Log("Exit");
    }
    
}
