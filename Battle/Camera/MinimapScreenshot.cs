using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MinimapScreenshot : MonoBehaviour
{
    [SerializeField] private RawImage minimapImage;
    [SerializeField] private int width = 256;
    [SerializeField] private int height = 256;

    private Camera minimapCamera;

    private void Awake()
    {
        minimapCamera = GetComponent<Camera>();
    }

    public void TakeScreenshot()
    {
        // Use a coroutine to ensure the frame is ready before capturing
        StartCoroutine(CaptureCoroutine());
    }

    private IEnumerator CaptureCoroutine()
    {
        // Wait for the end of the frame to ensure the scene is fully rendered
        yield return new WaitForEndOfFrame();

        Capture();
    }

    private void Capture()
    {
        // Create a RenderTexture to render the camera view into
        RenderTexture rt = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
        rt.Create();

        // Set the camera to render to the RenderTexture and force a render
        minimapCamera.targetTexture = rt;
        minimapCamera.Render();

        // Create a Texture2D to read the pixels into
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);

        // Set the active RenderTexture and read the pixels
        RenderTexture.active = rt;
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();

        // Assign the texture to the RawImage
        minimapImage.texture = tex;

        // Clean up
        minimapCamera.targetTexture = null;
        RenderTexture.active = null;
        rt.Release();

        // Turn off the camera after capture
        minimapCamera.enabled = false;
    }
}