using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class Projectile : MonoBehaviour
{
    [SerializeField]
    private int damage = 1;
    [SerializeField]
    private float impactForce = 25;
    [SerializeField] 
    private float fwdForce;
    [SerializeField] 
    private float upForce;
    [SerializeField]
    private GameObject hitEffect;
    [SerializeField]
    private GameObject startEffect;
    [SerializeField]
    private GameObject dettachOnCollide;
    [SerializeField]
    private bool startScale = true;

    void Start()
    {
        GetComponent<Rigidbody>().AddForce(transform.forward * fwdForce + Vector3.up * upForce, ForceMode.Impulse);
    
        if (startEffect)
            Instantiate(startEffect, transform.position, transform.rotation);

        Destroy(gameObject, 10);

        if (startScale)
            StartCoroutine(ScaleUP());
    }

    IEnumerator ScaleUP()
    {
        Vector3 endScale = transform.localScale;
        transform.localScale = Vector3.zero;
        while(Vector3.Distance(transform.localScale, endScale) > 0.1f)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, endScale, Time.deltaTime * 6);
            yield return null;
        }

        transform.localScale = endScale;

        yield break;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == this.gameObject)
            return;

        if (other.CompareTag("Player"))
        {
            Vector3 dir = other.transform.position - transform.position;
            dir.y = 0;
            dir = dir.normalized;
            other.gameObject.GetComponent<PlayerHealth>().TakeDamage(damage, dir * impactForce);
        }

        if (!other.GetComponent<Collider>().isTrigger)
            DestroyProjectile();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == this.gameObject)
            return;

        if (collision.gameObject.CompareTag("Player"))
        {
            Vector3 dir = collision.transform.position - transform.position;
            dir.y = 0;
            dir = dir.normalized;

            collision.gameObject.GetComponent<PlayerHealth>().TakeDamage(damage, dir * impactForce);
        }

        if (!collision.gameObject.GetComponent<Collider>().isTrigger)
            DestroyProjectile();
    }

    private void DestroyProjectile()
    {
        if (hitEffect)
            Instantiate(hitEffect, transform.position, transform.rotation);

        dettachOnCollide.transform.parent = null;
        Destroy(dettachOnCollide, 3);
        Destroy(gameObject);
    }

}
