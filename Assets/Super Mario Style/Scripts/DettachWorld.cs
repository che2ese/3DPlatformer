using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DettachWorld : MonoBehaviour
{
    //For better performance on large worlds:
    void Awake()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            // Get the current child
            GameObject child = transform.GetChild(i).gameObject;

            // Set the child's parent to null
            child.transform.parent = null;
        }
    }


}
