using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    public bool applyForce;
    public float force = 2;
    public Vector3 direction;

    private void Start()
    {
        if (applyForce)
        {
            var rb = this.gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
        }
    }

    void Update()
    {
        transform.Rotate(direction * Time.deltaTime);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (applyForce)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                collision.gameObject.GetComponent<Rigidbody>().AddForce(direction * force, ForceMode.Acceleration);
           
            }
        }

    }

}
