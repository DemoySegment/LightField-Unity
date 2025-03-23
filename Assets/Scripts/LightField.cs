using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

[ExecuteInEditMode]
public class LightField : MonoBehaviour
{
    public Plane STPlane;
    public Plane UVPlane;
    public float PlaneDistance = 0.2f;
    public Transform CameraArrayTransform;

    public int CameraNum;
    public int ViewHeight = 256;
    public int ViewWidth = 256;

    public GameObject CameraPrefab;

    public List<GameObject> CameraArray = new List<GameObject>();


    public int ViewIndex;

    public bool SaveView;

    public bool DoRender;

    [SerializeField]
    private bool _hasRenderResults = false;
    private RenderTexture _cameraViews;

    public Action OnCameraReady;
    public Action OnCameraReSet;
    void OnEnable()
    {
        //Debug.Log(renderTexture.format);
        PlaneAdjustment();

        _cameraViews = new RenderTexture(ViewWidth, ViewHeight, 16, RenderTextureFormat.BGRA32, RenderTextureReadWrite.Linear);
        _cameraViews.dimension = UnityEngine.Rendering.TextureDimension.Tex2DArray;
        _cameraViews.volumeDepth = CameraNum * CameraNum;
        _cameraViews.enableRandomWrite = true;
        _cameraViews.Create();

        if (CameraArray.Count != 0)
        {
            return;
        }
        // create a camera array on the UVplane
        Vector3 center = UVPlane.transform.position;
        Vector3 right = UVPlane.transform.right;
        Vector3 up = UVPlane.transform.up;
        Vector3 front = UVPlane.transform.forward;

        var left_up_corner = center - UVPlane.planeWidth / 2 * right + UVPlane.planeHeight / 2 * up;

        var width_increase = UVPlane.planeWidth / (CameraNum + 1);
        var height_increase = UVPlane.planeHeight / (CameraNum + 1);



        for (int i = 0; i < CameraNum; i++)
        {
            for (int j = 0; j < CameraNum; j++)
            {
                // find camera position
                var location = left_up_corner + width_increase * (j + 1) * right - height_increase * (i + 1) * up;

                // instantiate the camera
                var item = Instantiate(CameraPrefab);
                item.transform.position = location;
                item.transform.rotation = UVPlane.transform.rotation;
                item.GetComponent<OffAxisProjection>().projectionPlane = STPlane;
                item.transform.SetParent(CameraArrayTransform);

                
                

                
                
                CameraArray.Add(item);
            }
        }

        //tempRT.Release();

        _hasRenderResults = false;

    }

 

    void Render()
    {
        RenderTexture tempRT = new RenderTexture(ViewWidth, ViewHeight, 16, RenderTextureFormat.BGRA32);
        tempRT.Create();

        int i = 0;
        foreach (var item in CameraArray)
        {
            var camera = item.GetComponent<Camera>();


            camera.targetTexture = tempRT;
            camera.Render();
            Graphics.CopyTexture(tempRT, 0, 0, _cameraViews, i, 0);
            camera.targetTexture = null;
            i++;
        }
        Debug.Log("Save Render Result to GPU");
    }

    void SaveCameraView(bool newRender = false)
    {
        RenderTexture tempRT = new RenderTexture(ViewWidth, ViewHeight, 16, RenderTextureFormat.BGRA32);
        tempRT.Create();
        var currentRT = RenderTexture.active;
        RenderTexture.active = tempRT;

        if (newRender)
        {
            var camera = CameraArray[ViewIndex].GetComponent<Camera>();
            camera.targetTexture = tempRT;
            camera.Render();
            camera.targetTexture = null;
        }




        else
        {
            Graphics.CopyTexture(_cameraViews, ViewIndex, 0, tempRT, 0, 0);
        }



        ViewCapture.SaveRenderTexture(tempRT);
        RenderTexture.active = currentRT;
        
        tempRT.Release();
    }

    void OnDisable()
    {
        // clear the cameraArray

        foreach (GameObject child in CameraArray)
        {

            if (Application.isPlaying)
                Destroy(child);
            else
                DestroyImmediate(child);


        }

        CameraArray.Clear();

        _cameraViews.Release();
        _cameraViews = null;

        _hasRenderResults = false;

        OnCameraReSet?.Invoke();
    }

    // adjust UV plane based on ST plane Setting
    void PlaneAdjustment()
    {
        UVPlane.planeHeight = STPlane.planeHeight;
        UVPlane.planeWidth = STPlane.planeWidth;

        Vector3 center = STPlane.transform.position;


        UVPlane.transform.position = center - STPlane.transform.forward * PlaneDistance;
        UVPlane.transform.rotation = STPlane.transform.rotation;

    }

    

    // Update is called once per frame
    void Update()
    {
        // save curtain view
        if (SaveView)
        {
            if (ViewIndex <= CameraArray.Count)
            {
                SaveCameraView();

            }

            SaveView = false;
        }

        if (DoRender)
        {
            Render();
            _hasRenderResults = true;
            OnCameraReady?.Invoke();
            DoRender = false;
        }
    }

    // todo: fill this
    void SaveOverallImage()
    {
        Texture2D singleViewTex = new Texture2D(ViewWidth, ViewHeight, TextureFormat.RGBA32, false);

        RenderTexture tempRT = new RenderTexture(ViewWidth, ViewHeight, 16, RenderTextureFormat.ARGB32);
        tempRT.Create();


        Graphics.CopyTexture(_cameraViews, 1, 0, tempRT, 0, 0);


        ViewCapture.SaveRenderTexture(tempRT);

        tempRT.Release();
        DestroyImmediate(tempRT);

        
    }

    public RenderTexture GetRenderTextureGroup()
    {
        return _cameraViews;
    }
}
