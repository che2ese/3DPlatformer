using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform target;

    [SerializeField] private Vector3 myRotation;
    [SerializeField] private float distance = 20;
    [SerializeField] private float height = 1;

    private Vector3 myOffset;

    [SerializeField] private float speedTransition = 2;
    [SerializeField] private bool doFollow = true;

    private Coroutine coRotate;
    private Coroutine coOffset;

    public static CameraController instance;

    private void Awake()
    {
        instance = this;
        transform.eulerAngles = myRotation;
    }

    private void LateUpdate()
    {
        if (doFollow)
        {
            target.position = player.position + myOffset;
            transform.position = target.position - (transform.forward * distance) + (Vector3.up * height);
        }
    }

    public void Rotate(Vector3 finalRot)
    {

        if (coRotate != null)
            StopCoroutine(coRotate);

        coRotate = StartCoroutine(DoRotation(finalRot));
    }

    public void Offset(Vector3 finalOffset)
    {

        if (coOffset != null)
            StopCoroutine(coOffset);

        coOffset = StartCoroutine(DoOffset(finalOffset));
    }

    IEnumerator DoRotation(Vector3 finalRot)
    {
        while (Vector3.Distance(finalRot, myRotation) > 0.1f)
        {
            myRotation = Vector3.Lerp(myRotation, finalRot, Time.deltaTime * speedTransition);
            transform.eulerAngles = myRotation;
            yield return null;
        }

        myRotation = finalRot;
        transform.eulerAngles = finalRot;

        coRotate = null;
        yield break;
    }
   
    IEnumerator DoOffset(Vector3 finalOffset)
    {
        while (Vector3.Distance(finalOffset, myOffset) > 0.1f)
        {
            myOffset = Vector3.Lerp(myOffset, finalOffset, Time.deltaTime * speedTransition);
            yield return null;
        }

        myOffset = finalOffset;

        coOffset = null;
        yield break;
    }

}
