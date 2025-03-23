using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
public class OffAxisProjection : MonoBehaviour
{
    // fixed projection plane in the world
    public Plane projectionPlane; 



    private Camera cam;

    void OnEnable()
    {
        cam = GetComponent<Camera>();
    }
    void LateUpdate()
    {
        if (projectionPlane == null || cam == null)
            return;

        Vector3 eye = cam.transform.position;

        Vector3 right = projectionPlane.transform.right;
        Vector3 up = projectionPlane.transform.up;
        Vector3 normal = projectionPlane.transform.forward;
        Vector3 center = projectionPlane.transform.position;

        Vector3 va = center - right * projectionPlane.planeWidth / 2 - up * projectionPlane.planeHeight / 2;
        Vector3 vb = center + right * projectionPlane.planeWidth / 2 - up * projectionPlane.planeHeight / 2;
        Vector3 vc = center - right * projectionPlane.planeWidth / 2 + up * projectionPlane.planeHeight / 2;

        Vector3 pa = va - eye;
        Vector3 pb = vb - eye;
        Vector3 pc = vc - eye;

        float d = Vector3.Dot(pa, normal);
        float near = cam.nearClipPlane;
        float far = cam.farClipPlane;

        float l = Vector3.Dot(right, pa) * near / d;
        float r = Vector3.Dot(right, pb) * near / d;
        float b = Vector3.Dot(up, pa) * near / d;
        float t = Vector3.Dot(up, pc) * near / d;

        cam.projectionMatrix = PerspectiveOffCenter(l, r, b, t, near, far);
    }

    // glFrustum
    Matrix4x4 PerspectiveOffCenter(float l, float r, float b, float t, float near, float far)
    {
        Matrix4x4 m = new Matrix4x4();

        m[0, 0] = 2f * near / (r - l);
        m[0, 1] = 0;
        m[0, 2] = (r + l) / (r - l);
        m[0, 3] = 0;

        m[1, 0] = 0;
        m[1, 1] = 2f * near / (t - b);
        m[1, 2] = (t + b) / (t - b);
        m[1, 3] = 0;

        m[2, 0] = 0;
        m[2, 1] = 0;
        m[2, 2] = -(far + near) / (far - near);
        m[2, 3] = -(2f * far * near) / (far - near);

        m[3, 0] = 0;
        m[3, 1] = 0;
        m[3, 2] = -1;
        m[3, 3] = 0;

        return m;
    }

    void OnDisable()
    {
        if (cam != null)
            cam.ResetProjectionMatrix();
    }

    void OnDrawGizmos()
    {
        if (projectionPlane == null || cam == null)
            return;

        Gizmos.color = Color.green;
        Vector3 eye = cam.transform.position;

        Vector3 right = projectionPlane.transform.right;
        Vector3 up = projectionPlane.transform.up;
        Vector3 normal = projectionPlane.transform.forward;
        Vector3 center = projectionPlane.transform.position;

        Vector3 va = center - right * projectionPlane.planeWidth / 2 - up * projectionPlane.planeHeight / 2;
        Vector3 vb = center + right * projectionPlane.planeWidth / 2 - up * projectionPlane.planeHeight / 2;
        Vector3 vc = center + right * projectionPlane.planeWidth / 2 + up * projectionPlane.planeHeight / 2;
        Vector3 vd = center - right * projectionPlane.planeWidth / 2 + up * projectionPlane.planeHeight / 2;

        // draw projection plane
        Gizmos.DrawLine(va, vb);
        Gizmos.DrawLine(vb, vc);
        Gizmos.DrawLine(vc, vd);
        Gizmos.DrawLine(vd, va);

        // draw view port line
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(eye, va);
        Gizmos.DrawLine(eye, vb);
        Gizmos.DrawLine(eye, vc);
        Gizmos.DrawLine(eye, vd);
    }
}

