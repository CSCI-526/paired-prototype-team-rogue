using UnityEngine;
using Unity.Cinemachine;
using System.Collections;
using Unity.Cinemachine;

public class CameraController : MonoBehaviour
{
    [Header("Cinemachine References")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private CinemachineImpulseSource impulseSource;
    
    [Header("Camera Settings")]
    [SerializeField] private float defaultOrthoSize = 5f;
    [SerializeField] private float zoomSpeed = 2f;
    
    [Header("Shake Settings")]
    [SerializeField] private float lightShakeForce = 1f;
    [SerializeField] private float mediumShakeForce = 2f;
    [SerializeField] private float heavyShakeForce = 4f;
    
    private float _targetOrthoSize;
    private Coroutine _zoomCoroutine;
    
    void Start()
    {
        // Find virtual camera if not assigned
        if (virtualCamera == null)
        {
            virtualCamera = GetComponentInChildren<CinemachineVirtualCamera>();
            if (virtualCamera == null)
            {
                Debug.LogError("CinemachineVirtualCamera not found!");
            }
        }
        
        // Create impulse source if not exists
        if (impulseSource == null)
        {
            impulseSource = GetComponent<CinemachineImpulseSource>();
            if (impulseSource == null)
            {
                impulseSource = gameObject.AddComponent<CinemachineImpulseSource>();
            }
        }
        
        // Set default ortho size
        if (virtualCamera != null)
        {
            virtualCamera.m_Lens.OrthographicSize = defaultOrthoSize;
            _targetOrthoSize = defaultOrthoSize;
        }
        
        // Auto find and follow player
        FindAndFollowPlayer();
    }
    
    public void FindAndFollowPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && virtualCamera != null)
        {
            virtualCamera.Follow = player.transform;
            virtualCamera.LookAt = player.transform;
        }
    }
    
    // Camera shake methods
    public void ShakeCamera(ShakeIntensity intensity)
    {
        float force = 0f;
        
        switch (intensity)
        {
            case ShakeIntensity.Light:
                force = lightShakeForce;
                break;
            case ShakeIntensity.Medium:
                force = mediumShakeForce;
                break;
            case ShakeIntensity.Heavy:
                force = heavyShakeForce;
                break;
        }
        
        if (impulseSource != null)
        {
            impulseSource.GenerateImpulse(force);
        }
    }
    
    // Custom shake with specific force
    public void ShakeCamera(float force)
    {
        if (impulseSource != null)
        {
            impulseSource.GenerateImpulse(force);
        }
    }
    
    // Zoom methods
    public void SetZoom(float newSize, float duration = 1f)
    {
        if (_zoomCoroutine != null)
        {
            StopCoroutine(_zoomCoroutine);
        }
        
        _targetOrthoSize = newSize;
        _zoomCoroutine = StartCoroutine(ZoomCoroutine(newSize, duration));
    }
    
    public void ResetZoom(float duration = 1f)
    {
        SetZoom(defaultOrthoSize, duration);
    }
    
    IEnumerator ZoomCoroutine(float targetSize, float duration)
    {
        if (virtualCamera == null) yield break;
        
        float startSize = virtualCamera.m_Lens.OrthographicSize;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Smooth interpolation
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            virtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(startSize, targetSize, smoothT);
            yield return null;
        }
        
        virtualCamera.m_Lens.OrthographicSize = targetSize;
    }
    
    // Temporary zoom for special attacks or events
    public void TemporaryZoom(float zoomSize, float duration, float holdTime = 0f)
    {
        StartCoroutine(TemporaryZoomCoroutine(zoomSize, duration, holdTime));
    }
    
    IEnumerator TemporaryZoomCoroutine(float zoomSize, float duration, float holdTime)
    {
        // Zoom in
        SetZoom(zoomSize, duration);
        yield return new WaitForSeconds(duration);
        
        // Hold
        if (holdTime > 0f)
        {
            yield return new WaitForSeconds(holdTime);
        }
        
        // Zoom back
        ResetZoom(duration);
    }
    
    public enum ShakeIntensity
    {
        Light,
        Medium,
        Heavy
    }
}