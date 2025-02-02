using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingBlock : MonoBehaviour
{
    [Header("Movement Settings")]
    public int version;

    public Vector3 pos1;
    public Vector3 pos2;
    public float speed;

    // 버전 1 전용 변수 
    bool movingToPos2 = true;

    void Update()
    {
        switch (version)
        {
            case 1:
                MoveBetweenTwoPoints();
                break;
            case 2:
                break;
            default:
                break;
        }
    }

    // 버전 1 : pos1 ↔ pos2 왕복 이동
    void MoveBetweenTwoPoints()
    {
        Vector3 target = movingToPos2 ? pos2 : pos1;
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.1f)
        {
            movingToPos2 = !movingToPos2;
        }
    }

    // 플레이어가 블록 따라가는 기능
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.parent = transform; 
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.parent = null;
        }
    }
}
