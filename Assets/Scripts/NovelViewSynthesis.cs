using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Tutorials.Core.Editor;
using UnityEngine;


public enum sampleMethod{
    Nearest,
    QuadrilinearInterpolation
}
[ExecuteInEditMode]

// idea: the compute shader render on the rendertexture, then the material render things with this rendertexture
public class NovelViewSynthesis : MonoBehaviour
{
    public ComputeShader ComputeShader;

    public Material Material;

    public Transform NovelViewPos;

    public LightField lf;

    public int ViewResolutionX = 2048;
    public int ViewResolutionY = 2048;
    public sampleMethod SampleMethod = sampleMethod.Nearest;
    public bool SaveRenderTexture = false;

    private int _kernelIndex;
    private RenderTexture _novelViewResult;
    private Vector3 _pixelStepX;
    private Vector3 _pixelStepY;
    private Vector3 _pixel00World;
    private ComputeBuffer _cameraPosBuffer;
    private ComputeBuffer _cameraProjBuffer;
    [SerializeField]
    private bool _doRender = false;




    void OnEnable()
    {
        

        _kernelIndex = ComputeShader.FindKernel("CSMain");
        _novelViewResult = new RenderTexture(ViewResolutionX, ViewResolutionY, 16);
        _novelViewResult.enableRandomWrite = true;
        _novelViewResult.Create();
        Material.mainTexture = _novelViewResult;

        

        lf.OnCameraReady += StartNovelViewSynthesis;
        lf.OnCameraReSet += StopNovelViewSynthesis;

    }

    void OnDisable()
    {
        _doRender = false;
        lf.OnCameraReady -= StartNovelViewSynthesis;
    }

    void StartNovelViewSynthesis()
    {
        Vector3[] cameraPositions = new Vector3[lf.CameraArray.Count];
        Matrix4x4[] cameraViewProjs = new Matrix4x4[lf.CameraArray.Count];


        for (int index = 0; index < lf.CameraArray.Count; index++)
        {
            
            cameraPositions[index] = lf.CameraArray[index].transform.position;
            cameraViewProjs[index] = lf.CameraArray[index].GetComponent<Camera>().projectionMatrix * lf.CameraArray[index].GetComponent<Camera>().worldToCameraMatrix;
            // Debug.Log($"camera positions: {lf.CameraArray[index].transform.position}");
        }

        
    

        _cameraPosBuffer = new ComputeBuffer(cameraPositions.Length, sizeof(float) * 3);
        _cameraPosBuffer.SetData(cameraPositions);
        _cameraProjBuffer = new ComputeBuffer(cameraViewProjs.Length, sizeof(float) * 16);
        _cameraProjBuffer.SetData(cameraViewProjs);
        _doRender = true;
    }

    void StopNovelViewSynthesis()
    {
        _doRender = false;
    }
    void Start()
    {

        StartNovelViewSynthesis();
    }


    void Update()
    {
        if (!_doRender)
        {
            return;
        }



        var mRenderTexture = lf.GetRenderTextureGroup();
        ComputeShader.SetInt("width", _novelViewResult.width);
        ComputeShader.SetInt("height", _novelViewResult.height);
        ComputeShader.SetInt("cameraViewWidth", lf.ViewHeight);
        ComputeShader.SetInt("cameraViewHeight", lf.ViewWidth);

        ComputeShader.SetInt("sampleMethod", (int)SampleMethod);

        // for view synthesis
        ComputeShader.SetInt("cameraCount", lf.CameraNum);
        ComputeShader.SetBuffer(_kernelIndex, "cameraPositions", _cameraPosBuffer);
        ComputeShader.SetBuffer(_kernelIndex, "cameraViewProjs", _cameraProjBuffer);
        ComputeShader.SetTexture(_kernelIndex, "cameraViews", mRenderTexture);



        var mesh = GetComponent<MeshFilter>().sharedMesh;
        var localSize = mesh.bounds.size;

        var lossyScale = transform.lossyScale;

        float planeWidth = localSize.x * lossyScale.x;
        float planeHeight = localSize.z * lossyScale.z;

        Vector3 planeRight = transform.right * planeWidth * 0.5f;
        Vector3 planeUp = transform.forward * planeHeight * 0.5f;
        Vector3 planeCenter = transform.position;
        Vector3 topLeft = planeCenter - planeRight + planeUp;
        Vector3 topRight = planeCenter + planeRight + planeUp;
        Vector3 bottomLeft = planeCenter - planeRight - planeUp;



        _pixelStepX = (topRight - topLeft) / ViewResolutionX;
        _pixelStepY = (topLeft - bottomLeft) / ViewResolutionY;
        //Debug.Log($"pixel width: {_pixelStepX}, pixel height: {_pixelStepY},");

        _pixel00World = bottomLeft + 0.5f * _pixelStepX + 0.5f * _pixelStepY;
        // for ray
        ComputeShader.SetVector("novelViewPos", NovelViewPos.position);
        ComputeShader.SetVector("pixel00", _pixel00World);
        ComputeShader.SetVector("pixelStepX", _pixelStepX);
        ComputeShader.SetVector("pixelStepY", _pixelStepY);


        ComputeShader.SetTexture(_kernelIndex, "Result", _novelViewResult);
        
        ComputeShader.Dispatch(_kernelIndex, ViewResolutionX / 8, ViewResolutionY / 8, 1);

        if (SaveRenderTexture)
        {
            ViewCapture.SaveRenderTexture(_novelViewResult);
            SaveRenderTexture = false;
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            ViewCapture.SaveRenderTexture(_novelViewResult);
        }
    }

    void OnDrawGizmos()
    {
        var mesh = GetComponent<MeshFilter>().sharedMesh;
        var localSize = mesh.bounds.size;

        var lossyScale = transform.lossyScale;

        float planeWidth = localSize.x * lossyScale.x;
        float planeHeight = localSize.z * lossyScale.z;

        Vector3 planeRight = transform.right * planeWidth * 0.5f;
        Vector3 planeUp = transform.forward * planeHeight * 0.5f;
        Vector3 planeCenter = transform.position;
        Vector3 topLeft = planeCenter - planeRight + planeUp;
        Vector3 topRight = planeCenter + planeRight + planeUp;
        Vector3 bottomLeft = planeCenter - planeRight - planeUp;
        Vector3 bottomRight = planeCenter + planeRight - planeUp;


        // draw projection plane
        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);

        // draw view port line
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(NovelViewPos.position, topLeft);
        Gizmos.DrawLine(NovelViewPos.position, topRight);
        Gizmos.DrawLine(NovelViewPos.position, bottomRight);
        Gizmos.DrawLine(NovelViewPos.position, bottomLeft);
    }
}
