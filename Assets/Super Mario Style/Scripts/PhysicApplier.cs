using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicApplier : MonoBehaviour
{
    [SerializeField] private float directionalForce = 100;
    [SerializeField] private Vector3 extraForce = new Vector3(0, 100, 0);

    [SerializeField] private float torque = 10.0f;

    [SerializeField] private List<Rigidbody> elements = new List<Rigidbody>();
    [SerializeField] private float lifeTime = 2;

    void Start()
    {
        Apply();
    }

    public void Apply()
    {
        foreach (var e in elements)
        {
            e.AddForce((e.transform.position - transform.position) * directionalForce + extraForce, ForceMode.Impulse);
            Vector3 randomRotation = Random.insideUnitSphere;
            e.AddTorque(randomRotation * torque, ForceMode.Impulse);

            if (GameManager.instance)
            {
                if (e.GetComponent<Collider>())
                {
                    GameManager.instance.IgnorePlayerCollision(e.GetComponent<Collider>());
                }
            }
        }
        
        StartCoroutine(DestroyElements());

        Destroy(gameObject, lifeTime * 2);
    }

    IEnumerator DestroyElements()
    {
        yield return new WaitForSeconds(lifeTime);

        float scale = 1;
        List<Vector3> scales = new List<Vector3>();
        foreach (var e in elements)
        {
            scales.Add(e.transform.localScale);
        }

        while (scale > 0.01f)
        {
            scale = Mathf.MoveTowards(scale, 0, Time.deltaTime * 2);
            for (int i = 0; i < elements.Count; i++)
            {
                elements[i].transform.localScale = scales[i] * scale; 
            }

            yield return null;
        }

        scale = 0;
        for (int i = 0; i < elements.Count; i++)
        {
            Destroy(elements[i]);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.1f);
        Gizmos.DrawWireSphere(transform.position, 1.2f);
    }

}
