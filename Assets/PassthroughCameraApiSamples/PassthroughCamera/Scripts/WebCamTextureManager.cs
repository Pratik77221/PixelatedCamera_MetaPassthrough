// Copyright (c) Meta Platforms, Inc. and affiliates.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Meta.XR.Samples;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using PassthroughCameraSamples.StartScene;
using PCD = PassthroughCameraSamples.PassthroughCameraDebugger;

namespace PassthroughCameraSamples
{
    [MetaCodeSample("PassthroughCameraApiSamples-PassthroughCamera")]
    public class WebCamTextureManager : MonoBehaviour
    {
        [Header("Camera Settings")]
        [SerializeField] public PassthroughCameraEye Eye = PassthroughCameraEye.Left;
        [SerializeField, Tooltip("The requested resolution of the camera may not be supported by the chosen camera. In such cases, the closest available values will be used.\n\n" +
                                 "When set to (0,0), the highest supported resolution will be used.")]
        public Vector2Int RequestedResolution;
        [SerializeField] public PassthroughCameraPermissions CameraPermissions;
        
        [Header("UI Settings")]
        [SerializeField, Tooltip("Show resolution control UI on startup")]
        public bool ShowResolutionUI = true;
        
        [Header("Manual Button Assignment (Optional)")]
        [SerializeField, Tooltip("Assign these buttons manually if you want to use custom UI buttons instead of DebugUI")]
        public Button button144p;
        [SerializeField] public Button button240p;
        [SerializeField] public Button button360p;
        [SerializeField] public Button button480p;
        [SerializeField] public Button button720p;
        [SerializeField] public Button button1080p;

        /// <summary>
        /// Returns <see cref="WebCamTexture"/> reference if required permissions were granted and this component is enabled. Else, returns null.
        /// </summary>
        public WebCamTexture WebCamTexture { get; private set; }

        private bool m_hasPermission;
        private DebugUIBuilder m_debugUI;
        private bool m_uiInitialized = false;

        // Predefined resolutions
        private readonly Dictionary<string, Vector2Int> m_resolutionOptions = new Dictionary<string, Vector2Int>
        {
            { "144p", new Vector2Int(256, 144) },
            { "240p", new Vector2Int(426, 240) },
            { "360p", new Vector2Int(640, 360) },
            { "480p", new Vector2Int(854, 480) },
            { "720p", new Vector2Int(1280, 720) },
            { "1080p", new Vector2Int(1920, 1080) }
        };

        private void Awake()
        {
            PCD.DebugMessage(LogType.Log, $"{nameof(WebCamTextureManager)}.{nameof(Awake)}() was called");
            Assert.AreEqual(1, FindObjectsByType<WebCamTextureManager>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length,
                $"PCA: Passthrough Camera: more than one {nameof(WebCamTextureManager)} component. Only one instance is allowed at a time. Current instance: {name}");
#if UNITY_ANDROID
            CameraPermissions.AskCameraPermissions();
#endif
        }

        private void Start()
        {
            // Setup manual button assignments if provided
            SetupManualButtons();
        }

        private void OnEnable()
        {
            PCD.DebugMessage(LogType.Log, $"PCA: {nameof(OnEnable)}() was called");
            if (!PassthroughCameraUtils.IsSupported)
            {
                PCD.DebugMessage(LogType.Log, "PCA: Passthrough Camera functionality is not supported by the current device." +
                          $" Disabling {nameof(WebCamTextureManager)} object");
                enabled = false;
                return;
            }

            m_hasPermission = PassthroughCameraPermissions.HasCameraPermission == true;
            if (!m_hasPermission)
            {
                PCD.DebugMessage(LogType.Error,
                    $"PCA: Passthrough Camera requires permission(s) {string.Join(" and ", PassthroughCameraPermissions.CameraPermissions)}. Waiting for them to be granted...");
                return;
            }

            PCD.DebugMessage(LogType.Log, "PCA: All permissions have been granted");
            _ = StartCoroutine(InitializeWebCamTexture());
            
            // Initialize resolution control UI only if no manual buttons are assigned
            if (ShowResolutionUI && !HasManualButtonsAssigned())
            {
                InitializeResolutionUI();
            }
        }

        private void OnDisable()
        {
            PCD.DebugMessage(LogType.Log, $"PCA: {nameof(OnDisable)}() was called");
            StopCoroutine(InitializeWebCamTexture());
            if (WebCamTexture != null)
            {
                WebCamTexture.Stop();
                Destroy(WebCamTexture);
                WebCamTexture = null;
            }
            
            // Hide UI when disabled
            if (m_debugUI != null)
            {
                m_debugUI.Hide();
            }
        }

        private void Update()
        {
            if (!m_hasPermission)
            {
                if (PassthroughCameraPermissions.HasCameraPermission != true)
                    return;

                m_hasPermission = true;
                _ = StartCoroutine(InitializeWebCamTexture());
            }
            
            // Toggle UI with 'R' key for testing (only for DebugUI, not manual buttons)
            if (Input.GetKeyDown(KeyCode.R) && !HasManualButtonsAssigned())
            {
                ToggleResolutionUI();
            }
        }

        private bool HasManualButtonsAssigned()
        {
            return button144p != null || button240p != null || button360p != null || 
                   button480p != null || button720p != null || button1080p != null;
        }

        private void SetupManualButtons()
        {
            // Setup manually assigned buttons
            if (button144p != null) 
            {
                button144p.onClick.RemoveAllListeners();
                button144p.onClick.AddListener(() => SetResolution(m_resolutionOptions["144p"]));
            }
            if (button240p != null) 
            {
                button240p.onClick.RemoveAllListeners();
                button240p.onClick.AddListener(() => SetResolution(m_resolutionOptions["240p"]));
            }
            if (button360p != null) 
            {
                button360p.onClick.RemoveAllListeners();
                button360p.onClick.AddListener(() => SetResolution(m_resolutionOptions["360p"]));
            }
            if (button480p != null) 
            {
                button480p.onClick.RemoveAllListeners();
                button480p.onClick.AddListener(() => SetResolution(m_resolutionOptions["480p"]));
            }
            if (button720p != null) 
            {
                button720p.onClick.RemoveAllListeners();
                button720p.onClick.AddListener(() => SetResolution(m_resolutionOptions["720p"]));
            }
            if (button1080p != null) 
            {
                button1080p.onClick.RemoveAllListeners();
                button1080p.onClick.AddListener(() => SetResolution(m_resolutionOptions["1080p"]));
            }

            PCD.DebugMessage(LogType.Log, $"WebCamTextureManager: Manual buttons setup complete. Assigned buttons: {GetAssignedButtonCount()}");
        }

        private int GetAssignedButtonCount()
        {
            int count = 0;
            if (button144p != null) count++;
            if (button240p != null) count++;
            if (button360p != null) count++;
            if (button480p != null) count++;
            if (button720p != null) count++;
            if (button1080p != null) count++;
            return count;
        }

        private void InitializeResolutionUI()
        {
            if (m_uiInitialized) return;

            m_debugUI = DebugUIBuilder.Instance;
            if (m_debugUI == null)
            {
                PCD.DebugMessage(LogType.Warning, "WebCamTextureManager: DebugUIBuilder not found. Resolution controls will not be available.");
                return;
            }

            // Add title label
            m_debugUI.AddLabel("Camera Resolution Control", DebugUIBuilder.DEBUG_PANE_CENTER);
            m_debugUI.AddDivider(DebugUIBuilder.DEBUG_PANE_CENTER);

            // Add current resolution display
            var currentResText = RequestedResolution == Vector2Int.zero 
                ? "Auto (Highest)" 
                : $"{RequestedResolution.x}x{RequestedResolution.y}";
            m_debugUI.AddLabel($"Current: {currentResText}", DebugUIBuilder.DEBUG_PANE_CENTER);

            // Add resolution buttons
            foreach (var resolution in m_resolutionOptions)
            {
                m_debugUI.AddButton($"{resolution.Key} ({resolution.Value.x}x{resolution.Value.y})", 
                    () => SetResolution(resolution.Value), 
                    -1, DebugUIBuilder.DEBUG_PANE_CENTER);
            }

            m_debugUI.AddDivider(DebugUIBuilder.DEBUG_PANE_CENTER);

            m_uiInitialized = true;
            PCD.DebugMessage(LogType.Log, "WebCamTextureManager: Resolution UI initialized successfully");
            
            if (ShowResolutionUI)
            {
                m_debugUI.Show();
            }
        }

        public void SetResolution(Vector2Int newResolution)
        {
            // Update the requested resolution
            RequestedResolution = newResolution;

            var resolutionText = newResolution == Vector2Int.zero ? "Auto (Highest)" : $"{newResolution.x}x{newResolution.y}";
            PCD.DebugMessage(LogType.Log, $"WebCamTextureManager: Resolution changed to {resolutionText}");

            // If the WebCamTexture is already running, we need to restart it with the new resolution
            if (WebCamTexture != null && WebCamTexture.isPlaying)
            {
                RestartWebCamTexture();
            }
        }

        private void RestartWebCamTexture()
        {
            // Stop current texture
            if (WebCamTexture != null)
            {
                WebCamTexture.Stop();
                Destroy(WebCamTexture);
                WebCamTexture = null;
            }
            
            // Restart with new resolution
            _ = StartCoroutine(InitializeWebCamTexture());
        }

        public void ToggleResolutionUI()
        {
            if (m_debugUI == null) return;

            if (m_debugUI.gameObject.activeInHierarchy)
            {
                m_debugUI.Hide();
            }
            else
            {
                if (!m_uiInitialized)
                {
                    InitializeResolutionUI();
                }
                m_debugUI.Show();
            }
        }

        // Public methods for external access
        public void SetResolution144p() => SetResolution(m_resolutionOptions["144p"]);
        public void SetResolution240p() => SetResolution(m_resolutionOptions["240p"]);
        public void SetResolution360p() => SetResolution(m_resolutionOptions["360p"]);
        public void SetResolution480p() => SetResolution(m_resolutionOptions["480p"]);
        public void SetResolution720p() => SetResolution(m_resolutionOptions["720p"]);
        public void SetResolution1080p() => SetResolution(m_resolutionOptions["1080p"]);

        private IEnumerator InitializeWebCamTexture()
        {
            // Check if Passhtrough is present in the scene and is enabled
            var ptLayer = FindAnyObjectByType<OVRPassthroughLayer>();
            if (ptLayer == null || !PassthroughCameraUtils.IsPassthroughEnabled())
            {
                PCD.DebugMessage(LogType.Error, "Passthrough must be enabled to use the Passthrough Camera API.");
                yield break;
            }

#if !UNITY_6000_OR_NEWER
            // There is a bug on Unity 2022 that causes a crash if you don't wait a frame before initializing the WebCamTexture.
            // Waiting for one frame is important and prevents the bug.
            yield return new WaitForEndOfFrame();
#endif

            while (true)
            {
                var devices = WebCamTexture.devices;
                if (PassthroughCameraUtils.EnsureInitialized() && PassthroughCameraUtils.CameraEyeToCameraIdMap.TryGetValue(Eye, out var cameraData))
                {
                    if (cameraData.index < devices.Length)
                    {
                        var deviceName = devices[cameraData.index].name;
                        WebCamTexture webCamTexture;
                        if (RequestedResolution == Vector2Int.zero)
                        {
                            var largestResolution = PassthroughCameraUtils.GetOutputSizes(Eye).OrderBy(static size => size.x * size.y).Last();
                            webCamTexture = new WebCamTexture(deviceName, largestResolution.x, largestResolution.y);
                        }
                        else
                        {
                            webCamTexture = new WebCamTexture(deviceName, RequestedResolution.x, RequestedResolution.y);
                        }
                        webCamTexture.Play();
                        var currentResolution = new Vector2Int(webCamTexture.width, webCamTexture.height);
                        if (RequestedResolution != Vector2Int.zero && RequestedResolution != currentResolution)
                        {
                            PCD.DebugMessage(LogType.Warning, $"WebCamTexture created, but '{nameof(RequestedResolution)}' {RequestedResolution} is not supported. Current resolution: {currentResolution}.");
                        }
                        WebCamTexture = webCamTexture;
                        PCD.DebugMessage(LogType.Log, $"WebCamTexture created, texturePtr: {WebCamTexture.GetNativeTexturePtr()}, size: {WebCamTexture.width}/{WebCamTexture.height}");
                        yield break;
                    }
                }

                PCD.DebugMessage(LogType.Error, $"Requested camera is not present in WebCamTexture.devices: {string.Join(", ", devices)}.");
                yield return null;
            }
        }
    }

    /// <summary>
    /// Defines the position of a passthrough camera relative to the headset
    /// </summary>
    public enum PassthroughCameraEye
    {
        Left,
        Right
    }
}
