using UnityEngine;
using UnityEditor;
using System.IO;
using Unity.VisualScripting;

public class ViewCapture : MonoBehaviour
{
    [MenuItem("Tools/Save Camera Output RenderTexture to PNG")]
    static void SaveCameraRenderTexture()
    {
        RenderTexture renderTexture = Selection.activeGameObject.GetComponent<Camera>().targetTexture;
        SaveRenderTexture(renderTexture);


    }



    public static void SaveRenderTexture(RenderTexture renderTexture)
    {
        if (renderTexture == null)
        {
            Debug.LogError("No RenderTexture found. Please select a Camera with a RenderTexture.");
            return;
        }
        var currentRT = RenderTexture.active;
        Texture2D texture2D = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
        RenderTexture.active = renderTexture;
        texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture2D.Apply();
        RenderTexture.active = currentRT; // Reset the active RenderTexture

        byte[] byteArray = texture2D.EncodeToPNG();
        string path = EditorUtility.SaveFilePanel("Save RenderTexture as PNG", "", "RenderTexture.png", "png");
        if (!string.IsNullOrEmpty(path))
        {
            File.WriteAllBytes(path, byteArray);
            Debug.Log("Saved RenderTexture to " + path);
        }
    }

    
}


