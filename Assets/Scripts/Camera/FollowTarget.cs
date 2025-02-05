using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    public Transform target;
    void FixedUpdate()
    {
        if (target)
            transform.position = target.position;
    }
}
