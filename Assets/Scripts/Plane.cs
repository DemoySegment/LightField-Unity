using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plane : MonoBehaviour
{
    public float planeWidth = 1.0f;
    public float planeHeight = 0.9f;

    void OnDrawGizmos()
    {

        Gizmos.color = Color.yellow;

        Vector3 right = transform.right;
        Vector3 up = transform.up;
        Vector3 center = transform.position;

        Vector3 va = center - right * planeWidth / 2 - up * planeHeight / 2;
        Vector3 vb = center + right * planeWidth / 2 - up * planeHeight / 2;
        Vector3 vc = center + right * planeWidth / 2 + up * planeHeight / 2;
        Vector3 vd = center - right * planeWidth / 2 + up * planeHeight / 2;

        // draw plane
        Gizmos.DrawLine(va, vb);
        Gizmos.DrawLine(vb, vc);
        Gizmos.DrawLine(vc, vd);
        Gizmos.DrawLine(vd, va);

        // draw center point
        Gizmos.DrawSphere(center, 0.02f);
    }
}
