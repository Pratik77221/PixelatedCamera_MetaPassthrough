using TMPro;
using UnityEngine;
using UnityEngine.UI;
using PassthroughCameraSamples;

public class CameraDisplay : MonoBehaviour
{
    [Header("Camera Settings")]
    public WebCamTextureManager WebcamManager;
    public RenderTexture TargetRenderTexture;
    
    [Header("Resolution Control")]
    public Button ButtonRes144;  // True 144p (256x144)
    public Button ButtonRes360;  // True 360p (640x360)
    public Button ButtonRes720;  // True 720p (1280x720)
    public Button ButtonResFull; // Full HD (1920x1080)
    
    [Header("UI Elements")]
    public TMP_Text ResolutionText;
    
    // Our resolution options with actual requested values
    private readonly Vector2Int[] m_resolutions = new[] {
        new Vector2Int(256, 144),   // True 144p
        new Vector2Int(640, 360),   // True 360p
        new Vector2Int(1280, 720),  // True 720p
        new Vector2Int(2064, 2280)  // Full HD
    };
    
    // Track current resolution index
    private int m_currentResIndex = 0;
    
    private void Start()
    {
        // Initialize buttons
        SetupButtons();
        
        // Set initial resolution to the lowest
        // This will update our existing RenderTexture's dimensions
        SetResolution(0);
    }

    private void Update()
    {
        if (WebcamManager != null && WebcamManager.WebCamTexture != null && TargetRenderTexture != null)
        {
            // Copy webcam texture to render texture - simple blit operation
            Graphics.Blit(WebcamManager.WebCamTexture, TargetRenderTexture);
            
            // Update resolution text to show RenderTexture resolution, not camera resolution
            if (ResolutionText != null)
            {
                ResolutionText.text = $"{TargetRenderTexture.width}x{TargetRenderTexture.height}";
            }
        }
    }
    
    private void SetupButtons()
    {
        if (ButtonRes144 != null)
        {
            ButtonRes144.onClick.RemoveAllListeners();
            ButtonRes144.onClick.AddListener(() => SetResolution(0));
        }
        
        if (ButtonRes360 != null)
        {
            ButtonRes360.onClick.RemoveAllListeners();
            ButtonRes360.onClick.AddListener(() => SetResolution(1));
        }
        
        if (ButtonRes720 != null)
        {
            ButtonRes720.onClick.RemoveAllListeners();
            ButtonRes720.onClick.AddListener(() => SetResolution(2));
        }
        
        if (ButtonResFull != null)
        {
            ButtonResFull.onClick.RemoveAllListeners();
            ButtonResFull.onClick.AddListener(() => SetResolution(3));
        }
    }
    
    // Set resolution by index
    public void SetResolution(int resIndex)
    {
        if (resIndex < 0 || resIndex >= m_resolutions.Length)
            return;
            
        m_currentResIndex = resIndex;
        
        // Update the camera resolution if WebcamManager is available
        if (WebcamManager != null)
        {
            WebcamManager.SetResolution(m_resolutions[resIndex]);
        }
        
        // Always update the render texture to our desired resolution
        UpdateRenderTextureResolution();
        
        Debug.Log($"Set resolution to {m_resolutions[resIndex].x}x{m_resolutions[resIndex].y}");
    }
    
    // Helper method to update the render texture with the selected resolution
    private void UpdateRenderTextureResolution()
    {
        if (TargetRenderTexture == null)
            return;
            
        // Get the desired resolution based on button selection
        var targetRes = m_resolutions[m_currentResIndex];
        
        // Update the existing RenderTexture's resolution
        Debug.Log($"Updating render texture to {targetRes.x}x{targetRes.y}");
        
        // Instead of creating a new RenderTexture, change the size of the existing one
        TargetRenderTexture.Release();
        TargetRenderTexture.width = targetRes.x;
        TargetRenderTexture.height = targetRes.y;
        TargetRenderTexture.Create();
    }
}
