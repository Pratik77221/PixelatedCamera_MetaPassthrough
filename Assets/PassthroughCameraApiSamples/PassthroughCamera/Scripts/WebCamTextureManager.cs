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
using TMPro;

namespace PassthroughCameraSamples
{
    [MetaCodeSample("PassthroughCameraApiSamples-PassthroughCamera")]
    public class WebCamTextureManager : MonoBehaviour
    {
        [Header("Camera Settings")]
        [SerializeField] public PassthroughCameraEye Eye = PassthroughCameraEye.Left;
        [SerializeField] public PassthroughCameraPermissions CameraPermissions;
        
        // Resolution will be controlled by CameraDisplay now
        [HideInInspector]
        public Vector2Int RequestedResolution;

        /// <summary>
        /// Returns <see cref="WebCamTexture"/> reference if required permissions were granted and this component is enabled. Else, returns null.
        /// </summary>
        public WebCamTexture WebCamTexture { get; private set; }

        private bool m_hasPermission;

        private void Awake()
        {
            PCD.DebugMessage(LogType.Log, $"{nameof(WebCamTextureManager)}.{nameof(Awake)}() was called");
            Assert.AreEqual(1, FindObjectsByType<WebCamTextureManager>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length,
                $"PCA: Passthrough Camera: more than one {nameof(WebCamTextureManager)} component. Only one instance is allowed at a time. Current instance: {name}");
#if UNITY_ANDROID
            CameraPermissions.AskCameraPermissions();
#endif
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
        }

        // This method is now public so CameraDisplay can set the resolution
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
