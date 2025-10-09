using UnityEngine;
using PassthroughCameraSamples;

public class CameraDisplay : MonoBehaviour
{

    public WebCamTextureManager webcamManager;
    public Renderer displayRenderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (webcamManager.WebCamTexture != null)
        {
            displayRenderer.material.mainTexture = webcamManager.WebCamTexture;
        }

    }
}
