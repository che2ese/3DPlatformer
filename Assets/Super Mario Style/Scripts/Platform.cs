using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Platform : MonoBehaviour
{
    public enum type
    {
        Lineal,
        Lerp,
        Damp,
    }

    public enum Cycle
    {
        PingPong,
        Loop,
        StartOnPlayer
    }

    [System.Serializable]
    public struct pointOfPath
    {
        public Transform point;
        public float waitOnPoint;
    }

    [Header("All platforms must have the scale in 1, 1, 1")]

    [SerializeField] private float speed = 10;
    [SerializeField] private type movement;
    [Tooltip("StartOnPlayer: only start to move when the player is on the platform.")]
    [SerializeField] private Cycle mode;
    [Space(10)]
    [SerializeField] private List<pointOfPath> path = new List<pointOfPath>();
    [Space(10)]
    [SerializeField] private Vector3 gizmoSize;
    [SerializeField] private int gizmoPoint = 0;

    private Coroutine coWork;
    private int dir = 1;
    private int currentPath = 0;
    void Start()
    {
        transform.localScale = new Vector3(1, 1, 1);

        foreach (var p in path)
        {
            p.point.parent = null;
        }

        transform.position = path[0].point.position;

        if (mode != Cycle.StartOnPlayer)
        {
            coWork = StartCoroutine(Work());
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Vector3 contactNormal = collision.contacts[0].normal;
            float angle = Vector3.Angle(contactNormal, Vector3.down);

            if (angle < 45f)
            {
                collision.transform.parent = this.transform;
                
                if (mode == Cycle.StartOnPlayer)
                {
                    if (coWork == null)
                        coWork = StartCoroutine(Work());
                }
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.parent = null;
        }
    }

    IEnumerator Work()
    {
       
        bool end = false;
      

        while(!end)
        {
            if (movement == type.Lineal)
            {
                transform.position = Vector3.MoveTowards(transform.position, path[currentPath].point.position, Time.deltaTime * speed);
            }
            else if (movement == type.Lerp)
            {
                transform.position = Vector3.Lerp(transform.position, path[currentPath].point.position, Time.deltaTime * speed);
            }
            else
            {
                Vector3 vel = Vector3.zero;
                transform.position = Vector3.SmoothDamp(transform.position, path[currentPath].point.position, ref vel, Time.deltaTime * speed);
            }

            if (Vector3.Distance(transform.position, path[currentPath].point.position) <= 0.1f)
            {
                yield return new WaitForSeconds(path[currentPath].waitOnPoint);

                if (mode == Cycle.PingPong)
                {
                    currentPath += dir;

                    if (currentPath >= path.Count)
                    {
                        dir = -1;
                        currentPath = path.Count - 2;
                    }
                    else if (currentPath < 0)
                    {
                        dir = 1;
                        currentPath = 1;
                    }

                }
                else if (mode == Cycle.Loop)
                {
                    currentPath += 1;

                    if (currentPath >= path.Count)
                    {
                        currentPath = 0;
                    }
                }
                else if (mode == Cycle.StartOnPlayer)
                {
                    currentPath += dir;

                    if (dir == 1)
                    {
                        if (currentPath >= path.Count)
                        {
                            dir = -1;
                            currentPath = path.Count - 2;
                            end = true;
                        }
                    }
                    else
                    {
                        if (currentPath < 0)
                        {
                            dir = 1;
                            currentPath = 1;
                            end = true;
                        }
                    }
                }
            }

            yield return null;
        }

        coWork = null;
        yield break;

    }

    private void OnDrawGizmos()
    {
        if (gizmoPoint >= path.Count)
            gizmoPoint = path.Count - 1;

        if (gizmoPoint >= path.Count)
            return;

        Gizmos.color = new Color(1, 1, 1, 0.4f);

        Matrix4x4 cubeTransform = Matrix4x4.TRS(path[gizmoPoint].point.position, transform.rotation, transform.localScale + gizmoSize);
        Matrix4x4 oldGizmosMatrix = Gizmos.matrix;

        Gizmos.matrix *= cubeTransform;

        Gizmos.DrawCube(Vector3.zero, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);

        Gizmos.matrix = oldGizmosMatrix;
    }

    private void OnDrawGizmosSelected()
    {
        if (gizmoPoint >= path.Count)
            gizmoPoint = path.Count - 1;

        if (gizmoPoint >= path.Count)
            return;

        Gizmos.color = new Color(1, 1, 1, 0.6f);

        Matrix4x4 cubeTransform = Matrix4x4.TRS(path[gizmoPoint].point.position, transform.rotation, transform.localScale + gizmoSize);
        Matrix4x4 oldGizmosMatrix = Gizmos.matrix;

        Gizmos.matrix *= cubeTransform;

        Gizmos.DrawCube(Vector3.zero, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);

        Gizmos.matrix = oldGizmosMatrix;


        for (int i = 0; i < path.Count; i++)
        {
            Color blue = Color.blue;
            blue.a = 0.5f;

            Gizmos.color = blue;
            
            if (i == 0 || i == path.Count-1)
            {
                Gizmos.DrawCube(path[i].point.position, new Vector3(0.5f, 0.5f, 0.5f));
            }
            else
            {
                Gizmos.DrawSphere(path[i].point.position, 0.5f);
            }

            if (mode == Cycle.Loop)
            {
                int next = i+1;

                if (i == path.Count - 1)
                    next = 0;

                Gizmos.DrawLine(path[i].point.position, path[next].point.position);
            }
            else
            {
                if (i != path.Count - 1)
                    Gizmos.DrawLine(path[i].point.position, path[i + 1].point.position);
            }
  
        }

    }


}
